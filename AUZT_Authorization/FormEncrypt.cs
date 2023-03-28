using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AUZT_Authorization
{
    public partial class FormEncrypt : Form
    {
        public FormEncrypt()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = sqlhelper.Encrypt(textBox1.Text, "佳阳sama赛高");
        }
    }
}
