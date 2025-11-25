using System.Drawing;
using System.Windows.Forms;

namespace Library_Management_System.Models
{
    internal static class DataGridViewHelper
    {
        public static void ApplyDefaultStyle(DataGridView dgv)
        {
            // Common settings
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;

       

            // ★ Make all rows same background
            Color rowColor = Color.FromArgb(242, 229, 217);

            dgv.BackgroundColor = rowColor;
            dgv.DefaultCellStyle.BackColor = rowColor;
            dgv.RowsDefaultCellStyle.BackColor = rowColor;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = rowColor;  // remove stripe
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 200, 185); // optional highlight
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Header styling
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = rowColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        }
    }
}
