using System.ComponentModel.DataAnnotations.Schema;

namespace SportsEventManager.Models
{
    [Table("Events")]
    public class Event
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("event_name")]
        public string EventName { get; set; } = string.Empty;

        [Column("start_date", TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date", TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("updated_by_user_id")]
        public long? UpdatedByUserId { get; set; }

        public List<MatchLog> MatchLogs { get; set; } = new();
    }
}