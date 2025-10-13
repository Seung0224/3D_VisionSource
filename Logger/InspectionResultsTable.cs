using Sunny.UI;
using System.ComponentModel;

namespace _3D_VisionSource
{
    // 한 행 데이터 모델
    public class DefectRow
    {
        public int No { get; set; }
        public double AreaMm2 { get; set; }
    }

    public static class InspectionResultsTable
    {
        public static BindingList<DefectRow> ToRows(InspectionResults res)
        {
            var rows = new BindingList<DefectRow>();
            if (res == null || res.CompAreaMm2 == null) return rows;

            for (int i = 0; i < res.CompAreaMm2.Count; i++)
                rows.Add(new DefectRow { No = i + 1, AreaMm2 = res.CompAreaMm2[i] });

            return rows;
        }

        public static void Bind(UIDataGridView grid, BindingList<DefectRow> rows)
        {
            grid.AutoGenerateColumns = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.Columns.Clear();

            // 두 컬럼만 추가
            var c1 = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = "No",
                DataPropertyName = "No",
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
            };
            var c2 = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = "Area (mm²)",
                DataPropertyName = "AreaMm2",
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = { Format = "F3" }
            };
            grid.Columns.Add(c1);
            grid.Columns.Add(c2);

            grid.DataSource = rows;
            grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
        }
    }
}
