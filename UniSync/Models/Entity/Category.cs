using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
}
