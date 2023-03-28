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
    public partial class Form2 : CCSkinMain
    {
        public Form2()
        {
          
            InitializeComponent();
        }
        public delegate void RefreshTheForm(string str);//声明委托
        public RefreshTheForm Refreshtheform;

        private void Form2_Load(object sender, EventArgs e)
        {
            //this.Owner.Hide();
        }//委托对象
    }
}
