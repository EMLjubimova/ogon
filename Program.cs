using System;
using System.Windows.Forms;

namespace FireStationApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var login = new LoginForm();
            var result = login.ShowDialog();

            if (result != DialogResult.OK)
            {
                Application.Exit();
                return;
            }

            string role = login.UserRole;
            int empId = login.EmployeeId;
            string empName = login.UserName;
            login.Dispose();

            if (role == "admin")
            {
                Application.Run(new Form1());
            }
            else
            {
                Application.Run(new EmployeeRequestForm(empId, empName));
            }
        }
    }
}