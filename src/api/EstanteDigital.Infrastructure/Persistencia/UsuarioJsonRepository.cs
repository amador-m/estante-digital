using System.Text.Json;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Persistencia;

/// <summary>
/// Mesma estratégia e mesma razão do ArquivoJsonLivroRepository (ver os
/// comentários lá): persiste em um arquivo JSON porque o EF Core/Npgsql não
/// pode ser baixado nem compilado neste sandbox. É esta implementação —
/// testável de ponta a ponta por mim — que uso para validar toda a lógica
/// de cadastro/login/isolamento por usuário antes de also escrever a versão
/// EF Core + PostgreSQL (em EstanteDigital.Infrastructure/Persistencia/Ef),
/// que só você consegue rodar e testar de fato, no seu PC.
///
/// Nunca guarda a senha em texto puro — o campo persistido é
/// Usuario.SenhaHash, já produzido pelo SenhaHasher antes de chegar aqui.
/// </summary>
public class UsuarioJsonRepository : IUsuarioRepository
{
    private readonly string _caminhoArquivo;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private static readonly JsonSerializerOptions OpcoesJson = new() { WriteIndented = true };

    public UsuarioJsonRepository(string caminhoArquivo, IReadOnlyList<Usuario> dadosIniciaisSeNaoExistir)
    {
        _caminhoArquivo = caminhoArquivo;
        GarantirArquivoCriado(dadosIniciaisSeNaoExistir);
    }

    private void GarantirArquivoCriado(IReadOnlyList<Usuario> dadosIniciais)
    {
        var diretorio = Path.GetDirectoryName(_caminhoArquivo);
        if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }

        if (!File.Exists(_caminhoArquivo))
        {
            File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(dadosIniciais, OpcoesJson));
        }
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        var usuarios = await LerArquivoAsync();
        return usuarios.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Usuario?> BuscarPorIdAsync(Guid id)
    {
        var usuarios = await LerArquivoAsync();
        return usuarios.FirstOrDefault(u => u.Id == id);
    }

    public async Task<Usuario?> BuscarPorNomeUsuarioAsync(string nomeUsuario)
    {
        var usuarios = await LerArquivoAsync();
        return usuarios.FirstOrDefault(u => string.Equals(u.NomeUsuario, nomeUsuario, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<Usuario>> BuscarPorNomeParcialAsync(string trecho)
    {
        var usuarios = await LerArquivoAsync();
        return usuarios
            .Where(u => u.NomeUsuario.Contains(trecho, StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.NomeUsuario)
            .ToList();
    }

    public async Task<Usuario> AdicionarAsync(Usuario usuario)
    {
        await _trava.WaitAsync();
        try
        {
            var usuarios = (await LerArquivoSemTravaAsync()).ToList();
            usuarios.Add(usuario);
            await EscreverArquivoSemTravaAsync(usuarios);
            return usuario;
        }
        finally
        {
            _trava.Release();
        }
    }

    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        await _trava.WaitAsync();
        try
        {
            var usuarios = (await LerArquivoSemTravaAsync()).ToList();
            var indice = usuarios.FindIndex(u => u.Id == usuario.Id);
            if (indice == -1)
            {
                throw new InvalidOperationException("Usuário não encontrado para atualizar.");
            }

            usuarios[indice] = usuario;
            await EscreverArquivoSemTravaAsync(usuarios);
            return usuario;
        }
        finally
        {
            _trava.Release();
        }
    }

    private async Task<List<Usuario>> LerArquivoAsync()
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

    private async Task<List<Usuario>> LerArquivoSemTravaAsync()
    {
        await using var stream = File.OpenRead(_caminhoArquivo);
        var usuarios = await JsonSerializer.DeserializeAsync<List<Usuario>>(stream, OpcoesJson);
        return usuarios ?? new List<Usuario>();
    }

    private async Task EscreverArquivoSemTravaAsync(List<Usuario> usuarios)
    {
        var json = JsonSerializer.Serialize(usuarios, OpcoesJson);
        await File.WriteAllTextAsync(_caminhoArquivo, json);
    }
}
