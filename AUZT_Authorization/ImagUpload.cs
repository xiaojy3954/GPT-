using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using O2S.Components.PDFRender4NET;

namespace AUZT_Authorization
{
    public partial class ImagUpload : Form
    {
        public enum Definition
        {
            One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10
        }
        private int ID = -1;
        public ImagUpload(int id)
        {
            InitializeComponent();
            this.ID = id;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public delegate void RefreshTheForm();//声明委托
        public RefreshTheForm Refreshtheform;//委托对象
        private void ImagUpload_MouseDown(object sender, MouseEventArgs e)
        {
            //Main.ReleaseCapture();
            //Main.SendMessage(base.Handle, 274, 61458, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //初始化一个OpenFileDialog类
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "(PDF文件)|*.pdf|(图片文件)|*.jpg;*.png;*.jpeg;*.bmp";
                //判断用户是否正确的选择了文件
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    //获取用户选择文件的后缀名
                    string extension = Path.GetExtension(fileDialog.FileName);
                    //声明允许的后缀名

                    string[] str = new string[] { ".pdf", ".PDF", ".pdf", ".jpg", ".JPG", ".png", ".PNG", ".jpeg", ".JPEG", ".BMP" };
                    if (!((IList)str).Contains(extension))
                    {
                        MessageBox.Show("仅能上传pdf或图片格式的文件！");
                    }
                    else
                    {
                        txt_path.Text = fileDialog.FileName;

                    }
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void ImagUpload_Load(object sender, EventArgs e)
        {
            try
            {
                if (ID == -1)
                {
                    MessageBox.Show("参数丢失");
                    button2.Visible = false;
                    button1.Visible = false;
                }
                else
                {
                    //根据id获取授权书打印记录
                    DataSet ds = sqlhelper.GetDYJlDataById(ID);
                    lbl_AUZT_NO.Text = ds.Tables[0].Rows[0]["AUZT_NO"].ToString().Trim();
                    lbl_CUST_NAME.Text = ds.Tables[0].Rows[0]["CUST_NAME"].ToString().Trim();
                    lbl_CUST_NAME_EN.Text = ds.Tables[0].Rows[0]["CUST_NAME_EN"].ToString().Trim();
                    lbl_COMPANY.Text = ds.Tables[0].Rows[0]["COMPANY"].ToString().Trim();
                    lbl_COMPANY_EN.Text = ds.Tables[0].Rows[0]["COMPANY_EN"].ToString().Trim();
                    string PROJECT = ds.Tables[0].Rows[0]["PROJECT"].ToString().Trim();
                    lbl_PROJECT.Text = string.IsNullOrEmpty(PROJECT) ? "无" : PROJECT;
                    string PROJECT_EN = ds.Tables[0].Rows[0]["PROJECT_EN"].ToString().Trim();
                    lbl_PROJECT_EN.Text = string.IsNullOrEmpty(PROJECT_EN) ? "无" : PROJECT_EN;

                    lbl_START_DATE.Text = ds.Tables[0].Rows[0]["START_DATE"].ToString().Trim();
                    lbl_END_DATE.Text = ds.Tables[0].Rows[0]["END_DATE"].ToString().Trim();
                    lbl_OP_USER.Text = ds.Tables[0].Rows[0]["OP_USER"].ToString().Trim();
                    lbl_OP_TIME.Text = ds.Tables[0].Rows[0]["OP_TIME"].ToString().Trim();

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string path = txt_path.Text.Trim();
                byte[] fileBytes = new byte[0];
                #region pdf

                string s = System.IO.Path.GetExtension(path).ToUpper();
                if ( s== ".PDF")
                {
                    Bitmap image = ConvertPDF2Image(path, 0, 1, ImageFormat.Jpeg, Definition.Five);
                    fileBytes = ImageToBytes(image);
                }

                #endregion
                #region 图片

                else
                {
                    FileStream fs = new FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    //MessageBox.Show("path:" + path);
                     fileBytes = new byte[fs.Length];
                    fs.Read(fileBytes, 0, fileBytes.Length);
                    fs.Close();
                    fs.Dispose();
                    //MessageBox.Show("fileBytes:" + fileBytes.LongLength.ToString()); 
                }
               
                #endregion


                string AUZT_NO = lbl_AUZT_NO.Text.Trim();
                //服务器连接地址
                string httpPath = AppConfig.GetValue("httpPath");
                string filedValue = AUZT_NO, responseText = "";
              

               
                HttpRequestClient httpRequestClient = new HttpRequestClient();
                httpRequestClient.SetFieldValue("key", filedValue);
                httpRequestClient.SetFieldValue("uploadfile", path, "application/octet-stream", fileBytes);
                bool b = httpRequestClient.Upload(httpPath, out responseText);
                //MessageBox.Show("返回值b:" + b.ToString());
                if (b)
                {
                    //添加路径到数据库
                    Hashtable userLogin = UserLogin.GetUserLogin();
                    User user = (User)userLogin["user"];
                    string filePath = responseText.Split('<')[0].Trim();
                    bool c = sqlhelper.UpdatePrintrecord(user.NAME, filePath, ID);
                    if (c)
                    {
                        MessageBox.Show("图片上传成功");
                    }
                    else
                    {
                        MessageBox.Show("图片上传失败");
                    }
                    if (Refreshtheform != null)
                    {
                        Refreshtheform();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("图片上传失败(" + responseText + ")");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message);

            }

        }
        /// <summary>
        /// Convert Image to Byte[]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(Image image)
        {
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
               
                    image.Save(ms, ImageFormat.Jpeg);
              
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// 将PDF文档转换为图片的方法
        /// </summary>
        /// <param name="pdfInputPath">PDF文件路径</param>
        /// <param name="imageOutputPath">图片输出路径</param>
        /// <param name="imageName">生成图片的名字</param>
        /// <param name="startPageNum">从PDF文档的第几页开始转换</param>
        /// <param name="endPageNum">从PDF文档的第几页开始停止转换</param>
        /// <param name="imageFormat">设置所需图片格式</param>
        /// <param name="definition">设置图片的清晰度，数字越大越清晰</param>
        public static Bitmap ConvertPDF2Image(string pdfInputPath,
            int startPageNum, int endPageNum, ImageFormat imageFormat, Definition definition)
        {
            try
            {
                PDFFile pdfFile = PDFFile.Open(pdfInputPath);


                // validate pageNum
                if (startPageNum <= 0)
                {
                    startPageNum = 1;
                }

                if (endPageNum > pdfFile.PageCount)
                {
                    endPageNum = pdfFile.PageCount;
                }

                if (startPageNum > endPageNum)
                {
                    int tempPageNum = startPageNum;
                    startPageNum = endPageNum;
                    endPageNum = startPageNum;
                }

                // start to convert each page
                Bitmap pageImage = null;
                for (int i = startPageNum; i <= endPageNum; i++)
                {
                    pageImage = pdfFile.GetPageImage(i - 1, 56 * (int)definition);
                     //pageImage.Save(imageOutputPath + imageName + i.ToString() + "." + imageFormat.ToString(), imageFormat);
                    //pageImage.Dispose();
                }


                pdfFile.Dispose();
                return pageImage;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

    }




}

