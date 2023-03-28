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
    public partial class Main_JK : CCSkinMain
    {
        public Main_JK()
        {
            InitializeComponent();
        }
        User user=new User();
      

        private void Main_JK_MouseDown(object sender, MouseEventArgs e)
        {
            //Main.ReleaseCapture();
            //Main.SendMessage(base.Handle, 274, 61458, 0);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                splitContainer1.Panel1Collapsed = false;
                grp_flt_fj.Enabled = false;
                grp_oem_fj.Enabled = false;
                grp_ind_fj.Enabled = false;
                grp_min_fj.Enabled = false;
                grp_ref_fj.Enabled = false;
            }
            else
            {
                splitContainer1.Panel1Collapsed = true;
                grp_flt_fj.Enabled = true;
                grp_oem_fj.Enabled = true;
                grp_ind_fj.Enabled = true;
                grp_min_fj.Enabled = true;
                grp_ref_fj.Enabled = true;
            }
        }

        private void btn_OEM_Click(object sender, EventArgs e)
        {
            string fjmail = string.Empty;
            string fjmailpass = string.Empty;
            try
            {
                if (checkBox1.Checked)
                {
                    fjmail = txt_FJ_MAIL.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD.Text.Trim();
                }
                else
                {
                    fjmail = txt_FJ_MAIL_OEM.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD_OEM.Text.Trim();
                }

                int i = sqlhelper.Addmail("OEM", txt_SJ_MAIL_OEM.Text.Trim(), txt_CC_MAIL_OEM.Text.Trim(), fjmail,
                                          fjmailpass, user.NAME);
                if (i == 1)
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
        }

        private void Main_JK_Load(object sender, EventArgs e)
        {
            Hashtable userLogin = UserLogin.GetUserLogin();
            user = (User)userLogin["user"];
            DataSet ds = sqlhelper.GetMail();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                if (dr["DEPT"].ToString() == "OEM")
                {
                    txt_FJ_MAIL_OEM.Text = dr["FJ_MAIL"].ToString().Trim();
                    txt_FJ_MAIL_PWD_OEM.Text = dr["FJ_MAIL_PWD"].ToString().Trim();
                    txt_SJ_MAIL_OEM.Text = dr["SJ_MAIL"].ToString().Trim();
                    txt_CC_MAIL_OEM.Text = dr["CC_MAIL"].ToString().Trim();
                }
                if (dr["DEPT"].ToString() == "IND")
                {
                    txt_FJ_MAIL_IND.Text = dr["FJ_MAIL"].ToString().Trim();
                    txt_FJ_MAIL_PWD_IND.Text = dr["FJ_MAIL_PWD"].ToString().Trim();
                    txt_SJ_MAIL_IND.Text = dr["SJ_MAIL"].ToString().Trim();
                    txt_CC_MAIL_IND.Text = dr["CC_MAIL"].ToString().Trim();
                }
                if (dr["DEPT"].ToString() == "REF")
                {
                    txt_FJ_MAIL_REF.Text = dr["FJ_MAIL"].ToString().Trim();
                    txt_FJ_MAIL_PWD_REF.Text = dr["FJ_MAIL_PWD"].ToString().Trim();
                    txt_SJ_MAIL_REF.Text = dr["SJ_MAIL"].ToString().Trim();
                    txt_CC_MAIL_REF.Text = dr["CC_MAIL"].ToString().Trim();
                }
                if (dr["DEPT"].ToString() == "FLT")
                {
                    txt_FJ_MAIL_FLT.Text = dr["FJ_MAIL"].ToString().Trim();
                    txt_FJ_MAIL_PWD_FLT.Text = dr["FJ_MAIL_PWD"].ToString().Trim();
                    txt_SJ_MAIL_FLT.Text = dr["SJ_MAIL"].ToString().Trim();
                    txt_CC_MAIL_FLT.Text = dr["CC_MAIL"].ToString().Trim();
                }
                if (dr["DEPT"].ToString() == "MIN")
                {
                    txt_FJ_MAIL_MIN.Text = dr["FJ_MAIL"].ToString().Trim();
                    txt_FJ_MAIL_PWD_MIN.Text = dr["FJ_MAIL_PWD"].ToString().Trim();
                    txt_SJ_MAIL_MIN.Text = dr["SJ_MAIL"].ToString().Trim();
                    txt_CC_MAIL_MIN.Text = dr["CC_MAIL"].ToString().Trim();
                }
            }
        }

        private void btn_IND_Click(object sender, EventArgs e)
        {
            string fjmail = string.Empty;
            string fjmailpass = string.Empty;
            try
            {
                if (checkBox1.Checked)
                {
                    fjmail = txt_FJ_MAIL.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD.Text.Trim();
                }
                else
                {
                    fjmail = txt_FJ_MAIL_IND.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD_IND.Text.Trim();
                }

                int i = sqlhelper.Addmail("IND", txt_SJ_MAIL_IND.Text.Trim(), txt_CC_MAIL_IND.Text.Trim(), fjmail,
                                          fjmailpass, user.NAME);
                if (i == 1)
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
        }

        private void btn_REF_Click(object sender, EventArgs e)
        {
            string fjmail = string.Empty;
            string fjmailpass = string.Empty;
            try
            {
                if (checkBox1.Checked)
                {
                    fjmail = txt_FJ_MAIL.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD.Text.Trim();
                }
                else
                {
                    fjmail = txt_FJ_MAIL_REF.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD_REF.Text.Trim();
                }

                int i = sqlhelper.Addmail("REF", txt_SJ_MAIL_REF.Text.Trim(), txt_CC_MAIL_REF.Text.Trim(), fjmail,
                                          fjmailpass, user.NAME);
                if (i == 1)
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btn_FLT_Click(object sender, EventArgs e)
        {
             string fjmail = string.Empty;
            string fjmailpass = string.Empty;
            try
            {
                if (checkBox1.Checked)
                {
                    fjmail = txt_FJ_MAIL.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD.Text.Trim();
                }
                else
                {
                    fjmail = txt_FJ_MAIL_FLT.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD_FLT.Text.Trim();
                }

                int i = sqlhelper.Addmail("FLT", txt_SJ_MAIL_FLT.Text.Trim(), txt_CC_MAIL_FLT.Text.Trim(), fjmail,
                                          fjmailpass, user.NAME);
                if (i == 1)
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btn_MIN_Click(object sender, EventArgs e)
        {
            string fjmail = string.Empty;
            string fjmailpass = string.Empty;
            try
            {
                if (checkBox1.Checked)
                {
                    fjmail = txt_FJ_MAIL.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD.Text.Trim();
                }
                else
                {
                    fjmail = txt_FJ_MAIL_MIN.Text.Trim();
                    fjmailpass = txt_FJ_MAIL_PWD_MIN.Text.Trim();
                }

                int i = sqlhelper.Addmail("MIN", txt_SJ_MAIL_MIN.Text.Trim(), txt_CC_MAIL_MIN.Text.Trim(), fjmail,
                                          fjmailpass, user.NAME);
                if (i == 1)
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
     
      

       
    }
}
