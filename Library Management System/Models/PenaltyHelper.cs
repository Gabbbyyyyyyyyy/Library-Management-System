using System;

namespace LibraryManagementSystem.Helpers
{
    public static class PenaltyHelper
    {
        /// <summary>
        /// Calculates penalty based on library hours (7am to 5pm = 10 hours/day)
        /// ₱2 per overdue hour if less than a full day, ₱10 per full day if 10+ hours.
        /// Only counts library open hours (7am-5pm).
        /// </summary>
        public static void CalculatePenalty(DateTime dueDate, DateTime? returnDate, out double totalPenalty, out int daysOverdue, out int hoursOverdue)
        {
            DateTime endDate = returnDate ?? DateTime.Now;

            TimeSpan opening = TimeSpan.FromHours(7);   // 7:00 AM
            TimeSpan closing = TimeSpan.FromHours(17);  // 5:00 PM
            int libraryHoursPerDay = 10;

            // Adjust due date: cannot be before 7 AM
            if (dueDate.TimeOfDay < opening)
                dueDate = dueDate.Date + opening;

            // If returned before due date → no penalty
            if (endDate <= dueDate)
            {
                totalPenalty = 0;
                daysOverdue = 0;
                hoursOverdue = 0;
                return;
            }

            // Count library hours between dueDate and returnDate
            double totalLibraryHours = 0;
            DateTime cursor = dueDate;

            while (cursor.Date <= endDate.Date)
            {
                // Determine start of counting for this day
                DateTime dayStart = cursor.Date + opening;
                DateTime dayEnd = cursor.Date + closing;

                // If first day, start from dueDate time
                if (cursor.Date == dueDate.Date)
                    dayStart = cursor;

                // If last day, end at return time
                if (cursor.Date == endDate.Date)
                    dayEnd = endDate;

                // Only count if dayEnd > dayStart
                if (dayEnd > dayStart)
                    totalLibraryHours += (dayEnd - dayStart).TotalHours;

                cursor = cursor.AddDays(1).Date; // move to next day
            }

            // Calculate full days and leftover hours
            if (totalLibraryHours >= libraryHoursPerDay)
            {
                daysOverdue = (int)Math.Floor(totalLibraryHours / libraryHoursPerDay);
                hoursOverdue = 0;
                totalPenalty = daysOverdue * 10;
            }
            else
            {
                daysOverdue = 0;
                hoursOverdue = (int)Math.Ceiling(totalLibraryHours);

                // If still in the first hour (7:00–7:59) → no penalty
                if (totalLibraryHours < 1)
                    hoursOverdue = 0;

                totalPenalty = hoursOverdue * 2;
            }
        }
    }
}
