using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Data.SqlClient;
using System.IO.Ports;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis; //用于生成响应的事件
using System.Speech;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using Automation.BDaq;
using TotalProduceLineSys.Common;
using Qixuan.Common;
using TotalProduceLineSys.ServiceReference2;
using TotalProduceLineSys.Entity;
using System.Collections;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using StackExchange.Redis;
using System.Collections.Concurrent;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Cognex.DataMan.SDK.Utils;
using Qixuan.Common.WebClient;

namespace TotalProduceLineSys
{
    public partial class MainForm_1L4L_ZJ : Form
    {
        public BindingData bindingdata;
        private RedisHelp redisHelper_ocr1;
        private readonly object _lockObj = new object();
        private Queue<string> CheckBarcodeQueue = new Queue<string>();
        private Queue<OCR1ViewDataClass> _ocr1Queue = new Queue<OCR1ViewDataClass>();
        private Queue<BoxAndItemBundle> _ocr1Queue_boxItem = new Queue<BoxAndItemBundle>();

        private Queue<OCR1ViewDataClass> _ocr1Queue_remove = new Queue<OCR1ViewDataClass>();
        private Queue<BoxAndItemBundle> _ocr1Queue_remove_boxItem = new Queue<BoxAndItemBundle>();

        //    private JJIOCard ioj;
        public MainForm_1L4L_ZJ()
        {
            InitializeComponent();
            bindingdata = BindingData.getBindingData;

            view_tong.DataSource = bindingdata.OCR1List;
            view_ocr1.DataSource = bindingdata.OCR1List;
            view_box2.DataSource = bindingdata.OCR3List;
            view_ch1.DataSource = bindingdata.CH1_List;
           
            view_ch2.DataSource = bindingdata.CH2_List;
            view_ch3.DataSource = bindingdata.CH3_List;
            view_ch4.DataSource = bindingdata.CH4_List;
            view_ch5.DataSource = bindingdata.CH5_List;
            view_ch6.DataSource = bindingdata.CH6_List;

            

            view_makedBox.DataSource = bindingdata.MakedBox_List;
            bindingdata.Box_Item_List.ListChanged += Box_Item_List_ListChanged;
            bindingdata.Box_Item_List.BeforeRemove += Box_Item_List_BeforeRemove;
            bindingdata.OCR1List.ListChanged += OCR1List_ListChanged;
            bindingdata.OCR1List.BeforeRemove +=OCR1List_BeforeRemove;
            bindingdata.OCR3List.ListChanged += OCR3List_ListChanged;
            bindingdata.OCR3List.BeforeRemove += OCR1List_BeforeRemove;
            bindingdata.CH1_List.ListChanged += CH1_List_ListChanged;
            bindingdata.CH1_List.BeforeRemove += OCR1List_BeforeRemove;
            bindingdata.CH2_List.ListChanged += CH2_List_ListChanged;
            bindingdata.CH2_List.BeforeRemove += OCR1List_BeforeRemove;
            bindingdata.CH3_List.ListChanged += CH3_List_ListChanged;
            bindingdata.CH3_List.BeforeRemove += OCR1List_BeforeRemove;
            bindingdata.CH4_List.ListChanged += CH4_List_ListChanged;
            bindingdata.CH4_List.BeforeRemove += OCR1List_BeforeRemove;
            bindingdata.CH5_List.ListChanged += CH5_List_ListChanged;
            bindingdata.CH5_List.BeforeRemove += OCR1List_BeforeRemove;
            bindingdata.CH6_List.ListChanged += CH6_List_ListChanged;
            bindingdata.CH6_List.BeforeRemove += OCR1List_BeforeRemove;
            this.redisHelper_ocr1 = new RedisHelp();

        }

        private async void Box_Item_List_BeforeRemove(BoxAndItemBundle deletedItem)
        {
             _ocr1Queue_remove_boxItem.Enqueue(deletedItem);
            waitRedisRemove_boxitem.Set();
        }

        private void Box_Item_List_ListChanged(object sender, ListChangedEventArgs e)
        {

            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.Box_Item_List[e.NewIndex];
                    _ocr1Queue_boxItem.Enqueue(ocrData);
                    waitRedis_boxitem.Set();
                }
            }
        }

        /// <summary>
        /// 列表删除事件
        /// </summary>
        /// <param name="deletedItem"></param>
        /// <exception cref="NotImplementedException"></exception>
        private  void OCR1List_BeforeRemove(OCR1ViewDataClass deletedItem)
        {
            Task.Run(() =>
            {
                _ocr1Queue_remove.Enqueue(new OCR1ViewDataClass {Code=deletedItem.Code,Allcode=deletedItem.Allcode,Name=deletedItem.Name } );
                waitRedisRemove.Set();
            });
         
        }

        private void CH1_List_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 数据变更时触发事件
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.CH1_List[e.NewIndex];
                    ocrData.Name = "CH1List";
                    _ocr1Queue.Enqueue(ocrData);
                  
                    waitRedis.Set();
                }
            }
            this.Invoke(new Action(() =>
            {

                ch1count1.Text = ch1count2.Text= bindingdata.CH1_List.Count.ToString();
              
            }));
        }
        private void CH2_List_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 数据变更时触发事件
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.CH2_List[e.NewIndex];
                    ocrData.Name = "CH2List";
                    _ocr1Queue.Enqueue(ocrData);
                  
                    waitRedis.Set();
                }
            }
            this.Invoke(new Action(() =>
            {
                ch2count1.Text = ch2count2.Text=bindingdata.CH2_List.Count.ToString();
            }));
        }
        private void CH6_List_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 数据变更时触发事件
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.CH6_List[e.NewIndex];
                    ocrData.Name = "CH6List";
                    _ocr1Queue.Enqueue(ocrData);
                 
                    waitRedis.Set();
                }
            }
            ch6count1.Text = ch6count2.Text = bindingdata.CH6_List.Count.ToString();
        }
        private void CH3_List_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 数据变更时触发事件
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.CH3_List[e.NewIndex];
                    ocrData.Name = "CH3List";
                    _ocr1Queue.Enqueue(ocrData);
                 

                    waitRedis.Set();
                }
            }
            ch3count1.Text = ch3count2.Text = bindingdata.CH3_List.Count.ToString();
        }
        private void CH4_List_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.CH4_List[e.NewIndex];
                    ocrData.Name = "CH4List";
                    _ocr1Queue.Enqueue(ocrData);
                   

                    waitRedis.Set();
                }
            }
            ch4count1.Text = ch4count2.Text = bindingdata.CH4_List.Count.ToString();
        }
        private async void CH5_List_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.CH5_List[e.NewIndex];
                    ocrData.Name = "CH5List";
                    _ocr1Queue.Enqueue(ocrData);
                  
                    waitRedis.Set();
                }
            }
            ch5count1.Text = ch5count2.Text = bindingdata.CH5_List.Count.ToString();
        }
        private async void OCR1List_ListChanged(object sender, ListChangedEventArgs e)
        {



            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.OCR1List[e.NewIndex];
                    ocrData.Name = "OCR1List";
                    _ocr1Queue.Enqueue(ocrData);
                  
                    waitRedis.Set();
                }
            }

        }


        private async void OCR3List_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                if (e != null)
                {
                    var ocrData = bindingdata.OCR3List[e.NewIndex];
                    ocrData.Name = "OCR3List";
                    _ocr1Queue.Enqueue(ocrData);
                 
                    waitRedis.Set();
                }
            }

        }
        ManualResetEvent waitUploadData = new ManualResetEvent(true);//上传等待信号
        ManualResetEvent waitHandler_PLCFD = new ManualResetEvent(false);//PLC分道线程等待信号
        ManualResetEvent waitHandler_MakeBox = new ManualResetEvent(false);//组箱线程等待信号
        ManualResetEvent waitHandler_MakeBox_fd = new ManualResetEvent(false);//提取分道数据等待新号
        ManualResetEvent waitRedis = new ManualResetEvent(true);//缓存等待信号
        ManualResetEvent waitRedis_boxitem = new ManualResetEvent(true);//缓存等待信号
        ManualResetEvent waitRedisRemove = new ManualResetEvent(true);//缓存等待信号
        ManualResetEvent waitRedisRemove_boxitem = new ManualResetEvent(true);//缓存等待信号
    
        ManualResetEvent wait_pallet = new ManualResetEvent(true);//ocr3组托线程等待信号
        ThreadStart myThreaddelegate_channel = null;//分道线程
        Thread m_thd_MakeBox = null;//组箱线程
        Thread worker_UploadData = null;//数据上传线程
        Thread RedisTh = null;//缓存线程
        Thread RedisTh_boxitem = null;//缓存线程
        
        Thread RedisThRemove = null;//缓存线程
        Thread RedisThRemove_boxitem = null;//缓存线程
       
        private string last_barcode = string.Empty;//上一个正确条码 判断是否重复标志
        private string lastOCR2Barcode = string.Empty;//上一个正确条码 判断是否重复标志
        private string lastOCR3Barcode = string.Empty;//上一个正确条码 判断是否重复标志
        static AutoResetEvent makeBox_ResetEvent = new AutoResetEvent(false); //组箱事件通知
        static AutoResetEvent checkPallet_ResetEvent = new AutoResetEvent(false);//检测组托事件通知

        bool bottleWithFWCode = false;//瓶身是否带防伪码 
        bool isBottle = false;//是否开启瓶子剔除功能
        bool isBox = false;//是否开启箱子剔除功能
        bool isDeleteBox = false;//是否开启删除箱子
        bool isOCRCheck = false;//是否开启OCR校验
        int removedqty = 0;//剔除瓶子计数，本地qx_ocr3_check表中添加该字段
        int makeBoxAmount = 0;//一次组箱数量
        private int makeBoxNumbers = 1;//组箱顺序， 第一道 1 2 3 ；第二道 4 5 6；或是 第一道 1 2 ；第二道 3 4 或是 第一道 1 ；第二道 2；
        private string lastBarcode = string.Empty;//上一个正确瓶码条码 判断是否重复标志
        #region //事件通知
        static AutoResetEvent saveCache_ResetEvent = new AutoResetEvent(false); //保存零头事件通知
        static AutoResetEvent forceMakePallet_ResetEvent = new AutoResetEvent(false); //强制组拖事件通知
        #endregion
        static object lock_ocr2_list = new object();
        static object lock_ch1_list = new object();
        static object lock_ch2_list = new object();
        static object lock_ch3_list = new object();
        static object lock_ch4_list = new object();
        static object lock_ch5_list = new object();
        static object lock_ch6_list = new object();
        static object lock_boxReadTime_list = new object();
        private bool uploadDataFinished = true;//是否上传数据完成
        progressBarShow progressBar;//进度条
        int channelErrorFirePassTime = 0;//分道误触发过滤时间 毫秒

        #region 箱码误触发相关
        int box_barcode_fire_pass_time = 0;//箱码误触发过滤时间毫
        List<BoxReadTime> List_BoxReadTime = new List<BoxReadTime>(); //存储读码的箱码和读码时间 作为后续 12noread3 误触发判断过滤依据
        #endregion 
        List<string> List_ItemBarcode = new List<string>();//ocr1读取到的瓶码队列
        int productChCounts = 100;//分道数量上限，超出数量则报警

        #region
        List<Channel> List_channel = new List<Channel>(); //存储所有分道数量
        int OCR2_electric_eye_fired_amount = 0; //ocr2电眼触发总次数
        int OCR2_read_code_amount = 0; //ocr2读码总次数

        int OCR3_electric_eye_fired_amount = 0; //ocr3电眼触发总次数
        int OCR3_read_code_amount = 0; //ocr3读码总次数
        int sendPLCSignalDelay = 0; //发送给plc信号延时时间，单位秒。这之后再发送其他信号
        int ocr3_counts = 0;//用于瓶箱关联。
        #endregion

        #region 看板相关
        bool enableboard = false;//是否启用看板功能
        BoardTool board;
        int outPut = 0;
        #endregion

        #region 统计ocr1、ocr2、ocr3读码率
        int OCR1ReadAmount = 0;
        int OCR1NoReadAmount = 0;
        int OCR2ReadAmount = 0;
        int OCR2NoReadAmount = 0;
        int OCR3ReadAmount = 0;
        int OCR3NoReadAmount = 0;
        #endregion

        bool isPQMSSku = false;//是否为PQMS产品
        bool isDPCA = false;//是否为DPCA产品，注：100%读码率的产品，需要发短信通知
        #region 校验变量
        int check_no = 0;//校验索引，用于组托
        string ocr_check_box_barcode = string.Empty;//校验箱码
        string act_box_barcode = string.Empty;//替换箱码
        string remark_check = string.Empty;//组托异常原因
        string ZJ_LN = string.Empty;
        string TJ_LN = string.Empty;
        string GZ_LN = string.Empty;

        #endregion
        #region OCR断连时间变量
        int Disconnection_time;  //OCR断连时间
        #endregion
        //退出程序
        private void btn_Exit_Click(object sender, EventArgs e)
        {
            if (Common.Common.TS("确定停止生产并且退出系统？"))
            {
                AddInfoLog("Exit system", "TopRight button, User=" + m_set.UserName);
                System.Environment.Exit(0);
            }
        }

        bool isProducing = false;//是否正在生产
        SqlConnection m_conn = null;

        int execute_sql(string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, m_conn))
            {
                try
                {
                    cmd.CommandTimeout = 7200;
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    AddErrorLog(true, "sql_executesql_exception", "sql_executesql_exception=" + e.Message, "sql_executesql_exception", "sql_executesql_exception=" + e.Message + "," + e.StackTrace);
                    return 0;
                }
            }
        }



        string o3 = Common.Config.ReadValue("Port", "port3");
        string o3_3 = Common.Config.ReadValue("Port", "fw3");
        int s = 1;//解除报警查询次数
        /// <summary>
        /// 系统时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lb_timer.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch { }
        }

        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Sys_Click(object sender, EventArgs e)
        {
            FormSysConfig fs = new FormSysConfig();
            fs.ShowDialog();
            AddInfoLog("系统配置", "User=" + m_set.UserName);
        }

        /// <summary>
        /// 进度条显示
        /// </summary>
        private void SetProgress(object obj)
        {
            if (progressBar.InvokeRequired)
            {
                myDelegate3 _myinvoke = new myDelegate3(SetProgress);
                progressBar.Invoke(_myinvoke, new object[] { });
            }
            else
            {
                progressBar.Show();
                int percentage = 0;
                while (true)
                {
                    if (uploadDataFinished)
                    {
                        percentage = 100;
                        progressBar.Refresh();
                        progressBar.Close();
                        return;
                    }
                    else
                    {
                        if (percentage <= 90)
                        {
                            percentage += 10;
                            progressBar.Refresh();
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }

        void SendMail()
        {
            #region
            string subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "停止生产";
            int allProduceAmount = Common.Common.AllProduceAmount;
            int noReadAmount = Common.Common.NoReadAmount;
            string repeatCodeAmount = Common.Common.RepeatCodeAmount.ToString();
            string readCodeRate = allProduceAmount == 0 ? "0%" : (100 * (Convert.ToDecimal(allProduceAmount - noReadAmount) / Convert.ToDecimal(allProduceAmount)) + "%");
            string content = "停止生产时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "，单号:" + tDocno.Text + ", 信息=" + tDocInfo.Text;
            content += "本次生产总数：" + allProduceAmount.ToString() + "瓶桶";
            content += "NoRead：" + noReadAmount.ToString() + "次";
            content += "重复读码：" + repeatCodeAmount.ToString();
            content += "读码成功率：" + readCodeRate;
            Common.Common.SendMail_Thread(subject, content);
            #endregion
        }

        #region 关闭OCR连接
        /// <summary>
        /// 关闭连接通用版 by 徐元丰 2021.02.07
        /// </summary>
        public void CloseOCRorPLC(Thread thread, Socket socket, string ocrName, GroupBox groupBox, string groupBoxMessage, string errorMessage1, string errorMessage2)
        {
            try
            {
                if (thread != null)
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }

                if (groupBox != null)
                {
                    groupBox.Text = groupBoxMessage;
                    groupBox.ForeColor = Color.Red;
                }

                if (socket != null)//2020.12.16添加判断是否为null by 徐元丰
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, errorMessage1, ex.Message, errorMessage2, ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 关闭连接通用版 修改版 by 雷子华 
        /// </summary>
        public void CloseOCRorPLC1(Thread thread, Socket socket, string ocrName, string groupBoxMessage, string errorMessage1, string errorMessage2)
        {
            try
            {
                if (thread != null)
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }

                //if (groupBox != null)
                //{
                //    groupBox.Text = groupBoxMessage;
                //    groupBox.ForeColor = Color.Red;
                //}

                if (socket != null)//2020.12.16添加判断是否为null by 徐元丰
                {
                    //socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, errorMessage1, ex.Message, errorMessage2, ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 关闭PLC分道通道
        /// </summary>
        void ClosePLC()
        {
            try
            {
                if (myThread_PLC != null)
                {
                    if (myThread_PLC.IsAlive)
                    {
                        myThread_PLC.Abort();
                        group_PLC.Text = "分道PLC-未启动";
                        group_PLC.ForeColor = Color.Red;
                        lbl_channel1.ForeColor = Color.Blue;
                        lbl_channel2.ForeColor = Color.Blue;
                        lbl_channel3.ForeColor = Color.Blue;
                        lbl_channel4.ForeColor = Color.Blue;
                        lbl_channel5.ForeColor = Color.Blue;
                        lbl_channel6.ForeColor = Color.Blue;
                        lbl_channelAmount.ForeColor = Color.Blue;
                    }
                }
                if (socket_PLC != null)
                {
                    socket_PLC.Shutdown(SocketShutdown.Both);
                    socket_PLC.Close();
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止生产-停止分道PLC检查线程-异常", ex.Message, "Stop Line Exception-Check Channel PLC", ex.Message + " " + ex.StackTrace);
            }
        }

        public void CloseThread(Thread thread, string errorMessage1, string errorMessage2)
        {
            try
            {
                if (thread != null)
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, errorMessage1, ex.Message, errorMessage2, ex.Message + " " + ex.StackTrace);
            }
        }
        #endregion

        public void Stop(ManualResetEvent waitHandler, Thread worker)
        {
            try
            {
                isClosing = true;
                waitHandler.Set();
                Thread.Sleep(100);
                if (worker != null)
                {
                    worker.Join(1000);
                    if (worker.ThreadState != ThreadState.Stopped)
                        worker.Abort();
                    worker = null;
                }

            }
            catch (Exception ex)
            {

            }
        }

        void StopProduce()
        {
            AddInfoLog("停止生产", "User=" + m_set.UserName);
            m_set.OrderNo = string.Empty;
            tDocno.Text = "";

            #region 分道数量调整
            ShowChannel();//显示分道控件 by 2021.8.27 徐元丰

            #endregion



            box_List_delete_copy = new List<string>(box_List_delete.ToArray());
            lock (_lockObj)
            {
                bindingdata.OCR1List.Clear();
            }
            bindingdata.CH1_List.Clear();
            bindingdata.CH2_List.Clear();
            bindingdata.CH3_List.Clear();
            bindingdata.CH4_List.Clear();
            bindingdata.CH5_List.Clear();
            bindingdata.CH6_List.Clear();

            box_List_delete.Clear();
            bindingdata.OCR2List.Clear();
            bindingdata.OCR3List.Clear();
            OCR7_List.Clear();
            MakedBox_List.Clear();
            List_ItemBarcode.Clear();
            bindingdata.Box_Item_List.Clear();

            //view_tong.Rows.Clear();
            //view_ocr1.Rows.Clear();

            view_box2.Rows.Clear();
            view_makedBox.Rows.Clear();
            view_box6.Rows.Clear();

            bindingdata.CH1_List.Clear();
            bindingdata.CH2_List.Clear();
            bindingdata.CH3_List.Clear();
            bindingdata.CH4_List.Clear();
            bindingdata.CH5_List.Clear();
            bindingdata.CH6_List.Clear();
            //List_PLC.Clear();//清空PLC分道数据
            bindingdata.List_PLC.Clear();
            view_ch1.Rows.Clear();
            view_ch2.Rows.Clear();
            view_ch3.Rows.Clear();
            view_ch4.Rows.Clear();
            view_ch5.Rows.Clear();
            view_ch6.Rows.Clear();

            ch1count1.Text = "0";
            ch1count2.Text = "0";
            ch2count1.Text = "0";
            ch2count2.Text = "0";
            ch3count1.Text = "0";
            ch3count2.Text = "0";
            ch4count1.Text = "0";
            ch4count2.Text = "0";
            ch5count1.Text = "0";
            ch5count2.Text = "0";
            ch6count1.Text = "0";
            ch6count2.Text = "0";
            ch1sumcount1.Text = "0";
            ch2sumcount1.Text = "0";
            ch3sumcount1.Text = "0";
            ch4sumcount1.Text = "0";
            ch5sumcount1.Text = "0";
            ch6sumcount1.Text = "0";

            try
            {
                if (myThread_OcrHeartBeat.IsAlive)
                {
                    myThread_OcrHeartBeat.Abort();
                }

                Stop(waitUploadData, worker_UploadData);//结束数据上传线程
                //结束分道线程
                Stop(waitHandler_PLCFD, m_thd_channel);
                //结束组箱线程 将缓存组箱数据存入数据库
                Stop(waitHandler_MakeBox, m_thd_MakeBox);
                //结束组箱线程 将数据存入缓存队列
                Stop(waitHandler_MakeBox_fd, m_thd_checkMakeBox);
                //结束redis缓存线程
                Stop(waitRedis, RedisTh);
                //结束redis缓存删除数据线程
                Stop(waitRedisRemove, RedisThRemove);
                //结束redis缓存线程
                Stop(waitRedis_boxitem, RedisTh_boxitem);
                //结束redis缓存删除数据线程
                Stop(waitRedisRemove_boxitem, RedisThRemove_boxitem);
                //结束组托线程
                Stop(wait_pallet, m_thd_checkpallet);


            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止生产-停止ocr心跳包检查线程-异常", ex.Message, "Stop Line Exception-ocr heart beat check", ex.Message + " " + ex.StackTrace);
            }
            ////优化代码by 徐元丰 2021.02.07
            //关闭OCR1
            CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
            //关闭OCR2
            CloseOCRorPLC(myThread2, ss2, "OCR2", null, "箱码OCR2-未启动", "停止生产-OCR2异常", "Stop Line Exception-OCR2");
            //关闭OCR3
            CloseOCRorPLC(myThread3, ss3, "OCR3", group_Ocr3, "箱码OCR3-未启动", "停止生产-OCR3异常", "Stop Line Exception-OCR3");
            //关闭OCR7 根据实际情况填写message信息
            CloseOCRorPLC(myThread7, ss7, "OCR7", group_Ocr7, "托盘OCR4-未启动", "停止生产-OCR4异常", "Stop Line Exception-OCR4");
            //关闭OCR2_Counter
            CloseOCRorPLC(myThread2_Counter, ss2_counter, "OCR2_Counter", null, "", "停止生产-OCR2_Counter", "Stop Line Exception-OCR2_Counter");
            //关闭OCR3_Counter
            CloseOCRorPLC(myThread3_Counter, ss3_counter, "OCR3_Counter", null, "", "停止生产-OCR3_Counter", "Stop Line Exception-OCR3_Counter");

            CloseThread(m_thd_checkMakeBox, "停止生产-停止满箱检查线程-异常", "Stop Line Exception-Check makeBox");

            CloseThread(m_thd_checkpallet, "停止生产-停止满托检查线程-异常", "Stop Line Exception-Check Pallet");

            CloseThread(m_thd_channel, "停止生产-停止分道检查线程-异常", "Stop Line Exception-Check Channel");

            Stop_PLC_Counte();//停止plc 结束生产

            ClosePLC();

            CloseOCRorPLC(myThread_PLC2, socket_PLC_Sender, "PLC_Sender", null, "", "停止生产-停止PLC发送线程-异常", "Stop Line Exception Sender PLC");

            lb_xiang.Text = "0";
            lb_xiang1.Text = "0";
            lb_tuo.Text = "0";
            lb_tong.Text = "0";
            ocr1Count.Text = "0";
            tocr1count.Text = "0";

            ocr3Count.Text = "0";
            ocr7Count.Text = "0";
            makedBoxCount.Text = "0";
            lb_msg.Text = "0";
            tItemCount.Text = "0";
            tItemCount2.Text = "0";

            ResetPLCCountVar();//重置plc计数变量

            isProducing = false;

            //保存ocr读码数量
            SaveOcrReadInfo();

            tDocInfo.Text = "";

            #region 重置读码率相关变量
            OCR1NoReadAmount = 0;
            OCR1ReadAmount = 0;
            OCR2NoReadAmount = 0;
            OCR2ReadAmount = 0;
            OCR3NoReadAmount = 0;
            OCR3ReadAmount = 0;
            lblOCR1ReadRate.Text = "0%";
            lblOCR2ReadRate.Text = "0%";
            lblOCR3ReadRate.Text = "0%";
            #endregion
            #region 清空托盘信息
            view_pallet_no.DataSource = null;
            view_pallet_box.DataSource = null;
            txt_pallet_box.Text = "";
            lbl_pallet_no.Text = "";
            lbl_pallet_no_number.Text = "";
            view_pallet_no.Rows.Clear();
            #endregion
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            try
            {


                if (!uploadDataFinished)
                {
                    MessageBox.Show("数据上传中，请稍后再结束生产！");
                    return;
                }

                #region 显示当天有多少异常托盘  雷子华 20220729  
                //托盘信息有误提醒的是当前批次的   雷子华 2022.20.20
                //显示当天有多少异常托盘
                string sql = "select count(subject) as OCR4错误统计 from qx_operation where datediff(day, op_time, getdate()) = 0 and subject = 'CR4校验位置有问题' and key2 = '" + m_set.ProductNo + "' and key3 ='" + m_set.Batch + "'";
                DataSet dt = DbHelperSQL.Query(sql);
                int log_num = Common.Common.ToInt32(dt.Tables[0].Rows[0]["OCR4错误统计"].ToString());
                //MessageBox.Show("今日截至到当前时间校验检索码不一致一共出现了" + log_num + "条异常信息" + "(托盘)");
                if (log_num == 0)
                {

                }
                else
                {
                    if (Common.Common.TS("当前批次校验检索码不一致一共出现了  " + log_num + " 条异常信息" + "(托盘)，是否要处理?需要处理请点击确认，不处理请点击取消方可继续停止生产下一步"))
                    {
                        AddInfoLog("人工已点击确认，人工会去处理托盘异常");
                        AddInfoLog("当前批次校验检索码不一致一共出现了  " + log_num + " 条异常信息" + "(托盘)");
                        return;
                    }
                }
                #endregion

                if (Common.Common.TS("当前操作只针对批次全部生产完成的情况，确定停止生产吗？"))
                {

                    AddInfoLog("停止生产");
                    string error = "";
                    #region 1、输入起始和结束流水号
                    if (Common.Config.ReadValue("Line", "enableFlowInput") == "1") //录入流水号段开关
                    {
                        FlowNumInput fnum = new FlowNumInput(m_set.ProduceLine, m_set.site_no, m_set.ProductNo, m_set.Batch);
                        DialogResult dr = fnum.ShowDialog();
                        if (dr == DialogResult.Yes)
                        {
                            Common.Common.SaveFlowNum(m_set.site_no, m_set.ProduceLine, m_set.OrderNo, m_set.ProductNo, m_set.Batch, Common.Common.startFlowNum, Common.Common.endFlowNum, Common.Common.actQty, out error);
                            if (error != "")
                            {
                                AddErrorLog(true, "保存首尾流水号错误", error.Split('@')[0] + " " + error.Split('@')[1], "保存首尾流水号错误", error.Split('@')[0] + " " + error.Split('@')[1]);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    #endregion

                    #region  写入PQMS短信队列 结束生产
                    SendPQMSMsg(false, out error);
                    if (error != "")
                    {
                        return;
                    }
                    #endregion

                    SendMail();//2、发送邮件


                    this.Enabled = false;
                    lbl_showProgress.Enabled = true;
                    lbl_showProgress.Visible = true;
                    this.Update();
                    #region 3、最后零头组拖


                    lbl_showProgress.Text = "正在零头组拖,请稍后...";
                    this.Update();

                    ForceMakePallet();//3、最后零头组拖

                    forceMakePallet_ResetEvent = new AutoResetEvent(false);//
                    forceMakePallet_ResetEvent.WaitOne();
                    #endregion
                    #region 4、停止生产
                    lbl_showProgress.Text = "正在停止生产,请稍后...";
                    this.Update();
                    StopProduce();//4、停止生产
                    #endregion
                    this.Enabled = true;
                    lbl_showProgress.Visible = false;
                    //瓶箱关联变量清零
                    ocr3_counts = 0;
                    lvLog2.Items.Clear();//清空看板 2022.11.15 雷子华
                    lvLog3.Items.Clear();//清空看板 2022.12.16 雷子华
                    lvLog4.Items.Clear();//清空看板 2022.12.16 雷子华
                    bindingdata.Status = RunType.停止;
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止生产错误", ex.Message + " " + ex.StackTrace, "停止生产错误", ex.Message + " " + ex.StackTrace);
                this.Enabled = true;
                lbl_showProgress.Visible = false;
            }
        }


        public Socket serverSocket = null;
        public Thread m_th_listen = null;

        string m_mailto = "";
        string m_line_desc = "";
        int m_device_number = 0;
        string m_box_pack = "";//1:2,2:2,3:2,4:0,5:0,6:0 冒号前面是分道编号，后面是这个分道提供几瓶来装箱，总的意思代表 第一道取两瓶，第二道取两瓶，第三道取取两瓶，一共6瓶包装成一箱 瓶数为0代表此分道不使用
        string m_channel_port = "";//1:0,2:1,3:2,4:3,5:4,6:5，冒号前面是分道编号，后面是这个分道对应io卡的端口0里面0-7编号的哪个输入点
        int[] m_channel_to_port = new int[6];//分道编号与io卡端口0里面0-7编号的对应关系
        int[] m_channel_to_boxpack = new int[6];//分道编号与装箱的关系，瓶数为0代表此分道不使用

        /*
[Channel]
device_number=0
box_pack=1:2,2:2,3:2
channel_port=1:0,2:1,3:2,4:3,5:4,6:5
        */

        /// <summary>
        /// 窗体第一次登入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.bindingContext.DataSource = BindingData.getBindingData;

            string title_part = "产线关联 - iLineConnect - ";
            lbl_title.Text = title_part + m_set.ProduceLine + " -V" + Common.Operation.Version;
            Videojet_Load();//加载Videojet喷码机

            #region 分道调整数量鼠标移上去提示
            toolTip1.SetToolTip(this.btn_Channel1_Add, "增加当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel1_Subtrac, "减少当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel2_Add, "增加当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel2_Subtrac, "减少当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel3_Add, "增加当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel3_Subtrac, "减少当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel4_Add, "增加当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel4_Subtrac, "减少当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel5_Add, "增加当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel5_Subtrac, "减少当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel6_Add, "增加当前分道瓶数");
            toolTip1.SetToolTip(this.btn_Channel6_Subtrac, "减少当前分道瓶数");
            #endregion
            button2.Enabled = false;
            button3.Enabled = false;
            lbl_showProgress.Location = new Point((this.Width - lbl_showProgress.Width) / 2, (this.Height - lbl_showProgress.Height) / 2);

            m_device_number = Common.Common.ToInt32(Common.Config.ReadValue("Channel", "device_number"));
            m_box_pack = Common.Config.ReadValue("Channel", "box_pack");
            m_channel_port = Common.Config.ReadValue("Channel", "channel_port");

            string[] channel = m_channel_port.Split(',');


            m_conn = new SqlConnection(DbHelperSQL.connectionString);
            m_conn.Open();

            int i;
            for (i = 0; i < 6; i++)
            {
                if (channel[i].Substring(0, 1) == "1")
                {
                    m_channel_to_port[0] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "2")
                {
                    m_channel_to_port[1] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "3")
                {
                    m_channel_to_port[2] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "4")
                {
                    m_channel_to_port[3] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "5")
                {
                    m_channel_to_port[4] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "6")
                {
                    m_channel_to_port[5] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
            }

            string[] boxpack = m_box_pack.Split(',');

            for (i = 0; i < 6; i++)
            {
                if (channel[i].Substring(0, 1) == "1")
                {
                    m_channel_to_boxpack[0] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "2")
                {
                    m_channel_to_boxpack[1] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "3")
                {
                    m_channel_to_boxpack[2] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "4")
                {
                    m_channel_to_boxpack[3] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "5")
                {
                    m_channel_to_boxpack[4] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "6")
                {
                    m_channel_to_boxpack[5] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
            }

            m_line_desc = Common.Config.ReadValue("Line", "linedesc");

            AddInfoLog("主窗体加载", "Version=" + Operation.Version + ", User=" + m_set.UserName + ", Factory=" + m_set.site_no + "/" + m_set.site_desc + ", PLine_no=" + m_set.ProduceLine);

            lb_timer.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            toolStripStatusLabel2.Text = m_set.site_no + "@" + m_set.ProduceLine;

            group_Ocr1.Text = "瓶码OCR1-未启动";

            lblOCR3ReadRateTitle.Visible = true;
            lblOCR3ReadRate.Visible = true;
            group_Ocr3.Visible = true;
            group_Ocr7.Visible = true;
            button3.Enabled = false;

            for (i = 0; i < this.view_ocr1.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ocr1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            for (i = 0; i < this.view_ch1.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ch1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            for (i = 0; i < this.view_ch2.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ch2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            for (i = 0; i < this.view_ch3.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ch3.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            for (i = 0; i < this.view_ch4.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ch4.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            for (i = 0; i < this.view_ch5.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ch5.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            for (i = 0; i < this.view_ch6.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_ch6.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            for (i = 0; i < this.view_tong.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_tong.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }



            for (i = 0; i < this.view_box2.Columns.Count; i++)//禁止DataGridView 排序
            {
                this.view_box2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            lb_UserName.Text = m_set.UserName;

            IPAddress serverIp;
            int serverPort;

            int hh = (Screen.PrimaryScreen.Bounds.Height - 130) / 2;
            group_ch1.Height = hh;
            group_ch2.Height = hh;
            group_ch3.Height = hh;
            group_ch4.Height = hh;
            group_ch5.Height = hh;
            group_ch6.Height = hh;

            group_ch4.Top = group_ch1.Height + 10;
            group_ch5.Top = group_ch2.Height + 10;
            group_ch6.Top = group_ch3.Height + 10;

            try
            {
                m_mailto = Common.Config.ReadValue("Mail", "mail_to");

                string myport = Common.Config.ReadValue("PDA", "pda_port");

                serverIp = IPAddress.Any;

                serverPort = Common.Common.ToInt32(myport);

                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(serverIp, serverPort));  //绑定IP地址：端口  
                serverSocket.Listen(10);    //设定最多10个排队连接请求  
                AddInfoLog("PDA开始监听socket", serverSocket.LocalEndPoint.ToString());

                //通过Clientsoket发送数据  
                m_th_listen = new Thread(ListenClientConnect);
                m_th_listen.Start();

                #region 初始化看板
                enableboard = Common.Config.ReadValue("Board", "enable") == "1" ? true : false;
                if (enableboard)
                {
                    string boardIp = Common.Config.ReadValue("Board", "ip");
                    string boardPort = Common.Config.ReadValue("Board", "port");
                    board = new BoardTool(boardIp, boardPort);
                    board.OnReceiveMsg += new MsgHandler(board_ReceiveMsg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "PDA监听-异常", (ex.Message), "PDA Listen Exception", (ex.Message + " " + ex.StackTrace));

                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Close();
            }
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private void ListenClientConnect()
        {
            Socket clientSocket = null;

            while (true)
            {
                try
                {
                    clientSocket = serverSocket.Accept();
                    ThreadPool.QueueUserWorkItem(SocketKicker_ReceiveMessage, clientSocket);
                    AddInfoLog("PDA已经连接", clientSocket.LocalEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "PDA连接-异常", (ex.Message), "PDA Connect Exception", (ex.Message + " " + ex.StackTrace));
                }
            }
        }


        /*
        [START]起始标志 [END]结束标志
        中间的命令格式为，
        item_add, http://weixin.qq.com/r/b3VVTQ3Eo1DcrUku9yB9/q?bc=0104492203445767
        item_del,http://weixin.qq.com/r/b3VVTQ3Eo1DcrUku9yB9/q?bc=0104492203445767
        对工控机队列里增加和删除瓶码
        box_add,A#N#N#99#B#A#N#3315000#20170101#20170105#99001212
        box_del,A#N#N#99#B#A#N#3315000#20170101#20170105#99001212
        对工控机队列里增加和删除箱码
        以上为pda发送到工控机的命令格式
        工控机回复的格式为
        [START]1[END]代表删除或增加成功
        [START]0[END]代表失败
        [START]item_del,http://weixin.qq.com/r/b3VVTQ3Eo1DcrUku9yB9/q?bc=0104492203445767[END]
        pda发工控机的请求命令举例
        A#N#N#99#B#A#N#3315000#20170101#20170105#99001212
        康普顿箱码规则
        托盘是8位数字
        */

        /// <summary>  
        /// PDA剔除的SOCKET处理  
        /// </summary>  
        /// <param name="clientSocket"></param>  
        private async void SocketKicker_ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            byte[] bbuf = new byte[1024];
            string buf = "";

            string prefix = "";
            int sn = 0, sn1, sn2;

            string[] s = new string[30];

            string s_buf = "";
            string sReceive = "";
            int readbytes = 0;
            byte[] byteReceive = new byte[1024];
            byte[] byteSend = new byte[1024];
            int rbtotal = 0;

            int pos1, pos2, pos3;

            string type, data, data2;
            bool br;
            bool br2;//删除箱码 已组箱队列
            bool br3;//删除箱码 ocr3队列
            int ret = 0;
            string msg = string.Empty;//反馈给pad消息
            string sql = string.Empty;
            List<string> li_sql = new List<string>();

            while (true)
            {
                try
                {
                    sReceive = "";

                    while (true)
                    {
                        myClientSocket.ReceiveTimeout = 2000;//每次读数据延时2秒，如果抛出异常，说明连接中断
                        //我们当前剔除的连接是短连接，PDA剔除后自动断开，如果不能检测到连接中断的话，每个线程都不会关闭，CPU占用率会变得非常高。
                        readbytes = myClientSocket.Receive(byteReceive);
                        rbtotal += readbytes;
                        sReceive = sReceive + System.Text.Encoding.Default.GetString(byteReceive, 0, readbytes);
                        if (readbytes > 0)
                        {
                            AddInfoLog("Socket read=" + System.Text.Encoding.Default.GetString(byteReceive, 0, readbytes) + "\r\n");
                        }

                        if (sReceive.IndexOf("[END]") >= 0)
                        {
                            pos1 = sReceive.IndexOf("[START]");
                            pos3 = sReceive.IndexOf("[END]");

                            type = sReceive.Substring(pos1 + 7, sReceive.Length - pos1 - 7 - (sReceive.Length - pos3));
                            sReceive = sReceive.Substring(pos3 + 5);//多余的数据下次读取后继续使用

                            string[] s12 = type.Split(',');
                            pos2 = type.IndexOf(",");

                            type = type.Substring(0, pos2);

                            data = s12[1];
                            if (type == "item_rep")
                            {
                                data2 = s12[2];
                            }
                            else
                            {
                                data2 = "";
                            }

                            ret = 0;

                            if (type == "item_del")
                            {
                                if ((btn_Start.Enabled == false) && (btn_Stop.Enabled == true))
                                {

                                    if (data.Length != 65 && data.Length != 60 && data.Length != 12 && data.Length != 23) //增加sgm 12位瓶码剔除 增加23位PDCA瓶码剔除
                                    {
                                        ret = 0;
                                        msg = "非法条码";
                                    }
                                    else
                                    {
                                        OCR1ViewDataClass ocr1data = bindingdata.OCR1List.Select(a => a).Where(a => a.Allcode == data).First();
                                        if (ocr1data != null)
                                        {
                                            this.Invoke(new Action(() =>
                                            {
                                                lock (_lockObj)
                                                {
                                                    bindingdata.OCR1List.Remove(ocr1data);
                                                }
                                            }));
                                            setItemCount(Common.Common.ToInt32(tItemCount.Text) - 1);
                                            ret = 1;
                                            msg = "剔除成功";
                                        }
                                        else
                                        {
                                            ret = 0;
                                            for (i = 0; i < 6; i++)
                                            {
                                                if (m_channel_to_boxpack[i] > 0)
                                                {
                                                    OCR1ViewDataClass data_model = bindingdata.OCR1List.Select(a => a).Where(a => a.Allcode == data).First();
                                                    if (data_model != null)
                                                    {
                                                        string propertyName = $"CH{i + 1}_List";
                                                        PropertyInfo propertyInfo = bindingdata.GetType().GetProperty(propertyName);
                                                        if (propertyInfo != null)
                                                        {
                                                            object propertyValue = propertyInfo.GetValue(bindingdata);
                                                            BindingList<OCR1ViewDataClass> ch1List = propertyValue as BindingList<OCR1ViewDataClass>;
                                                            this.Invoke(new Action(() =>
                                                            {
                                                                ch1List.Remove(data_model);
                                                            }));
                                                            ret = 1;
                                                            msg = "剔除成功";
                                                            setItemCount(Common.Common.ToInt32(tItemCount.Text) - 1);

                                                            break;
                                                        }

                                                    }

                                                }//for循环结束
                                                ret = 0;
                                                msg = "剔除失败";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ret = 0;
                                    msg = "剔除失败";
                                }
                                AddInfoLog("PDA剔除瓶码", type + "," + data + "," + ret + msg);
                            }
                            else if (type == "box_del")
                            {
                                if ((btn_Start.Enabled == false) && (btn_Stop.Enabled == true))
                                {
                                    //1L线生产结束时箱子晃动d导致noread,需要用pda剔除这些noread码，（pda选择剔除“箱码”,手输"noread"或扫描noread值的条码）
                                    if (data.ToUpper() == "NOREAD")
                                    {
                                        string lastNoReadCode = bindingdata.OCR2List.FindLast(p => p.Length == 40);
                                        if (lastNoReadCode == null)
                                        {
                                            ret = 0;
                                            msg = "未找到noread数据，请核实。";
                                        }
                                        else
                                        {

                                            br = bindingdata.OCR2List.Remove(lastNoReadCode);
                                            if (br)
                                            {
                                                ret = 1;
                                                msg = "剔除成功";
                                            }
                                            else
                                            {
                                                msg = "剔除失败";
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (data.Length != 31)
                                        {
                                            ret = 0;
                                            msg = "非法条码";
                                        }
                                        else
                                        {
                                            if (m_set.ProduceLine == "1L")
                                            {

                                                br = bindingdata.OCR2List.Remove(data);

                                                if (br)
                                                {
                                                  
                                                    //删除缓存中的瓶箱数据 
                                                     await redisHelper_ocr1.RemoveOCR1ViewDataByKey("box_item_list:"+data, m_set.OrderNo);
                                                    ret = 1;
                                                    msg = "剔除成功";
                                                }
                                                else
                                                {
                                                    msg = "剔除失败";
                                                }


                                            }
                                            else
                                            {
                                                this.Invoke(new Action(() =>
                                                {
                                                    lock (_lockObj)
                                                    {
                                                        bindingdata.OCR2List.Remove(data);
                                                        var d = bindingdata.OCR3List.Where(a => a.Allcode == data).First();
                                                        bindingdata.OCR3List.Remove(d);
                                                         
                                                    }
                                                }));
                                              
                                                br2 = MakedBox_List.Remove(data);
                                              

                                              
                                                if (  (br2 == true) )
                                                {
                                                   

                                                    //删除缓存中的瓶箱数据 
                                                    await redisHelper_ocr1.RemoveOCR1ViewDataByKey("box_item_list:" + data, m_set.OrderNo);
                                                    //reload_ocr3list();
                                                    ret = 1;
                                                    msg = "剔除成功";
                                                }
                                                else
                                                {
                                                    ret = 0;
                                                    msg = "剔除失败";
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                                AddInfoLog("PDA剔除箱码", type + "," + data + "," + ret + msg);
                            }
                            else if (type == "item_rep")
                            {
                                if ((btn_Start.Enabled == false) && (btn_Stop.Enabled == true))
                                {
                                    if (data.Substring(0, 4).ToLower() != "http" || data2.Substring(0, 4).ToLower() != "http")
                                    {
                                        ret = 0;
                                        msg = "非法条码";
                                    }
                                    else
                                    {
                                        int myindex = bindingdata.OCR1List.IndexOf(bindingdata.OCR1List.Select(a => a).Where(a => a.Allcode == data).First());

                                        if (myindex < 0)
                                        {
                                            ret = 0;
                                            msg = "替换失败";
                                        }
                                        else
                                        {
                                            OCR1ViewDataClass oCR1ViewDataClass = GetOCR1ViewDataClass(data2);
                                            if (oCR1ViewDataClass == null)
                                            {
                                                ret = 0;
                                                msg = "替换失败";
                                            }
                                            else
                                            {
                                                lock (_lockObj)
                                                {
                                                    bindingdata.OCR1List[i] = oCR1ViewDataClass;
                                                }
                                                ret = 1;
                                                msg = "替换成功";
                                            }


                                        }
                                    }
                                }
                                AddInfoLog("PDA替换瓶码", type + "," + data + "," + data2 + "," + ret + msg);
                            }
                            else if (type == "box_item_add")
                            {
                                if ((btn_Start.Enabled == false) && (btn_Stop.Enabled == true) && m_set.XiangCount == (s12.Length - 2))
                                {
                                    if (data.Length != 31)
                                    {
                                        ret = 0;
                                        msg = "非法箱子条码";
                                    }
                                    else
                                    {
                                        bool flag = true;
                                        for (int i = 0; i < m_set.XiangCount; i++)
                                        {
                                            if (s12[i + 2].ToString().Trim().Substring(0, 4).ToLower() != "http")
                                            {
                                                ret = 0;
                                                msg = "非法瓶子条码";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (flag)
                                        {
                                            DataSet ds = DbHelperSQL.Query("select * from qx_bundle_px_temp where box_barcode ='" + data + "' order by box_barcode, mfd_date desc");
                                            if (ds.Tables.Count > 0)
                                            {
                                                if (ds.Tables[0].Rows.Count > 0)
                                                {
                                                    ret = 0;
                                                    msg = "该箱码已存在，不能新增";
                                                }
                                                else
                                                {
                                                    for (int k = 0; k < m_set.XiangCount; k++)
                                                    {
                                                        //加入瓶箱关联队列

                                                        bindingdata.Box_Item_List.Add(new BoxAndItemBundle { box_barcode = data, item_barcode = s12[k + 2].ToString() });
                                                        sql = string.Format("insert into qx_bundle_px_temp(site_no,pline_no,doc_no,sku_no,lot_no,mfd_date,box_barcode,item_barcode,op_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',getdate())", m_set.site_no, m_set.ProduceLine, m_set.OrderNo, m_set.ProductNo, m_set.Batch, m_set.ProduceDate, data, s12[k + 2].ToString());
                                                        li_sql.Add(sql);
                                                        AddInfoLog("sql=" + sql + "\r\n");
                                                    }
                                                    int rets = DbHelperSQL.ExecuteSqlTran(li_sql);
                                                    br = rets > 0 ? true : false;
                                                    if (br)
                                                    {
                                                        ret = 1;
                                                        msg = "新增箱子成功";
                                                        AddInfoLog("新增箱子成功：" + data + "成功");
                                                    }
                                                    else
                                                    {
                                                        ret = 0;
                                                        msg = "新增箱子失败";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (int k = 0; k < m_set.XiangCount; k++)
                                                {
                                                    //加入瓶箱关联队列

                                                    bindingdata.Box_Item_List.Add(new BoxAndItemBundle { box_barcode = data, item_barcode = s12[k + 2].ToString() });
                                                    sql = string.Format("insert into qx_bundle_px_temp(site_no,pline_no,doc_no,sku_no,lot_no,mfd_date,box_barcode,item_barcode,op_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',getdate())", m_set.site_no, m_set.ProduceLine, m_set.OrderNo, m_set.ProductNo, m_set.Batch, m_set.ProduceDate, data, s12[k + 2].ToString());
                                                    li_sql.Add(sql);
                                                    AddInfoLog("sql=" + sql + "\r\n");
                                                }
                                                int rets = DbHelperSQL.ExecuteSqlTran(li_sql);
                                                br = rets > 0 ? true : false;
                                                if (br)
                                                {
                                                    ret = 1;
                                                    msg = "新增箱子成功";
                                                    AddInfoLog("新增箱子成功：" + data + "成功");
                                                }
                                                else
                                                {
                                                    ret = 0;
                                                    msg = "新增箱子失败";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ret = 0;
                                            msg = "该箱码已存在，不能新增";
                                        }
                                    }
                                }
                                else
                                {
                                    ret = 0;
                                    msg = "箱子数量不正确";
                                }
                                AddInfoLog("PDA新增箱码", type + "," + data + ",length：" + s12.Length + "," + ret + msg);
                            }
                            else if (type == "box_itme_rep")
                            {
                                if ((btn_Start.Enabled == false) && (btn_Stop.Enabled == true) && m_set.XiangCount == (s12.Length - 2))
                                {
                                    if (data.Length != 31)
                                    {
                                        ret = 0;
                                        msg = "非法箱子条码";
                                    }
                                    else
                                    {
                                        bool flag = true;
                                        for (int i = 0; i < m_set.XiangCount; i++)
                                        {
                                            //if (s12[i + 2].ToString().Trim().Substring(0, 4).ToLower() != "http")
                                            if (s12[i + 2].ToString().Trim().Length != 65 && s12[i + 2].ToString().Trim().Length != 60 && s12[i + 2].ToString().Trim().Length != 12 && s12[i + 2].ToString().Trim().Length != 23) //增加sgm 12位瓶码剔除 增加23位PDCA瓶码剔除
                                            {
                                                ret = 0;
                                                msg = "非法瓶子条码";
                                                flag = false;
                                                break;
                                            }
                                        }
                                        if (flag)
                                        {
                                            DataSet ds = DbHelperSQL.Query("select top " + m_set.XiangCount + " * from qx_bundle_px_temp where box_barcode like 'ocr-%' and sku_no ='" + m_set.ProductNo + "' and lot_no='" + m_set.Batch + "' order by  autoid desc");
                                            if (ds.Tables.Count > 0)
                                            {
                                                if (ds.Tables[0].Rows.Count > 0)
                                                {
                                                    sql = string.Empty;
                                                    li_sql = new List<string>();
                                                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                                                    {
                                                        sql = string.Format("insert into qx_bundle_px_temp_history(site_no,pline_no,doc_no,sku_no,lot_no,mfd_date,box_barcode,item_barcode,op_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',getdate())", m_set.site_no, m_set.ProduceLine, m_set.OrderNo, m_set.ProductNo, m_set.Batch, m_set.ProduceDate, ds.Tables[0].Rows[j]["box_barcode"].ToString(), ds.Tables[0].Rows[j]["item_barcode"].ToString());
                                                        li_sql.Add(sql);
                                                        AddInfoLog("sql=" + sql + "\r\n");
                                                    }
                                                    int rets = DbHelperSQL.ExecuteSqlTran(li_sql);
                                                    if (rets > 0)
                                                    {
                                                     
                                                        //删除缓存中的瓶箱数据 
                                                        await redisHelper_ocr1.RemoveOCR1ViewDataByKey("box_item_list:" + ds.Tables[0].Rows[0], m_set.OrderNo);
                                                        li_sql = new List<string>();
                                                        DbHelperSQL.ExecuteSql("delete qx_bundle_px_temp where box_barcode='" + ds.Tables[0].Rows[0]["box_barcode"].ToString() + "'");
                                                        AddInfoLog("删除本地数据成功：" + ds.Tables[0].Rows[0]["box_barcode"].ToString() + "成功");
                                                        for (int k = 0; k < m_set.XiangCount; k++)
                                                        {
                                                            //加入瓶箱关联队列
                                                            bindingdata.Box_Item_List.Add(new BoxAndItemBundle { box_barcode = data, item_barcode = s12[k + 2].ToString() });
                                                            sql = string.Format("insert into qx_bundle_px_temp(site_no,pline_no,doc_no,sku_no,lot_no,mfd_date,box_barcode,item_barcode,op_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',getdate())", m_set.site_no, m_set.ProduceLine, m_set.OrderNo, m_set.ProductNo, m_set.Batch, m_set.ProduceDate, data, s12[k + 2].ToString());
                                                            li_sql.Add(sql);
                                                            AddInfoLog("sql=" + sql + "\r\n");
                                                        }
                                                        rets = DbHelperSQL.ExecuteSqlTran(li_sql);
                                                        br = rets > 0 ? true : false;
                                                        if (br)
                                                        {
                                                            ret = 1;
                                                            msg = "替换成功";
                                                            AddInfoLog("组箱替换成功：" + data + "成功");
                                                        }
                                                        else
                                                        {
                                                            ret = 0;
                                                            msg = "替换失败";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ret = 0;
                                                        msg = "插入历史数据失败";
                                                    }
                                                }
                                                else
                                                {
                                                    ret = 0;
                                                    msg = "替换失败无数据";
                                                }
                                            }
                                            else
                                            {
                                                ret = 0;
                                                msg = "替换失败无数据";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ret = 0;
                                    msg = "瓶子数量不正确";
                                }
                                AddInfoLog("PDA替换箱码", type + "," + data + "," + ret + msg);
                            }

                            rbtotal = sReceive.Length;

                            byteSend = Encoding.Default.GetBytes(ret.ToString() + "@" + msg);
                            myClientSocket.Send(byteSend);
                        }
                        Thread.Sleep(2000);//给出客户端断开连接的等待时间
                    }//while             
                }
                catch (Exception ex)
                {
                    //远程主机强迫关闭了一个现有的连接。代表client已经close
                    AddErrorLog(true, "PDA socket 异常", (ex.Message), "PDA socket Exception", (ex.Message + " " + ex.StackTrace));
                    try
                    {
                        byteSend = Encoding.Default.GetBytes("[START]" + 0 + "[END]");
                        myClientSocket.Send(byteSend);
                    }
                    catch { }
                    try
                    {
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                    }
                    catch { }

                    return;
                }
            }
        }



        /// <summary>
        /// 更新OCR消息标题
        /// </summary>
        public void refresh_group_Ocr(GroupBox gb, string text, Color color)
        {
            if (gb.InvokeRequired)
            {
                gb.Invoke(new myDelegate4(refresh_group_Ocr), new object[] { gb, text, color });
            }
            else
            {
                gb.Text = text;
                gb.ForeColor = color;
            }
        }

        /// <summary>
        /// 初始化OCR
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="gb"></param>
        /// <param name="td"></param>
        /// <param name="thread1delegate"></param>
        /// <param name="OCRName"></param>
        void OCRStart(string OCRName, Socket socket, GroupBox gb, Thread td)
        {
            IPAddress ipaddress = IPAddress.Parse(Common.Config.ReadValue(OCRName, "IP"));
            IPEndPoint endpoint = new IPEndPoint(ipaddress, Convert.ToInt32(Common.Config.ReadValue(OCRName, "Port")));
            socket.Connect(endpoint);
            if (gb != null)
            {
                if (OCRName == "OCR7")//已有OCR4，改名字
                {
                    OCRName = "OCR4";
                }
                refresh_group_Ocr(gb, OCRName == "OCR1" ? "瓶码" + OCRName + "-已启动" : (OCRName == "PLC" ? "分道PLC-已启动" : "箱码" + OCRName + "-已启动"), Color.Blue);
            }
            td.Start();
        }

        /// <summary>
        /// 开始生产
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (tDocno.Text == "")
                {
                    MessageBox.Show("请先选择一个生产任务！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //plc发送信号延时
                sendPLCSignalDelay = Convert.ToInt32(Common.Config.ReadValue("PLC", "sendPLCSignalDelay"));

                GetChannelAndChannelBottleAmount(m_set.site_no, m_set.ProductNo);
                AddInfoLog("加载分道信息", "m_channel_port=" + m_channel_port + ",m_box_pack=" + m_box_pack);
                AddInfoLog("生产日期", "生产日期：" + m_set.ProduceDate);
                if (m_set.ProduceDate.ToString().Trim().Length == 0)
                {
                    MessageBox.Show("生产日期", "生产日期为空，请联系技术人员");
                    return;
                }

                //镇江4L 组箱规格，3B4L一次6箱，其他一次4箱 by 2021.6.20 徐元丰
                if (m_set.PackageSize.IndexOf("3B") >= 0)
                {
                    makeBoxAmount = 6;
                }
                else
                {
                    makeBoxAmount = 4;
                }
                makeBoxNumbers = 1;//初始化 组箱顺序

                if (Common.Common.TS("确定开始生产？"))
                {
                    bottleWithFWCode = Common.Common.TS("瓶身是否带防伪码？"); //开始生产增加是否为防伪码判断，为后续oc1 noread报警提供类型判断 增加时间：2019-04-17 增加人：陶侕春
                    isBottle = Common.Config.ReadValue("Eliminate", "Isable1_Bottle").ToString() != "false" ? true : false;//是否开启瓶子剔除功能
                    isBox = Common.Config.ReadValue("Eliminate", "Isable1_Box").ToString() != "false" ? true : false;//是否开启箱子剔除功能
                    isDeleteBox = Common.Config.ReadValue("Eliminate", "IsDeleteBox").ToString() != "false" ? true : false;//是否开启箱子删除功能
                    isOCRCheck = Common.Config.ReadValue("Eliminate", "IsOCRCheck").ToString() != "false" ? true : false;//是否开启OCR校验功能
                    if (isBottle)//有剔除瓶子功能，绑定计数
                    {
                        removedqty = 0;
                        if (!string.IsNullOrEmpty(Common.Config.ReadValue("Line", "removedqty")))
                        {
                            removedqty = int.Parse(Common.Config.ReadValue("Line", "removedqty"));
                        }
                    }

                    if (Common.Common.TS("请确认OCR4读码器位置调整到正确位置！"))
                    {
                        AddInfoLog("开始生产", "User=" + m_set.UserName);

                        string subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "开始生产";
                        string content = "开始生产时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "，单号:" + tDocno.Text + ", 信息=" + tDocInfo.Text;
                        Common.Common.SendMail_Thread(subject, content);

                        #region 判断是否为PQMS产品
                        string pqms_items = pqms_items = Common.Config.ReadValue("Line", "PQMS_ITEM");
                        if (pqms_items == "")
                        {
                            MessageBox.Show("请在配置文件中配置PQMS_ITEM节点！", "PQMS产品配置错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (pqms_items.Split(',').Contains(m_set.ProductNo))
                        {
                            isPQMSSku = true;
                        }
                        else
                        {
                            isPQMSSku = false;
                        }
                        #endregion

                        #region 判断是否为DPCA产品 by 2021.6.21 徐元丰 以下产品不写在配置文件里了（安全），第一怕被人改了，第二怕没更新配置文件
                        string dpca_items = "217660,217619,190417,190418,190416,190415,195304,195305,201821,201820,190371";
                        if (dpca_items.Split(',').Contains(m_set.ProductNo))
                        {
                            isDPCA = true;
                        }
                        else
                        {
                            isDPCA = false;
                        }
                        #endregion

                        #region  写入PQMS短信队列 开始生产
                        string error_pqms = "";
                        SendPQMSMsg(true, out error_pqms);
                        if (error_pqms != "")
                        {
                            return;
                        }
                        #endregion

                        ResetPLCCountVar();//重置PLC计数相关变量

                        channelErrorFirePassTime = Convert.ToInt32(Common.Config.ReadValue("Channel", "errorFirePassTime"));
                        box_barcode_fire_pass_time = Convert.ToInt32(Common.Config.ReadValue("Line", "boxErrorFirePassTime"));



                        box_List_delete_copy = new List<string>(box_List_delete.ToArray());


                        //bindingdata.OCR1List.Clear();
                        box_List_delete.Clear();
                        //OCR2_List.Clear();
                        //OCR3_List.Clear();

                        //view_tong.Rows.Clear();
                        //view_ocr1.Rows.Clear();

                        view_box2.Rows.Clear();

                        //CH1_List.Clear();
                        //CH2_List.Clear();
                        //CH3_List.Clear();
                        //CH4_List.Clear();
                        //CH5_List.Clear();
                        //CH6_List.Clear();
                        //List_PLC.Clear();//清空PLC分道数据
                        bindingdata.List_PLC.Clear();

                        view_ch1.Rows.Clear();
                        view_ch2.Rows.Clear();
                        view_ch3.Rows.Clear();
                        view_ch4.Rows.Clear();
                        view_ch5.Rows.Clear();
                        view_ch6.Rows.Clear();

                        ch1count1.Text = "0";
                        ch1count2.Text = "0";
                        ch2count1.Text = "0";
                        ch2count2.Text = "0";
                        ch3count1.Text = "0";
                        ch3count2.Text = "0";
                        ch4count1.Text = "0";
                        ch4count2.Text = "0";
                        ch5count1.Text = "0";
                        ch5count2.Text = "0";
                        ch6count1.Text = "0";
                        ch6count2.Text = "0";
                        ch1sumcount1.Text = "0";
                        ch2sumcount1.Text = "0";
                        ch3sumcount1.Text = "0";
                        ch4sumcount1.Text = "0";
                        ch5sumcount1.Text = "0";
                        ch6sumcount1.Text = "0";

                        lb_xiang.Text = "0";
                        lb_tuo.Text = "0";
                        lb_tong.Text = "0";
                        ocr1Count.Text = "0";
                        tocr1count.Text = "0";

                        ocr3Count.Text = "0";
                        lb_msg.Text = "0";
                        tItemCount.Text = "0";
                        tItemCount2.Text = "0";

                        #region 韦迪捷喷码机开启 by 徐元丰 2021.3.2
                        //批号为6位，如果只有5位，就在开头补一个工厂代码。镇江开头为A；天津为B；广州为C；
                        string batch = m_set.Batch.Trim();
                        if (batch.Trim().Length == 5)
                        {
                            if (Common.Config.ReadValue("Line", "type").Trim().Length == 0)
                            {
                                batch = "A" + batch;//镇江开头为A；天津为B；广州为C；
                            }
                            else
                            {
                                batch = Common.Config.ReadValue("Line", "type") + batch;//镇江开头为A；天津为B；广州为C；
                            }
                        }
                        else if (batch.Trim().Length == 6)
                        {
                        }
                        else
                        {
                            MessageBox.Show("产品信息", m_set.ProductNo + "批号有误" + m_set.Batch);
                            return;
                        }
                        lbl_title2.Text = "";
                        lbl_title2.ForeColor = Color.Red;
                        if (bottleWithFWCode)//防伪码使用网络通信给喷码机发送打印内容
                        {
                            Videojet_Start_FWCode(batch);
                        }
                        else
                        {
                            Videojet_Start_NoFWCode(batch);
                        }
                        if (lbl_title2.Text.Trim().Length > 0)
                        {
                            lbl_title2.Text = "喷码机打印内容：" + lbl_title2.Text;
                        }
                        #endregion

                        #region OCR启动并且监听
                        try
                        {
                            Start_OCR1();
                        }
                        catch (Exception ex)
                        {
                            AddErrorLog(true, "OCR1连接异常", ex.Message, "OCR1 connect exception!", ex.Message + " " + ex.StackTrace);
                            MessageBox.Show(ex.Message, "OCR1连接异常");
                            return;
                        }

                        try
                        {
                            Start_OCR2();
                        }
                        catch (Exception ex)
                        {
                            //关闭OCR1
                            CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
                            AddErrorLog(true, "OCR2连接异常", ex.Message, "OCR2 connect exception!", ex.Message + " " + ex.StackTrace);
                            MessageBox.Show(ex.Message, "OCR2连接异常");
                            return;
                        }

                        try
                        {
                            Start_OCR3();
                        }
                        catch (Exception ex)
                        {
                            //关闭OCR1
                            CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
                            //关闭OCR2
                            CloseOCRorPLC(myThread2, ss2, "OCR2", null, "箱码OCR2-未启动", "停止生产-OCR2异常", "Stop Line Exception-OCR2");
                            AddErrorLog(true, "OCR3连接异常", ex.Message, "OCR3 connect exception!", ex.Message + " " + ex.StackTrace);
                            MessageBox.Show(ex.Message, "OCR3连接异常");
                            return;
                        }

                        try
                        {
                            if (isOCRCheck)// by 2022.2.11 ocr校验配置
                            {
                                Start_OCR4();
                            }
                        }
                        catch (Exception ex)
                        {
                            //关闭OCR1
                            CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
                            //关闭OCR2
                            CloseOCRorPLC(myThread2, ss2, "OCR2", null, "箱码OCR2-未启动", "停止生产-OCR2异常", "Stop Line Exception-OCR2");
                            //关闭OCR3
                            CloseOCRorPLC(myThread3, ss3, "OCR3", group_Ocr3, "箱码OCR3-未启动", "停止生产-OCR3异常", "Stop Line Exception-OCR3");
                            AddErrorLog(true, "OCR4连接异常", ex.Message, "OCR4 connect exception!", ex.Message + " " + ex.StackTrace);
                            MessageBox.Show(ex.Message, "OCR4连接异常");
                            return;
                        }

                        #region 分道PLC
                        try
                        {
                            Start_PLC();
                        }
                        catch (Exception ex)
                        {
                            //关闭OCR1
                            CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
                            //关闭OCR2
                            CloseOCRorPLC(myThread2, ss2, "OCR2", null, "箱码OCR2-未启动", "停止生产-OCR2异常", "Stop Line Exception-OCR2");
                            //关闭OCR3
                            CloseOCRorPLC(myThread3, ss3, "OCR3", group_Ocr3, "箱码OCR3-未启动", "停止生产-OCR3异常", "Stop Line Exception-OCR3");
                            //关闭OCR7 根据实际情况填写message信息
                            CloseOCRorPLC(myThread7, ss7, "OCR7", group_Ocr7, "托盘OCR4-未启动", "停止生产-OCR4异常", "Stop Line Exception-OCR4");
                            AddErrorLog(true, "PLC连接异常", ex.Message, "PLC connect exception!", ex.Message + " " + ex.StackTrace);
                            MessageBox.Show(ex.Message, "PLC连接异常");
                            return;
                        }
                        #endregion

                        #region PLC发送
                        try
                        {
                            Start_PLC_Sender();
                        }
                        catch (Exception ex)
                        {
                            ClosePLC();
                            //关闭OCR1
                            CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
                            //关闭OCR2
                            CloseOCRorPLC(myThread2, ss2, "OCR2", null, "箱码OCR2-未启动", "停止生产-OCR2异常", "Stop Line Exception-OCR2");
                            //关闭OCR3
                            CloseOCRorPLC(myThread3, ss3, "OCR3", group_Ocr3, "箱码OCR3-未启动", "停止生产-OCR3异常", "Stop Line Exception-OCR3");
                            //关闭OCR7 根据实际情况填写message信息
                            CloseOCRorPLC(myThread7, ss7, "OCR7", group_Ocr7, "托盘OCR4-未启动", "停止生产-OCR4异常", "Stop Line Exception-OCR4");
                            //关闭OCR2_Counter
                            CloseOCRorPLC(myThread2_Counter, ss2_counter, "OCR2_Counter", null, "", "停止生产-OCR2_Counter", "Stop Line Exception-OCR2_Counter");
                            //关闭OCR3_Counter
                            CloseOCRorPLC(myThread3_Counter, ss3_counter, "OCR3_Counter", null, "", "停止生产-OCR3_Counter", "Stop Line Exception-OCR3_Counter");

                            AddErrorLog(true, "PLC 发送连接异常", ex.Message, "PLC Sender connect exception!", ex.Message + " " + ex.StackTrace);
                            MessageBox.Show(ex.Message, "PLC 发送连接异常");
                            return;
                        }
                        #endregion

                        #endregion

                        #region 分道线程

                        myThreaddelegate_channel = new ThreadStart(th_channel_new);
                        m_thd_channel = new Thread(myThreaddelegate_channel);
                        m_thd_channel.Start();
                        #endregion

                        #region 组箱线程  xjy 2023-03-09 将ocr2 队列 数据进行组箱，并将数据存入数据库线程，其他事情什么都不做，与下面的组箱组托线程无关

                        m_thd_MakeBox = new Thread(th_MakeBox);
                        m_thd_MakeBox.Name = string.Format("[{0}]:[{1}]", "UploadData", this.GetType().Name);
                        m_thd_MakeBox.Start();
                        #endregion

                        #region 组箱组托线程
                        ThreadStart myThreaddelegate_checkpallet = null;

                        //4L线组箱线程
                        if (m_set.ProduceLine == "4L")
                        {
                            m_thd_checkMakeBox = new Thread(new ThreadStart(th_CheckMakeBox4L));//TODO by 2021.6.20 测试
                            m_thd_checkMakeBox.Start();
                        }
                        else //1L
                        {
                            m_thd_checkMakeBox = new Thread(new ThreadStart(th_CheckMakeBox1L_2));//产线改造成和4L一样，装箱方式不变 by 2021.6.30 徐元丰
                            m_thd_checkMakeBox.Start();
                        }
                        //不管1L还是4L的产线都以OCR3（倒数第二个OCR）读取的数据为准，OCR7（最后一个OCR）校验完，再组托
                        if (isOCRCheck)// by 2022.2.11 ocr校验配置
                        {
                            myThreaddelegate_checkpallet = new ThreadStart(th_CheckPallet2);//ocr4校验，但是不修正数据
                        }
                        else
                        {
                            myThreaddelegate_checkpallet = new ThreadStart(th_CheckPallet_4L); //ocr3组托，ocr4功能不启用
                        }
                        m_thd_checkpallet = new Thread(myThreaddelegate_checkpallet);
                        m_thd_checkpallet.Start();
                        #endregion

                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#ALLRET%"));//先全局复位 by 徐元丰 2021.6.23
                        AddInfoLog("全局复位", "全局复位：" + "#ALLRET%");
                        Thread.Sleep(100);
                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#LANRET%"));//分道计数复位 by 徐元丰 2021.6.23
                        AddInfoLog("分道计数复位", "分道计数复位：" + "#LANRET%");
                        Thread.Sleep(100);
                        //启动plc开始生产
                        Start_PLC_Counte();
                        AddInfoLog("启动plc开始生产", "启动plc开始生产：" + "#ALLSTT%");

                        #region 加载ocr读码数量,并计算读码率
                        string error = string.Empty;
                        OcrReadInfo read = Common.Common.LoadOcrReadInfo(m_set.OrderNo, out error);

                        if (error != string.Empty)
                        {
                            AddErrorLog(true, error.Split('@')[0], error.Split('@')[0], error.Split('@')[1], error.Split('@')[1]);
                        }
                        else
                        {
                            if (read != null)
                            {
                                OCR1ReadAmount = read.ocr1_read;
                                OCR1NoReadAmount = read.ocr1_noread;
                                OCR2ReadAmount = read.ocr2_read;
                                OCR2NoReadAmount = read.ocr2_noread;
                                OCR3ReadAmount = read.ocr3_read;
                                OCR3NoReadAmount = read.ocr3_noread;
                                if (OCR1ReadAmount != 0)
                                {
                                    lblOCR1ReadRate.Text = Common.Common.GetReadRate(OCR1ReadAmount, OCR1NoReadAmount);
                                }
                                if (OCR2ReadAmount != 0)
                                {
                                    lblOCR2ReadRate.Text = Common.Common.GetReadRate(OCR2ReadAmount, OCR2NoReadAmount);
                                }
                                if (OCR3ReadAmount != 0)
                                {
                                    lblOCR3ReadRate.Text = Common.Common.GetReadRate(OCR3ReadAmount, OCR3NoReadAmount);
                                }
                            }
                        }
                        #endregion

                        #region 有零头自动加载零头
                        error = "";
                        //if (Common.Common.CheckHasCacheData(m_set.OrderNo, out error))
                        //{
                        AddInfoLog("自动加载零头", "自动加载零头，订单号：" + m_set.OrderNo);
                        LoadCacheData();//自动加载零头
                        if (!string.IsNullOrEmpty(Common.Config.ReadValue("Line", "makeBoxNumbers").ToString()))
                        {
                            makeBoxNumbers = int.Parse(Common.Config.ReadValue("Line", "makeBoxNumbers"));
                        }
                        //}
                        //else
                        //{
                        //    if (error != "")
                        //    {
                        //        MessageBox.Show("加载零头错误，请重试！" + error.Split('@')[0]);
                        //        AddErrorLog(true, "加载零头数错误", error, "load cacheData error", error);
                        //        return;
                        //    }
                        //}


                        //加载ocr1缓存数据
                        //GetRedisData(tDocno.Text.Trim());


                        #endregion

                        if (enableboard)
                        {
                            outPut = m_set.act_qty;//产量                
                            th_UpdateBoardShow("1", m_set.Batch, m_set.req_qty.ToString(), outPut.ToString());//更新看板
                        }

                        //将是否为防伪码瓶子信号传递给p瓶码PLC
                        string FWCode_Signal = Common.Config.ReadValue("OCR1", "FWCode_Signal");
                        string NoFWCode_Signal = Common.Config.ReadValue("OCR1", "NoFWCode_Signal");
                        string signal = bottleWithFWCode ? FWCode_Signal : NoFWCode_Signal;
                        if (m_set.ProduceLine == "4L") //4L线 ocr1就是一个plc
                        {
                            socket_PLC_Sender.Send(Encoding.Default.GetBytes(signal));
                            AddInfoLog("是否为防伪码瓶子信号", "是否为防伪码瓶子信号：" + signal);
                        }
                        else //1L线
                        {
                            Thread.Sleep(100);
                            if (bottleWithFWCode)
                            {
                                Start_Produce_Signal("#BOTSTT%");//启用单瓶读码 by 2021.7.3
                                AddInfoLog("启用单瓶读码", "启用单瓶读码：" + "#BOTSTT%");
                            }
                            else
                            {
                                Start_Produce_Signal("#BOTSPP%");//关闭单瓶读码 by 2021.7.3
                                AddInfoLog("关闭单瓶读码", "关闭单瓶读码：" + "#BOTSPP%");
                            }
                        }

                        //button1.Enabled = false;//创建生产任务
                        //btn_getOrder.Enabled = false;
                        //btn_upload.Enabled = false;
                        //btn_Sys.Enabled = false;
                        //btn_Start.Enabled = false;
                        //btn_Stop.Enabled = true;
                        //btn_Pause.Enabled = true;

                        //btnRestPLC.Enabled = true;
                        //btn_close.Enabled = false;
                        //btn_Exit.Enabled = false;
                        //ImportOrderNo.Enabled = false;
                        //button2.Enabled = true;
                        //button3.Enabled = true;
                        //btn_pallet_add_box.Enabled = true;
                        //btn_pallet_delete_box.Enabled = true;
                        //btn_refresh.Enabled = true;

                        if (isDeleteBox)
                        {
                            btnDeleteBox.Enabled = true;
                        }
                        else
                        {
                            btnDeleteBox.Enabled = false;
                        }

                        #region 分道调整数量
                        //btn_Channel1_Add.Enabled = true;
                        //btn_Channel1_Subtrac.Enabled = true;
                        //btn_Channel2_Add.Enabled = true;
                        //btn_Channel2_Subtrac.Enabled = true;
                        //btn_Channel3_Add.Enabled = true;
                        //btn_Channel3_Subtrac.Enabled = true;
                        //btn_Channel4_Add.Enabled = true;
                        //btn_Channel4_Subtrac.Enabled = true;
                        //btn_Channel5_Add.Enabled = true;
                        //btn_Channel5_Subtrac.Enabled = true;
                        //btn_Channel6_Add.Enabled = true;
                        //btn_Channel6_Subtrac.Enabled = true;
                        #endregion
                        //btnClearOCR1.Enabled = true;

                        #region 非防伪产品隐藏分道显示 修改日期：2020-09-09
                        HiddenChannel();//根据分道加载的数据，隐藏不用的分道控件 by 2021.8.27 徐元丰
                        #endregion

                        isProducing = true;//正在生产
                        Thread_OCRHeartBeat();//ocr心跳包掉线检测
                    }
                    #region 开启数据上传线程
                    if (worker_UploadData != null && worker_UploadData.ThreadState == ThreadState.Running)
                    {
                        waitUploadData.Set();

                    }
                    else
                    {
                        worker_UploadData = new Thread(new ParameterizedThreadStart(th_UploadData));
                        worker_UploadData.Name = string.Format("[{0}]:[{1}]", "UploadData", this.GetType().Name);
                        worker_UploadData.Start();
                    }
                    #endregion
                    #region 开启数据缓存线程
                    if (RedisTh != null)
                    {
                        waitRedis.Set();
                    }
                    else
                    {
                        RedisTh = new Thread(new ParameterizedThreadStart(th_Redis));
                        RedisTh.Name = string.Format("[{0}]:[{1}]", "QxRedisData", this.GetType().Name);
                        RedisTh.Start();
                    }
                    if (RedisTh_boxitem != null)
                    {
                        waitRedis_boxitem.Set();
                    }
                    else
                    {
                        RedisTh_boxitem = new Thread(new ParameterizedThreadStart(th_Redis_boxitem));
                        RedisTh_boxitem.Name = string.Format("[{0}]:[{1}]", "QxRedisData_boxitem ", this.GetType().Name);
                        RedisTh_boxitem.Start();
                    }
                    if (RedisThRemove != null)
                    {
                        waitRedisRemove.Set();
                    }
                    else
                    {
                        RedisThRemove = new Thread(new ParameterizedThreadStart(th_RedisRemove));
                        RedisThRemove.Name = string.Format("[{0}]:[{1}]", "QxRedisData", this.GetType().Name);
                        RedisThRemove.Start();
                    }
                    if (RedisThRemove_boxitem != null)
                    {
                        waitRedisRemove_boxitem.Set();
                    }
                    else
                    {
                        RedisThRemove_boxitem = new Thread(new ParameterizedThreadStart(th_RedisRemove_boxitem));
                        RedisThRemove_boxitem.Name = string.Format("[{0}]:[{1}]", "QxRedisData_boxitem ", this.GetType().Name);
                        RedisThRemove_boxitem.Start();
                    }
                    
                    #endregion

                    bindingdata.Status = RunType.运行;
                }
            }
            catch (Exception ex)
            {
                ClosePLC();
                //关闭OCR1
                CloseOCRorPLC(myThread1, ss1, "OCR1", group_Ocr1, "瓶码OCR1-未启动", "停止生产-OCR1异常", "Stop Line Exception-OCR1");
                //关闭OCR2
                CloseOCRorPLC(myThread2, ss2, "OCR2", null, "箱码OCR2-未启动", "停止生产-OCR2异常", "Stop Line Exception-OCR2");
                //关闭OCR3
                CloseOCRorPLC(myThread3, ss3, "OCR3", group_Ocr3, "箱码OCR3-未启动", "停止生产-OCR3异常", "Stop Line Exception-OCR3");
                //关闭OCR7 根据实际情况填写message信息
                CloseOCRorPLC(myThread7, ss7, "OCR7", group_Ocr7, "托盘OCR4-未启动", "停止生产-OCR4异常", "Stop Line Exception-OCR4");
                //关闭PLC_Sender
                CloseOCRorPLC(null, socket_PLC_Sender, "PLC_Sender", null, "", "停止生产-停止PLC发送线程-异常", "Stop Line Exception Sender PLC");
                bindingdata.Status = RunType.停止;
                AddErrorLog2("开始生产报错", ex.Message + " " + ex.StackTrace);
                isProducing = false;
                //捕获到异常信息，创建一个新的comm对象，之前的不能用了。  
                MessageBox.Show(ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


        }


        #region ocr心跳包掉线检测
        void Thread_OCRHeartBeat()
        {
            List<Socket> li_Socket = new List<Socket>();
            #region 4L 剔除方案 瓶码是plc，不发心跳包 修改时间：2019-04-29
            if (m_set.ProduceLine == "1L")
            {
                if (ss1 != null)
                {
                    li_Socket.Add(ss1);
                }
            }
            #endregion
            if (ss2 != null)
            {
                li_Socket.Add(ss2);
            }
            if (ss3 != null)
            {
                li_Socket.Add(ss3);
            }
            if (ss7 != null)
            {
                li_Socket.Add(ss7);
            }
            myThread_OcrHeartBeat = new Thread(new ParameterizedThreadStart(OCRHeartBeat));
            myThread_OcrHeartBeat.Start(li_Socket);
        }
        #endregion
        #region 参数设置
        SpeechSynthesizer synth;
        Setting m_set = Setting.Instance;

        Thread m_thd_checkpallet;
        Thread m_thd_channel;
        Thread m_thd_checkMakeBox;//检测是否满箱线程
        public Socket ss1;//OCR1
        public Thread myThread1 { get; set; }
        public delegate void MyInvoke1(string str);

        public Socket ss2;//OCR2
        public Thread myThread2 { get; set; }
        public Socket ss2_counter;
        public Thread myThread2_Counter { get; set; }
        public delegate void MyInvoke2(string str);
        public delegate void MyInvoke4(string str1, string str2);
        public Socket ss3;//OCR3
        public Thread myThread3 { get; set; }
        public Socket ss3_counter;//
        public delegate void MyInvoke7(string str);
        public Socket ss7;//OCR7 校验组托
        public Thread myThread7 { get; set; }
        public Thread myThread3_Counter { get; set; }
        public Socket socket_PLC;
        public Thread myThread_PLC { get; set; }
        public Socket socket_PLC_Sender;
        public Thread myThread_PLC2 { get; set; }
        public Thread myThread_OcrHeartBeat { get; set; } //心跳包检测ocr掉线线程
        //public List<string> List_PLC = new List<string>();//PLC
        public delegate void MyInvoke3(string str);

        //List<string> OCR2_List = new List<string>();//OCR2
        //List<string> OCR3_List = new List<string>();//OCR3
        List<string> MakedBox_List = new List<string>();//已组箱队列
        List<string> OCR7_List = new List<string>();//OCR7
        List<string> OCR_Box_List = new List<string>();//OCR3+OCR4 队列
        //List<BoxAndItemBundle> box_item_list = new List<BoxAndItemBundle>();

        //List<string> CH1_List = new List<string>();//分道1
        //List<string> CH2_List = new List<string>();//分道2
        //List<string> CH3_List = new List<string>();//分道3
        //List<string> CH4_List = new List<string>();//分道4
        //List<string> CH5_List = new List<string>();//分道5
        //List<string> CH6_List = new List<string>();//分道6

        List<string> item_List = new List<string>();//OCR1
        List<string> box_List = new List<string>();//OCR2
        List<string> pallet_List = new List<string>();//OCR3

        List<string> box_List_delete = new List<string>();//删除箱码list
        List<string> box_List_delete_copy = new List<string>();//删除箱码list

        public string m_firstboxbarcode_copy = "";

        //喷码机发送和接收（2个）
        //防伪码产品时，使用
        SocketHelper sendSocket1 = new SocketHelper();//定义发送数据的Socket通讯类   
        SocketHelper ackSocket1 = new SocketHelper();//定义接收打印完成信号的Socket通讯类

        SocketHelper sendSocket2 = new SocketHelper();//定义发送数据的Socket通讯类   
        SocketHelper ackSocket2 = new SocketHelper();//定义接收打印完成信号的Socket通讯类

        //非防伪码产品时，使用
        SocketHelper sendSocket1_1 = new SocketHelper();//定义发送数据的Socket通讯类   
        SocketHelper ackSocket1_1 = new SocketHelper();//定义接收打印完成信号的Socket通讯类

        SocketHelper sendSocket2_1 = new SocketHelper();//定义发送数据的Socket通讯类   
        SocketHelper ackSocket2_1 = new SocketHelper();//定义接收打印完成信号的Socket通讯类

        SocketHelper sendSocket3_1 = new SocketHelper();//定义发送数据的Socket通讯类   
        SocketHelper ackSocket3_1 = new SocketHelper();//定义接收打印完成信号的Socket通讯类
        #endregion

        /// <summary>
        /// 从服务器下载分道使用信息和分道装瓶数量
        /// </summary>
        /// <param name="site_no">工厂代码</param>
        /// <param name="sku_no">产品代码</param>
        public void GetChannelAndChannelBottleAmount(string site_no, string sku_no)
        {
            string sql = string.Format("select a.lane,a.lane_bottle_amount,a.package_size,a.check_no,b.spec_type,b.pack_spec,b.ZJ_LN,b.TJ_LN,b.GZ_LN from qx_lane a inner join qx_master b on a.package_size=b.package_size where a.site_no='{0}' and b.sku_no='{1}'", site_no, sku_no);
            string encsql = Common.MD5ALGO.Encrypt(sql);
            DataSet ds = DbHelperSQL.Query(sql);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return;
            DataRow dr = ds.Tables[0].Rows[0];
            string channel_port = dr[0].ToString();
            string channel_pack = dr[1].ToString();
            //获取产品包装规格   
            m_set.PackageSize = dr[2].ToString();
            m_set.spec_type = dr[4].ToString();
            m_set.XiangCount = Convert.ToInt32(dr["pack_spec"].ToString());
            m_box_pack = channel_pack;
            m_channel_port = channel_port;
            check_no = Convert.ToInt32(dr["check_no"].ToString());
            ZJ_LN = dr["ZJ_LN"].ToString();
            TJ_LN = dr["TJ_LN"].ToString();
            GZ_LN = dr["GZ_LN"].ToString();

            string[] channel = m_channel_port.Split(',');
            int i;
            for (i = 0; i < 6; i++)
            {
                if (channel[i].Substring(0, 1) == "1")
                {
                    m_channel_to_port[0] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "2")
                {
                    m_channel_to_port[1] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "3")
                {
                    m_channel_to_port[2] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "4")
                {
                    m_channel_to_port[3] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "5")
                {
                    m_channel_to_port[4] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "6")
                {
                    m_channel_to_port[5] = Common.Common.ToInt32(channel[i].Substring(2, 1));
                }
            }

            string[] boxpack = m_box_pack.Split(',');

            for (i = 0; i < 6; i++)
            {
                if (channel[i].Substring(0, 1) == "1")
                {
                    m_channel_to_boxpack[0] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "2")
                {
                    m_channel_to_boxpack[1] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "3")
                {
                    m_channel_to_boxpack[2] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "4")
                {
                    m_channel_to_boxpack[3] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "5")
                {
                    m_channel_to_boxpack[4] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
                else if (channel[i].Substring(0, 1) == "6")
                {
                    m_channel_to_boxpack[5] = Common.Common.ToInt32(boxpack[i].Substring(2, 1));
                }
            }
        }





        int get_channel_item_count()//所有分道瓶码数量
        {
            int i;
            int box_count = 0;

            for (i = 0; i < 6; i++)
            {
                if (m_channel_to_boxpack[i] > 0)
                {
                    if (i == 0)
                    {
                        box_count += bindingdata.CH1_List.Count;
                    }
                    else if (i == 1)
                    {
                        box_count += bindingdata.CH2_List.Count;
                    }
                    else if (i == 2)
                    {
                        box_count += bindingdata.CH3_List.Count;
                    }
                    else if (i == 3)
                    {
                        box_count += bindingdata.CH4_List.Count;
                    }
                    else if (i == 4)
                    {
                        box_count += bindingdata.CH5_List.Count;
                    }
                    else if (i == 5)
                    {
                        box_count += bindingdata.CH6_List.Count;
                    }
                }
            }//for循环结束
            return box_count;
        }



        #region 线程中显示文字 修改日期：2019-11-06
        delegate void myDelegate_ui_settext_Label(Label tb, string str);//定义委托
        void ui_settext_Label(Label tb, string str)
        {
            if (tb.InvokeRequired)
            {
                tb.Invoke(new myDelegate_ui_settext_Label(ui_settext_Label), new object[] { tb, str });
            }
            else
            {
                tb.Text = str;
            }
        }
        #endregion



        //public void reload_ocr3list()
        //{
        //    try
        //    {
        //        if (view_box2.InvokeRequired)
        //        {
        //            myDelegate_reload_ocr3list _myinvoke = new myDelegate_reload_ocr3list(reload_ocr3list);
        //            view_box2.BeginInvoke(_myinvoke, new object[] { });
        //        }
        //        else
        //        {
        //            view_box2.Rows.Clear();
        //            int i;
        //            object[] objvalues = new object[2];

        //            for (i = 0; i < OCR3_List.Count; i++)
        //            {
        //                objvalues[0] = OCR3_List[i].Substring(21, 9);
        //                if (OCR3_List[i].Substring(0, 4).ToLower() == "ocr-")//万能码
        //                {
        //                    objvalues[0] = "NOREAD";
        //                }
        //                objvalues[1] = OCR3_List[i];
        //                this.view_box2.Rows.Add(objvalues);
        //            }

        //            if (view_box2.Rows.Count > 0)
        //            {
        //                view_box2.Rows[view_box2.Rows.Count - 1].Selected = true;               //设置为选中. 
        //                view_box2.FirstDisplayedScrollingRowIndex = view_box2.Rows.Count - 1;   //设置第一行显示
        //            }
        //            view_box2.Focus();//保证滑动条永远处于最新一条数据位置
        //            ocr3Count.Text = "" + view_box2.Rows.Count;
        //            lb_xiang.Text = ocr3Count.Text;
        //            lb_xiang1.Text = ocr3Count.Text;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddErrorLog2("reload view", ex.Message + "," + ex.StackTrace);
        //    }
        //}



        void AppendBoxOCRViewControl(DataGridView dgv, object[] objvalues)
        {
            dgv.Rows.Add(objvalues);
            dgv.Rows[dgv.Rows.Count - 1].Selected = true;               //设置为选中. 
            dgv.FirstDisplayedScrollingRowIndex = dgv.Rows.Count - 1;   //设置第一行显示
            dgv.Focus();//保证滑动条永远处于最新一条数据位置
        }
        void AppendBoxOCRView(string ocrName, string barcode)
        {
            object[] objvalues = new object[2];
            objvalues[0] = barcode.Substring(21, 9);
            if (barcode.Substring(0, 4).ToLower() == "ocr-")//万能码
            {
                switch (ocrName)
                {
                    case "OCR2":
                        OCR2NoReadAmount++;
                        break;
                    case "OCR3":
                        OCR3NoReadAmount++;
                        break;
                    default:
                        break;
                }
                objvalues[0] = "NOREAD";
            }
            else
            {
                switch (ocrName)
                {
                    case "OCR2":
                        OCR2ReadAmount++;
                        break;
                    case "OCR3":
                        OCR3ReadAmount++;
                        break;
                    default:
                        break;
                }
            }
            objvalues[1] = barcode;

            switch (ocrName)
            {


                case "OCR7":
                    OCR7_List.Add(barcode);
                    AppendBoxOCRViewControl(view_box6, objvalues);
                    int ocr7 = OCR7_List.Count;
                    ocr7Count.Text = "" + ocr7;
                    #region 显示ocr7读码率
                    //lblOCR7ReadRate.Text = Common.Common.GetReadRate(OCR7ReadAmount, OCR7NoReadAmount);
                    #endregion
                    break;
            }
        }


        /// <summary>
        ///校验组托的数据，根据扫出来的箱码及对应的位置（倒数第2箱），检验OCR3对应的索引位置是否一致。如果一致说明组托无误，如果不一致说明组托有误并报警
        ///注：该方法只报警，不对数据做修正（重要）
        /// </summary>
        /// <param name="ocrName">需要校验的OCR名称</param>
        /// <param name="barcode">当前扫码的信息</param>
        /// <param name="index">校验的位置</param>
        /// <param name="pallet_pack_maxqty">组托的数量</param>
        void AppendBoxOCRViewCheckBox2(string ocrName, string barcode, int index, int pallet_pack_maxqty)
        {
            object[] objvalues = new object[2];
            objvalues[0] = barcode.Substring(21, 9);
            objvalues[1] = barcode;
            int actual_index = 0;

            switch (ocrName)
            {
                case "OCR2":
                    break;
                case "OCR3":
                    #region ocr3
                    ocr_check_box_barcode = string.Empty;
                    act_box_barcode = string.Empty;
                    remark_check = string.Empty;

                    if (CheckBarcodeQueue.Count > 0)
                    {
                        string ocr3check = CheckBarcodeQueue.Dequeue();
                        ocr_check_box_barcode = barcode;
                        if (ocr3check != barcode)//校验索引对应的箱码，如果相同就直接开始组托
                        {
                            //发送消息给PLC
                            BJ_PLC_HOME_th("OCR3读取数据和校验检索码不一致");//TODO
                                                                //原提示 OCR3校验数据异常
                            AddErrorLog3(true, "CR4校验位置有问题", "OCR3读取数据和校验检索码不一致（OCR3：" + ocr3check.Split('|')[1] + "校验码：" + barcode + "）",
                                "CR4校验位置有问题" + barcode, "OCR3读取数据和校验检索码不一致（OCR3：" + ocr3check.Split('|')[1] + "校验码：" + barcode + "）", true, m_set.ProductNo, m_set.Batch);
                            AddErrorLog4(true, "CR4校验位置有问题", "OCR3读取数据和校验检索码不一致（OCR3：" + ocr3check.Split('|')[1] + "校验码：" + barcode + "）",
                                "CR4校验位置有问题" + barcode, "OCR3读取数据和校验检索码不一致（OCR3：" + ocr3check.Split('|')[1] + "校验码：" + barcode + "）", true, m_set.ProductNo, m_set.Batch);

                        }

                        #region OCR 校验数据保存
                        int isFWCode = bottleWithFWCode ? 1 : 0;
                        int isCheckVal = remark_check.Length > 0 ? 0 : 1;
                        string sql = "insert into qx_OCR_Check_Data([site_no],[site_desc],[pline_no],[sku_no],[sku_desc],[lot_no],[pallet_no],[box_barcode],[act_box_barcode],[isFWCode],[isCheckVal],[isRecover],[server_sync],[pb_date],[server_sync_time],[remark],[doc_date],[removedqty])"
                             + " values('" + m_set.site_no + "','" + m_set.site_desc + "','" + m_set.ProduceLine + "','" + m_set.ProductNo + "','" + m_set.ProductName + "','" + m_set.Batch + "','" + ocr3check.Split('|')[0] + "','" + ocr_check_box_barcode + "','" + ocr3check.Split('|')[1] + "','" + isFWCode + "','" + isCheckVal + "','" + isCheckVal + "','0',getdate(),null,'" + remark_check + "','" + m_set.doc_date + "','" + removedqty + "')";
                        //sql = "insert into qx_OCR_Check_Data([site_no],[site_desc],[pline_no],[sku_no],[sku_desc],[lot_no],[pallet_no],[box_barcode],[act_box_barcode],[isFWCode],[isCheckVal],[isRecover],[server_sync],[pb_date],[server_sync_time],[remark],[doc_date])"
                        //    + " values('" + m_set.site_no + "','" + m_set.site_desc + "','" + m_set.ProduceLine + "','" + m_set.ProductNo + "','" + m_set.ProductName + "','" + m_set.Batch + "','" + pallet_no + "','" + ocr_check_box_barcode + "','" + act_box_barcode + "','" + isFWCode + "','" + isCheckVal + "','" + isCheckVal + "','0',getdate(),null,'" + remark_check + "','" + m_set.doc_date + "')";
                        AddInfoLog("sql=" + sql + "\r\n");
                        ocr_check_box_barcode = string.Empty;
                        act_box_barcode = string.Empty;
                        remark_check = string.Empty;
                        removedqty = 0;
                        int ret = DbHelperSQL.ExecuteSql(sql);
                        if (ret != 1)
                        {
                            AddErrorLog(true, "OCR3校验数据异常", "校验信息插入数据库异常", "OCR3校验数据异常" + barcode, "校验信息插入数据库异常");
                        }
                        #endregion

                        //th_CheckPallet2();//开始组托
                    }
                    else
                    {
                        //发送消息给PLC
                        BJ_PLC_HOME_th(ocrName + "已组托数据不足");//TODO
                        AddErrorLog(true, "OCR3校验数据异常", "OCR3已组托数据不足", "OCR3校验数据异常" + barcode, "OCR3已组托数据不足");
                    }
                    #endregion
                    #region 显示ocr3读码率
                    //reload_ocr3list();//OCR3列表数据更新（把已组托的箱码删除）
                    int ocr3 = bindingdata.OCR3List.Count;
                    ocr3Count.Text = "" + ocr3;
                    lblOCR3ReadRate.Text = Common.Common.GetReadRate(OCR3ReadAmount, OCR3NoReadAmount);
                    #endregion
                    break;
                case "OCR4":
                    break;
                case "OCR5":
                    break;
                case "OCR6":
                    break;
                case "OCR7":
                    break;
            }
        }
        #region OCR接收数据处理

        #region OCR1
        public void OCR1_ScanBarcode()
        {
            string sql = "";
            byte[] data = new byte[1024];
            int ms1, ms2;
            string stringdata = string.Empty;
            string ocrname = "OCR1";
            while (true)
            {
                try
                {
                    ms1 = System.Environment.TickCount;
                    try
                    {
                        Common.Common.ReceiveOCRData(OCR1_ProcessBarcode, AddInfoLog, AloneLog.AddInfoLog, ss1, m_set.site_desc, m_set.ProduceLine, ref stringdata, "OCR1");
                        ms2 = System.Environment.TickCount;
                        if ((ms2 - ms1 > 0))
                        {
                            AddInfoLog("OCR1 ScanBarcode()耗时=" + (ms2 - ms1) + "\r\n");
                        }
                    }
                    catch (SocketException ex)
                    {
                        AddErrorLog(true, "OCR1工作异常", ex.Message, "OCR1 Exception", ex.Message + " " + ex.StackTrace);
                        AloneLog.AddErrorLog("OCR1", "OCR1工作异常 " + ex.Message + " " + ex.StackTrace);
                        refresh_group_Ocr(group_Ocr1, "OCR1工位-异常", Color.Red);
                        //报警
                        BJ_PLC_th("OCR1工作异常");//TODO
                        AddErrorLog4(true, "OCR1工作异常", ex.Message, "OCR1 Exception", ex.Message + " " + ex.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    ReConnectOCR:
                        {
                            try
                            {
                                CloseOCRorPLC1(myThread1, ss1, ocrname, "关闭" + ocrname, "关闭" + ocrname, "Stop Line Exception-" + ocrname);
                                Start_OCR1();
                            }
                            catch (Exception ex1)
                            {
                                AddErrorLog(true, "OCR1工作异常", ex1.Message, "OCR1 Exception", ex1.Message + " " + ex1.StackTrace);
                                AloneLog.AddErrorLog("OCR1", "OCR1工作异常 " + ex1.Message + " " + ex1.StackTrace);
                                goto ReConnectOCR;
                            }
                            return;
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "OCR1工作异常", e.Message, "OCR1 Exception", e.Message + " " + e.StackTrace);
                    AloneLog.AddErrorLog("OCR1", "OCR1工作异常 " + e.Message + " " + e.StackTrace);
                    refresh_group_Ocr(group_Ocr1, "OCR1工位-异常", Color.Red);
                    AddErrorLog4(true, "OCR1工作异常", e.Message, "OCR1 Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    continue;
                }
            }
        }

        void Start_OCR1()
        {
            ss1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread1 = new Thread(new ThreadStart(OCR1_ScanBarcode));
            OCRStart("OCR1", ss1, group_Ocr1, myThread1);
        }

        void Start_OCR2()
        {
            ss2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread2 = new Thread(new ThreadStart(OCR2_ScanBarcode));
            OCRStart("OCR2", ss2, null, myThread2);
        }

        /// <summary>
        /// 接收OCR2计数
        /// </summary>
        void Start_OCR2_Counter()
        {
            ss2_counter = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread2_Counter = new Thread(new ThreadStart(OCR2_Counter_Process));
            OCRStart("OCR2_Counter", ss2_counter, null, myThread2_Counter);
        }
        void Start_OCR3()
        {
            ss3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread3 = new Thread(new ThreadStart(OCR3_ScanBarcode));
            OCRStart("OCR3", ss3, group_Ocr3, myThread3);
        }

        /// <summary>
        /// 接收OCR3计数
        /// </summary>
        void Start_OCR3_Counter()
        {
            ss3_counter = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread3_Counter = new Thread(new ThreadStart(OCR3_Counter_Process));
            OCRStart("OCR3_Counter", ss3_counter, null, myThread3_Counter);
        }
        /// <summary>
        /// 开启ocr4 读码线程
        /// </summary>
        void Start_OCR4()
        {
            ss7 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread7 = new Thread(new ThreadStart(OCR4_ScanBarcode));
            OCRStart("OCR4", ss7, group_Ocr7, myThread7);
        }

        void Start_PLC()
        {
            socket_PLC = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            myThread_PLC = new Thread(new ThreadStart(PLC_Process));
            OCRStart("PLC", socket_PLC, group_PLC, myThread_PLC);
        }

        /// <summary>
        /// 给plc发数据
        /// </summary>
        void Start_PLC_Sender()
        {
            socket_PLC_Sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(Common.Config.ReadValue("PLC_Sender", "IP"));
            IPEndPoint endpoint = new IPEndPoint(ipaddress, Convert.ToInt32(Common.Config.ReadValue("PLC_Sender", "Port")));
            socket_PLC_Sender.Connect(endpoint);
            myThread_PLC2 = new Thread(new ThreadStart(PLC_ReceiveMsg));//TODO
            myThread_PLC2.Start();//TODO
        }
        void Start_PLC_Signal()
        {
            socket_PLC = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = IPAddress.Parse(Common.Config.ReadValue("PLC", "IP"));
            IPEndPoint endpoint = new IPEndPoint(ipaddress, Convert.ToInt32(Common.Config.ReadValue("PLC", "Port")));
            socket_PLC.Connect(endpoint);
        }
        int ocr1 = 0;
        public void OCR1_ProcessBarcode(string msg)
        {
            int ms1, ms2;
            int ms01, ms02;
            {
                ms1 = System.Environment.TickCount;

                //在线程里以安全方式调用控件
                if (this.InvokeRequired)
                {
                    MyInvoke1 _myinvoke = new MyInvoke1(OCR1_ProcessBarcode);
                    this.Invoke(_myinvoke, new object[] { msg });
                }
                else
                {
                    msg = msg.TrimStart('?').Replace("\r\n", "").Replace("\0", "").Trim();

                    if (string.IsNullOrEmpty(msg))//如果消息长度为空则不处理
                    {
                        return;
                    }

                    //取消前4位判断 解决条码部分读取程序崩溃的问题 2018-03-24  msg.Substring(0, 4).ToLower() != "http"  1、增加SGM 12位一维码兼容 修改时间2018-06-05
                    //增加60位马自达瓶码支持http://mazda.qixuan.cc/?s=P01&e=26&c=001M&f=8845488603030725
                    if (msg.Contains("NOREAD") || string.IsNullOrEmpty(msg) || msg == "0" || (msg.Length != 65 && msg.Length != 12 && msg.Length != 23 && msg.Length != 60)) //23位瓶子 神龙 DPCA  
                    {
                        #region 报警程序 - 读码失败报警
                        lb_msg.Text = "OCR1错误-二维码读取失败！" + msg.Length.ToString();
                        AddErrorLog(true, "OCR1 错误", "二维码读取失败! " + msg, "OCR1 Error", "QRCODE read failed! " + msg, false);
                        AddErrorLog4(true, "OCR1 错误", "二维码读取失败! " + msg, "OCR1 Error", "QRCODE read failed! " + msg, true, m_set.ProductNo, m_set.Batch);
                        #endregion
                        if (msg.Contains("NOREAD"))
                        {
                            //msg = "ocr-" + Guid.NewGuid().ToString();
                            Common.Common.NoReadAmount++;
                            #region 增加防伪瓶子 ocr1 noread报警功能,非防伪码瓶子不报警 增加时间 2019-04-17 修改人：陶侕春 
                            if (bottleWithFWCode)
                            {
                                if (isBottle)// by 2021.6.20 瓶子剔除
                                {
                                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOTOUT%"));
                                    AddInfoLog("发送开始报警信息#BOTOUT%到PLC成功,消息：" + msg.ToString());
                                    removedqty++;
                                }
                                else
                                {
                                    msg = "ocr-" + Guid.NewGuid().ToString();
                                }
                                BJ_PLC_th("ocr1 noread报警" + msg);//TODO by 2021.7.8 没有剔除，只报警并给万能码
                            }
                            #endregion
                            //插入万能码 ocr-04db6b2f-f0fe-44e0-b55b-62857817cf03，长度40
                            goto OCR1_PB;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        #region 条码长度正确的情况下再判断是否含有特殊字符 增加时间 2019-07-15
                        string msg_temp = msg.Replace("/", "").Replace(":", "").Replace(".", "").Replace("?", "").Replace("=", "").Replace("&", "");
                        if (!Common.Common.IsLetterOrNumber(msg_temp))
                        {
                            lb_msg.Text = "OCR1错误-读取数据含有特殊字符";
                            if (isBottle) //by 2021.6.20 瓶子剔除
                            {
                                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOTOUT%"));
                                AddInfoLog("发送开始报警信息BOTOUT到PLC成功,消息：" + msg.ToString());
                                removedqty++;
                            }
                            else
                            {
                                BJ_PLC_th("OCR1错误-读取数据含有特殊字符" + msg);//TODO by 2021.7.8 没有剔除，只报警并给万能码
                            }
                            AddErrorLog(true, "OCR1读取数据-错误", "OCR1读取数据含有特殊字符" + msg, "OCR1读取数据含有特殊字符" + msg, msg);
                            AddErrorLog4(true, "OCR1读取数据-错误", "OCR1读取数据含有特殊字符" + msg, "OCR1读取数据含有特殊字符" + msg, msg, true, m_set.ProductNo, m_set.Batch);
                            return;
                        }
                        #endregion

                        //判断同一个桶是否重复读码
                        lb_msg.Text = "";
                        if (lastBarcode == msg)
                        {
                            BJ_PLC_th("OCR1读取数据-重码" + msg);
                            AddErrorLog(true, "OCR1读取数据-重码", "OCR1读取数据-重码" + msg, "OCR1读取数据-重码" + msg, msg);
                            AddErrorLog4(true, "OCR1读取数据-重码", "OCR1读取数据-重码" + msg, "OCR1读取数据-重码" + msg, msg, true, m_set.ProductNo, m_set.Batch);
                            return;
                        }
                        lastBarcode = msg;
                        //#region 判断同一个桶是否重复读码
                        //last_barcode = msg;
                        //for (int j = 0; j < OCR1_List.Count; j++)
                        //{
                        //    if (OCR1_List[j].ToString().Trim() == msg.Trim())
                        //    {
                        //        Common.Common.RepeatCodeAmount++;
                        //        AddErrorLog(true, "OCR1读取数据-重码", "OCR1读取数据-重码" + msg, "OCR1读取数据-重码" + msg, msg);
                        //        last_barcode = "ocr-" + Guid.NewGuid().ToString();
                        //        //报警
                        //        BJ_th();
                        //        BJ_PLC_th("OCR1读取数据-重码" + msg);//TODO
                        //        break;
                        //    }
                        //}
                        //#endregion
                    }
                OCR1_PB:
                    {
                        bool flagNoread = false;
                        try
                        {
                            //判断是否为NOREAD瓶子，如果是并有剔除功能则不进入ocr1List队列里
                            if (isBottle)// by 2021.6.20 瓶子剔除
                            {
                                if (!msg.Contains("NOREAD"))
                                {
                                    flagNoread = true;
                                }
                                else
                                {
                                    flagNoread = false;
                                }
                            }
                            else
                            {
                                flagNoread = true;
                            }
                            if (lastBarcode != "" && flagNoread)//2021.06.26 判断是否重码或是NOREAD， 上述信息不进入OCR1 by 徐元丰
                            {
                                Common.Common.AllProduceAmount++;
                                // OCR1_List.Add(msg);

                                string intime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                object[] objvalues = new object[2];
                                objvalues[0] = (msg.Length == 65 || msg.Length == 60) ? msg.Substring(msg.Length - 16, 16) : msg;
                                if (msg.Substring(0, 4).ToLower() == "ocr-")//万能码
                                {
                                    OCR1NoReadAmount++;
                                    objvalues[0] = "NOREAD";
                                }
                                else
                                {
                                    OCR1ReadAmount++;
                                }
                                objvalues[1] = msg;

                                ms01 = System.Environment.TickCount;


                                this.Invoke(new Action(() =>
                                {
                                    lock (_lockObj)
                                    {
                                        bindingdata.OCR1List.Add(new OCR1ViewDataClass() { Code = objvalues[0].ToString(), Allcode = objvalues[1].ToString() });
                                    }
                                }));
                                //this.view_tong.Rows.Add(objvalues);
                                //view_tong.Rows[view_tong.Rows.Count - 1].Selected = true;               //设置为选中. 
                                //view_tong.FirstDisplayedScrollingRowIndex = view_tong.Rows.Count - 1;   //设置第一行显示
                                //view_tong.Focus();//保证滑动条永远处于最新一条数据位置

                                ms02 = System.Environment.TickCount;
                                AddInfoLog("OCR1 ProcessBarcode()界面处理耗时=" + (ms02 - ms01) + "\r\n");

                                ms01 = System.Environment.TickCount;

                                List_ItemBarcode.Add(msg);

                                ms02 = System.Environment.TickCount;

                                ocr1 = bindingdata.OCR1List.Count;
                                ocr1Count.Text = "" + ocr1;

                                #region 显示ocr1读码率
                                lblOCR1ReadRate.Text = Common.Common.GetReadRate(OCR1ReadAmount, OCR1NoReadAmount);
                                #endregion
                                lb_tong.Text = ocr1Count.Text;
                                setItemCount(Common.Common.ToInt32(tItemCount.Text) + 1);

                                ms2 = System.Environment.TickCount;
                                if ((ms2 - ms1 > 0))
                                {
                                    AddInfoLog("OCR1 ProcessBarcode()耗时=" + (ms2 - ms1) + "\r\n");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AddErrorLog(true, "瓶码OCR1异常", ex.Message, "OCR1 Exception", ex.Message + " " + ex.StackTrace);
                            AloneLog.AddErrorLog("OCR1", "瓶码OCR1异常 " + ex.Message + " " + ex.StackTrace);
                        }
                    }
                }
            }
        }
        #endregion
        public OCR1ViewDataClass GetOCR1ViewDataClass(string msg)
        {
            try
            {
                object[] objvalues = new object[2];
                objvalues[0] = (msg.Length == 65 || msg.Length == 60) ? msg.Substring(msg.Length - 16, 16) : msg;
                if (msg.Substring(0, 4).ToLower() == "ocr-")//万能码
                {
                    objvalues[0] = "NOREAD";
                }

                objvalues[1] = msg;

                return new OCR1ViewDataClass() { Code = objvalues[0].ToString(), Allcode = objvalues[1].ToString() };
            }
            catch
            {
                return null;
            }

        }
        public OCR1ViewDataClass GetOCR2ViewDataClass(string msg)
        {
            try
            {
                object[] objvalues = new object[2];

                objvalues[0] = msg.Substring(21, 9);
                if (msg.Substring(0, 4).ToLower() == "ocr-")//万能码
                {
                    objvalues[0] = "NOREAD";
                }
                objvalues[1] = msg;
                return new OCR1ViewDataClass() { Code = objvalues[0].ToString(), Allcode = objvalues[1].ToString() };
            }
            catch
            {
                return null;
            }

        }

        #region OCR2
        public void OCR2_ScanBarcode()
        {
            byte[] data = new byte[1024];
            string stringdata = string.Empty;
            string ocrname = "OCR2";
            while (true)
            {
                try
                {
                    try
                    {
                        Common.Common.ReceiveOCRData(OCR2_ProcessBarcode, AddInfoLog, AloneLog.AddInfoLog, ss2, m_set.site_desc, m_set.ProduceLine, ref stringdata, "OCR2");
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, "OCR2工作异常", e.Message, "OCR2 Exception", e.Message + " " + e.StackTrace);
                        AloneLog.AddErrorLog("OCR2", "OCR2工作异常 " + e.Message + " " + e.StackTrace);

                        BJ_PLC_OCR_th("OCR2工作异常");//TODO
                        AddErrorLog4(true, "OCR2工作异常", e.Message, "OCR2 Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    ReConnectOCR:
                        {
                            try
                            {
                                CloseOCRorPLC1(myThread2, ss2, ocrname, "关闭" + ocrname, "关闭" + ocrname, "Stop Line Exception-" + ocrname);
                                Start_OCR2();
                            }
                            catch (Exception ex1)
                            {
                                AddErrorLog(true, "OCR2工作异常", ex1.Message, "OCR2 Exception", ex1.Message + " " + ex1.StackTrace);
                                AloneLog.AddErrorLog("OCR2", "OCR2工作异常 " + ex1.Message + " " + ex1.StackTrace);

                                goto ReConnectOCR;
                            }
                            return;
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "OCR2工作异常", e.Message, "OCR2 Exception", e.Message + " " + e.StackTrace);
                    AddErrorLog4(true, "OCR2工作异常", e.Message, "OCR2 Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    AloneLog.AddErrorLog("OCR2", "OCR2工作异常 " + e.Message + " " + e.StackTrace);

                    BJ_PLC_OCR_th("OCR2工作异常");//TODO
                    continue;
                }
            }
        }

        public void OCR2_ProcessBarcode(string msg)
        {
            string sku_no_barcode = "";
            string lot_no_barcode = "";
            string mfd_date_barcode = "";

            //在线程里以安全方式调用控件
            if (lb_msg.InvokeRequired)
            {
                MyInvoke2 _myinvoke = new MyInvoke2(OCR2_ProcessBarcode);
                lb_msg.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                msg = msg.TrimStart('?').Trim();
                if (msg.Length == 0)//如果消息长度为空则不处理
                    return;
                #region OCR2读码次数累加 
                OCR2_read_code_amount++;
                #endregion

                //道达尔箱码需要核对产品代码，批号，生产日期
                // m_set.ProductNo  m_set.ProductName m_set.Batch m_set.doc_date
                //A198319HD210170421200000877566A

                string[] mf = msg.Split('#');
                int fields = mf.Length;

                if (msg.Contains("NOREAD") || string.IsNullOrEmpty(msg) || msg == "0" || msg.Length != 31)
                {
                    #region 报警程序 - 读码失败报警
                    lb_msg.Text = "OCR2错误-二维码读取失败！";
                    BJ_PLC_HOME_th("OCR2读取数据 - 二维码读取失败");//2021-01-16 ocr3报警改成PLC报警
                    AddErrorLog(true, "OCR2 错误", "二维码读取失败! " + msg, "OCR2 Error", "QRCODE read failed! " + msg);
                    AddErrorLog4(true, "OCR2 错误", "二维码读取失败! " + msg, "OCR2 Error", "QRCODE read failed! " + msg, true, m_set.ProductNo, m_set.Batch);
                    AloneLog.AddErrorLog("OCR2", "OCR2 错误,二维码读取失败! " + msg);
                    if (isBox)// by 2021.7.8 箱子剔除
                    {
                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOXOUT%"));//TODO by 2021.6.20 箱子剔除
                        AddInfoLog("发送剔除信息#BOXOUT%到PLC成功");
                    }
                    BJ_PLC_OCR_th("OCR2读取数据 - 二维码读取失败");//2021.7.7 ocr2报警改成PLC报警,
                    #endregion

                    //为了调试需要，临时允许9位一维码箱码进入系统，当noread处理
                    if (msg.Length == 9 + 2 || msg.Length == 9)
                    {
                        msg = "NOREAD";
                    }

                    if (msg.Contains("NOREAD"))
                    {
                        msg = "ocr-" + Guid.NewGuid().ToString();
                        //插入万能码 ocr-04db6b2f-f0fe-44e0-b55b-62857817cf03，长度40
                        goto OCR2_PB;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    sku_no_barcode = msg.Substring(1, 6);
                    lot_no_barcode = msg.Substring(7, 5);
                    mfd_date_barcode = "";

                    //康普顿箱码含产品代码，核对产品是否正确   
                    bool isError = false;

                    if (sku_no_barcode.ToUpper() != m_set.ProductNo.ToUpper())
                    {
                        isError = true;
                        lb_msg.Text = "OCR2错误-产品名称不对！";
                        AddErrorLog(true, "OCR2 错误", "产品名称不对！", "OCR2 Error", "Sku_no mismatch！");
                        AloneLog.AddErrorLog("OCR2", "OCR2 错误 二维码产品不对！");
                    }

                    if (lot_no_barcode.ToUpper() != m_set.Batch.ToUpper())
                    {
                        isError = true;
                        lb_msg.Text = "OCR2错误-批号不对！";
                        AddErrorLog(true, "OCR2 错误", "批号不对！", "OCR2 Error", "lot_no mismatch！");
                        AloneLog.AddErrorLog("OCR2", "OCR2 错误 二维码批号不对！");
                    }

                    if (isError)
                    {
                        if (isBox)// by 2021.7.8 箱子剔除
                        {
                            socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOXOUT%"));//TODO by 2021.6.20 箱子剔除
                            AddInfoLog("发送剔除信息#BOXOUT%到PLC成功");
                        }
                        BJ_PLC_OCR_th("OCR2错误-产品名称或是批号不对！");//TODO
                    }
                }

                lb_msg.Text = "";
                #region 判断同一个箱码是否重复读码
                bool isRepeat = false;
                if (lastOCR2Barcode == msg)
                {
                    isRepeat = true;
                }

                if (isRepeat)
                {
                    Common.Common.RepeatCodeAmount++;
                    AddErrorLog(true, "OCR2读取数据-重码", "OCR2读取数据-重码" + msg, "OCR2读取数据-重码" + msg, msg);
                    AloneLog.AddErrorLog("OCR2", "OCR2读取数据-重码 " + msg);
                    BJ_PLC_OCR_th("OCR2读取数据 - 重码");//2019-12-03 ocr2报警改成PLC报警
                    return;
                }
                lastOCR2Barcode = msg;
            #endregion
            OCR2_PB:
                {
                    try
                    {

                        //OCR2_List.Add(msg);
                        bindingdata.OCR2List.Add(msg);
                        waitHandler_MakeBox_fd.Set();
                        Application.DoEvents();

                        int ocr2 = bindingdata.OCR2List.Count;

                        #region 显示ocr2读码率
                        lblOCR2ReadRate.Text = Common.Common.GetReadRate(OCR2ReadAmount, OCR2NoReadAmount);
                        #endregion
                        lb_xiang.Text = "" + ocr2;
                    }
                    catch (Exception ex)
                    {
                        AddErrorLog(true, "箱码OCR2异常", ex.Message, "OCR2 Exception", ex.Message + " " + ex.StackTrace);
                        AloneLog.AddErrorLog("OCR2", "箱码OCR2异常 " + ex.Message + " " + ex.StackTrace);
                    }
                }
            }
        }

        public void OCR2_Counter_Process()
        {
            string sql = "";
            byte[] data = new byte[1024];
            while (true)
            {
                string stringdata = string.Empty;
                try
                {
                    try
                    {
                        int recv = ss2_counter.Receive(data);
                        stringdata = Encoding.UTF8.GetString(data, 0, recv);
                        if (stringdata.Contains("#s99%"))// by 2021.3.25 徐元丰
                        {
                            AddErrorLog(true, "PLC接收工作异常", "指令：" + stringdata, "PLC 接收Exception", "指令：" + stringdata);
                        }
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, "PLC OCR2计数PLC工作异常", e.Message, "PLC OCR2_Counter Exception", e.Message + " " + e.StackTrace);
                        Start_OCR2_Counter();
                        BJ_PLC_th("PLC OCR2计数PLC工作异常");//TODO
                        AddErrorLog4(true, "PLC OCR2计数PLC工作异常", e.Message, "PLC OCR2_Counter Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                        return;
                    }

                    string[] barcode = Regex.Split(stringdata, "%", RegexOptions.IgnoreCase);
                    if (barcode.Length > 0)
                    {
                        AddInfoLog("PLC OCR2_Counter whole Read=" + stringdata + "\r\n");
                        AloneLog.AddInfoLog("PLC OCR2_Counter", "PLC OCR2_Counter whole Read=" + stringdata + "\r\n");
                    }
                    int i;
                    string data_str = string.Empty;
                    for (i = 0; i < barcode.Length; i++)
                    {
                        data_str = barcode[i].Trim();
                        if (data_str != "")
                        {
                            string line_data = data_str.Split('#')[1];
                            OCR2_electric_eye_fired_amount = Convert.ToInt32(line_data.Split(',')[1].Trim());
                            AddInfoLog("PLC OCR2_Counter Read=" + data_str + "\r\n");
                            AloneLog.AddInfoLog("PLC OCR2_Counter", "PLC OCR2_Counter Read=" + data_str + "\r\n");
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "PLC OCR2_Counter工作异常", e.Message, "PLC OCR2_Counter Exception", e.Message + " " + e.StackTrace);
                    AloneLog.AddErrorLog("PLC OCR2_Counter", "PLC工作异常 " + e.Message + " " + e.StackTrace);
                    BJ_PLC_th("PLC OCR2_Counter工作异常");//TODO
                    continue;
                }
            }
        }

        public void PLC_Process_Sender()
        {
            string sql = "";
            byte[] data = new byte[1024];
            while (true)
            {
                string stringdata = string.Empty;
                try
                {
                    try
                    {
                        int recv = ss3_counter.Receive(data);
                        stringdata = Encoding.UTF8.GetString(data, 0, recv);
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, "PLC OCR3计数PLC工作异常", e.Message, "PLC OCR3_Counter Exception", e.Message + " " + e.StackTrace);
                        Start_OCR2_Counter();
                        BJ_PLC_th("PLC OCR3计数PLC工作异常");//TODO
                        return;
                    }

                    string[] barcode = Regex.Split(stringdata, "%", RegexOptions.IgnoreCase);
                    if (barcode.Length > 0)
                    {
                        AddInfoLog("PLC OCR3_Counter whole Read=" + stringdata + "\r\n");
                        AloneLog.AddInfoLog("PLC OCR3_Counter", "PLC OCR3_Counter whole Read=" + stringdata + "\r\n");
                    }
                    int i;
                    string data_str = string.Empty;
                    for (i = 0; i < barcode.Length; i++)
                    {
                        data_str = barcode[i].Trim();
                        if (data_str != "")
                        {
                            string line_data = data_str.Split('#')[1];
                            OCR3_electric_eye_fired_amount = Convert.ToInt32(line_data.Split(',')[1].Trim());
                            AddInfoLog("PLC OCR3_Counter Read=" + data_str + "\r\n");
                            AloneLog.AddInfoLog("PLC OCR3_Counter", "PLC OCR3_Counter Read=" + data_str + "\r\n");
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "PLC OCR3_Counter工作异常", e.Message, "PLC OCR3_Counter Exception", e.Message + " " + e.StackTrace);
                    AloneLog.AddErrorLog("PLC OCR3_Counter", "PLC工作异常 " + e.Message + " " + e.StackTrace);
                    BJ_PLC_th("PLC OCR3_Counter工作异常");//TODO
                    continue;
                }
            }
        }
        #endregion

        #region OCR3
        public void OCR3_ScanBarcode()
        {
            byte[] data = new byte[1024];
            string stringdata = string.Empty;
            string ocrname = "OCR3"; //注 只是改了界面上OCR的名字，让用户体验好点。实际上配置文件OCR3为组托数据，OCR7作为校验

            while (true)
            {
                try
                {
                    try
                    {
                        Common.Common.ReceiveOCRData(OCR3_ProcessBarcode, AddInfoLog, AloneLog.AddInfoLog, ss3, m_set.site_desc, m_set.ProduceLine, ref stringdata, "OCR3");
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, ocrname + "工作异常", e.Message, ocrname + " Exception", e.Message + " " + e.StackTrace);
                        AloneLog.AddErrorLog(ocrname, ocrname + "工作异常 " + e.Message + " " + e.StackTrace);
                        refresh_group_Ocr(group_Ocr3, ocrname + "工位-异常", Color.Red);
                        BJ_PLC_HOME_th(ocrname + "工作异常");//TODO
                        AddErrorLog4(true, ocrname + "工作异常", e.Message, ocrname + " Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    ReConnectOCR:
                        {
                            try
                            {
                                CloseOCRorPLC1(myThread3, ss3, ocrname, "关闭" + ocrname, "关闭" + ocrname, "Stop Line Exception-" + ocrname);
                                Start_OCR3();
                            }
                            catch (Exception ex1)
                            {
                                AddErrorLog(true, ocrname + "工作异常", ex1.Message, ocrname + " Exception", ex1.Message + " " + ex1.StackTrace);
                                AloneLog.AddErrorLog(ocrname, ocrname + "工作异常 " + ex1.Message + " " + ex1.StackTrace);
                                goto ReConnectOCR;
                            }
                            return;
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, ocrname + "工作异常", e.Message, ocrname + " Exception", e.Message + " " + e.StackTrace);
                    AloneLog.AddErrorLog(ocrname, ocrname + "工作异常 " + e.Message + " " + e.StackTrace);
                    refresh_group_Ocr(group_Ocr3, ocrname + "工作异常 ", Color.Red);
                    BJ_PLC_HOME_th(ocrname + "工作异常");//TODO
                    AddErrorLog4(true, ocrname + "工作异常", e.Message, ocrname + " Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    continue;
                }
            }
        }

        public void OCR3_ProcessBarcode(string msg)
        {
            string sku_no_barcode = "";
            string lot_no_barcode = "";
            string mfd_date_barcode = "";
            string tempsql = string.Empty;
            DataSet dsTemp = new DataSet();
            string guidTemp = string.Empty;

            //在线程里以安全方式调用控件
            if (view_box2.InvokeRequired)
            {
                MyInvoke3 _myinvoke = new MyInvoke3(OCR3_ProcessBarcode);
                view_box2.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                msg = msg.TrimStart('?').Trim();

                //道达尔箱码需要核对产品代码，批号，生产日期
                // m_set.ProductNo  m_set.ProductName m_set.Batch m_set.doc_date
                //A198319HD210170421200000877566A

                string[] mf = msg.Split('#');
                int fields = mf.Length;

                if (msg.Contains("NOREAD") || string.IsNullOrEmpty(msg) || msg == "0" || msg.Length != 31)
                {
                    #region 报警程序 - 读码失败报警
                    //报警
                    BJ_PLC_HOME_th("OCR3读取数据 - 二维码读取失败");//2021-01-16 ocr3报警改成PLC报警
                    lb_msg.Text = "OCR3错误-二维码读取失败！";
                    AddErrorLog(true, "OCR3 错误", "二维码读取失败! " + msg, "OCR3 Error", "QRCODE read failed! " + msg);
                    AddErrorLog4(true, "OCR3 错误", "二维码读取失败! " + msg, "OCR3 Error", "QRCODE read failed! " + msg, true, m_set.ProductNo, m_set.Batch);
                    #endregion

                    //为了调试需要，临时允许9位一维码箱码进入系统，当noread处理
                    if (msg.Length == 9 + 2 || msg.Length == 9)
                    {
                        msg = "NOREAD";
                    }

                    if (msg.Contains("NOREAD"))
                    {
                        msg = "ocr-" + Guid.NewGuid().ToString();
                        //插入万能码 ocr-04db6b2f-f0fe-44e0-b55b-62857817cf03，长度40
                        goto OCR3_PB;
                    }
                    else
                    {
                        return;
                    }
                }

                sku_no_barcode = msg.Substring(1, 6);
                lot_no_barcode = msg.Substring(7, 5);
                mfd_date_barcode = "";

                //康普顿箱码含产品代码，核对产品是否正确                    
                if (!msg.ToUpper().Contains("NOREAD") && sku_no_barcode.ToUpper() != m_set.ProductNo.ToUpper())
                {
                    lb_msg.Text = "OCR3错误-二维码产品不对！";
                    AddErrorLog(true, "OCR3 错误", "二维码产品不对！", "OCR3 Error", "Sku_no mismatch！");
                }

                if (!msg.ToUpper().Contains("NOREAD") && lot_no_barcode.ToUpper() != m_set.Batch.ToUpper())
                {
                    lb_msg.Text = "OCR3错误-二维码批号不对！";
                    AddErrorLog(true, "OCR3 错误", "二维码批号不对！", "OCR3 Error", "lot_no mismatch！");
                }

                //判断是否大于2个托盘的规格，如果大于就说明ocr4校验位置有问题，提示报警，让工人去调整
                if (bindingdata.OCR3List.Count > m_set.pallet_pack_maxqty * 2)
                {
                    BJ_PLC_HOME_th("OCR4校验位置有问题，请确认一下！" + msg);
                    AddErrorLog(true, "大于2个托盘的规格,OCR4校验位置有问题",
                      "批号：" + m_set.Batch + "   产品编号：" + m_set.ProductNo + "    CR4校验位置有问题    " + "  箱码： " + msg,
                      "批号：" + m_set.Batch + "   产品编号：" + m_set.ProductNo + "    CR4校验位置有问题    " + "  箱码： " + msg,
                      "批号：" + m_set.Batch + "   产品编号：" + m_set.ProductNo + "    CR4校验位置有问题    " + "  箱码： " + msg, false, m_set.ProductNo, m_set.Batch);

                }

                //判断是否重复读码

                if (bindingdata.OCR3List.Any(item => item.Allcode == msg))
                {
                    BJ_PLC_HOME_th("OCR3读取数据-重码" + msg);
                    AddErrorLog(true, "OCR3读取数据-重码", "OCR3读取数据-重码" + msg, "OCR3读取数据-重码" + msg, msg);
                    AddErrorLog4(true, "OCR3读取数据-重码", "OCR3读取数据-重码" + msg, "OCR3读取数据-重码" + msg, msg, true, m_set.ProductNo, m_set.Batch);
                    //return;
                    var data = bindingdata.OCR3List.Select(a => a).Where(a => a.Allcode == msg).First();
                    this.Invoke(new Action(() =>
                    {
                        lock (_lockObj)
                        {
                            bindingdata.OCR3List.Remove(data);
                        }
                    }));
                   
                }

                //校验本地其他托盘上是否有重码 by 2021.12.28 徐元丰
                string sqlTemp = "select pallet_no,box_barcode from qx_bundle with(nolock) where box_barcode='" + msg + "' and sku_no='" + m_set.ProductNo + "' and lot_no='" + m_set.Batch + "' and site_no='" + m_set.site_no + "'";
                dsTemp = DbHelperSQL.Query(sqlTemp);
                if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                {
                    ServiceReference2.WebService1SoapClient ws = new ServiceReference2.WebService1SoapClient("WebService1Soap1", Common.Config.ReadValue("LinkServer", "ws") + "/webservice1.asmx");
                    BJ_PLC_HOME_th("OCR3读取数据-重码其他托盘上已有该箱码" + msg);
                    AddErrorLog(true, "OCR3读取数据-重码", "OCR3读取数据-重码其他托盘上已有该箱码" + msg, "OCR3读取数据-重码其他托盘上已有该箱码", msg);
                    AddErrorLog4(true, "OCR3读取数据-重码", "OCR3读取数据-重码其他托盘上已有该箱码" + msg, "OCR3读取数据-重码其他托盘上已有该箱码", msg, true, m_set.ProductNo, m_set.Batch);
                    guidTemp = "ocr-" + Guid.NewGuid().ToString();
                    for (int i = 0; i < dsTemp.Tables[0].Rows.Count; i++)
                    {
                        try
                        {
                            sqlTemp = "update qx_bundle set box_barcode = '" + guidTemp + "' where pallet_no='" + dsTemp.Tables[0].Rows[i]["pallet_no"].ToString().Trim() + "' and box_barcode='" + dsTemp.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                            DbHelperSQL.ExecuteSql(sqlTemp);//更新本地 qx_bundle
                            string encsql = Common.MD5ALGO.Encrypt(sqlTemp);
                            ws.ExecuteSqlTran(encsql);//更新服务上qx_bundle的数据
                            sqlTemp = "update qx_inventory set box_barcode = '" + guidTemp + "' where pallet_no='" + dsTemp.Tables[0].Rows[i]["pallet_no"].ToString().Trim() + "' and box_barcode='" + dsTemp.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                            DbHelperSQL.ExecuteSql(sqlTemp);//更新本地 qx_inventory
                            encsql = Common.MD5ALGO.Encrypt(sqlTemp);
                            ws.ExecuteSqlTran(encsql);//更新服务上qx_inventory的数据
                        }
                        catch (Exception ex)
                        {
                            AddInfoLog("OCR3修正数据异常", "OCR3 Exception " + ex.Message + " " + ex.StackTrace);
                        }
                    }
                }

            OCR3_PB:
                {
                    try
                    {
                        bindingdata.OCR3List.Add(GetOCR2ViewDataClass(msg));


                        string intime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                        Application.DoEvents();
                        {
                            int ocr3 = bindingdata.OCR3List.Count;
                            ocr3Count.Text = "" + ocr3;
                            #region 显示ocr3读码率
                            lblOCR3ReadRate.Text = Common.Common.GetReadRate(OCR3ReadAmount, OCR3NoReadAmount);
                            #endregion
                            lb_xiang1.Text = ocr3Count.Text;
                        }
                        wait_pallet.Set();//启动组托线程
                        //即时插入本地数据库，用来预防产线断电引起的数据丢失 by 2021.11.10 徐元丰
                        //DbHelperSQL.ExecuteSql("insert into qx_OCR3_CacheData(doc_no,barcode,op_time) values('" + m_set.OrderNo + "','" + msg + "',getdate())");
                    }
                    catch (Exception ex)
                    {
                        AddErrorLog(true, "箱码OCR3异常", ex.Message, "OCR3 Exception", ex.Message + " " + ex.StackTrace);
                    }
                }
            }
        }
        public void OCR3_Counter_Process()
        {
            byte[] data = new byte[1024];
            while (true)
            {
                string stringdata = string.Empty;
                try
                {
                    try
                    {
                        int recv = ss3_counter.Receive(data);
                        stringdata = Encoding.UTF8.GetString(data, 0, recv);
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, "PLC OCR3计数PLC工作异常", e.Message, "PLC OCR3_Counter Exception", e.Message + " " + e.StackTrace);
                        Start_OCR3_Counter();
                        BJ_PLC_th("PLC OCR3计数PLC工作异常");//TODO
                        AddErrorLog4(true, "PLC OCR3计数PLC工作异常", e.Message, "PLC OCR3_Counter Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                        return;
                    }

                    string[] barcode = Regex.Split(stringdata, "%", RegexOptions.IgnoreCase);
                    if (barcode.Length > 0)
                    {
                        AddInfoLog("PLC OCR3_Counter whole Read=" + stringdata + "\r\n");
                        AloneLog.AddInfoLog("PLC OCR3_Counter", "PLC OCR3_Counter whole Read=" + stringdata + "\r\n");
                    }
                    int i;
                    string data_str = string.Empty;
                    for (i = 0; i < barcode.Length; i++)
                    {
                        data_str = barcode[i].Trim();
                        if (data_str != "")
                        {
                            string line_data = data_str.Split('#')[1];
                            OCR3_electric_eye_fired_amount = Convert.ToInt32(line_data.Split(',')[1].Trim());
                            AddInfoLog("PLC OCR3_Counter Read=" + data_str + "\r\n");
                            AloneLog.AddInfoLog("PLC OCR3_Counter", "PLC OCR3_Counter Read=" + data_str + "\r\n");
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "PLC OCR3_Counter工作异常", e.Message, "PLC OCR3_Counter Exception", e.Message + " " + e.StackTrace);
                    AloneLog.AddErrorLog("PLC OCR3_Counter", "PLC工作异常 " + e.Message + " " + e.StackTrace);
                    BJ_PLC_th("PLC OCR3_Counter工作异常");//TODO
                    continue;
                }
            }
        }
        #endregion

        #region OCR7
        public void OCR4_ScanBarcode()
        {
            string sql = "";
            byte[] data = new byte[1024];
            string stringdata = string.Empty;
            string ocrname = "OCR4";//注 只是改了界面上OCR的名字，让用户体验好点。实际上配置文件OCR3为组托数据，OCR7作为校验

            while (true)
            {
                try
                {
                    try
                    {
                        Common.Common.ReceiveOCRData(OCR7_ProcessBarcode, AddInfoLog, AloneLog.AddInfoLog, ss7, m_set.site_desc, m_set.ProduceLine, ref stringdata, "OCR7");
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, ocrname + "工作异常", e.Message, ocrname + " Exception", e.Message + " " + e.StackTrace);
                        refresh_group_Ocr(group_Ocr7, ocrname + "工位-异常", Color.Red);
                        BJ_PLC_HOME_th(ocrname + "工作异常");//TODO
                    ReConnectOCR:
                        {
                            try
                            {
                                CloseOCRorPLC1(myThread7, ss7, ocrname, "关闭" + ocrname, "关闭" + ocrname, "Stop Line Exception-" + ocrname);//l
                                Start_OCR4();
                            }
                            catch (Exception ex7)
                            {
                                AddErrorLog(true, ocrname + "工作异常", ex7.Message, ocrname + " Exception", ex7.Message + " " + ex7.StackTrace);
                                goto ReConnectOCR;
                            }
                            return;
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, ocrname + "工作异常", e.Message, ocrname + " Exception", e.Message + " " + e.StackTrace);
                    BJ_PLC_HOME_th(ocrname + "工作异常");//TODO
                    continue;
                }
            }
        }

        public void OCR7_ProcessBarcode(string msg)
        {
            string ocrname = "OCR4";//注 只是改了界面上OCR的名字，让用户体验好点。实际上配置文件OCR3为组托数据，OCR7作为校验

            //在线程里以安全方式调用控件
            if (lb_msg.InvokeRequired)
            {
                MyInvoke7 _myinvoke = new MyInvoke7(OCR7_ProcessBarcode);
                lb_msg.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                msg = msg.TrimStart('?').Trim();

                //道达尔箱码需要核对产品代码，批号，生产日期
                // m_set.ProductNo  m_set.ProductName m_set.Batch m_set.doc_date
                //A198319HD210170421200000877566A

                string[] mf = msg.Split('#');
                int fields = mf.Length;

                if (bindingdata.OCR3List.Any(a => a.Allcode == msg))//检查OCR3箱组里是否存在
                {
                    if (msg.Contains("NOREAD") || string.IsNullOrEmpty(msg) || msg == "0" || msg.Length != 31)
                    {
                        #region 报警程序 - 读码失败报警
                        lb_msg.Text = ocrname + "错误-二维码读取失败！";
                        AddErrorLog(true, ocrname + " 错误", "二维码读取失败! " + msg, ocrname + " Error", "QRCODE read failed! " + msg);
                        AddErrorLog4(true, ocrname + " 错误", "二维码读取失败! " + msg, ocrname + " Error", "QRCODE read failed! " + msg, true, m_set.ProductNo, m_set.Batch);
                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMRED%"));//TODO by 徐元丰 2021.6.23
                        AddInfoLog("发送开始报警HOMRED到PLC成功,消息：" + msg.ToString());
                        Thread.Sleep(100);
                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#CHERED%"));
                        AddInfoLog("发送开始报警CHERED到PLC成功,消息：" + msg.ToString());
                        Thread.Sleep(5000);//持续5秒钟
                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMRET%"));
                        AddInfoLog("发送开始报警HOMRET到PLC成功,消息：" + msg.ToString());
                        Thread.Sleep(100);
                        socket_PLC_Sender.Send(Encoding.Default.GetBytes("#CHERET%"));
                        AddInfoLog("发送开始报警CHERET到PLC成功,消息：" + msg.ToString());
                        if (msg.Contains("NOREAD"))
                        {
                            msg = "ocr-" + Guid.NewGuid().ToString();
                        }
                        #endregion
                    }
                }
                else
                {
                    lb_msg.Text = ocrname + "错误-二维码读取失败！校验码不存在";
                    AddErrorLog(true, "箱码" + ocrname + "校验异常", "校验码不存在" + msg, ocrname + " Error", "校验码不存在,QRCODE read failed!" + msg);

                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMRED%"));//TODO by 徐元丰 2021.6.23
                    AddInfoLog("发送开始报警HOMRED到PLC成功,消息：" + msg.ToString());
                    Thread.Sleep(100);
                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#CHERED%"));
                    AddInfoLog("发送开始报警CHERED到PLC成功,消息：" + msg.ToString());
                    Thread.Sleep(5000);//持续5秒钟
                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMRET%"));
                    AddInfoLog("发送开始报警HOMRET到PLC成功,消息：" + msg.ToString());
                    Thread.Sleep(100);
                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#CHERET%"));
                    AddInfoLog("发送开始报警CHERET到PLC成功,消息：" + msg.ToString());
                    msg = "ocr-" + Guid.NewGuid().ToString();
                }

                try
                {
                    CheckBoxPallet("OCR3", msg);//校验OCR3箱托
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "箱码" + ocrname + "校验异常", ex.Message, ocrname + " Exception", ex.Message + " " + ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// OCR7校验箱托
        /// <param name="check_ocr_number">需要校验的OCR名称</param>
        /// <param name="msg">箱码</param>
        /// </summary>
        private void CheckBoxPallet(string check_ocr_number, string msg)
        {
            //4L线 组箱动作开启
            if (m_set.ProduceLine == "4L")
            {
                makeBox_ResetEvent.Set();
            }

            lock (lock_boxReadTime_list)
            {
                List_BoxReadTime.Add(new BoxReadTime { box_barcode = msg, readTime = System.Environment.TickCount });
            }

            //界面显示更新和List队列更新
            AppendBoxOCRView("OCR7", msg);

            //"12B1L"规格有2种组托（52瓶一托；75瓶一托），根据这个来决定校验索引
            if (m_set.PackageSize != null && m_set.PackageSize.Contains("12B1L"))
            {
                if (m_set.pallet_pack_maxqty == 75)
                {
                    //重复的规格不能在数据库里添加配置，只能写在config.ini配置文件里用括号来区分
                    check_no = Convert.ToInt32(Common.Config.ReadValue(m_set.site_no, "12B1L(HAojne)"));
                }
            }

            //check_no = 2;//测试用TODO
            if (check_no > 0)
            {
                //校验组托
                //AppendBoxOCRViewCheckBox(check_ocr_number, msg, check_no, m_set.pallet_pack_maxqty);//校验，并修正数据
                AppendBoxOCRViewCheckBox2(check_ocr_number, msg, check_no, m_set.pallet_pack_maxqty);//校验，但是不修正数据 by 2022.2.10 徐元丰
            }
            else
            {
                BJ_PLC_HOME_th("箱码OCR4异常,没有设置校验索引");//TODO
                AddErrorLog(true, "箱码OCR4异常", "没有设置校验索引", "OCR4 Exception", "没有设置校验索引");
            }
        }

        #endregion

        #region 
        public void PLC_Process()
        {
            byte[] data = new byte[1024];
            while (true)
            {
                string stringdata = string.Empty;
                try
                {
                    try
                    {
                        int recv = socket_PLC.Receive(data);
                        stringdata = Encoding.UTF8.GetString(data, 0, recv);
                    }
                    catch (SocketException e)
                    {
                        AddErrorLog(true, "PLC工作异常", e.Message, "PLC Exception", e.Message + " " + e.StackTrace);
                        refresh_group_Ocr(group_PLC, "PLC工位-异常", Color.Red);
                        Start_PLC();
                        BJ_PLC_th("PLC工作异常");//TODO
                        AddErrorLog4(true, "PLC工作异常", e.Message, "PLC Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                        return;
                    }

                    string[] barcode = Regex.Split(stringdata, "%", RegexOptions.IgnoreCase);
                    if (barcode.Length > 0)
                    {
                        AddInfoLog("PLC whole Read=" + stringdata + "\r\n");
                        AloneLog.AddInfoLog("PLC", "PLC whole Read=" + stringdata + "\r\n");
                    }
                    int i;
                    string data_str = string.Empty;
                    for (i = 0; i < barcode.Length; i++)
                    {
                        data_str = barcode[i].Trim();
                        if (data_str != "")
                        {
                            //List_PLC.Add(data_str);

                            SavePLCFD(data_str);

                            AddInfoLog("PLC Read=" + data_str + "\r\n");
                            AloneLog.AddInfoLog("PLC", "PLC Read=" + data_str + "\r\n");
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "PLC工作异常", e.Message, "PLC Exception", e.Message + " " + e.StackTrace);
                    AloneLog.AddErrorLog("PLC", "PLC工作异常 " + e.Message + " " + e.StackTrace);
                    refresh_group_Ocr(group_PLC, "PLC工位-异常", Color.Red);
                    BJ_PLC_th("PLC工作异常");//TODO
                    AddErrorLog4(true, "PLC工作异常", e.Message, "PLC Exception", e.Message + " " + e.StackTrace, true, m_set.ProductNo, m_set.Batch);
                    continue;
                }
            }
        }
        #endregion

        #region 接受PLC分道数据
        public void PLC_ReceiveMsg()
        {
            int recv = 0;
            byte[] data = new byte[1024];
            string stringdata = string.Empty;
            while (true)
            {
                try
                {
                    if (socket_PLC_Sender.Connected != false)
                    {
                        data = new byte[1024];
                        recv = socket_PLC_Sender.Receive(data);
                        stringdata = Encoding.UTF8.GetString(data, 0, recv);
                        if (!stringdata.Contains("#s22%"))// by 2021.3.25 徐元丰
                        {
                            AddErrorLog(true, "PLC接收工作异常", "指令：" + stringdata, "PLC 接收Exception", "指令：" + stringdata);
                        }
                    }
                }
                catch (SocketException e)
                {
                    AddErrorLog(true, "PLC接收工作异常", e.Message, "PLC 接收Exception", e.Message + " " + e.StackTrace);
                    continue;
                }
            }
        }
        #endregion


        string Tuo = Guid.NewGuid().ToString();
        #endregion

        #region PLC报警
        public void BJ_PLC_th(string msg)
        {
            Thread td = new Thread(new ParameterizedThreadStart(BJ_PLC));
            td.Start(msg);
        }
        public void BJ_Stop_PLC_th()
        {
            Thread td = new Thread(new ThreadStart(BJ_Stop_PLC));
            td.Start();
        }

        /// <summary>
        /// 发送报警信号给plc
        /// </summary>
        void BJ_PLC(object msg)
        {
            try
            {
                ui_settext_Label(lb_msg, msg.ToString());
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOTRED%"));
                AddInfoLog("发送开始报警信息BOTRED到PLC成功,消息：" + msg.ToString());
                Thread.Sleep(5000);//持续5秒钟
                BJ_Stop_PLC();
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "发送开始报警信号到PLC失败", ex.Message + " " + ex.StackTrace, "发送开始报警信号到PLC失败", ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 发送吧报警信号给plc
        /// </summary>
        void BJ_Stop_PLC()
        {
            try
            {
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOTRET%"));
                AddInfoLog("发送停止报警信息BOTRET到PLC成功");
                AloneLog.AddInfoLog("PLC", "发送停止报警信息到PLC成功");
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "发送停止报警信号到PLC失败", ex.Message + " " + ex.StackTrace, "发送停止报警信号到PLC失败", ex.Message + " " + ex.StackTrace);
                AloneLog.AddErrorLog("PLC", "发送停止报警信号到PLC失败 " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void BJ_PLC_OCR_th(string msg)
        {
            Thread td = new Thread(new ParameterizedThreadStart(BJ_PLC_OCR));
            td.Start(msg);
        }

        void BJ_PLC_OCR(object msg)
        {
            try
            {
                ui_settext_Label(lb_msg, msg.ToString());
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOXRED%"));
                AddInfoLog("发送开始报警信息BOXRED到PLC成功,消息：" + msg.ToString());
                Thread.Sleep(5000);//持续5秒钟
                BJ_Stop_PLC_OCR();
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "发送开始报警信号到PLC失败", ex.Message + " " + ex.StackTrace, "发送开始报警信号到PLC失败", ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 发送吧报警信号给plc
        /// </summary>
        void BJ_Stop_PLC_OCR()
        {
            try
            {
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#BOXRET%"));
                AddInfoLog("发送停止报警信息BOXRET到PLC成功");
                AloneLog.AddInfoLog("PLC", "发送停止报警信息到PLC成功");
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "发送停止报警信号到PLC失败", ex.Message + " " + ex.StackTrace, "发送停止报警信号到PLC失败", ex.Message + " " + ex.StackTrace);
                AloneLog.AddErrorLog("PLC", "发送停止报警信号到PLC失败 " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void BJ_PLC_HOME_th(string msg)
        {
            Thread td = new Thread(new ParameterizedThreadStart(BJ_PLC_HOME));
            td.Start(msg);
        }

        void BJ_PLC_HOME(object msg)
        {
            try
            {
                ui_settext_Label(lb_msg, msg.ToString());
                Thread.Sleep(100);
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMRED%"));//TODO by 徐元丰 2021.6.23
                AddInfoLog("发送开始报警HOMRED到PLC成功,消息：" + msg.ToString());
                Thread.Sleep(10000);//持续10秒钟
                BJ_Stop_PLC_HOME();
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "发送开始报警信号到PLC失败", ex.Message + " " + ex.StackTrace, "发送开始报警信号到PLC失败", ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 发送吧报警信号给plc
        /// </summary>
        void BJ_Stop_PLC_HOME()
        {
            try
            {
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMRET%"));
                AddInfoLog("发送开始报警HOMRET到PLC成功");
                AloneLog.AddInfoLog("PLC", "发送停止报警信息HOMRET到PLC成功");
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "发送停止报警信号HOMRET到PLC失败", ex.Message + " " + ex.StackTrace, "发送停止报警信号HOMRET到PLC失败", ex.Message + " " + ex.StackTrace);
                AloneLog.AddErrorLog("PLC", "发送停止报警信号HOMRET到PLC失败 " + ex.Message + " " + ex.StackTrace);
            }
        }

        #endregion

        #region 其他
        /// <summary>
        /// 检查更新系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Update_Click(object sender, EventArgs e) //作废代码
        {
            try
            {
                //1.从服务器下载配置文件，知道是否有新版本
                string url = @"http://wms.valvoline.com.cn/AutomaticUpdate/GKJ/update.xml";
                string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + @"\update\update.xml";
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
                //如果程序版本小于服务器版本，则需要更新
                if (Convert.ToInt32(Operation.Version.Replace(".", "")) < Convert.ToInt32(newVersion.Replace(".", "")))
                {
                    // 取消提示是否升级，让客户必须更新才能使用
                    if (DialogResult.Yes == MessageBox.Show("已检测有新版本，是否升级？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1))
                    {
                        System.Diagnostics.Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + @"\update\AutoUpdate.exe", null);
                        Application.Exit();
                        KillProcess.KillPro();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("已更新至最新版本", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("检查更新失败", "系统提示");
            }
        }

        /// <summary>
        /// 系统最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Small_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// 获取工单数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_getOrder_Click(object sender, EventArgs e)
        {
            FormQueryDoc fm = new FormQueryDoc();
            DialogResult dr = fm.ShowDialog();

            if (dr == DialogResult.OK)
            {
                tDocno.Text = m_set.OrderNo;

                tDocInfo.Text = "产品: " + m_set.ProductNo + "," + m_set.ProductName + ",产地=" + m_set.Madein + "\r\n" + "批号: " + m_set.Batch + ",日期: " + m_set.doc_date
                    + ", 数量: " + m_set.act_qty + "/" + m_set.req_qty + "\r\n 外箱: " + m_set.box_pack_maxqty + ", 托盘: " + m_set.pallet_pack_maxqty + ", 关联: " + m_set.pack_relation;

                AddInfoLog("搜索生产任务", "doc_no=" + m_set.OrderNo + ",doc_date=" + m_set.doc_date + ",Madein=" + m_set.Madein + ",sku_no=" + m_set.ProductNo + ",lot_no=" + m_set.Batch + ",qty=" + m_set.act_qty + "/" + m_set.req_qty);
            }

            if (tDocno.Text != "")
            {
                btn_Start.Enabled = true;
            }
            else
            {
                btn_Start.Enabled = false;
            }
        }
        #endregion


        #region 记录日志 数据库和本地文件、日志显示列表

        public void AddInfoLog(string Logs)
        {
            if (Common.Common.m_log > 0)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Info.txt", "[" + Common.Common.GetNow1() + "]" + Logs + "\r\n");
            }
        }
        public void AddChannelLog(object Logs)
        {
            if (Common.Common.m_log > 0)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_channel.txt", "[" + Common.Common.GetNow1() + "]" + Logs.ToString() + "\r\n");
            }
        }

        /// <summary>
        /// 显示出来的日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Logs"></param>
        public void AddInfoLog(string title, string Logs)
        {
            ShowLog(title, Logs);
            if (Common.Common.m_log > 0)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Info.txt", "[" + Common.Common.GetNow1() + "]" + title + Logs + "\r\n");
            }
        }

        public void operationAddInfoLog(string Logs)
        {
            if (Common.Common.m_log > 0)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_operationInfo.txt", "[" + Common.Common.GetNow1() + "]" + Logs + "\r\n");
            }
        }
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Logs"></param>
        public void AddDebugLog(string Logs)
        {
            if (Common.Common.m_log > 0 && Common.Common.m_log_debug > 0)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Debug.txt", "[" + Common.Common.GetNow1() + "]" + Logs + "\r\n");
            }
        }

        /// <summary>
        /// 增加错误日志
        /// </summary>
        /// <param name="ProduceLine"></param>
        /// <param name="Location"></param>
        /// <param name="Logs"></param>
        public void AddErrorLog(bool isError, string title, string Logs, string title_en, string Logs_en, bool writeToDataBase = false, string sku_no_barcode = "", string lot_no_barcode = "")
        {
            string strSql = string.Empty;
            try
            {
                ShowLog(title, Logs);
                ShowLog2(title, Logs);
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Error.txt", "[" + Common.Common.GetNow1() + "]" + title_en + Logs_en + "\r\n");
                if (writeToDataBase)
                {
                    strSql = "insert into qx_operation([op_time],[op_user] ,[subject],[operation],[site_no],[site_desc]"
                        + " ,[op_PDA_no],[log_level] ,[server_sync],[server_sync_time],key1) values(getdate(),'" + m_set.UserName + "','" + title + "','"
                        + Logs + "','" + m_set.site_no + "','" + m_set.site_desc + "','','8',0,getdate(),'" + m_set.ProduceLine + "')";
                    DbHelperSQL.ExecuteSql(strSql);
                }
            }
            catch (Exception ex)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Error.txt", "[" + Common.Common.GetNow1() + "]" + ex.Message + ex.StackTrace + strSql + "\r\n");
            }
        }

        #region 新增异常信息  20221215

        private void ShowLog3(string title, string Logs)
        {
            if (lvLog3.InvokeRequired)
            {
                lvLog3.BeginInvoke(new myDelegate1(ShowLog3), new object[] { title, Logs });
            }
            else
            {
                try
                {
                    string[] s = new string[10];
                    s[0] = Common.Common.GetNow1();
                    s[1] = title;
                    s[2] = Logs;
                    if (lvLog3.Items.Count >= 500) //超过500条自动清空
                        lvLog3.Items.Clear();
                    ListViewItem lvi = new ListViewItem(s);
                    lvLog3.Items.Add(lvi);
                    this.lvLog3.EnsureVisible(this.lvLog3.Items.Count - 1);
                    this.lvLog3.Items[this.lvLog3.Items.Count - 1].Selected = true;
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "更新显示日志异常", ex.Message, "show log3 exception", ex.Message + "," + ex.StackTrace);
                }
            }
        }

        private void ShowLog4(string title, string Logs)
        {
            if (lvLog4.InvokeRequired)
            {
                lvLog4.BeginInvoke(new myDelegate1(ShowLog4), new object[] { title, Logs });
            }
            else
            {
                try
                {
                    string[] s = new string[10];
                    s[0] = Common.Common.GetNow1();
                    s[1] = title;
                    s[2] = Logs;
                    if (lvLog4.Items.Count >= 1000) //超过500条自动清空
                        lvLog4.Items.Clear();
                    ListViewItem lvi = new ListViewItem(s);
                    lvLog4.Items.Add(lvi);
                    this.lvLog4.EnsureVisible(this.lvLog4.Items.Count - 1);
                    this.lvLog4.Items[this.lvLog4.Items.Count - 1].Selected = true;
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "更新显示日志异常", ex.Message, "show log4 exception", ex.Message + "," + ex.StackTrace);
                }
            }
        }
        public void AddErrorLog4(bool isError, string title, string Logs, string title_en, string Logs_en, bool writeToDataBase = true, string sku_no_barcode = "", string lot_no_barcode = "")
        {
            string strSql = string.Empty;
            try
            {
                ShowLog3(title, Logs);
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_" + m_set.ProductNo + "_" + m_set.Batch + "_异常信息_Error.txt", "[" + Common.Common.GetNow1() + "]" + title_en + Logs_en + "\r\n");
                if (writeToDataBase)
                {
                    strSql = "insert into qx_operation([op_time],[op_user] ,[subject],[operation],[site_no],[site_desc]"
                        + " ,[op_PDA_no],[log_level] ,[server_sync],[server_sync_time],key1,key2,key3,key4) values(getdate(),'" + m_set.UserName + "','" + title + "','"
                        + Logs + "','" + m_set.site_no + "','" + m_set.site_desc + "','','8',0,getdate(),'" + m_set.ProduceLine + "','" + sku_no_barcode + "','" + lot_no_barcode + "','" + m_set.ProduceDate + "')";
                    DbHelperSQL.ExecuteSql(strSql);
                }
            }
            catch (Exception ex)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_" + m_set.ProductNo + "_" + m_set.Batch + "_异常信息_Error.txt", "[" + Common.Common.GetNow1() + "]" + ex.Message + ex.StackTrace + strSql + "\r\n");
            }
        }


        public void AddErrorLog5(bool isError, string title, string Logs, string title_en, string Logs_en, bool writeToDataBase = false)
        {
            string strSql = string.Empty;
            try
            {
                ShowLog4(title, Logs);
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_" + m_set.ProductNo + "_" + m_set.Batch + "_心跳包异常_Error.txt", "[" + Common.Common.GetNow1() + "]" + title_en + Logs_en + "\r\n");
                if (writeToDataBase)
                {
                    strSql = "insert into qx_operation([op_time],[op_user] ,[subject],[operation],[site_no],[site_desc]"
                        + " ,[op_PDA_no],[log_level] ,[server_sync],[server_sync_time],key1) values(getdate(),'" + m_set.UserName + "','" + title + "','"
                        + Logs + "','" + m_set.site_no + "','" + m_set.site_desc + "','','8',0,getdate(),'" + m_set.ProduceLine + "')";
                    DbHelperSQL.ExecuteSql(strSql);
                }
            }
            catch (Exception ex)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_" + m_set.ProductNo + "_" + m_set.Batch + "_心跳包异常_Error.txt", "[" + Common.Common.GetNow1() + "]" + ex.Message + ex.StackTrace + strSql + "\r\n");
            }
        }



        private void MakedBox_List_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = view_makedBox.CurrentRow.Index;
            string strNumLast = "A";
            FormInputBoxOCR2 fm = new FormInputBoxOCR2();
            FormInputBoxOCR2.m_text = "选中的箱码：" + MakedBox_List[index].ToString();
            FormInputBoxOCR2.m_result = "";
            DialogResult dr = fm.ShowDialog(this);
            if (dr == DialogResult.Yes)
            {
                FormInputBoxOCR2.m_text = "选中的箱码：" + MakedBox_List[index].ToString() + "。如要修改，请输入9位箱码（不符合要求，不能修改）";
                string tempBoxCode = string.Empty;
                if (Regex.IsMatch(FormInputBoxOCR2.m_result.Trim(), @"^[+-]?\d*[.]?\d*$"))
                {
                    if (MakedBox_List[index].ToString().Trim().Substring(0, 3) == "ocr")
                    {
                        for (int i = ZJ_LN.Length; i < 3; i++)
                        {
                            if (ZJ_LN.Length < 3)
                            {
                                ZJ_LN = ZJ_LN + "0";
                            }
                        }
                        if (m_set.ProduceLine == "4L")
                        {
                            //只有镇江工厂的4L产品才是A,其余工厂都是D
                            strNumLast = "A";
                        }
                        if (m_set.ProduceLine == "1L")
                        {
                            strNumLast = "D";
                        }
                        if (m_set.ProduceLine == "大标签")
                        {
                            strNumLast = "B";
                        }
                        if (Common.Config.ReadValue("Line", "type").Trim().Length == 0)
                        {
                            tempBoxCode = "A" + m_set.ProductNo + m_set.Batch + m_set.ProduceDate.Substring(2, 2) + m_set.ProduceDate.Substring(5, 2) + m_set.ProduceDate.Substring(8, 2) + ZJ_LN + FormInputBoxOCR2.m_result.Trim() + strNumLast;//镇江开头为A；天津为B；广州为C；
                        }
                        else
                        {
                            tempBoxCode = Common.Config.ReadValue("Line", "type") + m_set.ProductNo + m_set.Batch + m_set.ProduceDate.Substring(2, 2) + m_set.ProduceDate.Substring(5, 2) + m_set.ProduceDate.Substring(8, 2) + ZJ_LN + FormInputBoxOCR2.m_result.Trim() + strNumLast;//镇江开头为A；天津为B；广州为C；
                        }
                    }
                    else
                    {
                        tempBoxCode = MakedBox_List[index].ToString().Trim().Substring(0, 21) + FormInputBoxOCR2.m_result.Trim() + MakedBox_List[index].ToString().Trim().Substring(MakedBox_List[index].ToString().Length - 1, 1);
                    }

                    if (MakedBox_List.IndexOf(tempBoxCode.Trim()) > -1)
                    {
                        MessageBox.Show("您输入的箱码重复了，请再次核对一下要修改的箱码！");
                        return;
                    }
                    else
                    {
                        if (Common.Common.TS("确认要把" + MakedBox_List[index].ToString() + "数据修改成" + tempBoxCode.Trim() + "吗？"))
                        {
                            AddInfoLog("修改箱子（软件）OCR2", "OCR2 箱码：" + MakedBox_List[index].ToString() + " OCR2 改成：" + tempBoxCode.Trim());
                            AddErrorLog4(true, "修改箱子（软件）OCR2", "OCR2 改成：" + tempBoxCode.Trim(), " OCR2 箱码：" + MakedBox_List[index].ToString() + "  OCR2 改成：" + tempBoxCode.Trim(), "     OCR2 已改成的箱码：" + tempBoxCode.Trim(), true, m_set.ProductNo, m_set.Batch);
                            MakedBox_List[index] = tempBoxCode.Trim();

                        }
                    }
                }
                else
                {
                    MessageBox.Show("您输入的箱码有误，请检查一下！");
                    return;
                }
            }
            else if (dr == DialogResult.No)
            {
                MessageBox.Show("修改失败，填写的箱码格式有错误！");
                return;
            }
            else
            {
                return;
            }
        }


        public void reload_ocr2list2()
        {
            try
            {
                if (view_makedBox.InvokeRequired)
                {
                    myDelegate_reload_ocr2list _myinvoke = new myDelegate_reload_ocr2list(reload_ocr2list2);
                    view_makedBox.Invoke(_myinvoke, new object[] { });
                }
                else
                {
                    view_makedBox.Rows.Clear();
                    int i;
                    object[] objvalues = new object[2];

                    for (i = 0; i < MakedBox_List.Count; i++)
                    {
                        objvalues[0] = MakedBox_List[i].Substring(21, 9);
                        if (MakedBox_List[i].Substring(0, 4).ToLower() == "ocr-")//万能码
                        {
                            objvalues[0] = "NOREAD";
                        }

                        objvalues[1] = MakedBox_List[i];
                        this.view_makedBox.Rows.Add(objvalues);
                    }

                    //if (view_makedBox.Rows.Count > 0)
                    //{
                    //    view_makedBox.Rows[view_makedBox.Rows.Count - 1].Selected = true;               //设置为选中. 
                    //    view_makedBox.FirstDisplayedScrollingRowIndex = view_makedBox.Rows.Count - 1;   //设置第一行显示
                    //}
                    //view_makedBox.Focus();//保证滑动条永远处于最新一条数据位置
                }
            }
            catch (Exception ex)
            {
                AddErrorLog2("reload view", ex.Message + "," + ex.StackTrace);
            }
        }

        #endregion

        /// <summary>
        /// 增加错误日志，上传错误日志
        /// </summary>
        /// <param name="ProduceLine"></param>
        /// <param name="Location"></param>
        /// <param name="Logs"></param>
        public void AddErrorLog3(bool isError, string title, string Logs, string title_en, string Logs_en, bool writeToDataBase = true, string sku_no_barcode = "", string lot_no_barcode = "")
        {
            string strSql = string.Empty;
            try
            {
                ShowLog(title, Logs);
                ShowLog2(title, Logs);

                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_ Error.txt", "[" + Common.Common.GetNow1() + "]" + title_en + Logs_en + "\r\n");
                if (writeToDataBase)
                {
                    strSql = "insert into qx_operation([op_time],[op_user] ,[subject],[operation],[site_no],[site_desc]"
                        + " ,[op_PDA_no],[log_level] ,[server_sync],[server_sync_time],key1,key2,key3,key4) values(getdate(),'" + m_set.UserName + "','" + title + "','"
                        + Logs + "','" + m_set.site_no + "','" + m_set.site_desc + "','','8',0,getdate(),'" + m_set.ProduceLine + "','" + sku_no_barcode + "','" + lot_no_barcode + "','" + m_set.ProduceDate + "')";
                    DbHelperSQL.ExecuteSql(strSql);
                }
            }
            catch (Exception ex)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_OCR4校验位置Error.txt", "[" + Common.Common.GetNow1() + "]" + ex.Message + ex.StackTrace + strSql + "\r\n");
            }
        }

        #endregion      

        //记录日志 数据库和本地文件、不显示在日志显示列表
        public void AddErrorLog2(string title, string Logs)//不显示在listview
        {
            try
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Error.txt", "[" + Common.Common.GetNow1() + "]" + title + Logs + "\r\n");
                string strSql = "insert into qx_operation([op_time],[op_user] ,[subject],[operation],[site_no],[site_desc]"
                    + " ,[op_PDA_no],[log_level] ) values(getdate(),'" + m_set.UserName + "','" + title + "','"
                    + Logs + "','" + m_set.site_no + "','" + m_set.site_desc + "','','8')";
                DbHelperSQL.ExecuteSql(strSql);
            }
            catch (Exception ex)
            {
                Common.Common.AppendStringToFile("mylog[" + DateTime.Now.ToString("yyyy-MM-dd") + "]_Error.txt", "[" + Common.Common.GetNow1() + "]" + ex.Message + ex.StackTrace + "\r\n");
            }
        }

        private delegate void myDelegate1(string title, string Logs);//定义委托
        private delegate void myDelegate2();//定义委托
        private delegate void myDelegate3(object obj);//定义委托
        private delegate void myDelegate4(GroupBox gb, string text, Color color);//定义委托 group_OCR1
        private delegate void myDelegate_refreshqty1();//定义委托
        private delegate void myDelegate_refreshqty2();//定义委托
        private delegate void myDelegate_setitemcount(int value);//定义委托
        private delegate void myDelegate_view_pallet_no_gv();//定义委托
        private delegate void myDelegate_view_pallet_box_gv();//定义委托

        private delegate void myDelegate_viewtongremoveat(object index);//定义委托
        private delegate void myDelegate_viewch1add(object s);//定义委托
        private delegate void myDelegate_viewch2add(object s);//定义委托
        private delegate void myDelegate_viewch3add(object s);//定义委托
        private delegate void myDelegate_viewch4add(object s);//定义委托
        private delegate void myDelegate_viewch5add(object s);//定义委托
        private delegate void myDelegate_viewch6add(object s);//定义委托
        private delegate void myDelegate_viewmakedboxadd(string s1);//定义委托

        private delegate void myDelegate_reload_all_channel_list();//定义委托
        private delegate void myDelegate_reload_ocr1list();//定义委托
        private delegate void myDelegate_reload_ocr2list();//定义委托
        private delegate void myDelegate_reload_ocr3list();//定义委托

        private void ShowLog(string title, string Logs)
        {
            if (lvLog.InvokeRequired)
            {
                lvLog.BeginInvoke(new myDelegate1(ShowLog), new object[] { title, Logs });
            }
            else
            {
                try
                {
                    string[] s = new string[10];
                    s[0] = Common.Common.GetNow1();
                    s[1] = title;
                    s[2] = Logs;
                    if (lvLog.Items.Count >= 100) //超过500条自动清空
                        lvLog.Items.Clear();
                    ListViewItem lvi = new ListViewItem(s);
                    lvLog.Items.Add(lvi);
                    this.lvLog.EnsureVisible(this.lvLog.Items.Count - 1);
                    this.lvLog.Items[this.lvLog.Items.Count - 1].Selected = true;
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "更新显示日志异常", ex.Message, "show log exception", ex.Message + "," + ex.StackTrace);
                }
            }
        }

        private void ShowLog2(string title, string Logs)
        {
            if (lvLog2.InvokeRequired)
            {
                lvLog2.BeginInvoke(new myDelegate1(ShowLog2), new object[] { title, Logs });
            }
            else
            {
                try
                {
                    string[] s = new string[10];
                    s[0] = Common.Common.GetNow1();
                    s[1] = title;
                    s[2] = Logs;
                    if (lvLog2.Items.Count >= 500) //超过500条自动清空
                        lvLog2.Items.Clear();
                    ListViewItem lvi = new ListViewItem(s);
                    lvLog2.Items.Add(lvi);
                    this.lvLog2.EnsureVisible(this.lvLog2.Items.Count - 1);
                    this.lvLog2.Items[this.lvLog2.Items.Count - 1].Selected = true;
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "更新显示日志异常", ex.Message, "show log2 exception", ex.Message + "," + ex.StackTrace);
                }
            }
        }

        int i = 1;
        #region 默认开启线程
        bool m_dont_touch_ocrlist = false;

        void th_CheckPallet_4L()
        {
            item_List = new List<string>();//OCR1
            box_List = new List<string>();//OCR2
            while (true)
            {
                Thread.Sleep(3000);
                if (bindingdata.OCR3List.Count >= m_set.pallet_pack_maxqty)
                {
                    #region 误触发检测
                    string subject = "";
                    string content = "";
                    List<PassBarcodeInfo> li_passBoxBarcode = Common.Common.GetNeedPassErrorFireBoxBarcode(List_BoxReadTime, m_set.pallet_pack_maxqty + 1, box_barcode_fire_pass_time, lock_boxReadTime_list);
                    if (li_passBoxBarcode.Count > 0)
                    {
                        foreach (PassBarcodeInfo item1 in li_passBoxBarcode)
                        {
                            if (bindingdata.OCR3List.Any(a => a.Allcode == item1.PassBarcode))
                            {
                                var data = bindingdata.OCR3List.Select(a => a).Where(a => a.Allcode == item1.PassBarcode).First();

                                this.Invoke(new Action(() =>
                                {
                                    lock (_lockObj)
                                    {
                                        bindingdata.OCR3List.Remove(data);
                                    }
                                }));
                                AddInfoLog("箱码误触发", " 被剔除的码：" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode);
                                subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "箱码误触发";
                                content = "误触发箱码:" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode + ",误触发剔除时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                Common.Common.SendMail_Thread(subject, content);
                            }
                        }

                        if (bindingdata.OCR3List.Count < m_set.pallet_pack_maxqty)
                        {
                            continue;
                        }
                    }
                    #endregion

                    item_List.Clear();
                    box_List.Clear();
                    for (int i = 0; i < m_set.pallet_pack_maxqty; i++)
                    {
                        string box_barcode = bindingdata.OCR3List[0].Allcode;
                        this.Invoke(new Action(() =>
                        {
                            lock (_lockObj)
                            {
                                bindingdata.OCR3List.RemoveAt(0);
                            }
                        }));

                        box_List.Add(box_barcode);
                        //MakedBox_List.Remove(box_barcode);
                        List<BoxAndItemBundle> li_res = bindingdata.Box_Item_List.Where(p => p.box_barcode == box_barcode).ToList();
                        //在瓶箱绑定关系表中没有找到入托箱码，则补万能码
                        if (li_res == null || li_res.Count == 0)
                        {
                            for (int x = 0; x < m_set.box_pack_maxqty; x++)
                            {
                                item_List.Add("nof-" + Guid.NewGuid().ToString());
                            }
                        }
                        else
                        {
                            int addedCount = 0;
                            foreach (BoxAndItemBundle box_item in li_res)
                            {
                                if (addedCount >= m_set.box_pack_maxqty)
                                    break;
                                else
                                {
                                    item_List.Add(box_item.item_barcode);
                                    addedCount++;
                                }
                             
                                //删除缓存中的瓶箱数据 
                                 redisHelper_ocr1.RemoveOCR1ViewDataByKey("box_item_list:" + box_item, m_set.OrderNo);

                            }
                        }

                    }

                    //reload_makedBox_list();

                    //开启组托线程
                    //Thread td = new Thread(new ParameterizedThreadStart(th_MakePallet));
                    //string pallet_no = Guid.NewGuid().ToString();
                    //td.Start(false, pallet_no);

                    //瓶箱关联变量清零
                    ocr3_counts = bindingdata.OCR3List.Count;
                }
            }
        }



        void th_CheckPallet_4L_Manual()
        {
            AddInfoLog("强制组托", "强制组托, box_qty=" + bindingdata.OCR3List.Count.ToString() + "\r\n");
            item_List = new List<string>();//OCR1
            box_List = new List<string>();//OCR2

            if (bindingdata.OCR3List.Count == 0)
            {
                AddInfoLog("没有可组托的数据");
                forceMakePallet_ResetEvent.Set();
                return;
            }

            #region 误触发检测
            string subject = "";
            string content = "";
            List<PassBarcodeInfo> li_passBoxBarcode = Common.Common.GetNeedPassErrorFireBoxBarcode(List_BoxReadTime, m_set.pallet_pack_maxqty + 1, box_barcode_fire_pass_time, lock_boxReadTime_list);
            if (li_passBoxBarcode.Count > 0)
            {
                foreach (PassBarcodeInfo item1 in li_passBoxBarcode)
                {
                    if (bindingdata.OCR3List.Any(a => a.Allcode == item1.PassBarcode))
                    {
                        var data = bindingdata.OCR3List.Select(a => a).Where(a => a.Allcode == item1.PassBarcode).First();

                        this.Invoke(new Action(() =>
                        {
                            lock (_lockObj)
                            {
                                bindingdata.OCR3List.Remove(data);
                            }
                        }));
                        AddInfoLog("箱码误触发", " 被剔除的码：" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode);
                        subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "箱码误触发";
                        content = "误触发箱码:" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode + ",误触发剔除时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        Common.Common.SendMail_Thread(subject, content);
                    }
                }

            }
            #endregion

            item_List.Clear();
            box_List.Clear();
            for (int i = 0; i < bindingdata.OCR3List.Count; i++)
            {
                string box_barcode = bindingdata.OCR3List[i].Allcode;
                //MakedBox_List.RemoveAt(0);
                box_List.Add(box_barcode);

                List<BoxAndItemBundle> li_res = bindingdata.Box_Item_List.Where(p => p.box_barcode == box_barcode).ToList();
                //在瓶箱绑定关系表中没有找到入托箱码，则补万能码
                if (li_res == null || li_res.Count == 0)
                {
                    for (int x = 0; x < m_set.box_pack_maxqty; x++)
                    {
                        item_List.Add("nof-" + Guid.NewGuid().ToString());
                    }
                }
                else
                {
                    int addedCount = 0;
                    foreach (BoxAndItemBundle box_item in li_res)
                    {
                        if (addedCount >= m_set.box_pack_maxqty)
                            break;
                        else
                        {
                            item_List.Add(box_item.item_barcode);
                            addedCount++;
                        }
                 
                        redisHelper_ocr1.RemoveOCR1ViewDataByKey("box_item_list:" + box_item, m_set.OrderNo);
                    }
                }

            }
            redisHelper_ocr1.RemoveOCR1ViewDataByKey("OCR3List", m_set.OrderNo);
            bindingdata.OCR3List.Clear();


            //开启组托线程
            //Thread td = new Thread(new ParameterizedThreadStart(th_MakePallet));
            //td.Start(true);
        }

        void th_CheckPallet_1L_Manual()
        {
            //MakedBox_List替换成OCR3_List 注1L的强制组托根据OCR3的数据来
            AddInfoLog("强制组托", "强制组托, box_qty=" + bindingdata.OCR3List.Count.ToString() + "\r\n");
            item_List = new List<string>();//OCR1
            box_List = new List<string>();//OCR2

            if (bindingdata.OCR3List.Count == 0)
            {
                AddInfoLog("没有可组托的数据");
                forceMakePallet_ResetEvent.Set();
                return;
            }

            #region 误触发检测
            string subject = "";
            string content = "";
            List<PassBarcodeInfo> li_passBoxBarcode = Common.Common.GetNeedPassErrorFireBoxBarcode(List_BoxReadTime, m_set.pallet_pack_maxqty + 1, box_barcode_fire_pass_time, lock_boxReadTime_list, false);
            if (li_passBoxBarcode.Count > 0)
            {
                foreach (PassBarcodeInfo item1 in li_passBoxBarcode)
                {
                    if (bindingdata.OCR3List.Any(a => a.Allcode == item1.PassBarcode))
                    {
                        var data = bindingdata.OCR3List.Select(a => a).Where(a => a.Allcode == item1.PassBarcode).First();

                        this.Invoke(new Action(() =>
                        {
                            lock (_lockObj)
                            {
                                bindingdata.OCR3List.Remove(data);
                            }
                        }));
                        AddInfoLog("箱码误触发", "被剔除的码：" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode);
                        subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "箱码误触发";
                        content = "误触发箱码:" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode + ",误触发剔除时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        Common.Common.SendMail_Thread(subject, content);
                    }
                }

            }
            #endregion

            item_List.Clear();
            box_List.Clear();
            for (int i = 0; i < bindingdata.OCR3List.Count; i++)
            {
                string box_barcode = bindingdata.OCR3List[i].Allcode;
                //MakedBox_List.RemoveAt(0);
                box_List.Add(box_barcode);
                List<BoxAndItemBundle> li_res = bindingdata.Box_Item_List.Where(p => p.box_barcode == box_barcode).ToList();
                //在瓶箱绑定关系表中没有找到入托箱码，则补万能码
                if (li_res == null || li_res.Count == 0)
                {
                    for (int x = 0; x < m_set.box_pack_maxqty; x++)
                    {
                        item_List.Add("nof-" + Guid.NewGuid().ToString());
                    }
                }
                else
                {
                    int addedCount = 0;
                    foreach (BoxAndItemBundle box_item in li_res)
                    {
                        if (addedCount >= m_set.box_pack_maxqty)
                            break;
                        else
                        {
                            item_List.Add(box_item.item_barcode);
                            addedCount++;
                        }
                    
                        redisHelper_ocr1.RemoveOCR1ViewDataByKey("box_item_list:" + box_item, m_set.OrderNo);
                    }
                }

            }
            redisHelper_ocr1.RemoveOCR1ViewDataByKey("OCR3List", m_set.OrderNo);
            bindingdata.OCR3List.Clear();

            //开启组托线程
            //Thread td = new Thread(new ParameterizedThreadStart(th_MakePallet));
            //td.Start(true);
        }
        async void th_CheckPallet2()
        {
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;

                    if (bindingdata.OCR3List.Count < m_set.pallet_pack_maxqty)
                        wait_pallet.Reset();//重置状态

                    wait_pallet.WaitOne();
                    if (bindingdata.OCR3List.Count >= m_set.pallet_pack_maxqty)
                    {
                        //组托任务进度条计数
                      
                      
                        #region 误触发检测
                        string subject = "";
                        string content = "";
                        List<PassBarcodeInfo> li_passBoxBarcode = Common.Common.GetNeedPassErrorFireBoxBarcode(List_BoxReadTime, m_set.pallet_pack_maxqty, box_barcode_fire_pass_time, lock_boxReadTime_list);
                        if (li_passBoxBarcode.Count > 0)
                        {
                            foreach (PassBarcodeInfo item1 in li_passBoxBarcode)
                            {
                                if (bindingdata.OCR3List.Any(a => a.Allcode == item1.PassBarcode))
                                {
                                    var data = bindingdata.OCR3List.Select(a => a).Where(a => a.Allcode == item1.PassBarcode).First();

                                    this.Invoke(new Action(() =>
                                    {
                                        lock (_lockObj)
                                        {
                                            bindingdata.OCR3List.Remove(data);
                                        }
                                    }));
                                    AddInfoLog("箱码误触发", " 被剔除的码：" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode);
                                    subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "箱码误触发";
                                    content = "误触发箱码:" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode + ",误触发剔除时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    Common.Common.SendMail_Thread(subject, content);
                                }
                            }
                        }
                        #endregion

                        //校验组托的箱码是否有重复
                        int ocr3_counts = m_set.pallet_pack_maxqty;

                        for (int j = 0; j < ocr3_counts; j++)
                        {
                            for (int k = 0; k < ocr3_counts; k++)
                            {
                                if (j != k)
                                {
                                    if (bindingdata.OCR3List[j].Allcode.ToString() == bindingdata.OCR3List[k].Allcode.ToString())
                                    {
                                        BJ_PLC_HOME_th("组托中箱码重复");//TODO
                                        AddErrorLog(true, "OCR3校验数据异常", "组托中箱码重复：" + bindingdata.OCR3List[j].ToString(), "OCR3校验数据异常" + bindingdata.OCR3List[j].ToString(), "组托中箱码重复：" + bindingdata.OCR3List[j].ToString());
                                    }
                                }
                            }
                        }
                        //将系统检验码存入队列
                        string pallet_no = Guid.NewGuid().ToString();
                        CheckBarcodeQueue.Enqueue(pallet_no + "|" + bindingdata.OCR3List[check_no - 1].Allcode);

                        box_List.Clear();
                        item_List.Clear();
                      
                        for (int i = 0; i < m_set.pallet_pack_maxqty; i++)
                        {
                            string box_barcode = bindingdata.OCR3List[0].Allcode;
                            this.Invoke(new Action(() =>
                            {
                                lock (_lockObj)
                                {
                                    bindingdata.OCR3List.RemoveAt(0);
                                }
                            }));
                           
                            box_List.Add(box_barcode);
                          
                            //List<BoxAndItemBundle> li_res = list.Where(p => p.box_barcode == box_barcode).ToList();

                           var li_res = redisHelper_ocr1.GetBoxitemListByBoxBarcodeAsync("box_item_list",m_set.OrderNo,box_barcode).Result;
                            //在瓶箱绑定关系缓存中没有找到入托箱码，则查询px关联关系数据库
                            if (li_res == null || li_res.Count() == 0)
                            {
                                //查询瓶箱关联关系数据库
                                string sql = "Select * from qx_bundle_px_temp WITH(NOLOCK) where server_sync=0 and doc_no='" + m_set.OrderNo + "' and box_barcode='" + box_barcode + "'";
                                var ds_boxItem = DbHelperSQL.Query(sql);
                                if (ds_boxItem.Tables[0].Rows.Count <= 0)
                                {
                                    //如果数据库也查不到数据则添加万能码
                                    for (int x = 0; x < m_set.box_pack_maxqty; x++)
                                    {
                                        item_List.Add("ocr-" + Guid.NewGuid().ToString());
                                    }
                                }
                                else
                                {
                                    //否则使用数据库数据组托
                                    foreach (DataRow item in ds_boxItem.Tables[0].Rows)
                                    {
                                        item_List.Add(item["item_barcode"].ToString());
                                    }
                                }

                            }
                            else
                            {
                                List<BoxAndItemBundle> li_res_boxpack=new List<BoxAndItemBundle>();
                                foreach (RedisValue item in li_res)
                                {
                                    li_res_boxpack.Add(new BoxAndItemBundle { box_barcode=box_barcode,item_barcode=item.ToString()});
                                }
                                //List<BoxAndItemBundle> li_res_boxpack = li_res.Take(m_set.box_pack_maxqty).ToList();
                                foreach (BoxAndItemBundle box_item in li_res_boxpack)
                                {
                                    item_List.Add(box_item.item_barcode);
                                }
                            }
                        }

                        //开启组托线程
                        Thread td = new Thread(new ParameterizedThreadStart(th_MakePallet));
                        td.Start(pallet_no);
                        //组托方法
                        //th_MakePallet(false, pallet_no);
                        //瓶箱关联变量清零
                        ocr3_counts = bindingdata.OCR3List.Count;
                    }
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "组托异常", ex.ToString(), " Exception", ex.Message + " " + ex.StackTrace);
                }
            }
            isClosing = false;
        }

        void th_CheckPallet3(List<OCR1ViewDataClass> ocrList)
        {
            Thread.Sleep(2000);
            if (ocrList.Count >= m_set.pallet_pack_maxqty)
            {
                #region 误触发检测
                string subject = "";
                string content = "";
                List<PassBarcodeInfo> li_passBoxBarcode = Common.Common.GetNeedPassErrorFireBoxBarcode(List_BoxReadTime, m_set.pallet_pack_maxqty, box_barcode_fire_pass_time, lock_boxReadTime_list);
                if (li_passBoxBarcode.Count > 0)
                {
                    foreach (PassBarcodeInfo item1 in li_passBoxBarcode)
                    {
                        if (ocrList.Any(a => a.Allcode == item1.PassBarcode))
                        {

                            ocrList.Remove(ocrList.Select(a => a).Where(a => a.Allcode == item1.PassBarcode).First());

                            AddInfoLog("箱码误触发", " 被剔除的码：" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode);
                            subject = "道达尔-" + m_set.site_desc + "-" + m_set.ProduceLine + "箱码误触发";
                            content = "误触发箱码:" + item1.PassBarcode + ",上一个码：" + item1.LastBarcode + ",下一个码：" + item1.NextBarcode + ",误触发剔除时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            Common.Common.SendMail_Thread(subject, content);
                        }
                    }
                }
                #endregion

                box_List.Clear();
                item_List.Clear();
                for (int i = 0; i < m_set.pallet_pack_maxqty; i++)
                {
                    string box_barcode = ocrList[0].Allcode;
                    ocrList.RemoveAt(0);


                    this.Invoke(new Action(() =>
                    {
                        lock (_lockObj)
                        {
                            bindingdata.OCR3List.RemoveAt(0);
                        }
                    }));
                    box_List.Add(box_barcode);
                    List<BoxAndItemBundle> li_res = bindingdata.Box_Item_List.Where(p => p.box_barcode == box_barcode).ToList();
                    //在瓶箱绑定关系表中没有找到入托箱码，则补万能码
                    if (li_res == null || li_res.Count == 0)
                    {
                        for (int x = 0; x < m_set.box_pack_maxqty; x++)
                        {
                            item_List.Add("ocr-" + Guid.NewGuid().ToString());
                        }
                    }
                    else
                    {
                        List<BoxAndItemBundle> li_res_boxpack = li_res.Take(m_set.box_pack_maxqty).ToList();
                        foreach (BoxAndItemBundle box_item in li_res_boxpack)
                        {
                            item_List.Add(box_item.item_barcode);
                        }
                    }
                }

                //开启组托线程
                //th_MakePallet(false);
            }
        }

        /// <summary>
        /// 重置箱码、分道漏触发相关计数变量
        /// </summary>
        void ResetPLCCountVar()
        {
            List_channel.Clear();
            OCR2_electric_eye_fired_amount = 0;
            OCR2_read_code_amount = 0;
            OCR3_electric_eye_fired_amount = 0;
            OCR3_read_code_amount = 0;
        }
        /// <summary>
        /// 获取队列中的下一条数据
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        string GetNextData()
        {

            string item = null;

            lock (((ICollection)bindingdata.List_PLC).SyncRoot)
            {
                if (bindingdata.List_PLC.Count == 0)
                    waitHandler_PLCFD.Reset();//重置状态
                else
                    item = bindingdata.List_PLC.Dequeue();
            }
            //阻止线程并等待
            waitHandler_PLCFD.WaitOne();//如果状态重置过则等待，否则不等待

            return item;
        }
        /// <summary>
        /// 获取瓶箱绑定关系队列中的下一条数据
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        BoxAndItems2 GetNextArray_ItemData()
        {

            BoxAndItems2 item = null;

            lock (((ICollection)bindingdata.Array_Item).SyncRoot)
            {
                if (bindingdata.Array_Item.Count == 0)
                    waitHandler_MakeBox.Reset();//重置状态
                else
                    item = bindingdata.Array_Item.Dequeue();
            }
            //阻止线程并等待
            waitHandler_MakeBox.WaitOne();//如果状态重置过则等待，否则不等待

            return item;
        }
        /// <summary>
        /// 保存分道信号队列
        /// </summary>
        /// <param name="saverData"></param>
        public void SavePLCFD(string saverData)
        {
            if (saverData == null)
                return;

            lock (((ICollection)bindingdata.List_PLC).SyncRoot)
            {
                bindingdata.List_PLC.Enqueue(saverData);
            }

            waitHandler_PLCFD.Set();

        }

        public void th_channel_new()
        {
            string ioread = "";
            List<ChannelTick> li_ChannelTick = new List<ChannelTick>();//用了存放前一次触发分道的系统时间 
            int i;
            int ms1, ms2;
            int ms01, ms02;
            int cycle = 0;
            object[] objvalues = new object[2];
            OCR1ViewDataClass barcode = new OCR1ViewDataClass();
            string channel_data = "";

            int firedChannelNum;//触发分道索引
            int plcChannelAmounts;//plc分道计数汇总
            int channelMemoryAmount;//历史分道记录的累加
            int channelAmountDiff;//plc分道中的汇总和历史分道记录的累加差异
            string err_title = "";
            string err_content = "";
            string compareStr = "";
            IEnumerable<Channel> iEnumerable_channel;//从List_channel中查询触发的channelIndex对应的分道实体

            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    Thread.Sleep(5); //取消延迟读取   如果为0：好多误触发

                    //ams1 = System.Environment.TickCount;
                    cycle++;

                    ms01 = System.Environment.TickCount;

                    string str = GetNextData();
                    if (str == null)
                        continue;
                    ms1 = System.Environment.TickCount;
                    //List_PLC.RemoveAt(0);

                    channel_data = str.Split('#')[1];
                    #region 修改日期：2019-11-06
                    firedChannelNum = Convert.ToInt32(channel_data.Split(',')[0]);
                    plcChannelAmounts = Convert.ToInt32(channel_data.Split(',')[1].Trim());
                    #endregion
                    switch (firedChannelNum)
                    {
                        case 0:
                            ioread = "10000000";
                            break;
                        case 1:
                            ioread = "01000000";
                            break;
                        case 2:
                            ioread = "00100000";
                            break;
                        case 3:
                            ioread = "00010000";
                            break;
                        case 4:
                            ioread = "00001000";
                            break;
                        case 5:
                            ioread = "00000100";
                            break;
                        default:
                            break;
                    }

                    if (Common.Common.m_log > 0)
                    {
                        if (ioread != "00000000")
                        {
                            AddInfoLog("cycle=" + cycle + ", DI=" + ioread + "\r\n");
                        }
                    }

                    for (i = 0; i < 6; i++)
                    {
                        if (m_channel_to_boxpack[i] == 0)//不使用的分道跳过不处理
                        {
                            continue;
                        }

                        if (i == Convert.ToInt32(firedChannelNum))
                        {
                            #region 将每次触发分道数量累加存储起来，为后续跟plc中的总数对比
                            bool is_channel_exist = false;// 开始生产任务时，初始化List_channel，然后再判断分道数量，再累加（因为，偶发性会出现会出现生产过程中重置List_channel) by 徐元丰 2021.06.26
                            foreach (Channel item in List_channel)
                            {
                                if (item.ChannelNo == i)
                                {
                                    is_channel_exist = true;
                                    item.Amount = item.Amount + 1;
                                    break;
                                }
                            }
                            if (!is_channel_exist)
                            {
                                List_channel.Add(new Channel { ChannelNo = i, Amount = 1 });
                                channelMemoryAmount = 1;
                            }
                            #endregion

                            AddInfoLog("分道=" + (i + 1) + "触发，开始处理\r\n");

                            AloneLog.AddInfoLog("Channel", "分道=" + (i + 1) + "触发，触发总次数：" + plcChannelAmounts.ToString() + "\r\n");
                            //从分道历史记录中查找累加的分道总数
                            iEnumerable_channel = List_channel.Where(p => p.ChannelNo == firedChannelNum);
                            channelMemoryAmount = iEnumerable_channel.Count() > 0 ? iEnumerable_channel.ToList<Channel>()[0].Amount : 0;
                            //plc收到的总数和程序累加的总数比较，
                            //如果等于1正常，如果大于1说明有漏触发现象发生，需要自动补足缺失的分道信号，且报警。如果小于1说明plc传过来的数据异常，要报警，且要停线处理
                            channelAmountDiff = plcChannelAmounts - channelMemoryAmount + 1;
                            if (channelAmountDiff != 1)
                            {
                                err_title = "分道：" + (firedChannelNum + 1).ToString() + "异常";
                                compareStr = channelAmountDiff > 1 ? "大于" : "小于";
                                err_content = "PLC收到的分道数量" + compareStr + "每次累加的数量，PLC数量：" + plcChannelAmounts + ",累加数量：" + channelMemoryAmount;
                                //ui_settext_Label(lb_msg, err_title + err_content);
                                //AddErrorLog(true, err_title, err_content, err_title, err_content);
                                AloneLog.AddErrorLog("Channel", err_title + err_content);
                                channelAmountDiff = 1;//不以plc给的数据为准，如果不一致，还是读1瓶码，因为plc计数有可能没有重置。by 2021.12.1 徐元丰
                            }

                            //处理分道
                            for (int n = 0; n < channelAmountDiff; n++)
                            {
                                ms01 = System.Environment.TickCount;
                                if (bindingdata.OCR1List.Count > 0)
                                {
                                    barcode = bindingdata.OCR1List[0];
                                    this.Invoke(new Action(() =>
                                    {
                                       
                                        bindingdata.OCR1List.RemoveAt(0);
                                     
                                    }));
                                    th_view_tong_removeat(0); //待更新
                                }
                                else
                                {//如果瓶码列表里为空，那么补个万能码
                                    barcode.Allcode = "ocr-" + Guid.NewGuid().ToString();
                                    barcode.Code = "NOREAD";
                                    if (bottleWithFWCode)//防伪码产品如果没有读到码需要补万能并报警，非防伪码产品只需补万能码 by 徐元丰2021.02.26
                                    {
                                        err_title = "分道自动补码";
                                        err_content = "OCR1队列为空";
                                        AddErrorLog(true, err_title, err_content, err_title, err_content);
                                        AloneLog.AddErrorLog("Channel", err_title + err_content);
                                    }
                                }
                                ms02 = System.Environment.TickCount;

                                AddInfoLog("分道计算-OCR1耗时=" + (ms02 - ms01) + "\r\n");
                                AloneLog.AddInfoLog("Channel", "分道计算-OCR1耗时=" + (ms02 - ms01) + "\r\n");



                                string propertyName = $"CH{i + 1}_List";
                                PropertyInfo propertyInfo = bindingdata.GetType().GetProperty(propertyName);
                                if (propertyInfo != null)
                                {
                                    object propertyValue = propertyInfo.GetValue(bindingdata);
                                    BindingList<OCR1ViewDataClass> ch1List = propertyValue as BindingList<OCR1ViewDataClass>;
                                    this.Invoke(new Action(() =>
                                    {
                                        ch1List.Add(barcode);

                                    }));
                                    ms02 = System.Environment.TickCount;
                                    AddInfoLog("分道计算-分道1耗时=" + (ms02 - ms01) + "\r\n");
                                    AloneLog.AddInfoLog("Channel", "分道计算-分道" + i + 1 + "耗时=" + (ms02 - ms01) + "\r\n");
                                }
                                ms2 = System.Environment.TickCount;

                                AddInfoLog("分道计算单循环耗时=" + (ms2 - ms1) + "\r\n");
                                AloneLog.AddInfoLog("Channel", "分道计算单循环耗时=" + (ms2 - ms1) + "\r\n");
                            }

                            #region 事后要加补足内存中差异的数量
                            foreach (Channel item in List_channel)
                            {
                                if (item.ChannelNo == i)
                                {
                                    item.Amount = item.Amount + channelAmountDiff - 1;
                                    break;
                                }
                            }
                            #endregion
                            break;
                        }
                    }//for循环结束


                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "分道异常", "Exception: " + ex.Message, "Channel Exception", "Exception: " + ex.Message + " " + ex.StackTrace);
                    AloneLog.AddErrorLog("Channel", "分道异常: " + ex.Message + " " + ex.StackTrace);
                    continue;
                }
            }
            isClosing = false;
        }




        public void th_CheckMakeBox1L_2()
        {
            string sql = string.Empty;
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    Thread.Sleep(1000);
                    List<OCR1ViewDataClass> array_Item = new List<OCR1ViewDataClass>();
                    int takeCHRstCount = 0;//从分道中拿到的
                    if (bindingdata.OCR2List.Count <= 0)
                        waitHandler_MakeBox_fd.Reset();//重置状态

                    waitHandler_MakeBox_fd.WaitOne();
                    if (bindingdata.OCR2List.Count > 0)
                    {


                        //2、从分道中获取瓶码数据

                        for (int i = 0; i < 6; i++)
                        {
                            int chanel_boxpack_amount = m_channel_to_boxpack[i];
                            IEnumerable<OCR1ViewDataClass> takeCHRst;
                            string propertyName = $"CH{i + 1}_List";
                            PropertyInfo propertyInfo = bindingdata.GetType().GetProperty(propertyName);
                            if (propertyInfo != null)
                            {
                                object propertyValue = propertyInfo.GetValue(bindingdata);
                                BindingList<OCR1ViewDataClass> ch1List = propertyValue as BindingList<OCR1ViewDataClass>;
                                lock (lock_ch1_list)
                                {
                                    takeCHRst = ch1List.Take(chanel_boxpack_amount);
                                }

                                array_Item.AddRange(takeCHRst);
                                takeCHRstCount = takeCHRst.Count();
                                int diffAmount = chanel_boxpack_amount - takeCHRstCount;
                                if (diffAmount > 0)
                                {
                                    AddDebugLog("分道" + (i + 1) + "数量：" + takeCHRstCount + ",不足：" + chanel_boxpack_amount + ",自动补齐万能码");
                                    for (int x = 0; x < diffAmount; x++)
                                    {
                                        array_Item.Add(GetOCR1ViewDataClass("mbx-" + Guid.NewGuid().ToString()));
                                    }
                                }
                                lock (lock_ch1_list)
                                {
                                    for (int j = 0; j < 0 + takeCHRstCount; j++)
                                    {
                                        this.Invoke(new Action(() =>
                                        {

                                            ch1List.RemoveAt(0);

                                        }));
                                    }

                                }



                            }
                        }
                        string box_barcode = string.Empty;

                        box_barcode = bindingdata.OCR2List[0];

                        bindingdata.OCR2List.RemoveAt(0);



                        bindingdata.Array_Item.Enqueue(new BoxAndItems2 { box_barcode = box_barcode, item_array = array_Item });
                        waitHandler_MakeBox.Set();//继续组箱线程

                    }

                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "组箱异常", "Exception: " + ex.Message, "Make Box Exception", "Exception: " + ex.Message + " " + ex.StackTrace);
                }
            }
            isClosing = false;
        }

        /// <summary>
        /// 4L线组箱 2021.6.20，镇江4L产线改造，只有2个分道，1分道前4瓶为第一箱，后4瓶为第二箱；2分道前4瓶为第三箱，后4瓶为第四箱
        /// </summary>
        /// <param name="obj"></param>
        public void th_CheckMakeBox4L()
        {
            string sql = string.Empty;
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    List<OCR1ViewDataClass> array_Item = new List<OCR1ViewDataClass>();
                    int takeCHRstCount = 0;//从分道中拿到的

                    if (bindingdata.OCR2List.Count > 0)
                    {
                        string tempStr = makeBoxNumber(bindingdata.CH1_List.Count, bindingdata.CH2_List.Count);

                        //2、从分道中获取瓶码数据
                        int chanel_boxpack_amount = 0;
                        IEnumerable<OCR1ViewDataClass> takeCHRst;

                        if (tempStr == "1")
                        {
                            chanel_boxpack_amount = m_channel_to_boxpack[0];
                            lock (lock_ch1_list)
                            {
                                takeCHRst = bindingdata.CH1_List.Take(chanel_boxpack_amount);
                            }
                            array_Item.AddRange(takeCHRst);
                            takeCHRstCount = takeCHRst.Count();
                            int diffAmount = chanel_boxpack_amount - takeCHRstCount;
                            if (diffAmount > 0)
                            {
                                AddDebugLog("分道1数量：" + takeCHRstCount + ",不足：" + chanel_boxpack_amount + ",自动补齐万能码");
                                for (int x = 0; x < diffAmount; x++)
                                {
                                    array_Item.Add(GetOCR1ViewDataClass("mbx-" + Guid.NewGuid().ToString()));
                                }
                            }
                            lock (lock_ch1_list)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 0 + takeCHRstCount; j++)
                                    {
                                        bindingdata.CH1_List.RemoveAt(0);

                                    }

                                }));
                            }
                        }
                        else
                        {
                            chanel_boxpack_amount = m_channel_to_boxpack[1];
                            lock (lock_ch2_list)
                            {
                                takeCHRst = bindingdata.CH2_List.Take(chanel_boxpack_amount);
                            }
                            array_Item.AddRange(takeCHRst);
                            takeCHRstCount = takeCHRst.Count();
                            int diffAmount = chanel_boxpack_amount - takeCHRstCount;
                            if (diffAmount > 0)
                            {
                                AddDebugLog("分道2数量：" + takeCHRstCount + ",不足：" + chanel_boxpack_amount + ",自动补齐万能码");
                                for (int x = 0; x < diffAmount; x++)
                                {
                                    array_Item.Add(GetOCR1ViewDataClass("mbx-" + Guid.NewGuid().ToString()));
                                }
                            }
                            lock (lock_ch2_list)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 0 + takeCHRstCount; j++)
                                    {

                                        bindingdata.CH2_List.RemoveAt(0);

                                    }

                                }));
                            }
                        }



                        string box_barcode = bindingdata.OCR2List[0];


                        bindingdata.OCR2List.RemoveAt(0);

                        MakedBox_List.Add(box_barcode);
                        AppendMakedBoxView(box_barcode);
                        List<string> array_Item_str = new List<string>();
                        foreach (var item in array_Item)
                        {
                            array_Item_str.Add(item.Allcode);
                        }
                        //3、组箱
                        //Thread td = new Thread(new ParameterizedThreadStart(th_MakeBox));
                        //td.Start(new BoxAndItems { box_barcode = box_barcode, item_array = array_Item_str });
                    }
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "组箱异常", "Exception: " + ex.Message, "Make Box Exception", "Exception: " + ex.Message + " " + ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 新增一条已组箱队列
        /// </summary>
        /// <param name="msg"></param>
        void AppendMakedBoxView(string msg)
        {
            if (view_makedBox.InvokeRequired)
            {
                myDelegate_viewmakedboxadd _myinvoke = new myDelegate_viewmakedboxadd(AppendMakedBoxView);
                view_makedBox.Invoke(_myinvoke, new object[] { msg });
            }
            else
            {
                object[] objvalues = new object[2];
                objvalues[0] = msg.Substring(21, 9);
                if (msg.Substring(0, 4).ToLower() == "ocr-")//万能码
                {
                    objvalues[0] = "NOREAD";
                }
                objvalues[1] = msg;

                this.view_makedBox.Rows.Add(objvalues);
                view_makedBox.Rows[view_makedBox.Rows.Count - 1].Selected = true;               //设置为选中. 
                view_makedBox.FirstDisplayedScrollingRowIndex = view_makedBox.Rows.Count - 1;   //设置第一行显示
                view_makedBox.Focus();//保证滑动条永远处于最新一条数据位置
                makedBoxCount.Text = "" + view_makedBox.Rows.Count;
            }
        }
        volatile bool isClosing = false;
        void th_MakeBox()
        {
            string box_barcode = string.Empty;
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    string sql = string.Empty;
                    string sql2 = string.Empty;
                    List<string> li_sql = new List<string>();
                    List<string> li_sql2 = new List<string>();
                    BoxAndItems2 boxItems = GetNextArray_ItemData();//获取队列中的下一条数据
                    if (boxItems == null)
                        continue;
                    box_barcode = boxItems.box_barcode;
                    List<OCR1ViewDataClass> item_array = boxItems.item_array;


                    foreach (OCR1ViewDataClass item in item_array)
                    {
                        //加入瓶箱关联队列
                        bindingdata.Box_Item_List.Add(new BoxAndItemBundle { box_barcode = box_barcode, item_barcode = item.Allcode });

                        sql = string.Format("insert into qx_bundle_px_temp(site_no,pline_no,doc_no,sku_no,lot_no,mfd_date,box_barcode,item_barcode,op_time) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',getdate())", m_set.site_no, m_set.ProduceLine, m_set.OrderNo, m_set.ProductNo, m_set.Batch, m_set.ProduceDate, box_barcode, item.Allcode);
                        li_sql.Add(sql);
                        AddInfoLog("sql=" + sql + "\r\n");

                    }
                    //DbHelperSQL.ExecuteSqlTran(li_sql2); //即时插入本地数据库，用来预防产线断电引起的数据丢失 by 2021.11.10 徐元丰
                    int ret = DbHelperSQL.ExecuteSqlTran(li_sql);
                    if (ret > 0)
                    {
                        AddInfoLog("组箱", "组箱成功：" + box_barcode + "成功");
                        this.Invoke(new Action(() =>
                        {
                            //增加 箱码队列数据  
                            bindingdata.MakedBox_List.Add(GetOCR2ViewDataClass(box_barcode));
                        }));

                    }
                    else
                    {
                        AddErrorLog(true, "组箱失败", "组箱失败：" + box_barcode, "make box failed", box_barcode);
                    }
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "组箱异常", box_barcode + " " + ex.Message, "makeBox exception", box_barcode + " " + ex.Message + " " + ex.StackTrace);
                }
            }
            isClosing = false;
        }

        public async void th_MakePallet( object pallet_no)
        {
            string sql = "";
            List<string> box_barcode = new List<string>();
            int i, t;

            int ret;
            int pallet_pack_id;
            List<string> sql_list = new List<string>();

            try
            {
                AddInfoLog("Thread th_MakePallet Start..." + "\r\n");

                if (box_List.Count > 0 && item_List.Count > 0)
                {
                    AddInfoLog("MakePallet，pallet=" + pallet_no + "，box=" + box_List[0] + "，item=" + item_List[0] + "\r\n");
                }
                else
                {
                    AddInfoLog("MakePallet，pallet=" + pallet_no + "，boxCount=" + box_List.Count + "，itemCount=" + item_List.Count + "\r\n");
                }

                t = 0;
                string item_index = "";
                string box_index = "";
                pallet_pack_id = 0;
                string oldboxbarcode = "";
                int newbox = 0;
                //int box_qty = item_List.Count / m_set.box_pack_maxqty;

                #region 批量写入瓶码激活表
                string[] activeItem_List = List_ItemBarcode.ToArray();
                for (int x = 0; x < activeItem_List.Length; x++)
                {
                    if (List_ItemBarcode.Count > 0)
                    {
                        List_ItemBarcode.RemoveAt(0);
                    }
                }

                foreach (string itemBarcode in activeItem_List)
                {
                    sql = "insert into qx_bundle_itembarcode([item_barcode],[doc_no],[line_no],[op_user],[op_time],site_no,pline_no,item_status,sku_no,lot_no,mfd_date,item_barcode_16)"
                       + " values('" + itemBarcode + "','" + m_set.OrderNo + "',0,'" + m_set.UserName + "',getdate(),'" + m_set.site_no + "','" + m_set.ProduceLine + "','正常','" + m_set.ProductNo + "','" + m_set.Batch + "','" + m_set.ProduceDate + "','" + ((itemBarcode.Length == 65 || itemBarcode.Length == 60 || itemBarcode.Length == 40 || itemBarcode.Length == 23) ? (itemBarcode.Substring(itemBarcode.Length - 16)) : itemBarcode) + "')";
                    AddInfoLog("sql=" + sql + "\r\n");
                    sql_list.Add(sql);
                }
                #endregion

                for (i = 0; i < item_List.Count; i++)
                {
                    if ((i) % m_set.box_pack_maxqty == 0)//箱码计数
                    {
                        if (i != 0)
                        {
                            t++;
                        }
                        pallet_pack_id++;
                        newbox = 1;
                    }
                    else
                    {
                        newbox = 0;
                    }

                    if (item_List[i].Substring(0, 4).ToLower() == "ocr-")//万能码noread
                    {
                        item_index = "0";
                    }
                    else
                    {
                        item_index = (item_List[i].Length == 65 || item_List[i].Length == 60 || item_List[i].Length == 40 || item_List[i].Length == 23) ? item_List[i].Substring(item_List[i].Length - 16, 16) : item_List[i];
                    }

                    if (box_List[t].Substring(0, 4).ToLower() == "ocr-")//万能码
                    {
                        box_index = "0";
                    }
                    else
                    {
                        box_index = box_List[t].Substring(21, 9);
                    }

                    #region 完整的托、箱、瓶关联
                    sql = "insert into qx_bundle([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                     + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                     + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                     + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                     + ",[site_no],[site_desc],[doc_no])"
                     + " values('" + pallet_no + "','" + box_List[t] + "','" + item_List[i] + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                     + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                     + ",getdate(),getdate(),getdate()"
                     + ",'" + m_set.UserName + "','生产任务单',''"
                     + ",'" + box_List.Count + "','" + pallet_pack_id + "'"
                     + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                     + ",'" + m_set.OrderNo + "')";

                    sql_list.Add(sql);

                    AddInfoLog("sql=" + sql + "\r\n");
                    #endregion

                    #region 瓶箱关联
                    sql = string.Format("insert into qx_bundle_px([box_barcode],[item_barcode]) values('{0}','{1}')", box_List[t], item_List[i]);

                    sql_list.Add(sql);

                    AddInfoLog("sql=" + sql + "\r\n");
                    #endregion
                    #region 箱托关联
                    if (newbox == 1)
                    {
                        sql = "insert into qx_bundle_pb([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                        + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                        + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                        + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                        + ",[site_no],[site_desc],[doc_no])"
                        + " values('" + pallet_no + "','" + box_List[t] + "','" + item_List[i] + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                        + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                        + ",getdate(),getdate(),getdate()"
                        + ",'" + m_set.UserName + "','生产任务单',''"
                        + ",'" + box_List.Count + "','" + pallet_pack_id + "'"
                        + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                        + ",'" + m_set.OrderNo + "')";

                        sql_list.Add(sql);

                        AddInfoLog("sql=" + sql + "\r\n");
                    }
                    #endregion
                }

                //增加订单行实际数量，并且修改状态
                sql = "update qx_doc set act_qty=act_qty+" + item_List.Count + ",doc_status='执行中',guid='',server_sync=0,pline_no='" + m_set.ProduceLine + "' where doc_no='" + m_set.OrderNo + "' and line_no=" + m_set.LineNo + " and doc_catalog='生产任务单'";
                sql_list.Add(sql);

                AddInfoLog("sql=" + sql + "\r\n");
                try
                {
                  

                  
                    ret =  DbHelperSQL.ExecuteSqlTran(sql_list);
                    outPut += item_List.Count;

                    if (enableboard)
                    {
                        //更新看板
                        th_UpdateBoardShow("0", m_set.Batch, m_set.req_qty.ToString(), outPut.ToString());
                    }
                }
                catch (Exception ex_1)
                {
                    AddErrorLog(true, "组托异常", "Local SQL transaction Exception: " + ex_1.Message, "Make Pallet", "Local SQL transaction Exception: " + ex_1.Message + " " + ex_1.StackTrace);
                    ret = 0;
                }

                if (ret < sql_list.Count)//如果写入失败报警
                {
                    #region 报警程序 - 数据写入失败报警
                    AddErrorLog(true, "组托错误", "组托数据写入本地数据库失败, pallet=" + pallet_no + "," + ret + "/" + sql_list.Count, "Make Pallet Error", "fail to save to local database, pallet=" + pallet_no + "," + ret + "/" + sql_list.Count);

                    SaveSQLtoFile(sql_list, "sqlfile-" + pallet_no + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
                    return;
                    #endregion
                }
                AddInfoLog("组托成功!", " pallet=" + pallet_no);

                //刷新生产数量
                DataSet ds = DbHelperSQL.Query("select act_qty from qx_doc where doc_no='" + m_set.OrderNo + "'");
                m_set.act_qty = Common.Common.ToInt32(ds.Tables[0].Rows[0]["act_qty"].ToString());

                refreshQty1();
                refreshQty2();
                view_pallet_no_gv();
                view_pallet_box_gv();
               
              
                #region 同步数据到主服务器
                waitUploadData.Set();//数据上传线程给信号
                AddInfoLog("同步数据到主数据库线程执行信号Set()", "User=" + m_set.UserName);
                //if (uploadDataFinished)
                //{
                //    uploadDataFinished = false;
                //    // Thread td = new Thread(new ParameterizedThreadStart(th_UploadData));
                //    //td.Start(isManualStop);
                //}
                //else
                //{
                //    AddInfoLog("同步数据到主数据库", "数据正在上传中，不能再开启同步线程！");
                //    forceMakePallet_ResetEvent.Set();
                //}
                #endregion
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "组托异常", "Exception: " + ex.Message, "Make Pallet Exception", "Exception: " + ex.Message + " " + ex.StackTrace);
                forceMakePallet_ResetEvent.Set();
            }
        }

        void SaveSQLtoFile(List<string> sql_list, string filename)
        {
            try
            {
                FileStream fs1 = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + filename, FileMode.Append);
                BinaryWriter bw1 = new BinaryWriter(fs1);

                int i;
                for (i = 0; i < sql_list.Count; i++)
                {
                    bw1.Write(System.Text.Encoding.UTF8.GetBytes(sql_list[i] + "\r\n"));
                }

                bw1.Close();
                fs1.Close();
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "组托失败后保存命令文件又失败", "Exception: " + ex.Message, "Save sql file Exception", "Exception: " + ex.Message + " " + ex.StackTrace);
            }

        }

        void th_view_tong_removeat(int index)
        {
            Thread td = new Thread(new ParameterizedThreadStart(view_tong_removeat));
            td.Start(index);
        }

        void view_tong_removeat(object index)
        {
            //在线程里以安全方式调用控件
            if (view_tong.InvokeRequired)
            {
                view_tong.Invoke(new myDelegate_viewtongremoveat(view_tong_removeat), new object[] { index });
            }
            else
            {
                //view_tong.Rows.RemoveAt(Convert.ToInt32(index));
                //view_ocr1.Rows.RemoveAt(Convert.ToInt32(index));
                ocr1Count.Text = "" + view_tong.Rows.Count;
                tocr1count.Text = "" + view_ocr1.Rows.Count;
                lb_tong.Text = "" + view_tong.Rows.Count;
            }
        }


        void refreshQty1()
        {
            //在线程里以安全方式调用控件
            if (tDocInfo.InvokeRequired)
            {
                tDocInfo.BeginInvoke(new myDelegate_refreshqty1(refreshQty1), new object[] { });
            }
            else
            {
                tDocInfo.Text = "产品: " + m_set.ProductNo + "," + m_set.ProductName + ",产地=" + m_set.Madein + "\r\n" + "批号: " + m_set.Batch + ",日期: " + m_set.doc_date
                        + ", 数量: " + m_set.act_qty + "/" + m_set.req_qty + "\r\n 外箱: " + m_set.box_pack_maxqty + ", 托盘: " + m_set.pallet_pack_maxqty + ", 关联: " + m_set.pack_relation;
            }
        }

        void refreshQty2()
        {
            //在线程里以安全方式调用控件
            if (lb_tuo.InvokeRequired)
            {
                lb_tuo.BeginInvoke(new myDelegate_refreshqty2(refreshQty2), new object[] { });
            }
            else
            {
                lb_tuo.Text = "" + (Common.Common.ToInt32(lb_tuo.Text) + 1);
            }
        }

        void setItemCount(int value)
        {
            //在线程里以安全方式调用控件
            if (lb_tuo.InvokeRequired)
            {
                lb_tuo.Invoke(new myDelegate_setitemcount(setItemCount), new object[] { value });
            }
            else
            {
                tItemCount.Text = "" + value;
                tItemCount2.Text = "" + value;
            }
        }

        void view_pallet_no_gv()
        {
            if (view_pallet_no.InvokeRequired)
            {
                view_pallet_no.BeginInvoke(new myDelegate_view_pallet_no_gv(view_pallet_no_gv), new object[] { });
            }
            else
            {
                DataSet ds = new DataSet();
                string sql = "select pallet_no,last_op_time from qx_bundle with(nolock) where sku_no ='" + m_set.ProductNo + "' and lot_no ='" + m_set.Batch + "' group by pallet_no,last_op_time order by last_op_time";
                ds = DbHelperSQL.Query(sql);
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = new DataTable();
                    DataView dv = new DataView(ds.Tables[0]);
                    dt = dv.ToTable(true, "pallet_no");
                    view_pallet_no.DataSource = dt;
                }
            }
        }

        void view_pallet_box_gv()
        {
            if (view_pallet_box.InvokeRequired)
            {
                view_pallet_box.BeginInvoke(new myDelegate_view_pallet_box_gv(view_pallet_box_gv), new object[] { });
            }
            else
            {
                view_pallet_box.DataSource = null;
            }
        }

        int m_sync_count = 0;

        public void th_UploadData(object isManualStop)
        {
            while (true)
            {
                if (isClosing)
                    break;
                uploadDataFinished = false;
                string sql = string.Empty;
                string sql_doc = string.Empty;
                string sql_bundle = string.Empty;
                string sql_bundle_pb = string.Empty;
                string sql_bundle_px = string.Empty;
                string sql_bundle_px_ocr2 = string.Empty;
                string sql_bundle_itembarcode = string.Empty;
                string sql_operation = string.Empty;
                string sql_ocr_check_data = string.Empty;
                string sql_ds_qx_operation = string.Empty; //雷子华 20220729

                int i;
                DataSet ds_operation;
                DataSet ds_doc;
                DataSet ds_bundle;
                DataSet ds_bundle_pb;
                DataSet ds_bundle_px;
                DataSet ds_bundle_px_ocr2;
                DataSet ds_bundle_itembarcode;
                DataSet ds_qx_operation;//雷子华  20220729
                DataSet ds_ocr_check_data;
                DataTable dt;
                List<string> sql_list_remote = new List<string>();
                List<string> sql_list_local = new List<string>();
                m_sync_count = 50;
                int read_count = 0;
                int read_count_total = 0; //当前循环上传总记录
                int read_count_all_total = 0;//当前线程上传总记录
                List<DataEntity> li_DataEntity = new List<DataEntity>();
                //int ms1 = 0;
                //int ms2 = 0;
                string link = Common.Config.ReadValue("LinkServer", "ws") + "/webservice1.asmx";
                string sqlTemp = string.Empty;
                DataSet dsTemp;
                ServiceReference2.WebService1SoapClient ws = new ServiceReference2.WebService1SoapClient("WebService1Soap1", link);
                try
                {
                    m_sync_count = Common.Common.ToInt32(TotalProduceLineSys.Common.Config.ReadValue("LinkServer", "sync_count"));
                    if (m_sync_count <= 0)
                    {
                        m_sync_count = 100;
                    }

                    AddInfoLog("Thread th_UploadData Start..." + "\r\n");

                    sql_list_remote.Clear();
                    sql_list_local.Clear();


                    read_count_total = 0;
                    sql_list_remote.Clear();
                    sql_list_local.Clear();

                    li_DataEntity.Clear();
                    string guid = Guid.NewGuid().ToString();

                    //核对本地guid是否已经上传成功了
                    string error = Common.Common.CheckLocalDataUploadStatus();
                    if (error != "")
                    {
                        waitUploadData.Reset();//数据校验异常，线程暂停，等待下次组托重新放行 尝试上传
                        AddErrorLog(true, error.Split('@')[0], error.Split('@')[1], error.Split('@')[0], error.Split('@')[1]);
                        Thread.Sleep(2000);
                        waitUploadData.WaitOne();//线程等待执行信号
                        continue;
                    }

                    //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                    //获取本地未上传的完整托、箱、瓶关联数据
                    #region 完整托、箱、瓶关联数据
                    StringBuilder sbtemp = new StringBuilder();
                    sbtemp.Append("0,");
                    dsTemp = DbHelperSQL.Query("Select top " + m_sync_count + " autoid from qx_bundle WITH(NOLOCK) where server_sync=0 order by autoid");
                    if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                    {
                        for (int a = 0; a < dsTemp.Tables[0].Rows.Count; a++)
                        {
                            if (dsTemp.Tables[0].Rows[a][0].ToString().Trim().Length > 0)
                            {
                                sbtemp.Append(dsTemp.Tables[0].Rows[a][0].ToString() + ",");
                            }
                        }
                    }
                    //sql_bundle = "update qx_bundle set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_bundle  where server_sync=0 order by autoid)";
                    sql_bundle = "update qx_bundle set guid='" + guid + "' where autoid in (" + sbtemp.ToString().Substring(0, sbtemp.Length - 1) + ")";
                    DbHelperSQL.ExecuteSql(sql_bundle);

                    sql_bundle = "Select top " + m_sync_count + " * from qx_bundle WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_bundle = DbHelperSQL.Query(sql_bundle);
                    #endregion

                    //获取本地未上传的托、箱关联数据
                    #region 托、箱关联数据
                    sbtemp = new StringBuilder();
                    sbtemp.Append("0,");
                    dsTemp = DbHelperSQL.Query("Select top " + m_sync_count + " autoid from qx_bundle_pb WITH(NOLOCK) where server_sync=0 order by autoid");
                    if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                    {
                        for (int a = 0; a < dsTemp.Tables[0].Rows.Count; a++)
                        {
                            if (dsTemp.Tables[0].Rows[a][0].ToString().Trim().Length > 0)
                            {
                                sbtemp.Append(dsTemp.Tables[0].Rows[a][0].ToString() + ",");
                            }
                        }
                    }
                    //sql_bundle_pb = "update qx_bundle_pb set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_bundle_pb  where server_sync=0 order by autoid)";
                    sql_bundle_pb = "update qx_bundle_pb set guid='" + guid + "' where autoid in (" + sbtemp.ToString().Substring(0, sbtemp.Length - 1) + ")";
                    DbHelperSQL.ExecuteSql(sql_bundle_pb);

                    sql_bundle_pb = "Select top " + m_sync_count + " * from qx_bundle_pb WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_bundle_pb = DbHelperSQL.Query(sql_bundle_pb);

                    //获取本地未上传的瓶、箱关联数据
                    sbtemp = new StringBuilder();
                    sbtemp.Append("0,");
                    dsTemp = DbHelperSQL.Query("Select top " + m_sync_count + " autoid from qx_bundle_px WITH(NOLOCK) where server_sync=0 order by autoid");
                    if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                    {
                        for (int a = 0; a < dsTemp.Tables[0].Rows.Count; a++)
                        {
                            if (dsTemp.Tables[0].Rows[a][0].ToString().Trim().Length > 0)
                            {
                                sbtemp.Append(dsTemp.Tables[0].Rows[a][0].ToString() + ",");
                            }
                        }
                    }
                    //sql_bundle_px = "update qx_bundle_px set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_bundle_px  where server_sync=0 order by autoid)";
                    sql_bundle_px = "update qx_bundle_px set guid='" + guid + "' where autoid in (" + sbtemp.ToString().Substring(0, sbtemp.Length - 1) + ")";
                    DbHelperSQL.ExecuteSql(sql_bundle_px);

                    sql_bundle_px = "Select top " + m_sync_count + " * from qx_bundle_px  WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_bundle_px = DbHelperSQL.Query(sql_bundle_px);
                    #endregion

                    //获取本地未上传的未激活码数据
                    #region 未激活码数据
                    sbtemp = new StringBuilder();
                    sbtemp.Append("0,");
                    dsTemp = DbHelperSQL.Query("Select top " + m_sync_count + " autoid from qx_bundle_itembarcode WITH(NOLOCK) where server_sync=0 order by autoid");
                    if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                    {
                        for (int a = 0; a < dsTemp.Tables[0].Rows.Count; a++)
                        {
                            if (dsTemp.Tables[0].Rows[a][0].ToString().Trim().Length > 0)
                            {
                                sbtemp.Append(dsTemp.Tables[0].Rows[a][0].ToString() + ",");
                            }
                        }
                    }
                    //sql_bundle_itembarcode = "update qx_bundle_itembarcode set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_bundle_itembarcode  where server_sync=0 order by autoid)";
                    sql_bundle_itembarcode = "update qx_bundle_itembarcode set guid='" + guid + "' where autoid in (" + sbtemp.ToString().Substring(0, sbtemp.Length - 1) + ")";
                    DbHelperSQL.ExecuteSql(sql_bundle_itembarcode);

                    sql_bundle_itembarcode = "Select top " + m_sync_count + " * from qx_bundle_itembarcode WITH(NOLOCK)  where server_sync=0 and guid='" + guid + "'";
                    ds_bundle_itembarcode = DbHelperSQL.Query(sql_bundle_itembarcode);
                    #endregion

                    //获取本地未上传日志数据   雷子华 20220729
                    #region 未上传日志数据   雷子华 20220729
                    sbtemp = new StringBuilder();
                    sbtemp.Append("0,");
                    dsTemp = DbHelperSQL.Query("Select top " + m_sync_count + " autoid from qx_operation WITH(NOLOCK) where server_sync=0 order by autoid");
                    if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                    {
                        for (int a = 0; a < dsTemp.Tables[0].Rows.Count; a++)
                        {
                            if (dsTemp.Tables[0].Rows[a][0].ToString().Trim().Length > 0)
                            {
                                sbtemp.Append(dsTemp.Tables[0].Rows[a][0].ToString() + ",");
                            }
                        }
                    }
                    sql_ds_qx_operation = "update qx_operation set guid='" + guid + "' where autoid in (" + sbtemp.ToString().Substring(0, sbtemp.Length - 1) + ")";
                    DbHelperSQL.ExecuteSql(sql_ds_qx_operation);

                    sql_ds_qx_operation = "Select top " + m_sync_count + " * from qx_operation WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_qx_operation = DbHelperSQL.Query(sql_ds_qx_operation);

                    //获取本地未上传的ocr校验数据
                    sbtemp = new StringBuilder();
                    sbtemp.Append("0,");
                    dsTemp = DbHelperSQL.Query("Select top " + m_sync_count + " autoid from qx_ocr_check_data WITH(NOLOCK) where server_sync=0 order by autoid");
                    if (dsTemp != null && dsTemp.Tables[0].Rows.Count > 0)
                    {
                        for (int a = 0; a < dsTemp.Tables[0].Rows.Count; a++)
                        {
                            if (dsTemp.Tables[0].Rows[a][0].ToString().Trim().Length > 0)
                            {
                                sbtemp.Append(dsTemp.Tables[0].Rows[a][0].ToString() + ",");
                            }
                        }
                    }
                    //sql_ocr_check_data = "update qx_ocr_check_data set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_ocr_check_data  where server_sync=0 order by autoid)";
                    sql_ocr_check_data = "update qx_ocr_check_data set guid='" + guid + "' where autoid in (" + sbtemp.ToString().Substring(0, sbtemp.Length - 1) + ")";
                    DbHelperSQL.ExecuteSql(sql_ocr_check_data);

                    sql_ocr_check_data = "Select top " + m_sync_count + " * from qx_ocr_check_data WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_ocr_check_data = DbHelperSQL.Query(sql_ocr_check_data);
                    #endregion


                    //获取本地未上传的瓶、箱关联数据(ocr2组箱数据)
                    #region 本地未上传的瓶、箱关联数据(ocr2组箱数据)
                    sql_bundle_px_ocr2 = "update qx_bundle_px_temp set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_bundle_px_temp WITH(NOLOCK) where server_sync=0 order by autoid)";
                    DbHelperSQL.ExecuteSql(sql_bundle_px_ocr2);

                    sql_bundle_px_ocr2 = "Select top " + m_sync_count + " * from qx_bundle_px_temp WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_bundle_px_ocr2 = DbHelperSQL.Query(sql_bundle_px_ocr2);
                    #endregion

                    #region 上传订单

                    sql_doc = "update qx_doc set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_doc WITH(NOLOCK) where server_sync=0 order by autoid)";
                    DbHelperSQL.ExecuteSql(sql_doc);

                    sql_doc = "Select top " + m_sync_count + " * from qx_doc WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_doc = DbHelperSQL.Query(sql_doc);
                    #endregion

                    #region 上传日志 雷子华 20220729
                    sql_operation = "update qx_operation set guid='" + guid + "' where autoid in (Select top " + m_sync_count + " autoid from qx_operation WITH(NOLOCK) where server_sync=0 order by autoid)";
                    DbHelperSQL.ExecuteSql(sql_operation);

                    sql_operation = "Select top " + m_sync_count + " * from qx_operation WITH(NOLOCK) where server_sync=0 and guid='" + guid + "'";
                    ds_operation = DbHelperSQL.Query(sql_operation);
                    #endregion

                    if (ds_bundle == null && ds_bundle_pb == null && ds_bundle_px == null && ds_bundle_itembarcode == null && ds_ocr_check_data == null && ds_bundle_px_ocr2 == null && ds_doc == null && ds_operation == null)
                    //if (ds_bundle == null && ds_bundle_pb == null && ds_bundle_px == null && ds_ocr_check_data == null)
                    {
                        waitUploadData.Reset();//没有数据需要上传时,重置等待信号，使线程等待
                        AddInfoLog("数据同步", "无数据需要上传，线程暂停");

                    }
                    else if (ds_bundle.Tables[0].Rows.Count <= 0 && ds_bundle_pb.Tables[0].Rows.Count <= 0 && ds_bundle_px.Tables[0].Rows.Count <= 0 && ds_bundle_itembarcode.Tables[0].Rows.Count <= 0 && ds_ocr_check_data.Tables[0].Rows.Count <= 0 && ds_bundle_px_ocr2.Tables[0].Rows.Count <= 0 && ds_doc.Tables[0].Rows.Count <= 0 && ds_operation.Tables[0].Rows.Count <= 0)
                    {
                        waitUploadData.Reset();//没有数据需要上传时,重置等待信号，使线程等待
                        AddInfoLog("数据同步", "无数据需要上传，线程暂停");

                    }
                    else
                    {
                        //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                        if (ds_bundle != null && ds_bundle.Tables[0].Rows.Count > 0)
                        {
                            dt = ds_bundle.Tables[0];
                            read_count = dt.Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "bundle";
                            List<string> li_data = new List<string>();
                            //更新主服务器的bundle和inventory
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string pallet_no = "'" + dt.Rows[i]["pallet_no"].ToString() + "'";
                                string box_barcode = "'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                string item_barcode = "'" + dt.Rows[i]["item_barcode"].ToString() + "'";
                                string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                string mfd_date = "'" + Convert.ToDateTime(dt.Rows[i]["mfd_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string pline_no = "'" + dt.Rows[i]["pline_no"].ToString() + "'";
                                string pline_desc = "'" + dt.Rows[i]["pline_desc"].ToString() + "'";
                                string pb_date = "'" + Convert.ToDateTime(dt.Rows[i]["pb_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string bi_date = "'" + Convert.ToDateTime(dt.Rows[i]["bi_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string last_op_time = "'" + Convert.ToDateTime(dt.Rows[i]["last_op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string last_op_user = "'" + dt.Rows[i]["last_op_user"].ToString() + "'";
                                string last_op_desc = "'生产任务单'";
                                string last_op_pda_no = "'" + dt.Rows[i]["last_op_pda_no"].ToString() + "'";
                                int pallet_pack_qty = Convert.ToInt32(dt.Rows[i]["pallet_pack_qty"]);
                                int pallet_pack_id = Convert.ToInt32(dt.Rows[i]["pallet_pack_id"]);
                                int box_pack_qty = Convert.ToInt32(dt.Rows[i]["box_pack_qty"]);
                                int box_pack_id = Convert.ToInt32(dt.Rows[i]["box_pack_id"]);
                                string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                string site_desc = "'" + dt.Rows[i]["site_desc"].ToString() + "'";
                                string status = "'6'";
                                string doc_no = "'" + dt.Rows[i]["doc_no"].ToString() + "'";

                                string dataStr = pallet_no + "," + box_barcode + "," + item_barcode + "," + sku_no + "," + lot_no + "," + mfd_date + "," + pline_no + "," + pline_desc + "," + pb_date + "," + bi_date + "," +
                                last_op_time + "," + last_op_user + "," + last_op_desc + "," + last_op_pda_no + "," + pallet_pack_qty + "," + pallet_pack_id + "," + box_pack_qty + "," + box_pack_id + "," + site_no + "," + site_desc + "," + status + "," + doc_no;

                                li_data.Add(dataStr);
                                AddInfoLog("qx_bundle " + dataStr + "\r\n");

                                //本地sql不加日志，方便远程sql复制出来运行
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                            //ms2 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            //AddInfoLog("数据同步", "获取qx_bundle数据,耗时：" + (ms2 - ms1));
                        }

                        if (ds_bundle_pb != null && ds_bundle_pb.Tables[0].Rows.Count > 0)
                        {

                            dt = ds_bundle_pb.Tables[0];
                            read_count = dt.Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "inventory";
                            List<string> li_data = new List<string>();
                            //更新主服务器的bundle和inventory
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string pallet_no = "'" + dt.Rows[i]["pallet_no"].ToString() + "'";
                                string box_barcode = "'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                string mfd_date = "'" + Convert.ToDateTime(dt.Rows[i]["mfd_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                string site_desc = "'" + dt.Rows[i]["site_desc"].ToString() + "'";
                                string location_no = "'" + dt.Rows[i]["doc_no"].ToString() + "'";
                                string last_op_time = "'" + Convert.ToDateTime(dt.Rows[i]["last_op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string last_op_user = "'" + dt.Rows[i]["last_op_user"].ToString() + "'";
                                string last_op_desc = "'" + dt.Rows[i]["last_op_desc"].ToString() + "'";
                                string last_op_pda_no = "'" + dt.Rows[i]["last_op_pda_no"].ToString() + "'";
                                int pallet_pack_qty = Convert.ToInt32(dt.Rows[i]["pallet_pack_qty"]);
                                int pallet_pack_id = Convert.ToInt32(dt.Rows[i]["pallet_pack_id"]);
                                int box_pack_qty = Convert.ToInt32(dt.Rows[i]["box_pack_qty"]);
                                string box_status = "'库存'";
                                int status = 6;
                                string create_desc = "'生产入库'";

                                string dataStr = pallet_no + "," + box_barcode + "," + sku_no + "," + lot_no + "," + mfd_date + "," + site_no + "," + site_desc + "," + location_no + "," + last_op_time + "," +
                                last_op_user + "," + last_op_desc + "," + last_op_pda_no + "," + pallet_pack_qty + "," + pallet_pack_id + "," + box_pack_qty + "," + box_status + "," + status + "," + create_desc;

                                li_data.Add(dataStr);
                                AddInfoLog("qx_inventory " + dataStr + "\r\n");

                                //本地sql不加日志，方便远程sql复制出来运行
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                            //删除临时表存放的数据 by 2021.11.11 徐元丰
                            dt = ds_bundle_pb.Tables[0];
                            List<string> li_data2 = new List<string>();
                            string ss = string.Empty;
                            string ss1 = string.Empty;
                            string ss2 = string.Empty;
                            ss = "delete qx_OCR3_CacheData where barcode in ('0'";
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                ss2 += ",'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                ss1 = dt.Rows[i]["doc_no"].ToString();
                            }
                            ss = ss + ss2 + ") and doc_no='" + ss1 + "'";
                            li_data2.Add(ss);
                            AddInfoLog("删除本地数据qx_OCR3_CacheData " + ss2 + "\r\n");
                            DbHelperSQL.ExecuteSqlTran(li_data2);
                        }

                        if (ds_bundle_px != null && ds_bundle_px.Tables[0].Rows.Count > 0)
                        {
                            //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            dt = ds_bundle_px.Tables[0];
                            read_count = dt.Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "inventory_px";
                            List<string> li_data = new List<string>();
                            //更新主服务器的qx_inventory_px
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string box_barcode = "'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                string item_barcode = "'" + dt.Rows[i]["item_barcode"].ToString() + "'";

                                string dataStr = box_barcode + "," + item_barcode;

                                li_data.Add(dataStr);
                                AddInfoLog("qx_inventory_px " + dataStr + "\r\n");

                                //本地sql不加日志，方便远程sql复制出来运行
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                            //ms2 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            //AddInfoLog("数据同步", "获取qx_bundle_px数据,耗时：" + (ms2 - ms1));
                        }

                        //更新主服务器的qx_bundle_itembarcode
                        if (ds_bundle_itembarcode != null && ds_bundle_itembarcode.Tables[0].Rows.Count > 0)
                        {
                            //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            dt = ds_bundle_itembarcode.Tables[0];
                            read_count = ds_bundle_itembarcode.Tables[0].Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "bundle_itembarcode";
                            List<string> li_data = new List<string>();
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string item_barcode = "'" + dt.Rows[i]["item_barcode"].ToString() + "'";
                                string doc_no = "'" + dt.Rows[i]["doc_no"].ToString() + "'";
                                int line_no = Convert.ToInt32(dt.Rows[i]["line_no"]);
                                string op_user = "'" + dt.Rows[i]["op_user"].ToString() + "'";
                                string op_time = "'" + Convert.ToDateTime(dt.Rows[i]["op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                string pline_no = "'" + dt.Rows[i]["pline_no"].ToString() + "'";
                                string item_status = "'" + dt.Rows[i]["item_status"].ToString() + "'";
                                string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                string mfd_date = "'" + Convert.ToDateTime(dt.Rows[i]["mfd_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string item_barcode_16 = "'" + dt.Rows[i]["item_barcode_16"].ToString() + "'";
                                string dataStr = item_barcode + "," + doc_no + "," + line_no + "," + op_user + "," + op_time + "," + site_no + "," + pline_no + "," + item_status
                                    + "," + sku_no + "," + lot_no + "," + mfd_date + "," + item_barcode_16;

                                li_data.Add(dataStr);
                                AddInfoLog("qx_bundle_itembarcode " + dataStr + "\r\n");
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                            //ms2 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            //AddInfoLog("数据同步", "获取qx_bundle_itembarcode数据,耗时：" + (ms2 - ms1));
                        }

                        if (ds_ocr_check_data != null && ds_ocr_check_data.Tables[0].Rows.Count > 0)
                        {
                            //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            dt = ds_ocr_check_data.Tables[0];
                            read_count = ds_ocr_check_data.Tables[0].Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "OCR_Check_Data";
                            List<string> li_data = new List<string>();
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                string site_desc = "'" + dt.Rows[i]["site_desc"].ToString() + "'";
                                string pline_no = "'" + dt.Rows[i]["pline_no"].ToString() + "'";
                                string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                string sku_desc = "'" + dt.Rows[i]["sku_desc"].ToString() + "'";
                                string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                string pallet_no = "'" + dt.Rows[i]["pallet_no"].ToString() + "'";
                                string box_barcode = "'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                string act_box_barcode = "'" + dt.Rows[i]["act_box_barcode"].ToString() + "'";
                                string isFWCode = "'" + dt.Rows[i]["isFWCode"].ToString() + "'";
                                string isCheckVal = "'" + dt.Rows[i]["isCheckVal"].ToString() + "'";
                                string isRecover = "'" + dt.Rows[i]["isRecover"].ToString() + "'";
                                string server_sync = "'" + dt.Rows[i]["server_sync"].ToString() + "'";
                                string pb_date = "'" + Convert.ToDateTime(dt.Rows[i]["pb_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string server_sync_time = "''";
                                if (dt.Rows[i]["server_sync_time"].ToString().Trim().Length > 0)
                                {
                                    server_sync_time = "'" + Convert.ToDateTime(dt.Rows[i]["server_sync_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                }
                                string remark = "'" + dt.Rows[i]["remark"].ToString() + "'";
                                string doc_date = "'" + m_set.doc_date + "'";
                                string removedqty = dt.Rows[i]["removedqty"].ToString();//不要忘了存储过程中加该字段，和本地添加该字段

                                string dataStr = site_no + "," + site_desc + "," + pline_no + "," + sku_no + "," + sku_desc + "," + lot_no + "," + pallet_no + "," + box_barcode
                                    + "," + act_box_barcode + "," + isFWCode + "," + isCheckVal + "," + isRecover + "," + server_sync + "," + pb_date + "," + server_sync_time + "," + remark + "," + doc_date + "," + removedqty;

                                //string dataStr = site_no + "," + site_desc + "," + pline_no + "," + sku_no + "," + sku_desc + "," + lot_no + "," + pallet_no + "," + box_barcode
                                //    + "," + act_box_barcode + "," + isFWCode + "," + isCheckVal + "," + isRecover + "," + server_sync + "," + pb_date + "," + server_sync_time + "," + remark + "," + doc_date;

                                li_data.Add(dataStr);
                                AddInfoLog("qx_OCR_Check_Data " + dataStr + "\r\n");
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                            //ms2 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            //AddInfoLog("数据同步", "获取qx_ocr_check_data数据,耗时：" + (ms2 - ms1));
                        }



                        #region 更新服务器上的qx_operation  雷子华   20220729
                        //更新主服务器的qx_operation
                        if (ds_qx_operation != null && ds_qx_operation.Tables[0].Rows.Count > 0)
                        {
                            dt = ds_qx_operation.Tables[0];
                            read_count = ds_qx_operation.Tables[0].Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "operation";
                            List<string> li_data = new List<string>();
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string op_time = "'" + Convert.ToDateTime(dt.Rows[i]["op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string op_user = "'" + dt.Rows[i]["op_user"].ToString() + "'";
                                string subject = "'" + dt.Rows[i]["subject"].ToString() + "'";
                                string operation = "'" + dt.Rows[i]["operation"].ToString() + "'";
                                string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                string site_desc = "'" + dt.Rows[i]["site_desc"].ToString() + "'";
                                string op_PDA_no = "'" + dt.Rows[i]["op_PDA_no"].ToString() + "'";
                                int log_level = Convert.ToInt32(dt.Rows[i]["log_level"]);
                                string key1 = "'" + dt.Rows[i]["key1"].ToString() + "'";
                                string key2 = "'" + dt.Rows[i]["key2"].ToString() + "'";
                                string key3 = "'" + dt.Rows[i]["key3"].ToString() + "'";
                                string key4 = "'" + dt.Rows[i]["key4"].ToString() + "'";
                                //int server_sync = Convert.ToInt32(dt.Rows[i]["server_sync"]);
                                //string server_sync_time = "'" + Convert.ToDateTime(dt.Rows[i]["server_sync_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string dataStr = op_time + "," + op_user + "," + subject + "," + operation + "," + site_no + "," + site_desc + "," + op_PDA_no + "," + log_level + "," + key1 + "," + key2 + "," + key3 + "," + key4;

                                li_data.Add(dataStr);
                                operationAddInfoLog("qx_operation " + dataStr + "\r\n");
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                        }
                        #endregion



                        //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                        //ocr2瓶箱关联

                        if (ds_bundle_px_ocr2 != null && ds_bundle_px_ocr2.Tables[0].Rows.Count > 0)
                        {
                            dt = ds_bundle_px_ocr2.Tables[0];
                            if (!Convert.ToBoolean(isManualStop))
                            {
                                if (dt.Rows.Count == 100)
                                {
                                    read_count = dt.Rows.Count;
                                    read_count_total += read_count;
                                    DataEntity dataEntity = new DataEntity();
                                    dataEntity.DataType = "bundle_px_ocr2";
                                    List<string> li_data = new List<string>();
                                    //更新主服务器的qx_inventory_px
                                    for (i = 0; i < dt.Rows.Count; i++)
                                    {
                                        string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                        string pline_no = "'" + dt.Rows[i]["pline_no"].ToString() + "'";
                                        string doc_no = "'" + dt.Rows[i]["doc_no"].ToString() + "'";
                                        string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                        string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                        string mfd_date = "'" + Convert.ToDateTime(dt.Rows[i]["mfd_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                        string box_barcode = "'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                        string item_barcode = "'" + dt.Rows[i]["item_barcode"].ToString() + "'";
                                        string op_time = "'" + Convert.ToDateTime(dt.Rows[i]["op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                        string dataStr = site_no + "," + pline_no + "," + doc_no + "," + sku_no + "," + lot_no + "," + mfd_date + "," + box_barcode + "," + item_barcode + "," + op_time;

                                        li_data.Add(dataStr);
                                        AddInfoLog("bundle_px_ocr2 " + dataStr + "\r\n");

                                        //本地sql不加日志，方便远程sql复制出来运行
                                    }
                                    dataEntity.Data = li_data.ToArray();
                                    li_DataEntity.Add(dataEntity);
                                }
                                else
                                {
                                    AddInfoLog("ocr2 数据条数小于100，不上传");
                                    break;
                                }
                            }
                            else
                            {
                                read_count = dt.Rows.Count;
                                read_count_total += read_count;
                                DataEntity dataEntity = new DataEntity();
                                dataEntity.DataType = "bundle_px_ocr2";
                                List<string> li_data = new List<string>();
                                //更新主服务器的qx_inventory_px
                                for (i = 0; i < dt.Rows.Count; i++)
                                {
                                    string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                    string pline_no = "'" + dt.Rows[i]["pline_no"].ToString() + "'";
                                    string doc_no = "'" + dt.Rows[i]["doc_no"].ToString() + "'";
                                    string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                    string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                    string mfd_date = "'" + Convert.ToDateTime(dt.Rows[i]["mfd_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                    string box_barcode = "'" + dt.Rows[i]["box_barcode"].ToString() + "'";
                                    string item_barcode = "'" + dt.Rows[i]["item_barcode"].ToString() + "'";
                                    string op_time = "'" + Convert.ToDateTime(dt.Rows[i]["op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                    string dataStr = site_no + "," + pline_no + "," + doc_no + "," + sku_no + "," + lot_no + "," + mfd_date + "," + box_barcode + "," + item_barcode + "," + op_time;

                                    li_data.Add(dataStr);
                                    AddInfoLog("bundle_px_ocr2 " + dataStr + "\r\n");

                                    //本地sql不加日志，方便远程sql复制出来运行
                                }
                                dataEntity.Data = li_data.ToArray();
                                li_DataEntity.Add(dataEntity);
                            }

                        }
                        //上传订单数据
                        if (ds_doc != null && ds_doc.Tables[0].Rows.Count > 0)
                        {
                            //ms1 = System.Environment.TickCount;//记录上传耗时 by 2021.7.27 徐元丰
                            dt = ds_doc.Tables[0];
                            read_count = dt.Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "doc";
                            List<string> li_data = new List<string>();
                            for (i = 0; i < dt.Rows.Count; i++)
                            {
                                string doc_no = "'" + dt.Rows[i]["doc_no"].ToString() + "'";
                                string doc_date = "'" + Convert.ToDateTime(dt.Rows[i]["doc_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string doc_catalog = "'" + dt.Rows[i]["doc_catalog"].ToString() + "'";
                                int line_no = Convert.ToInt32(dt.Rows[i]["line_no"]);
                                string lot_no = "'" + dt.Rows[i]["lot_no"].ToString() + "'";
                                string doc_status = "'" + dt.Rows[i]["doc_status"].ToString() + "'";
                                string customer_no = "'" + dt.Rows[i]["customer_no"].ToString() + "'";
                                string customer_desc = "'" + dt.Rows[i]["customer_desc"].ToString() + "'";
                                string sku_no = "'" + dt.Rows[i]["sku_no"].ToString() + "'";
                                string sku_desc = "'" + dt.Rows[i]["sku_desc"].ToString() + "'";
                                string site_no1 = "'" + dt.Rows[i]["site_no1"].ToString() + "'";
                                string site_desc1 = "'" + dt.Rows[i]["site_desc1"].ToString() + "'";
                                string site_no2 = "'" + dt.Rows[i]["site_no2"].ToString() + "'";
                                string site_desc2 = "'" + dt.Rows[i]["site_desc2"].ToString() + "'";
                                int act_qty = Convert.ToInt32(dt.Rows[i]["act_qty"]);
                                int req_qty = Convert.ToInt32(dt.Rows[i]["req_qty"]);
                                string doc_remark = "'" + dt.Rows[i]["doc_remark"].ToString() + "'";
                                string create_time = "'" + Convert.ToDateTime(dt.Rows[i]["create_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string create_user = "'" + dt.Rows[i]["create_user"].ToString() + "'";
                                string mfd_date = "'" + Convert.ToDateTime(dt.Rows[i]["mfd_date"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string key1 = "'" + dt.Rows[i]["key1"].ToString() + "'";
                                string dataStr = doc_no + "," + doc_date + "," + doc_catalog + "," + line_no + "," + lot_no + "," + doc_status + "," + customer_no + "," + customer_desc
                                        + "," + sku_no + "," + sku_desc + "," + site_no1 + "," + site_desc1 + "," + site_no2 + "," + site_desc2 + "," + act_qty + "," + req_qty + "," + doc_remark
                                        + "," + create_time + "," + create_user + "," + mfd_date + "," + key1;

                                li_data.Add(dataStr);
                                AddInfoLog("qx_doc " + dataStr + "\r\n");

                                //本地sql不加日志，方便远程sql复制出来运行
                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                        }
                        if (ds_operation != null && ds_operation.Tables[0].Rows.Count > 0)
                        {
                            dt = ds_operation.Tables[0];
                            read_count = dt.Rows.Count;
                            read_count_total += read_count;
                            DataEntity dataEntity = new DataEntity();
                            dataEntity.DataType = "operation";
                            List<string> li_data = new List<string>();

                            for (i = 0; i < dt.Rows.Count; i++)
                            {

                                string op_time = "'" + Convert.ToDateTime(dt.Rows[i]["op_time"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string op_user = "'" + dt.Rows[i]["op_user"].ToString() + "'";
                                string subject = "'" + dt.Rows[i]["subject"].ToString() + "'";
                                string operation = "'" + dt.Rows[i]["operation"].ToString() + "'";
                                string site_no = "'" + dt.Rows[i]["site_no"].ToString() + "'";
                                string site_desc = "'" + dt.Rows[i]["site_desc"].ToString() + "'"; ;
                                string op_PDA_no = "'" + dt.Rows[i]["op_PDA_no"].ToString() + "'";
                                int log_level = Convert.ToInt32(dt.Rows[i]["log_level"]);
                                string key1 = "'" + dt.Rows[i]["key1"].ToString() + "'";
                                string key2 = "'" + dt.Rows[i]["key2"].ToString() + "'";
                                string key3 = "'" + dt.Rows[i]["key3"].ToString() + "'";
                                string key4 = "'" + dt.Rows[i]["key4"].ToString() + "'";


                                string dataStr = op_time + "," + op_user + "," + subject + "," + operation + "," + site_no + "," + site_desc + "," + op_PDA_no + "," + log_level
                                       + "," + key1 + "," + key2 + "," + key3 + "," + key4;
                                //dataEntity.Data[i] = dataStr;
                                li_data.Add(dataStr);

                                operationAddInfoLog("qx_operation " + dataStr + "\r\n");


                                //本地sql不加日志，方便远程sql复制出来运行

                            }
                            dataEntity.Data = li_data.ToArray();
                            li_DataEntity.Add(dataEntity);
                        }

                        string error1 = Common.Common.UploadData("produceData", guid, li_DataEntity);
                        if (error1 != "")
                        {
                            AddErrorLog(true, error1.Split('@')[0], error1.Split('@')[1], error1.Split('@')[0], error1.Split('@')[1]);
                        }
                        else
                        {
                            read_count_all_total += read_count_total;
                            AddInfoLog("数据同步", "产线数据同步记录：" + read_count_total + ",guid：" + guid);
                        }
                    }





                    AddInfoLog("数据同步", "产线数据同步记录总计：" + read_count_all_total);

                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "数据同步异常", ex.Message, "Sync Data Exception", ex.Message + " " + ex.StackTrace);
                }
                uploadDataFinished = true;
                forceMakePallet_ResetEvent.Set();

                waitUploadData.WaitOne();//线程等待执行信号

            }
            isClosing = false;
        }
        T GetNextRedisData<T>(Queue<T> Queue, ManualResetEvent wait)
        {
            T item = default(T);

            lock (((ICollection)Queue).SyncRoot)
            {
                if (Queue.Count == 0)
                    wait.Reset();//重置状态
                else
                    item = Queue.Dequeue();
            }
            //阻止线程并等待
            wait.WaitOne();//如果状态重置过则等待，否则不等待

            return item;
        }
        public async void th_Redis(object isManualStop)
        {
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    //获取OCR1需要缓存的下一条数据
                    OCR1ViewDataClass data = GetNextRedisData(_ocr1Queue, waitRedis);
                    if (data == null)
                        continue;
                
                    
                    await redisHelper_ocr1.AddOCR1ViewDataAsync("OCR1List", data, m_set.OrderNo.Trim());
                ;
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "缓存数据异常", ex.Message, "Sync Data Exception", ex.Message + " " + ex.StackTrace);
                }

            }
            isClosing = false;
        }
        public async void th_Redis_boxitem(object isManualStop)
        {
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    //获取OCR1需要缓存的下一条数据
                  
                    //获取瓶箱关联缓存
                    BoxAndItemBundle data_boxitem = GetNextRedisData(_ocr1Queue_boxItem, waitRedis_boxitem);
                    if (data_boxitem == null)
                        continue;
                    await redisHelper_ocr1.AddOCR1ViewDataAsync("box_item_list", data_boxitem, m_set.OrderNo.Trim());
                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "缓存数据异常", ex.Message, "Sync Data Exception", ex.Message + " " + ex.StackTrace);
                }

            }
            isClosing = false;
        }
        

        public async void th_RedisRemove(object isManualStop)
        {
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    //获取OCR1需要缓存的下一条数据
                    OCR1ViewDataClass data = GetNextRedisData(_ocr1Queue_remove, waitRedisRemove);
                    if (data == null)
                        continue;
                    await redisHelper_ocr1.RemoveOCR1ViewDataAsync("OCR1List", m_set.OrderNo.Trim(), data);
                 

                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "缓存数据异常", ex.Message, "Sync Data Exception", ex.Message + " " + ex.StackTrace);
                }

            }
            isClosing = false;
        }
        public async void th_RedisRemove_boxitem(object isManualStop)
        {
            while (true)
            {
                try
                {
                    if (isClosing)
                        break;
                    //获取OCR1需要缓存的下一条数据
                    BoxAndItemBundle data_boxitem = GetNextRedisData(_ocr1Queue_remove_boxItem, waitRedisRemove_boxitem);
                    if (data_boxitem == null)
                        continue;
                    await redisHelper_ocr1.RemoveOCR1ViewDataAsync("box_item_list", m_set.OrderNo.Trim(), data_boxitem);


                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "缓存数据异常", ex.Message, "Sync Data Exception", ex.Message + " " + ex.StackTrace);
                }

            }
            isClosing = false;
        }
       

        #endregion

        #region 瓶码读取站队  --当超过站队系统报警
        /// <summary>
        /// 获取OCR3读取的箱码数量
        /// </summary>
        /// <returns></returns>
        private int GetOcr3Count()
        {
            try
            {
                string OrderCode = m_set.OrderNo;
                string strSql = string.Format("select COUNT(0) from dbo.ocr_3 where OrderCode='{0}'", OrderCode);
                int num = Convert.ToInt32(DbHelperSQL.GetSingle(strSql));
                return num;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        /// <summary>
        /// 手动上传代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_upload_Click(object sender, EventArgs e)
        {
            AddInfoLog("手动同步数据到主数据库", "User=" + m_set.UserName);
            //if (uploadDataFinished)
            //{
            //    Thread td = new Thread(new ParameterizedThreadStart(th_UploadData));
            //    td.Start(true);
            //}
            //else
            //{
            //    AddInfoLog("手动同步数据到主数据库", "数据正在上传中，不能再开启同步线程！");
            //    MessageBox.Show("数据正在上传中，请稍后再手动同步！");
            //}
            if (worker_UploadData != null && worker_UploadData.ThreadState == ThreadState.Running)
            {
                waitUploadData.Set();
            }
            else
            {
                worker_UploadData = new Thread(new ParameterizedThreadStart(th_UploadData));
                worker_UploadData.Name = string.Format("[{0}]:[{1}]", "UploadData", this.GetType().Name);
                worker_UploadData.Start();
            }


        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FormCreateDoc fm = new FormCreateDoc();
            DialogResult dr = fm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                tDocno.Text = m_set.OrderNo;
                tDocInfo.Text = "产品: " + m_set.ProductNo + "," + m_set.ProductName + ",产地=" + m_set.Madein + "\r\n" + "批号: " + m_set.Batch + ",日期: " + m_set.doc_date
                     + ", 数量: " + m_set.act_qty + "/" + m_set.req_qty + "\r\n 外箱: " + m_set.box_pack_maxqty + ", 托盘: " + m_set.pallet_pack_maxqty + ", 关联: " + m_set.pack_relation;

                AddInfoLog("创建生产任务", "doc_no=" + m_set.OrderNo + ",doc_date=" + m_set.doc_date + ",Madein=" + m_set.Madein + ",sku_no=" + m_set.ProductNo + ",lot_no=" + m_set.Batch + ",qty=" + m_set.act_qty + "/" + m_set.req_qty);
            }

            if (tDocno.Text != "")
            {
                btn_Start.Enabled = true;
            }
            else
            {
                btn_Start.Enabled = false;
            }
        }



        void ForceMakePallet()

        {
            int num = bindingdata.OCR3List.Count / m_set.pallet_pack_maxqty;
            for (int i = 0; i < num; i++)
            {
                AddErrorLog(true, "OCR3校验数据异常", "OCR3校验数据大于等于2托盘的数量", "OCR3校验数据异常", "OCR3校验数据大于等于2托盘的数量");
                th_CheckPallet3(bindingdata.OCR3List.Take(m_set.pallet_pack_maxqty).ToList());//TODO 2021.5.10 如果最后委托的数量大于组托的数量，就要拆分。 by 徐元丰
            }
            if (m_set.ProduceLine == "1L")
            {
                Thread td = new Thread(new ThreadStart(th_CheckPallet_1L_Manual));
                td.Start();
            }
            else
            {
                Thread td = new Thread(new ThreadStart(th_CheckPallet_4L_Manual));
                td.Start();
            }
        }

        #region 看板相关
        void th_UpdateBoardShow(string needSendLotNo, string lotNO, string reqQty, string actQty)
        {
            ThreadPool.QueueUserWorkItem(UpdateBoardShow, new string[] { needSendLotNo, lotNO, reqQty, actQty });
        }

        void UpdateBoardShow(Object obj)
        {
            string[] str_array = (string[])obj;
            string needSendLotNo = str_array[0];
            if (needSendLotNo == "1")
                board.SendLotNo(str_array[1]);
            board.SendQty(str_array[2], str_array[3]);
        }

        void board_ReceiveMsg(string type, string msg, string detailMsg)
        {
            if (type == "failed")
                AddErrorLog(true, "看板通讯-异常", msg, "Board comm Exception", msg + " " + detailMsg);
        }
        #endregion

        string getQuerySqlStr(string tableName, string doc_no)
        {
            return string.Format("select barcode from {0} where doc_no='{1}' order by autoid asc", tableName, doc_no);
        }
        string getQuerySqlStr1(string tableName, string doc_no)
        {
            return string.Format("select box_barcode,item_barcode from {0} where doc_no='{1}' order by autoid asc", tableName, doc_no);
        }
        string getInsertSqlStr(string tableName, string doc_no, string barcode)
        {
            return string.Format("insert into {0}(doc_no,barcode,op_time) values('{1}','{2}',getdate())", tableName, m_set.OrderNo, barcode);
        }
        string getInsertSqlStr(string tableName, string doc_no, string box_barcode, string item_barcode)
        {
            return string.Format("insert into {0}(doc_no,box_barcode,item_barcode,op_time) values('{1}','{2}','{3}',getdate())", tableName, m_set.OrderNo, box_barcode, item_barcode);
        }
        string getDeleteSqlStr(string tableName, string doc_no)
        {
            return string.Format("delete from {0} where doc_no='{1}'", tableName, doc_no);
        }
        string getDeleteSqlStr(string tableName, string doc_no, string barcode)
        {
            return string.Format("delete from {0} where doc_no='{1}' and barcode='{2}'", tableName, doc_no, barcode);
        }
        string getDeleteSqlStr(string tableName, string doc_no, string barcode, string item_barcode)
        {
            return string.Format("delete from {0} where doc_no='{1}' and box_barcode='{2}' and item_barcode='{3}'", tableName, doc_no, barcode, item_barcode);
        }


        void LoadCacheData()
        {
            try
            {

                #region 加载ocr1数据
                redisHelper_ocr1.GetOCR1ViewDataListAsync("OCR1List", tDocno.Text.Trim(), bindingdata.OCR1List);
                redisHelper_ocr1.GetOCR1ViewDataListAsync("OCR3List", tDocno.Text.Trim(), bindingdata.OCR3List);
                #endregion
                #region 加载分道数据
                redisHelper_ocr1.GetOCR1ViewDataListAsync("CH1List", tDocno.Text.Trim(), bindingdata.CH1_List);
                redisHelper_ocr1.GetOCR1ViewDataListAsync("CH2List", tDocno.Text.Trim(), bindingdata.CH2_List);
                redisHelper_ocr1.GetOCR1ViewDataListAsync("CH3List", tDocno.Text.Trim(), bindingdata.CH3_List);
                redisHelper_ocr1.GetOCR1ViewDataListAsync("CH4List", tDocno.Text.Trim(), bindingdata.CH4_List);
                redisHelper_ocr1.GetOCR1ViewDataListAsync("CH5List", tDocno.Text.Trim(), bindingdata.CH5_List);
                redisHelper_ocr1.GetOCR1ViewDataListAsync("CH6List", tDocno.Text.Trim(), bindingdata.CH6_List);


                #endregion



                //DataSet ds2 = DbHelperSQL.Query(getQuerySqlStr("qx_OCR2_CacheData", m_set.OrderNo));


                DataSet ds11 = DbHelperSQL.Query(getQuerySqlStr1("qx_BoxItem_CacheData", m_set.OrderNo));
                DataSet ds_ActiveItemBarcode = DbHelperSQL.Query(getQuerySqlStr("qx_ActiveItemBarcode_CacheData", m_set.OrderNo));

                bool hasData_ActiveItemBarcode = true;
                if (ds_ActiveItemBarcode == null || ds_ActiveItemBarcode.Tables[0].Rows.Count == 0)
                    hasData_ActiveItemBarcode = false;

                bool hasData1 = true;
                // if (ds1 == null || ds1.Tables[0].Rows.Count == 0)
                // hasData1 = false;
                bool hasData2 = true;

                bool hasData3 = true;

                bool hasData9 = true;
                //if (ds9 == null || ds9.Tables[0].Rows.Count == 0)
                //    hasData9 = false;
                bool hasData10 = true;

                bool hasData11 = true;
                if (ds11 == null || ds11.Tables[0].Rows.Count == 0)
                    hasData11 = false;

                if (!hasData_ActiveItemBarcode && !hasData1 && !hasData2 && !hasData3 && !hasData9 && !hasData10 && !hasData11)
                {
                    AddInfoLog("没有可导入零头的数据");
                    return;
                }

                if (hasData_ActiveItemBarcode)
                {
                    foreach (DataRow dr in ds_ActiveItemBarcode.Tables[0].Rows)
                    {
                        List_ItemBarcode.Add(dr["barcode"].ToString());
                    }
                }








                if (hasData11)
                {
                    foreach (DataRow dr in ds11.Tables[0].Rows)
                    {
                        bindingdata.Box_Item_List.Add(new BoxAndItemBundle() { box_barcode = dr["box_barcode"].ToString(), item_barcode = dr["item_barcode"].ToString() });
                    }
                }
                //设置总瓶数
                setItemCount(bindingdata.OCR1List.Count + get_channel_item_count());



                //原先加载完数据后，清空临时表里的数据，2021.11.10新增实时保存后，不需要删除qx_OCR3_CacheData，qx_MakedBox_CacheData，qx_BoxItem_CacheData这三张表在组托的时候被删除 by 徐元丰
                List<string> li_deleteSql = new List<string>();


                li_deleteSql.Add(getDeleteSqlStr("qx_BoxItem_CacheData", m_set.OrderNo));
                li_deleteSql.Add(getDeleteSqlStr("qx_ActiveItemBarcode_CacheData", m_set.OrderNo));
                int ret = DbHelperSQL.ExecuteSqlTran(li_deleteSql);
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "load cachedata error", "load cachedata error" + ex.Message + " " + ex.StackTrace, "load cachedata error", "load cachedata error" + ex.Message + " " + ex.StackTrace);
                MessageBox.Show("零头加载失败，请重试" + ex.Message);
            }
        }


        private void btn_Pause_Click(object sender, EventArgs e)
        {
            try
            {
                string link = string.Empty;

                link = Config.ReadValue("LinkServer", "ws") + "/webservice1.asmx";
                if (Common.Common.CheckPageUrl(link))//检查是否有网络，如果没有网络可以直接“暂停生产” by 徐元丰 2021.8.18
                {
                    if (!uploadDataFinished)
                    {
                        MessageBox.Show("数据上传中，请稍后再暂停生产！");
                        return;
                    }
                }
                else
                {
                    uploadDataFinished = true;//TODO
                }
                if (Common.Common.TS("当前操作只针对批次没有生产完成的情况，确定暂停生产吗？"))
                {
                    AddInfoLog("暂停生产");

                    #region  写入PQMS短信队列 结束生产
                    string error = "";
                    SendPQMSMsg(false, out error);
                    if (error != "")
                        return;
                    #endregion

                    lbl_showProgress.Enabled = true;
                    lbl_showProgress.Visible = true;
                    this.Update();
                    lbl_showProgress.Text = "正在保存零头,请稍后...";
                    this.Update();
                    SaveCache();//保存零头

                    lbl_showProgress.Text = "正在停止生产,请稍后...";
                    this.Update();
                    StopProduce();//停止生产

                    this.Enabled = true;
                    button3.Enabled = false;
                    button2.Enabled = false;
                    btn_pallet_add_box.Enabled = false;
                    btn_pallet_delete_box.Enabled = false;
                    btn_refresh.Enabled = false;
                    lbl_showProgress.Visible = false;
                    Common.Config.Write("Line", "makeBoxNumbers", makeBoxNumbers.ToString());//暂停生产，记录从第几分道?开始组箱 by 2021.6.20
                    bindingdata.Status = RunType.停止;
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "暂停生产错误", ex.Message + " " + ex.StackTrace, "暂停生产错误", ex.Message + " " + ex.StackTrace);
                this.Enabled = true;
                lbl_showProgress.Visible = false;
            }
        }

        /// <summary>
        /// 保存ocr读码数量
        /// </summary>
        void SaveOcrReadInfo()
        {
            #region 保存ocr读码数量
            string error = string.Empty;
            Common.Common.SaveOcrReadInfo(new Entity.OcrReadInfo
            {
                site_no = m_set.site_no,
                pline_no = m_set.ProduceLine,
                doc_no = tDocno.Text.Trim(),
                lot_no = m_set.Batch,
                mfd_date = Convert.ToDateTime(m_set.ProduceDate),
                ocr1_read = OCR1ReadAmount,
                ocr1_noread = OCR1NoReadAmount,
                ocr2_read = OCR2ReadAmount,
                ocr2_noread = OCR2NoReadAmount,
                ocr3_read = OCR3ReadAmount,
                ocr3_noread = OCR3NoReadAmount,
            }, out error);

            if (error != string.Empty)
            {
                AddErrorLog2(error.Split('@')[0], error.Split('@')[1]);
            }
            #endregion
        }

        public void closeProgress()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new myDelegate2(closeProgress));
            }
            else
            {
                progressBar.CloseForm();
            }
        }

        public void setProgress(object text)
        {
            try
            {
                if (progressBar.InvokeRequired)
                {
                    progressBar.Invoke(new myDelegate3(setProgress), new object[] { text });
                }
                else
                {
                    progressBar.setProgress(text.ToString());
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "更新进度出错", ex.Message + " " + ex.StackTrace, "更新进度出错", ex.Message + " " + ex.StackTrace);
            }
        }

        void SaveCache()
        {
            string msg = "保存零头(订单：" + m_set.OrderNo + "，产品：" + m_set.ProductNo + ",批号：" + m_set.Batch + ")";
            try
            {
                #region
                if (List_ItemBarcode.Count == 0 && bindingdata.OCR1List.Count == 0 && bindingdata.OCR2List.Count == 0 && bindingdata.CH1_List.Count == 0 && bindingdata.CH2_List.Count == 0 && bindingdata.CH3_List.Count == 0 && bindingdata.CH4_List.Count == 0 && bindingdata.CH5_List.Count == 0 && bindingdata.CH6_List.Count == 0 && MakedBox_List.Count == 0 && bindingdata.Box_Item_List.Count == 0)
                {
                    AddInfoLog("保存零头", msg + "没有可保存到零头数据");
                    return;
                }

                List<string> li_sql = new List<string>();
                string sql = string.Empty;

                //保存激活的瓶码
                foreach (string item in List_ItemBarcode)
                {
                    li_sql.Add(getInsertSqlStr("qx_ActiveItemBarcode_CacheData", m_set.OrderNo, item));
                }

                //保存ocr1
                //foreach (OCR1ViewDataClass item in bindingdata.OCR1List)
                //{
                //    li_sql.Add(getInsertSqlStr("qx_OCR1_CacheData", m_set.OrderNo, item.Allcode));
                //}
                //保存ocr2 
                foreach (string item in bindingdata.OCR2List)
                {
                    li_sql.Add(getInsertSqlStr("qx_OCR2_CacheData", m_set.OrderNo, item));
                }

                //保存已组箱
                foreach (string item in MakedBox_List)
                {
                    //li_sql.Add(getDeleteSqlStr("qx_MakedBox_CacheData", m_set.OrderNo, item));// by 2021.11.10 徐元丰
                    li_sql.Add(getInsertSqlStr("qx_MakedBox_CacheData", m_set.OrderNo, item));
                }
                //保存已组箱的瓶箱关系
                foreach (BoxAndItemBundle item in bindingdata.Box_Item_List)
                {
                    //li_sql.Add(getDeleteSqlStr("qx_BoxItem_CacheData", m_set.OrderNo, item.box_barcode, item.item_barcode));// by 2021.11.10 徐元丰
                    li_sql.Add(getInsertSqlStr("qx_BoxItem_CacheData", m_set.OrderNo, item.box_barcode, item.item_barcode));
                }


                int ret = DbHelperSQL.ExecuteSqlTran(li_sql);

                Common.Config.Write("Line", "removedqty", removedqty.ToString());//保存剔除瓶子的数量

                if (ret > 0)
                {
                    //bindingdata.OCR1List.Clear();

                    //OCR2_List.Clear();

                    MakedBox_List.Clear();
                    bindingdata.Box_Item_List.Clear();

                    List_ItemBarcode.Clear();





                    reload_ocr2list2();
                    //设置总瓶数
                    setItemCount(0);
                    AddInfoLog("保存零头", msg + "成功");

                }
                else
                {
                    AddErrorLog2(msg + "失败，请重试", msg + "失败，请重试");
                }
                #endregion
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "save cachedata error", "save cachedata error" + ex.Message + " " + ex.StackTrace, "save cachedata error", "save cachedata error" + ex.Message + " " + ex.StackTrace);
            }
        }

        private void MainForm_1L4L_ZJ_Resize(object sender, EventArgs e)
        {
            lbl_showProgress.Location = new Point((this.Width - lbl_showProgress.Width) / 2, (this.Height - lbl_showProgress.Height) / 2);
        }

        /// <summary>
        /// 给ocr定期发送心跳包
        /// </summary>
        /// <param name="li_Socket"></param>
        void OCRHeartBeat(object obj)
        {
            List<Socket> li_Socket = (List<Socket>)obj;
            while (true)
            {
                //如果没有在生产，则关闭
                if (!isProducing)
                {
                    return;
                }
                Thread.Sleep(2000);
                try
                {
                    byte b = new byte();

                    foreach (Socket ss in li_Socket)
                    {
                        if (ss != null)
                        {
                            ss.Send(new byte[] { b });
                        }

                    }


                }
                catch (Exception ex)
                {
                    AddErrorLog(true, "ocr心跳包错误", ex.Message + " " + ex.StackTrace, "ocr心跳包错误", ex.Message + " " + ex.StackTrace);
                    BJ_PLC_HOME_th("ocr心跳包错误");
                    AddErrorLog5(true, "ocr心跳包错误", ex.Message, "ocr心跳包错误", ex.Message);
                    Thread_OCRHeartBeat();
                    return;
                }
            }
        }

        //OCR断连
        public void Disconnection()
        {
            try
            {
                if (Disconnection_time > 10)
                {
                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMERED%"));//发送报警指令
                    AddInfoLog("开始报警" + "#HOMERED%");
                    Thread.Sleep(5000);//持续5秒钟
                    socket_PLC_Sender.Send(Encoding.Default.GetBytes("#HOMERET%"));//关闭报警指令
                    AddInfoLog("关闭报警 " + "#HOMERET%");
                    BJ_Stop_PLC_HOME();
                    //  MessageBox.Show(string.Format("{0}", Disconnection_time));
                    Disconnection_time = 0;
                    return;
                }

            }
            catch (Exception ex)
            {
                AddInfoLog("报警失败 " + ex);
            }

        }

        #region 分道数量手工调整数量
        private void btn_Channel1_Subtrac_Click(object sender, EventArgs e)
        {
            if (bindingdata.CH1_List.Count > 0)
            {
                lock (lock_ch1_list)
                {
                    ShowLog("分道1", "减少一瓶,瓶码：" + bindingdata.CH1_List[bindingdata.CH1_List.Count - 1].ToString());
                    AloneLog.AddInfoLog("Channel", "分道1：减少一瓶,瓶码：" + bindingdata.CH1_List[bindingdata.CH1_List.Count - 1].ToString() + "\r\n");
                    this.Invoke(new Action(() =>
                    {

                        bindingdata.CH1_List.RemoveAt(bindingdata.CH1_List.Count - 1);
                    }));
                }

            }
        }

        private void btn_Channel1_Add_Click(object sender, EventArgs e)
        {
            string guidStr = Guid.NewGuid().ToString();
            lock (lock_ch1_list)
            {
                this.Invoke(new Action(() =>
                {
                    bindingdata.CH1_List.Add(GetOCR1ViewDataClass("man-" + guidStr));
                }));

                ShowLog("分道1", "增加一瓶,瓶码：" + bindingdata.CH1_List[bindingdata.CH1_List.Count - 1].ToString());
                AloneLog.AddInfoLog("Channel", "分道1：增加一瓶,瓶码：" + bindingdata.CH1_List[bindingdata.CH1_List.Count - 1].ToString() + "\r\n");
            }

        }

        private void btn_Channel2_Subtrac_Click(object sender, EventArgs e)
        {
            if (bindingdata.CH2_List.Count > 0)
            {
                lock (lock_ch2_list)
                {
                    ShowLog("分道2", "减少一瓶,瓶码：" + bindingdata.CH2_List[bindingdata.CH2_List.Count - 1].ToString());
                    AloneLog.AddInfoLog("Channe1", "分道2：减少一瓶,瓶码：" + bindingdata.CH2_List[bindingdata.CH2_List.Count - 1].ToString() + "\r\n");
                    this.Invoke(new Action(() =>
                    {

                        bindingdata.CH2_List.RemoveAt(bindingdata.CH2_List.Count - 1);
                    }));
                }
            }
        }

        private void btn_Channel2_Add_Click(object sender, EventArgs e)
        {
            string guidStr = Guid.NewGuid().ToString();
            lock (lock_ch2_list)
            {
                this.Invoke(new Action(() =>
                {
                    bindingdata.CH2_List.Add(GetOCR1ViewDataClass("man-" + guidStr));
                }));

                ShowLog("分道2", "增加一瓶,瓶码：" + bindingdata.CH2_List[bindingdata.CH2_List.Count - 1].ToString());
                AloneLog.AddInfoLog("Channel", "分道2：增加一瓶,瓶码：" + bindingdata.CH2_List[bindingdata.CH2_List.Count - 1].ToString() + "\r\n");
            }

        }

        private void btn_Channel3_Subtrac_Click(object sender, EventArgs e)
        {
            if (bindingdata.CH3_List.Count > 0)
            {
                lock (lock_ch3_list)
                {
                    ShowLog("分道3", "减少一瓶,瓶码：" + bindingdata.CH3_List[bindingdata.CH3_List.Count - 1].ToString());
                    AloneLog.AddInfoLog("Channe1", "分道3：减少一瓶,瓶码：" + bindingdata.CH3_List[bindingdata.CH3_List.Count - 1].ToString() + "\r\n");
                    this.Invoke(new Action(() =>
                    {

                        bindingdata.CH3_List.RemoveAt(bindingdata.CH3_List.Count - 1);
                    }));
                }
            }
        }

        private void btn_Channel3_Add_Click(object sender, EventArgs e)
        {
            string guidStr = Guid.NewGuid().ToString();
            lock (lock_ch3_list)
            {
                this.Invoke(new Action(() =>
                {
                    bindingdata.CH3_List.Add(GetOCR1ViewDataClass("man-" + guidStr));
                }));

                ShowLog("分道3", "增加一瓶,瓶码：" + bindingdata.CH3_List[bindingdata.CH3_List.Count - 1].ToString());
                AloneLog.AddInfoLog("Channel", "分道3：增加一瓶,瓶码：" + bindingdata.CH3_List[bindingdata.CH3_List.Count - 1].ToString() + "\r\n");
            }
        }

        private void btn_Channel4_Subtrac_Click(object sender, EventArgs e)
        {
            if (bindingdata.CH4_List.Count > 0)
            {
                lock (lock_ch4_list)
                {
                    ShowLog("分道4", "减少一瓶,瓶码：" + bindingdata.CH4_List[bindingdata.CH4_List.Count - 1].ToString());
                    AloneLog.AddInfoLog("Channe1", "分道4：减少一瓶,瓶码：" + bindingdata.CH4_List[bindingdata.CH4_List.Count - 1].ToString() + "\r\n");
                    this.Invoke(new Action(() =>
                    {

                        bindingdata.CH4_List.RemoveAt(bindingdata.CH4_List.Count - 1);
                    }));
                }
            }
        }

        private void btn_Channel4_Add_Click(object sender, EventArgs e)
        {
            string guidStr = Guid.NewGuid().ToString();
            lock (lock_ch4_list)
            {
                this.Invoke(new Action(() =>
                {
                    bindingdata.CH4_List.Add(GetOCR1ViewDataClass("man-" + guidStr));
                }));

                ShowLog("分道4", "增加一瓶,瓶码：" + bindingdata.CH4_List[bindingdata.CH4_List.Count - 1].ToString());
                AloneLog.AddInfoLog("Channel", "分道4：增加一瓶,瓶码：" + bindingdata.CH4_List[bindingdata.CH4_List.Count - 1].ToString() + "\r\n");
            }
        }

        private void btn_Channel5_Subtrac_Click(object sender, EventArgs e)
        {
            if (bindingdata.CH5_List.Count > 0)
            {
                lock (lock_ch5_list)
                {
                    ShowLog("分道5", "减少一瓶,瓶码：" + bindingdata.CH5_List[bindingdata.CH5_List.Count - 1].ToString());
                    AloneLog.AddInfoLog("Channe1", "分道5：减少一瓶,瓶码：" + bindingdata.CH5_List[bindingdata.CH5_List.Count - 1].ToString() + "\r\n");
                    this.Invoke(new Action(() =>
                    {

                        bindingdata.CH5_List.RemoveAt(bindingdata.CH5_List.Count - 1);
                    }));
                }
            }
        }

        private void btn_Channel5_Add_Click(object sender, EventArgs e)
        {
            string guidStr = Guid.NewGuid().ToString();
            lock (lock_ch5_list)
            {
                this.Invoke(new Action(() =>
                {
                    bindingdata.CH5_List.Add(GetOCR1ViewDataClass("man-" + guidStr));
                }));

                ShowLog("分道5", "增加一瓶,瓶码：" + bindingdata.CH5_List[bindingdata.CH5_List.Count - 1].ToString());
                AloneLog.AddInfoLog("Channel", "分道5：增加一瓶,瓶码：" + bindingdata.CH5_List[bindingdata.CH5_List.Count - 1].ToString() + "\r\n");
            }
        }

        private void btn_Channel6_Subtrac_Click(object sender, EventArgs e)
        {
            if (bindingdata.CH6_List.Count > 0)
            {
                lock (lock_ch6_list)
                {
                    ShowLog("分道6", "减少一瓶,瓶码：" + bindingdata.CH6_List[bindingdata.CH6_List.Count - 1].ToString());
                    AloneLog.AddInfoLog("Channe1", "分道6：减少一瓶,瓶码：" + bindingdata.CH6_List[bindingdata.CH6_List.Count - 1].ToString() + "\r\n");
                    this.Invoke(new Action(() =>
                    {

                        bindingdata.CH6_List.RemoveAt(bindingdata.CH6_List.Count - 1);
                    }));
                }
            }
        }

        private void btn_Channel6_Add_Click(object sender, EventArgs e)
        {
            string guidStr = Guid.NewGuid().ToString();
            lock (lock_ch6_list)
            {
                this.Invoke(new Action(() =>
                {
                    bindingdata.CH6_List.Add(GetOCR1ViewDataClass("man-" + guidStr));
                }));

                ShowLog("分道6", "增加一瓶,瓶码：" + bindingdata.CH6_List[bindingdata.CH6_List.Count - 1].ToString());
                AloneLog.AddInfoLog("Channel", "分道6：增加一瓶,瓶码：" + bindingdata.CH6_List[bindingdata.CH6_List.Count - 1].ToString() + "\r\n");
            }
        }
        #endregion

        /// <summary>
        /// 清空ocr1数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearOCR1_Click(object sender, EventArgs e)
        {
            if (Common.Common.TS("此操作会清空ocr1数据队列，只在瓶箱关联异常纠正时使用， 确认要清空ocr1数据吗？"))
            {
                if (bindingdata.OCR1List.Count > 0)
                {
                    lock (_lockObj)
                    {
                        foreach (var item in bindingdata.OCR1List)
                        {
                            item.Name = "OCR1List";
                            _ocr1Queue_remove.Enqueue(item);
                            waitRedisRemove.Set();
                        }
                        bindingdata.OCR1List.Clear();
                    }
                    MessageBox.Show("清空成功！");
                }
                else
                {
                    MessageBox.Show("没有可清空的数据！");
                }
                lastBarcode = string.Empty;//清空ocr1比对的数据 by 2021.7.20 徐元丰
            }
        }

        /// <summary>
        /// PQMS写入短信队列
        /// </summary>
        /// <param name="error"></param>
        void SendPQMSMsg(bool isStartProduce, out string error)
        {
            error = "";
            #region  写入PQMS短信 开始生产
            if (isPQMSSku || isDPCA)
            {
                Common.Common.SendMsg(m_set.ProductNo, m_set.Batch, m_set.req_qty, m_set.ProduceDate, isStartProduce, out error);
                string title = "写入短信队列";
                string logs = "";
                string logs_details = "";
                string skuInfo = m_set.ProductNo + " " + m_set.Batch + " " + m_set.req_qty;
                if (error != "")
                {
                    string errorMsg = error.Split('@')[0];
                    string errorStack = error.Split('@')[1];
                    logs = "失败：" + errorMsg;
                    logs_details = "失败：" + skuInfo + " " + error;
                    AddErrorLog(true, title, logs, title, logs_details);
                    MessageBox.Show("异常:" + errorMsg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                logs = "成功：" + skuInfo;
                AddInfoLog(title, logs);
            }
            #endregion
        }

        /// <summary>
        /// 启动PLC 开始生产
        /// </summary>
        void Start_PLC_Counte()
        {
            socket_PLC_Sender.Send(Encoding.Default.GetBytes("#ALLSTT%"));
        }

        /// <summary>
        /// 停止PLC 结束生产
        /// </summary>
        void Stop_PLC_Counte()
        {
            try
            {
                socket_PLC_Sender.Send(Encoding.Default.GetBytes("#ALLSTP%"));
            }
            catch (Exception ex)
            {
                string msg = ex.Message + "," + ex.StackTrace;
                AddErrorLog(true, "停止PLC计数-异常", msg, "停止PLC计数-异常", msg);
            }
        }

        /// <summary>
        /// 发送开始生产信号给PLC
        /// msg 发送内容
        /// </summary>
        void Start_Produce_Signal(string msg)
        {
            try
            {
                if (socket_PLC_Sender != null)
                {
                    socket_PLC_Sender.Send(Encoding.Default.GetBytes(msg));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void btnRestPLC_Click(object sender, EventArgs e)
        {
            if (Common.Common.TS("此操作会重置PLC,确认重置吗？"))
            {
                try
                {
                    Stop_PLC_Counte();
                    Thread.Sleep(1000 * sendPLCSignalDelay);
                    Start_PLC_Counte();
                    //重置箱码、分道漏触发相关变量
                    ResetPLCCountVar();

                    MessageBox.Show("重置PLC成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("重置PLC失败," + ex.Message);
                    string msg = "重置PLC失败," + ex.Message + "," + ex.StackTrace;
                    AddErrorLog(false, "重置PLC", msg, "重置PLC", msg);
                }
            }
        }

        //韦迪捷喷码机发送端口连接
        public void Start_Send_Videojet_1()
        {
            try
            {
                if (!sendSocket1.Connected)
                {
                    if (sendSocket1.OpenClient(Common.Config.ReadValue("PMJ", "IP1"), 3100))
                    {
                        sendSocket1.SockReceiveData += sendSocket1_DataReceived;
                        AddInfoLog("喷码机1初始化", "连接成功");
                    }
                    else
                    {
                        AddInfoLog("喷码机1初始化", "发送数据端口连接失败！");
                        sendSocket1.CloseClient();
                        MessageBox.Show("喷码机1初始化，发送数据端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机1初始化", "发送端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1接收端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机1接收端口连接失败", ex.Message + " " + ex.StackTrace);
                sendSocket1.CloseClient();
                MessageBox.Show(ex.Message, "喷码机1接收端口连接失败");
                return;
            }
        }



        //韦迪捷喷码机接收端口连接
        public void Start_Ack_Videojet_1()
        {
            try
            {
                if (!ackSocket1.Connected)
                {
                    if (ackSocket1.OpenClient(Common.Config.ReadValue("PMJ", "IP2"), 3101))//3101
                    {
                        ackSocket1.SockReceiveData += ackSocket1_DataReceived;
                    }
                    else
                    {
                        AddInfoLog("喷码机1初始化", "接收端口连接失败！");
                        MessageBox.Show("喷码机1初始化,接收端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机1初始化", "接收端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1发送数据端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机1发送数据端口连接失败", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message, "喷码机1发送数据端口连接失败");
                return;
            }
        }
        //关闭韦迪捷喷码机发送端口
        public void Close_Send_Videojet_1()
        {
            sendSocket1.SockReceiveData -= sendSocket1_DataReceived;
            sendSocket1.CloseClient();
        }
        //关闭韦迪捷喷码机接收端口
        public void Close_Ack_Videojet_1()
        {
            ackSocket1.SockReceiveData -= ackSocket1_DataReceived;
            ackSocket1.CloseClient();
        }
        Common.Printer printer = new Printer();//封装打印类
        //接收打印完成端数据接收，判断指令是否执行成功
        int iPrinted1 = 0;
        private void ackSocket1_DataReceived(int iLeng, string readstring)
        {
            iPrinted1++;
            try
            {
                if (printer.Videojet1510_CheckAck(readstring))
                {
                    AddInfoLog("喷码机1", "打印完成！" + readstring + "#");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机1数据接收异常");
                return;
            }
        }
        //发送端数据接收，判断指令是否执行成功
        private void sendSocket1_DataReceived(int iLeng, string readstring)
        {
            try
            {
                if (printer.Videojet1510_CheckDone(readstring))
                {
                    AddInfoLog("喷码机1", "接收指令成功！" + readstring);
                }
                else
                {
                    AddInfoLog("喷码机1", "接收指令失败！" + readstring);
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机1数据接收异常");
                return;
            }
        }
        //韦迪捷喷码机发送端口连接
        public void Start_Send_Videojet_2()
        {
            try
            {
                if (!sendSocket2.Connected)
                {
                    if (sendSocket2.OpenClient(Common.Config.ReadValue("PMJ", "IP2"), 3100))
                    {
                        sendSocket2.SockReceiveData += sendSocket2_DataReceived;
                        AddInfoLog("喷码机2初始化", "连接成功");
                    }
                    else
                    {
                        AddInfoLog("喷码机2初始化", "发送数据端口连接失败！");
                        sendSocket2.CloseClient();
                        MessageBox.Show("喷码机2初始化,发送数据端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机2初始化", "发送端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2接收端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机2接收端口连接失败", ex.Message + " " + ex.StackTrace);
                sendSocket2.CloseClient();
                MessageBox.Show(ex.Message, "喷码机2接收端口连接失败");
                return;
            }
        }
        //韦迪捷喷码机接收端口连接
        public void Start_Ack_Videojet_2()
        {
            try
            {
                if (!ackSocket2.Connected)
                {
                    if (ackSocket2.OpenClient(Common.Config.ReadValue("PMJ", "IP2"), 3101))//3101
                    {
                        ackSocket2.SockReceiveData += ackSocket2_DataReceived;
                    }
                    else
                    {
                        AddInfoLog("喷码机2初始化", "接收端口连接失败！");
                        MessageBox.Show("喷码机2初始化,接收端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机2初始化", "接收端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2发送数据端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机2发送数据端口连接失败", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message, "喷码机2发送数据端口连接失败");
                return;
            }
        }
        //关闭韦迪捷喷码机发送端口
        public void Close_Send_Videojet_2()
        {
            sendSocket2.SockReceiveData -= sendSocket2_DataReceived;
            sendSocket2.CloseClient();
        }
        //关闭韦迪捷喷码机接收端口
        public void Close_Ack_Videojet_2()
        {
            ackSocket2.SockReceiveData -= ackSocket2_DataReceived;
            ackSocket2.CloseClient();
        }
        //接收打印完成端数据接收，判断指令是否执行成功
        int iPrinted2 = 0;
        private void ackSocket2_DataReceived(int iLeng, string readstring)
        {
            iPrinted2++;
            try
            {
                if (printer.Videojet1510_CheckAck(readstring))
                {
                    AddInfoLog("喷码机2", "打印完成！" + readstring + "#");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机2数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机2数据接收异常");
                return;
            }
        }
        //发送端数据接收，判断指令是否执行成功
        private void sendSocket2_DataReceived(int iLeng, string readstring)
        {
            try
            {
                if (printer.Videojet1510_CheckDone(readstring))
                {
                    AddInfoLog("喷码机2", "接收指令成功！" + readstring);
                }
                else
                {
                    AddInfoLog("喷码机2", "接收指令失败！" + readstring);
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机2数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机2数据接收异常");
                return;
            }
        }
        public void Start_Send_Videojet_1_1()
        {
            try
            {
                if (!sendSocket1_1.Connected)
                {
                    if (sendSocket1_1.OpenClient(Common.Config.ReadValue("PMJ", "IP1_1"), 3100))
                    {
                        sendSocket1_1.SockReceiveData += sendSocket1_1_DataReceived;
                        AddInfoLog("喷码机1_1初始化", "连接成功");
                    }
                    else
                    {
                        AddInfoLog("喷码机1_1初始化", "发送数据端口连接失败！");
                        sendSocket1_1.CloseClient();
                        MessageBox.Show("喷码机1_1初始化,发送数据端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机1_1初始化", "发送端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1_1接收端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机1_1接收端口连接失败", ex.Message + " " + ex.StackTrace);
                sendSocket1_1.CloseClient();
                MessageBox.Show(ex.Message, "喷码机1_1接收端口连接失败");
                return;
            }
        }
        //韦迪捷喷码机接收端口连接
        public void Start_Ack_Videojet_1_1()
        {
            try
            {
                if (!ackSocket1_1.Connected)
                {
                    if (ackSocket1_1.OpenClient(Common.Config.ReadValue("PMJ", "IP1_1"), 3101))//3101
                    {
                        ackSocket1_1.SockReceiveData += ackSocket1_1_DataReceived;
                    }
                    else
                    {
                        AddInfoLog("喷码机1_1初始化", "接收端口连接失败！");
                        MessageBox.Show("喷码机1_1初始化,接收端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机1_1初始化", "接收端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1_1发送数据端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机1_1发送数据端口连接失败", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message, "喷码机1_1发送数据端口连接失败");
                return;
            }
        }
        //关闭韦迪捷喷码机发送端口
        public void Close_Send_Videojet_1_1()
        {
            sendSocket1_1.SockReceiveData -= sendSocket1_1_DataReceived;
            sendSocket1_1.CloseClient();
        }
        //关闭韦迪捷喷码机接收端口
        public void Close_Ack_Videojet_1_1()
        {
            ackSocket1_1.SockReceiveData -= ackSocket1_1_DataReceived;
            ackSocket1_1.CloseClient();
        }
        //接收打印完成端数据接收，判断指令是否执行成功
        int iPrinted1_1 = 0;
        private void ackSocket1_1_DataReceived(int iLeng, string readstring)
        {
            iPrinted1_1++;
            try
            {
                if (printer.Videojet1510_CheckAck(readstring))
                {
                    AddInfoLog("喷码机1_1", "打印完成！" + readstring + "#");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1_1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机1_1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机1_1数据接收异常");
                return;
            }
        }
        //发送端数据接收，判断指令是否执行成功
        private void sendSocket1_1_DataReceived(int iLeng, string readstring)
        {
            try
            {
                if (printer.Videojet1510_CheckDone(readstring))
                {
                    AddInfoLog("喷码机1_1", "接收指令成功！" + readstring);
                }
                else
                {
                    AddInfoLog("喷码机1_1", "接收指令失败！" + readstring);
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机1_1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机1_1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机1_1数据接收异常");
                return;
            }
        }
        public void Start_Send_Videojet_2_1()
        {
            try
            {
                if (!sendSocket2_1.Connected)
                {
                    if (sendSocket2_1.OpenClient(Common.Config.ReadValue("PMJ", "IP2_1"), 3100))
                    {
                        sendSocket2_1.SockReceiveData += sendSocket2_1_DataReceived;
                        AddInfoLog("喷码机2_1初始化", "连接成功");
                    }
                    else
                    {
                        AddInfoLog("喷码机2_1初始化", "发送数据端口连接失败！");
                        sendSocket2_1.CloseClient();
                        MessageBox.Show("喷码机2_1初始化,发送数据端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机2_1初始化", "发送端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1接收端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机2_1接收端口连接失败", ex.Message + " " + ex.StackTrace);
                sendSocket2_1.CloseClient();
                MessageBox.Show(ex.Message, "喷码机2_1接收端口连接失败");
                return;
            }
        }
        //韦迪捷喷码机接收端口连接
        public void Start_Ack_Videojet_2_1()
        {
            try
            {
                if (!ackSocket2_1.Connected)
                {
                    if (ackSocket2_1.OpenClient(Common.Config.ReadValue("PMJ", "IP2_1"), 3101))//3101
                    {
                        ackSocket2_1.SockReceiveData += ackSocket2_1_DataReceived;
                    }
                    else
                    {
                        AddInfoLog("喷码机2_1初始化", "接收端口连接失败！");
                        MessageBox.Show("喷码机2_1初始化,接收端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机2_1初始化", "接收端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1发送数据端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机2_1发送数据端口连接失败", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message, "喷码机2_1发送数据端口连接失败");
                return;
            }
        }
        //关闭韦迪捷喷码机发送端口
        public void Close_Send_Videojet_2_1()
        {
            sendSocket2_1.SockReceiveData -= sendSocket2_1_DataReceived;
            sendSocket2_1.CloseClient();
        }
        //关闭韦迪捷喷码机接收端口
        public void Close_Ack_Videojet_2_1()
        {
            ackSocket2_1.SockReceiveData -= ackSocket2_1_DataReceived;
            ackSocket2_1.CloseClient();
        }
        //接收打印完成端数据接收，判断指令是否执行成功
        int iPrinted2_1 = 0;
        private void ackSocket2_1_DataReceived(int iLeng, string readstring)
        {
            iPrinted2_1++;
            try
            {
                if (printer.Videojet1510_CheckAck(readstring))
                {
                    AddInfoLog("喷码机2_1", "打印完成！" + readstring + "#");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常");
                return;
            }
        }
        //发送端数据接收，判断指令是否执行成功
        private void sendSocket2_1_DataReceived(int iLeng, string readstring)
        {
            try
            {
                if (printer.Videojet1510_CheckDone(readstring))
                {
                    AddInfoLog("喷码机2_1", "接收指令成功！" + readstring);
                }
                else
                {
                    AddInfoLog("喷码机2_1", "接收指令失败！" + readstring);
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常");
                return;
            }
        }
        public void Start_Send_Videojet_3_1()
        {
            try
            {
                if (!sendSocket3_1.Connected)
                {
                    if (sendSocket3_1.OpenClient(Common.Config.ReadValue("PMJ", "IP3_1"), 3100))
                    {
                        sendSocket3_1.SockReceiveData += sendSocket3_1_DataReceived;
                        AddInfoLog("喷码机2_1初始化", "连接成功");
                    }
                    else
                    {
                        AddInfoLog("喷码机2_1初始化", "发送数据端口连接失败！");
                        sendSocket3_1.CloseClient();
                        MessageBox.Show("喷码机2_1初始化,发送数据端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机2_1初始化", "发送端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1接收端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机2_1接收端口连接失败", ex.Message + " " + ex.StackTrace);
                sendSocket3_1.CloseClient();
                MessageBox.Show(ex.Message, "喷码机2_1接收端口连接失败");
                return;
            }
        }
        //韦迪捷喷码机接收端口连接
        public void Start_Ack_Videojet_3_1()
        {
            try
            {
                if (!ackSocket3_1.Connected)
                {
                    if (ackSocket3_1.OpenClient(Common.Config.ReadValue("PMJ", "IP3_1"), 3101))//3101
                    {
                        ackSocket3_1.SockReceiveData += ackSocket3_1_DataReceived;
                    }
                    else
                    {
                        AddInfoLog("喷码机2_1初始化", "接收端口连接失败！");
                        MessageBox.Show("喷码机2_1初始化,接收端口连接失败！");
                        return;
                    }
                }
                else
                {
                    AddInfoLog("喷码机2_1初始化", "接收端口已经连接！");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1发送数据端口连接失败", ex.Message + " " + ex.StackTrace, "喷码机2_1发送数据端口连接失败", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message, "喷码机2_1发送数据端口连接失败");
                return;
            }
        }
        //关闭韦迪捷喷码机发送端口
        public void Close_Send_Videojet_3_1()
        {
            sendSocket3_1.SockReceiveData -= sendSocket3_1_DataReceived;
            sendSocket3_1.CloseClient();
        }
        //关闭韦迪捷喷码机接收端口
        public void Close_Ack_Videojet_3_1()
        {
            ackSocket3_1.SockReceiveData -= ackSocket3_1_DataReceived;
            ackSocket3_1.CloseClient();
        }
        //接收打印完成端数据接收，判断指令是否执行成功
        int iPrinted3_1 = 0;
        private void ackSocket3_1_DataReceived(int iLeng, string readstring)
        {
            iPrinted3_1++;
            try
            {
                if (printer.Videojet1510_CheckAck(readstring))
                {
                    AddInfoLog("喷码机2_1", "打印完成！" + readstring + "#");
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常");
                return;
            }
        }
        //发送端数据接收，判断指令是否执行成功
        private void sendSocket3_1_DataReceived(int iLeng, string readstring)
        {
            try
            {
                if (printer.Videojet1510_CheckDone(readstring))
                {
                    AddInfoLog("喷码机2_1", "接收指令成功！" + readstring);
                }
                else
                {
                    AddInfoLog("喷码机2_1", "接收指令失败！" + readstring);
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常", ex.Message + " " + ex.StackTrace);
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "喷码机2_1数据接收异常");
                return;
            }
        }
        string bb, aa2, aa10, aa3;
        /// <summary>
        /// 发送信息给喷码机
        /// </summary>
        /// <param name="socketHelper">喷码机通信变量</param>
        /// <param name="jobName">模板</param>
        /// <param name="bLName">变量</param>
        /// <param name="sendMessage">发送内容</param>
        private void SendMessage_Videojet(SocketHelper socketHelper, string IP, string jobName, string bLName, string sendMessage)
        {
            if (socketHelper != null && socketHelper.Active)
            {
                socketHelper.SendClient(printer.Videojet1510_Select(jobName));//选择模板，注根据喷码机配置
                AddInfoLog("喷码机", "IP:" + IP + ";切换打印模版:" + jobName + ";变量:" + bLName + ";发送内容:" + sendMessage);
                ASCIIEncoding asc = new ASCIIEncoding();

                byte[] byteaa2 = new byte[] { (byte)2 };
                aa2 = asc.GetString(byteaa2);

                byte[] byteaa10 = new byte[] { (byte)10 };
                aa10 = asc.GetString(byteaa10);

                byte[] byteaa3 = new byte[] { (byte)3 };
                aa3 = asc.GetString(byteaa3);

                bb = "";

                bb = aa2 + "U" + bLName + aa10 + sendMessage + aa3;//发送内容格式，bLName和上面jobName必须和喷码机配置对上，不然发送打印会失败

                int ret = socketHelper.SendClient(bb);
                if (ret != 0)
                {
                    AddInfoLog("喷码机", IP + "发送打印内容成功！" + ret);
                }
                else
                {
                    AddInfoLog("喷码机", IP + "发送打印内容失败！" + ret);
                    MessageBox.Show(IP + "发送打印内容失败！" + ret, "喷码机");
                    return;
                }
            }
            else
            {
                AddErrorLog(true, "喷码机端口连接失败", "喷码机" + IP + "切换模板时，连接失败", "喷码机端口连接失败", "喷码机" + IP + "切换模板时，连接失败");
                MessageBox.Show("请先连接喷码机！", "喷码机");
                return;
            }
        }
        /// <summary>
        /// 获取9位编码
        /// </summary>
        /// <param name="lot_no">产品批号</param>
        private string GetPMJ_Message(string lot_no)
        {
            byte[] array = new byte[1];   //定义一组数组array
            array = System.Text.Encoding.ASCII.GetBytes(lot_no.Substring(0, 3)); //string转换的字母
            return array[0].ToString() + array[1].ToString() + array[2].ToString() + lot_no.Substring(3); //将转换一的ASCII码转换成string型
        }
        /// <summary>
        /// 加载Videojet喷码机
        /// </summary>
        private void Videojet_Load()
        {
            if (Common.Config.ReadValue("PMJ", "Isable1") == "true" && Common.Config.ReadValue("PMJ", "Isable2") == "true")
            {
                lb_msg.Text = " 喷码机连线配置已开启，等待连接";
            }
            else if (Common.Config.ReadValue("PMJ", "Isable1") == "true" && Common.Config.ReadValue("PMJ", "Isable2") == "false")
            {
                lb_msg.Text = " 喷码机1连线配置已开启,喷码机2连线配置失败";
                lb_msg.ForeColor = Color.Red;
            }
            else if (Common.Config.ReadValue("PMJ", "Isable1") == "false" && Common.Config.ReadValue("PMJ", "Isable2") == "true")
            {
                lb_msg.Text = " 喷码机1连线配置失败,喷码机2连线配置已开启";
                lb_msg.ForeColor = Color.Red;
            }
            else
            {
                lb_msg.Text = " 喷码机连线配置失败";
                lb_msg.ForeColor = Color.Red;
            }
        }
        /// <summary>
        /// 开启Videojet喷码机(防伪码)
        /// </summary>
        private void Videojet_Start_FWCode(string batch)
        {
            if (Common.Config.ReadValue("PMJ", "Isable1") == "true")
            {
                Start_Send_Videojet_1();
                //Start_Ack_Videojet_1();//不开启，开启后再关闭会有bug，线程无法中止                      
                string nineTemp = GetPMJ_Message(batch);//获取9位编码
                if (nineTemp.Trim().Length != 9)
                {
                    AddErrorLog(true, "喷码机获取9位编码有误", "编码为" + nineTemp.Trim(), "喷码机获取9位编码有误", "编码为" + nineTemp.Trim());
                    MessageBox.Show("喷码机1获取9位编码有误!", "错误");
                    return;
                }
                SendMessage_Videojet(sendSocket1, Common.Config.ReadValue("PMJ", "IP1"), Common.Config.ReadValue("PMJ", "Job1"), Common.Config.ReadValue("PMJ", "BL1"), nineTemp.Trim());
                lbl_title2.Text += nineTemp.Trim();
            }
            if (Common.Config.ReadValue("PMJ", "Isable2") == "true")
            {
                Start_Send_Videojet_2();
                //Start_Ack_Videojet_2();//不开启，开启后再关闭会有bug，线程无法中止
                SendMessage_Videojet(sendSocket2, Common.Config.ReadValue("PMJ", "IP2"), Common.Config.ReadValue("PMJ", "Job2"), Common.Config.ReadValue("PMJ", "BL2"), batch + " " + DateTime.Parse(m_set.ProduceDate).ToString("yyyy/MM/dd"));
                lbl_title2.Text += " " + batch + " " + DateTime.Parse(m_set.ProduceDate).ToString("yyyy/MM/dd");
            }
            Thread.Sleep(2000);
            try
            {
                Close_Send_Videojet_1();//关闭喷码机发送
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机1发送端口-异常", ex.Message, "停止喷码机1发送端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Ack_Videojet_1();//关闭喷码机接收
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机1接收端口-异常", ex.Message, "停止喷码机1接收端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Send_Videojet_2();//关闭喷码机发送
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机2发送端口-异常", ex.Message, "停止喷码机2发送端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Ack_Videojet_2();//关闭喷码机接收
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机2接收端口-异常", ex.Message, "停止喷码机2接收端口-异常", ex.Message + " " + ex.StackTrace);
            }
        }

        private void btnDeleteBox_Click(object sender, EventArgs e)
        {
            FormDeleteBoxsNumber frm = new FormDeleteBoxsNumber();
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {
                if (bindingdata.OCR3List.Count > 0)
                {
                    bool br = false;
                    OCR1ViewDataClass data;
                    int nubmer = int.Parse(frm.BoxNumber);
                    for (int i = 0; i < nubmer; i++)
                    {
                        if (bindingdata.OCR3List.Count > 0)
                        {
                            data = bindingdata.OCR3List[0];

                            br = OCR_Box_List.Remove(data.Allcode);
                            if (br)
                            {
                                if (bindingdata.OCR3List.Contains(data))
                                {
                                    this.Invoke(new Action(() =>
                                    {
                                        lock (_lockObj)
                                        {
                                            bindingdata.OCR3List.Remove(data);
                                        }
                                    }));
                                   
                                }
                                AddInfoLog("删除箱子", "箱码：" + data);
                            }
                            else
                            {
                                MessageBox.Show("剔除失败");
                                break;
                            }
                        }
                    }

                }
            }
            else if (frm.DialogResult == DialogResult.Cancel)
            {
                MessageBox.Show("没有要删除的数据!");
                return;
            }
        }

        /// <summary>
        /// 开启Videojet喷码机(非防伪码)
        /// </summary>
        private void Videojet_Start_NoFWCode(string batch)
        {
            try
            {
                string sql_vidowjet = "select * from qx_printdata where site_no='" + m_set.site_no + "' and lot_no='" + m_set.Batch + "' and sku_no='" + m_set.ProductNo + "'";
                string encsql_vidowjet = Common.MD5ALGO.Encrypt(sql_vidowjet);
                string link = Common.Config.ReadValue("LinkServer", "ws") + "/webservice1.asmx";
                ServiceReference2.WebService1SoapClient ws = new ServiceReference2.WebService1SoapClient("WebService1Soap1", link);
                DataSet ds_vidowjet = ws.ExecuteDataSet(encsql_vidowjet);
                if (Common.Config.ReadValue("PMJ", "Isable1_1") == "true")
                {
                    if (ds_vidowjet != null && ds_vidowjet.Tables.Count > 0)
                    {
                        Start_Send_Videojet_1_1();
                        if (!Common.Common.CheckHasCacheData(m_set.OrderNo, out string error))//判断是否“暂停生产”，如果“暂停生产”就不发送数据
                        {
                            SendMessage_Videojet(sendSocket1_1, Common.Config.ReadValue("PMJ", "IP1_1"), Common.Config.ReadValue("PMJ", "Job1_1"), Common.Config.ReadValue("PMJ", "BL1_1"), ds_vidowjet.Tables[0].Rows[0]["Start_box_sn"].ToString());
                            lbl_title2.Text += ds_vidowjet.Tables[0].Rows[0]["Start_box_sn"].ToString();
                        }
                        else
                        {
                            lbl_title2.Text += "非防伪产品跨天生产，不发送流水号！";
                        }
                    }
                    else
                    {
                        AddErrorLog(true, "获取产品流水号失败", "获取产品流水号失败", "获取产品流水号失败", "获取产品流水号失败");
                        MessageBox.Show("获取产品流水号失败！", "错误");
                        return;
                    }
                }
                if (Common.Config.ReadValue("PMJ", "Isable2_1") == "true")
                {
                    Start_Send_Videojet_2_1();
                    SendMessage_Videojet(sendSocket2_1, Common.Config.ReadValue("PMJ", "IP2_1"), Common.Config.ReadValue("PMJ", "Job2_1"), Common.Config.ReadValue("PMJ", "BL2_1"), batch + " " + DateTime.Parse(m_set.ProduceDate).ToString("yyyy/MM/dd"));
                    lbl_title2.Text += " " + batch + " " + DateTime.Parse(m_set.ProduceDate).ToString("yyyy/MM/dd");
                }
                if (Common.Config.ReadValue("PMJ", "Isable3_1") == "true")
                {
                    if (ds_vidowjet != null && ds_vidowjet.Tables.Count > 0)
                    {
                        Start_Send_Videojet_3_1();
                        if (!Common.Common.CheckHasCacheData(m_set.OrderNo, out string error))//判断是否“暂停生产”，如果“暂停生产”就不发送数据
                        {
                            SendMessage_Videojet(sendSocket3_1, Common.Config.ReadValue("PMJ", "IP3_1"), Common.Config.ReadValue("PMJ", "Job3_1"), Common.Config.ReadValue("PMJ", "BL3_1"), ds_vidowjet.Tables[0].Rows[0]["Start_box_sn"].ToString());
                            lbl_title2.Text += ds_vidowjet.Tables[0].Rows[0]["Start_box_sn"].ToString();
                        }
                        else
                        {
                            lbl_title2.Text += "非防伪产品跨天生产，不发送流水号！";
                        }
                    }
                    else
                    {
                        AddErrorLog(true, "获取产品流水号失败", "获取产品流水号失败", "获取产品流水号失败", "获取产品流水号失败");
                        MessageBox.Show("获取产品流水号失败！", "错误");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "喷码机获取服务器上数据失败", ex.Message, "喷码机获取服务器上数据失败", ex.Message + " " + ex.StackTrace);
                MessageBox.Show("喷码机获取服务器上数据失败！", "错误");
                return;
            }
            Thread.Sleep(2000);
            try
            {
                Close_Send_Videojet_1_1();//关闭喷码机发送
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机1_1发送端口-异常", ex.Message, "停止喷码机1_1发送端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Ack_Videojet_1_1();//关闭喷码机接收
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机1_1接收端口-异常", ex.Message, "停止喷码机1_1接收端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Send_Videojet_2_1();//关闭喷码机发送
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机2_1发送端口-异常", ex.Message, "停止喷码机2_1发送端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Ack_Videojet_2_1();//关闭喷码机接收
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机2_1接收端口-异常", ex.Message, "停止喷码机2_1接收端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Send_Videojet_3_1();//关闭喷码机发送
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机2_1发送端口-异常", ex.Message, "停止喷码机2_1发送端口-异常", ex.Message + " " + ex.StackTrace);
            }
            try
            {
                Close_Ack_Videojet_3_1();//关闭喷码机接收
            }
            catch (Exception ex)
            {
                AddErrorLog(true, "停止喷码机2_1接收端口-异常", ex.Message, "停止喷码机2_1接收端口-异常", ex.Message + " " + ex.StackTrace);
            }
        }
        //引单
        private void ImportOrderNo_Click(object sender, EventArgs e)
        {
            FormImportOrderNo fm = new FormImportOrderNo();
            DialogResult dr = fm.ShowDialog();

            if (dr == DialogResult.OK)
            {
                tDocno.Text = m_set.OrderNo;

                tDocInfo.Text = "产品: " + m_set.ProductNo + "," + m_set.ProductName + ",产地=" + m_set.Madein + "\r\n" + "批号: " + m_set.Batch + ",日期: " + m_set.doc_date
                    + ", 数量: " + m_set.act_qty + "/" + m_set.req_qty + "\r\n 外箱: " + m_set.box_pack_maxqty + ", 托盘: " + m_set.pallet_pack_maxqty + ", 关联: " + m_set.pack_relation;

                AddInfoLog("搜索生产任务", "doc_no=" + m_set.OrderNo + ",doc_date=" + m_set.doc_date + ",Madein=" + m_set.Madein + ",sku_no=" + m_set.ProductNo + ",lot_no=" + m_set.Batch + ",qty=" + m_set.act_qty + "/" + m_set.req_qty);

            }

            if (tDocno.Text != "")
            {
                btn_Start.Enabled = true;
            }
            else
            {
                btn_Start.Enabled = false;
            }
        }

        private void view_box2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = view_box2.CurrentRow.Index;
            string strNumLast = "A";
            FormInputBox fm = new FormInputBox();
            FormInputBox.m_text = "选中的箱码：" + bindingdata.OCR3List[index].Allcode;
            FormInputBox.m_result = "";
            DialogResult dr = fm.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                if (Common.Common.TS("确认要删除" + bindingdata.OCR3List[index].Allcode + "数据吗？"))
                {
                    AddInfoLog("删除箱子（软件）OCR3", "箱码：" + bindingdata.OCR3List[index].Allcode);
                    AddErrorLog4(true, "删除箱子（软件）OCR3", "箱码：" + bindingdata.OCR3List[index].Allcode, "箱码：" + bindingdata.OCR3List[index].Allcode, bindingdata.OCR3List[index].Allcode, true, m_set.ProductNo, m_set.Batch);

                    this.Invoke(new Action(() =>
                    {
                        lock (_lockObj)
                        {
                            bindingdata.OCR3List.RemoveAt(index);
                        }
                    }));
                   

                }
            }
            else if (dr == DialogResult.Yes)
            {
                FormInputBox.m_text = "选中的箱码：" + bindingdata.OCR3List[index].Allcode + "。如要修改，请输入9位箱码（不符合要求，不能修改）";
                string tempBoxCode = string.Empty;
                if (Regex.IsMatch(FormInputBox.m_result.Trim(), @"^[+-]?\d*[.]?\d*$"))
                {
                    if (bindingdata.OCR3List[index].Allcode.Trim().Substring(0, 3) == "ocr")
                    {
                        for (int i = ZJ_LN.Length; i < 3; i++)
                        {
                            if (ZJ_LN.Length < 3)
                            {
                                ZJ_LN = ZJ_LN + "0";
                            }
                        }
                        if (m_set.ProduceLine == "4L")
                        {
                            //只有镇江工厂的4L产品才是A,其余工厂都是D
                            strNumLast = "A";
                        }
                        if (m_set.ProduceLine == "1L")
                        {
                            strNumLast = "D";
                        }
                        if (m_set.ProduceLine == "大标签")
                        {
                            strNumLast = "B";
                        }
                        if (Common.Config.ReadValue("Line", "type").Trim().Length == 0)
                        {
                            tempBoxCode = "A" + m_set.ProductNo + m_set.Batch + m_set.ProduceDate.Substring(2, 2) + m_set.ProduceDate.Substring(5, 2) + m_set.ProduceDate.Substring(8, 2) + ZJ_LN + FormInputBox.m_result.Trim() + strNumLast;//镇江开头为A；天津为B；广州为C；
                        }
                        else
                        {
                            tempBoxCode = Common.Config.ReadValue("Line", "type") + m_set.ProductNo + m_set.Batch + m_set.ProduceDate.Substring(2, 2) + m_set.ProduceDate.Substring(5, 2) + m_set.ProduceDate.Substring(8, 2) + ZJ_LN + FormInputBox.m_result.Trim() + strNumLast;//镇江开头为A；天津为B；广州为C；
                        }
                    }
                    else
                    {
                        tempBoxCode = bindingdata.OCR3List[index].Allcode.Trim().Substring(0, 21) + FormInputBox.m_result.Trim() + bindingdata.OCR3List[index].Allcode.Trim().Substring(bindingdata.OCR3List[index].Allcode.Length - 1, 1);
                    }

                    if (bindingdata.OCR3List.Any(a => a.Allcode == tempBoxCode.Trim()))
                    {
                        MessageBox.Show("您输入的箱码重复了，请再次核对一下要修改的箱码！");
                        return;
                    }
                    else
                    {
                        if (Common.Common.TS("确认要把" + bindingdata.OCR3List[index].Allcode + "数据修改成" + tempBoxCode.Trim() + "吗？"))
                        {
                            AddInfoLog("修改箱子（软件）OCR3", "OCR3 箱码：" + bindingdata.OCR3List[index].Allcode + "OCR3 改成：" + tempBoxCode.Trim());
                            AddErrorLog4(true, "修改箱子（软件）OCR3", "OCR3 改成：" + tempBoxCode.Trim(), "OCR3 箱码：" + bindingdata.OCR3List[index].Allcode + "  OCR3 改成：" + tempBoxCode.Trim(), "  OCR3 已改成的箱码：" + tempBoxCode.Trim(), true, m_set.ProductNo, m_set.Batch);
                            bindingdata.OCR3List[index] = GetOCR2ViewDataClass(tempBoxCode.Trim());
                        }
                    }
                }
                else
                {
                    MessageBox.Show("您输入的箱码有误，请检查一下！");
                    return;
                }
            }
            else if (dr == DialogResult.No)
            {
                MessageBox.Show("修改失败，填写的箱码格式有错误！");
                return;
            }
            else
            {
                return;
            }
        }

        private void btnClearChannel_Click(object sender, EventArgs e)
        {
            if (Common.Common.TS("此操作会清空分道PLC数据队列，只在瓶箱关联异常纠正时使用， 确认要清空分道PLC数据吗？"))
            {
                int number = bindingdata.CH1_List.Count;
                if (bindingdata.CH1_List.Count > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        lock (lock_ch1_list)
                        {
                            ShowLog("分道1", "减少一瓶,瓶码：" + bindingdata.CH1_List[bindingdata.CH1_List.Count - 1].ToString());
                            AloneLog.AddInfoLog("Channel", "分道1：减少一瓶,瓶码：" + bindingdata.CH1_List[bindingdata.CH1_List.Count - 1].ToString() + "\r\n");
                            this.Invoke(new Action(() =>
                            {

                                bindingdata.CH1_List.RemoveAt(bindingdata.CH1_List.Count - 1);
                            }));
                        }
                    }
                }

                number = bindingdata.CH2_List.Count;
                if (bindingdata.CH2_List.Count > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        lock (lock_ch2_list)
                        {
                            ShowLog("分道2", "减少一瓶,瓶码：" + bindingdata.CH2_List[bindingdata.CH2_List.Count - 1].ToString());
                            AloneLog.AddInfoLog("Channel", "分道2：减少一瓶,瓶码：" + bindingdata.CH2_List[bindingdata.CH2_List.Count - 1].ToString() + "\r\n");
                            this.Invoke(new Action(() =>
                            {

                                bindingdata.CH2_List.RemoveAt(bindingdata.CH2_List.Count - 1);
                            }));
                        }

                    }
                }

                number = bindingdata.CH3_List.Count;
                if (bindingdata.CH3_List.Count > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        lock (lock_ch3_list)
                        {
                            ShowLog("分道3", "减少一瓶,瓶码：" + bindingdata.CH3_List[bindingdata.CH3_List.Count - 1].ToString());
                            AloneLog.AddInfoLog("Channel", "分道3：减少一瓶,瓶码：" + bindingdata.CH3_List[bindingdata.CH3_List.Count - 1].ToString() + "\r\n");
                            this.Invoke(new Action(() =>
                            {

                                bindingdata.CH3_List.RemoveAt(bindingdata.CH3_List.Count - 1);
                            }));
                        }

                    }
                }

                number = bindingdata.CH4_List.Count;
                if (bindingdata.CH4_List.Count > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        lock (lock_ch4_list)
                        {
                            ShowLog("分道4", "减少一瓶,瓶码：" + bindingdata.CH4_List[bindingdata.CH4_List.Count - 1].ToString());
                            AloneLog.AddInfoLog("Channel", "分道4：减少一瓶,瓶码：" + bindingdata.CH4_List[bindingdata.CH4_List.Count - 1].ToString() + "\r\n");
                            this.Invoke(new Action(() =>
                            {

                                bindingdata.CH4_List.RemoveAt(bindingdata.CH4_List.Count - 1);
                            }));
                        }

                    }
                }

                number = bindingdata.CH5_List.Count;
                if (bindingdata.CH5_List.Count > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        lock (lock_ch5_list)
                        {
                            ShowLog("分道5", "减少一瓶,瓶码：" + bindingdata.CH5_List[bindingdata.CH5_List.Count - 1].ToString());
                            AloneLog.AddInfoLog("Channel", "分道5：减少一瓶,瓶码：" + bindingdata.CH5_List[bindingdata.CH5_List.Count - 1].ToString() + "\r\n");
                            this.Invoke(new Action(() =>
                            {

                                bindingdata.CH5_List.RemoveAt(bindingdata.CH5_List.Count - 1);
                            }));
                        }

                    }
                }

                number = bindingdata.CH6_List.Count;
                if (bindingdata.CH6_List.Count > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        lock (lock_ch6_list)
                        {
                            ShowLog("分道6", "减少一瓶,瓶码：" + bindingdata.CH6_List[bindingdata.CH6_List.Count - 1].ToString());
                            AloneLog.AddInfoLog("Channel", "分道6：减少一瓶,瓶码：" + bindingdata.CH6_List[bindingdata.CH6_List.Count - 1].ToString() + "\r\n");
                            this.Invoke(new Action(() =>
                            {

                                bindingdata.CH6_List.RemoveAt(bindingdata.CH6_List.Count - 1);
                            }));
                        }

                    }
                }
            }
        }

        /// <summary>
        /// 镇江4L组箱 8种： 
        /// 1：第一分道 1 2 3；第二分道4 5 6
        /// 2：第一分道 1 2 3；第二分道4 5 
        /// 3：第一分道 1 2 3；第二分道4  
        /// 4：第一分道 1 2 3； 
        /// 5：第一分道 1 2 ；第二分道3 4
        /// 6：第一分道 1 2 ；第二分道3
        /// 7：第一分道 1 2 ；
        /// 8：第一分道 1
        /// 返回 1 表示：第一分道组箱；返回 2 表示： 第二分道组箱
        /// </summary>
        private string makeBoxNumber(int ch1_List_count, int ch2_List_count)
        {
            string result = string.Empty;
            int tempNumber1 = makeBoxAmount / 2 * m_set.XiangCount;
            if (makeBoxAmount == 6)
            {
                if (ch1_List_count == 0)
                {
                    makeBoxNumbers = 1;
                    return "2";
                }
                if (makeBoxNumbers <= 3)
                {
                    makeBoxNumbers++;
                    return "1";
                }
                else if (makeBoxNumbers > 3 && makeBoxNumbers <= 6)
                {
                    if (makeBoxNumbers == 6)
                    {
                        makeBoxNumbers = 1;
                        return "2";
                    }
                    else
                    {
                        makeBoxNumbers++;
                        return "2";
                    }
                }
                else
                {
                    return "0";
                }
            }
            else if (makeBoxAmount == 4)
            {
                if (makeBoxNumbers <= 2)
                {
                    makeBoxNumbers++;
                    return "1";
                }
                else if (makeBoxNumbers > 2 && makeBoxNumbers <= 4)
                {
                    if (makeBoxNumbers == 4)
                    {
                        makeBoxNumbers = 1;
                        return "2";
                    }
                    else
                    {
                        makeBoxNumbers++;
                        return "2";
                    }
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// 非防伪产品隐藏分道显示
        /// 根据分道加载的数据，隐藏不用的分道控件
        /// </summary>
        private void HiddenChannel()
        {
            if (bottleWithFWCode)
            {
                ch1count2.Visible = true;
                ch2count2.Visible = true;
                ch3count2.Visible = true;
                ch4count2.Visible = true;
                ch5count2.Visible = true;
                ch6count2.Visible = true;

                group_ch1.Visible = true;
                group_ch2.Visible = true;
                group_ch3.Visible = true;
                group_ch4.Visible = true;
                group_ch5.Visible = true;
                group_ch6.Visible = true;

                if (m_channel_to_boxpack[0] == 0)
                {
                    lbl_channel1.Visible = false;
                    ch1count2.Visible = false;
                    btn_Channel1_Subtrac.Visible = false;
                    btn_Channel1_Add.Visible = false;
                }
                if (m_channel_to_boxpack[1] == 0)
                {
                    lbl_channel2.Visible = false;
                    ch2count2.Visible = false;
                    btn_Channel2_Subtrac.Visible = false;
                    btn_Channel2_Add.Visible = false;
                }
                if (m_channel_to_boxpack[2] == 0)
                {
                    lbl_channel3.Visible = false;
                    ch3count2.Visible = false;
                    btn_Channel3_Subtrac.Visible = false;
                    btn_Channel3_Add.Visible = false;
                }
                if (m_channel_to_boxpack[3] == 0)
                {
                    lbl_channel4.Visible = false;
                    ch4count2.Visible = false;
                    btn_Channel4_Subtrac.Visible = false;
                    btn_Channel4_Add.Visible = false;
                }
                if (m_channel_to_boxpack[4] == 0)
                {
                    lbl_channel5.Visible = false;
                    ch5count2.Visible = false;
                    btn_Channel5_Subtrac.Visible = false;
                    btn_Channel5_Add.Visible = false;
                }
                if (m_channel_to_boxpack[5] == 0)
                {
                    lbl_channel6.Visible = false;
                    ch6count2.Visible = false;
                    btn_Channel6_Subtrac.Visible = false;
                    btn_Channel6_Add.Visible = false;
                }
            }
            else
            {
                ch1count2.Visible = false;
                ch2count2.Visible = false;
                ch3count2.Visible = false;
                ch4count2.Visible = false;
                ch5count2.Visible = false;
                ch6count2.Visible = false;

                group_ch1.Visible = false;
                group_ch2.Visible = false;
                group_ch3.Visible = false;
                group_ch4.Visible = false;
                group_ch5.Visible = false;
                group_ch6.Visible = false;
            }
        }
        /// <summary>
        /// 显示分道控件，用于“暂停”和“结束”生产。
        /// </summary>
        private void ShowChannel()
        {
            lbl_channel1.Visible = true;
            ch1count2.Visible = true;
            btn_Channel1_Subtrac.Visible = true;
            btn_Channel1_Add.Visible = true;

            lbl_channel2.Visible = true;
            ch2count2.Visible = true;
            btn_Channel2_Subtrac.Visible = true;
            btn_Channel2_Add.Visible = true;

            lbl_channel3.Visible = true;
            ch3count2.Visible = true;
            btn_Channel3_Subtrac.Visible = true;
            btn_Channel3_Add.Visible = true;

            lbl_channel4.Visible = true;
            ch4count2.Visible = true;
            btn_Channel4_Subtrac.Visible = true;
            btn_Channel4_Add.Visible = true;

            lbl_channel5.Visible = true;
            ch5count2.Visible = true;
            btn_Channel5_Subtrac.Visible = true;
            btn_Channel5_Add.Visible = true;

            lbl_channel6.Visible = true;
            ch6count2.Visible = true;
            btn_Channel6_Subtrac.Visible = true;
            btn_Channel6_Add.Visible = true;
        }

        /// <summary>
        /// 显示当前生产的托盘号
        /// </summary>
        public void BindPallet(string sku_no, string lot_no)
        {
            DataSet ds = new DataSet();
            string sql = "select pallet_no,last_op_time from qx_bundle with(nolock) where sku_no ='" + sku_no + "' and lot_no ='" + lot_no + "' group by pallet_no,last_op_time order by last_op_time";
            ds = DbHelperSQL.Query(sql);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = new DataTable();
                DataView dv = new DataView(ds.Tables[0]);
                dt = dv.ToTable(true, "pallet_no");
                view_pallet_no.DataSource = dt;
                lbl_pallet_no_count.Text = view_pallet_no.Rows.Count.ToString();
            }
        }
        /// <summary>
        /// 绑定托盘上的箱码
        /// </summary>
        public void BindPallet_Box(string pallet_no)
        {
            DataSet ds = new DataSet();
            string sql = "select box_barcode from qx_bundle with(nolock) where pallet_no ='" + pallet_no + "' group by box_barcode,last_op_time order by last_op_time";
            ds = DbHelperSQL.Query(sql);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = new DataTable();
                DataTable dtTemp = new DataTable();
                dtTemp.Columns.Add("箱码流水号", typeof(string));
                dtTemp.Columns.Add("箱码二维码", typeof(string));
                DataRow dr;
                DataView dv = new DataView(ds.Tables[0]);
                dt = dv.ToTable(true, "box_barcode");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dr = dtTemp.NewRow();
                    if (dt.Rows[i]["box_barcode"].ToString().Substring(0, 4) != "ocr-")
                    {
                        dr["箱码流水号"] = dt.Rows[i]["box_barcode"].ToString().Substring(21, 9);
                    }
                    else
                    {
                        dr["箱码流水号"] = "NOREAD";
                    }
                    dr["箱码二维码"] = dt.Rows[i]["box_barcode"].ToString();
                    dtTemp.Rows.Add(dr);
                }
                view_pallet_box.DataSource = dtTemp;
                view_pallet_box.Columns[0].Width = 200;
                view_pallet_box.Columns[1].Width = 800;
                lbl_box_count.Text = view_pallet_box.Rows.Count.ToString();
            }
        }



        public void BindcbBoxOrderNumber(int number)
        {
            cbBoxOrderNumber.Items.Clear();
            for (int i = 1; i < number + 1; i++)
            {
                cbBoxOrderNumber.Items.Add(i);
            }
            if (cbBoxOrderNumber.Items.Count < 1)
            {
                cbBoxOrderNumber.Items.Add(1);
            }
            cbBoxOrderNumber.SelectedIndex = 0;
        }

        private void view_pallet_no_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            view_pallet_no.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;//禁止托盘排序 雷子华 2022.11.1
            if (e.RowIndex > -1)
            {
                int index = view_pallet_no.CurrentRow.Index;

                if (view_pallet_no.Rows[index].Cells[0].Value.ToString().Trim().Length > 0)
                {
                    lbl_pallet_no.Text = view_pallet_no.Rows[index].Cells[0].Value.ToString().Trim();
                    lbl_pallet_no_number.Text = index.ToString();
                    BindPallet_Box(view_pallet_no.Rows[index].Cells[0].Value.ToString().Trim());
                    CheckPalletInfo();
                }
            }
        }

        //托盘信息做了操作后变色 2022.11.1  雷子华
        public void bsColor()
        {
            DataSet pallet_no_temp = new DataSet();
            DataSet box_barcode_temp = new DataSet();
            DataRow dr;
            //托盘数据
            string sql = $"select pallet_no,scColor from  qx_inventory where sku_no = '" + m_set.ProductNo + "' and lot_no = '" + m_set.Batch + "' and scColor = 1 group by pallet_no,scColor ";
            pallet_no_temp = DbHelperSQL.Query(sql);
            DataTable dt_pallet_no_temp = new DataTable();
            DataView dv_pallet_no_temp = new DataView(pallet_no_temp.Tables[0]);
            dt_pallet_no_temp = dv_pallet_no_temp.ToTable();

            //箱数据
            //string sql1 = $"select box_barcode,scColor from  qx_inventory where sku_no = '" + m_set.ProductNo + "' and lot_no = '" + m_set.Batch + "'and scColor = 1 group by box_barcode,scColor";
            //box_barcode_temp = DbHelperSQL.Query(sql1);
            //DataTable dt_box_barcode_temp = new DataTable();
            //DataView dv_box_barcode_temp = new DataView(box_barcode_temp.Tables[0]);
            //dt_box_barcode_temp = dv_box_barcode_temp.ToTable();

            for (int i = 0; i < dt_pallet_no_temp.Rows.Count; i++)
            {
                if (dt_pallet_no_temp.Rows[i]["scColor"].ToString() != null)
                {
                    foreach (DataGridViewRow item in view_pallet_no.Rows)
                    {
                        if (dt_pallet_no_temp.Rows[i]["pallet_no"].ToString() == item.Cells[0].Value.ToString())
                        {
                            item.DefaultCellStyle.BackColor = Color.Green;  //雷子华 2022.10.31 改变托背景色
                        }
                    }
                }
            }


        }



        private void btn_pallet_add_box_Click(object sender, EventArgs e)
        {
            //int number_temp = (int.Parse(cbBoxOrderNumber.Text));//新增箱码的位置
            if (Common.Common.TS("确认要新增箱码吗？"))
            {
                if (txt_pallet_box.Text.Trim().Length > 0 && lbl_pallet_no.Text.Trim().Length > 0 && m_set.ProductNo.Length > 0 && m_set.Batch.Length > 0)
                {
                    AddBox_BarCode(txt_pallet_box.Text.Trim(), lbl_pallet_no.Text.Trim(), m_set.ProductNo, m_set.Batch);
                    //view_pallet_box.Rows[number_temp].DefaultCellStyle.BackColor = Color.Green; //雷子华 2022.11.1

                    int row = view_pallet_box.Rows.Count;//得到总行数
                    int cell = view_pallet_box.Rows[2].Cells.Count;//得到总列数
                    for (int i = 0; i < row; i++)//得到总行数并在之内循环
                    {
                        for (int j = 1; j < cell; j++)//得到总列数并在之内循环
                        {
                            if (txt_pallet_box.Text == view_pallet_box.Rows[i].Cells[j].Value.ToString())
                            {   //对比TexBox中的值是否与dataGridView中的值相同（上面这句）
                                this.view_pallet_box.CurrentCell = this.view_pallet_box[j, i];//定位到相同的单元格
                                view_pallet_box.Rows[i].DefaultCellStyle.BackColor = Color.Green;
                                return;//返回
                            }
                        }
                    }
                    //bsColor();
                }
                else
                {
                    MessageBox.Show("产品信息不完整，不能新增！");
                    return;
                }
            }
        }

        private void btn_pallet_delete_box_Click(object sender, EventArgs e)
        {
            //删除正常箱码的时候提示工作人员，防止有误删除 雷子华 2022.10.20
            try
            {
                if (txt_pallet_box.Text.Trim().Length == 31 && lbl_pallet_no.Text.Trim().Length > 0 && m_set.ProductNo.Length > 0 && m_set.Batch.Length > 0)
                {
                    MessageBox.Show("此条码是正常条码" + txt_pallet_box.Text);
                    if (Common.Common.TS("此条码是正常箱码！！！ 请确认要删除的箱码" + txt_pallet_box.Text))
                    {
                        AddInfoLog("人工已确认要删除的箱码" + txt_pallet_box.Text);
                        DeleteBox_BarCode(txt_pallet_box.Text.Trim(), lbl_pallet_no.Text.Trim(), m_set.ProductNo, m_set.Batch);
                        bsColor();
                    }
                }
                else if (txt_pallet_box.Text.Trim().Length > 31 && lbl_pallet_no.Text.Trim().Length > 0 && m_set.ProductNo.Length > 0 && m_set.Batch.Length > 0)
                {
                    if (Common.Common.TS("此条码是NOREAD箱码，请确认要删除的箱码" + txt_pallet_box.Text))
                    {
                        AddInfoLog("人工已确认要删除的箱码" + txt_pallet_box.Text);
                        DeleteBox_BarCode(txt_pallet_box.Text.Trim(), lbl_pallet_no.Text.Trim(), m_set.ProductNo, m_set.Batch);
                        bsColor();
                    }
                }
                else
                {
                    MessageBox.Show("产品信息不完整，不能删除！");
                    return;
                }
            }
            catch
            {
                AddErrorLog(true, "托盘删除箱码失败", "托盘删除箱码失败" + txt_pallet_box.Text, "托盘删除箱码失败", txt_pallet_box.Text);
            }

        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            if (btn_Start.Enabled == false)
            {
                BindPallet(m_set.ProductNo, m_set.Batch);
                view_pallet_box.DataSource = null;
                txt_pallet_box.Text = "";
                lbl_pallet_no.Text = "";
                lbl_pallet_no_number.Text = "";
                lbl_pallet_info.Text = "提示信息";
                BindcbBoxOrderNumber(m_set.pallet_pack_maxqty);
            }
            bsColor();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Name == "tabPage3")
            {
                if (btn_Start.Enabled == false)
                {
                    BindPallet(m_set.ProductNo, m_set.Batch);
                    BindcbBoxOrderNumber(m_set.pallet_pack_maxqty);
                    if (lbl_pallet_no_number.Text.Trim().Length > 0)
                    {
                        view_pallet_no.Rows[int.Parse(lbl_pallet_no_number.Text)].Selected = true;
                        BindPallet_Box(lbl_pallet_no.Text.Trim());
                        bsColor();
                    }
                    else
                    {
                        view_pallet_box.DataSource = null;
                        txt_pallet_box.Text = "";
                        lbl_pallet_no.Text = "";
                        lbl_pallet_no_number.Text = "";
                        lbl_pallet_info.Text = "提示信息";
                        bsColor();
                    }
                }
            }
        }

        private void view_pallet_no_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(view_pallet_no.RowHeadersDefaultCellStyle.ForeColor))
            {
                int number = int.Parse(e.RowIndex.ToString(System.Globalization.CultureInfo.CurrentUICulture)) + 1;
                e.Graphics.DrawString(number.ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 20, e.RowBounds.Location.Y + 4);
            }
        }

        private void view_pallet_box_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(view_pallet_box.RowHeadersDefaultCellStyle.ForeColor))
            {
                int number = int.Parse(e.RowIndex.ToString(System.Globalization.CultureInfo.CurrentUICulture)) + 1;
                e.Graphics.DrawString(number.ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 20, e.RowBounds.Location.Y + 4);
            }
        }

        private void view_pallet_box_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                int index = view_pallet_box.CurrentRow.Index;
                if (view_pallet_box.Rows[index].Cells[1].Value.ToString().Trim().Length > 0)
                {
                    txt_pallet_box.Text = view_pallet_box.Rows[index].Cells[1].Value.ToString().Trim();
                    cbBoxOrderNumber.SelectedIndex = index;
                }
            }
        }
        //根据需要新增一个按钮强制组托 2022.11.2 雷子华
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (Common.Common.TS("确定要强制组托吗？如果确认要强制组托请点击确认，误操作请点击取消"))
                {
                    AddInfoLog("人工已确认强制组托");

                    lbl_showProgress.Text = "正在零头组拖,请稍后...";
                    this.Update();
                    ForceMakePallet();//3、最后零头组拖
                    forceMakePallet_ResetEvent = new AutoResetEvent(false);//
                    forceMakePallet_ResetEvent.WaitOne();
                    AddInfoLog("强制组托", "强制组托, box_qty=" + OCR_Box_List.Count.ToString() + "\r\n");
                    AddInfoLog("强制组托成功", "强制组托成功");
                }
            }
            catch
            {
                AddErrorLog(true, "强制组托失败", "强制组托失败", "强制组托失败", "强制组托失败");
            }
        }

        //OCR3增加箱子  2023.1.3 雷子华
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                FormInputBoxAddOCR3 fm = new FormInputBoxAddOCR3();
                FormInputBoxAddOCR3.m_result = "";
                DialogResult dr = fm.ShowDialog(this);
                string strNumLast = "A";
                string tempBoxCode = string.Empty;
                if (dr == DialogResult.Yes)
                {
                    if (m_set.ProduceLine == "4L")
                    {
                        //只有镇江工厂的4L产品才是A,其余工厂都是D
                        strNumLast = "A";
                    }
                    if (m_set.ProduceLine == "1L")
                    {
                        strNumLast = "D";
                    }
                    if (m_set.ProduceLine == "大标签")
                    {
                        strNumLast = "B";
                    }
                    if (Common.Config.ReadValue("Line", "type").Trim().Length == 0)
                    {
                        tempBoxCode = "A" + m_set.ProductNo + m_set.Batch + m_set.ProduceDate.Substring(2, 2) + m_set.ProduceDate.Substring(5, 2) + m_set.ProduceDate.Substring(8, 2) + FormInputBoxAddOCR3.m_result.Trim() + strNumLast;//镇江开头为A；天津为B；广州为C；
                    }
                    else
                    {
                        tempBoxCode = Common.Config.ReadValue("Line", "type") + m_set.ProductNo + m_set.Batch + m_set.ProduceDate.Substring(2, 2) + m_set.ProduceDate.Substring(5, 2) + m_set.ProduceDate.Substring(8, 2) + FormInputBoxAddOCR3.m_result.Trim() + strNumLast;//镇江开头为A；天津为B；广州为C；
                    }
                    bindingdata.OCR3List.Add(GetOCR2ViewDataClass(tempBoxCode));

                    //DbHelperSQL.ExecuteSql("insert into qx_OCR3_CacheData(doc_no,barcode,op_time) values('" + m_set.OrderNo + "','" + tempBoxCode + "',getdate())");
                    AddInfoLog("增加箱子（软件）OCR3", "箱码：" + tempBoxCode);
                    AddErrorLog4(true, "增加箱子（软件）OCR3", "箱码：" + tempBoxCode, "增加箱子（软件）OCR3", "箱码：" + tempBoxCode);
                }
                else if (dr == DialogResult.No)
                {
                    if (FormInputBoxAddOCR3.m_result.Trim().Length != 12)
                    {
                        MessageBox.Show("箱码不是12位的，不能新增！本次输入的箱码是：" + FormInputBoxAddOCR3.m_result.Trim());
                        return;
                    }
                }
            }
            catch
            {
                MessageBox.Show("OCR3 新增箱码失败！");
            }
        }



        public void AddBox_BarCode(string box_barcode, string pallet_no, string sku_no, string lot_no)
        {
            string ocr3Temp = string.Empty;
            if (box_barcode.Trim().Length != 31)
            {
                MessageBox.Show("箱码不足31位，不能新增！");
                return;
            }
            bool falg = true;
            string sql = "select count(box_barcode) from qx_bundle with(nolock) where box_barcode ='" + box_barcode + "'";
            falg = DbHelperSQL.Exists(sql);
            if (falg)//判断箱码是否存在
            {
                MessageBox.Show("箱码已存在，不能新增！");

                return;
            }
            else
            {
                sql = "select pallet_no,last_op_time from qx_bundle with(nolock)"
                + " where sku_no = '" + sku_no + "' and lot_no = '" + lot_no + "' and last_op_time >= "
                + "(select top 1 last_op_time from qx_bundle with(nolock) where pallet_no = '" + pallet_no + "' order by last_op_time)"
                + " and pallet_no <> '" + pallet_no + "' group by pallet_no,last_op_time order by last_op_time";
                DataSet ds_pallet_no = new DataSet();
                DataSet ds_box_barcode = new DataSet();
                DataSet ds_temp = new DataSet();
                List<string> li_data = new List<string>();
                List<string> li_data_load = new List<string>();
                string sql_load = string.Empty;
                string item_barcode_temp = string.Empty;
                int number_temp = (int.Parse(cbBoxOrderNumber.Text) - 1) * m_set.XiangCount;//新增箱码的位置
                //view_pallet_box.Rows[number_temp].DefaultCellStyle.BackColor = Color.Green; //雷子华 2022.11.1
                //view_pallet_box.Rows[number_temp].DefaultCellStyle.Font = new Font("宋体", 18); //雷子华 2022.11.1

                ds_temp = DbHelperSQL.Query(sql);//托盘数据
                DataView dv = new DataView(ds_temp.Tables[0]);
                ds_pallet_no.Tables.Add(dv.ToTable(true, "pallet_no")); //托盘数据,不会有当前“选中”托盘的数据，如 当前有2个托盘，选中第一个托盘新增，table里只有1条数据，不是2条。
                sql = "select * from qx_bundle with(nolock) where pallet_no = '" + pallet_no + "' order by last_op_time";
                ds_temp = DbHelperSQL.Query(sql);//获取当前托盘的数据，为新增做准备

                sql = string.Format("INSERT INTO [dbo].[qx_inventory] ([pallet_no],[box_barcode],[site_no],[site_desc],[location_no],[last_op_time],[last_op_user],[last_op_desc],[last_op_pda_no],[create_time],[create_user],[create_desc],[create_pda_no],[pallet_pack_qty],[box_pack_qty],[pallet_pack_id],[box_status],[status],[lot_no],[mfd_date],[sku_no]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}',{17},'{18}','{19}','{20}')", pallet_no, box_barcode, ds_temp.Tables[0].Rows[0]["site_no"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["site_desc"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["doc_no"].ToString().Trim(), ds_temp.Tables[0].Rows[number_temp]["last_op_time"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["last_op_user"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["last_op_desc"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["last_op_pda_no"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["create_time"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["create_user"].ToString().Trim(), "生产入库", "", ds_temp.Tables[0].Rows[0]["pallet_pack_qty"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["box_pack_qty"].ToString().Trim(), ds_temp.Tables[0].Rows[0]["pallet_pack_id"].ToString().Trim(), "库存", 6, lot_no, ds_temp.Tables[0].Rows[0]["mfd_date"].ToString().Trim(), sku_no);
                li_data.Add(sql);//新增箱码
                AddInfoLog("sql2=" + sql + "\r\n");
                sql = string.Format("update qx_inventory set scColor = 1 where sku_no = '" + sku_no + "' and lot_no = '" + lot_no + "' and box_barcode = '" + box_barcode + "' and pallet_no = '" + pallet_no + "'");
                li_data.Add(sql);//修改判断变色的值 雷子华 2022.11.1
                AddInfoLog("sql2=" + sql + "\r\n");

                for (int i = 0; i < m_set.XiangCount; i++)
                {
                    #region 完整的托、箱、瓶关联
                    item_barcode_temp = "ocr-" + Guid.NewGuid();
                    sql_load = "insert into qx_bundle([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                     + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                     + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                     + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                     + ",[site_no],[site_desc],[doc_no])"
                     + " values('" + pallet_no + "','" + box_barcode + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                     + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                     + ",getdate(),getdate(),'" + ds_temp.Tables[0].Rows[number_temp]["last_op_time"].ToString().Trim() + "'"
                     + ",'" + m_set.UserName + "','生产任务单',''"
                     + ",'" + box_List.Count + "','0'"
                     + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                     + ",'" + m_set.OrderNo + "')";

                    li_data_load.Add(sql_load);
                    AddInfoLog("sql2=" + sql_load + "\r\n");
                    #endregion

                    #region 箱托关联
                    sql_load = "insert into qx_bundle_pb([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                 + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                 + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                 + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                 + ",[site_no],[site_desc],[doc_no])"
                 + " values('" + pallet_no + "','" + box_barcode + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                 + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                 + ",getdate(),getdate(),'" + ds_temp.Tables[0].Rows[number_temp]["last_op_time"].ToString().Trim() + "'"
                 + ",'" + m_set.UserName + "','生产任务单',''"
                 + ",'" + box_List.Count + "','0'"
                 + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                 + ",'" + m_set.OrderNo + "')";

                    li_data_load.Add(sql_load);
                    AddInfoLog("sql2=" + sql_load + "\r\n");
                    #endregion
                }

                if (ds_pallet_no.Tables[0].Rows.Count == 0)
                {
                    ocr3Temp = ds_temp.Tables[0].Rows[ds_temp.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim();
                    sql = string.Format("DELETE FROM [dbo].[qx_inventory] where box_barcode = '{0}'", ocr3Temp);
                    li_data.Add(sql);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号的第一条。  qx_inventory 库存表
                    AddInfoLog("sql2=" + sql + "\r\n");

                    //修改本地
                    sql_load = string.Format("DELETE FROM [dbo].[qx_bundle] where box_barcode = '{0}'", ocr3Temp);
                    li_data_load.Add(sql_load);//更新数据，更新托盘后的每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。 qx_bundle 生产条码关联表
                    AddInfoLog("sql2=" + sql_load + "\r\n");
                }

                for (int i = 0; i < ds_pallet_no.Tables[0].Rows.Count; i++)
                {
                    sql = "select * from qx_bundle with(nolock) where pallet_no = '" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "' order by last_op_time";
                    ds_box_barcode = DbHelperSQL.Query(sql);//一托上的箱码数据  qx_bundle生产条码关联表

                    if (ds_box_barcode != null && ds_box_barcode.Tables.Count > 0)
                    {
                        if (i == 0)
                        {
                            //当前托盘修改数据，托盘变量ds_pallet_no不会有“选择”的托盘。如：选中
                            sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "' WHERE box_barcode='" + ds_temp.Tables[0].Rows[ds_temp.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                            li_data.Add(sql);//更新数据，更新托盘后每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。 qx_inventory 库存表
                            AddInfoLog("sql2=" + sql + "\r\n");

                            sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "' WHERE box_barcode='" + ds_temp.Tables[0].Rows[ds_temp.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                            li_data_load.Add(sql_load);//更新数据，更新托盘后每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。 qx_bundle 生产条码关联表
                            AddInfoLog("sql2=" + sql_load + "\r\n");
                            if (i != (ds_pallet_no.Tables[0].Rows.Count - 1))
                            {
                                //sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i + 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_temp.Tables[0].Rows[ds_temp.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                                //li_data.Add(sql);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                //AddInfoLog("sql2=" + sql + "\r\n");
                                sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i + 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[ds_box_barcode.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                                li_data.Add(sql);//更新数据，更新托盘后每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。 qx_inventory 库存表
                                AddInfoLog("sql2=" + sql + "\r\n");

                                //修改本地
                                sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i + 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[ds_box_barcode.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                                li_data_load.Add(sql_load);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql_load + "\r\n");
                            }
                            else
                            {
                                ocr3Temp = ds_box_barcode.Tables[0].Rows[ds_box_barcode.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim();
                                sql = string.Format("DELETE FROM [dbo].[qx_inventory] where box_barcode = '{0}'", ocr3Temp);
                                li_data.Add(sql);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql + "\r\n");

                                //修改本地
                                sql_load = string.Format("DELETE FROM [dbo].[qx_bundle] where box_barcode = '{0}'", ocr3Temp);
                                li_data_load.Add(sql_load);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql_load + "\r\n");
                            }
                        }
                        else
                        {
                            if (i == (ds_pallet_no.Tables[0].Rows.Count - 1))//最后一托的最后一箱，需要重新放到ocr3队列里，并放在第一行。
                            {
                                ocr3Temp = ds_box_barcode.Tables[0].Rows[ds_box_barcode.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim();
                                sql = string.Format("DELETE FROM [dbo].[qx_inventory] where box_barcode = '{0}'", ocr3Temp);
                                li_data.Add(sql);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql + "\r\n");

                                //修改本地
                                sql_load = string.Format("DELETE FROM [dbo].[qx_bundle] where box_barcode = '{0}'", ocr3Temp);
                                li_data_load.Add(sql_load);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql_load + "\r\n");
                            }
                            else
                            {
                                sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i + 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[ds_box_barcode.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                                li_data.Add(sql);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql + "\r\n");

                                //修改本地
                                sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i + 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[ds_box_barcode.Tables[0].Rows.Count - 1]["box_barcode"].ToString().Trim() + "'";
                                li_data_load.Add(sql_load);//更新数据，每一托最后一条数据下移即最后一条数据托盘号更新为下一托盘号。
                                AddInfoLog("sql2=" + sql_load + "\r\n");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("箱码数据异常，不能新增！");
                        AddErrorLog(true, "箱码数据异常，不能新增！", "箱码数据异常，不能新增！", "箱码数据异常，不能新增！", "箱码数据异常，不能新增！");
                        return;
                    }
                }

                try
                {
                    ServiceReference2.WebService1SoapClient ws = new ServiceReference2.WebService1SoapClient("WebService1Soap1", Common.Config.ReadValue("LinkServer", "ws") + "/webservice1.asmx");
                    int temp = 0;
                    sql = string.Join(";", li_data.ToArray());
                    string encsql = Common.MD5ALGO.Encrypt(sql);
                    temp = ws.ExecuteSqlTran(encsql);//更新服务上qx_bundle和qx_inventory的数据
                    if (temp < 1)
                    {
                        MessageBox.Show("更新服务器数据异常！");
                        AddErrorLog(true, "更新服务器数据异常", "更新服务器数据异常", "更新服务器数据异常", "更新服务器数据异常");
                        return;
                    }
                    else
                    {
                        AddErrorLog2("托盘操作", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！");
                        AddErrorLog4(true, "托盘操作", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！", txt_pallet_box.Text.Trim(), true, m_set.ProductNo, m_set.Batch);
                        ShowLog2("托盘操作", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！");
                        temp = DbHelperSQL.ExecuteSqlTran(li_data_load);//更新本地数据库
                        if (temp < 1)
                        {
                            MessageBox.Show("更新本地数据库异常！");
                            AddErrorLog(true, "更新本地数据库异常", "更新本地数据库异常", "更新本地数据库异常", "更新本地数据库异常");
                            return;
                        }
                        else
                        {
                            AddErrorLog2("托盘操作", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！");
                            AddErrorLog4(true, "托盘操作", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！", txt_pallet_box.Text.Trim(), true, m_set.ProductNo, m_set.Batch);
                            ShowLog2("托盘操作", "新增箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！");

                            try
                            {
                                if (ocr3Temp.Trim().Length > 0)
                                {
                                    bindingdata.OCR3List.Insert(0, GetOCR2ViewDataClass(ocr3Temp));

                                    string intime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    object[] objvalues = new object[2];
                                    objvalues[0] = ocr3Temp.Substring(21, 9);
                                    if (ocr3Temp.Trim().Substring(0, 4).ToLower() == "ocr-")//万能码
                                    {
                                        objvalues[0] = "NOREAD";
                                        OCR3NoReadAmount++;
                                    }
                                    else
                                    {
                                        OCR3ReadAmount++;
                                    }

                                    objvalues[1] = ocr3Temp.Trim();

                                    this.view_box2.Rows.Insert(0, objvalues);
                                    view_box2.Rows[view_box2.Rows.Count - 1].Selected = true;               //设置为选中. 
                                    view_box2.FirstDisplayedScrollingRowIndex = view_box2.Rows.Count - 1;   //设置第一行显示
                                    view_box2.Focus();//保证滑动条永远处于最新一条数据位置
                                    Application.DoEvents();
                                    {
                                        int ocr3 = bindingdata.OCR3List.Count;
                                        ocr3Count.Text = "" + ocr3;
                                        #region 显示ocr2读码率
                                        lblOCR3ReadRate.Text = Common.Common.GetReadRate(OCR3ReadAmount, OCR3NoReadAmount);
                                        #endregion
                                        lb_xiang1.Text = ocr3Count.Text;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                AddErrorLog(true, "新增箱码OCR3异常", ex.Message, "OCR3 Add Exception", ex.Message + " " + ex.StackTrace);
                            }
                            BindPallet(m_set.ProductNo, m_set.Batch);
                            BindcbBoxOrderNumber(m_set.pallet_pack_maxqty);
                            view_pallet_box.DataSource = null;
                            if (lbl_pallet_no_number.Text.Trim().Length > 0)
                            {
                                view_pallet_no.Rows[int.Parse(lbl_pallet_no_number.Text)].Selected = true;
                                BindPallet_Box(lbl_pallet_no.Text.Trim());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("更新本地数据库及服务器数据异常！");
                    AddErrorLog(true, "更新本地数据库及服务器数据异常", ex.Message, "托盘信息 Exception", ex.Message + " " + ex.StackTrace);
                    return;
                }
            }
        }

        public void DeleteBox_BarCode(string box_barcode, string pallet_no, string sku_no, string lot_no)
        {
            if (box_barcode.Trim().Length != 31)
            {
                if (box_barcode.Trim().Substring(0, 4) != "ocr-")
                {
                    MessageBox.Show("箱码不足31位，不能删除！");
                    return;
                }
            }
            bool falg = true;
            string sql = "select count(box_barcode) from qx_bundle with(nolock) where box_barcode ='" + box_barcode + "'";
            falg = DbHelperSQL.Exists(sql);
            if (!falg)//判断箱码是否存在
            {
                MessageBox.Show("箱码不存在，不能删除！");
                return;
            }
            else
            {
                sql = "select pallet_no,last_op_time from qx_bundle with(nolock)"
                + " where sku_no = '" + sku_no + "' and lot_no = '" + lot_no + "' and last_op_time > "
                + "(select top 1 last_op_time from qx_bundle with(nolock) where pallet_no = '" + pallet_no + "' order by last_op_time)"
                + " and pallet_no <> '" + pallet_no + "' group by pallet_no,last_op_time order by last_op_time";
                DataSet ds_pallet_no = new DataSet();
                DataSet ds_box_barcode = new DataSet();
                List<string> li_data = new List<string>();
                List<string> li_data_load = new List<string>();
                string sql_load = string.Empty;
                string item_barcode_temp = string.Empty;
                string box_barcode_temp = string.Empty;

                DataSet ds_temp = DbHelperSQL.Query(sql);//托盘数据
                DataView dv = new DataView(ds_temp.Tables[0]);
                ds_pallet_no.Tables.Add(dv.ToTable(true, "pallet_no"));
                //ds_pallet_no = DbHelperSQL.Query(sql);//托盘数据
                if (ds_pallet_no != null && ds_pallet_no.Tables.Count > 0)
                {
                    sql = string.Format("DELETE FROM [dbo].[qx_inventory] where box_barcode = '{0}'", box_barcode);
                    li_data.Add(sql);//新增箱码
                    AddInfoLog("sql2=" + sql + "\r\n");

                    sql_load = string.Format("DELETE FROM [dbo].[qx_bundle] where box_barcode = '{0}'", box_barcode);
                    li_data_load.Add(sql_load);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                    AddInfoLog("sql2=" + sql_load + "\r\n");

                    sql_load = string.Format("DELETE FROM [dbo].[qx_bundle_pb] where box_barcode = '{0}'", box_barcode);
                    li_data_load.Add(sql_load);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                    AddInfoLog("sql2=" + sql_load + "\r\n");

                    if (ds_pallet_no.Tables[0].Rows.Count == 0)
                    {
                        if (bindingdata.OCR3List.Count > 0)
                        {
                            box_barcode_temp = bindingdata.OCR3List[0].Allcode;
                        }
                        else
                        {
                            box_barcode_temp = "ocr-" + Guid.NewGuid().ToString();
                        }

                        sql = string.Format("INSERT INTO [dbo].[qx_inventory] ([pallet_no],[box_barcode],[site_no],[site_desc],[location_no],[last_op_time],[last_op_user],[last_op_desc],[last_op_pda_no],[create_time],[create_user],[create_desc],[create_pda_no],[pallet_pack_qty],[box_pack_qty],[pallet_pack_id],[box_status],[status],[lot_no],[mfd_date],[sku_no]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}',{17},'{18}','{19}','{20}')", pallet_no, box_barcode_temp, m_set.site_no, m_set.site_desc, m_set.OrderNo, DateTime.Now.ToString(), m_set.UserName, "生产任务单", "", DateTime.Now.ToString(), "", "生产入库", "", box_List.Count, m_set.box_pack_maxqty, "0", "库存", 6, lot_no, DateTime.Now.ToString("yyyy-MM-dd"), sku_no);
                        li_data.Add(sql);//新增箱码
                        AddInfoLog("sql2=" + sql + "\r\n");

                        for (int j = 0; j < m_set.XiangCount; j++)
                        {
                            #region 完整的托、箱、瓶关联
                            item_barcode_temp = "ocr-" + Guid.NewGuid();
                            sql_load = "insert into qx_bundle([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                             + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                             + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                             + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                             + ",[site_no],[site_desc],[doc_no])"
                             + " values('" + pallet_no + "','" + box_barcode_temp + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                             + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                             + ",getdate(),getdate(),getdate()"
                             + ",'" + m_set.UserName + "','生产任务单',''"
                             + ",'" + box_List.Count + "','0'"
                             + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                             + ",'" + m_set.OrderNo + "')";

                            li_data_load.Add(sql_load);
                            AddInfoLog("sql2=" + sql_load + "\r\n");
                            #endregion

                            #region 箱托关联
                            sql_load = "insert into qx_bundle_pb([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                         + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                         + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                         + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                         + ",[site_no],[site_desc],[doc_no])"
                         + " values('" + pallet_no + "','" + box_barcode_temp + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                         + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                         + ",getdate(),getdate(),getdate()"
                         + ",'" + m_set.UserName + "','生产任务单',''"
                         + ",'" + box_List.Count + "','0'"
                         + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                         + ",'" + m_set.OrderNo + "')";

                            li_data_load.Add(sql_load);
                            AddInfoLog("sql2=" + sql_load + "\r\n");
                            #endregion
                        }
                    }

                    for (int i = 0; i < ds_pallet_no.Tables[0].Rows.Count; i++)
                    {
                        sql = "select * from qx_bundle with(nolock) where pallet_no = '" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "' order by last_op_time";
                        ds_box_barcode = DbHelperSQL.Query(sql);//一托上的箱码数据
                        if (ds_box_barcode != null && ds_box_barcode.Tables.Count > 0)
                        {
                            if (i == 0)
                            {
                                if (i == (ds_pallet_no.Tables[0].Rows.Count - 1))//
                                {
                                    sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + pallet_no + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data.Add(sql);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql + "\r\n");

                                    //修改本地
                                    sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + pallet_no + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data_load.Add(sql_load);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql_load + "\r\n");

                                    if (bindingdata.OCR3List.Count > 0)
                                    {
                                        box_barcode_temp = bindingdata.OCR3List[0].Allcode;
                                    }
                                    else
                                    {
                                        box_barcode_temp = "ocr-" + Guid.NewGuid().ToString();
                                    }

                                    sql = string.Format("INSERT INTO [dbo].[qx_inventory] ([pallet_no],[box_barcode],[site_no],[site_desc],[location_no],[last_op_time],[last_op_user],[last_op_desc],[last_op_pda_no],[create_time],[create_user],[create_desc],[create_pda_no],[pallet_pack_qty],[box_pack_qty],[pallet_pack_id],[box_status],[status],[lot_no],[mfd_date],[sku_no]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}',{17},'{18}','{19}','{20}')", ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim(), box_barcode, ds_box_barcode.Tables[0].Rows[0]["site_no"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["site_desc"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["doc_no"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_time"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_user"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_desc"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_pda_no"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["create_time"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["create_user"].ToString().Trim(), "生产入库", "", ds_box_barcode.Tables[0].Rows[0]["pallet_pack_qty"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["box_pack_qty"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["pallet_pack_id"].ToString().Trim(), "库存", 6, lot_no, ds_box_barcode.Tables[0].Rows[0]["mfd_date"].ToString().Trim(), sku_no);
                                    li_data.Add(sql);//新增箱码
                                    AddInfoLog("sql2=" + sql + "\r\n");

                                    for (int j = 0; j < m_set.XiangCount; j++)
                                    {
                                        #region 完整的托、箱、瓶关联
                                        item_barcode_temp = "ocr-" + Guid.NewGuid();
                                        sql_load = "insert into qx_bundle([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                                         + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                                         + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                                         + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                                         + ",[site_no],[site_desc],[doc_no])"
                                         + " values('" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "','" + box_barcode_temp + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                                         + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                                         + ",getdate(),getdate(),getdate()"
                                         + ",'" + m_set.UserName + "','生产任务单',''"
                                         + ",'" + box_List.Count + "','0'"
                                         + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                                         + ",'" + m_set.OrderNo + "')";

                                        li_data_load.Add(sql_load);
                                        AddInfoLog("sql2=" + sql_load + "\r\n");
                                        #endregion

                                        #region 箱托关联
                                        sql_load = "insert into qx_bundle_pb([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                                     + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                                     + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                                     + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                                     + ",[site_no],[site_desc],[doc_no])"
                                     + " values('" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "','" + box_barcode_temp + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                                     + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                                     + ",getdate(),getdate(),getdate()"
                                     + ",'" + m_set.UserName + "','生产任务单',''"
                                     + ",'" + box_List.Count + "','0'"
                                     + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                                     + ",'" + m_set.OrderNo + "')";

                                        li_data_load.Add(sql_load);
                                        AddInfoLog("sql2=" + sql_load + "\r\n");
                                        #endregion
                                    }
                                }
                                else
                                {
                                    sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + pallet_no + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data.Add(sql);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql + "\r\n");

                                    //修改本地
                                    sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + pallet_no + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data_load.Add(sql_load);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql_load + "\r\n");
                                }
                            }
                            else
                            {
                                if (i == (ds_pallet_no.Tables[0].Rows.Count - 1))//
                                {
                                    sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i - 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data.Add(sql);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql + "\r\n");

                                    //修改本地
                                    sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i - 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data_load.Add(sql_load);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql_load + "\r\n");
                                    if (bindingdata.OCR3List.Count > 0)
                                    {
                                        box_barcode_temp = bindingdata.OCR3List[0].Allcode;
                                    }
                                    else
                                    {
                                        box_barcode_temp = "ocr-" + Guid.NewGuid().ToString();
                                    }

                                    sql = string.Format("INSERT INTO [dbo].[qx_inventory] ([pallet_no],[box_barcode],[site_no],[site_desc],[location_no],[last_op_time],[last_op_user],[last_op_desc],[last_op_pda_no],[create_time],[create_user],[create_desc],[create_pda_no],[pallet_pack_qty],[box_pack_qty],[pallet_pack_id],[box_status],[status],[lot_no],[mfd_date],[sku_no]) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}',{17},'{18}','{19}','{20}')", pallet_no, box_barcode, ds_box_barcode.Tables[0].Rows[0]["site_no"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["site_desc"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["doc_no"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_time"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_user"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_desc"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["last_op_pda_no"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["create_time"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["create_user"].ToString().Trim(), "生产入库", "", ds_box_barcode.Tables[0].Rows[0]["pallet_pack_qty"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["box_pack_qty"].ToString().Trim(), ds_box_barcode.Tables[0].Rows[0]["pallet_pack_id"].ToString().Trim(), "库存", 6, lot_no, ds_box_barcode.Tables[0].Rows[0]["mfd_date"].ToString().Trim(), sku_no);
                                    li_data.Add(sql);//新增箱码
                                    AddInfoLog("sql2=" + sql + "\r\n");

                                    for (int j = 0; j < m_set.XiangCount; j++)
                                    {
                                        #region 完整的托、箱、瓶关联
                                        item_barcode_temp = "ocr-" + Guid.NewGuid();
                                        sql_load = "insert into qx_bundle([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                                         + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                                         + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                                         + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                                         + ",[site_no],[site_desc],[doc_no])"
                                         + " values('" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "','" + box_barcode_temp + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                                         + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                                         + ",getdate(),getdate(),getdate()"
                                         + ",'" + m_set.UserName + "','生产任务单',''"
                                         + ",'" + box_List.Count + "','0'"
                                         + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                                         + ",'" + m_set.OrderNo + "')";

                                        li_data_load.Add(sql_load);
                                        AddInfoLog("sql2=" + sql_load + "\r\n");
                                        #endregion

                                        #region 箱托关联
                                        sql_load = "insert into qx_bundle_pb([pallet_no],[box_barcode],[item_barcode],[sku_no],[lot_no],[mfd_date]"
                                     + ",[pline_no],[pline_desc],[pb_date],[bi_date],[last_op_time]"
                                     + ",[last_op_user],[last_op_desc],[last_op_pda_no]"
                                     + ",[pallet_pack_qty],[pallet_pack_id],[box_pack_qty],[box_pack_id]"
                                     + ",[site_no],[site_desc],[doc_no])"
                                     + " values('" + ds_pallet_no.Tables[0].Rows[i][0].ToString().Trim() + "','" + box_barcode_temp + "','" + item_barcode_temp + "','" + m_set.ProductNo + "','" + m_set.Batch + "'"
                                     + ",'" + m_set.ProduceDate + "','" + m_set.ProduceLine + "',''"
                                     + ",getdate(),getdate(),getdate()"
                                     + ",'" + m_set.UserName + "','生产任务单',''"
                                     + ",'" + box_List.Count + "','0'"
                                     + ",'" + m_set.box_pack_maxqty + "','1','" + m_set.site_no + "','" + m_set.site_desc + "'"
                                     + ",'" + m_set.OrderNo + "')";

                                        li_data_load.Add(sql_load);
                                        AddInfoLog("sql2=" + sql_load + "\r\n");
                                        #endregion
                                    }
                                }
                                else
                                {
                                    sql = "UPDATE [dbo].[qx_inventory] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i - 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data.Add(sql);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql + "\r\n");

                                    //修改本地
                                    sql_load = "UPDATE [dbo].[qx_bundle] SET [pallet_no] ='" + ds_pallet_no.Tables[0].Rows[i - 1][0].ToString().Trim() + "' WHERE box_barcode='" + ds_box_barcode.Tables[0].Rows[0]["box_barcode"].ToString().Trim() + "'";
                                    li_data_load.Add(sql_load);//更新数据，每一托第一条数据上移即第一条数据托盘号更新为上一托盘号。
                                    AddInfoLog("sql2=" + sql_load + "\r\n");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("箱码数据异常，不能删除！");
                            AddErrorLog(true, "箱码数据异常，不能删除！", "箱码数据异常，不能删除！", "箱码数据异常，不能删除！", "箱码数据异常，不能删除！");
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("托盘数据异常，不能删除！");
                    AddErrorLog(true, "托盘数据异常，不能删除！", "托盘数据异常，不能删除！", "托盘数据异常，不能删除！", "托盘数据异常，不能删除！");
                    return;
                }

                try
                {
                    ServiceReference2.WebService1SoapClient ws = new ServiceReference2.WebService1SoapClient("WebService1Soap1", Common.Config.ReadValue("LinkServer", "ws") + "/webservice1.asmx");
                    int temp = 0;
                    sql = string.Join(";", li_data.ToArray());
                    string encsql = Common.MD5ALGO.Encrypt(sql);
                    temp = ws.ExecuteSqlTran(encsql);//更新服务上qx_bundle和qx_inventory的数据
                    if (temp < 1)
                    {
                        MessageBox.Show("更新服务器数据异常！");
                        AddErrorLog(true, "更新服务器数据异常！", "更新服务器数据异常！", "更新服务器数据异常！", "更新服务器数据异常！");
                        return;
                    }
                    else
                    {
                        AddErrorLog2("托盘操作", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！");
                        AddErrorLog4(true, "托盘操作", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！", txt_pallet_box.Text.Trim(), true, m_set.ProductNo, m_set.Batch);
                        ShowLog2("托盘操作", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新服务器数据成功！");
                        temp = DbHelperSQL.ExecuteSqlTran(li_data_load);//更新本地数据库
                        if (temp < 1)
                        {
                            MessageBox.Show("更新本地数据库异常！");
                            AddErrorLog(true, "更新本地数据库异常！", "更新本地数据库异常！", "更新本地数据库异常！", "更新本地数据库异常！");
                            return;
                        }
                        else
                        {
                            AddErrorLog2("托盘操作", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！");
                            AddErrorLog4(true, "托盘操作", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！", txt_pallet_box.Text.Trim(), true, m_set.ProductNo, m_set.Batch);
                            ShowLog2("托盘操作", "删除箱码" + txt_pallet_box.Text.Trim() + "成功，并更新本地数据成功！");
                            this.Invoke(new Action(() =>
                            {
                                lock (_lockObj)
                                {
                                    bindingdata.OCR3List.RemoveAt(0);
                                }
                            }));
                           
                            BindPallet(m_set.ProductNo, m_set.Batch);
                            BindcbBoxOrderNumber(m_set.pallet_pack_maxqty);
                            view_pallet_box.DataSource = null;
                            if (lbl_pallet_no_number.Text.Trim().Length > 0)
                            {
                                view_pallet_no.Rows[int.Parse(lbl_pallet_no_number.Text)].Selected = true;
                                BindPallet_Box(lbl_pallet_no.Text.Trim());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("更新本地数据库及服务器数据异常！");
                    AddErrorLog(true, "更新本地数据库及服务器数据异常", ex.Message, "托盘信息 Exception", ex.Message + " " + ex.StackTrace);
                    return;
                }
            }
        }

        public void CheckPalletInfo()
        {
            if (view_pallet_box.Rows.Count > 0)
            {
                int number = 0;
                int startNumber = 0;
                int endNumber = 0;
                for (int i = 0; i < view_pallet_box.Rows.Count; i++)
                {
                    if (view_pallet_box.Rows[i].Cells[0].Value.ToString().Contains("NOREAD"))
                    {
                        number++;
                        if (i != 0)
                        {
                            if (!view_pallet_box.Rows[i - 1].Cells[0].Value.ToString().Contains("NOREAD"))
                            {
                                startNumber = int.Parse(view_pallet_box.Rows[i - 1].Cells[0].Value.ToString());
                                for (int j = i + 1; j < view_pallet_box.Rows.Count; j++)
                                {
                                    if (!view_pallet_box.Rows[j].Cells[0].Value.ToString().Contains("NOREAD"))
                                    {
                                        endNumber = int.Parse(view_pallet_box.Rows[j].Cells[0].Value.ToString());
                                        if (startNumber == (endNumber - j + i))
                                        {
                                            lbl_pallet_info.Text = "托盘内NOREAD箱码，疑似为误触发！";
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (number > 0)
                {
                    lbl_pallet_info.Text = "托盘内有" + number + "个NOREAD箱码！";
                }
                else
                {
                    startNumber = int.Parse(view_pallet_box.Rows[0].Cells[0].Value.ToString());
                    endNumber = startNumber + view_pallet_box.Rows.Count - 1;
                    if (endNumber != int.Parse(view_pallet_box.Rows[view_pallet_box.Rows.Count - 1].Cells[0].Value.ToString()))
                    {
                        lbl_pallet_info.Text = "托盘内箱码号不连续！疑似漏箱！";
                    }
                    else
                    {
                        lbl_pallet_info.Text = "提示信息";
                    }
                }
            }
        }
    }
    public class BoxReadTime
    {
        public string box_barcode { get; set; }
        public int readTime { get; set; }
    }
    /// <summary>
    /// 存储分道计数数量
    /// </summary>
    public class Channel
    {
        public int ChannelNo { get; set; }
        public int Amount { get; set; }
    }

}
