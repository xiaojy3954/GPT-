using System;
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
    public partial class Main_SQF_ADD : CCSkinMain
    {
        private int Id;
        public Main_SQF_ADD(int id)
        {
            InitializeComponent();
            this.Id = id;
        }
        public delegate void RefreshTheForm();//声明委托
        public RefreshTheForm Refreshtheform;//委托对象
        private void Main_SQF_ADD_MouseDown(object sender, MouseEventArgs e)
        {
            //Main.ReleaseCapture();
            //Main.SendMessage(base.Handle, 274, 61458, 0);
        }

   

        private void Main_SQF_ADD_Load(object sender, EventArgs e)
        {
            try
            {
                if (Id != -1)
                {
                    //根据id获取授权商数据
                    DataSet ds = sqlhelper.GetSQFData(Id);
                    if (ds == null)
                    {
                        MessageBox.Show("获取经销商信息失败");
                    }
                    else
                    {
                        txt_COMPANY.Text = ds.Tables[0].Rows[0]["COMPANY"].ToString();

                        txt_COMPANY_EN.Text = ds.Tables[0].Rows[0]["COMPANY_EN"].ToString();
                       
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

        private void button1_Click(object sender, EventArgs e)
        {
            string COMPANY = txt_COMPANY.Text.Trim();
            string COMPANY_EN = txt_COMPANY_EN.Text.Trim();
          
            int status = 0;
            if (radioButton1.Checked)
            { status = 0; }
            else if (radioButton2.Checked)
            { status = 1; }

            if (string.IsNullOrEmpty(COMPANY))
            {
                MessageBox.Show("请填写授权方名称");
                return;
            }
            if (string.IsNullOrEmpty(COMPANY_EN))
            {
                MessageBox.Show("请填写经授权方英文名称");
                return;
            }
           
            try
            {
                bool b = false;
                if (Id == -1)
                {
                    b = sqlhelper.AddSQF_Add(COMPANY, COMPANY_EN, status);
                }
                else
                {
                    b = sqlhelper.UpdateSQF_Add(Id, COMPANY, COMPANY_EN, status);
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
    }
}
