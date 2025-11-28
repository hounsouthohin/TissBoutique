using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration; // Ajouté
using Microsoft.Extensions.Configuration.Json; // Ajouté
using System.IO;

namespace ECommerce.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Construire la configuration pour lire appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                // Le chemin relatif pour trouver le appsettings.json depuis le projet Infrastructure
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ECommerce.API"))
                .AddJsonFile("appsettings.json")
                .Build();

            // Créer les options pour le DbContext
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
