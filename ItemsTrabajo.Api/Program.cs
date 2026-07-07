using ItemsTrabajo.Api.Clients;
using ItemsTrabajo.Api.Repositories;
using ItemsTrabajo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IItemTrabajoRepository, ItemTrabajoRepository>();
builder.Services.AddSingleton<IMotorDistribucion, MotorDistribucion>();

builder.Services.AddHttpClient<IUsuariosApiClient, UsuariosApiClient>(client =>
{
    var baseUrl = builder.Configuration["UsuariosApi:BaseUrl"] ?? "https://localhost:7001/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<IItemTrabajoService, ItemTrabajoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();