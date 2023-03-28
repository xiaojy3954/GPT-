using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AUZT_Authorization
{
    public class AppConfig
    {
        public static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);  
  
        /// <summary>  
        /// 获取配置值  
        /// </summary>  
        /// <param name="key">配置标识</param>  
        /// <returns></returns>  
        public static string GetValue(string key)  
        {  
            string result = string.Empty;  
            if (config.AppSettings.Settings[key] != null)  
                result = config.AppSettings.Settings[key].Value;  
            return result;  
        }  
  
        /// <summary>  
        /// 修改或增加配置值  
        /// </summary>  
        /// <param name="key">配置标识</param>  
        /// <param name="value">配置值</param>  
        public static void SetValue(string AppKey, string AppValue)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            xDoc.Load(System.Windows.Forms.Application.ExecutablePath + ".config");

            System.Xml.XmlNode xNode;
            System.Xml.XmlElement xElem1;
            System.Xml.XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");

            xElem1 = (System.Xml.XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null) xElem1.SetAttribute("value", AppValue);
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(System.Windows.Forms.Application.ExecutablePath + ".config");
        }
  
        /// <summary>  
        /// 删除配置值  
        /// </summary>  
        /// <param name="key">配置标识</param>  
        public static void DeleteValue(string key)  
        {  
            config.AppSettings.Settings.Remove(key);  
        }  
    }
}
