using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.Entity
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, /*Range(7, 20)*/]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int? SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }

        [Range(1, 10)]
        public int? Course { get; set; }

        public ICollection<Article> Articles { get; set; }
        public ICollection<Subject> Subjects { get; set; }
    }
}
