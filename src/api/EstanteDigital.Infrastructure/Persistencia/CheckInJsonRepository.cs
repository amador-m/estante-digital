using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

public class CheckInJsonRepository : ICheckInRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public CheckInJsonRepository(string caminhoArquivo)
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
            File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(new List<CheckInLeitura>(), OpcoesJson));
        }
    }

    public async Task<CheckInLeitura> RegistrarAsync(Guid usuarioId, DateTime data)
    {
        var dataNormalizada = data.Date;
        await _trava.WaitAsync();
        try
        {
            var checkIns = (await LerArquivoSemTravaAsync()).ToList();
            var existente = checkIns.FirstOrDefault(c => c.UsuarioId == usuarioId && c.Data == dataNormalizada);
            if (existente is not null)
            {
                return existente;
            }

            var novo = new CheckInLeitura { Id = Guid.NewGuid(), UsuarioId = usuarioId, Data = dataNormalizada, CriadoEm = DateTime.UtcNow };
            checkIns.Add(novo);
            await EscreverArquivoSemTravaAsync(checkIns);
            return novo;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<bool> JaRegistrouAsync(Guid usuarioId, DateTime data)
    {
        var checkIns = await LerArquivoAsync();
        return checkIns.Any(c => c.UsuarioId == usuarioId && c.Data == data.Date);
    }

    public async Task<IReadOnlyList<DateTime>> ListarDatasAsync(Guid usuarioId)
    {
        var checkIns = await LerArquivoAsync();
        return checkIns.Where(c => c.UsuarioId == usuarioId).Select(c => c.Data).ToList();
    }

    private async Task<List<CheckInLeitura>> LerArquivoAsync()
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

    private async Task<List<CheckInLeitura>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var checkIns = await JsonSerializer.DeserializeAsync<List<CheckInLeitura>>(stream, OpcoesJson);
        return checkIns ?? new List<CheckInLeitura>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<CheckInLeitura> checkIns)
    {
        var json = JsonSerializer.Serialize(checkIns, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
