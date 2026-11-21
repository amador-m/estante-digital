using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

/// <summary>Mesma estratégia dos outros repositórios em JSON deste projeto
/// (ver ArquivoJsonLivroRepository/UsuarioJsonRepository) — arquivo próprio
/// (um por tipo de entidade, pra não misturar schemas num único arquivo),
/// protegido por um SemaphoreSlim contra escrita concorrente.</summary>
public class SeguidorJsonRepository : ISeguidorRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public SeguidorJsonRepository(string caminhoArquivo)
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
            File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(new List<Seguidor>(), OpcoesJson));
        }
    }

    public async Task<Seguidor?> SeguirAsync(Guid seguidorId, Guid seguidoId)
    {
        await _trava.WaitAsync();
        try
        {
            var relacoes = (await LerArquivoSemTravaAsync()).ToList();
            var existente = relacoes.FirstOrDefault(r => r.SeguidorId == seguidorId && r.SeguidoId == seguidoId);
            if (existente is not null)
            {
                // Idempotente: seguir quem já é seguido não duplica a relação.
                return existente;
            }

            var nova = new Seguidor { Id = Guid.NewGuid(), SeguidorId = seguidorId, SeguidoId = seguidoId, CriadoEm = DateTime.UtcNow };
            relacoes.Add(nova);
            await EscreverArquivoSemTravaAsync(relacoes);
            return nova;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task DeixarDeSeguirAsync(Guid seguidorId, Guid seguidoId)
    {
        await _trava.WaitAsync();
        try
        {
            var relacoes = (await LerArquivoSemTravaAsync()).ToList();
            var removidas = relacoes.RemoveAll(r => r.SeguidorId == seguidorId && r.SeguidoId == seguidoId);
            if (removidas > 0)
            {
                await EscreverArquivoSemTravaAsync(relacoes);
            }
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<bool> EstaSeguindoAsync(Guid seguidorId, Guid seguidoId)
    {
        var relacoes = await LerArquivoAsync();
        return relacoes.Any(r => r.SeguidorId == seguidorId && r.SeguidoId == seguidoId);
    }

    public async Task<int> ContarSeguidoresAsync(Guid usuarioId)
    {
        var relacoes = await LerArquivoAsync();
        return relacoes.Count(r => r.SeguidoId == usuarioId);
    }

    public async Task<int> ContarSeguindoAsync(Guid usuarioId)
    {
        var relacoes = await LerArquivoAsync();
        return relacoes.Count(r => r.SeguidorId == usuarioId);
    }

    public async Task<IReadOnlyList<Guid>> ListarSeguindoIdsAsync(Guid usuarioId)
    {
        var relacoes = await LerArquivoAsync();
        return relacoes.Where(r => r.SeguidorId == usuarioId).Select(r => r.SeguidoId).ToList();
    }

    public async Task<IReadOnlyList<Guid>> ListarSeguidoresIdsAsync(Guid usuarioId)
    {
        var relacoes = await LerArquivoAsync();
        return relacoes.Where(r => r.SeguidoId == usuarioId).Select(r => r.SeguidorId).ToList();
    }

    private async Task<List<Seguidor>> LerArquivoAsync()
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

    private async Task<List<Seguidor>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var relacoes = await JsonSerializer.DeserializeAsync<List<Seguidor>>(stream, OpcoesJson);
        return relacoes ?? new List<Seguidor>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<Seguidor> relacoes)
    {
        var json = JsonSerializer.Serialize(relacoes, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
