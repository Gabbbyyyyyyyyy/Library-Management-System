using System;

namespace LibraryManagementSystem.Helpers
{
    public static class PenaltyHelper
    {
        /// <summary>
        /// Calculates the total penalty based on overdue time.
        /// ₱2 per hour if overdue is less than 1 full day.
        /// ₱10 per day if overdue is 1 day or more.
        /// Hours are ignored once it reaches 1 full day.
        /// </summary>
        public static void CalculatePenalty(DateTime dueDate, DateTime? returnDate, out double totalPenalty, out int daysOverdue, out int hoursOverdue)
        {
            DateTime endDate = returnDate ?? DateTime.Now;
            TimeSpan diff = endDate - dueDate;

            // If not overdue at all
            if (diff.TotalMinutes <= 0)
            {
                totalPenalty = 0;
                daysOverdue = 0;
                hoursOverdue = 0;
                return;
            }

            if (diff.TotalDays >= 1)
            {
                // 1 day or more overdue → ₱10 per full day
                daysOverdue = (int)Math.Floor(diff.TotalDays);
                totalPenalty = daysOverdue * 10;
                hoursOverdue = 0; // ignore extra hours
            }
            else
            {
                // Less than 1 full day → ₱2 per hour
                daysOverdue = 0;
                hoursOverdue = (int)Math.Ceiling(diff.TotalHours);
                totalPenalty = hoursOverdue * 2;
            }
        }
    }
}
