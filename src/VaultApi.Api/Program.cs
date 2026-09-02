using VaultApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(VaultApi.Api.Auth.PolicyNames.RequireAdmin, policy =>
        policy.RequireClaim("nivel", nameof(VaultApi.Domain.Enums.Nivel.Admin)));
    options.AddPolicy(VaultApi.Api.Auth.PolicyNames.RequireRevendaOrAdmin, policy =>
        policy.RequireClaim("nivel",
            nameof(VaultApi.Domain.Enums.Nivel.Admin),
            nameof(VaultApi.Domain.Enums.Nivel.Revenda),
            nameof(VaultApi.Domain.Enums.Nivel.Usuario)));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VaultApi.Application.Abstractions.ICurrentUser, VaultApi.Api.Auth.CurrentUser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
