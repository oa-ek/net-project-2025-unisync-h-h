using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class Subject
    {
        [Key]
        public uint Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
