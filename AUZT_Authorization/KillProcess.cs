using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AUZT_Authorization
{
    public static class KillProcess
    {
        /// <summary>
        /// 杀掉进程
        /// </summary>
        /// <returns></returns>
        public static string KillPro()
        {
            try
            {
                Process[] ps = Process.GetProcesses();
                foreach (Process item in ps)
                {
                    if (item.ProcessName == "FuchsQRLine")
                    {
                        item.Kill();
                    }
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
