using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class EmployeesForm : Form
    {
        private DataGridView grid;
        private TextBox searchBox;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;

        public EmployeesForm()
        {
            this.Text = "Сотрудники";
            this.MinimumSize = new Size(700, 500);
            this.Size = new Size(900, 600);
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            BuildUI();
            LoadData();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Список сотрудников",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(title);

            searchBox = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Поиск по фамилии..."
            };
            searchBox.TextChanged += (s, e) => LoadData(searchBox.Text);
            this.Controls.Add(searchBox);

            btnAdd = MakeButton("+ Добавить", 340, 48, "#A89080", btnAdd_Click);
            btnEdit = MakeButton("✏ Изменить", 490, 48, "#B8C4B8", btnEdit_Click);
            btnDelete = MakeButton("✕ Удалить", 640, 48, "#D4B8B0", btnDelete_Click);

            grid = new DataGridView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(20, 90),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 110),
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
                Size = new Size(140, 32),
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

        private void LoadData(string search = "")
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                var query = @"SELECT e.Id, e.LastName AS Фамилия, e.FirstName AS Имя, 
                              e.MiddleName AS Отчество, p.Name AS Должность,
                              e.PersonnelNumber AS [Таб. номер], e.HireDate AS [Дата приёма]
                              FROM Employees e
                              LEFT JOIN Positions p ON e.PositionId = p.Id
                              WHERE e.IsActive = 1 AND e.LastName LIKE @search
                              ORDER BY e.LastName";
                var da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", "%" + search + "%");
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                if (grid.Columns.Contains("Id"))
                    grid.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new EmployeeEditForm(0);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Выберите сотрудника!"); return; }
            int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            var form = new EmployeeEditForm(id);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Выберите сотрудника!"); return; }
            if (MessageBox.Show("Удалить сотрудника?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            using var conn = Database.GetConnection();
            conn.Open();
            new SqlCommand($"UPDATE Employees SET IsActive=0 WHERE Id={id}", conn).ExecuteNonQuery();
            LoadData();
        }
    }
}