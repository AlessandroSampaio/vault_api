namespace VaultApi.Application.Clientes;

public record CriarClienteRequest(string Nome, string Cnpj, Guid? RevendaId);
public record ClienteResponse(Guid Id, string Nome, string Cnpj, Guid? RevendaId);
