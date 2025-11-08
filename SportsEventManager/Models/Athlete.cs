using System.ComponentModel.DataAnnotations.Schema;

namespace SportsEventManager.Models
{
    [Table("Athletes")] // назва таблиці
    public class Athlete
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        public long? UserId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("country_id")]
        public int CountryId { get; set; }

        // Поля аудиту, які повертає View
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("updated_by_user_id")]
        public long? UpdatedByUserId { get; set; }
    }
}