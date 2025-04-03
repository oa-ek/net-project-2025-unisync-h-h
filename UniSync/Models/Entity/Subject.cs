using System.ComponentModel.DataAnnotations;
using UniSync.Areas.Identity.Data;

namespace UniSync.Models.Entity
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

        // Навігаційна властивість до користувача
        public UniSyncUser User { get; set; }
    }
}