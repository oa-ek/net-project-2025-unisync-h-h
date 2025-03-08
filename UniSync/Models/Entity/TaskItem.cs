using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int Priority { get; set; }

        public DateTime? Deadline { get; set; }

        [Required]
        public string Status { get; set; }

        public string? Description { get; set; }   
    }
}
