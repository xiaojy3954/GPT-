using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AUZT_Authorization
{
    public partial class UpdateLog : Form
    {
        private string Updateurl;
        public UpdateLog()
        {
			
			this.InitializeComponent();
        }

        public UpdateLog(string updateurl)
        {
            Updateurl = updateurl;
            this.InitializeComponent();
        }
        // Token: 0x060008CB RID: 2251 RVA: 0x00006DDC File Offset: 0x00004FDC
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.ljuoKgOdRs != null)
            {
                this.ljuoKgOdRs.Dispose();
            }
            base.Dispose(disposing);
        }

        // Token: 0x060008C9 RID: 2249 RVA: 0x0003467C File Offset: 0x0003287C
        private void method_0()
        {
            try
            {
                string moduleName = Process.GetCurrentProcess().MainModule.ModuleName;
                this.SetTextValue("正在获取下载信息");
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Updateurl);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                long contentLength = httpWebResponse.ContentLength;
                if (contentLength / 1024L / 1024L < 1L)
                {
                    this.SetTextValue("文件大小不符，请检查网络后重试");
                    return;
                }
                this.SetProBarMax((int)contentLength);
                this.SetTextValue("文件大小:" + contentLength / 1024L + "KB,当前进度:0KB");
                this.TfXoYcYdx7 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase.Substring(0, Application.StartupPath.IndexOf(':')) + ":\\";
                Stream responseStream = httpWebResponse.GetResponseStream();
                if (File.Exists(this.TfXoYcYdx7 + moduleName))
                {
                    File.Delete(this.TfXoYcYdx7 + moduleName);
                }
                Stream stream = new FileStream(this.TfXoYcYdx7 + moduleName, FileMode.Create);
                long num = 0L;
                byte[] array = new byte[1024];
                int i = responseStream.Read(array, 0, array.Length);
                int num2 = 0;
                while (i > 0)
                {
                    num = (long)i + num;
                    num2 += i;
                    if (num2 >= 20480)
                    {
                        this.SetTextValue(string.Concat(new object[]
						{
							"文件大小:",
							contentLength / 1024L,
							"KB,当前进度:",
							num / 1024L,
							"KB"
						}));
                        num2 = 0;
                    }
                    Application.DoEvents();
                    stream.Write(array, 0, i);
                    this.SetProBarValue((int)num);
                    i = responseStream.Read(array, 0, array.Length);
                }
                this.SetTextValue(string.Concat(new object[]
				{
					"文件大小:",
					contentLength / 1024L,
					"KB,当前进度:",
					num / 1024L,
					"KB"
				}));
                stream.Close();
                responseStream.Close();
                this.SetProBarMax(1);
                string text = Application.StartupPath + "\\更新前的程序\\";
               
                string text2 = Application.StartupPath + "\\" + moduleName;
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }
                Directory.Move(text2, text + moduleName);
                Directory.Move(this.TfXoYcYdx7 + moduleName, text2);
            }
            catch
            {
            }
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    WorkingDirectory = Application.ExecutablePath
                }
            }.Start();
            Application.Exit();
          
        }
       
     
        // Token: 0x060008C8 RID: 2248 RVA: 0x00034638 File Offset: 0x00032838
        public void SetTextValue(string value)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new UpdateLog.SetText(this.SetTextValue), new object[]
				{
					value
				});
                return;
            }
            this.lblMsg.Text = value;
        }

        // Token: 0x060008C6 RID: 2246 RVA: 0x00034564 File Offset: 0x00032764
        public void SetProBarMax(int i)
        {
            if (this.probar.InvokeRequired)
            {
                this.probar.Invoke(new UpdateLog.SetValue(this.SetProBarMax), new object[]
				{
					i
				});
                return;
            }
            this.probar.Value = 0;
            this.probar.Maximum = i;
        }

        // Token: 0x060008C7 RID: 2247 RVA: 0x000345C0 File Offset: 0x000327C0
        public void SetProBarValue(int i)
        {
            if (this.probar.InvokeRequired)
            {
                this.probar.Invoke(new UpdateLog.SetValue(this.SetProBarValue), new object[]
				{
					i
				});
                return;
            }
            if (i <= this.probar.Maximum)
            {
                this.probar.Value = i;
                return;
            }
            this.probar.Value = this.probar.Maximum;
        }
        // Token: 0x060008CD RID: 2253 RVA: 0x00006DFB File Offset: 0x00004FFB
        [CompilerGenerated]
        private void method_1()
        {
            this.method_0();
        }

        private void UpdateLog_Load(object sender, EventArgs e)
        {
            //VoCode.isExit = true;
            int x = Screen.PrimaryScreen.WorkingArea.Size.Width - base.Width;
            int y = Screen.PrimaryScreen.WorkingArea.Size.Height - base.Height;
            base.SetDesktopLocation(x, y);
            new Thread(new ThreadStart(this.method_1))
            {
                IsBackground = true
            }.Start();
        }
    }
}
