using Aspel.CoreFiscal.Cancelacion.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Aspel.CoreFiscal.Cancelacion.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar MediatR escaneando el ensamblado actual
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Registrar FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Registrar Application Services
            services.AddScoped<IPacLoadBalancerService, PacLoadBalancerService>();

            return services;
        }
    }
}
