using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class Specialty
    {
        [Key]
        public uint Id { get; set; }

        [Required]
        public string Title { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
