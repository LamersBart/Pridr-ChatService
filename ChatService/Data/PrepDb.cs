using Microsoft.EntityFrameworkCore;

namespace ChatService.Data;

public static class PrepDb
{
    public static async Task PrepPopulation(IApplicationBuilder app, bool isProduction)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            await SeedData(serviceScope.ServiceProvider.GetRequiredService<AppDbContext>(), isProduction);
        }
    }

    private static async Task SeedData(AppDbContext context, bool isProduction)
    {
        if(isProduction)
        {
            Console.WriteLine("--> Attemt to apply migrations...");
            try
            { 
                if (await context.Database.GetPendingMigrationsAsync() is { } migrations && migrations.Any())
                {
                    await context.Database.MigrateAsync();
                }
                Console.WriteLine($"--> migration successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }
        }
    }
}