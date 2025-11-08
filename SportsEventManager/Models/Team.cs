using System.ComponentModel.DataAnnotations.Schema;

namespace SportsEventManager.Models
{
    [Table("Teams")] // назва таблиці
    public class Team
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("team_name")]
        public string TeamName { get; set; } = string.Empty;

        [Column("sport_id")]
        public int SportId { get; set; }

        [Column("coach_id")]
        public long? CoachId { get; set; }

        [Column("country_id")]
        public int? CountryId { get; set; }

        // Поля аудиту, які повертає View
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("updated_by_user_id")]
        public long? UpdatedByUserId { get; set; }
    }
}