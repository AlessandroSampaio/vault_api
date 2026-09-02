namespace VaultApi.Application.Revendas;

public record CriarRevendaRequest(string Nome, string Cnpj);
public record RevendaResponse(Guid Id, string Nome, string Cnpj, bool Ativo);
