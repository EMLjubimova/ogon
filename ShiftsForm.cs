using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class ShiftsForm : Form
    {
        private DataGridView grid;
        private DateTimePicker dtFrom, dtTo;
        private Button btnAdd, btnDelete;

        public ShiftsForm()
        {
            this.Text = "График дежурств";
            this.Size = new Size(950, 620);
            this.AutoScaleMode = AutoScaleMode.Dpi;      // ← добавить
            this.MinimumSize = new Size(800, 500);        // ← добавить
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "График дежурств сотрудников",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(title);

            this.Controls.Add(new Label { Text = "С:", Location = new Point(20, 55), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            dtFrom = new DateTimePicker { Location = new Point(40, 50), Size = new Size(150, 25), Font = new Font("Segoe UI", 10), Value = DateTime.Today.AddDays(-7) };
            dtFrom.ValueChanged += (s, e) => LoadData();
            this.Controls.Add(dtFrom);

            this.Controls.Add(new Label { Text = "По:", Location = new Point(205, 55), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            dtTo = new DateTimePicker { Location = new Point(230, 50), Size = new Size(150, 25), Font = new Font("Segoe UI", 10), Value = DateTime.Today.AddDays(7) };
            dtTo.ValueChanged += (s, e) => LoadData();
            this.Controls.Add(dtTo);

            btnAdd = MakeButton("✚ Назначить смену", 400, 48, "#A89080", btnAdd_Click);
            btnDelete = MakeButton("✕ Удалить", 580, 48, "#D4B8B0", btnDelete_Click);

            grid = new DataGridView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 95),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 115),
                BackgroundColor = ColorTranslator.FromHtml("#F5EFE8"),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 10),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#C8B8A2");
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.Controls.Add(grid);
        }

        private Button MakeButton(string text, int x, int y, string color, EventHandler handler)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(170, 32),
                BackColor = ColorTranslator.FromHtml(color),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += handler;
            this.Controls.Add(btn);
            return btn;
        }

        private void LoadData()
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                var query = @"SELECT tr.Id,
                    e.LastName + ' ' + e.FirstName AS Сотрудник,
                    p.Name AS Должность,
                    tr.Date AS Дата,
                    s.Name AS Смена,
                    s.StartTime AS [Начало],
                    s.EndTime AS [Конец],
                    tr.Status AS Статус
                    FROM TimeRecords tr
                    JOIN Employees e ON tr.EmployeeId = e.Id
                    JOIN Positions p ON e.PositionId = p.Id
                    LEFT JOIN Shifts s ON tr.ShiftId = s.Id
                    WHERE tr.Date BETWEEN @from AND @to
                    ORDER BY tr.Date, e.LastName";
                var da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@from", dtFrom.Value.Date);
                da.SelectCommand.Parameters.AddWithValue("@to", dtTo.Value.Date);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                if (grid.Columns.Contains("Id"))
                    grid.Columns["Id"].Visible = false;
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new TimeRecordEditForm(DateTime.Today);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Выберите запись!"); return; }
            if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            using var conn = Database.GetConnection();
            conn.Open();
            new SqlCommand($"DELETE FROM TimeRecords WHERE Id={id}", conn).ExecuteNonQuery();
            LoadData();
        }
    }
}