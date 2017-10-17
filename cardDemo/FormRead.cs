using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Costs;

namespace cardDemo
{
    public partial class FormRead : Form
    {
        public FormRead()
        {
            InitializeComponent();
        }
        public int icdev;
        int st;
        byte[] snr = new byte[5];
        //public static byte[] globalUseKey = { 0xee, 0xee, 0xee, 0xee, 0xee, 0xee };
        public static byte[] globalUseKey = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        public const int globalUseSector = 10;
        public static byte[] globalOldFullKey = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x07, 0x80, 0x69, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        public static byte[] globalUseFullKey = { 0xee, 0xee, 0xee, 0xee, 0xee, 0xee, 0xff, 0x07, 0x80, 0x69, 0xee, 0xee, 0xee, 0xee, 0xee, 0xee };
        private bool isRunning;
        private string oldCardPhyId;
        private int CostTime;
        private void btnInit_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            reloadKeys(true);
            st = Program.rf_card(icdev, 1, snr);
            if (st != 0)
            {
                addLog("rf_card error");
                return;
            }
            else
            {
                byte[] snr1 = new byte[8];
                addLog("rf_card right!");
                Program.hex_a(snr, snr1, 4);
                addLog(System.Text.Encoding.Default.GetString(snr1));
            }
            //初始化扇区
            if (false == InitSector(globalUseSector))
            {
                addLog("初始化卡号信息失败");
                return;
            }
            st = Program.rf_halt(icdev);
            if (st != 0)
            {
                addLog("rf_halt error!" + st.ToString());
                return;
            }
            else
            {
                addLog("rf_halt right!");
            }
        }

        private Boolean InitSector(int passSector)
        {
            int sector = passSector;
            st = Program.rf_authentication(icdev, 0, sector);
            if (st != 0)
            {
                addLog("rf_authentication error!");
                return false;
            }
            else
            {
                addLog("rf_authentication right!");
            }
            initDataBySector(passSector);

            //keys
            st = Program.rf_write(icdev, sector * 4 + 3, globalUseFullKey);
            if (st != 0)
            {
                addLog("rf_write error!" + st.ToString());
                return false;
            }
            else
            {
                addLog("rf_write right!");
            }
            return true;
        }
        public static string completeZero16(string value)
        {
            string sValue = value;
            for (int i = 0; i < 16; i++)
            {
                if (sValue.Length < 16)
                {
                    sValue = "0" + sValue;
                }
            }

            return sValue;
        }
        private void initDataBySector(int passSector)
        {
            byte[] data = new byte[16];
            string databuff = "";
            databuff = "" + 0;
            data = Encoding.Default.GetBytes(completeZero16(databuff));
            st = Program.rf_write(icdev, passSector * 4 + 1, data);
            if (st != 0)
            {
                addLog("rf_write error!" + st.ToString());
                return;
            }
            else
            {
                addLog("rf_write right!");
            }
        }

        private bool resetSector(int tSector)
        {
            st = Program.rf_authentication(icdev, 0, tSector);
            if (st != 0)
            {
                addLog("rf_authentication error!");
                return false;
            }
            else
            {
                addLog("rf_authentication right!");
            }
            st = Program.rf_write(icdev, tSector * 4 + 3, globalOldFullKey);
            if (st != 0)
            {
                addLog("rf_write error!" + st.ToString());
                return false;
            }
            else
            {
                addLog("rf_write right!");
            }
            return true;
        }


        private void reloadKeys(bool isOld)
        {
            int mode = 0;
            int sector = 0;
            for (int i = 0; i < 16; i++)
            {
                if (isOld)
                {
                    //byte[] key = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
                    //向读写器装载指定扇区的新密码mode：密码类型，sector：装载密码的扇区号（0-15），key:新密码
                    st = Program.rf_load_key(icdev, mode, sector, globalUseKey);
                }
                else
                {
                    st = Program.rf_load_key(icdev, mode, sector, globalUseKey);
                }

                if (st != 0)
                {
                    string s1 = Convert.ToString(sector);
                    addLog(s1 + " sector rf_load_key error!");
                }
                sector++;
            }
        
        }

        private void FormRead_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (icdev > 0)
            {
                st = Program.rf_exit(icdev);
                //st = Program.rf_usbclose(icdev);
                if (st != 0)
                {
                    addLog("rf_usbclose error");
                }
                else
                {
                    addLog("rf_usbclose success");
                }
            }
        }

        private void addLog(string p)
        {
            listBox1.Items.Add(p);
        }

        private void FormRead_Shown(object sender, EventArgs e)
        {
            icdev = Program.rf_init(0, 9600);
            if (icdev > 0)
            {
                addLog("Com Connect success!");
                byte[] status = new byte[30];
                st = Program.rf_get_status(icdev, status);
                addLog(System.Text.Encoding.Default.GetString(status));
                Program.rf_beep(icdev, 25);
            }
            else
                addLog("Com Connect failed!");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            reloadKeys(false);
            st = Program.rf_card(icdev, 1, snr);
            if (st != 0)
            {
                addLog("rf_card error");
                return;
            }
            else
            {
                byte[] snr1 = new byte[8];
                addLog("rf_card right!");
                Program.hex_a(snr, snr1, 4);
                addLog(System.Text.Encoding.Default.GetString(snr1));
            }
            //卡片卡号信息
            if (false == resetSector(globalUseSector))
            {
                addLog("重置卡号信息扇区失败");
                return;
            }
            st = Program.rf_halt(icdev);
            if (st != 0)
            {
                addLog("rf_halt error!" + st.ToString());
                return;
            }
            else
            {
                addLog("rf_halt right!");
            }
            addLog("清卡成功!");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(CostThreadMethod));
            isRunning = true;
            th.Start();
            addLog("扣费开始..");
        }

        /// <summary>
        /// 扣费线程
        /// </summary>
        void CostThreadMethod()
        {
            Console.WriteLine("辅助线程开始...");
            while (isRunning)
            {
                try
                {
                    doRequestAndCost();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(100);
            }
            Console.WriteLine("辅助线程结束.");
        }

        bool isInGame = false;
        private int gameEndCount;
        private void doRequestAndCost()
        {
            //String reqCardphyId = "";
            //if (requestCard(ref reqCardphyId) == 0)
            {

                if (readCard() == 0)
                {
                    if (isInGame)
                    {
                        addLog("游戏中，请稍候再试");
                        return;
                    }
                    isInGame = true;
                    gameEndCount = CostTime * 60;
                    sendMsg("START");
                }
            }
      
        }

        private void sendMsg(string p)
        {
            if (p.Equals("START"))
            {
                addLog("发送到六轴开始");
            }
            else
            {
                addLog("发送到六轴结束");
            }
        }
        /*
        internal int requestCard(ref string reqCardphyid)
        {
            st = Program.rf_card(icdev, 1, snr);
            if (st != 0)
            {
                oldCardPhyId = "";
                return 1;
            }
            else
            {
                byte[] snrTmp = new byte[8];
                Program.hex_a(snr, snrTmp, 4);
                reqCardphyid = System.Text.Encoding.Default.GetString(snrTmp);
                Console.WriteLine("====================寻到卡" + reqCardphyid + "==================");
                if (System.Text.Encoding.Default.GetString(snrTmp).Equals(oldCardPhyId))
                {
                    return 2;
                }
                oldCardPhyId = System.Text.Encoding.Default.GetString(snrTmp);
            }
            return 0;
        }
        */
        internal int readCard()
        {
            st = Program.rf_card(icdev, 1, snr);
            if (st != 0)
            {
                return 1;
            }
            else
            {
                byte[] snrTmp = new byte[8];
                Program.hex_a(snr, snrTmp, 4);
            }
            //读卡
            st = Program.rf_authentication(icdev, 0, globalUseSector);
            if (st != 0)
            {
                return 3;
            }
            byte[] databuffer = new byte[16];
            st = Program.rf_read(icdev, globalUseSector * 4 + 1, databuffer);
            if (st != 0)
            {
                return 4;
            }
            Console.WriteLine((System.Text.Encoding.Default.GetString(databuffer)));
            //结束
            st = Program.rf_halt(icdev);
            if (st != 0)
            {
                return 5;
            }
            st = Program.rf_reset(icdev, 10);
            if (st != 0)
            {
                return 6;
            }
            st = Program.rf_beep(icdev, 5);
            if (st != 0)
            {
                return 7;
            }
            return 0;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isRunning = false;
        }

        private void FormRead_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            //读取配置文件里的IP和端口号
            String svrInfoPath = System.Environment.CurrentDirectory + @"\config.ini";
            OperateIniFile oif = new OperateIniFile(svrInfoPath, "");
            CostTime = StrToInt(oif.GetValue("local", "time"));
        }

        internal static int StrToInt(string p)
        {
            try
            {
                return Int32.Parse(p);
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e.Message);
                return 0;
            }
        }

        private void timerCount_Tick(object sender, EventArgs e)
        {
            if (isInGame)
            {
                addLog("游戏剩余秒数" + gameEndCount--);
                if (gameEndCount <= 0)
                {
                    isInGame = false;
                    sendMsg("END");
                }
            }
        }
    }
}
