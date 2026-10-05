using System.Text.Json.Serialization;
using DesafioDev.Api;
using DesafioDev.Api.Endpoints;
using DesafioDev.Core.Comissao;
using DesafioDev.Core.Estoque;
using DesafioDev.Core.Juros;
using DesafioDev.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DominioExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // "Entrada"/"Saida" em vez de 1/2

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(sp => new CalculadoraJuros(sp.GetRequiredService<TimeProvider>()));

var dados = Path.Combine(AppContext.BaseDirectory, "Data");
builder.Services.AddSingleton(ArquivoJson.Carregar<ArquivoVendas>(Path.Combine(dados, "vendas.json")));
builder.Services.AddEstoqueSqlite(builder.Configuration.GetConnectionString("Estoque")
                                  ?? throw new InvalidOperationException("ConnectionStrings:Estoque não configurada."));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

await app.Services.InicializarEstoqueAsync(ArquivoJson.Carregar<ArquivoEstoque>(Path.Combine(dados, "estoque.json")));

var api = app.MapGroup("/api");
api.MapComissoes();
api.MapEstoque();
api.MapJuros();

app.Run();
