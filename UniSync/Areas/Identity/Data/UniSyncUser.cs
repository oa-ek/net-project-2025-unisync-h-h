using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UniSync.Models.Entity;

namespace UniSync.Areas.Identity.Data;

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
}

