using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CCWin;

namespace AUZT_Authorization
{
    public partial class Main_SQS_Add : CCSkinMain
    {
        private int Id;
         User user=new User();
        public Main_SQS_Add(int id)
        {
            InitializeComponent();
            this.Id = id;
        }
        public delegate void RefreshTheForm();//声明委托
        public RefreshTheForm Refreshtheform;//委托对象
      
        private void Main_SQS_Add_MouseDown(object sender, MouseEventArgs e)
        {
            //Main.ReleaseCapture();
            //Main.SendMessage(base.Handle, 274, 61458, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string CUST_NAME = txt_CUST_NAME.Text.Trim();
            string CUST_NAME_EN = txt_CUST_NAME_EN.Text.Trim();
            string CUST_NO = txt_CUST_NO.Text.Trim();
            int status = 0;
            if (radioButton1.Checked)
            { status = 0; }
            else if (radioButton2.Checked)
            { status = 1; }

            if (string.IsNullOrEmpty(CUST_NO))
            {
                MessageBox.Show("请填写经销商编号");
                return;
            }
            if (string.IsNullOrEmpty(CUST_NAME))
            {
                MessageBox.Show("请填写经销商名称");
                return;
            }
            if (string.IsNullOrEmpty(CUST_NAME_EN))
            {
                MessageBox.Show("请填写经销商英文名称");
                return;
            }
            try
            {
                bool b = false;
                if(Id==-1)
                {
                    b = sqlhelper.AddSQS_Add(CUST_NO, CUST_NAME, CUST_NAME_EN, status,user.NAME);
                }
                else
                {
                    b = sqlhelper.UpdateSQS_Add( Id,CUST_NAME, CUST_NAME_EN, status,user.NAME);
                }
                //添加经销商数据
              
                if (!b)
                {
                    MessageBox.Show("操作失败");
                }
                else
                {
                    MessageBox.Show("操作成功");
                    if (Refreshtheform != null)
                    {
                        Refreshtheform();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Main_SQS_Add_Load(object sender, EventArgs e)
        {
            try
            {
                Hashtable userLogin = UserLogin.GetUserLogin();
                 user = (User)userLogin["user"];
                if (Id != -1)
                {
                    //根据id获取授权商数据
                    DataSet ds = sqlhelper.GetSQSData(Id);
                    if (ds == null)
                    {
                        MessageBox.Show("获取经销商信息失败");
                    }
                    else
                    {
                        txt_CUST_NO.Text = ds.Tables[0].Rows[0]["CUST_NO"].ToString();
                        txt_CUST_NO.Enabled = false;
                        txt_CUST_NAME.Text = ds.Tables[0].Rows[0]["CUST_NAME"].ToString();
                        txt_CUST_NAME_EN.Text = ds.Tables[0].Rows[0]["CUST_NAME_EN"].ToString();
                        if (ds.Tables[0].Rows[0]["STATUS"].ToString() == "0")
                        {
                            radioButton1.Checked = true;
                        }
                        else
                        {
                            radioButton2.Checked = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
          
        }


    }
}
