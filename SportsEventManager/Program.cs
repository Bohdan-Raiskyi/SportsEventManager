using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SportsEventManager.Data;       // Our DbContext
using SportsEventManager.Models;     // Our Models
using SportsEventManager.Repositories; // Our Repositories

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        Console.WriteLine("Application started!");
        Console.WriteLine("----------------------------------");

        // --- Test our Unit of Work ---
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var uow = services.GetRequiredService<IUnitOfWork>();

                // This ID must exist in your "Users" table (e.g., the one you inserted)
                long currentUserId = 1;

                // --- 1. Test IEventRepository (Full CRUD) ---
                Console.WriteLine("\n[1] Testing 'Events'...");

                // Create
                Console.WriteLine("  Creating 'Olympics 2028'...");
                var newEvent = new Event
                {
                    EventName = "Olympics 2028",
                    StartDate = new DateTime(2028, 7, 14, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2028, 7, 30, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow, // These fields are required by the model,
                    UpdatedAt = DateTime.UtcNow  // even if the SP doesn't use them directly
                };
                await uow.Events.CreateAsync(newEvent, currentUserId);
                Console.WriteLine("  Event created.");

                // Read
                Console.WriteLine("  Reading all active events...");
                var allEvents = (await uow.Events.GetAllActiveAsync()).ToList();
                foreach (var ev in allEvents)
                {
                    Console.WriteLine($"    -> ID: {ev.Id}, Name: {ev.EventName}");
                }

                // Delete (Soft Delete)
                if (allEvents.Any())
                {
                    var eventToDelete = allEvents.First();
                    Console.WriteLine($"  Soft-deleting event ID: {eventToDelete.Id}...");
                    await uow.Events.DeleteAsync(eventToDelete.Id, currentUserId);
                    Console.WriteLine("  Event deleted.");
                }

                // --- 2. Test IAthleteRepository (Read, Delete) ---
                Console.WriteLine("\n[2] Testing 'Athletes'...");
                Console.WriteLine("  Reading all active athletes...");
                var allAthletes = (await uow.Athletes.GetAllActiveAsync()).ToList();
                if (!allAthletes.Any())
                {
                    Console.WriteLine("    -> No active athletes found.");
                }
                foreach (var athlete in allAthletes)
                {
                    Console.WriteLine($"    -> ID: {athlete.Id}, Name: {athlete.FirstName} {athlete.LastName}");
                }

                // (You can add an athlete manually in the DB to test the delete)
                if (allAthletes.Any())
                {
                    var athleteToDelete = allAthletes.First();
                    Console.WriteLine($"  Soft-deleting athlete ID: {athleteToDelete.Id}...");
                    await uow.Athletes.DeleteAsync(athleteToDelete.Id, currentUserId);
                    Console.WriteLine("  Athlete deleted.");
                }

                // --- 3. Test ITeamRepository (Read-Only) ---
                Console.WriteLine("\n[3] Testing 'Teams' (Read-Only)...");
                Console.WriteLine("  Reading all active teams...");
                var allTeams = (await uow.Teams.GetAllActiveAsync()).ToList();
                if (!allTeams.Any())
                {
                    Console.WriteLine("    -> No active teams found.");
                }
                foreach (var team in allTeams)
                {
                    Console.WriteLine($"    -> ID: {team.Id}, Name: {team.TeamName}");
                }

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nRUNTIME ERROR: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine("\n----------------------------------");
        Console.WriteLine("Application finished. Press Enter to exit.");
        Console.ReadLine();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // 1. Set your Connection String
                string connectionString = "Host=localhost;Database=sports_event_management_system;Username=postgres;Password=2325";

                // 2. Register the DbContext
                services.AddDbContext<SportsDbContext>(options =>
                    options.UseNpgsql(connectionString)
                );

                // --- 3. Register Repositories and UnitOfWork ---
                services.AddScoped<IEventRepository, EventRepository>();
                services.AddScoped<IAthleteRepository, AthleteRepository>();
                services.AddScoped<ITeamRepository, TeamRepository>();

                services.AddScoped<IUnitOfWork, UnitOfWork>();
            });
}