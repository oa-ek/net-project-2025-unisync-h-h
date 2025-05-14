using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniSync.Areas.Identity.Data;

namespace UniSync.Models.Entity
{
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок є обов'язковим")]
        [StringLength(100, ErrorMessage = "Заголовок не може перевищувати 100 символів")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Зміст є обов'язковим")]
        public string Content { get; set; }

        [Display(Name = "Короткий опис")]
        [StringLength(200, ErrorMessage = "Короткий опис не може перевищувати 200 символів")]
        public string Summary { get; set; }

        [Display(Name = "Дата публікації")]
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [Display(Name = "Важлива новина")]
        public bool IsImportant { get; set; }

        [Display(Name = "Автор")]
        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public string AuthorName { get; set; } // Залишаємо для кешування імені

        [Display(Name = "Зображення")]
        [StringLength(500, ErrorMessage = "URL зображення не може перевищувати 500 символів")]
        public string ImageUrl { get; set; }

        [Display(Name = "Категорія")]
        public NewsCategory Category { get; set; }

        public UniSyncUser Author { get; set; }
    }

    public enum NewsCategory
    {
        [Display(Name = "Загальні")]
        General,
        [Display(Name = "Навчання")]
        Academic,
        [Display(Name = "Події")]
        Events,
        [Display(Name = "Оголошення")]
        Announcements
    }
}