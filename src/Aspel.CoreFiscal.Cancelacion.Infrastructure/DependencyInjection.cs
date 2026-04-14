using Aspel.CoreFiscal.Cancelacion.Application.Interfaces;
using Aspel.CoreFiscal.Cancelacion.Infrastructure.Caching;
using Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices;
using Aspel.CoreFiscal.Cancelacion.Infrastructure.ExternalServices.Adapters;
using Aspel.CoreFiscal.Cancelacion.Infrastructure.Persitence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Aspel.CoreFiscal.Cancelacion.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Dapper / Repositorio SQL
            services.AddScoped<ICfdiCancellationRepository, CfdiCancellationRepository>();

            // 2. Redis Multiplexer (Singleton para máxima eficiencia)
            var redisConnString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnString));
            services.AddScoped<IPacStateRepository, RedisPacStateRepository>();

            // 3. HTTP Clients con Polly v8 Integrado (StandardResilienceHandler)
            services.AddScoped<IPacIntegrationClient, PacIntegrationClient>();

            // Configuración para Comercio Digital
            services.AddHttpClient("ComercioDigitalClient", client =>
            {
                client.BaseAddress = new Uri("https://ws.comerciodigital.mx");
                client.Timeout = TimeSpan.FromSeconds(30); // TimeOut base
            })
            .AddStandardResilienceHandler(options =>
            {
                // Polly: Circuit Breaker y Reintentos automáticos configurados con mejores prácticas
                options.Retry.MaxRetryAttempts = 3;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
                options.CircuitBreaker.FailureRatio = 0.5; // Si el 50% falla, abre el circuito
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30); // Castigo por 30s
            });

            // Configuración para Pegaso (Soporte TLS y certificados)
            services.AddHttpClient("PegasoClient", client =>
            {
                client.BaseAddress = new Uri("https://ws.pegasotecnologia.mx");
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "TU_API_KEY");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddStandardResilienceHandler(); // Misma protección para Pegaso

            // Configuración para Aspel (Primario)
            services.AddHttpClient("AspelClient", client =>
            {
                client.BaseAddress = new Uri("https://pcfdi.aspel.com.mx");
                client.Timeout = TimeSpan.FromSeconds(15); // Exigimos respuesta más rápida al primario
            })
            .AddStandardResilienceHandler();

            services.AddScoped<IPacAdapter, ComercioDigitalAdapter>();
            services.AddScoped<IPacAdapter, PegasoAdapter>();

            return services;
        }
    }
}
