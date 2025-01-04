using Microsoft.EntityFrameworkCore;

namespace ChatService.Data;

public static class PrepDb
{
    public static void PrepPopulation(IApplicationBuilder app, bool isProduction)
    {
        using (var serviceScope = app.ApplicationServices.CreateScope())
        {
            SeedData(serviceScope.ServiceProvider.GetRequiredService<AppDbContext>(), isProduction);
        }
    }

    private static void SeedData(AppDbContext context, bool isProduction)
    {
        if(isProduction)
        {
            Console.WriteLine("--> Attemt to apply migrations...");
            try
            { 
                context.Database.Migrate();
                Console.WriteLine($"--> migration successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not run migrations: {ex.Message}");
            }
        }
    }
}