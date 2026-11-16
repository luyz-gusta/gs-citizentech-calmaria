using System.Text.Json;
using System.Text.Json.Serialization;
using Calmaria.Api.Comum;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Infrastructure.Persistencia.Seed;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Conexão por variável de ambiente (ConnectionStrings__Calmaria) ou user-secrets; nunca no appsettings do Git.
var conexao = builder.Configuration.GetConnectionString("Calmaria");
if (string.IsNullOrWhiteSpace(conexao) && !builder.Environment.IsEnvironment("Testing"))
    throw new InvalidOperationException(
        "Falta a conexão com o PostgreSQL. Defina ConnectionStrings__Calmaria ou, em desenvolvimento, " +
        "rode api-dotnet/scripts/criar-banco-dev.ps1.");

builder.Services.AddPersistencia(conexao ?? string.Empty);
builder.Services.AddModuloSensorial(builder.Configuration);
builder.Services.AddModuloComunidade();
builder.Services.AddModuloPrivacidade(builder.Configuration);

// Valores fixos saem em maiúsculas com sublinhado (ESTACAO, LUZ_FORTE), como manda o contrato.
var conversorDeEnum = new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper);
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(conversorDeEnum));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(conversorDeEnum));
builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = ErrosDaApi.DadosInvalidos);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeErrosDeDominio>();
builder.Services.AddOpenApi(o => o.AddDocumentTransformer((documento, _, _) =>
{
    documento.Info.Title = "API Calmaria";
    documento.Info.Description =
        "O mapa sensorial de São Paulo. Módulos Sensorial, Comunidade e Privacidade. Global Solution 2026.2, FIAP.";
    return Task.CompletedTask;
}));
builder.Services.AddCors(o => o.AddPolicy("app", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Em desenvolvimento, a API cria e atualiza as tabelas ao subir (na VM, quem faz isso é o efbundle) e, com o
// banco vazio, grava 4 semanas de leituras simuladas.
if (app.Configuration.GetValue<bool>("Banco:MigrarAoIniciar"))
{
    using var escopo = app.Services.CreateScope();
    var db = escopo.ServiceProvider.GetRequiredService<CalmariaDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue<bool>("Banco:SemearLeituras"))
    {
        var gravadas = await SeederDeLeituras.SemearAsync(db,
            escopo.ServiceProvider.GetRequiredService<ICalculadoraIndice>(),
            escopo.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime);
        if (gravadas > 0)
            app.Logger.LogInformation("Seed: {Quantidade} leituras simuladas gravadas.", gravadas);
    }
}

app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
    app.UseCors("app");

app.MapOpenApi();
app.MapScalarApiReference(o => o.WithTitle("API Calmaria"));
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>Exposto para os testes de endpoint (WebApplicationFactory).</summary>
public partial class Program;
