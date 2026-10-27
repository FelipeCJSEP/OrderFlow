using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace OrderFlow.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Define o container do SQL Server do Testcontainers
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();

        // Inicia o container e aplica as migrations
        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            // Roda as Migrations para criar as tabelas no container novo
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        // Substitui o DbContext da API para apontar para o container do Testcontainers
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove o DbContext original configurado no appsettings
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Registra o DbContext apontando para a ConnectionString dinâmica do Testcontainers
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_dbContainer.GetConnectionString()));
            });
        }

        //Ao finalizar os testes: derruba o container
        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }
    }
}
