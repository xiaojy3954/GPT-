using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CCWin;
using System.Reflection;

namespace AUZT_Authorization
{
    public partial class Main : CCSkinMain
    {

        private BackgroundWorker bkWorker = new BackgroundWorker();
        private BackgroundWorker bkWorker2 = new BackgroundWorker();
        private Form1 notifyForm = new Form1();
        private string msg = string.Empty;
        int index = 0;
        public Main()
        {
            InitializeComponent();

            // 使用BackgroundWorker时不能在工作线程中访问UI线程部分，  
            // 即你不能在BackgroundWorker的事件和方法中操作UI，否则会抛跨线程操作无效的异常  
            // 添加下列语句可以避免异常。  
            CheckForIllegalCrossThreadCalls = false;

            bkWorker.WorkerReportsProgress = true;
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker2.WorkerReportsProgress = true;
            bkWorker2.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new DoWorkEventHandler(DoWork);
            bkWorker2.DoWork += new DoWorkEventHandler(DoWork2);
            bkWorker.ProgressChanged += new ProgressChangedEventHandler(ProgessChanged);
            bkWorker2.ProgressChanged += new ProgressChangedEventHandler(ProgessChanged2);
            bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompleteWork);
            bkWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompleteWork2);

            messages = new List<string>();
            messages.Add("如果打印时经销商一直保持第一条不变，可能是经销商没有同步，可以试试手动同步");
            messages.Add("鼠标点击预览图片后即可用鼠标滚轮放大缩小预览图");
          

            lbl_tgText.AutoSize = true;
            lbl_tgText.Text = messages[index];
        }
        #region 进度条代码
        public void CompleteWork(object sender, RunWorkerCompletedEventArgs e)
        {

            notifyForm.Close();
            MessageBoxEx.Show(msg);
        }
        public void CompleteWork2(object sender, RunWorkerCompletedEventArgs e)
        {

            getCUSTOMER();
            notifyForm.Close();
            MessageBoxEx.Show(msg);
        }
        public void ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            // bkWorker.ReportProgress 会调用到这里，此处可以进行自定义报告方式  
            notifyForm.SetNotifyInfo(e.ProgressPercentage, "正在下载打印记录       处理进度:" + Convert.ToString(e.ProgressPercentage) + "%");
        }
        public void ProgessChanged2(object sender, ProgressChangedEventArgs e)
        {
            // bkWorker.ReportProgress 会调用到这里，此处可以进行自定义报告方式  
            notifyForm.SetNotifyInfo(e.ProgressPercentage, "正在同步经销商数据       处理进度:" + Convert.ToString(e.ProgressPercentage) + "%");
        }
        public void DoWork(object sender, DoWorkEventArgs e)
        {
            // 事件处理，指定处理函数  
            e.Result = ProcessProgress(bkWorker, e);
        }
        public void DoWork2(object sender, DoWorkEventArgs e)
        {
            // 事件处理，指定处理函数  
            e.Result = ProcessProgress2(bkWorker2, e);
        }
        SaveFileDialog saveDialog = new SaveFileDialog();
        /// <summary>
        /// 同步经销商数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private int ProcessProgress2(object sender, DoWorkEventArgs e)
        {
            string sql_ins = string.Empty;
            try
            {
                string sql = string.Format(@"SELECT * FROM BARERP.DBO.V_BAR_CUSTOMER WHERE NAMECUST not like '%STOP%'");
                DataTable table = DB.Query(sql).Tables[0];

                decimal cnt = 0;
                foreach (DataRow row in table.Rows)
                {
                    Application.DoEvents();
                    sql = string.Format("SELECT * FROM [AUZT_SYS_DB].[dbo].[QX_CUSTOMER] WHERE LTRIM(RTRIM(CUST_NO))='{0}' AND LTRIM(RTRIM(CUST_NAME)) = '{1}'",
                        row["IDCUST"].ToString().Trim(), row["NAMECUST"].ToString().Trim());
                    DataTable table_tmp = DB.Query(sql).Tables[0];
                    if (table_tmp.Rows.Count == 0)
                    {
                        sql_ins = string.Format(@"INSERT INTO [AUZT_SYS_DB].[dbo].[QX_CUSTOMER](CUST_NO,CUST_NAME,CUST_NAME_EN,STATUS,OP_USER)
                        VALUES (@CUST_NO,@CUST_NAME,@CUST_NAME_EN,@STATUS,@OP_USER)");
                        SqlParameter[] parameters = { 
                                    new SqlParameter("@CUST_NO", SqlDbType.VarChar, 50),
                                    new SqlParameter("@CUST_NAME", SqlDbType.VarChar, 100),
                                    new SqlParameter("@CUST_NAME_EN", SqlDbType.VarChar, 100),
                                    new SqlParameter("@STATUS", SqlDbType.VarChar, 50),
                                    new SqlParameter("@OP_USER", SqlDbType.VarChar, 50)};
                        parameters[0].Value = row["IDCUST"].ToString().Trim();
                        parameters[1].Value = row["NAMECUST"].ToString().Trim();
                        parameters[2].Value = row["NAMECUST_ENG"].ToString().Trim();
                        parameters[3].Value = "0";
                        parameters[4].Value = "autoSync";
                        try
                        {
                            cnt += DB.ExecuteSql(sql_ins, parameters);

                            //lblMessage.Text = "已同步记录数：" + cnt.ToString(); lblMessage.Update();
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                    }
                    else
                    {
                        cnt += 1;
                    }
                    // 状态报告  
                    bkWorker2.ReportProgress(Convert.ToInt32(Math.Round(cnt / table.Rows.Count, 2) * 100));
                    //bkWorker.ReportProgress(cnt / table.Rows.Count * 100);

                    // 等待，用于UI刷新界面，很重要  
                    //System.Threading.Thread.Sleep(1);


                }

                msg = "同步成功";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return -1;
        }
        private int ProcessProgress(object sender, DoWorkEventArgs e)
        {
            string saveFileName = "";
            saveFileName = saveDialog.FileName;
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
            {
                msg = "无法创建Excel对象，可能您的机子未安装Excel";
                return -1;
            }

            Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;
            Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];//取得sheet1  

            //写入标题  
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
            }
            //写入数值  
            for (int r = 0; r < dataGridView1.Rows.Count; r++)
            {
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    worksheet.Cells[r + 2, i + 1] = dataGridView1.Rows[r].Cells[i].Value;
                }
                System.Windows.Forms.Application.DoEvents();
                // 状态报告  
                bkWorker.ReportProgress(Convert.ToInt32(Math.Round(((decimal)r + 1) / dataGridView1.Rows.Count, 2) * 100));

                // 等待，用于UI刷新界面，很重要  
                System.Threading.Thread.Sleep(1);
            }
            worksheet.Columns.EntireColumn.AutoFit();//列宽自适应  
            //if (Microsoft.Office.Interop.cmbxType.Text != "Notification")  
            //{  
            //    Excel.Range rg = worksheet.get_Range(worksheet.Cells[2, 2], worksheet.Cells[ds.Tables[0].Rows.Count + 1, 2]);  
            //    rg.NumberFormat = "00000000";  
            //}  

            if (saveFileName != "")
            {
                try
                {
                    workbook.Saved = true;
                    workbook.SaveCopyAs(saveFileName);
                    //fileSaved = true;  
                }
                catch (Exception ex)
                {
                    //fileSaved = false;  
                    msg = "导出文件时出错,文件可能正被打开！\n" + ex.Message;
                }

            }
            //else  
            //{  
            //    fileSaved = false;  
            //}  
            xlApp.Quit();
            GC.Collect();//强行销毁   
            // if (fileSaved && System.IO.File.Exists(saveFileName)) System.Diagnostics.Process.Start(saveFileName); //打开EXCEL  
            msg = "导出成功";


            return -1;
        }
        #endregion

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        User user = new User();
        private bool isBd = false;
        public string biaoti;
        public string wenben;
        public string sqsNo;
        public string number;

        public string CUST_NO;
        private DataRowView dr_EN;
        private bool isDY = false;
        private bool isBD = false;
        public delegate void RefreshTheForm(string str);//声明委托
        public RefreshTheForm Refreshtheform;//委托对象
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                printPreviewControl1.MouseWheel += new MouseEventHandler(printPreviewControl1_MouseWheel);
                printPreviewControl1.Click += new EventHandler(ppcPreview_Click);
                printPreviewControl2.MouseWheel += new MouseEventHandler(printPreviewControl1_MouseWheel);
                printPreviewControl2.Click += new EventHandler(ppcPreview_Click);
                label38.Text = "欢迎使用福斯授权书打印系统 V" + Operation.Version;
                skinLabel8.Text = "版本：V" + Operation.Version;

                Hashtable userLogin = UserLogin.GetUserLogin();
                user = (User)userLogin["user"];
                label34.Text = user.NAME;

                label41.Text = user.DEPT == "-1" ? "无" : user.DEPT;
                if (user.DEPT == "-1")
                {
                    pictureBox4.Visible = true;
                    this.SysButtonItems[0].Visibale = true;
                }

                getCOMPANY();
                Refreshtheform("加载授权方数据...");
                getCUSTOMER();
                Refreshtheform("加载经销商数据...");
                getDEPT();
                Refreshtheform("加载部门数据...");






                comboBox4.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox4.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox3.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox3.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox2.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                #region 打印记录属性控制
                dateTimePicker3.Value = Convert.ToDateTime(DateTime.Now.Year + "-01-01");
                dateTimePicker4.Value = Convert.ToDateTime(DateTime.Now.Year + 1 + "-12-31");
                dateTimePicker5.Value = Convert.ToDateTime(DateTime.Now.Year + "-01-01");
                dateTimePicker6.Value = Convert.ToDateTime(DateTime.Now.Year + 1 + "-12-31");
                comboBox4.Text = string.Empty;
                comboBox3.Text = string.Empty;
                #endregion


                dateTimePicker1.Value = DateTime.Now;
                dateTimePicker2.Value = DateTime.Now.AddYears(1);


                printdoc.PrintPage += new PrintPageEventHandler(PrintPage1);
                printdoc2.PrintPage += new PrintPageEventHandler(PrintPage2);
                //this.dateTimePicker1.CustomFormat = "yyyy-MM-dd";
                //this.dateTimePicker2.CustomFormat = "yyyy-MM-dd";

                string no = DateTime.Now.Year.ToString() + user.DEPT + "001";
                label10.Text = no;
                #region 关闭登录界面
                this.Owner.Hide();
                #endregion


                getYPLX();
                textBox1.Text = AppConfig.GetValue("bt_X");
                textBox2.Text = AppConfig.GetValue("bt_Y");
                textBox3.Text = AppConfig.GetValue("wb_Y");
                textBox4.Text = AppConfig.GetValue("wb_X");

                textBox5.Text = AppConfig.GetValue("wb_Y_bd");
                textBox6.Text = AppConfig.GetValue("wb_X_bd");
                textBox7.Text = AppConfig.GetValue("bt_Y_bd");
                textBox8.Text = AppConfig.GetValue("bt_X_bd");
                this.skinTabControl2.SelectedIndex = 0;
                this.skinTabPage8.Parent = null;
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
            }

        }

        private void printPreviewControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                if (((PrintPreviewControl)sender).Name == "printPreviewControl1")
                {
                    if (e.Delta > 0) //向上
                    {
                        zoom = zoom + 0.03;
                        if (zoom > 5.0)
                        {
                            zoom = 5.0;
                        }
                        ((PrintPreviewControl)sender).Zoom = zoom;
                    }
                    else //向下
                    {
                        zoom = zoom - 0.03;
                        if (zoom < 0.1)
                        {
                            zoom = 0.1;
                        }
                        ((PrintPreviewControl)sender).Zoom = zoom;
                    }
                }
                else
                {
                    if (e.Delta > 0) //向上
                    {
                        zoom2 = zoom2 + 0.03;
                        if (zoom2 > 5.0)
                        {
                            zoom2 = 5.0;
                        }
                        ((PrintPreviewControl)sender).Zoom = zoom2;
                    }
                    else //向下
                    {
                        zoom2 = zoom2 - 0.03;
                        if (zoom2 < 0.1)
                        {
                            zoom2 = 0.1;
                        }
                        ((PrintPreviewControl)sender).Zoom = zoom2;
                    }
                }

            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        /// <summary>  
        /// 鼠标在控件上点击时，需要处理获得焦点，因为默认不会获得焦点  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void ppcPreview_Click(object sender, EventArgs e)
        {
            ((PrintPreviewControl)sender).Select();
            ((PrintPreviewControl)sender).Focus();
        }


        private void getCOMPANY()
        {
            try
            {
                //var db = DB.conntionDB;
                string sql = " select * from QX_COMPANY  ";
                var ds = DB.Query(sql);
                if (ds.Tables[0].Rows.Count != 0)
                {
                    comboBox2.DataSource = ds.Tables[0].Copy().DefaultView;
                    comboBox2.DisplayMember = "COMPANY";
                    comboBox2.ValueMember = "COMPANY_EN";
                    comboBox4.DataSource = ds.Tables[0].Copy().DefaultView;
                    comboBox4.DisplayMember = "COMPANY";
                    comboBox4.ValueMember = "COMPANY_EN";
                    comboBox4.Text = string.Empty;

                }
                else
                {
                    MessageBoxEx.Show("授权方绑定失败，请联系管理员");
                }

            }
            catch (Exception ex)
            {
                MessageBoxEx.Show("授权方绑定失败，请联系管理员");
            }
        }

        private void getCUSTOMER()
        {

            try
            {
                //var db = DB.conntionDB;
                string sql = " select * from QX_CUSTOMER  ";
                var ds = DB.Query(sql);
                if (ds.Tables[0].Rows.Count != 0)
                {
                    comboBox1.DataSource = ds.Tables[0].Copy().DefaultView;
                    comboBox1.DisplayMember = "CUST_NAME";
                    comboBox1.ValueMember = "CUST_NO";
                    comboBox3.DataSource = ds.Tables[0].Copy().DefaultView;
                    comboBox3.DisplayMember = "CUST_NAME";
                    comboBox3.ValueMember = "CUST_NO";

                    comboBox3.Text = string.Empty;
                }
                else
                {
                    MessageBoxEx.Show("授权经销商绑定失败，请联系管理员");
                }

            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message + "\r\n请联系管理员");
            }
        }

        private void getDEPT()
        {
            try
            {
                //var db = DB.conntionDB;
                string sql = " select * from [QX_MAIL]  ";
                var ds = DB.Query(sql);
                if (ds.Tables[0].Rows.Count != 0)
                {
                    com_DEPT.DataSource = ds.Tables[0].DefaultView;
                    com_DEPT.DisplayMember = "DEPT";
                    com_DEPT.ValueMember = "DEPT";
                    if (user.DEPT != "-1")
                    {
                        com_DEPT.SelectedValue = user.DEPT;
                        com_DEPT.Enabled = false;

                    }

                }
                else
                {
                    MessageBoxEx.Show("授权经销商绑定失败，请联系管理员");
                }

            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message + "\r\n请联系管理员");
            }
        }
        private void getYPLX()
        {
            try
            {
                DataTable dt = new DataTable();
                DataColumn dc1 = new DataColumn("YPLX_Name", Type.GetType("System.String"));
                DataColumn dc2 = new DataColumn("YPLX_EN", Type.GetType("System.String"));

                dt.Columns.Add(dc1);
                dt.Columns.Add(dc2);

                DataRow dr = dt.NewRow();
                dr["YPLX_Name"] = "全部";
                dr["YPLX_EN"] = "-1";
                dt.Rows.Add(dr);
                DataRow dr2 = dt.NewRow();
                dr2["YPLX_Name"] = "车油OEM";
                dr2["YPLX_EN"] = "Auto OEM";
                dt.Rows.Add(dr2);
                DataRow dr3 = dt.NewRow();
                dr3["YPLX_Name"] = "车油零售";
                dr3["YPLX_EN"] = "Auto Retail";
                dt.Rows.Add(dr3);
                DataRow dr4 = dt.NewRow();
                dr4["YPLX_Name"] = "工业油";
                dr4["YPLX_EN"] = "Industrial";
                dt.Rows.Add(dr4);
                DataRow dr5 = dt.NewRow();
                dr5["YPLX_Name"] = "特种油";
                dr5["YPLX_EN"] = "LUBRITECH";
                dt.Rows.Add(dr5);
                DataRow dr6 = dt.NewRow();
                dr6["YPLX_Name"] = "矿山油";
                dr6["YPLX_EN"] = "Mining";
                dt.Rows.Add(dr6);

                comb_YPLX.DataSource = dt.DefaultView;
                comb_YPLX.DisplayMember = "YPLX_Name";
                comb_YPLX.ValueMember = "YPLX_EN";
                if (user.DEPT != "-1")
                {
                    comb_YPLX.SelectedValue = selectYPLX(user.DEPT);
                    comb_YPLX.Enabled = false;

                }


            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message + "\r\n请联系管理员");
            }
        }
        public string selectYPLX(string str)
        {
            string selectValue = "-1";
            switch (str)
            {
                case "OEM":
                    selectValue = "Auto OEM";
                    break;
                case "IND":
                    selectValue = "Industrial";
                    break;
                case "RET":
                    selectValue = "Auto Retail";
                    break;
                case "FLT":
                    selectValue = "LUBRITECH";
                    break;
                case "MIN":
                    selectValue = "Mining";
                    break;


            }
            return selectValue;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text.Trim()))
            {
                MessageBoxEx.Show("请选择经销商");
                return;
            }
            if (string.IsNullOrEmpty(comboBox2.Text.Trim()))
            {
                MessageBoxEx.Show("请选择授权方");
                return;
            }
            if (string.IsNullOrEmpty(txt_PROJECT.Text.Trim()) && string.IsNullOrEmpty(txt_DQ.Text.Trim()))
            {
                MessageBoxEx.Show("请是输入项目或地区");
                return;
            }
            if (!string.IsNullOrEmpty(txt_PROJECT.Text.Trim()))
            {
                if (string.IsNullOrEmpty(txt_PROJECT_EN.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入项目英文名称");
                    return;
                }
            }
            if (!string.IsNullOrEmpty(txt_DQ.Text.Trim()))
            {
                if (string.IsNullOrEmpty(txt_DQ_EN.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入地区英文名称");
                    return;
                }
            }
            if (dateTimePicker1.Value > dateTimePicker2.Value)
            {
                MessageBoxEx.Show("生效日期不能大于失效日期");
                return;
            }
            if (checkBox1.Checked)
            {
                if (string.IsNullOrEmpty(txt_PROJECT.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入项目名称");
                }
                if (string.IsNullOrEmpty(txt_PROJECT_EN.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入项目英文名称");
                }
            }
            isDY = true;
            //判断是否打印过
            string sql = "select * from QX_AUZT_PRINT where CUST_NO=@CUST_NO and COMPANY=@COMPANY and START_DATE=@START_DATE and END_DATE=@END_DATE and isDelete=0 and PROJECT=@PROJECT and QYNAME=@QYNAME and productName=@productName and productName_EN=@productName_EN  ";

            SqlParameter[] parameters = {
                    new SqlParameter("@CUST_NO", comboBox1.SelectedValue),  //自定义参数  与参数类型    
                    new SqlParameter("@COMPANY",  comboBox2.Text),
                     new SqlParameter("@START_DATE", dateTimePicker1.Value.Date),
                     new SqlParameter("@END_DATE", dateTimePicker2.Value.Date),
                       new SqlParameter("@PROJECT", txt_PROJECT.Text.Trim()),
                         new SqlParameter("@QYNAME", txt_DQ.Text.Trim()),
                          new SqlParameter("@productName", txt_product.Text.Trim()),
                         new SqlParameter("@productName_EN", txt_product_EN.Text.Trim())
               
            };

            DataSet ds = DB.Query(sql, parameters);
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    MessageBoxEx.Show("授权书已打印，如需补打，请前往打印记录补打");
                    return;
                }
            }
            printDialog1.Document = printdoc;
            DialogResult dr = printDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                printdoc.EndPrint += new PrintEventHandler(printdoc_EndPrint);
                printdoc.BeginPrint += new PrintEventHandler(printdoc_BeginPrint);
                printdoc.Print();
            }

        }

        void printdoc_BeginPrint(object sender, PrintEventArgs e)
        {
            //获取授权书编号
            number = sqlhelper.GetNoByNo(com_DEPT.Text);
            isDY = true;
            printdoc.BeginPrint -= printdoc_BeginPrint;

        }

        void printdoc_EndPrint(object sender, EventArgs e)
        {
            try
            {

                //插入打印记录
                bool b = sqlhelper.AddPrintrecord(com_DEPT.Text.Trim(), number, comboBox1.SelectedValue.ToString(), comboBox1.Text, dr_EN["CUST_NAME_EN"].ToString(), comboBox2.Text, comboBox2.SelectedValue.ToString(), txt_PROJECT.Text, txt_PROJECT_EN.Text, dateTimePicker1.Value.Date, sqlhelper.DateTimeBystring_EN(dateTimePicker1.Value), dateTimePicker2.Value.Date, sqlhelper.DateTimeBystring_EN(dateTimePicker2.Value), ewm, user.NAME, txt_DQ.Text.Trim(), txt_DQ_EN.Text.Trim(), comb_YPLX.Text.Trim(), comb_YPLX.SelectedValue.ToString(), checkBox1.Checked, txt_product.Text.Trim(), txt_product_EN.Text.Trim());
                number = string.Empty;
                isDY = false;
                if (!b)
                {
                    MessageBoxEx.Show("添加打印记录失败，请联系管理员");
                }
                else
                {
                    MessageBoxEx.Show("打印成功");
                }
                printdoc.EndPrint -= printdoc_EndPrint;


            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);

            }


        }


        PrintDocument printdoc = new PrintDocument();


        PrintDocument printdoc2 = new PrintDocument();
        private void button2_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
            if (dateTimePicker1.Value > dateTimePicker2.Value)
            {
                MessageBoxEx.Show("生效日期不能大于失效日期");
                return;
            }
            if (string.IsNullOrEmpty(comboBox1.Text.Trim()))
            {
                MessageBoxEx.Show("请选择经销商");
                return;
            }
            if (string.IsNullOrEmpty(comboBox2.Text.Trim()))
            {
                MessageBoxEx.Show("请选择授权方");
                return;
            }
            if (string.IsNullOrEmpty(txt_PROJECT.Text.Trim()) && string.IsNullOrEmpty(txt_DQ.Text.Trim()))
            {
                MessageBoxEx.Show("请输入项目或地区");
                return;
            }
            if (!string.IsNullOrEmpty(txt_PROJECT.Text.Trim()))
            {
                if (string.IsNullOrEmpty(txt_PROJECT_EN.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入项目英文名称");
                    return;
                }
            }
            if (!string.IsNullOrEmpty(txt_DQ.Text.Trim()))
            {
                if (string.IsNullOrEmpty(txt_DQ_EN.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入地区英文名称");
                    return;
                }
            }
            if (checkBox1.Checked)
            {
                if (string.IsNullOrEmpty(txt_PROJECT.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入项目名称");
                }
                if (string.IsNullOrEmpty(txt_PROJECT_EN.Text.Trim()))
                {
                    MessageBoxEx.Show("请输入项目英文名称");
                }
            }

            printPreviewControl1.Document = printdoc;
        }

        private string ewm = string.Empty;
        /// <summary>
        /// 打印预览PrintPage1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintPage1(object sender, PrintPageEventArgs e)
        {
            bool result = false;
           
            foreach (System.Drawing.Printing.PaperSize pSize in printdoc.PrinterSettings.PaperSizes)
            {
                if (pSize.Kind == System.Drawing.Printing.PaperKind.A4)
                {
                    printdoc.DefaultPageSettings.PaperSize = pSize;
                    result = true;
                    break;
                }
            }
            if (!result)
            {
                MessageBoxEx.Show("当前打印机不支持该纸张类型");
                return;
            }



            lableFZ();
            //PrintHelper.SetProfileString(FuchsPrintingSystem.Printer2.ToString());
            //二维码链接
            string appUrl = AppConfig.GetValue("html5_url");
            ewm = appUrl + "?CUST_NO=" + comboBox1.SelectedValue;
            Graphics gp = e.Graphics;
            Font ft = new Font("微软雅黑", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            Font ft2 = new Font("微软雅黑", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            Rectangle rt = new Rectangle(0, 0, 500, 480);
            //  PaperSize pageSize = new PaperSize("First custom size", 800, 600);  
            //printdoc.DefaultPageSettings.PaperSize = pageSize;  

            if (!isDY)
            {
                Image bimg = pictureBox2.BackgroundImage;
                gp.DrawImage(bimg,
                             new Rectangle(0, 0, printdoc.DefaultPageSettings.PaperSize.Width,
                                           printdoc.DefaultPageSettings.PaperSize.Height));
            }
            Image qm_1 = sqlhelper.GetQm(com_DEPT.Text);
            if (isDY)
            {
                gp.DrawImage(qm_1, new Rectangle(140, 690, 200, 80));
            }
            else
            {
                gp.DrawImage(qm_1, new Rectangle(150, 760, 200, 80));
            }

            //Image qm_2 = sqlhelper.GetQmCEO(com_DEPT.Text);
            //if(isDY)
            //{
            //    gp.DrawImage(qm_2, new Rectangle(420, 770 - 70, 200, 80));
            //}else
            //{
            //    gp.DrawImage(qm_2, new Rectangle(420, 770, 200, 80));
            //}

            if (isDY)
            {
                Tools.SetQRCodeImage(ewm, printdoc.DefaultPageSettings.PaperSize.Width - 220 + 10, printdoc.DefaultPageSettings.PaperSize.Height - 175 - 50 - 10, 100, 100, gp);
            }
            else
            {
                Tools.SetQRCodeImage(ewm, printdoc.DefaultPageSettings.PaperSize.Width - 220, printdoc.DefaultPageSettings.PaperSize.Height - 175, 100, 100, gp);

            }



            if (isDY)
            {
                rt.Y = 400 - 55;
            }
            else
            {
                rt.Y = 400;
            }
            string str1 = label1.Text;
            //string str2 = label2.Text + label3.Text + label4.Text + label5.Text + label6.Text + label7.Text;
            //string str3 = label9.Text;
            string str4 = label8.Text;
            string str5 = number;

            string strall = str4;
            Graphics g = this.CreateGraphics();
            //标题居中代码
            #region 标题居中代码
            bool w = true;
            int titleWidth = 0;
            while (w)
            {

                SizeF siF = gp.MeasureString(str1, ft); // 两个参数，第一个是字符串内容，第二个是字符串的字体，我看你声明的是这两个
                titleWidth = Convert.ToInt32(siF.Width); // 这就是标题的宽度 
                if (titleWidth > rt.Width)
                {
                    ft = new Font("微软雅黑", ft.Size - 1, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                }
                else
                {
                    w = false;
                }

            }
            rt.X = (printdoc.DefaultPageSettings.PaperSize.Width - titleWidth) / 2;
            if (isDY)
            {
                rt.X = (printdoc.DefaultPageSettings.PaperSize.Width - titleWidth - 40) / 2;
            }
            #endregion
            gp.DrawString(str1, ft, Brushes.Black, rt);

            rt.X = 180;
            if (isDY)
            {
                rt.X = 140;
                rt.Y = 450 - 55;
            }
            else
            {
                rt.Y = 450;
            }
            PointF startPoint;
            if (isDY)
            {
                startPoint = new PointF(180, 450 - 55);
            }
            else
            {
                startPoint = new PointF(180, 450);
            }
            int y;

            DrawStringEx(strall, gp, startPoint, ft2, Brushes.Black, -2, out y);
            rt.Y = y;
            gp.DrawString(strall.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1], ft2, Brushes.Black, rt);


            //gp.DrawString(strall, ft2, Brushes.Black, rt);
            //rt.X = 200;
            //rt.Y = 460;
            //gp.DrawString(str3, ft2, Brushes.Blue, rt);
            //rt.X = 200;
            //rt.Y = 490;
            //gp.DrawString(str4, ft2, Brushes.Blue, rt);


            if (isDY)
            {
                rt.X = 190 - 10;
                rt.Y = 810 + 10;
            }
            else
            {
                rt.X = 190;
                rt.Y = 810;
            }
            gp.DrawString(str5, ft2, Brushes.Black, rt);

            gp.Dispose();
            GC.Collect();
            isDY = false;

            e.HasMorePages = false;




        }

        ///   <summary>
        ///   绘制任意间距文字
        /// </summary>
        ///   <param   name= "text "> 文本 </param>
        ///   <param   name= "g "> 绘图对象 </param>
        ///   <param   name= "startPoint "> 起始位置 </param>
        ///   <param   name= "font "> 字体 </param>
        ///   <param   name= "brush "> 画刷 </param>
        ///   <param   name= "sepDist "> 间距 </param>
        private void DrawStringEx(string text, Graphics g, PointF startPoint, Font font, Brush brush, float sepDist, out int y)
        {
            PointF pf = startPoint;
            SizeF charSize;
            text = text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            char[] ch = text.ToCharArray();
            int h = 0;
            foreach (char c in ch)
            {

                charSize = g.MeasureString(c.ToString(), font);
                h = Convert.ToInt32(charSize.Height);
                g.DrawString(c.ToString(), font, brush, pf);
                if (pf.X > 620)
                {
                    pf.X = 180;
                    if (isBd || isDY)
                    {
                        pf.X = 140;
                    }
                    pf.Y += Convert.ToInt32(charSize.Height + 1);
                }
                else
                {
                    pf.X += Convert.ToInt32(charSize.Width + sepDist);
                }

            }
            y = Convert.ToInt32(pf.Y) + h;
        }

        /// <summary>
        /// 打印预览PrintPage1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintPage2(object sender, PrintPageEventArgs e)
        {
            //PrintHelper.SetProfileString(FuchsPrintingSystem.Printer2.ToString());
            bool result = false;
            
            foreach (System.Drawing.Printing.PaperSize pSize in printdoc2.PrinterSettings.PaperSizes)
            {
                if (pSize.Kind == System.Drawing.Printing.PaperKind.A4)
                {
                    printdoc2.DefaultPageSettings.PaperSize = pSize;
                    result = true;
                    break;
                }
            }
            if (!result)
            {
                MessageBoxEx.Show("当前打印机不支持该纸张类型");
                return;
            }


            Graphics gp = e.Graphics;
            Font ft = new Font("微软雅黑",20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            Font ft2 = new Font("微软雅黑", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            Rectangle rt = new Rectangle(0, 0, 500, 480);
            //  PaperSize pageSize = new PaperSize("First custom size", 800, 600);  
            //printdoc.DefaultPageSettings.PaperSize = pageSize;  
            //二维码链接
            string appUrl = AppConfig.GetValue("html5_url");
            string ewm = appUrl + "?CUST_NO=" + CUST_NO;
            if (!isBd)
            {
                Image bimg = pictureBox2.BackgroundImage;
                gp.DrawImage(bimg,
                             new Rectangle(0, 0, printdoc2.DefaultPageSettings.PaperSize.Width,
                                           printdoc2.DefaultPageSettings.PaperSize.Height));
            }
            Image qm_1 = sqlhelper.GetQm(lbl_DEPT.Text);
            if (isBd)
            {
                gp.DrawImage(qm_1, new Rectangle(150 - 10, 760 - 70, 200, 80));
            }
            else
            {
                gp.DrawImage(qm_1, new Rectangle(150, 760, 200, 80));
            }
            //Image qm_2 = sqlhelper.GetQmCEO(lbl_DEPT.Text);
            //if (isBd)
            //{
            //    gp.DrawImage(qm_2, new Rectangle(420, 770 - 70, 200, 80));
            //}
            //else
            //{
            //    gp.DrawImage(qm_2, new Rectangle(420, 770, 200, 80));
            //}
            if (isBd)
            {
                Tools.SetQRCodeImage(ewm, printdoc.DefaultPageSettings.PaperSize.Width - 220 + 10, printdoc.DefaultPageSettings.PaperSize.Height - 175 - 50 - 10, 100, 100, gp);
            }
            else
            {
                Tools.SetQRCodeImage(ewm, printdoc.DefaultPageSettings.PaperSize.Width - 220, printdoc.DefaultPageSettings.PaperSize.Height - 175, 100, 100, gp);

            }


            if (isBd)
            {
                rt.Y = 400 - 55;
            }
            else
            {
                rt.Y = 400;
            }



            //string str2 = label2.Text + label3.Text + label4.Text + label5.Text + label6.Text + label7.Text;
            //string str3 = label9.Text;

            //标题居中代码
            #region 标题居中代码

            bool w = true;
            int titleWidth=0;
            while (w)
            {
              
                  SizeF siF = gp.MeasureString(biaoti, ft); // 两个参数，第一个是字符串内容，第二个是字符串的字体，我看你声明的是这两个
             titleWidth = Convert.ToInt32(siF.Width); // 这就是标题的宽度 
             if (titleWidth>rt.Width)
             {
                 ft = new  Font("微软雅黑",ft.Size-1, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
             }
             else
             {
                 w = false;
             }
               
            }
            rt.X = (printdoc.DefaultPageSettings.PaperSize.Width - titleWidth) / 2;
            if (isBd)
            {
                rt.X = (printdoc.DefaultPageSettings.PaperSize.Width - titleWidth - 40) / 2;
            }

            #endregion


            gp.DrawString(biaoti, ft, Brushes.Black, rt);
            rt.X = 180;
            if (isBd)
            {
                rt.X = 140;
                rt.Y = 450 - 55;
            }
            else
            {
                rt.Y = 450;
            }
            PointF startPoint;
            if (isBd)
            {
                startPoint = new PointF(180, 450 - 55);
            }
            else
            {
                startPoint = new PointF(180, 450);
            }
            int y;

            DrawStringEx(wenben, gp, startPoint, ft2, Brushes.Black, -2, out y);
            rt.Y = y;
            gp.DrawString(wenben.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1], ft2, Brushes.Black, rt);



            if (isBd)
            {
                rt.X = 190 - 10;
                rt.Y = 810 + 10;
            }
            else
            {
                rt.X = 205;
                rt.Y = 877;
            }
            gp.DrawString(sqsNo, ft2, Brushes.Black, rt);
            gp.Dispose();
            GC.Collect();
            isDY = false;
            e.HasMorePages = false;
        }
     


        public void lableFZ()
        {
            if (dr_EN != null)
            {
                DateTime date = dateTimePicker1.Value;
                string st = sqlhelper.DateTimeBystring_EN(date);
                string st2 = sqlhelper.DateTimeBystring_EN(dateTimePicker2.Value);

                label8.Text =
                    string.Format(
                        "      兹授权{0}为{1}{11}{13}在{10}{2}{12}授权经销商.此授权将从{4}起生效,有效期至{5}止.\r\n{3} is hereby authorized as the distributor of {17} Oil {14} {15} {9} {16} by {6} The authorization shall take effect from {7} and expire on {8} .",
                        comboBox1.Text.Trim(), comboBox2.Text.Trim(), string.IsNullOrEmpty(txt_PROJECT.Text.Trim()) ? "" : (txt_PROJECT.Text.Trim()), dr_EN["CUST_NAME_EN"].ToString(),
                        dateTimePicker1.Value.ToString("yyyy年MM月dd日"), dateTimePicker2.Value.ToString("yyyy年MM月dd日"),
                        comboBox2.SelectedValue.ToString().Trim(), st, st2, string.IsNullOrEmpty(txt_PROJECT_EN.Text.Trim()) ? "" : "for  " + txt_PROJECT_EN.Text.Trim(), txt_DQ.Text.Trim(), comb_YPLX.SelectedValue == "-1" ? "" : (comb_YPLX.Text.Trim()), checkBox1.Checked ? "的竞标工作" : "", txt_product.Text.Trim(), string.IsNullOrEmpty(txt_product_EN.Text.Trim()) ? "" : txt_product_EN.Text.Trim(),
                        string.IsNullOrEmpty(txt_DQ_EN.Text.Trim()) ? "" : "in " + txt_DQ_EN.Text.Trim(), checkBox1.Checked ? "Tender" : "", comb_YPLX.SelectedValue);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            string name_EN = comboBox2.SelectedValue.ToString().Trim();
            if (string.IsNullOrEmpty(name_EN))
            {
                this.errorProvider2.SetError(comboBox2, "该授权方没有英文名称，请先维护");
            }
            else
            {
                this.errorProvider2.Clear();
            }
            lableFZ();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                AppConfig.SetValue("bt_X", textBox1.Text);
                AppConfig.SetValue("bt_Y", textBox2.Text);
                AppConfig.SetValue("wb_Y", textBox3.Text);
                AppConfig.SetValue("wb_X", textBox4.Text);
                MessageBoxEx.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
            }

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

            lableFZ();
        }

        //private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        //{
        //    if(treeView1.SelectedNode.Index==0)
        //    {
        //        panel_jl.Visible = false;
        //        panel_bd.Visible = false;
        //        panel_dy.Visible = true;

        //    }
        //    else if (treeView1.SelectedNode.Index == 1)
        //    {
        //        panel_dy.Visible = false;
        //        panel_bd.Visible = false;
        //        panel_jl.Visible = true;
        //        BandindataGridView();
        //    }else if(treeView1.SelectedNode.Index==3)
        //    {
        //        panel_jl.Visible = false;
        //        panel_dy.Visible = false;
        //        panel_bd.Visible = true;


        //    }
        //}

        private void BandindataGridView_click()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(txt_path.Text.Trim()))
            {
                sb.AppendFormat(" and AUZT_NO like '%{0}%'", txt_path.Text.Trim());
            }
            if (rad_Y.Checked)
            {
                sb.AppendFormat(" and isUpload={0}", 1);
            }
            if (rad_n.Checked)
            {
                sb.AppendFormat(" and isUpload={0}", 0);
            }

            if (!string.IsNullOrEmpty(comboBox3.Text.Trim()))
            {
                sb.AppendFormat(" and CUST_NO = '{0}'", comboBox3.SelectedValue.ToString().Trim());
            }
            if (!string.IsNullOrEmpty(comboBox4.Text.Trim()))
            {
                sb.AppendFormat(" and COMPANY like '%{0}%'", comboBox4.Text.Trim());
            }
            sb.AppendFormat(" and START_DATE >='{0}' and END_DATE<='{1}'", dateTimePicker3.Value.Date, dateTimePicker4.Value.Date);
            sb.AppendFormat(" and OP_TIME >='{0}' and OP_TIME<='{1} 23:59:59'", dateTimePicker5.Value.Date, dateTimePicker6.Value.Date.ToString("yyyy/MM/dd"));

            //查询数据
            DataSet ds = sqlhelper.GetDYJlData(sb.ToString(), checkBox2.Checked ? 1 : 0);
            if (ds != null)
            {
                this.dataGridView1.DataSource = ds.Tables[0];
            }
        }

        private int id;
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {

                id = Convert.ToInt32(this.dataGridView1.SelectedRows[0].Cells["id"].Value);
                //根据id获取打印记录信息
                DataSet ds = sqlhelper.GetDYJlDataById(id);
                if (ds == null)
                {
                    MessageBoxEx.Show("获取打印记录数据失败");

                }
                else
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        sqsNo = dr["AUZT_NO"].ToString();
                        lbl_AUZT_NO.Text = sqsNo;
                        lbl_COMPANY.Text = dr["COMPANY"].ToString();
                        lbl_CUST_NAME.Text = dr["CUST_NAME"].ToString();
                        lbl_END_DATE.Text = sqlhelper.DateTimeBystring(Convert.ToDateTime(dr["END_DATE"]));
                        lbl_START_DATE.Text = sqlhelper.DateTimeBystring(Convert.ToDateTime(dr["START_DATE"]));
                        biaoti = dr["CUST_NAME"].ToString();
                        lbl_PROJECT.Text = dr["PROJECT"].ToString();
                        CUST_NO = dr["CUST_NO"].ToString();
                        lbl_DEPT.Text = dr["DEPT"].ToString();
                        lbl_QY.Text = dr["QYNAME"].ToString();
                        lbl_YPLB.Text = dr["YPLB"].ToString();
                        lbl_isJB.Text = Convert.ToBoolean(dr["isJB"]) ? "是" : "否";
                        lbl_productName.Text = string.IsNullOrEmpty(dr["productName"].ToString()) ? "无" : dr["productName"].ToString();
                        wenben = string.Format("      兹授权{0}为{1}{11}{13}在{10}{2}{12}授权经销商。此授权将从{4}起生效，有效期至{5}止。\r\n{6} is hereby authorized as the distributor of {17} Oil {14} {15} {9} {16} by {3}The authorization shall take effect from {7} and expire on {8} .",
                            dr["CUST_NAME"].ToString().Trim(), dr["COMPANY"].ToString().Trim(),
                            string.IsNullOrEmpty(dr["PROJECT"].ToString().Trim()) ? "" : (dr["PROJECT"].ToString().Trim()), dr["COMPANY_EN"].ToString().Trim(), lbl_START_DATE.Text.Trim(), lbl_END_DATE.Text.Trim(), dr["CUST_NAME_EN"].ToString().Trim(), dr["START_DATE_EN"].ToString().Trim(), dr["END_DATE_EN"].ToString().Trim(), string.IsNullOrEmpty(dr["PROJECT_EN"].ToString()) ? "" : "for  " + dr["PROJECT_EN"].ToString().Trim(),
                            dr["QYNAME"],
                            string.IsNullOrEmpty(dr["YPLB"].ToString().Trim()) ? "" : (dr["YPLB"]),
                            Convert.ToBoolean(dr["isJB"]) ? "的竞标工作" : "",
                            dr["productName"].ToString().Trim(), string.IsNullOrEmpty(dr["productName_EN"].ToString().Trim()) ? "" : dr["productName_EN"].ToString().Trim(),
                            string.IsNullOrEmpty(dr["QY_EN"].ToString().Trim()) ? "" : "in " + dr["QY_EN"].ToString().Trim(), Convert.ToBoolean(dr["isJB"].ToString().Trim()) ? "Tender" : "", dr["YPLB_EN"]);

                        button5_Click(sender, e);
                        //treeView1.SelectedNode = treeView1.Nodes[3];
                        //treeView1.Nodes[0].Checked = true;
                        this.skinTabPage8.Parent = this.skinTabControl2;
                        this.skinTabControl2.SelectedIndex = 2;
                    }
                }

            }
            catch (Exception)
            {
                MessageBoxEx.Show("请选中行");
            }

        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex == -1) return;
                dataGridView1.Rows[e.RowIndex].Selected = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            panel1.BackColor = Color.White;
           
            printPreviewControl2.Document = printdoc2;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            isBd = true;
            printDialog1.Document = printdoc2;
            DialogResult dr = printDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                printdoc2.EndPrint += new PrintEventHandler(printdoc_EndPrint2);
                printdoc2.Print();
            }
        }
        void printdoc_EndPrint2(object sender, EventArgs e)
        {
            try
            {
                isBd = false;
                bool b = sqlhelper.AddPrintrecord_Add(id, user.NAME);
                //插入补打记录
                if (!b)
                {
                    MessageBoxEx.Show("添加补打记录失败，请联系管理员");
                }
                else
                {
                    MessageBoxEx.Show("补打成功");
                    printdoc2.EndPrint -= printdoc_EndPrint2;
                }



            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);

            }

        }





        private void 授权商维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main_SQS sqs = new Main_SQS();
            sqs.Refreshtheform = getCUSTOMER;
            sqs.ShowDialog();
        }

        private void 授权方维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main_SQF sqs = new Main_SQF();
            sqs.Refreshtheform = getCOMPANY;
            sqs.ShowDialog();
        }


        private void 上传授权书图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                id = Convert.ToInt32(this.dataGridView1.SelectedRows[0].Cells["id"].Value);
                ImagUpload upload = new ImagUpload(id);
                upload.Refreshtheform = BandindataGridView_click;//将方法赋给委托对象  
                upload.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBoxEx.Show("请选中行");
            }

        }
        /// <summary>
        /// 搜索打印记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            if (dateTimePicker3.Value > dateTimePicker4.Value)
            {
                MessageBoxEx.Show("生效日期不能大于失效日期");
                return;
            }
            if (dateTimePicker5.Value > dateTimePicker6.Value)
            {
                MessageBoxEx.Show("打印时间范围选择不正确");
                return;
            }
            BandindataGridView_click();
        }

        private void button13_Click(object sender, EventArgs e)
        {

            if (dataGridView1.Rows.Count > 0)
            {
                string fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + DateTime.Now.Millisecond;
                string saveFileName = "";
                //bool fileSaved = false;  

                saveDialog.DefaultExt = "xls";
                saveDialog.Filter = "Excel文件|*.xls";
                saveDialog.FileName = fileName;
                saveDialog.ShowDialog();
                saveFileName = saveDialog.FileName;
                if (saveFileName.IndexOf(":") < 0) return; //被点了取消   
                notifyForm.StartPosition = FormStartPosition.CenterParent;

                bkWorker.RunWorkerAsync();
                notifyForm.ShowDialog(this);
            }
            else
            {
                MessageBoxEx.Show("报表为空,无表格需要导出");
            }
            //sqlhelper.ExportExcel("123",dataGridView1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {

                AppConfig.SetValue("wb_Y_bd", textBox5.Text);
                AppConfig.SetValue("wb_X_bd", textBox6.Text);
                AppConfig.SetValue("bt_Y_bd", textBox7.Text);
                AppConfig.SetValue("bt_X_bd", textBox8.Text);
                MessageBoxEx.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
            }

        }

        private void textBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //updateview();
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                try
                {
                    if (this.dataGridView1.Rows[i].Cells["是否上传图片"].Value.ToString() == "未上传")
                    {
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Tomato;
                    }
                    else
                    {
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                    }

                }
                catch (Exception ex)
                {
                    MessageBoxEx.Show(ex.Message);
                }
            }
        }

        private void 监控维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main_JK jk = new Main_JK();
            //sqs.Refreshtheform = getCOMPANY;
            jk.ShowDialog();
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {

            //     System.Drawing.Drawing2D.GraphicsPath buttonPath =
            //new System.Drawing.Drawing2D.GraphicsPath();
            //     Point[] pons = new Point[]
            //                        {
            //                            new Point(0,40),
            //                            new Point(712,40),
            //                            new Point(712,  12),
            //                             new Point(712+175,12),
            //                             new Point(712+175,40),
            //                             new Point(997, 40),
            //                             new Point(997, 736),
            //                             new Point(0, 736)

            //                        };


            //     buttonPath.AddPolygon(pons);

            //     this.Region = new System.Drawing.Region(buttonPath);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                id = Convert.ToInt32(this.dataGridView1.SelectedRows[0].Cells["id"].Value);

                //修改打印记录状态
                int i = sqlhelper.DeleteDYJL(id, 1);
                if (i != 1)
                {
                    MessageBoxEx.Show("删除失败");
                }
                else
                {
                    MessageBoxEx.Show("删除成功");
                    BandindataGridView_click();
                }


            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
            }

        }

        private void 还原ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                id = Convert.ToInt32(this.dataGridView1.SelectedRows[0].Cells["id"].Value);

                //修改打印记录状态
                int i = sqlhelper.DeleteDYJL(id, 0);
                if (i != 1)
                {
                    MessageBoxEx.Show("还原失败");
                }
                else
                {
                    MessageBoxEx.Show("还原成功");
                    BandindataGridView_click();
                }


            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count <= 0)
            {
                return;
            }
            if (user.DEPT != "-1")
            {
                删除ToolStripMenuItem2.Visible = false;
                还原ToolStripMenuItem2.Visible = false;
            }
            else
            {
                string str = this.dataGridView1.SelectedRows[0].Cells["状态"].Value.ToString();
                if (str == "正常")
                {
                    还原ToolStripMenuItem2.Visible = false;
                    删除ToolStripMenuItem2.Visible = true;
                }
                else
                {
                    还原ToolStripMenuItem2.Visible = true;
                    删除ToolStripMenuItem2.Visible = false;
                }
            }



        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            dr_EN = (DataRowView)comboBox1.Items[comboBox1.SelectedIndex];
            string name_EN = dr_EN["CUST_NAME_EN"].ToString().Trim();
            if (string.IsNullOrEmpty(name_EN))
            {
                this.errorProvider1.SetError(comboBox1, "该经销商没有英文名称，请先维护");
            }
            else
            {
                this.errorProvider1.Clear();
            }
            label1.Text = comboBox1.Text;

            lableFZ();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            try
            {
                zoom = zoom + 0.1;
                if (zoom > 5.0)
                {
                    zoom = 5.0;
                }
                this.printPreviewControl1.Zoom = zoom;
            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private double zoom = 0.4;
        private double zoom2 = 0.4;
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            try
            {
                zoom = zoom - 0.1;
                if (zoom < 0.1)
                {
                    zoom = 0.1;
                }
                this.printPreviewControl1.Zoom = zoom;
            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            try
            {
                this.printPreviewControl1.AutoZoom = true;
                zoom = 0.4;
            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            try
            {
                zoom2 = zoom2 + 0.1;
                if (zoom2 > 5.0)
                {
                    zoom2 = 5.0;
                }
                this.printPreviewControl2.Zoom = zoom2;
            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            try
            {
                zoom2 = zoom2 - 0.1;
                if (zoom2 < 0.1)
                {
                    zoom2 = 0.1;
                }
                this.printPreviewControl2.Zoom = zoom2;
            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {

            try
            {
                this.printPreviewControl2.AutoZoom = true;
                zoom2 = 0.4;
            }
            catch (Exception excep)
            {
                MessageBoxEx.Show(excep.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if (DialogResult.Yes == MessageBoxEx.Show("数据较大同步时间较长。\r\n确认同步经销商数据？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                notifyForm.StartPosition = FormStartPosition.CenterParent;

                bkWorker2.RunWorkerAsync();
                notifyForm.ShowDialog(this);

            }
        }

        private void Main_SysBottomClick(object sender, CCWin.SkinControl.SysButtonEventArgs e)
        {
            //如果点击的是工具菜单按钮
            if (e.SysButton.Name == "pictureBox4")
            {
                Point l = PointToScreen(e.SysButton.Location);
                l.Y += e.SysButton.Size.Height + 1;
                MobileMenu.Show(l);

            }
        }

        private void skinTabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage.Name != "skinTabPage8")
            {
                skinTabPage8.Parent = null;
            }
        }
        //存放要显示的信息
        List<string> messages;
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
          
                if (MessageBoxEx.Show("是否关闭窗口", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;

                }
                else
                {
                    System.Environment.Exit(0);
                }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //滚动显示
            index = (index + 1) % messages.Count;
            lbl_tgText.Text = messages[index];
        }

      

    

























    }
}
