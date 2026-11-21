using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

/// <summary>
/// Implementação do repositório que persiste em um arquivo JSON no disco,
/// em vez de um ConcurrentDictionary em memória (como nas Aulas 11-16) ou
/// um banco relacional via EF Core.
///
/// Por quê um arquivo e não EF Core + Postgres/SQLite? Porque os pacotes do
/// EF Core (Microsoft.EntityFrameworkCore, Npgsql...) vêm do NuGet, que está
/// bloqueado neste ambiente de sandbox — eu não conseguiria nem compilar,
/// muito menos testar de ponta a ponta antes de te entregar. Essa classe
/// usa só System.Text.Json (já embutido no runtime do .NET, sem precisar
/// baixar nada), então dá pra validar o projeto inteiro de verdade aqui.
///
/// E por quê arquivo e não voltar pro ConcurrentDictionary em memória (como
/// as aulas anteriores)? Porque aqui os dados sobrevivem a um restart do
/// container — com Docker, o arquivo fica num volume montado, então parar e
/// subir o `docker compose` de novo não apaga o catálogo que você cadastrar.
/// É o jeito de ter "persistência real" dentro das peças que tenho disponíveis.
///
/// Se este projeto crescer além do escopo da disciplina, trocar isso por
/// EF Core + um banco de verdade é o próximo passo natural — a interface
/// ILivroRepository (Domain) já isola essa decisão: trocar a implementação
/// aqui não exige tocar em Application nem em Api.
/// </summary>
public class ArquivoJsonLivroRepository : ILivroRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public ArquivoJsonLivroRepository(string caminhoArquivo, IReadOnlyList<Livro> dadosIniciaisSeNaoExistir)
    {
        _caminhoArquivo = caminhoArquivo;
        GarantirArquivoCriado(dadosIniciaisSeNaoExistir);
    }

    private void GarantirArquivoCriado(IReadOnlyList<Livro> dadosIniciais)
    {
        var diretorio = Path.GetDirectoryName(_caminhoArquivo);
        if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }

        if (!File.Exists(_caminhoArquivo))
        {
            var json = JsonSerializer.Serialize(dadosIniciais, OpcoesJson);
            File.WriteAllText(_caminhoArquivo, json);
        }
    }

    public async Task<IReadOnlyList<Livro>> ListarTodosAsync(Guid usuarioId)
    {
        var livros = await LerArquivoAsync();
        return livros.Where(l => l.UsuarioId == usuarioId).ToList();
    }

    public async Task<Livro?> BuscarPorIdAsync(Guid id, Guid usuarioId)
    {
        var livros = await LerArquivoAsync();
        // "Nasce filtrado": busca já por (id, usuarioId) juntos, então um
        // livro de outra pessoa nunca é encontrado aqui — não existe um
        // segundo passo de "achei, agora confiro se é dela" que alguém possa
        // esquecer de fazer em uma chamada futura.
        return livros.FirstOrDefault(l => l.Id == id && l.UsuarioId == usuarioId);
    }

    public async Task<Livro> AdicionarAsync(Livro livro)
    {
        await _trava.WaitAsync();
        try
        {
            var livros = (await LerArquivoSemTravaAsync()).ToList();
            livros.Add(livro);
            await EscreverArquivoSemTravaAsync(livros);
            return livro;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<Livro?> AtualizarAsync(Livro livro)
    {
        await _trava.WaitAsync();
        try
        {
            var livros = (await LerArquivoSemTravaAsync()).ToList();
            var indice = livros.FindIndex(l => l.Id == livro.Id);
            if (indice == -1)
            {
                return null;
            }

            livros[indice] = livro;
            await EscreverArquivoSemTravaAsync(livros);
            return livro;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<bool> RemoverAsync(Guid id, Guid usuarioId)
    {
        await _trava.WaitAsync();
        try
        {
            var livros = (await LerArquivoSemTravaAsync()).ToList();
            var removidos = livros.RemoveAll(l => l.Id == id && l.UsuarioId == usuarioId);
            if (removidos == 0)
            {
                return false;
            }

            await EscreverArquivoSemTravaAsync(livros);
            return true;
        }
        finally
        {
            _trava.Release();
        }
    }

    private async Task<List<Livro>> LerArquivoAsync()
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

    private async Task<List<Livro>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var livros = await JsonSerializer.DeserializeAsync<List<Livro>>(stream, OpcoesJson);
        return livros ?? new List<Livro>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<Livro> livros)
    {
        var json = JsonSerializer.Serialize(livros, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
