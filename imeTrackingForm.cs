using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class TimeTrackingForm : Form
    {
        private DataGridView grid;
        private DateTimePicker dtPicker;
        private ComboBox cmbEmployee;
        private Button btnMark;

        public TimeTrackingForm()
        {
            this.Text = "Учёт рабочего времени";
            this.Size = new Size(950, 620);
            this.AutoScaleMode = AutoScaleMode.Dpi;      // ← добавить
            this.MinimumSize = new Size(800, 500);        // ← добавить
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadEmployees();
            LoadData();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Журнал учёта рабочего времени",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(title);

            // Фильтр по дате
            this.Controls.Add(new Label { Text = "Дата:", Location = new Point(20, 55), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            dtPicker = new DateTimePicker { Location = new Point(60, 50), Size = new Size(160, 25), Font = new Font("Segoe UI", 10), Value = DateTime.Today };
            dtPicker.ValueChanged += (s, e) => LoadData();
            this.Controls.Add(dtPicker);

            // Фильтр по сотруднику
            this.Controls.Add(new Label { Text = "Сотрудник:", Location = new Point(240, 55), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            cmbEmployee = new ComboBox { Location = new Point(320, 50), Size = new Size(220, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEmployee.SelectedIndexChanged += (s, e) => LoadData();
            this.Controls.Add(cmbEmployee);

            btnMark = new Button
            {
                Text = "✚ Добавить",
                Location = new Point(560, 48),
                Size = new Size(110, 32),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnMark.FlatAppearance.BorderSize = 0;
            btnMark.Click += btnMark_Click;
            this.Controls.Add(btnMark);

            var btnEdit = new Button
            {
                Text = "✏ Статус",
                Location = new Point(680, 48),
                Size = new Size(110, 32),
                BackColor = ColorTranslator.FromHtml("#B8C4B8"),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += btnEdit_Click;
            this.Controls.Add(btnEdit);

            var btnDelete = new Button
            {
                Text = "✕ Удалить",
                Location = new Point(800, 48),
                Size = new Size(110, 32),
                BackColor = ColorTranslator.FromHtml("#D4B8B0"),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += btnDelete_Click;
            this.Controls.Add(btnDelete);

            // Таблица
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

        private void LoadEmployees()
        {
            cmbEmployee.Items.Clear();
            cmbEmployee.Items.Add(new { Id = 0, Name = "Все сотрудники" });
            using var conn = Database.GetConnection();
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, LastName + ' ' + FirstName AS Name FROM Employees WHERE IsActive=1 ORDER BY LastName", conn);
            var r = cmd.ExecuteReader();
            while (r.Read())
                cmbEmployee.Items.Add(new { Id = r.GetInt32(0), Name = r.GetString(1) });
            cmbEmployee.DisplayMember = "Name";
            cmbEmployee.SelectedIndex = 0;
        }

        private void LoadData()
        {
            try
            {
                dynamic sel = cmbEmployee.SelectedItem;
                int empId = sel?.Id ?? 0;
                DateTime date = dtPicker.Value.Date;

                using var conn = Database.GetConnection();
                conn.Open();
                string query = @"SELECT tr.Id,
                    e.LastName + ' ' + e.FirstName AS Сотрудник,
                    p.Name AS Должность,
                    tr.Date AS Дата,
                    s.Name AS Смена,
                    CONVERT(varchar, tr.CheckIn, 108) AS Приход,
                    CONVERT(varchar, tr.CheckOut, 108) AS Уход,
                    tr.ActualHours AS [Часов отработано],
                    tr.Status AS Статус,
                    tr.Notes AS Примечание
                    FROM TimeRecords tr
                    JOIN Employees e ON tr.EmployeeId = e.Id
                    JOIN Positions p ON e.PositionId = p.Id
                    LEFT JOIN Shifts s ON tr.ShiftId = s.Id
                    WHERE tr.Date = @date AND (@empId = 0 OR tr.EmployeeId = @empId)
                    ORDER BY e.LastName";
                var da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@date", date);
                da.SelectCommand.Parameters.AddWithValue("@empId", empId);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                if (grid.Columns.Contains("Id"))
                    grid.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите запись в таблице!", "Внимание");
                return;
            }

            int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            string currentStatus = grid.SelectedRows[0].Cells["Статус"].Value?.ToString() ?? "";

            // Окно выбора статуса
            var form = new Form
            {
                Text = "Изменить статус",
                Size = new Size(320, 220),
                BackColor = ColorTranslator.FromHtml("#E8DDD0"),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            form.Controls.Add(new Label
            {
                Text = "Выберите новый статус:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml("#4A3F35")
            });

            var cmb = new ComboBox
            {
                Location = new Point(20, 50),
                Size = new Size(260, 28),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmb.Items.AddRange(new[] {
        "Отработано",
        "Отсутствие",
        "Нарушение (не явился)",
        "Больничный",
        "Отпуск"
    });
            cmb.Text = currentStatus;
            form.Controls.Add(cmb);

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(20, 100),
                Size = new Size(260, 38),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, ev) =>
            {
                if (cmb.SelectedItem == null) { MessageBox.Show("Выберите статус!"); return; }
                using var conn = Database.GetConnection();
                conn.Open();
                var cmd = new SqlCommand("UPDATE TimeRecords SET Status=@status WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@status", cmb.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                form.Close();
                LoadData();
                MessageBox.Show("Статус обновлён!", "Готово");
            };
            form.Controls.Add(btnSave);
            form.ShowDialog();
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
        private void btnMark_Click(object sender, EventArgs e)
        {
            var form = new TimeRecordEditForm(dtPicker.Value.Date);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }
    }
}