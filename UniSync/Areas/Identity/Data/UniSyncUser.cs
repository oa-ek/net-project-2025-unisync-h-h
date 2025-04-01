using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using UniSync.Models.Entity;

namespace UniSync.Areas.Identity.Data
{
    public class UniSyncUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }

        [PersonalData]
        public string LastName { get; set; }

        public int? SpecialtyId { get; set; }
        public Specialty? Specialty { get; set; }

        [Range(1, 10)]
        public int? Course { get; set; }

        public ICollection<Article> Articles { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Subject> Subjects { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
}