using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int? SubjectId { get; set; }
        public Subject Subject { get; set; }

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

        [Required]
        public string Progress { get; set; }

        public ICollection<TaskItem> Tasks { get; set; }
        public ICollection<Category> Categories { get; set; }
    }
}
