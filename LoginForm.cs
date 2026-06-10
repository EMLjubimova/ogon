using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class LoginForm : Form
    {
        private TextBox txtLogin, txtPassword;
        private Button btnLogin;
        private Label lblError;

        public string UserRole { get; private set; }
        public int EmployeeId { get; private set; }
        public string UserName { get; private set; }

        public LoginForm()
        {
            this.Text = "Вход в систему";
            this.Size = new Size(380, 360);
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.FormClosing += (s, e) => { if (this.DialogResult != DialogResult.OK) Application.Exit(); }; // <- добавь эту строку
            BuildUI();
        }

        private void BuildUI()
        {
            var title = new Label
            {
                Text = "Пожарная часть",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#4A3F35"),
                AutoSize = true,
                Location = new Point(90, 25)
            };
            this.Controls.Add(title);

            var subtitle = new Label
            {
                Text = "Система учёта рабочего времени",
                Font = new Font("Segoe UI", 9),
                ForeColor = ColorTranslator.FromHtml("#A89080"),
                AutoSize = true,
                Location = new Point(70, 55)
            };
            this.Controls.Add(subtitle);

            this.Controls.Add(new Label { Text = "Логин:", Location = new Point(40, 100), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            txtLogin = new TextBox { Location = new Point(40, 122), Size = new Size(280, 28), Font = new Font("Segoe UI", 11) };
            this.Controls.Add(txtLogin);

            this.Controls.Add(new Label { Text = "Пароль:", Location = new Point(40, 160), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            txtPassword = new TextBox { Location = new Point(40, 182), Size = new Size(280, 28), Font = new Font("Segoe UI", 11), PasswordChar = '●' };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) TryLogin(); };
            this.Controls.Add(txtPassword);

            lblError = new Label
            {
                Text = "",
                Location = new Point(40, 218),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red
            };
            this.Controls.Add(lblError);

            btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(40, 240),
                Size = new Size(280, 38),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => TryLogin();
            this.Controls.Add(btnLogin);
        }

        private void TryLogin()
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Введите логин и пароль!";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Проверка...";

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT u.Role, u.EmployeeId, 
                   ISNULL(e.LastName + ' ' + e.FirstName, 'Администратор') AS Name
            FROM Users u
            LEFT JOIN Employees e ON u.EmployeeId = e.Id
            WHERE u.Login = @login AND u.Password = @password", conn);
                cmd.Parameters.AddWithValue("@login", txtLogin.Text.Trim());
                cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    UserRole = reader["Role"].ToString();
                    EmployeeId = reader["EmployeeId"] == DBNull.Value ? 0 : (int)reader["EmployeeId"];
                    UserName = reader["Name"].ToString();
                    reader.Close();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    reader.Close();
                    lblError.Text = "Неверный логин или пароль!";
                    txtPassword.Clear();
                    txtPassword.Focus();
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Войти";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Ошибка подключения к БД";
                btnLogin.Enabled = true;
                btnLogin.Text = "Войти";
            }
        }
    }
}