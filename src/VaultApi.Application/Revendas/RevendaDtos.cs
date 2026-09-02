namespace VaultApi.Application.Revendas;

public record CriarRevendaRequest(
    string RazaoSocial,
    string Cnpj,
    string? NomeFantasia = null,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Estado = null,
    string? Email = null,
    string? Telefone = null,
    string? Whatsapp = null,
    string? Responsavel = null);

public record RevendaResponse(
    Guid Id,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    bool Ativo,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Email,
    string? Telefone,
    string? Whatsapp,
    string? Responsavel,
    DateTimeOffset CriadoEm);
