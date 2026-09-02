namespace VaultApi.Application.Clientes;

public record CriarClienteRequest(
    string RazaoSocial,
    string Cnpj,
    Guid? RevendaId,
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

public record ClienteResponse(
    Guid Id,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    Guid? RevendaId,
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
