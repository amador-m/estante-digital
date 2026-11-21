using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

/// <summary>Mesma estratégia dos outros repositórios em JSON deste projeto
/// (ver SeguidorJsonRepository) — arquivo próprio protegido por um
/// SemaphoreSlim contra escrita concorrente.</summary>
public class DesafioJsonRepository : IDesafioRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public DesafioJsonRepository(string caminhoArquivo)
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
            File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(new List<Desafio>(), OpcoesJson));
        }
    }

    public async Task<Desafio> CriarAsync(Desafio desafio)
    {
        await _trava.WaitAsync();
        try
        {
            var desafios = (await LerArquivoSemTravaAsync()).ToList();
            desafios.Add(desafio);
            await EscreverArquivoSemTravaAsync(desafios);
            return desafio;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<Desafio?> BuscarPorIdAsync(Guid id)
    {
        var desafios = await LerArquivoAsync();
        return desafios.FirstOrDefault(d => d.Id == id);
    }

    public async Task<IReadOnlyList<Desafio>> ListarTodosAsync() => await LerArquivoAsync();

    public async Task<bool> RemoverAsync(Guid id, Guid criadorId)
    {
        await _trava.WaitAsync();
        try
        {
            var desafios = (await LerArquivoSemTravaAsync()).ToList();
            var removidos = desafios.RemoveAll(d => d.Id == id && d.CriadorId == criadorId);
            if (removidos > 0)
            {
                await EscreverArquivoSemTravaAsync(desafios);
                return true;
            }

            return false;
        }
        finally
        {
            _trava.Release();
        }
    }

    private async Task<List<Desafio>> LerArquivoAsync()
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

    private async Task<List<Desafio>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var desafios = await JsonSerializer.DeserializeAsync<List<Desafio>>(stream, OpcoesJson);
        return desafios ?? new List<Desafio>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<Desafio> desafios)
    {
        var json = JsonSerializer.Serialize(desafios, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
