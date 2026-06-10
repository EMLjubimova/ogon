using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class EmployeeEditForm : Form
    {
        private int employeeId;
        private TextBox txtLast, txtFirst, txtMiddle, txtNumber;
        private ComboBox cmbPosition;
        private DateTimePicker dtHire;

        public EmployeeEditForm(int id)
        {
            employeeId = id;
            this.Text = id == 0 ? "Добавить сотрудника" : "Редактировать сотрудника";
            this.Size = new Size(400, 500);
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterParent;
            BuildUI();
            if (id != 0) LoadEmployee();
        }

        private void BuildUI()
        {
            int y = 20;
            txtLast = AddField("Фамилия:", ref y);
            txtFirst = AddField("Имя:", ref y);
            txtMiddle = AddField("Отчество:", ref y);
            txtNumber = AddField("Табельный номер:", ref y);

            this.Controls.Add(new Label { Text = "Должность:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            y += 20;
            cmbPosition = new ComboBox { Location = new Point(20, y), Size = new Size(340, 25), Font = new Font("Segoe UI", 10) };
            LoadPositions();
            this.Controls.Add(cmbPosition);
            y += 40;

            this.Controls.Add(new Label { Text = "Дата приёма:", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            y += 20;
            dtHire = new DateTimePicker { Location = new Point(20, y), Size = new Size(340, 25), Font = new Font("Segoe UI", 10) };
            this.Controls.Add(dtHire);
            y += 50;

            var btnSave = new Button { Text = "Сохранить", Location = new Point(20, y), Size = new Size(150, 35), BackColor = ColorTranslator.FromHtml("#A89080"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private TextBox AddField(string label, ref int y)
        {
            this.Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
            y += 20;
            var tb = new TextBox { Location = new Point(20, y), Size = new Size(340, 25), Font = new Font("Segoe UI", 10) };
            this.Controls.Add(tb);
            y += 40;
            return tb;
        }

        private void LoadPositions()
        {
            using var conn = Database.GetConnection();
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, Name FROM Positions", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
                cmbPosition.Items.Add(new { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            cmbPosition.DisplayMember = "Name";
        }

        private void LoadEmployee()
        {
            using var conn = Database.GetConnection();
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Employees WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", employeeId);
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                txtLast.Text = r["LastName"].ToString();
                txtFirst.Text = r["FirstName"].ToString();
                txtMiddle.Text = r["MiddleName"].ToString();
                txtNumber.Text = r["PersonnelNumber"].ToString();
                dtHire.Value = (DateTime)r["HireDate"];
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLast.Text) || string.IsNullOrWhiteSpace(txtFirst.Text))
            { MessageBox.Show("Заполните фамилию и имя!"); return; }

            dynamic pos = cmbPosition.SelectedItem;
            int posId = pos?.Id ?? 1;

            using var conn = Database.GetConnection();
            conn.Open();
            string sql = employeeId == 0
                ? "INSERT INTO Employees (LastName,FirstName,MiddleName,PositionId,PersonnelNumber,HireDate,IsActive) VALUES (@l,@f,@m,@p,@n,@d,1)"
                : "UPDATE Employees SET LastName=@l,FirstName=@f,MiddleName=@m,PositionId=@p,PersonnelNumber=@n,HireDate=@d WHERE Id=@id";
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@l", txtLast.Text);
            cmd.Parameters.AddWithValue("@f", txtFirst.Text);
            cmd.Parameters.AddWithValue("@m", txtMiddle.Text);
            cmd.Parameters.AddWithValue("@p", posId);
            cmd.Parameters.AddWithValue("@n", txtNumber.Text);
            cmd.Parameters.AddWithValue("@d", dtHire.Value.Date);
            if (employeeId != 0) cmd.Parameters.AddWithValue("@id", employeeId);
            cmd.ExecuteNonQuery();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}