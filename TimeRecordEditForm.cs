using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace FireStationApp
{
    public class TimeRecordEditForm : Form
    {
        private ComboBox cmbEmployee, cmbShift, cmbStatus;
        private DateTimePicker dtCheckIn, dtCheckOut;
        private TextBox txtNotes;
        private DateTime recordDate;

        public TimeRecordEditForm(DateTime date)
        {
            recordDate = date;
            this.Text = "Добавить запись — " + date.ToString("dd.MM.yyyy");
            this.Size = new Size(420, 540);
            this.BackColor = ColorTranslator.FromHtml("#E8DDD0");
            this.StartPosition = FormStartPosition.CenterParent;
            BuildUI();
        }

        private void BuildUI()
        {
            int y = 20;

            AddLabel("Сотрудник:", y); y += 22;
            cmbEmployee = new ComboBox { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            LoadEmployees();
            this.Controls.Add(cmbEmployee); y += 45;

            AddLabel("Смена:", y); y += 22;
            cmbShift = new ComboBox { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            LoadShifts();
            this.Controls.Add(cmbShift); y += 45;

            AddLabel("Статус:", y); y += 22;
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
            this.Controls.Add(cmbStatus); y += 45;

            AddLabel("Время прихода:", y); y += 22;
            dtCheckIn = new DateTimePicker { Location = new Point(20, y), Size = new Size(360, 25), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font("Segoe UI", 10) };
            dtCheckIn.Value = DateTime.Today.AddHours(8);
            this.Controls.Add(dtCheckIn); y += 45;

            AddLabel("Время ухода:", y); y += 22;
            dtCheckOut = new DateTimePicker { Location = new Point(20, y), Size = new Size(360, 25), Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font("Segoe UI", 10) };
            dtCheckOut.Value = DateTime.Today.AddHours(20);
            this.Controls.Add(dtCheckOut); y += 45;

            AddLabel("Примечание:", y); y += 22;
            txtNotes = new TextBox { Location = new Point(20, y), Size = new Size(360, 25), Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtNotes); y += 50;

            var btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(20, y),
                Size = new Size(150, 35),
                BackColor = ColorTranslator.FromHtml("#A89080"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);
        }

        private void AddLabel(string text, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = ColorTranslator.FromHtml("#4A3F35") });
        }

        private void LoadEmployees()
        {
            using var conn = Database.GetConnection();
            conn.Open();
            var cmd = new SqlCommand("SELECT Id, LastName + ' ' + FirstName AS Name FROM Employees WHERE IsActive=1 ORDER BY LastName", conn);
            var r = cmd.ExecuteReader();
            while (r.Read())
                cmbEmployee.Items.Add(new { Id = r.GetInt32(0), Name = r.GetString(1) });
            cmbEmployee.DisplayMember = "Name";
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbEmployee.SelectedItem == null) { MessageBox.Show("Выберите сотрудника!"); return; }
            dynamic emp = cmbEmployee.SelectedItem;
            dynamic shift = cmbShift.SelectedItem;

            var checkIn = recordDate.Date + dtCheckIn.Value.TimeOfDay;
            var checkOut = recordDate.Date + dtCheckOut.Value.TimeOfDay;
            double hours = (checkOut - checkIn).TotalHours;
            if (hours < 0) hours = 0;

            using var conn = Database.GetConnection();
            conn.Open();
            var cmd = new SqlCommand(@"INSERT INTO TimeRecords 
                (EmployeeId, ShiftId, Date, CheckIn, CheckOut, ActualHours, Status, Notes)
                VALUES (@emp, @shift, @date, @in, @out, @hours, @status, @notes)", conn);
            cmd.Parameters.AddWithValue("@emp", emp.Id);
            cmd.Parameters.AddWithValue("@shift", shift?.Id ?? 1);
            cmd.Parameters.AddWithValue("@date", recordDate.Date);
            cmd.Parameters.AddWithValue("@in", checkIn);
            cmd.Parameters.AddWithValue("@out", checkOut);
            cmd.Parameters.AddWithValue("@hours", hours);
            cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
            cmd.Parameters.AddWithValue("@notes", txtNotes.Text);
            cmd.ExecuteNonQuery();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}