using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using UniSync.Models.Entity;

namespace UniSync.ViewModels
{
    public class NewsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок є обов'язковим")]
        [StringLength(100, ErrorMessage = "Заголовок не може перевищувати 100 символів")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Зміст є обов'язковим")]
        [Display(Name = "Зміст")]
        public string Content { get; set; }

        [Display(Name = "Короткий опис")]
        [StringLength(200, ErrorMessage = "Короткий опис не може перевищувати 200 символів")]
        public string Summary { get; set; }

        [Display(Name = "Важлива новина")]
        public bool IsImportant { get; set; }

        [Display(Name = "Зображення")]
        public IFormFile Image { get; set; }

        [Display(Name = "Категорія")]
        public NewsCategory Category { get; set; }
    }
}