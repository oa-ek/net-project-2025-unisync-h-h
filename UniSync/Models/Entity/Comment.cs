using System.ComponentModel.DataAnnotations;
using UniSync.Areas.Identity.Data;

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
        public string UserId { get; set; }
        public UniSyncUser User { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}