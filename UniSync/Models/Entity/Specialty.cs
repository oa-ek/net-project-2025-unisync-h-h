using System.ComponentModel.DataAnnotations;
using UniSync.Areas.Identity.Data;

namespace UniSync.Models.Entity
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public ICollection<UniSyncUser> Users { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}