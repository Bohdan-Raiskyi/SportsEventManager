using StackExchange.Redis;

namespace SportsEventManager.Repositories
{
    public interface ILeaderboardRepository
    {
        Task AddScoreAsync(string athleteName, double score);
        Task<List<string>> GetTopAthletesAsync(int count);
        Task<long?> GetRankAsync(string athleteName);
        Task RemoveAthleteAsync(string athleteName);
    }

    // Реалізація
    public class RedisLeaderboardRepository : ILeaderboardRepository
    {
        private readonly IDatabase _db;
        // Ключ, за яким у Redis буде зберігатися наша таблиця
        private const string LeaderboardKey = "live_leaderboard";

        public RedisLeaderboardRepository(string connectionString)
        {
            // Підключаємося до Redis з WSL
            var redis = ConnectionMultiplexer.Connect(connectionString);
            _db = redis.GetDatabase();
        }

        public async Task AddScoreAsync(string athleteName, double score)
        {
            await _db.SortedSetAddAsync(LeaderboardKey, athleteName, score);
        }

        public async Task<List<string>> GetTopAthletesAsync(int count)
        {
            var result = await _db.SortedSetRangeByRankWithScoresAsync(
                LeaderboardKey,
                start: 0,
                stop: count - 1,
                order: Order.Descending
            );

            // Перетворюємо результат Redis у звичайний список рядків
            return result.Select((x, i) =>
                $"#{i + 1} {x.Element}: {x.Score} pts"
            ).ToList();
        }
        public async Task<long?> GetRankAsync(string athleteName)
        {
            var rank = await _db.SortedSetRankAsync(LeaderboardKey, athleteName, Order.Descending);

            if (rank.HasValue) return rank.Value + 1;
            return null; // Атлета не знайдено
        }

        public async Task RemoveAthleteAsync(string athleteName)
        {
            await _db.SortedSetRemoveAsync(LeaderboardKey, athleteName);
        }
    }
}