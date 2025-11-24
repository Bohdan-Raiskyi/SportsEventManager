using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using SportsEventManager.Data;
using SportsEventManager.Models;
using SportsEventManager.Repositories;
using System.Diagnostics; // Для заміру часу

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();


        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            Console.Write("Generate 10,000 events for the test? (y/n): ");
            var key = Console.ReadLine();

            if (key?.ToLower() == "y")
            {
                await SeedLargeData(services);
            }

            await RunBenchmark(services);
        }

        Console.WriteLine("       PART 6: REDIS (Key-Value NoSQL)");

        try
        {
            var redisRepo = new RedisLeaderboardRepository("localhost:6379");

            Console.WriteLine("Simulating live match updates...");

            // 2. Додаємо дані (нібито йде змагання)
            await redisRepo.AddScoreAsync("Usain Bolt", 9.58);
            await redisRepo.AddScoreAsync("Tyson Gay", 9.69);
            await redisRepo.AddScoreAsync("Yohan Blake", 9.75);
            await redisRepo.AddScoreAsync("Asafa Powell", 9.72);

            // 3. Імітуємо подію: хтось побив рекорд!
            Console.WriteLine(">> EVENT: New record set by a Rookie!");
            await Task.Delay(500); // Імітація затримки
            await redisRepo.AddScoreAsync("Super Rookie", 9.40); // Значення що має стати саме першим

            // 4. Отримуємо Топ-3
            Console.WriteLine("\n--- LIVE TOP 3 LEADERBOARD ---");
            var top3 = await redisRepo.GetTopAthletesAsync(3);

            foreach (var line in top3)
            {
                Console.WriteLine(line);
            }

            // 5. Перевірка конкретного місця
            string myPlayer = "Tyson Gay";
            var rank = await redisRepo.GetRankAsync(myPlayer);
            Console.WriteLine($"\nRank of {myPlayer}: #{rank}");

            // 6. Видалення (Дискваліфікація)
            Console.WriteLine($">> EVENT: {myPlayer} disqualified (doping)!");
            await redisRepo.RemoveAthleteAsync(myPlayer);

            // Перевіряємо топ знову
            Console.WriteLine("\n--- UPDATED TOP 3 ---");
            var updatedTop = await redisRepo.GetTopAthletesAsync(3);

            foreach (var line in updatedTop)
            {
                Console.WriteLine(line);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Error: {ex.Message}");
            Console.WriteLine("Make sure Redis is running in WSL (sudo service redis-server start)");
        }

        Console.ReadLine();
    }

    // --- МЕТОД ТЕСТУВАННЯ ШВИДКОДІЇ ---
    private static async Task RunBenchmark(IServiceProvider services)
    {
        var sqlContext = services.GetRequiredService<SportsDbContext>();
        var mongoRepo = services.GetRequiredService<IMatchDetailsRepository>();
        var random = new Random();

        Console.WriteLine("\n============================================");
        Console.WriteLine("       BENCHMARK: SQL vs NoSQL READ");
        Console.WriteLine("============================================");

        // 1. Prepare random IDs to fetch (to avoid caching bias)
        int numberOfReads = 1000; // Кількість запитів
        int maxId = 10000;        // Максимальний ID (скільки ми згенерували)
        var randomIds = new List<long>();
        for (int i = 0; i < numberOfReads; i++)
        {
            randomIds.Add(random.Next(1, maxId));
        }

        Console.WriteLine($"Executing {numberOfReads} reads for each database...");

        _ = await sqlContext.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == 1);

        var stopwatch = Stopwatch.StartNew();

        foreach (var id in randomIds)
        {
            // EF Core генерує SQL: SELECT * FROM Events JOIN MatchLogs ON ...
            var ev = await sqlContext.Events
                .AsNoTracking()
                .Include(e => e.MatchLogs)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        stopwatch.Stop();
        long sqlTime = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"[SQL] PostgreSQL Time: {sqlTime} ms");
        Console.WriteLine($"[SQL] Avg per request: {(double)sqlTime / numberOfReads:F3} ms");


        // --- TEST 2: MongoDB (Document Fetch) ---
        _ = await mongoRepo.GetByEventIdAsync(1);

        stopwatch.Restart();

        foreach (var id in randomIds)
        {
            var doc = await mongoRepo.GetByEventIdAsync(id);
        }

        stopwatch.Stop();
        long mongoTime = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"[NoSQL] MongoDB Time : {mongoTime} ms");
        Console.WriteLine($"[NoSQL] Avg per request: {(double)mongoTime / numberOfReads:F3} ms");

        // --- RESULTS ---
        Console.WriteLine("--------------------------------------------");
        if (mongoTime < sqlTime)
        {
            double factor = (double)sqlTime / mongoTime;
            Console.WriteLine($"RESULT: MongoDB is {factor:F1}x faster!");
        }
        else
        {
            Console.WriteLine("RESULT: SQL is faster (check indexes or data volume).");
        }
        Console.WriteLine("============================================");
    }

    // --- МЕТОД ГЕНЕРАЦІЇ ДАНИХ ---
    private static async Task SeedLargeData(IServiceProvider services)
    {
        var sqlContext = services.GetRequiredService<SportsDbContext>();
        var mongoRepo = services.GetRequiredService<IMatchDetailsRepository>();

        int eventsCount = 10000; // Кількість подій
        int batchSize = 1000;
        var random = new Random();

        Console.WriteLine($"\nПочинаємо генерацію {eventsCount} подій...");
        var stopwatch = Stopwatch.StartNew();

        for (int batch = 0; batch < eventsCount; batch += batchSize)
        {
            var sqlEvents = new List<Event>();
            var sqlLogs = new List<MatchLog>();
            var mongoDetailsList = new List<MatchDetails>();

            for (int i = 0; i < batchSize; i++)
            {
                long currentId = batch + i + 1; // Штучний ID

                // 1. Підготовка даних для SQL (Event)
                var newEvent = new Event
                {
                    EventName = $"Match #{currentId} - Team A vs Team B",
                    StartDate = DateTime.UtcNow.AddDays(random.Next(-100, 100)),
                    EndDate = DateTime.UtcNow.AddDays(random.Next(-100, 100)).AddHours(2),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                sqlEvents.Add(newEvent);

                // 2. Підготовка логів (спільні дані)
                var logsCount = random.Next(5, 25); // Від 5 до 25 подій у матчі
                var logsList = new List<MatchLogItem>(); // Для Mongo

                for (int j = 0; j < logsCount; j++)
                {
                    int minute = random.Next(1, 90);
                    string action = GetRandomAction(random);
                    string player = $"Player {random.Next(1, 22)}";

                    // Для SQL
                    sqlLogs.Add(new MatchLog
                    {
                        EventId = 0,
                        Minute = minute,
                        Action = action,
                        PlayerName = player
                    });

                    // Для Mongo
                    logsList.Add(new MatchLogItem { Minute = minute, Action = action, Player = player });
                }

                // 3. Підготовка даних для Mongo
                mongoDetailsList.Add(new MatchDetails
                {
                    SqlEventId = currentId,
                    Logs = logsList,
                    ExtraData = new BsonDocument { { "attendance", random.Next(1000, 80000) } }
                });
            }

            // --- ЗБЕРЕЖЕННЯ В SQL ---
            await sqlContext.Events.AddRangeAsync(sqlEvents);
            await sqlContext.SaveChangesAsync(); // Зберігаємо події, щоб отримати їх ID

            int logIndex = 0;
            foreach (var ev in sqlEvents)
            {
                int count = random.Next(5, 15);
                for (int k = 0; k < count; k++)
                {
                    sqlContext.MatchLogs.Add(new MatchLog
                    {
                        EventId = ev.Id,
                        Minute = random.Next(1, 90),
                        Action = GetRandomAction(random),
                        PlayerName = "Bot"
                    });
                }
            }
            await sqlContext.SaveChangesAsync();

            // --- ЗБЕРЕЖЕННЯ В MONGO ---
            for (int i = 0; i < batchSize; i++)
            {
                mongoDetailsList[i].SqlEventId = sqlEvents[i].Id;
            }

            foreach (var doc in mongoDetailsList)
            {
                await mongoRepo.CreateAsync(doc);
            }

            Console.WriteLine($"Пакет {batch + batchSize}/{eventsCount} збережено...");
        }

        stopwatch.Stop();
        Console.WriteLine($"\nГенерацію завершено за {stopwatch.Elapsed.TotalSeconds:F2} секунд.");
    }

    private static string GetRandomAction(Random rnd)
    {
        string[] actions = { "Pass", "Shot", "Goal", "Foul", "Yellow Card", "Corner", "Offside" };
        return actions[rnd.Next(actions.Length)];
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Налаштування SQL
                string connectionString = "Host=localhost;Database=sports_event_management_system;Username=postgres;Password=2325";
                services.AddDbContext<SportsDbContext>(options => options.UseNpgsql(connectionString));

                // Налаштування NoSQL
                services.AddSingleton<IMatchDetailsRepository>(sp => new MatchDetailsRepository("mongodb://localhost:27017"));

                // Решта репозиторіїв...
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            });
}