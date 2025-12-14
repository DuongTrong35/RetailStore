using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailStore.Models
{
    [Table("reset_password_tokens")]
    public class ResetPasswordToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("expired_at")]
        public DateTime ExpiredAt { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; }

        // 🔗 Navigation
        public virtual User User { get; set; }
    }
}
