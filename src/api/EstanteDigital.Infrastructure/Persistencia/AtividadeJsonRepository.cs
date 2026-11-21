using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

public class AtividadeJsonRepository : IAtividadeRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public AtividadeJsonRepository(string caminhoArquivo)
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
            File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(new List<Atividade>(), OpcoesJson));
        }
    }

    public async Task<Atividade> RegistrarAsync(Atividade atividade)
    {
        await _trava.WaitAsync();
        try
        {
            var atividades = (await LerArquivoSemTravaAsync()).ToList();
            atividades.Add(atividade);
            await EscreverArquivoSemTravaAsync(atividades);
            return atividade;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<IReadOnlyList<Atividade>> ListarDeUsuariosAsync(IReadOnlyList<Guid> usuarioIds, int limite)
    {
        var atividades = await LerArquivoAsync();
        var idsSet = usuarioIds.ToHashSet();
        return atividades
            .Where(a => idsSet.Contains(a.UsuarioId))
            .OrderByDescending(a => a.CriadoEm)
            .Take(limite)
            .ToList();
    }

    private async Task<List<Atividade>> LerArquivoAsync()
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

    private async Task<List<Atividade>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var atividades = await JsonSerializer.DeserializeAsync<List<Atividade>>(stream, OpcoesJson);
        return atividades ?? new List<Atividade>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<Atividade> atividades)
    {
        var json = JsonSerializer.Serialize(atividades, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
