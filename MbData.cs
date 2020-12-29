using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DNS1_ARM_heater
{
     public class MbData : INotifyPropertyChanged
    {

        private ushort pgaz_kol { get; set; }//Давление газа в коллекторе
        private ushort tneft { get; set; }//Температура нефти на выходе
        private ushort tvod { get; set; }//Температура воды
        private ushort pneft_in { get; set; }//Давление нефти на входе
        private ushort p_gas_reg { get; set; }//Давление газа после регулятора
        private string stageWork { get; set; }//Этап работы
        private string[] error1 { get; set; } = new string[16];//Ошибки часть1
        private string[] error2 { get; set; } = new string[16];//ошибки часть2
        private ushort pgaz_kol2 { get; set; }//Давление газа в коллекторе
        private ushort tneft2 { get; set; }//Температура нефти на выходе
        private ushort tvod2 { get; set; }//Температура воды
        private ushort pneft_in2 { get; set; }//Давление нефти на входе
        private ushort p_gas_reg2 { get; set; }//Давление газа после регулятора
        private string stageWork2 { get; set; }//Этап работы
        private string[] error12 { get; set; } = new string[16];//Ошибки часть1
        private string[] error22 { get; set; } = new string[16];//ошибки часть2

        public ushort Pgaz_kol//Давление газа в коллекторе
        { get { return pgaz_kol; }
            set
            { pgaz_kol = value;
                OnPropertyChanged("Pgaz_kol");
            }
        }
        public ushort Tneft//Температура нефти на выходе
        {
            get { return tneft; }
            set
            {
                tneft = value;
                OnPropertyChanged("Tneft");
            }
        }
        public ushort Tvod//Температура воды
        {
            get { return tvod; }
            set
            {
                tvod = value;
                OnPropertyChanged("Tvod");
            }
        }
        public ushort Pneft_in//Давление нефти на входе
        {
            get { return pneft_in; }
            set
            {
                pneft_in = value;
                OnPropertyChanged("Pneft_in");
            }
        }
        public ushort P_gas_reg//Давление газа после регулятора
        {
            get { return p_gas_reg; }
            set
            {
                p_gas_reg = value;
                OnPropertyChanged("P_gas_reg");
            }
        }
        public string StageWork//Этап работы
        {
            get { return stageWork; }
            set
            {
                stageWork = value;
                OnPropertyChanged("StageWork");
            }
        }
        public string[] Error1
        {
            get { return error1; }
            set
            {
                error1 = value;
                OnPropertyChanged("Error1");
            }
        }
       //     = new string[16];//Ошибки часть1
        public string[] Error2
        {
            get { return error2; }
            set
            {
                error2 = value;
                OnPropertyChanged("Error2");
            }
        }

        public ushort Pgaz_kol2//Давление газа в коллекторе
        {
            get { return pgaz_kol2; }
            set
            {
                pgaz_kol2 = value;
                OnPropertyChanged("Pgaz_kol2");
            }
        }
        public ushort Tneft2//Температура нефти на выходе
        {
            get { return tneft2; }
            set
            {
                tneft2 = value;
                OnPropertyChanged("Tneft2");
            }
        }
        public ushort Tvod2//Температура воды
        {
            get { return tvod2; }
            set
            {
                tvod2 = value;
                OnPropertyChanged("Tvod2");
            }
        }
        public ushort Pneft_in2//Давление нефти на входе
        {
            get { return pneft_in2; }
            set
            {
                pneft_in2 = value;
                OnPropertyChanged("Pneft_in2");
            }
        }
        public ushort P_gas_reg2//Давление газа после регулятора
        {
            get { return p_gas_reg2; }
            set
            {
                p_gas_reg2 = value;
                OnPropertyChanged("P_gas_reg2");
            }
        }
        public string StageWork2//Этап работы
        {
            get { return stageWork2; }
            set
            {
                stageWork2 = value;
                OnPropertyChanged("StageWork2");
            }
        }
        public string[] Error12
        {
            get { return error12; }
            set
            {
                error12 = value;
                OnPropertyChanged("Error12");
            }
        }
        //     = new string[16];//Ошибки часть1
        public string[] Error22
        {
            get { return error22; }
            set
            {
                error22 = value;
                OnPropertyChanged("Error22");
            }
        }
        //= new string[16];//ошибки часть2

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
