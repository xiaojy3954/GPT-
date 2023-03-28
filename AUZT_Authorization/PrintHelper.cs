using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AUZT_Authorization
{
    public static class PrintHelper
    {
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(long hwnd, long wMsg, long wParam, string lParam);
        [DllImport("kernel32.dll")]
        static extern bool WriteProfileString(string lpAppName, string lpKeyName, string lpString);
        private const long HWND_BROADCAST = 0xffffL;
        private const long WM_WININICHANGE = 0x1a;

        private static PrintDocument fPrintDocument = new PrintDocument();
        /// <summary>
        /// 获取本机默认打印机名称
        /// </summary>
        public static String DefaultPrinter
        {
            get { return fPrintDocument.PrinterSettings.PrinterName; }
        }
        /// <summary>
        /// 获取本机的打印机列表。列表中的第一项就是默认打印机。
        /// </summary>
        public static List<String> GetLocalPrinters()
        {

            List<String> fPrinters = new List<string>();
            fPrinters.Add(DefaultPrinter); // 默认打印机始终出现在列表的第一项
            foreach (String fPrinterName in PrinterSettings.InstalledPrinters)
            {
                if (!fPrinters.Contains(fPrinterName))
                    fPrinters.Add(fPrinterName);
            }
            return fPrinters;
        }

        /// <summary>
        /// 设定默认打印机
        /// </summary>
        /// <param name="sPrintName">打印机名称</param>
        public static void SetProfileString(string sPrintName)
        {
            string DeviceLine = sPrintName + ",,";

            // 使用 WriteProfileString 设定默认打印机
            WriteProfileString("windows", "Device", DeviceLine);

            // 使用 SendMessage 传送正确的通知给所有最上层的层级窗口。
            // WIN.INI 要在意的应用程序接听此讯息，并且视需要重新读取 WIN.ini
            //SendMessage(HWND_BROADCAST, WM_WININICHANGE, 0, "windows");
        }
    }
}
