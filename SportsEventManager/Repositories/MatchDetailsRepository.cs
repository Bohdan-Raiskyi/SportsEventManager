using MongoDB.Driver;
using SportsEventManager.Models;

namespace SportsEventManager.Repositories
{
    public class MatchDetailsRepository : IMatchDetailsRepository
    {
        private readonly IMongoCollection<MatchDetails> _collection;

        public MatchDetailsRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("SportsNoSQL"); // Назва бази
            _collection = database.GetCollection<MatchDetails>("match_logs"); // Назва колекції (таблиці)

            var indexKeys = Builders<MatchDetails>.IndexKeys.Ascending(x => x.SqlEventId);
            var indexModel = new CreateIndexModel<MatchDetails>(indexKeys);
            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task CreateAsync(MatchDetails details)
        {
            await _collection.InsertOneAsync(details);
        }

        public async Task<MatchDetails?> GetByEventIdAsync(long sqlEventId)
        {
            // Знайти документ, де sql_event_id == параметру
            return await _collection.Find(x => x.SqlEventId == sqlEventId).FirstOrDefaultAsync();
        }
    }
}