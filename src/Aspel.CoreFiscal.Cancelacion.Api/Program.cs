using Aspel.CoreFiscal.Cancelacion.Api.Endpoints;
using Aspel.CoreFiscal.Cancelacion.Api.Middlewares;
using Aspel.CoreFiscal.Cancelacion.Api.Soap;
using Aspel.CoreFiscal.Cancelacion.Application;
using Aspel.CoreFiscal.Cancelacion.Infrastructure;
using CoreWCF.Configuration;
using CoreWCF.Description;

var builder = WebApplication.CreateBuilder(args);

// 1. Registro de Capas (Clean Architecture)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Registro de utilidades de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Aspel CoreFiscal Cancelacion API", Version = "v1" });
});

// 3. Registro del manejador global de excepciones
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Registro de CoreWCF para soporte SOAP legacy
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

// 5. Configuración del Pipeline HTTP
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6. Mapeo de Endpoints REST (Minimal APIs)
app.MapCancelacionEndpoints();

// 7. Mapeo de Endpoints SOAP (CoreWCF)
app.UseServiceModel(builder =>
{
    builder.AddService<SrvCancelacionCfdiSoapService>(serviceOptions => { })
           .AddServiceEndpoint<SrvCancelacionCfdiSoapService, ISrvCancelacionCfdiSoap>(
               new CoreWCF.BasicHttpBinding(),
               "/soap/IsrvCancelacionCFDI"); // Ruta compatible con el legacy C++
});

var serviceMetadataBehavior = app.Services.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;

app.Run();