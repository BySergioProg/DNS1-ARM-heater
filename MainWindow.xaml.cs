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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Modbus.Device;
using System.IO.Ports;
using System.Windows.Threading;
using System.Collections;
using System.Data.SqlClient;

namespace DNS1_ARM_heater
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer ordersWorker = new DispatcherTimer();
        private DispatcherTimer ordersWorker2 = new DispatcherTimer();
        private SerialPort serialPort = new SerialPort(); //Create a new SerialPort object.
        public MbData mbData = new MbData();
        public MainWindow()
        {
            InitializeComponent();
            SerialOpen();
            ordersWorker.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ordersWorker.Tick += ReadModbus;
            ordersWorker.Start();
            ordersWorker2.Interval = new TimeSpan(0, 0, 0, 60, 0);
            ordersWorker2.Tick += SaveData;
            ordersWorker2.Start();
            this.DataContext = mbData;
        }
        private void SerialOpen()//Данные COM порта, подключение к порту
        {
            serialPort.PortName = Properties.Settings.Default.PortName;
            serialPort.BaudRate = Properties.Settings.Default.BaudRate;
            serialPort.DataBits = Properties.Settings.Default.DataBits;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.Open();
        }
        private ushort[] ReadMB(byte slaveID, ushort startAddress, ushort numOfPoints)
        {
            try
            {
                ModbusSerialMaster master = ModbusSerialMaster.CreateRtu(serialPort);
                master.Transport.ReadTimeout = 300; //milliseconds
                ushort[] holding_register = master.ReadHoldingRegisters(slaveID, startAddress,
                  numOfPoints);
                return holding_register;
            }
            catch
            { throw; }
        }
        private string[] SetErrors(ushort ErrData)
        {
            string[] Visible = new string[16];
            BitArray Command = new BitArray(new int[] { ErrData });
            for (int i=0; i<16; i++)
            {
                if (Command[i])
                { Visible[i] = "Visible"; }
                else
                { Visible[i] = "Collapsed"; }
            }
            return Visible;

        }
        private void SaveErrors(bool Err, string ErrText)
        {
            string Dir= Properties.Settings.Default.DBadress;
            string connectionString = $@"Data Source={Dir};Initial Catalog=DNS_HEAT;Integrated Security=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (Err)
                {

                    string sqlExpression = $"SELECT * FROM Errors WHERE Message='{ErrText}' and Activ=1";
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                    using (SqlConnection connection2 = new SqlConnection(connectionString))
                        {
                            connection2.Open();
                            DateTime date = DateTime.Now;
                            string date_str = date.ToString("yyyy-MM-ddTHH:mm:ss");
                            sqlExpression = $"INSERT INTO Errors (DateTime, Activ, Message) VALUES ('{date_str}', 1, '{ErrText}' )";
                            SqlCommand command2 = new SqlCommand(sqlExpression, connection2);
                            int number = command2.ExecuteNonQuery();
                            connection2.Close();
                        }
                    }
                }
                else
                {
                    using (SqlConnection connection2 = new SqlConnection(connectionString))
                    {
                        connection2.Open();
                        DateTime date = DateTime.Now;
                        string date_str = date.ToString("yyyy-MM-ddTHH:mm:ss");
                        string sqlExpression = $"UPDATE Errors SET Activ=0 WHERE Activ=1 AND  Message='{ErrText}'";
                        SqlCommand command2 = new SqlCommand(sqlExpression, connection2);
                        int number = command2.ExecuteNonQuery();
                        connection2.Close();
                    }
                }
                    connection.Close();
            }
        }
        private void SaveData(object sender, EventArgs e)//Сохранение данных в БД
        {
            try
            {
                string Dir = Properties.Settings.Default.DBadress;
                string connectionString = $@"Data Source={Dir};Initial Catalog=DNS_HEAT;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DateTime date = DateTime.Now;
                    string date_str = date.ToString("yyyy-MM-ddTHH:mm:ss");
                    string sqlExpression = $"INSERT INTO History (DateTime, Pgaz_kol, Tneft, Tvod, Pneft_in, P_gas_reg) VALUES ( '{date_str}', {mbData.Pgaz_kol}, {mbData.Tneft}, {mbData.Tvod}, {mbData.Pneft_in}, {mbData.P_gas_reg})";
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    int number = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        private void ReadModbus(object sender, EventArgs e)
        {
            try
            {

                if (serialPort.IsOpen)
                {
                    byte slaveID = Properties.Settings.Default.slaveID;
                    ushort[] holding_register = ReadMB(slaveID, 265, 1);
                    mbData.Pgaz_kol = holding_register[0];
                    holding_register = ReadMB(slaveID, 313, 6);
                    mbData.Tneft = holding_register[0];
                    mbData.P_gas_reg = holding_register[2];
                    mbData.Tvod = holding_register[4];
                    mbData.Pneft_in = holding_register[5];
                    holding_register = ReadMB(slaveID, 273, 1);
                    switch (holding_register[0])
                    {
                       case 1:
                            mbData.StageWork = "Предпроверка";
                         break;
                        case 2:
                            mbData.StageWork = "Вентиляция топки";
                            break;
                        case 3:
                            mbData.StageWork = "Опрессовка отсечного клапана";
                            break;
                        case 4:
                            mbData.StageWork = "Заполнение газом коллектора";
                            break;
                        case 5:
                            mbData.StageWork = "Опрессовка коллектора";
                            break;
                        case 6:
                            mbData.StageWork = "Сброс газа";
                            break;
                        case 7:
                            mbData.StageWork = "Розжиг запальника";
                            break;
                        case 8:
                            mbData.StageWork = "Контроль пламени запальника";
                            break;
                        case 9:
                            mbData.StageWork = "Розжиг основной горелки";
                            break;
                        case 10:
                            mbData.StageWork = "Стабилизация пламени основной горелки";
                            break;
                        case 11:
                            mbData.StageWork = "Прогрев топки";
                            break;
                        case 12:
                            mbData.StageWork = "Выход на режим";
                            break;
                        case 13:
                            mbData.StageWork = "Активен ручной режим";
                            break;
                        case 14:
                            mbData.StageWork = "Предпроверка";
                            break;
                        default:
                            mbData.StageWork = "Не опоределено";
                            break;
                    }
                    holding_register = ReadMB(slaveID, 267, 2);
                    mbData.Error1 = SetErrors(holding_register[0]);
                    mbData.Error2 = SetErrors(holding_register[1]);
                    BitArray Error1 = new BitArray(new int[] { holding_register[0] });
                    BitArray Error2 = new BitArray(new int[] { holding_register[1] });
                    SaveErrors(Error1[0], "Наличие пламени до розжига");
                    SaveErrors(Error1[2], "Низкий уровень воды");
                    SaveErrors(Error1[5], "Высокое давление нефти на входе печи");
                    SaveErrors(Error1[6], "Низкое давление нефти на входе печи");
                    SaveErrors(Error1[7], "Не герметичен отсечной клапан на горелке");
                    SaveErrors(Error1[9], "Утечка газа в коллекторе");
                    SaveErrors(Error1[11], "Нет пламени на горелке");
                    SaveErrors(Error1[13], "Высокое давление газа на горелке");
                    SaveErrors(Error1[14], "Низкое давление газа на горелке");
                    SaveErrors(Error2[1], "Высокая температура воды");
                    SaveErrors(Error2[2], "Высокая температура нефти");
                }
                else
                { SerialOpen(); }

            }
            catch(Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }


        }

        private void AlarmHilstoryFormOpen(object sender, RoutedEventArgs e)
        {
            ErrorHistory errorHistory = new ErrorHistory();
            errorHistory.Owner = this;
            errorHistory.Show();
        }
    }
}
