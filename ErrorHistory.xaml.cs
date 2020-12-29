using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using NLog;

namespace DNS1_ARM_heater
{
    /// <summary>
    /// Логика взаимодействия для ErrorHistory.xaml
    /// </summary>
    public partial class ErrorHistory : Window
    {
        private class Errors
        {
            public string Time { get; set; }
            public string BoilerNo { get; set; }
            public string Alarm { get; set; }
        }
        public ErrorHistory()
        {
            InitializeComponent();
            Date_start.SelectedDate = DateTime.Now.Date;
            Date_end.SelectedDate = DateTime.Now.Date.AddHours(24);
            Show_Data();
        }
        private void Show_Data()
        {
            try
            {
                string boilerNo;
                string Dir = Properties.Settings.Default.DBadress;
                string connectionString = $@"Data Source={Dir};Initial Catalog=DNS_HEAT;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string date_start = Convert.ToDateTime(Date_start.SelectedDate).ToString("yyyy-MM-ddTHH:mm:ss");
                    string date_end = Convert.ToDateTime(Date_end.SelectedDate).ToString("yyyy-MM-ddTHH:mm:ss");
                    List<Errors> errors = new List<Errors>();
                    connection.Open();
                    string sqlExpression = $"SELECT * FROM Errors WHERE DateTime>='{date_start}' AND DateTime<='{date_end}' ORDER BY DateTime DESC";
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if ((int)reader.GetValue(4) == 1) {  boilerNo = "ПП-0.63"; }
                            else
                            { boilerNo = "ПП-0.63А"; }
                            errors.Add(new Errors()
                            {
                                Time = Convert.ToString(reader.GetValue(1)),
                                Alarm = (string)(reader.GetValue(3)),
                                BoilerNo= boilerNo

                            });

                        }
                    }

                    History.ItemsSource = errors;

                    connection.Close();
                }
            }
            catch (Exception Ex)
            {
                Logger log = LogManager.GetCurrentClassLogger();
                log.Error($"Ошибка доступа к БД {Ex.Message}");
                MessageBox.Show(Ex.Message);
            }

        }
        private void Date_start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Show_Data();
        }

        private void Date_end_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Show_Data();
        }
    }
}
