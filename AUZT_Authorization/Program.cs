using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace AUZT_Authorization
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //sqlhelper.UnProtectSection("connectionStrings");
            //string ConnectionString = sqlhelper.Encrypt(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString, "佳阳sama赛高");
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
            //Application.Run(new FormEncrypt());

        }
    }
}
