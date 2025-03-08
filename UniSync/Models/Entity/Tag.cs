using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}
