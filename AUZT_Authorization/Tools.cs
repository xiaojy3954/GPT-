using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AUZT_Authorization
{
    class Tools
    {
        public static bool IsNumOrZimString(string strNum)
        {
            Regex notWholePattern = new Regex("^[A-Za-z0-9]+$");
            return notWholePattern.IsMatch(strNum, 0);

        }
        public static string AddZero(string txtstr, int length)
        {
            bool flagg = true;
            while (flagg)
            {
                if (txtstr.Length != length)
                {
                    txtstr = "0" + txtstr;
                }
                else
                {
                    flagg = false;
                }

            }
            return txtstr;
        }

        /// <summary>
        /// 给当前Graphics对象画二维码图像
        /// </summary>
        /// <param name="contents">二维码内容</param>
        /// <param name="X">X轴坐标</param>
        /// <param name="Y">Y轴坐标</param>
        /// <param name="width">二维码宽度</param>
        /// <param name="height">二维码高度</param>
        /// <param name="gp">当前画图对象</param>
        public static void SetQRCodeImage(string contents, int X, int Y, int width, int height, Graphics gp)
        {
            DotNetBarcode bc = new DotNetBarcode();
            bc.Type = DotNetBarcode.Types.QRCode;
            bc.PrintCheckDigitChar = true;
            bc.WriteBar(contents, X, Y, width, height, gp);
        }
    }
}
