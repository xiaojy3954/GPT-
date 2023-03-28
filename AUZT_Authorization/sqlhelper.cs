using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace AUZT_Authorization
{
    public class sqlhelper
    {
        /// <summary>
        /// 插入打印记录
        /// </summary>
        /// <param name="DEPT">部门</param>
        /// <param name="AUZT_NO">授权书编号</param>
        /// <param name="CUST_NO">经销商编号</param>
        /// <param name="CUST_NAME">经销商名称</param>
        /// <param name="CUST_NAME_EN">经销商英文名称</param>
        /// <param name="COMPANY">授权方公司</param>
        /// <param name="COMPANY_EN">授权方公司英文名称</param>
        /// <param name="PROJECT">竞标项目</param>
        /// <param name="PROJECT_EN">竞标项目英文</param>
        /// <param name="START_DATE">生效日期</param>
        /// <param name="START_DATE_EN">生效日期英文</param>
        /// <param name="END_DATE">到期日期</param>
        /// <param name="END_DATE_EN">到期日期英文</param>
        /// <param name="AUZT_QR">授权书二维码内容</param>
        /// <param name="OP_USER">操作人</param>
        /// <returns></returns>
        public static bool AddPrintrecord(string DEPT, string AUZT_NO, string CUST_NO, string CUST_NAME, string CUST_NAME_EN, string COMPANY, string COMPANY_EN, string PROJECT, string PROJECT_EN, DateTime START_DATE, string START_DATE_EN, DateTime END_DATE, string END_DATE_EN, string AUZT_QR, string OP_USER, string QY_Name, string QY_EN, string YPLB, string YPLB_EN, bool isJB, string productName, string productName_EN)
        {

            string sql = "insert into QX_AUZT_PRINT(DEPT,AUZT_NO,CUST_NO,CUST_NAME,CUST_NAME_EN,COMPANY,COMPANY_EN,PROJECT,PROJECT_EN,START_DATE,START_DATE_EN,END_DATE,END_DATE_EN,AUZT_QR,PRINT_CNT,OP_USER,OP_TIME,QYNAME,QY_EN,YPLB,YPLB_EN,isJB,productName,productName_EN) values(@DEPT,@AUZT_NO,@CUST_NO,@CUST_NAME,@CUST_NAME_EN,@COMPANY,@COMPANY_EN,@PROJECT,@PROJECT_EN,@START_DATE,@START_DATE_EN,@END_DATE,@END_DATE_EN,@AUZT_QR,1,@OP_USER,getdate(),@QYNAME,@QY_EN,@YPLB,@YPLB_EN,@isJB,@productName,@productName_EN ); ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@DEPT",DEPT),  //自定义参数  与参数类型    
                    new SqlParameter("@AUZT_NO", AUZT_NO),
                    new SqlParameter("@CUST_NO", CUST_NO),
                    new SqlParameter("@CUST_NAME", CUST_NAME),
                    new SqlParameter("@CUST_NAME_EN", CUST_NAME_EN),
                    new SqlParameter("@COMPANY", COMPANY),
                    new SqlParameter("@COMPANY_EN", COMPANY_EN),
                    new SqlParameter("@PROJECT", PROJECT),
                    new SqlParameter("@PROJECT_EN", PROJECT_EN),
                    new SqlParameter("@START_DATE", START_DATE),

                    new SqlParameter("@START_DATE_EN",  START_DATE_EN),
                    new SqlParameter("@END_DATE", END_DATE),
                    new SqlParameter("@END_DATE_EN", END_DATE_EN),
                    new SqlParameter("@AUZT_QR", AUZT_QR),
                    new SqlParameter("@OP_USER", OP_USER),
                    new SqlParameter("@QYNAME",QY_Name),

                    new SqlParameter("@QY_EN",  QY_EN),
                    new SqlParameter("@YPLB", YPLB),
                    new SqlParameter("@YPLB_EN", YPLB_EN),
                    new SqlParameter("@isJB", isJB),
                    new SqlParameter("@productName",productName),
                    new SqlParameter("@productName_EN", productName_EN),
            };

                int ds = DB.ExecuteSql(sql, parameters);
                return ds == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 修改打印路径，添加授权书图片路径
        /// </summary>
        /// <param name="uploadUser">图片上传人</param>
        /// <param name="ImageUrl">图片路径</param>
        /// <param name="time">上传时间</param>
        ///  <param name="id">打印记录id</param>
        /// <returns></returns>
        public static bool UpdatePrintrecord(string uploadUser, string ImageUrl, int id)
        {

            string sql = "update  [QX_AUZT_PRINT] set SCANCOPY=@SCANCOPY,uploadUser=@uploadUser,uploadTIME=getdate(),isUpload=1 where AUTOID=@AUTOID  ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@SCANCOPY",ImageUrl),  //自定义参数  与参数类型    
                    new SqlParameter("@uploadUser", uploadUser),
                    new SqlParameter("@AUTOID", id)
            };
                int ds = DB.ExecuteSql(sql, parameters);
                return ds == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 插入补打记录
        /// </summary>
        /// <param name="id">授权记录id</param>
        /// <param name="OP_USER">操作人</param>
        /// <returns></returns>
        public static bool AddPrintrecord_Add(int id, string OP_USER)
        {
            DataSet ds = GetDYJlDataById(id);

            string sql = "insert into QX_AUZT_PRINT_ADD(DEPT,AUZT_NO,OP_USER,OP_TIME) values(@DEPT,@AUZT_NO,@OP_USER,getdate()); ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@DEPT",ds.Tables[0].Rows[0]["DEPT"]),  //自定义参数  与参数类型    
                    new SqlParameter("@AUZT_NO",  ds.Tables[0].Rows[0]["AUZT_NO"]),
                    new SqlParameter("@OP_USER", OP_USER)
            };

                int i = DB.ExecuteSql(sql, parameters);
                return i == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 修改授权商信息
        /// </summary>
        /// <param name="CUST_NAME">授权商名称</param>
        /// <param name="CUST_NAME_EN">授权商名称——英文</param>
        /// <param name="STATUS">状态</param>
        /// <param name="OP_USER">操作人</param>
        /// <returns></returns>
        public static bool UpdateSQS_Add(int id, string CUST_NAME, string CUST_NAME_EN, int STATUS, string OP_USER)
        {

            string sql = "update  [QX_CUSTOMER] set CUST_NAME=@CUST_NAME,CUST_NAME_EN=@CUST_NAME_EN,STATUS=@STATUS,OP_USER=@OP_USER,OP_TIME= getdate() where AUTOID=@AUTOID ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@AUTOID",id),  //自定义参数  与参数类型    
                    new SqlParameter("@CUST_NAME", CUST_NAME),
                    new SqlParameter("@CUST_NAME_EN", CUST_NAME_EN),
                     new SqlParameter("@STATUS", STATUS),
                      new SqlParameter("@OP_USER", OP_USER)
            };

                int i = DB.ExecuteSql(sql, parameters);
                return i == 1;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 修改授权方信息
        /// </summary>
        /// <param name="CUST_NAME">授权商名称</param>
        /// <param name="CUST_NAME_EN">授权商名称——英文</param>
        /// <param name="STATUS">状态</param>
        /// <param name="OP_USER">操作人</param>
        /// <returns></returns>
        public static bool UpdateSQF_Add(int id, string COMPANY, string COMPANY_EN, int STATUS)
        {
            string sql = "update  [QX_COMPANY] set COMPANY=@COMPANY,COMPANY_EN=@COMPANY_EN,STATUS=@STATUS where AUTOID=@AUTOID ";

            try
            {

                SqlParameter[] parameters = {
                    new SqlParameter("@AUTOID",id),  //自定义参数  与参数类型    
                    new SqlParameter("@COMPANY", COMPANY),
                    new SqlParameter("@COMPANY_EN", COMPANY_EN),
                     new SqlParameter("@STATUS", STATUS)
                    
            };


                int i = DB.ExecuteSql(sql, parameters);
                return i == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 插入授权商信息
        /// </summary>
        /// <param name="CUST_NO">授权商编号</param>
        /// <param name="CUST_NAME">授权商名称</param>
        /// <param name="CUST_NAME_EN">授权商英文名称</param>
        /// <param name="STATUS">状态</param>
        /// <param name="OP_USER">操作人</param>
        /// <returns></returns>
        public static bool AddSQS_Add(string CUST_NO, string CUST_NAME, string CUST_NAME_EN, int STATUS, string OP_USER)
        {

            string sql = "insert into [QX_CUSTOMER](CUST_NO,CUST_NAME,CUST_NAME_EN,STATUS,OP_USER,OP_TIME) values(@CUST_NO,@CUST_NAME,@CUST_NAME_EN,@STATUS,@OP_USER,getdate()); ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@CUST_NO",CUST_NO),  //自定义参数  与参数类型    
                    new SqlParameter("@CUST_NAME", CUST_NAME),
                    new SqlParameter("@CUST_NAME_EN", CUST_NAME_EN),
                     new SqlParameter("@STATUS", STATUS),
                     new SqlParameter("@OP_USER", OP_USER)
            };

                int i = DB.ExecuteSql(sql, parameters);
                return i == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 插入授权方信息
        /// </summary>
        /// <param name="CUST_NO">授权方名称</param>
        /// <param name="CUST_NAME">授权方英文名称</param>
        /// <param name="STATUS">状态</param>
        /// <returns></returns>
        public static bool AddSQF_Add(string COMPANY, string COMPANY_EN, int STATUS)
        {

            string sql = "insert into [QX_COMPANY](COMPANY,COMPANY_EN,STATUS) values(@COMPANY,@COMPANY_EN,@STATUS); ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@COMPANY",COMPANY),  //自定义参数  与参数类型    
                    new SqlParameter("@COMPANY_EN", COMPANY_EN),
                  
                     new SqlParameter("@STATUS", STATUS)
                    
            };
                int i = DB.ExecuteSql(sql, parameters);
                return i == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 插入授权书扫描表
        /// </summary>
        /// <param name="CUST_NO">授权方名称</param>
        /// <param name="CUST_NAME">授权方英文名称</param>
        /// <param name="STATUS">状态</param>
        /// <returns></returns>
        public static bool AddAUZT_COPY(string AUZT_NO, string START_DATE, string END_DATE, string SCANCOPY, string OP_USER)
        {

            string sql = "insert into [QX_AUZT_COPY](AUZT_NO,START_DATE,END_DATE,SCANCOPY,OP_USER,OP_TIME) values(@AUZT_NO,@START_DATE,@END_DATE,@SCANCOPY,@OP_USER,getdate()); ";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@AUZT_NO",AUZT_NO),  //自定义参数  与参数类型    
                    new SqlParameter("@START_DATE", START_DATE),
                     new SqlParameter("@END_DATE", END_DATE),
                     new SqlParameter("@SCANCOPY", SCANCOPY),
                 new SqlParameter("@OP_USER", OP_USER)
               
            };

                int i = DB.ExecuteSql(sql, parameters);
                return i == 1;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        ///// <summary>
        ///// 获取所有的打印记录
        ///// </summary>
        ///// <returns></returns>
        //public static DataSet GetDYJlData()
        //{
        //    Hashtable userLogin = UserLogin.GetUserLogin();
        //    User user = (User)userLogin["user"];
        //    string sql = string.Empty;

        //    sql = "select AUTOID id,DEPT 部门,AUZT_NO 授权书编号,CUST_NAME 经销商,COMPANY 授权方公司,PROJECT 竞标项目,START_DATE 生效日期,END_DATE 失效日期,OP_USER 操作人,OP_TIME 操作时间,(case [isUpload] when 0 then '未上传' when 1 then '已上传' else '空的' end)是否上传图片  from QX_AUZT_PRINT  ";


        //    var db = DB.conntionDB;
        //    var dbc = db.GetSqlStringCommand(sql);
        //    try
        //    {
        //        if (user.DEPT != "-1")
        //        {
        //            db.AddInParameter(dbc, "@DEPT", DbType.AnsiString, user.DEPT);
        //        }
        //        DataSet ds = db.ExecuteDataSet(dbc);
        //        return ds;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
        /// <summary>
        /// 获取所有的打印记录
        /// </summary>
        /// <returns></returns>
        public static DataSet GetDYJlData(string sqlwhere, int isdelete)
        {

            string sql = string.Empty;

            sql = "select AUTOID id,DEPT 部门,AUZT_NO 授权书编号,CUST_NAME 经销商,COMPANY 授权方公司,PROJECT 竞标项目,START_DATE 生效日期,END_DATE 失效日期,OP_USER 操作人,OP_TIME 操作时间,(case [isUpload] when 0 then '未上传' when 1 then '已上传' else '空的' end)是否上传图片,(case [isDelete] when 0 then '正常' when 1 then '删除' else '空的' end)状态  from QX_AUZT_PRINT where isDelete=@isDelete " + sqlwhere + "ORDER BY OP_TIME desc";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@isDelete",isdelete)//自定义参数  与参数类型    
            };
                DataSet ds = DB.Query(sql, parameters);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 删除还原打印记录
        /// </summary>
        /// <param name="id">打印记录id</param>
        /// <returns></returns>
        public static int DeleteDYJL(int id, int state)
        {
            string sqlstr = string.Format(" update QX_AUZT_PRINT   set isDelete=@isDelete where AUTOID=@AUTOID and isDelete!=@isDelete ");

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@AUTOID",id),//自定义参数  与参数类型    
                    new SqlParameter("@isDelete",state)
            };
                int i = DB.ExecuteSql(sqlstr, parameters);

                return i;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取所有经销商信息
        /// </summary>
        /// <returns></returns>
        public static DataSet GetSQSData()
        {

            string sql = "SELECT  [AUTOID] id,[CUST_NO] 经销商编号,[CUST_NAME] 经销商名称,[CUST_NAME_EN] 经销商英文名称,[OP_USER] 操作人,[OP_TIME] 操作时间,(case [STATUS] when 0 then '启用' when 1 then '禁用' else '无' end)状态 FROM [QX_CUSTOMER]";

            try
            {
                DataSet ds = DB.Query(sql);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 获取所有授权方信息
        /// </summary>
        /// <returns></returns>
        public static DataSet GetSQFData()
        {

            string sql = "SELECT  [AUTOID] id,[COMPANY] 授权方名称,[COMPANY_EN] 授权方英文名称,(case [STATUS] when 0 then '启用' when 1 then '禁用' else '无' end)状态  FROM [QX_COMPANY]";

            try
            {
                DataSet ds = DB.Query(sql);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据id获取经销商信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DataSet GetSQSData(int id)
        {

            string sql = "SELECT STATUS,[AUTOID],[CUST_NO] ,[CUST_NAME] ,[CUST_NAME_EN] ,[OP_USER] ,[OP_TIME]  FROM [QX_CUSTOMER] where AUTOID=@AUTOID";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@AUTOID",id)//自定义参数  与参数类型    
                
            };

                DataSet ds = DB.Query(sql, parameters);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据sql条件获取经销商信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DataSet GetSQSData(string sqlwhere)
        {

            string sql = "SELECT [AUTOID] id,[CUST_NO] 经销商编号,[CUST_NAME] 经销商名称,[CUST_NAME_EN] 经销商英文名称,[OP_USER] 操作人,[OP_TIME] 操作时间,(case [STATUS] when 0 then '启用' when 1 then '禁用' else '无' end)状态  FROM [QX_CUSTOMER] where 1=1 " + sqlwhere;

            try
            {
                DataSet ds = DB.Query(sql);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据id获取授权方信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DataSet GetSQFData(int id)
        {

            string sql = "SELECT STATUS,[AUTOID],[COMPANY] ,[COMPANY_EN]  FROM [QX_COMPANY] where AUTOID=@AUTOID";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@AUTOID",id)//自定义参数  与参数类型    
                
            };

                DataSet ds = DB.Query(sql, parameters);
                return ds;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据查询条件获取授权方信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DataSet GetSQFData(string sqlwhere)
        {

            string sql = "SELECT [AUTOID] id,[COMPANY] 授权方名称,[COMPANY_EN] 授权方英文名称,(case [STATUS] when 0 then '启用' when 1 then '禁用' else '无' end)状态  FROM [QX_COMPANY] where 1=1" + sqlwhere;

            try
            {
                DataSet ds = DB.Query(sql);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 获取所有的打印记录
        /// </summary>
        /// <returns></returns>
        public static DataSet GetDYJlDataById(int id)
        {
            string sql = "select * from QX_AUZT_PRINT where AUTOID=@id";

            try
            {
                SqlParameter[] parameters = {
                    new SqlParameter("@id",id)//自定义参数  与参数类型    
                
            };
                DataSet ds = DB.Query(sql, parameters);
                return ds;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 根据最新订单号生成新授权书编号
        /// </summary>
        /// <param name="dept">部门编号</param>
        /// <returns></returns>
        public static string GetNoByNo(string dept)
        {
            //获取最新的授权书编号
            string sql = "select value from QX_Numbering where DEPT=@DEPT";
            try
            {
                string newNO = string.Empty;
                SqlParameter[] parameters = {
                    new SqlParameter("@DEPT",dept)//自定义参数  与参数类型    
            };

                string orderNo = DB.GetSingle(sql, parameters).ToString();
                string year = orderNo.Substring(0, 4);
                string number = orderNo.Substring(7, 4);
                string DEPT = orderNo.Substring(4, 3);

                if (year == DateTime.Now.Year.ToString())
                {
                    int num = Convert.ToInt32(number) + 1;
                    newNO = year + DEPT + num.ToString("0000");

                }
                else
                {
                    newNO = DateTime.Now.Year.ToString() + DEPT + "0001";
                }
                //将新生成的订单号存入流水号表
                string sql_inserint = "update  QX_Numbering set value=@value where DEPT=@DEPT ";

                SqlParameter[] parameters2 = {
                    new SqlParameter("@DEPT",dept),//自定义参数  与参数类型    
                     new SqlParameter("@value",newNO)
            };

                int i = DB.ExecuteSql(sql_inserint, parameters2);
                if (i == 1)
                {
                    return newNO;
                }
                else
                {
                    return string.Empty;
                }


            }
            catch (Exception ex)
            {
                return string.Empty;
            }



        }
        /// <summary>
        /// 导出报表
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="myDGV"></param>
        public static void ExportExcel(string fileName, DataGridView myDGV)
        {
            if (myDGV.Rows.Count > 0)
            {

                string saveFileName = "";
                //bool fileSaved = false;  
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = "xls";
                saveDialog.Filter = "Excel文件|*.xls";
                saveDialog.FileName = fileName;
                saveDialog.ShowDialog();
                saveFileName = saveDialog.FileName;
                if (saveFileName.IndexOf(":") < 0) return; //被点了取消   
                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                if (xlApp == null)
                {
                    MessageBox.Show("无法创建Excel对象，可能您的机子未安装Excel");
                    return;
                }

                Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];//取得sheet1  

                //写入标题  
                for (int i = 0; i < myDGV.ColumnCount; i++)
                {
                    worksheet.Cells[1, i + 1] = myDGV.Columns[i].HeaderText;
                }
                //写入数值  
                for (int r = 0; r < myDGV.Rows.Count; r++)
                {
                    for (int i = 0; i < myDGV.ColumnCount; i++)
                    {
                        worksheet.Cells[r + 2, i + 1] = myDGV.Rows[r].Cells[i].Value;
                    }
                    System.Windows.Forms.Application.DoEvents();
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
                        MessageBox.Show("导出文件时出错,文件可能正被打开！\n" + ex.Message);
                    }

                }
                //else  
                //{  
                //    fileSaved = false;  
                //}  
                xlApp.Quit();
                GC.Collect();//强行销毁   
                // if (fileSaved && System.IO.File.Exists(saveFileName)) System.Diagnostics.Process.Start(saveFileName); //打开EXCEL  
                MessageBox.Show(fileName + "的简明资料保存成功", "提示", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("报表为空,无表格需要导出", "提示", MessageBoxButtons.OK);
            }

        }
        /// <summary>
        /// 获取英文日期
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateTimeBystring_EN(DateTime dt)
        {

            string[] m = new string[] { "January ", "February ", "March ", "April ", "May ", "June ", "July ", "August ", "September ", "October ", "November ", "December " };

            string[] d = new string[] { "st", "nd", "rd", "th" };
            int mn = Convert.ToInt32(dt.Month);
            int wn = Convert.ToInt32(dt.DayOfWeek);
            int dn = Convert.ToInt32(dt.Day);
            string dns;
            if (((dn % 10) < 1) || ((dn % 10) > 3))
            {
                dns = d[3];
            }
            else
            {
                dns = d[(dn % 10) - 1];
                if ((dn == 11) || (dn == 12))
                {
                    dns = d[3];
                }
            }

            return m[mn - 1] + " " + dn+dns + "," + dt.Year.ToString();

        }
        /// <summary>
        /// 获取中文日期
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DateTimeBystring(DateTime dt)
        {

            return dt.Year + "年" + dt.Month + "月" + dt.Day + "日";
        }
        public static Image GetQm(string dept)
        {
            switch (dept)
            {
                case "OEM":
                    return Properties.Resources.空;
                case "IND":
                    return Properties.Resources.Dr_Arcwres;
                case "RET":
                    return Properties.Resources.GAOXUEMEI;
                case "MIN":
                    return Properties.Resources.ZHANGXIONGYI;
                case "FLT":
                    return Properties.Resources.mertel;
                default:
                    return null;
            }

        }
        public static Image GetQmCEO(string dept)
        {
            return Properties.Resources.ZHUQINGPING;

        }
        /// <summary>
        /// 添加或修改监控邮件信息
        /// </summary>
        /// <param name="DEPT">部门</param>
        /// <param name="SJ_MAIL">收件箱账号</param>
        /// <param name="CC_MAIL">抄送账号</param>
        /// <param name="FJ_MAIL">发件箱账号</param>
        /// <param name="FJ_MAIL_PWD">发件箱密码</param>
        /// <param name="OP_USER">操作人</param>
        /// <returns></returns>
        public static int Addmail(string DEPT, string SJ_MAIL, string CC_MAIL, string FJ_MAIL, string FJ_MAIL_PWD, string OP_USER)
        {
            string sql = @"if exists(select * from [QX_MAIL] where [DEPT]=@DEPT)   
                             update QX_MAIL set SJ_MAIL=@SJ_MAIL,CC_MAIL=@CC_MAIL,FJ_MAIL=@FJ_MAIL,FJ_MAIL_PWD=@FJ_MAIL_PWD,OP_USER=@OP_USER,OP_TIME=getdate() where DEPT=@DEPT
                           else   
                             INSERT INTO QX_MAIL (DEPT,SJ_MAIL,CC_MAIL,FJ_MAIL,FJ_MAIL_PWD,OP_USER,OP_TIME) VALUES (@DEPT,@SJ_MAIL,@CC_MAIL,@FJ_MAIL,@FJ_MAIL_PWD,@OP_USER,getdate())
                            ";
            try
            {
                SqlParameter[] parameters2 = {
                    new SqlParameter("@DEPT",DEPT),//自定义参数  与参数类型    
                     new SqlParameter("@SJ_MAIL",SJ_MAIL),
                     new SqlParameter("@CC_MAIL",CC_MAIL),
                     new SqlParameter("@FJ_MAIL",FJ_MAIL),
                     new SqlParameter("@FJ_MAIL_PWD",FJ_MAIL_PWD),
                     new SqlParameter("@OP_USER",OP_USER)
                  
            };

                int i = DB.ExecuteSql(sql, parameters2);
                return i;
            }
            catch (Exception ex)
            {
                return -1;
            }

        }

        public static DataSet GetMail()
        {
            string sql = "select * from [QX_MAIL]";
            try
            {
                DataSet ds = DB.Query(sql);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #region MyRegion
        /// <summary>
        /// 进行DES加密。
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串。</param>
        /// <param name="sKey">密钥，且必须为8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        public static string Encrypt(string pToEncrypt, string sKey)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                return str;
            }
        }


        // <summary>
        // 进行DES解密。
        // </summary>
        // <param name="pToDecrypt">要解密的以Base64</param>
        // <param name="sKey">密钥，且必须为8位。</param>
        // <returns>已解密的字符串。</returns>
        public static string Decrypt(string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                return str;
            }
        }
        #endregion

        //加密web.Config中的指定节
        public static void ProtectSection(string sectionName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionName);
            if (section != null && !section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                config.Save();
            }
        }

        //解密web.Config中的指定节
        public static void UnProtectSection(string sectionName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection section = config.GetSection(sectionName);
            if (section != null && section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();
                config.Save();
            }
        }

      
    }
}
