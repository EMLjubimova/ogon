using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class ReportsForm : Form
    {
        private DataGridView grid;
        private DateTimePicker dtFrom, dtTo;
        private ComboBox cmbEmployee;
        private Label lblTotalHours, lblTotalDays, lblAbsent, lblViolations;
        private Button btnGenerate;

        public ReportsForm()
        {
            this.Text = "Отчёты";
            this.Size = new Size(950, 650);
            this.AutoScaleMode = AutoScaleMode.Dpi;      // ← добавить
            this.MinimumSize = new Size(800, 500);        // ← добавить
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            BuildUI();
            LoadEmployees();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Отчёт по рабочему времени",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(title);

            // Фильтры
            this.Controls.Add(new Label { Text = "С:", Location = new Point(20, 58), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            dtFrom = new DateTimePicker { Location = new Point(40, 53), Size = new Size(150, 25), Font = new Font("Segoe UI", 10), Value = DateTime.Today.AddMonths(-1) };
            this.Controls.Add(dtFrom);

            this.Controls.Add(new Label { Text = "По:", Location = new Point(205, 58), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            dtTo = new DateTimePicker { Location = new Point(228, 53), Size = new Size(150, 25), Font = new Font("Segoe UI", 10), Value = DateTime.Today };
            this.Controls.Add(dtTo);

            this.Controls.Add(new Label { Text = "Сотрудник:", Location = new Point(395, 58), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            cmbEmployee = new ComboBox { Location = new Point(470, 53), Size = new Size(200, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbEmployee);

            btnGenerate = new Button
            {
                Text = "📊 Сформировать",
                Location = new Point(685, 51),
                Size = new Size(160, 32),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += (s, e) => LoadReport();
            this.Controls.Add(btnGenerate);

            // Карточки итогов
            lblTotalDays = AddStatCard("Дней отработано", "—", 20, 100, "#C8B8A2");
            lblTotalHours = AddStatCard("Часов всего", "—", 220, 100, "#B8C4B8");
            lblAbsent = AddStatCard("Дней отсутствия", "—", 420, 100, "#D4B8B0");
            lblViolations = AddStatCard("Нарушений", "—", 620, 100, "#C4A8A8");

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

        private Label AddStatCard(string title, string value, int x, int y, string color)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(180, 80),
                BackColor = ColorTranslator.FromHtml(color)
            };
            card.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(10, 8)
            });
            var valLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(10, 28)
            };
            card.Controls.Add(valLabel);
            this.Controls.Add(card);
            return valLabel;
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

        private void LoadReport()
        {
            try
            {
                dynamic sel = cmbEmployee.SelectedItem;
                int empId = sel?.Id ?? 0;

                using var conn = Database.GetConnection();
                conn.Open();

                // Основная таблица
                var query = @"SELECT
                    e.LastName + ' ' + e.FirstName AS Сотрудник,
                    p.Name AS Должность,
                    COUNT(CASE WHEN tr.Status='Отработано' THEN 1 END) AS [Дней отработано],
                    ROUND(SUM(CASE WHEN tr.Status='Отработано' THEN tr.ActualHours ELSE 0 END), 1) AS [Часов отработано],
                    COUNT(CASE WHEN tr.Status='Больничный' THEN 1 END) AS [Больничных],
                    COUNT(CASE WHEN tr.Status='Отпуск' THEN 1 END) AS [Дней отпуска],
                    COUNT(CASE WHEN tr.Status='Отгул' THEN 1 END) AS [Отгулов],
                    COUNT(CASE WHEN tr.Status='Учебный отпуск' THEN 1 END) AS [Учебных отпусков],
                    COUNT(CASE WHEN tr.Status='Командировка' THEN 1 END) AS [Командировок],
                    COUNT(CASE WHEN tr.Status='Административный отпуск' THEN 1 END) AS [Адм. отпусков],
                    COUNT(CASE WHEN tr.Status='Отсутствие' THEN 1 END) AS [Отсутствий],
                    COUNT(CASE WHEN tr.Status='Нарушение (не явился)' THEN 1 END) AS [Нарушений]
                    FROM TimeRecords tr
                    JOIN Employees e ON tr.EmployeeId = e.Id
                    JOIN Positions p ON e.PositionId = p.Id
                    WHERE tr.Date BETWEEN @from AND @to
                    AND (@empId = 0 OR tr.EmployeeId = @empId)
                    GROUP BY e.Id, e.LastName, e.FirstName, p.Name
                    ORDER BY e.LastName";

                var da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@from", dtFrom.Value.Date);
                da.SelectCommand.Parameters.AddWithValue("@to", dtTo.Value.Date);
                da.SelectCommand.Parameters.AddWithValue("@empId", empId);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;

                // Итоги в карточках
                int totalDays = 0;
                double totalHours = 0;
                int totalAbsent = 0;
                int totalViolations = 0;
                foreach (DataRow row in dt.Rows)
                {
                    totalDays += Convert.ToInt32(row["Дней отработано"]);
                    totalHours += Convert.ToDouble(row["Часов отработано"]);
                    totalAbsent += Convert.ToInt32(row["Больничных"])
                                 + Convert.ToInt32(row["Отсутствий"])
                                 + Convert.ToInt32(row["Отгулов"])
                                 + Convert.ToInt32(row["Учебных отпусков"])
                                 + Convert.ToInt32(row["Командировок"])
                                 + Convert.ToInt32(row["Адм. отпусков"]);
                    totalViolations += Convert.ToInt32(row["Нарушений"]);
                }
                lblTotalDays.Text = totalDays.ToString();
                lblTotalHours.Text = totalHours.ToString("F1");
                lblAbsent.Text = totalAbsent.ToString();
                lblViolations.Text = totalViolations.ToString();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }
    }
}