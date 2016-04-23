using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Web;
using System.Management;

namespace DFangFesionSoft
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string url = "http://haijia.bjxueche.net";//登陆页面
        string imgUrl = "http://haijia.bjxueche.net/tools/CreateCode.ashx?key=ImgCode&random=" + new Random().Next(1000000, 9999999);
        string userAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
        string imgCode = "";
        bool gotCookieId;

        public string yyrq;//预约日期
        public string yysd;//预约时段
        public string yykm;//预约科目
        public int carNum = 0;//当前查询车辆数
        public int calNum = 0;//当前计数器车辆索引
        bool isYueChe = true;//查询到有车是否进行提交
        bool isSaveUser = false;//是否保存用户信息
        public int errorNum = 0;
        public bool isSuccess = false;
        public DateTime serverTime;
        public string strIP, strSubnet, strGateway, strDNS;
        public 
        Thread t1;
        Thread t2;
        Thread t3;
        string[] l1;
        string[] l2;
        string[] l3;
        //以下内容很重要，必须转义，否则传过去无法解析，直接报服务器异常
        string __VIEWSTATE = "%2FwEPDwUJMjkyMDI4NzYyD2QWAgIDD2QWAgINDxYCHglpbm5lcmh0bWwFAjIzZGR%2BdUQQ95cyfJK%2Fn%2BgUw4CA7QRRXsInn4Z72fAViUDexw%3D%3D";
        string __VIEWSTATEGENERATOR = "C2EE9ABB";
        string __EVENTVALIDATION = "%2FwEdAAbh34%2FkrVU3L24YsEhvRmaeY3plgk0YBAefRz3MyBlTcHY2%2BMc6SrnAqio3oCKbxYZxHHBh6T%2F0qvM7nnT1C8JEQiUagUcDcu68gyetszRkSY7bzJhCrs4nlwf1kSE7uBAHS2QKPeQToHahK7qvd06%2FvH7NuUyF8vqWqAHHkh5l3g%3D%3D";
        private void btnAboutCar_Click(object sender, EventArgs e)
        {
            loginMain();
        }

        public void loginMain() 
        {
            string txtUserName = this.txtUserName.Text.ToString();
            string txtPassword = this.txtUserPwd.Text.ToString();
            string txtIMGCode = this.txtImageCode.Text.ToString();
            if (txtUserName.Equals("") || txtUserPwd.Equals("") || txtImageCode.Equals(""))
            {
                MessageBox.Show("请输入用户名或者密码！");
                return;
            }
            if (txtImageCode.Equals(""))
            {
                MessageBox.Show("请输入验证码！");
                return;
            }
            if (!HttpRequestHelper.getUserLimitData(txtUserName))
            {
                printLog("您当前账号尚未开通权限，请联系管理员进行权限开通！");
                MessageBox.Show("当前账号未开通权限，请联系管理员进行权限开通才能使用！");
                return;
            }
            string loginPost = "__VIEWSTATE=" + __VIEWSTATE + "&__VIEWSTATEGENERATOR=" + __VIEWSTATEGENERATOR + "&__EVENTVALIDATION=" + __EVENTVALIDATION + "&txtUserName=" + txtUserName + "&txtPassword=" + txtPassword + "&txtIMGCode=" + txtIMGCode + "&BtnLogin=登  录";//登陆提交
            string html = "";
            string error = "";
            try
            {
                html = HttpRequestHelper.HttpPost(url, loginPost);
                html = HttpRequestHelper.replaceComma(html);
                int errorStart = html.IndexOf("<script>alert('");
                int errorEnd = html.IndexOf("');</script>");
                if (errorStart > -1)
                {
                    error = html.Substring(errorStart + 15, errorEnd - errorStart - 15);
                    if (!error.Equals(""))
                    {
                        printLog(error);
                        MessageBox.Show(error);
                    }

                }
                else
                {
                    if (error.Equals("") && html.IndexOf("ych2.aspx") > -1)
                    {
                        //启用按钮
                        this.button1.Enabled = true;
                        this.btnSubCnbh.Enabled = true;
                        printLog("登陆成功，请在7点35分左右开始约车！");
                        this.saveUserInfo(txtUserName, txtPassword);
                        //登陆成功直接获取车辆信息
                        this.getCarsList();
                    }
                    else
                    {
                        printLog("登陆异常，请重新登陆！");
                    }
                }

            }
            catch (Exception)
            {
                printLog("网络异常，请稍后重试！", Color.Red);
            }
        }

        //获取验证码
        private void btnGetCookie_Click(object sender, EventArgs e)
        {
            try
            {
                //打开网站，获取cookie
                HttpRequestHelper.HttpGet(url, "", false);
                //生成验证码
                this.createImageCode();
            }
            catch (Exception ex)
            {
                printLog("网络异常" + ex.Message);
            }
        }

        //重新生成验证码
        public void createImageCode()
        {
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(imgUrl);
                myRequest.CookieContainer = HttpRequestHelper.cookies;
                myRequest.Accept = "image/png, image/svg+xml, image/*;q=0.8, */*;q=0.5";
                myRequest.Method = "GET";
                myRequest.KeepAlive = true;
                myRequest.UserAgent = this.userAgent;
                myRequest.Referer = "http://haijia.bjxueche.net";
                myRequest.AllowAutoRedirect = true;
                //gzip, deflate
                HttpWebResponse resp = (HttpWebResponse)myRequest.GetResponse();
                foreach (Cookie ck in resp.Cookies)
                {
                    HttpRequestHelper.cookies.Add(ck);
                }
                Image image = Bitmap.FromStream(resp.GetResponseStream());
                Bitmap img = new Bitmap(image, 76, 43);
                this.pictureBox1.Image = img;
            }
            catch (Exception ex)
            {
                printLog("图片验证码获取失败，错误信息为："+ex.Message);
                MessageBox.Show("错误：图片验证码获取失败！" + ex.Message);
            }
        }


        //开始检漏
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.txtRefreshTime.Text.ToString().Equals(""))
            {
                MessageBox.Show("请输入刷新时间！");
                return;
            }
            try
            {

                this.timer1.Interval = Int32.Parse(this.txtRefreshTime.Text.ToString()) * 1000;
            }
            catch
            {
                this.timer1.Interval = 60000;
                this.txtRefreshTime.Text = "60";
            }
            bool flag = true;
            
            if (this.button1.Text.Equals("开始检漏"))
            {
                this.timer1.Enabled = true;
                this.button1.Text = "停止检漏";
                printLog("*************系统当前已开启检漏模式******************");
            }
            else
            {
                flag = false;
                this.timer1.Enabled = false;
                this.button1.Text = "开始检漏";
                printLog("*************系统当前已关闭检漏模式******************");
            }
            if (flag)
            {
                printLog("您当前设定的检漏科目为：" + this.cboKemu.Text.ToString());
                printLog("您当前设定的检漏日期为：" + this.dtYyrq.Value);
                printLog("您当前设定的检漏时段为：" + this.cboYysd.Text.ToString());
            }
            

        }

        //定时器没隔一定时间，查询一次车辆，如果当前勾选了提交，则查询到车辆后，多线程自动提交
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.getCarsList();
        }

        //获取车辆信息列表
        public void getCarsList()
        {
            //判断有车是否提交
            if (this.rdoYes.Checked)
            {
                isYueChe = true;
            }
            else
            {
                isYueChe = false;
            }

            //预约科目
            yykm = this.cboKemu.Text.ToString();
            if (yykm.Equals("科二"))
            {
                yykm = "2";
            }
            else 
            {
                yykm = "3";
            }
            //预约日期
            int year = this.dtYyrq.Value.Year;
            int month = this.dtYyrq.Value.Month;
            int day = this.dtYyrq.Value.Day;
            yyrq = "" + year;
            if (month < 10)
            {
                yyrq += "0" + month;
            }
            else
            {
                yyrq +=""+ month;
            }
            if (day < 10)
            {
                yyrq += "0" + day;
            }
            else
            {
                yyrq +=""+ day;
            }

            //预约时段
            string yysdName = this.cboYysd.Text.ToString();
            yysd = HttpRequestHelper.getXnsd(yysdName);

            //累计错误次数
            errorNum = 0;
            //初始化是否约车成功状态，一单约车成功，线程立马终止
            isSuccess = false;

            //查询科目二
            if (false)
            {
                string getCarUrl = "http://haijia.bjxueche.net/Han/ServiceBooking.asmx/GetCars";
                string param = "{\"yyrq\":\"" + yyrq + "\",\"yysd\":\"" + yysd + "\",\"xllxID\":\"" + yykm + "\",\"pageSize\":35,\"pageNum\":1}";
                //string param = "yyrq=" + yyrq + "&yysd=" + yysd + "&xllxID=" + yykm + "&pageSize=35&pageNum=1";
                string html = "";
                try
                {
                    html = HttpRequestHelper.HttpPost(getCarUrl, param);
                    html = html.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "").Replace("<string xmlns=\"http://tempuri.org/\">", "").Replace("</string>", "");
                    string jsonString = html.Split('_')[0];
                    int num = Convert.ToInt32(html.Split('_')[1]);
                    List<CarInfo> obj = FromJsonTo<List<CarInfo>>(jsonString);
                    if (obj!=null && obj.Count > 0)
                    {
                        printLog("您当前查询的" + this.dtYyrq.Value + " " + HttpRequestHelper.getXnsdName(yysd)+"，一共有"+obj.Count+"辆车可以预约。", Color.Green);
                        if (isYueChe)
                        {
                            string[] arr = new string[obj.Count];
                            int i = 0;
                            foreach (CarInfo item in obj)
                            {
                                arr[i++] = item.CNBH;
                            }

                            carNum = obj.Count;

                            //string[] ary = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };//要分割的数组  
                            int splitSize = 3;//分割线程数量
                            Object[] arrys = HttpRequestHelper.splitAry(arr, splitSize);//分割后的子块数组  
                            for (int l = 0; l < arrys.Length; l++)
                            {
                                if (l == 0)
                                {
                                    l1 = (string[])arrys[l];
                                }
                                if (l == 1)
                                {
                                    l2 = (string[])arrys[l];
                                }
                                if (l == 2)
                                {
                                    l3 = (string[])arrys[l];
                                }
                            }
                            printLog("系统已开启多线程预约功能...");
                            //调用约车功能
                            startAllThreadYueChe();

                        }
                    }
                    else
                    {
                        printLog("当前暂未查询到车辆。");
                    }
                }
                catch (Exception)
                {
                    printLog("查询车辆失败",Color.Red);
                }
            }
            else 
            {
                string getCarUrl = "http://haijia.bjxueche.net/Han/ServiceBooking.asmx/GetCars";
                string param = "{\"yyrq\":\"" + yyrq + "\",\"yysd\":\"" + yysd + "\",\"xllxID\":\"" + yykm + "\",\"pageSize\":35,\"pageNum\":1}";
                string html = "";
                try
                {
                    html = HttpRequestHelper.HttpPostByJson(getCarUrl, param);
                    html = HttpRequestHelper.replaceComma(html);
                    if (html.IndexOf("LoginOut") > -1)
                    {
                        printLog("系统已自动断开连接，软件正尝试登陆...！");
                        loginMain();
                    }
                    else 
                    {
                        html = html.Replace("{\"d\":\"", "");
                        string jsonString = html.Split('_')[0];
                        int num = Convert.ToInt32(html.Split('_')[1].Replace("\"}", ""));
                        List<CarInfo> obj = FromJsonTo<List<CarInfo>>(jsonString);
                        if (obj!=null && obj.Count > 0)
                        {
                            printLog("您当前查询的" + this.dtYyrq.Value + " " + HttpRequestHelper.getXnsdName(yysd) + "，一共有" + obj.Count + "辆车可以预约。", Color.Green);
                            if (isYueChe)
                            {
                                string[] arr = new string[obj.Count];
                                int i = 0;
                                foreach (CarInfo item in obj)
                                {
                                    arr[i++] = item.CNBH;
                                }

                                carNum = obj.Count;

                                //string[] ary = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };//要分割的数组  
                                int splitSize = 3;//分割线程数量
                                Object[] arrys = HttpRequestHelper.splitAry(arr, splitSize);//分割后的子块数组  
                                for (int l = 0; l < arrys.Length; l++)
                                {
                                    if (l == 0)
                                    {
                                        l1 = (string[])arrys[l];
                                    }
                                    if (l == 1)
                                    {
                                        l2 = (string[])arrys[l];
                                    }
                                    if (l == 2)
                                    {
                                        l3 = (string[])arrys[l];
                                    }
                                }
                                printLog("系统已开启多线程预约功能...");
                                //开启约车功能
                                startAllThreadYueChe();

                            }
                        }
                        else
                        {
                            printLog("当前暂未查询到车辆。");
                        }
                    }
                }
                catch (Exception)
                {
                    printLog("查询车辆失败", Color.Red);
                }
                
            }
            
        }


        //约车功能
        public void bookCar(string yyrq,string yysd,string cnbhStr)
        {
            if (isSuccess)
            {
                resetState();
                stopThread();
            }
            else 
            {
                string bookCarUrl = "http://haijia.bjxueche.net/Han/ServiceBooking.asmx/BookingCar";
                //{"yyrq":"20160328","xnsd":"712","cnbh":"02258","imgCode":"","KMID":"3"}
                string param = "{\"yyrq\":\"" + yyrq + "\",\"xnsd\":\"" + yysd + "\",\"cnbh\":\"" + cnbhStr + "\",\"imgCode\":\"\",\"KMID\":\"" + yykm + "\"}";
                string html = "";
                try
                {
                    html = HttpRequestHelper.HttpPostByJson(bookCarUrl, param);
                    html = HttpRequestHelper.replaceComma(html);
                    if (html.IndexOf("LoginOut") > -1)
                    {
                        printLog("系统已自动断开连接，请重新登陆！");
                    }
                    else
                    {
                        if (html.IndexOf("\"Result\":true") > -1)
                        {
                            printLog("您预约的" + this.dtYyrq.Value + " " + HttpRequestHelper.getXnsdName(yysd) + " 车牌号" + cnbhStr + "的车辆预约成功！", Color.Green);
                            MessageBox.Show("恭喜你，约车成功，您当前预约的车牌号为：" + cnbhStr + "，记得给我一个小小的评价，谢谢！");
                            isSuccess = true;
                            resetState();
                            stopThread();
                        }
                        else
                        {
                            printLog("车牌号为" + cnbhStr + "的车辆，预约失败！", Color.Red);
                            //累计错误次数，过多要及时暂停，否则可能会被系统查处
                            if (errorNum > 9)
                            {
                                stopThread();
                                resetState();
                            }
                            errorNum++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    printLog("约车失败：" + ex.Message, Color.Red);
                    stopThread();
                    resetState();
                }
            }
            
        }

        
        //开始约车
        public void btnSubCnbh_Click(object sender, EventArgs e)
        {
            //先查询是否有车辆
            this.getCarsList();
        }

        //开启所有的线程进行约车
        public void startAllThreadYueChe() 
        {
            if (this.btnSubCnbh.Text.ToString().Equals("开始约车"))
            {
                if (carNum < 0)
                {
                    return;
                }
                else
                {
                    //l1 = new string[] { "1","2","3","4"};
                    //l2 = new string[] { "5","6","7","8"};
                    //l3 = new string[] { "9","10","11"};
                    this.btnSubCnbh.Text = "停止约车";
                    t1 = new Thread(new ThreadStart(threadYueChe1));
                    t1.Name = "Thread_1";
                    t1.IsBackground = true;
                    t1.Start();
                    //t1.Join();
                    t2 = new Thread(new ThreadStart(threadYueChe2));
                    t2.Name = "Thread_2";
                    t2.IsBackground = true;
                    t2.Start();
                    //t2.Join();
                    t3 = new Thread(new ThreadStart(threadYueChe3));
                    t3.Name = "Thread_3";
                    t3.IsBackground = true;
                    t3.Start();
                    this.threadIsStop.Enabled = true;
                }

            }
            else
            {
                stopThread();
                this.btnSubCnbh.Text = "开始约车";
                printLog("您已暂停约车。");
            }
            
        }

        public object locker = new object();
        public void threadYueChe1()
        {

            if (l1 != null && l1.Length > 0)
            {
                for (int i = 0; i < l1.Length; i++)
                {
                    lock (locker)
                    {
                        printLog(Thread.CurrentThread.Name + ":您当前预约车辆编号为：" + l1[i]);
                        this.bookCar(yyrq, yysd, l1[i]);
                    }
                    Thread.Sleep(1000);
                }
            }

        }
        //线程2对应的方法
        public void threadYueChe2()
        {

            if (l2 != null && l2.Length > 0)
            {
                for (int j = 0; j < l2.Length; j++)
                {
                    lock (locker)
                    {
                        printLog(Thread.CurrentThread.Name + ":您当前预约车辆编号为：" + l2[j]);
                        this.bookCar(yyrq, yysd, l1[j]);
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        //线程3对应的方法
        public void threadYueChe3()
        {

            if (l3 != null && l3.Length > 0)
            {
                for (int k = 0; k < l3.Length; k++)
                {
                    lock (locker)
                    {
                        printLog(Thread.CurrentThread.Name + ":您当前预约车辆编号为：" + l3[k]);
                        this.bookCar(yyrq, yysd, l1[k]);
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        public void stopThread()
        {
            t1.Abort();
            t2.Abort();
            t3.Abort();
        }
        //将Json对象进行序列发
        public T FromJsonTo<T>(string jsonString)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                T jsonObject = (T)ser.ReadObject(ms);
                return jsonObject;
            }
            catch (Exception ex) 
            {
                printLog("数据转换异常，请及时联系管理员." + ex.Message,Color.Red);
                return default(T);
            }
        }

        //打印日志，不带颜色
        public void printLog(string logStr)
        {
            string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.logPrint.SelectionColor = Color.Black;

            this.logPrint.AppendText(datetime.Split(' ')[1] + " " + logStr + "\r\n");
            this.logPrint.Focus();
        }
        //打印日志，带颜色
        public void printLog(string logStr, Color color) 
        {
            string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (color == null)
            {
                this.logPrint.SelectionColor = Color.Black;
            }
            else
            {
                this.logPrint.SelectionColor = color;
            }

            this.logPrint.AppendText(datetime.Split(' ')[1] + " " + logStr + "\r\n");
            this.logPrint.Focus();
        }

        public void resetState()
        {
            this.btnSubCnbh.Text = "开始约车";
            this.timer1.Enabled = false;
            this.button1.Text = "开始检漏";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                cboYysd.SelectedIndex = 0;
                cboKemu.SelectedIndex = 0;
                DateTime dt = DateTime.Now;
                dt = dt.AddDays(7);
                this.dtYyrq.Value = dt;//获取服务器时间
                string dateHtml = HttpRequestHelper.HttpGet("http://haijia.xuechebu.com:8008/API/GetServiceDate", "", true);

                dateHtml = HttpRequestHelper.replaceComma(dateHtml);
                MessageCode timeInfo = FromJsonTo<MessageCode>(dateHtml);
                if (timeInfo.code == 0)
                {
                    serverTime = StampToDateTime(timeInfo.data);
                    lblServerTime.Text = serverTime.ToLongTimeString().ToString();
                    this.timerServer.Enabled = true;
                }
                printLog("如果您早上约车，请在7点35分准时登陆，右上角显示的是驾校服务器时间,");
                Control.CheckForIllegalCrossThreadCalls = false;
                FileStream aFile = new FileStream("c:/haijia.txt", FileMode.OpenOrCreate);
                StreamReader sr = new StreamReader(aFile);
                if (sr != null)
                {
                    string username = sr.ReadLine();
                    if (username == null)
                    {
                        isSaveUser = false;
                        sr.Close();
                        return;
                    }
                    else
                    {
                        this.txtUserName.Text = username;
                        string pwd = sr.ReadLine();
                        this.txtUserPwd.Text = pwd;
                        isSaveUser = true;
                    }
                }
                else 
                {
                    isSaveUser = false;
                }

            }
            catch
            {
                isSaveUser = false;
            }
        }

        // 时间戳转为C#格式时间
        private DateTime StampToDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        //将登录信息保存在本地，以便下次使用
        public void saveUserInfo(string username, string userpwd)
        {
            try
            {
                FileStream aFile = new FileStream("c:/haijia.txt", FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(aFile);
                // Write data to file.
                sw.WriteLine(username);
                sw.WriteLine(userpwd);
                sw.Write("##以上信息主要为了保存您当前约车软件的账号和密码，方便下次登录，避免每次都输入账号和密码##");
                sw.Write("##第一行为用户名即学习证号，第二行为密码，请不要随意调换格式##");
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }
        }


        private void threadIsStop_Tick(object sender, EventArgs e)
        {
            if ((t1 != null && !t1.IsAlive) && (t2 != null && !t2.IsAlive) && (t3 != null && !t3.IsAlive))
            {
                this.btnSubCnbh.Text = "开始约车";
                this.threadIsStop.Enabled = false;
            }
        }

        private void timerServer_Tick(object sender, EventArgs e)
        {
            serverTime = serverTime.AddSeconds(1);
            lblServerTime.Text = serverTime.ToLongTimeString().ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ("".Equals(strIP))
            {
                GetIPAndDNS();
            }
            SetNetworkAdapter();
        }

        public void GetIPAndDNS()
        {
            strIP = "0.0.0.0";
            strSubnet = "0.0.0.0";
            strGateway = "0.0.0.0";
            strDNS = "0.0.0.0";
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection nics = mc.GetInstances();
                foreach (ManagementObject nic in nics)
                {
                    try
                    {
                        if (Convert.ToBoolean(nic["IPEnabled"]) == true)
                        {

                            if ((nic["IPAddress"] as String[]).Length > 0 && strIP == "0.0.0.0")
                            {
                                strIP = (nic["IPAddress"] as String[])[0];
                            }
                            if ((nic["IPSubnet"] as String[]).Length > 0 && strSubnet == "0.0.0.0")
                            {
                                strSubnet = (nic["IPSubnet"] as String[])[0];
                            }
                            if ((nic["DefaultIPGateway"] as String[]).Length > 0 && strGateway == "0.0.0.0")
                            {
                                strGateway = (nic["DefaultIPGateway"] as String[])[0];
                            }
                            if ((nic["DNSServerSearchOrder"] as String[]).Length > 0 && strDNS == "0.0.0.0")
                            {
                                strDNS = (nic["DNSServerSearchOrder"] as String[])[0];
                            }

                            printLog("本机IP:" + strIP, Color.Orange);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        //修改电脑ip
        private void SetNetworkAdapter()
        {
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            string newIp = "";
            ManagementObjectCollection moc = mc.GetInstances();

            Random ran = new Random();
            int RandKey = ran.Next(1, 255);
            
            newIp = strIP.Split('.')[0] + "." + strIP.Split('.')[1] + "." + strIP.Split('.')[2] + "." + RandKey.ToString();
            
            foreach (ManagementObject mo in moc)
            {
                if (!(bool)mo["IPEnabled"])
                    continue;

                //设置ip地址和子网掩码
                inPar = mo.GetMethodParameters("EnableStatic");

                inPar["IPAddress"] = new string[] { newIp };// 1.备用 2.IP
                inPar["SubnetMask"] = new string[] { strSubnet };
                outPar = mo.InvokeMethod("EnableStatic", inPar, null);

                //设置网关地址
                inPar = mo.GetMethodParameters("SetGateways");
                inPar["DefaultIPGateway"] = new string[] { strGateway };// 1.网关;2.备用网关
                outPar = mo.InvokeMethod("SetGateways", inPar, null);

                //设置DNS
                inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                inPar["DNSServerSearchOrder"] = new string[] { strDNS };// 1.DNS 2.备用DNS
                outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                break;
            }
            printLog("修改后的IP为：" + newIp + ",如果此IP与其他人冲突，则从新获取即可。");
        }
    }
}
