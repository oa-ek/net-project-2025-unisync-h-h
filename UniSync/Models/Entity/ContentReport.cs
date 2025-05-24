using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniSync.Areas.Identity.Data;

namespace UniSync.Models.Entity
{
    public class ContentReport
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

        [Required]
        [Display(Name = "Дата створення")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Статус")]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [Display(Name = "Дата обробки")]
        public DateTime? ResolvedAt { get; set; }

        [Display(Name = "Коментар модератора")]
        [StringLength(500, ErrorMessage = "Коментар не може перевищувати 500 символів")]
        public string ModeratorComment { get; set; }

        [ForeignKey("Reporter")]
        [Display(Name = "Відправник")]
        public string ReporterId { get; set; }

        [ForeignKey("Moderator")]
        [Display(Name = "Модератор")]
        public string ModeratorId { get; set; }

        public UniSyncUser Reporter { get; set; }
        public UniSyncUser Moderator { get; set; }

        [NotMapped]
        public News ReportedNews { get; set; }
    }

    public enum ContentType
    {
        [Display(Name = "Новина")]
        News,
        [Display(Name = "Коментар")]
        Comment,
        [Display(Name = "Профіль користувача")]
        UserProfile,
        // Додайте інші типи контенту за необхідності
    }

    public enum ReportReason
    {
        [Display(Name = "Спам")]
        Spam,
        [Display(Name = "Образливий вміст")]
        Offensive,
        [Display(Name = "Недостовірна інформація")]
        Misinformation,
        [Display(Name = "Порушення правил")]
        RuleViolation,
        [Display(Name = "Інше")]
        Other
    }

    public enum ReportStatus
    {
        [Display(Name = "Очікує розгляду")]
        Pending,
        [Display(Name = "Розглянуто - Порушення")]
        Confirmed,
        [Display(Name = "Розглянуто - Не порушення")]
        Rejected,
        [Display(Name = "Вирішено")]
        Resolved
    }
}