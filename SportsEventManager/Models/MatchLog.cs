using System.ComponentModel.DataAnnotations.Schema;

namespace SportsEventManager.Models
{
    [Table("MatchLogs")]
    public class MatchLog
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("event_id")]
        public long EventId { get; set; }

        [Column("minute")]
        public int Minute { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("player_name")]
        public string PlayerName { get; set; } = string.Empty;
    }
}