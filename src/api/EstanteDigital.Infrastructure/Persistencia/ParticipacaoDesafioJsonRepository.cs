using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

public class ParticipacaoDesafioJsonRepository : IParticipacaoDesafioRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public ParticipacaoDesafioJsonRepository(string caminhoArquivo)
    {
        _caminhoArquivo = caminhoArquivo;
        GarantirArquivoCriado();
    }

    private void GarantirArquivoCriado()
    {
        var diretorio = Path.GetDirectoryName(_caminhoArquivo);
        if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }

        if (!File.Exists(_caminhoArquivo))
        {
            File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(new List<ParticipacaoDesafio>(), OpcoesJson));
        }
    }

    public async Task<ParticipacaoDesafio> EntrarAsync(Guid desafioId, Guid usuarioId)
    {
        await _trava.WaitAsync();
        try
        {
            var participacoes = (await LerArquivoSemTravaAsync()).ToList();
            var existente = participacoes.FirstOrDefault(p => p.DesafioId == desafioId && p.UsuarioId == usuarioId);
            if (existente is not null)
            {
                return existente;
            }

            var nova = new ParticipacaoDesafio { Id = Guid.NewGuid(), DesafioId = desafioId, UsuarioId = usuarioId, EntrouEm = DateTime.UtcNow };
            participacoes.Add(nova);
            await EscreverArquivoSemTravaAsync(participacoes);
            return nova;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task SairAsync(Guid desafioId, Guid usuarioId)
    {
        await _trava.WaitAsync();
        try
        {
            var participacoes = (await LerArquivoSemTravaAsync()).ToList();
            var removidas = participacoes.RemoveAll(p => p.DesafioId == desafioId && p.UsuarioId == usuarioId);
            if (removidas > 0)
            {
                await EscreverArquivoSemTravaAsync(participacoes);
            }
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<bool> ParticipaAsync(Guid desafioId, Guid usuarioId)
    {
        var participacoes = await LerArquivoAsync();
        return participacoes.Any(p => p.DesafioId == desafioId && p.UsuarioId == usuarioId);
    }

    public async Task<IReadOnlyList<Guid>> ListarParticipantesIdsAsync(Guid desafioId)
    {
        var participacoes = await LerArquivoAsync();
        return participacoes.Where(p => p.DesafioId == desafioId).Select(p => p.UsuarioId).ToList();
    }

    public async Task<IReadOnlyList<Guid>> ListarDesafioIdsDoUsuarioAsync(Guid usuarioId)
    {
        var participacoes = await LerArquivoAsync();
        return participacoes.Where(p => p.UsuarioId == usuarioId).Select(p => p.DesafioId).ToList();
    }

    public async Task RemoverTodasDoDesafioAsync(Guid desafioId)
    {
        await _trava.WaitAsync();
        try
        {
            var participacoes = (await LerArquivoSemTravaAsync()).ToList();
            var removidas = participacoes.RemoveAll(p => p.DesafioId == desafioId);
            if (removidas > 0)
            {
                await EscreverArquivoSemTravaAsync(participacoes);
            }
        }
        finally
        {
            _trava.Release();
        }
    }

    private async Task<List<ParticipacaoDesafio>> LerArquivoAsync()
    {
        await _trava.WaitAsync();
        try
        {
            return await LerArquivoSemTravaAsync();
        }
        finally
        {
            _trava.Release();
        }
    }

    private async Task<List<ParticipacaoDesafio>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var participacoes = await JsonSerializer.DeserializeAsync<List<ParticipacaoDesafio>>(stream, OpcoesJson);
        return participacoes ?? new List<ParticipacaoDesafio>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<ParticipacaoDesafio> participacoes)
    {
        var json = JsonSerializer.Serialize(participacoes, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
