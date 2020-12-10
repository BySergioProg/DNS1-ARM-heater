using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace DNS1_ARM_heater
{
    /// <summary>
    /// Логика взаимодействия для Journal.xaml
    /// </summary>
    public partial class Journal : Window
    {
        private class Data
        {
            public string DateTime { get; set; } 
            public int Pgaz_kol { get; set; }//Давление газа в коллекторе
            public int Tneft { get; set; }//Температура нефти на выходе
            public int Tvod { get; set; }//Температура воды
            public int Pneft_in { get; set; }//Давление нефти на входе
            public int P_gas_reg { get; set; }//Давление газа после регулятора

        }
        public Journal()
        {
            InitializeComponent();
            Date_start.SelectedDate = DateTime.Now.Date;
                                DateTime Date1 = new DateTime();
                    Date1 = Convert.ToDateTime(Date_start.SelectedDate).AddHours(24);
        }
        private void Show_Data()
        {
            string Dir = Properties.Settings.Default.DBadress;
            string connectionString = $@"Data Source={Dir};Initial Catalog=DNS_HEAT;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string date_start = Convert.ToDateTime(Date_start.SelectedDate).ToString("yyyy-MM-ddTHH:mm:ss");
                DateTime Date1 = new DateTime();
                Date1 = Convert.ToDateTime(Date_start.SelectedDate).AddHours(24);
                string date_end = Convert.ToDateTime(Date1).ToString("yyyy-MM-ddTHH:mm:ss");
                List<Data> data = new List<Data>();
                connection.Open();
                string sqlExpression = $"SELECT * FROM History WHERE DateTime>='{date_start}' AND DateTime<='{date_end}' ORDER BY DateTime DESC";
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        data.Add(new Data()
                        {
                            DateTime=Convert.ToString(reader.GetValue(1)),
                            Pgaz_kol=(int)reader.GetValue(2),
                            Tneft=(int)reader.GetValue(3),
                            Tvod=(int)reader.GetValue(4),
                            Pneft_in=(int)reader.GetValue(5),
                            P_gas_reg=(int)reader.GetValue(6)

                        });

                    }
                }

                DataExit.ItemsSource = data;

                connection.Close();
            }

        }
        private void SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Show_Data();
        }
    }
}
