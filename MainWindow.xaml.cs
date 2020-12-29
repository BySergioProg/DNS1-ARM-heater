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
using NLog;

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
            serialPort.Parity = Properties.Settings.Default.Parity;
            serialPort.StopBits = Properties.Settings.Default.StopBits;
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
            {
                  throw;
            }
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
        private void SaveErrors(bool Err, string ErrText, int BoilerNo)
        {
            try
            {
                string Dir = Properties.Settings.Default.DBadress;
                string connectionString = $@"Data Source={Dir};Initial Catalog=DNS_HEAT;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    if (Err)
                    {

                        string sqlExpression = $"SELECT * FROM Errors WHERE Message='{ErrText}' and Activ=1 and BoilerNo={BoilerNo}";
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        SqlDataReader reader = command.ExecuteReader();
                        if (!reader.HasRows)
                        {
                            using (SqlConnection connection2 = new SqlConnection(connectionString))
                            {
                                connection2.Open();
                                DateTime date = DateTime.Now;
                                string date_str = date.ToString("yyyy-MM-ddTHH:mm:ss");
                                sqlExpression = $"INSERT INTO Errors (DateTime, Activ, Message, BoilerNo) VALUES ('{date_str}', 1, '{ErrText}', {BoilerNo} )";
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
            catch (Exception Ex)
            {

                Logger log = LogManager.GetCurrentClassLogger();
                log.Error($"Ошибка сохранения аварий в БД {Ex.Message}");
                MessageBox.Show(Ex.Message);
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
                    string sqlExpression = $"INSERT INTO History (DateTime, Pgaz_kol, Tneft, Tvod, Pneft_in, P_gas_reg, Pgaz_kol1, Tneft1, Tvod1, Pneft_in1, P_gas_reg1) VALUES ( '{date_str}', {mbData.Pgaz_kol}, {mbData.Tneft}, {mbData.Tvod}, {mbData.Pneft_in}, {mbData.P_gas_reg}, {mbData.Pgaz_kol2}, {mbData.Tneft2}, {mbData.Tvod2}, {mbData.Pneft_in2}, {mbData.P_gas_reg2})";
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    int number = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception Ex)
            {
         
                Logger log = LogManager.GetCurrentClassLogger();
                log.Error($"Ошибка сохранения данных в БД {Ex.Message}");
                MessageBox.Show(Ex.Message);
            }
        }
        private void ReadModbus(object sender, EventArgs e)
        {
            try
            {
                //if (this.WindowState == WindowState.Minimized)
                //{
                //    this.WindowState = WindowState.Normal;
                //}

                if (serialPort.IsOpen)
                {
                    //Данные печи ПП-0.63 
                    #region PP-063
                    byte slaveID = Properties.Settings.Default.slaveID;
                    ushort[] holding_register = ReadMB(slaveID, 264, 2);//265
                    mbData.P_gas_reg = holding_register[0];
                    mbData.Pgaz_kol = holding_register[1];
                    holding_register = ReadMB(slaveID, 337, 1);//338
                    mbData.Tneft = holding_register[0];
                    holding_register = ReadMB(slaveID, 341, 2);//342
                    mbData.Tvod = holding_register[0];
                    mbData.Pneft_in = holding_register[1];
                    holding_register = ReadMB(slaveID, 272, 1);//273
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
                            mbData.StageWork = "Не определено";
                            break;
                    }
                    holding_register = ReadMB(slaveID, 266, 2);//267
                    mbData.Error1 = SetErrors(holding_register[0]);
                    mbData.Error2 = SetErrors(holding_register[1]);
                    BitArray Error1 = new BitArray(new int[] { holding_register[0] });
                    BitArray Error2 = new BitArray(new int[] { holding_register[1] });
                    SaveErrors(Error1[0], "Наличие пламени до розжига",1 );
                    SaveErrors(Error1[2], "Низкий уровень воды", 1);
                    SaveErrors(Error1[5], "Высокое давление нефти на входе печи", 1);
                    SaveErrors(Error1[6], "Низкое давление нефти на входе печи", 1);
                    SaveErrors(Error1[7], "Не герметичен отсечной клапан на горелке", 1);
                    SaveErrors(Error1[9], "Утечка газа в коллекторе", 1);
                    SaveErrors(Error1[11], "Нет пламени на горелке", 1);
                    SaveErrors(Error1[13], "Высокое давление газа на горелке", 1);
                    SaveErrors(Error1[14], "Низкое давление газа на горелке", 1);
                    SaveErrors(Error2[1], "Высокая температура воды", 1);
                    SaveErrors(Error2[2], "Высокая температура нефти", 1);
                    #endregion
                    //Данные печи ПП-0.63А

                    #region PP-063A
                    slaveID = Properties.Settings.Default.slaveID2;
                    holding_register = ReadMB(slaveID, 264, 1);//265
                    mbData.P_gas_reg2 = holding_register[0];
                    holding_register = ReadMB(slaveID, 312, 6);//313
                    mbData.Tneft2 = holding_register[0];
                    mbData.Pgaz_kol2 = holding_register[2];
                    mbData.Tvod2 = holding_register[4];
                    mbData.Pneft_in2 = holding_register[5];
                    holding_register = ReadMB(slaveID, 272, 1);//273
                    switch (holding_register[0])
                    {
                        case 1:
                            mbData.StageWork2 = "Предпроверка";
                            break;
                        case 2:
                            mbData.StageWork2 = "Вентиляция топки";
                            break;
                        case 3:
                            mbData.StageWork2 = "Опрессовка отсечного клапана";
                            break;
                        case 4:
                            mbData.StageWork2 = "Заполнение газом коллектора";
                            break;
                        case 5:
                            mbData.StageWork2 = "Опрессовка коллектора";
                            break;
                        case 6:
                            mbData.StageWork2 = "Сброс газа";
                            break;
                        case 7:
                            mbData.StageWork2 = "Розжиг запальника";
                            break;
                        case 8:
                            mbData.StageWork2 = "Контроль пламени запальника";
                            break;
                        case 9:
                            mbData.StageWork2 = "Розжиг основной горелки";
                            break;
                        case 10:
                            mbData.StageWork2 = "Стабилизация пламени основной горелки";
                            break;
                        case 11:
                            mbData.StageWork2 = "Прогрев топки";
                            break;
                        case 12:
                            mbData.StageWork2 = "Выход на режим";
                            break;
                        case 13:
                            mbData.StageWork2 = "Активен ручной режим";
                            break;
                        case 14:
                            mbData.StageWork2 = "Предпроверка";
                            break;
                        default:
                            mbData.StageWork2 = "Не определено";
                            break;
                    }
                    holding_register = ReadMB(slaveID, 266, 2);//267
                    mbData.Error12 = SetErrors(holding_register[0]);
                    mbData.Error22 = SetErrors(holding_register[1]);
                    BitArray Error12 = new BitArray(new int[] { holding_register[0] });
                    BitArray Error22 = new BitArray(new int[] { holding_register[1] });
                    SaveErrors(Error12[0], "Наличие пламени до розжига", 2);
                    SaveErrors(Error12[2], "Низкий уровень воды", 2);
                    SaveErrors(Error12[5], "Высокое давление нефти на входе печи", 2);
                    SaveErrors(Error12[6], "Низкое давление нефти на входе печи", 2);
                    SaveErrors(Error12[7], "Не герметичен отсечной клапан на горелке", 2);
                    SaveErrors(Error12[9], "Утечка газа в коллекторе", 2);
                    SaveErrors(Error12[11], "Нет пламени на горелке", 2);
                    SaveErrors(Error12[13], "Высокое давление газа на горелке", 2);
                    SaveErrors(Error12[14], "Низкое давление газа на горелке", 2);
                    SaveErrors(Error22[1], "Высокая температура воды", 2);
                    SaveErrors(Error22[2], "Высокая температура нефти", 2);
                    #endregion
                }
                else
                { SerialOpen(); }

            }
            catch(Exception Ex)
            {
                Logger log = LogManager.GetCurrentClassLogger();
                log.Error($"Ошибка чтения данных с контроллера. {Ex.Message}");
                MessageBox.Show($"Ошибка чтения данных с контроллера. {Ex.Message}");
            }


        }

        private void AlarmHilstoryFormOpen(object sender, RoutedEventArgs e)
        {
            ErrorHistory errorHistory = new ErrorHistory();
            errorHistory.Owner = this;
            errorHistory.Show();
        }

        private void ParamFormOpen(object sender, RoutedEventArgs e)
        {
            Journal journal = new Journal();
            journal.Owner = this;
            journal.Show();
        }
    }
}
