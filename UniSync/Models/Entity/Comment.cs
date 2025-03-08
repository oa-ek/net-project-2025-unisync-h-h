using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }
        public Article Article { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
