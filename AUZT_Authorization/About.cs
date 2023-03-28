using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using AUZT_Authorization.Properties;
using CCWin;

namespace AUZT_Authorization
{
    public partial class About : CCSkinMain
    {
        private string Updatelog ;
        private string NewVersion;
        private string Updateurl;
        public About()
        {
            InitializeComponent();
        }
        public About(string updatelog, string newVersion,string updateurl)
        {
            Updatelog = updatelog;
            NewVersion = newVersion;
            Updateurl = updateurl;
            InitializeComponent();
        }

       
      
        private void About_Load(object sender, EventArgs e)
        {
            this.label5.Text = string.IsNullOrEmpty(NewVersion) ? Operation.Version : NewVersion;
            this.label4.Text = Operation.Version;
            new Thread(new ThreadStart(this.getlog))
            {
                IsBackground = true
            }.Start();
        }
        private void uifangfa(string str)
        {
            if (this.textBox1.InvokeRequired)
            {
                if (!this.textBox1.IsDisposed)
                {
                    this.textBox1.BeginInvoke(new MethodInvoker(delegate
                    {
                        this.textBox1.Text = str;
                    }));
                    return;
                }
            }
            else
            {
                this.textBox1.Text = str;
            }
        }
        // Token: 0x060006AA RID: 1706 RVA: 0x0000F640 File Offset: 0x0000D840
        [CompilerGenerated]
        private void getlog()
        {
            try
            {
                //从服务器获取更新内容
                string responseSTRING = @"[
    {
        ""Edition"": ""1.0.0.1"",
        ""UpdateTime"": ""2017-02-12"",
        ""UpdateContent"": [
            ""123""
           
        ]
    }
]
";

              //  VersionLogs v=new VersionLogs();
              //   List<VersionLogs> listv=new List<VersionLogs>();
              //  v.Edition = "1.0.0.1";
              //  v.UpdateTime = DateTime.Now;
              //  v.UpdateContent=new List<string>(){"123","234","345"};
              //  listv.Add(v);
              //  JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
              //string str=  javaScriptSerializer.Serialize(listv);

                if (!string.IsNullOrEmpty(Updatelog))
                {
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    List<VersionLogs> list = javaScriptSerializer.Deserialize<List<VersionLogs>>(Updatelog);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("全部日志");
                    foreach (VersionLogs current in list)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append("版本：[" + current.Edition.Trim() + "]");
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append("时间：" + current.UpdateTime.ToString("yyyy-MM-dd"));
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append("内容：");
                        for (int i = 1; i <= current.UpdateContent.Count; i++)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(i.ToString() + "." + current.UpdateContent[i - 1]);
                        }
                        stringBuilder.Append(Environment.NewLine);
                    }
                    uifangfa(stringBuilder.ToString());
                }
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            base.Visible = false;
            UpdateLog update = new UpdateLog(Updateurl);
            update.ShowDialog();
        }
        int i = 10;
        private void timer1_Tick(object sender, EventArgs e)
        {
           
            if(i<=1)
            {
                timer1.Enabled = false;
                button1.Text = "自动更新";
                button1.Enabled = true;
            }
            else
            {
                i = i - 1;
                button1.Text = "自动更新("+i+")";
            }
        }

    }
}
