using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AUZT_Authorization.Properties;
using CCWin;
using Microsoft;

namespace AUZT_Authorization
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int IParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
       User user=new User();
      

        private void hltdpLbvon_MouseEnter(object sender, EventArgs e)
        {
            hltdpLbvon.BackgroundImage = Resources.关闭_red;
        }

        private void hltdpLbvon_MouseLeave(object sender, EventArgs e)
        {
            hltdpLbvon.BackgroundImage = Resources.关闭;
        }

        private void hltdpLbvon_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Login.ReleaseCapture();
            Login.SendMessage(base.Handle, 274, 61458, 0);
        }
        public void ShowAForm()
        {
            Main main = new Main();
            main.Refreshtheform = button1ui;
            main.Owner = this;
            main.ShowDialog(this);
            //Thread thread = new Thread(ThreadFuntion);
            //thread.ApartmentState = ApartmentState.STA;
            //thread.IsBackground = true;
            //thread.Start();
           
        }
       

        private string uname = string.Empty;
        private string UserPwd = string.Empty;
        private void ThreadFuntion2()
        {
            try
            {
                //var db = DB.conntionDB;
                string sql = " select * from QX_USERS where USERID=@USERID and PWD=@PWD ";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@USERID", uname), //自定义参数  与参数类型    
                    new SqlParameter("@PWD", UserPwd)
                };

                var ds = DB.Query(sql, parameters);
                if (ds.Tables[0].Rows.Count == 1)
                {
                    UserLogin.Init();
                    int i = UserLogin.AddUserToList("user", Convert.ToInt32(ds.Tables[0].Rows[0]["AUTOID"]), ds.Tables[0].Rows[0]["USERID"].ToString(), ds.Tables[0].Rows[0]["NAME"].ToString(), ds.Tables[0].Rows[0]["DEPT"].ToString(), ds.Tables[0].Rows[0]["STATUS"].ToString());
                    if (i == 0)
                    {
                        ShowAForm();
                    }
                    else
                    {
                        MessageBox.Show("系统异常，登录失败");
                        button1ui("登录");
                    }


                }
                else
                {
                    MessageBox.Show("账号或密码输入错误");
                    button1ui("登录");
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("无法访问服务器"))
                {      
                    MessageBox.Show("无法访问服务器");
                    button1ui("登录");
                }
                else
                {
                    MessageBox.Show(ex.Message);
                    button1ui("登录");
                }
                
                
            }
           

        }
        //public void updateUi(string str)
        //{
        //    if (label2.InvokeRequired)
        //    {
        //        // 当一个控件的InvokeRequired属性值为真时，说明有一个创建它以外的线程想访问它
        //        Action<string> actionDelegate = (x) => { this.button1.Text = x.ToString(); };
        //        // 或者
        //        // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
        //        this.button1.Invoke(actionDelegate, str);
        //    }
        //    else
        //    {
        //        this.button1.Text = str.ToString();
        //    }
        //}
        private void button1_Click(object sender, EventArgs e)
        {
           
             uname = this.comboBox1.Text.Trim();

            if (string.IsNullOrEmpty(this.comboBox1.Text.Trim()))
            {
                MessageBox.Show("请输入用户名");
                return;
            }

            if (string.IsNullOrEmpty(bkAddvlNk8.Text.Trim()))
            {
                MessageBox.Show("请输入密码");
                return;
            }
             UserPwd = MD5ALGO.getMd5Hash(bkAddvlNk8.Text);
            button1ui("正在登录...");
            Thread thread = new Thread(ThreadFuntion2);
            thread.ApartmentState = ApartmentState.STA;
            thread.IsBackground = true;
            thread.Start();

        }

        private void Login_Paint(object sender, PaintEventArgs e)
        {

            System.Drawing.Drawing2D.GraphicsPath buttonPath =
       new System.Drawing.Drawing2D.GraphicsPath();
            Point[] pons = new Point[]
                               {
                                   
                                   new Point(0,85),
                                   new Point(351,85),
                                   new Point(351,  61),
                                    new Point(351+159,61),
                                    new Point(351+159,85),
                                    new Point(570, 85),
                                    new Point(570, 443),
                                    new Point(0, 443)
                                   
                               };


            buttonPath.AddPolygon(pons);

            this.Region = new System.Drawing.Region(buttonPath);
        }

        private void button1_TextChanged(object sender, EventArgs e)
        {
            if(button1.Text=="登录")
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled=false;
            }
        }

       
    

        private void Login_Load(object sender, EventArgs e)
        {
          
            label4.Text = "版本号：V" + Operation.Version;
            //Thread thread = new Thread(new ThreadStart(this.showAbout))
            //{
            //    IsBackground = true
            //};
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
          
        }
      
        // Token: 0x0600087A RID: 2170 RVA: 0x00031038 File Offset: 0x0002F238
        private void button1ui(string str)
        {
            if (this.button1.InvokeRequired)
            {
                if (!this.button1.IsDisposed)
                {
                    this.button1.BeginInvoke(new MethodInvoker(delegate
                    {
                        this.button1.Text = str;
                    }));
                    return;
                }
            }
            else
            {
                this.button1.Text = str;
            }
        }
        private void showAbout()
        {
            string text = Application.StartupPath + "\\更新前的程序\\";
            DeleteFile(text);
            this.button1ui("正在联网检测更新...");
            if (this.showAbout_bool())//有更新同意更新
            {
                this.button1ui("登录");
               
            }else
            {
                this.button1ui("服务器连接失败...");
                Thread.Sleep(2000);
                base.DialogResult = DialogResult.Cancel;
            }
          
        }
        public void DeleteFile(string path)
        {
            if (Directory.Exists(path))
            {
                FileAttributes attr = File.GetAttributes(path);
                if (attr == FileAttributes.Directory)
                {
                    Directory.Delete(path, true);
                }
                else
                {
                    File.Delete(path);
                }
            }
           
        }
        // Token: 0x0600087B RID: 2171 RVA: 0x000310A4 File Offset: 0x0002F2A4
        private bool showAbout_bool()
        {
            bool b = false;
            try
            {
                //链接服务取查询版本差异判断是否需要更新
                string Url = AppConfig.GetValue("update_url");//Common.Config.ReadValue("UserInfo", "Url");
                //1.从服务器下载配置文件，知道是否有新版本
                string url = Url + "/update.xml";
                string filepath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + @"\update.xml";
                try
                {
                    string result = DownFiles.DownloadFile(url, filepath, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //2.读取update.xml文件
                XmlFiles xmlfiles = new XmlFiles(filepath);
                string newVersion = xmlfiles.FindNode("UpdateConfig").SelectSingleNode("NewVersion").InnerText;
                string updatelog = xmlfiles.FindNode("UpdateConfig").SelectSingleNode("updatelog").InnerText;
                string updateurl = xmlfiles.FindNode("UpdateConfig").SelectSingleNode("Url").InnerText + xmlfiles.FindNode("UpdateConfig").SelectSingleNode("Files").InnerText;
                //如果程序版本小于服务器版本，则需要更新

                if (Convert.ToInt32(Operation.Version.Replace(".", "")) < Convert.ToInt32(newVersion.Replace(".", "")))
                {
                    base.Invoke(new Action(delegate
                    {
                        About about = new About(updatelog, newVersion, updateurl);
                        //about.js(MessageInfo.br, string_0);
                        DialogResult dialogResult = about.ShowDialog();
                        if (dialogResult == DialogResult.Abort)
                        {
                            b = true;
                        }
                    }));
                }
                else
                {
                    b = true;
                }
            }
            catch (Exception)
            {

                b = false;
            }
           

                return b;
        }


     
      
    }
}
