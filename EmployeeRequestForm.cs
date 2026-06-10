using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class EmployeeRequestForm : Form
    {
        private int employeeId;
        private string employeeName;
        private ComboBox cmbShift, cmbStatus;
        private DateTimePicker dtDate, dtCheckIn, dtCheckOut;
        private TextBox txtNotes;
        private DataGridView grid;
        private Label lblTotalHours, lblTotalDays;
        private TabControl tabs;

        public EmployeeRequestForm(int empId, string empName)
        {
            employeeId = empId;
            employeeName = empName;
            this.Text = "Личный кабинет — " + empName;
            this.Size = new Size(870, 680);
            this.MinimumSize = new Size(870, 680);
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.None;
            BuildUI();
            LoadMyShifts();
        }

        private void BuildUI()
        {
            // Сначала создаём вкладки
            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            var tabShifts = new TabPage("📅 Мои смены");
            tabShifts.BackColor = ColorTranslator.FromHtml("#F5EFE8");
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
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
            tabShifts.Controls.Add(grid);

            var tabRequest = new TabPage("✚ Создать заявку");
            tabRequest.BackColor = ColorTranslator.FromHtml("#F5EFE8");
            BuildRequestTab(tabRequest);

            var tabFuture = new TabPage("🗓 Расписание");
            tabFuture.BackColor = ColorTranslator.FromHtml("#F5EFE8");
            BuildFutureTab(tabFuture);

            tabs.TabPages.Add(tabShifts);
            tabs.TabPages.Add(tabRequest);
            tabs.TabPages.Add(tabFuture);

            // Верхняя панель
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = ColorTranslator.FromHtml("#E8DDD0")
            };

            var title = new Label
            {
                Text = "Добро пожаловать, " + employeeName + "!",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            topPanel.Controls.Add(title);

            var cardDays = new Panel { Location = new Point(20, 55), Size = new Size(180, 75), BackColor = ColorTranslator.FromHtml("#C8B8A2") };
            cardDays.Controls.Add(new Label { Text = "Дней отработано", Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35"), AutoSize = true, Location = new Point(10, 8) });
            lblTotalDays = new Label { Text = "—", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#4A3F35"), AutoSize = true, Location = new Point(10, 28) };
            cardDays.Controls.Add(lblTotalDays);
            topPanel.Controls.Add(cardDays);

            var cardHours = new Panel { Location = new Point(215, 55), Size = new Size(180, 75), BackColor = ColorTranslator.FromHtml("#B8C4B8") };
            cardHours.Controls.Add(new Label { Text = "Часов всего", Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35"), AutoSize = true, Location = new Point(10, 8) });
            lblTotalHours = new Label { Text = "—", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#4A3F35"), AutoSize = true, Location = new Point(10, 28) };
            cardHours.Controls.Add(lblTotalHours);
            topPanel.Controls.Add(cardHours);

            // Важно: сначала Fill, потом Top
            this.Controls.Add(tabs);
            this.Controls.Add(topPanel);
        }

        private void BuildRequestTab(TabPage tab)
        {
            int y = 15;
            tab.AutoScroll = true;

            // Сотрудник (только отображение)
            AddTabLabel(tab, "Сотрудник:", y); y += 22;
            var lblEmp = new Label
            {
                Text = employeeName,
                Location = new Point(20, y),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35")
            };
            tab.Controls.Add(lblEmp); y += 40;

            // Дата
            AddTabLabel(tab, "Дата смены:", y); y += 22;
            dtDate = new DateTimePicker { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10), Value = DateTime.Today };
            tab.Controls.Add(dtDate); y += 40;

            // Смена
            AddTabLabel(tab, "Смена:", y); y += 22;
            cmbShift = new ComboBox { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            LoadShifts();
            tab.Controls.Add(cmbShift); y += 40;

            // Статус
            AddTabLabel(tab, "Тип заявки:", y); y += 22;
            cmbStatus = new ComboBox { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new[] {
    "Отработано",
    "Отпуск",
    "Больничный",
    "Отгул",
    "Учебный отпуск",
    "Командировка",
    "Административный отпуск"
});
            cmbStatus.SelectedIndex = 0;
            tab.Controls.Add(cmbStatus); y += 40;

            // Время прихода
            AddTabLabel(tab, "Время прихода:", y); y += 22;
            dtCheckIn = new DateTimePicker { Location = new Point(20, y), Size = new Size(360, 25), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font("Segoe UI", 10) };
            dtCheckIn.Value = DateTime.Today.AddHours(8);
            tab.Controls.Add(dtCheckIn); y += 40;

            // Время ухода
            AddTabLabel(tab, "Время ухода:", y); y += 22;
            dtCheckOut = new DateTimePicker { Location = new Point(20, y), Size = new Size(360, 25), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font("Segoe UI", 10) };
            dtCheckOut.Value = DateTime.Today.AddHours(20);
            tab.Controls.Add(dtCheckOut); y += 40;

            // Примечание
            AddTabLabel(tab, "Примечание:", y); y += 22;
            txtNotes = new TextBox { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10) };
            tab.Controls.Add(txtNotes); y += 45;

            var btnSave = new Button
            {
                Text = "Отправить заявку",
                Location = new Point(20, y),
                Size = new Size(360, 38),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            tab.Controls.Add(btnSave);
        }

        private Label AddStatCard(string title, string value, int x, int y, string color)
        {
            var card = new Panel { Location = new Point(x, y), Size = new Size(180, 75), BackColor = ColorTranslator.FromHtml(color) };
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35"), AutoSize = true, Location = new Point(10, 8) });
            var val = new Label { Text = value, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#4A3F35"), AutoSize = true, Location = new Point(10, 28) };
            card.Controls.Add(val);
            this.Controls.Add(card);
            return val;
        }

        private void AddTabLabel(TabPage tab, string text, int y)
        {
            tab.Controls.Add(new Label { Text = text, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#A89080") });
        }

        private void LoadShifts()
        {
            using var conn = Database.GetConnection();
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Name FROM Shifts", conn);
            var r = cmd.ExecuteReader();
            while (r.Read())
                cmbShift.Items.Add(new { Id = r.GetInt32(0), Name = r.GetString(1) });
            cmbShift.DisplayMember = "Name";
            if (cmbShift.Items.Count > 0) cmbShift.SelectedIndex = 0;
        }

        private void LoadMyShifts()
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();

                var query = @"SELECT 
                    tr.Date AS Дата,
                    s.Name AS Смена,
                    CONVERT(varchar, tr.CheckIn, 108) AS Приход,
                    CONVERT(varchar, tr.CheckOut, 108) AS Уход,
                    tr.ActualHours AS [Часов],
                    tr.Status AS Статус,
                    tr.Notes AS Примечание
                    FROM TimeRecords tr
                    LEFT JOIN Shifts s ON tr.ShiftId = s.Id
                    WHERE tr.EmployeeId = @empId
                    ORDER BY tr.Date DESC";

                var da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@empId", employeeId);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;

                // Считаем итоги
                var statsCmd = new SqlCommand(@"
                    SELECT 
                        COUNT(CASE WHEN Status='Отработано' THEN 1 END) AS Days,
                        ISNULL(SUM(CASE WHEN Status='Отработано' THEN ActualHours ELSE 0 END), 0) AS Hours
                    FROM TimeRecords WHERE EmployeeId = @empId", conn);
                statsCmd.Parameters.AddWithValue("@empId", employeeId);
                var r = statsCmd.ExecuteReader();
                if (r.Read())
                {
                    lblTotalDays.Text = r["Days"].ToString();
                    lblTotalHours.Text = Convert.ToDouble(r["Hours"]).ToString("F1");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            dynamic shift = cmbShift.SelectedItem;
            var checkIn = dtDate.Value.Date + dtCheckIn.Value.TimeOfDay;
            var checkOut = dtDate.Value.Date + dtCheckOut.Value.TimeOfDay;
            double hours = Math.Max((checkOut - checkIn).TotalHours, 0);

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();

                var check = new SqlCommand("SELECT COUNT(*) FROM TimeRecords WHERE EmployeeId=@emp AND Date=@date", conn);
                check.Parameters.AddWithValue("@emp", employeeId);
                check.Parameters.AddWithValue("@date", dtDate.Value.Date);
                if ((int)check.ExecuteScalar() > 0)
                {
                    MessageBox.Show("На эту дату уже есть заявка!", "Внимание");
                    return;
                }

                var cmd = new SqlCommand(@"INSERT INTO TimeRecords 
                    (EmployeeId, ShiftId, Date, CheckIn, CheckOut, ActualHours, Status, Notes)
                    VALUES (@emp, @shift, @date, @in, @out, @hours, @status, @notes)", conn);
                cmd.Parameters.AddWithValue("@emp", employeeId);
                cmd.Parameters.AddWithValue("@shift", shift?.Id ?? 1);
                cmd.Parameters.AddWithValue("@date", dtDate.Value.Date);
                cmd.Parameters.AddWithValue("@in", checkIn);
                cmd.Parameters.AddWithValue("@out", checkOut);
                cmd.Parameters.AddWithValue("@hours", hours);
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@notes", txtNotes.Text);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Заявка успешно отправлена!", "Готово");
                LoadMyShifts();
                tabs.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        private void BuildFutureTab(TabPage tab)
        {
            var lblInfo = new Label
            {
                Text = "Ваши предстоящие смены на ближайшие 30 дней:",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            tab.Controls.Add(lblInfo);

            var gridFuture = new DataGridView
            {
                Location = new Point(10, 40),
                Size = new Size(755, 350),
                BackgroundColor = ColorTranslator.FromHtml("#F5EFE8"),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 10),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Name = "gridFuture"
            };
            gridFuture.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#C8B8A2");
            gridFuture.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            tab.Controls.Add(gridFuture);

            // Итоговая плашка
            var panel = new Panel
            {
                Location = new Point(10, 400),
                Size = new Size(755, 50),
                BackColor = ColorTranslator.FromHtml("#C8B8A2")
            };
            var lblSummary = new Label
            {
                Name = "lblFutureSummary",
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            panel.Controls.Add(lblSummary);
            tab.Controls.Add(panel);

            LoadFutureShifts(gridFuture, lblSummary);
        }

        private void LoadFutureShifts(DataGridView gridFuture, Label lblSummary)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();

                var query = @"SELECT 
            tr.Date AS Дата,
            DATENAME(WEEKDAY, tr.Date) AS [День недели],
            s.Name AS Смена,
            CONVERT(varchar, s.StartTime, 108) AS [Начало],
            CONVERT(varchar, s.EndTime, 108) AS [Конец],
            tr.ActualHours AS [Часов],
            tr.Status AS Статус
            FROM TimeRecords tr
            LEFT JOIN Shifts s ON tr.ShiftId = s.Id
            WHERE tr.EmployeeId = @empId
            AND tr.Date >= CAST(GETDATE() AS DATE)
            AND tr.Date <= DATEADD(DAY, 30, GETDATE())
            ORDER BY tr.Date";

                var da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@empId", employeeId);
                var dt = new DataTable();
                da.Fill(dt);
                gridFuture.DataSource = dt;

                // Раскраска строк по статусу
                gridFuture.RowPrePaint -= GridFuture_RowPrePaint;
                gridFuture.RowPrePaint += GridFuture_RowPrePaint;

                // Итог
                var countCmd = new SqlCommand(@"
            SELECT COUNT(*) AS Смен,
            ISNULL(SUM(tr.ActualHours), 0) AS Часов
            FROM TimeRecords tr
            LEFT JOIN Shifts s ON tr.ShiftId = s.Id
            WHERE tr.EmployeeId = @empId
            AND tr.Date >= CAST(GETDATE() AS DATE)
            AND tr.Date <= DATEADD(DAY, 30, GETDATE())", conn);
                countCmd.Parameters.AddWithValue("@empId", employeeId);
                var r = countCmd.ExecuteReader();
                if (r.Read())
                {
                    lblSummary.Text = $"Предстоящих смен: {r["Смен"]}    Часов запланировано: {r["Часов"]}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void GridFuture_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null || e.RowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[e.RowIndex];
            var status = row.Cells["Статус"]?.Value?.ToString();
            row.DefaultCellStyle.BackColor = status switch
            {
                "Отработано" => ColorTranslator.FromHtml("#D4E8D4"),
                "Больничный" => ColorTranslator.FromHtml("#F0D4D4"),
                "Отпуск" => ColorTranslator.FromHtml("#D4D4F0"),
                _ => ColorTranslator.FromHtml("#F5EFE8")
            };
        }
    }
}