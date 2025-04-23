using CambioAPI.Data.Seeders;
using Microsoft.Extensions.DependencyInjection;

namespace CambioAPI.Extensions
{
    public static class SeederExtensions
    {
        public static IServiceCollection AddSeeders(this IServiceCollection services)
        {
            services.AddScoped<CurrencyDataSeeder>();
            services.AddScoped<UserDataSeeder>();
            return services;
        }

        public static async Task RunSeedersAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var currencySeeder = services.GetRequiredService<CurrencyDataSeeder>();
            await currencySeeder.SeedAsync();

            var userSeeder = services.GetRequiredService<UserDataSeeder>();
            await userSeeder.SeedAsync();
        }
    }
} 