using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace AUZT_Authorization
{
   public class ImgGDI
    {
        public ImgGDI()
        {
            //构造函数
        }
        ///
        /// Bitmap转换byte[]数组
        ///
        ///
        ///
        public byte[] Bmptobyte(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            ms.Flush();
            byte[] buffer = ms.GetBuffer();
            ms.Close();
            return buffer;
        }
        ///
        /// byte[]数组转换Bitmap
        ///
        ///
        ///
        public static Bitmap bytetobmp(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);
            Bitmap bmp = new Bitmap(ms);
            ms.Close();
            return bmp;
        }

        ///
        /// GDI压缩图片
        ///
        /// 传入参数Bitmap
        ///
        public static byte[] ImageGdi(Bitmap bmp)
        {
            Bitmap xbmp = new Bitmap(bmp);
            MemoryStream ms = new MemoryStream();
            xbmp.Save(ms, ImageFormat.Jpeg);
            byte[] buffer;
            ms.Flush();
            if (ms.Length > 95000)
            {
                //buffer = ms.GetBuffer();
                double new_width = 0;
                double new_height = 0;

                Image m_src_image = Image.FromStream(ms);
                if (m_src_image.Width >= m_src_image.Height)
                {
                    new_width = 1024;
                    new_height = new_width * m_src_image.Height / (double)m_src_image.Width;
                }
                else if (m_src_image.Height >= m_src_image.Width)
                {
                    new_height = 768;
                    new_width = new_height * m_src_image.Width / (double)m_src_image.Height;
                }

                Bitmap bbmp = new Bitmap((int)new_width, (int)new_height, m_src_image.PixelFormat);
                Graphics m_graphics = Graphics.FromImage(bbmp);
                m_graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                m_graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                m_graphics.DrawImage(m_src_image, 0, 0, bbmp.Width, bbmp.Height);

                ms = new MemoryStream();

                bbmp.Save(ms, ImageFormat.Jpeg);
                buffer = ms.GetBuffer();
                ms.Close();

                return buffer;
            }
            else
            {
                buffer = ms.GetBuffer();
                ms.Close();
                return buffer;
            }
        }

    }
}
