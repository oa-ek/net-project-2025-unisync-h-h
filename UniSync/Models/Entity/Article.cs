using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace UniSync.Models.Entity
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int? SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }

        public int? Course { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
