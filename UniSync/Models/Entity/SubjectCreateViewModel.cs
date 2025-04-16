using System;
using System.ComponentModel.DataAnnotations;

namespace UniSync.Models.ViewModels
{
    public class SubjectCreateViewModel
    {
        [Required(ErrorMessage = "Назва предмета обов'язкова")]
        public string Title { get; set; }
    }
}