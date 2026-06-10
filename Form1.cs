using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Учёт времени сотрудников пожарной части";
            this.MinimumSize = new Size(700, 400);
            this.Size = new Size(900, 550);
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Resize += (s, e) => BuildUI();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Clear();

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            // Заголовок
            var title = new Label
            {
                Text = "Пожарная часть — учёт рабочего времени",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = false,
                Size = new Size(w - 40, 35),
                Location = new Point(20, 15),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(title);

            // Карточки — ширина адаптивная
            int cardW = (w - 60) / 3;
            AddStatCard("Сотрудников", GetCount("SELECT COUNT(*) FROM Employees WHERE IsActive=1"), 20, 60, "#C8B8A2", cardW);
            AddStatCard("На смене сегодня", GetCount("SELECT COUNT(*) FROM TimeRecords WHERE Date=CAST(GETDATE() AS DATE) AND Status='Отработано'"), 20 + cardW + 10, 60, "#B8C4B8", cardW);
            AddStatCard("Отсутствуют", GetCount("SELECT COUNT(*) FROM TimeRecords WHERE Date=CAST(GETDATE() AS DATE) AND Status<>'Отработано'"), 20 + (cardW + 10) * 2, 60, "#D4B8B0", cardW);

            // Кнопки — ширина адаптивная
            int btnY = 165;
            int btnW = (w - 60) / 4;
            AddNavButton("👤 Сотрудники", 20, btnY, btnW, btnEmployees_Click);
            AddNavButton("🕐 Учёт времени", 20 + (btnW + 10), btnY, btnW, btnTime_Click);
            AddNavButton("📅 График смен", 20 + (btnW + 10) * 2, btnY, btnW, btnShifts_Click);
            AddNavButton("📊 Отчёты", 20 + (btnW + 10) * 3, btnY, btnW, btnReports_Click);
        }

        private void AddStatCard(string label, string value, int x, int y, string color, int width)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 90),
                BackColor = ColorTranslator.FromHtml(color),
            };
            card.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = false,
                Size = new Size(width - 20, 20),
                Location = new Point(10, 8)
            });
            card.Controls.Add(new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = false,
                Size = new Size(width - 20, 40),
                Location = new Point(10, 30)
            });
            this.Controls.Add(card);
        }


        private void AddNavButton(string text, int x, int y, int width, EventHandler handler)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 55),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += handler;
            this.Controls.Add(btn);
        }

        private string GetCount(string query)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                return cmd.ExecuteScalar()?.ToString() ?? "0";
            }
            catch { return "—"; }
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            new EmployeesForm().ShowDialog();
        }
        private void btnTime_Click(object sender, EventArgs e)
        {
            new TimeTrackingForm().ShowDialog();
        }
        private void btnShifts_Click(object sender, EventArgs e)
        {
            new ShiftsForm().ShowDialog();
        }
        private void btnReports_Click(object sender, EventArgs e)
        {
            new ReportsForm().ShowDialog();
        }
    }
}