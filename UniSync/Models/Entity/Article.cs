using System.ComponentModel.DataAnnotations;
using UniSync.Areas.Identity.Data;

namespace UniSync.Models.Entity
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public UniSyncUser User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int? SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }

        public int? Course { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}