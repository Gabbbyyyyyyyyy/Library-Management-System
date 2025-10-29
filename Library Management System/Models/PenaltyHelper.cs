using System;

namespace LibraryManagementSystem.Helpers
{
    public static class PenaltyHelper
    {
        /// <summary>
        /// Calculates penalty and days overdue for a borrowed book.
        /// </summary>
        /// <param name="dueDate">The due date of the borrowing.</param>
        /// <param name="returnDate">The return date; if null, uses current time.</param>
        /// <param name="penalty">Outputs the total penalty in PHP.</param>
        /// <param name="daysOverdue">Outputs the total days overdue.</param>
        public static void CalculatePenalty(DateTime dueDate, DateTime? returnDate, out double penalty, out int daysOverdue)
        {
            DateTime actualReturn = returnDate ?? DateTime.Now;
            penalty = 0;
            daysOverdue = 0;

            // Fine starts 1 hour after due date
            DateTime fineStart = dueDate.AddHours(1);

            if (actualReturn > fineStart)
            {
                TimeSpan diff = actualReturn - fineStart;
                int totalHours = (int)Math.Floor(diff.TotalHours);
                int totalDays = (int)Math.Floor(diff.TotalDays);

                penalty = (totalHours * 2) + (totalDays * 10);
                daysOverdue = (int)Math.Ceiling(diff.TotalDays);
            }
        }
    }
}
