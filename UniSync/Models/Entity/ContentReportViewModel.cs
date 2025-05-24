using System.ComponentModel.DataAnnotations;
using UniSync.Models.Entity;

namespace UniSync.ViewModels
{
    public class ContentReportViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Тип контенту є обов'язковим")]
        [Display(Name = "Тип контенту")]
        public ContentType ContentType { get; set; }

        [Required(ErrorMessage = "ID контенту є обов'язковим")]
        [Display(Name = "ID контенту")]
        public int ContentId { get; set; }

        [Required(ErrorMessage = "Причина є обов'язковою")]
        [Display(Name = "Причина")]
        public ReportReason Reason { get; set; }

        [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        [Display(Name = "Додатковий опис")]
        public string Description { get; set; }

        // Поля для модератора
        [Display(Name = "Статус")]
        public ReportStatus Status { get; set; }

        [StringLength(500, ErrorMessage = "Коментар не може перевищувати 500 символів")]
        [Display(Name = "Коментар модератора")]
        public string ModeratorComment { get; set; }
    }
}