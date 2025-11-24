using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SportsEventManager.Models
{
    public class MatchDetails
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("sql_event_id")]
        public long SqlEventId { get; set; }

        [BsonElement("match_log")]
        public List<MatchLogItem> Logs { get; set; } = new();

        [BsonExtraElements]
        public BsonDocument ExtraData { get; set; }
    }

    public class MatchLogItem
    {
        [BsonElement("minute")]
        public int Minute { get; set; }

        [BsonElement("action")]
        public string Action { get; set; } = string.Empty;

        [BsonElement("player")]
        public string Player { get; set; } = string.Empty;
    }
}