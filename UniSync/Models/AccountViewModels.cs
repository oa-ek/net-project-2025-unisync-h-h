using System;

namespace UniSync.Models.ViewModels
{
    public class LockedAccountViewModel
    {
        public string UserName { get; set; } = string.Empty; // Fix: Initialize with a default value
        public string Email { get; set; } = string.Empty; // Fix: Initialize with a default value
        public DateTimeOffset? LockoutEnd { get; set; }
        public string LockoutReason { get; set; } = string.Empty; // Fix: Initialize with a default value

        public bool IsPermanent => LockoutEnd?.Year > DateTime.Now.Year + 10;

        public string? FormattedLockoutEnd => IsPermanent
            ? "Назавжди"
            : LockoutEnd?.LocalDateTime.ToString("dd.MM.yyyy HH:mm");

        public string TimeRemaining
        {
            get
            {
                if (IsPermanent) return "Назавжди";
                if (!LockoutEnd.HasValue) return "Невідомо";

                var timeLeft = LockoutEnd.Value - DateTimeOffset.Now;

                if (timeLeft.TotalDays > 1)
                {
                    int days = (int)Math.Floor(timeLeft.TotalDays);
                    return $"{days} {GetDaysText(days)}";
                }
                else if (timeLeft.TotalHours > 1)
                {
                    int hours = (int)Math.Floor(timeLeft.TotalHours);
                    return $"{hours} {GetHoursText(hours)}";
                }
                else
                {
                    int minutes = (int)Math.Floor(timeLeft.TotalMinutes);
                    return $"{minutes} {GetMinutesText(minutes)}";
                }
            }
        }

        private string GetDaysText(int days)
        {
            if (days % 10 == 1 && days % 100 != 11) return "день";
            else if (days % 10 >= 2 && days % 10 <= 4 && (days % 100 < 12 || days % 100 > 14)) return "дні";
            else return "днів";
        }

        private string GetHoursText(int hours)
        {
            if (hours % 10 == 1 && hours % 100 != 11) return "година";
            else if (hours % 10 >= 2 && hours % 10 <= 4 && (hours % 100 < 12 || hours % 100 > 14)) return "години";
            else return "годин";
        }

        private string GetMinutesText(int minutes)
        {
            if (minutes % 10 == 1 && minutes % 100 != 11) return "хвилина";
            else if (minutes % 10 >= 2 && minutes % 10 <= 4 && (minutes % 100 < 12 || minutes % 100 > 14)) return "хвилини";
            else return "хвилин";
        }
    }
}