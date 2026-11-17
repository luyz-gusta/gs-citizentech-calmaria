using System.Text.Json;
using System.Text.Json.Serialization;
using Calmaria.Api.Comum;
using Calmaria.Api.Privacidade;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Infrastructure.Persistencia.Seed;
using Microsoft.AspNetCore.HttpLogging;
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

// O log em arquivo (um por dia) é o que a varredura de privacidade confere. LogEmArquivo:Pasta vazio desliga.
var pastaDeLogs = builder.Configuration["LogEmArquivo:Pasta"] is { Length: > 0 } pasta
    ? Path.GetFullPath(pasta, builder.Environment.ContentRootPath)
    : null;
if (pastaDeLogs is not null)
    builder.Services.AddSingleton<ILoggerProvider>(sp =>
        new ProvedorDeLogEmArquivo(pastaDeLogs, sp.GetRequiredService<TimeProvider>()));

builder.Services.AddPersistencia(conexao ?? string.Empty);
builder.Services.AddModuloSensorial(builder.Configuration);
builder.Services.AddModuloComunidade();
builder.Services.AddModuloPrivacidade(builder.Configuration, pastaDeLogs,
    RaizDoRepositorio(builder.Environment.ContentRootPath));

// Valores fixos saem em maiúsculas com sublinhado (ESTACAO, LUZ_FORTE), como manda o contrato.
var conversorDeEnum = new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper);
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(conversorDeEnum));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(conversorDeEnum));
builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = ErrosDaApi.DadosInvalidos);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeErrosDeDominio>();
builder.Services.AddOpenApi(o =>
{
    o.AddDocumentTransformer((documento, _, _) =>
    {
        documento.Info.Title = "API Calmaria";
        documento.Info.Description =
            "O mapa sensorial de São Paulo. Módulos Sensorial, Comunidade e Privacidade. Global Solution 2026.2, FIAP. " +
            "As rotas de cadastro de lugares, de importação de previsões e de privacidade (varreduras, Risco Cidadão) " +
            "exigem o cabeçalho X-Admin-Key.";
        return Task.CompletedTask;
    });
    DocumentacaoOpenApi.Configurar(o);
});
builder.Services.AddCors(o => o.AddPolicy("app", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Só para a demonstração do "antes" no vídeo de Cybersecurity: o log de depuração do corpo das requisições, o
// erro comum que grava o comentário cru no log. Com o guarda de log ligado, a linha sai mascarada.
var logarCorpo = builder.Configuration.GetValue<bool>("Privacidade:LogarCorpoDasRequisicoes");
if (logarCorpo)
{
    builder.Services.AddHttpLogging(o =>
    {
        o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.RequestBody;
        o.RequestBodyLogLimit = 4096;
    });
    builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Information);
}

// O guarda de saída do log embrulha todos os provedores. Desligar (Privacidade:RedigirLogs=false) só para mostrar
// o antes da correção.
if (builder.Configuration.GetValue("Privacidade:RedigirLogs", true))
    builder.Services.ProtegerLogsComRedacao();

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
if (logarCorpo)
    app.UseHttpLogging();
if (app.Environment.IsDevelopment())
    app.UseCors("app");

// O painel Risco Cidadão (/seguranca) é estático e só fala com a própria API: a política de conteúdo não deixa
// script de fora, a não ser o Chart.js fixado por versão.
app.Use(async (contexto, proximo) =>
{
    contexto.Response.Headers.XContentTypeOptions = "nosniff";
    contexto.Response.Headers["Referrer-Policy"] = "no-referrer";
    if (contexto.Request.Path.StartsWithSegments("/seguranca"))
    {
        contexto.Response.Headers.ContentSecurityPolicy =
            "default-src 'self'; script-src 'self' https://cdn.jsdelivr.net; style-src 'self' https://fonts.googleapis.com; " +
            "font-src https://fonts.gstatic.com; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; " +
            "base-uri 'none'; form-action 'none'";
        contexto.Response.Headers.XFrameOptions = "DENY";
    }

    await proximo();
});
app.UseStaticFiles();
app.MapGet("/seguranca", () => Results.Redirect("/seguranca/index.html")).ExcludeFromDescription();

app.MapOpenApi();
app.MapScalarApiReference(o => o
    .WithTitle("API Calmaria")
    .WithFavicon("/favicon.svg")
    .DisableDefaultFonts()
    .WithCustomCss(TemaDoScalar.Css)
    // Sem a barra de ferramentas, sem o chat de IA (mandaria a documentação ao serviço do Scalar) e sem telemetria.
    .HideDeveloperTools()
    .DisableAgent()
    .DisableTelemetry());
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// A raiz do repositório (a pasta com .git), para a busca de segredo no código. Na VM não há repositório, e a
// fonte de código fica de fora da varredura.
static string? RaizDoRepositorio(string inicio)
{
    for (var pasta = new DirectoryInfo(inicio); pasta is not null; pasta = pasta.Parent)
    {
        var git = Path.Combine(pasta.FullName, ".git");
        if (Directory.Exists(git) || File.Exists(git))
            return pasta.FullName;
    }

    return null;
}

/// <summary>Exposto para os testes de endpoint (WebApplicationFactory).</summary>
public partial class Program;
