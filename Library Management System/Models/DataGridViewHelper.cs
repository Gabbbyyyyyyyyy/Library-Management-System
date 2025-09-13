using System.Windows.Forms;

namespace Library_Management_System.Models
{
    internal static class DataGridViewHelper
    {
        public static void ApplyDefaultStyle(DataGridView dgv)
        {
            // ✅ Common settings
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ✅ Optional styling (can be removed if you want plain look)
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(245, 245, 245); // Light gray alternate rows
            dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);      // Dark header
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgv.EnableHeadersVisualStyles = false;
        }
    }
}
