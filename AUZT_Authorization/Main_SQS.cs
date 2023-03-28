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
    public partial class Main_SQS : CCSkinMain
    {
        public Main_SQS()
        {
            InitializeComponent();
        }
        public delegate void RefreshTheForm();//声明委托
        public RefreshTheForm Refreshtheform;//委托对象
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Main_SQS_MouseDown(object sender, MouseEventArgs e)
        {
            //Main.ReleaseCapture();
            //Main.SendMessage(base.Handle, 274, 61458, 0);
        }

        private void Main_SQS_Load(object sender, EventArgs e)
        {
            BandindataGridView();
        }
        private void BandindataGridView()
        {
            //查询授权商数据
            DataSet ds = sqlhelper.GetSQSData();
            if (ds != null)
            {

                this.dataGridView1.DataSource = ds.Tables[0];
            }
            
        }

        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main_SQS_Add sqs_add=new Main_SQS_Add(-1);
            sqs_add.Refreshtheform = BandindataGridView;//将方法赋给委托对象  
            sqs_add.ShowDialog();
        }
        private int id;
        private void 修改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            id = Convert.ToInt32(this.dataGridView1.SelectedRows[0].Cells["id"].Value);
            Main_SQS_Add sqs_add = new Main_SQS_Add(id);
            sqs_add.Refreshtheform = BandindataGridView;//将方法赋给委托对象  
            sqs_add.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder sqlwhere = new StringBuilder();

            if (!string.IsNullOrEmpty(txt_CUST_NO.Text.Trim()))
            {
                sqlwhere.Append(string.Format("and CUST_NO like '%{0}%'", txt_CUST_NO.Text.Trim()));
            }
            if (!string.IsNullOrEmpty(txt_CUST_NAME.Text.Trim()))
            {
                sqlwhere.Append(string.Format("and CUST_NAME like '%{0}%'", txt_CUST_NAME.Text.Trim()));
            }
            //查询授权方数据
            DataSet ds = sqlhelper.GetSQSData(sqlwhere.ToString());
            if (ds != null)
            {

                this.dataGridView1.DataSource = ds.Tables[0];
            }
            else
            {
                this.dataGridView1.DataSource = null;
            }
        }

        private void Main_SQS_FormClosing(object sender, FormClosingEventArgs e)
        {
            //使用委托更新下啦框数据
            if (Refreshtheform != null)
            {
                Refreshtheform();
               
            }
        }

    }
}
