using System;

namespace LibraryManagementSystem.Helpers
{
    public static class PenaltyHelper
    {
        /// <summary>
        /// Calculates penalty based on library hours (7am–5pm = 10 hours/day)
        /// ₱2 per overdue hour (<10 hrs), ₱10 per full day (≥10 hrs), ignoring leftover hours.
        /// Grace period: 8:00–8:59 AM on due date.
        /// Counts strictly library hours only.
        /// </summary>
        public static void CalculatePenalty(DateTime dueDate, DateTime? returnDate,
            out double totalPenalty, out int daysOverdue, out int hoursOverdue)
        {
            DateTime endDate = returnDate ?? DateTime.Now;

            TimeSpan opening = TimeSpan.FromHours(7);   // 7:00 AM
            TimeSpan closing = TimeSpan.FromHours(17);  // 5:00 PM
            int libraryHoursPerDay = 10;

            // --- Adjust due date to standardized 8:00–8:59 AM range ---
            if (dueDate.TimeOfDay < TimeSpan.FromHours(8) || dueDate.TimeOfDay >= TimeSpan.FromHours(9))
                dueDate = dueDate.Date.AddHours(8);

            // --- Grace period until 8:59 AM ---
            DateTime graceEnd = dueDate.Date.AddHours(8).AddMinutes(59);
            if (endDate <= graceEnd)
            {
                totalPenalty = 0;
                daysOverdue = 0;
                hoursOverdue = 0;
                return;
            }

            // --- Start counting penalty from 9:00 AM ---
            DateTime penaltyStart = dueDate.Date.AddHours(9);
            if (endDate <= penaltyStart)
            {
                totalPenalty = 0;
                daysOverdue = 0;
                hoursOverdue = 0;
                return;
            }

            // --- Count total library hours between penaltyStart and endDate ---
            int totalLibraryHours = 0;
            DateTime cursor = penaltyStart;

            while (cursor < endDate)
            {
                // Only count hours inside library open hours
                if (cursor.TimeOfDay >= opening && cursor.TimeOfDay < closing)
                    totalLibraryHours++;

                // Move cursor to next hour
                cursor = cursor.AddHours(1);
            }

            // --- Compute penalties ---
            if (totalLibraryHours >= libraryHoursPerDay)
            {
                // Full days only, ignore leftover hours
                daysOverdue = totalLibraryHours / libraryHoursPerDay;
                hoursOverdue = 0;
                totalPenalty = daysOverdue * 10;
            }
            else
            {
                daysOverdue = 0;
                hoursOverdue = totalLibraryHours;
                totalPenalty = hoursOverdue * 2;
            }
        }
    }
}
