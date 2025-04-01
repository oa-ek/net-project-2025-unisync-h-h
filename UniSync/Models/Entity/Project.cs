using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniSync.Areas.Identity.Data;

namespace UniSync.Models.Entity
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        [Display(Name = "Created By")]
        [StringLength(450)]
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public virtual UniSyncUser User { get; set; }

        [Required]
        [Display(Name = "Created At")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium";

        [Display(Name = "Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Required]
        public string Status { get; set; } = "Not Started";

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Progress")]
        [Range(0, 100)]
        public int Progress { get; set; } = 0;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}