using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace CKS.Common
{
    public enum HTMLEncodeLevel
    {
        /// <summary>
        /// 不编码，适用于网站内部数据
        /// </summary>
        None,
        /// <summary>
        /// 轻度编码，只保证json格式正确性
        /// </summary>
        Light,
        /// <summary>
        /// 中度编码，防范一般的脚本提交
        /// </summary>
        Medium,
        /// <summary>
        /// 重度编码，特殊符号全部编码
        /// </summary>
        Heavy
    }

    /// <summary>
    /// 转换JSON
    /// </summary>
    public class CS2JSON
    {
        /// <summary>
        /// 将DataTable转化成JSON  默认轻度编码
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="JsonName">JsonName</param>
        /// <returns></returns>
        public static string DataTable2Json(DataTable dt, string JsonName)
        {
            return DataTable2Json(dt, JsonName, HTMLEncodeLevel.Light);
        }

        /// <summary>
        /// 将DataTable转化成JSON
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="JsonName">JsonName</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns></returns>
        public static string DataTable2Json(DataTable dt, string JsonName, HTMLEncodeLevel encodeLevel)
        {
            //Exception Handling       
            if (dt != null && dt.Rows.Count > 0)
            {
                var JsonString = new StringBuilder();
                JsonString.Append("{");
                JsonString.Append(string.Format("\"{0}\":[", JsonName));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    JsonString.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j < dt.Columns.Count - 1)
                        {
                            JsonString.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + HtmlEncode(dt.Rows[i][j].ToString(), encodeLevel) + "\",");
                        }
                        else if (j == dt.Columns.Count - 1)
                        {
                            JsonString.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + HtmlEncode(dt.Rows[i][j].ToString(), encodeLevel) + "\"");
                        }
                    }
                    /*end Of String*/
                    if (i == dt.Rows.Count - 1)
                    {
                        JsonString.Append("}");
                    }
                    else
                    {
                        JsonString.Append("},");
                    }
                }
                JsonString.Append("]}");
                return JsonString.ToString();
            }
            return "{\"" + JsonName + "\":[]}";
        }

        /// <summary> 
        /// DataReader转换为Json 默认轻度编码
        /// </summary> 
        /// <param name="sdr">DataReader对象</param> 
        /// <param name="JsonName">JsonName</param>
        /// <returns>Json字符串</returns> 
        public static string DataReader2Json(SqlDataReader sdr, string JsonName)
        {
            return DataReader2Json(sdr, JsonName, HTMLEncodeLevel.Light);
        }

        /// <summary> 
        /// DataReader转换为Json 
        /// </summary> 
        /// <param name="sdr">DataReader对象</param> 
        /// <param name="JsonName">JsonName</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns>Json字符串</returns> 
        public static string DataReader2Json(SqlDataReader sdr, string JsonName, HTMLEncodeLevel encodeLevel)
        {
            var sb = new StringBuilder();
            if (sdr.HasRows)
            {
                sb.Append("{");
                sb.Append(string.Format("\"{0}\":[", JsonName));
                //循环结果集,拼接成Json
                while (sdr.Read())
                {
                    sb.Append("{");
                    //遍历列
                    for (int i = 0; i < sdr.FieldCount; i++)
                    {
                        //如果不是最后一列,则在后面加上,
                        if (i < sdr.FieldCount - 1)
                        {
                            //获取当前列的名称和值,拼接到JSON,值要加''
                            sb.Append(string.Format("\"{0}\":\"{1}\",", sdr.GetName(i), HtmlEncode(sdr[i].ToString(), encodeLevel)));
                        }
                        else
                        {
                            sb.Append(string.Format("\"{0}\":\"{1}\"", sdr.GetName(i), HtmlEncode(sdr[i].ToString(), encodeLevel)));
                        }
                    }
                    //单条记录结束
                    sb.Append("},");
                }
                //去掉最后一项后面的逗号
                sb.Remove(sb.Length - 1, 1);
                //结束标记
                sb.Append("]}");
            }
            return sb.ToString();
        }

        /// <summary> 
        /// DataReader转换为Json 默认轻度编码
        /// </summary> 
        /// <param name="dr">DataReader对象</param> 
        /// <param name="JsonName">JsonName</param>
        /// <returns>Json字符串</returns> 
        public static string DataReader2Json(IDataReader dr, string JsonName)
        {
            return DataReader2Json(dr, JsonName, HTMLEncodeLevel.Light);
        }

        /// <summary> 
        /// DataReader转换为Json 
        /// </summary> 
        /// <param name="dr">DataReader对象</param> 
        /// <param name="JsonName">JsonName</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns>Json字符串</returns> 
        public static string DataReader2Json(IDataReader dr, string JsonName, HTMLEncodeLevel encodeLevel)
        {
            bool hasData = false;
            var sb = new StringBuilder();

            if (dr.FieldCount > 0)
            {
                sb.Append("{");
                sb.Append(string.Format("\"{0}\":[", JsonName));
                //循环结果集,拼接成Json
                while (dr.Read())
                {
                    hasData = true;
                    sb.Append("{");
                    //遍历列
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        //如果不是最后一列,则在后面加上","
                        if (i < dr.FieldCount - 1)
                        {
                            //获取当前列的名称和值,拼接到JSON,值要加"''"
                            sb.Append(string.Format("\"{0}\":\"{1}\",", dr.GetName(i),
                                                    HtmlEncode(dr[i].ToString(), encodeLevel)));
                        }
                        else
                        {
                            sb.Append(string.Format("\"{0}\":\"{1}\"", dr.GetName(i),
                                                    HtmlEncode(dr[i].ToString(), encodeLevel)));
                        }
                    }
                    //单条记录结束
                    sb.Append("},");
                }
                //去年最后一项后面的逗号
                sb.Remove(sb.Length - 1, 1);
                //结束标记
                sb.Append("]}");
            }

            return hasData ? sb.ToString() : "{\"" + JsonName + "\":[]}";
        }



        /// <summary>
        /// 数组转换为json格式 默认轻度编码
        /// </summary>
        /// <param name="arr">数组对象</param>
        /// <returns></returns>
        public static string Array2Json(string[] arr)
        {
            return Array2Json(arr, HTMLEncodeLevel.Light);
        }

        /// <summary>
        /// 数组转换为json格式
        /// </summary>
        /// <param name="arr">数组对象</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns></returns>
        public static string Array2Json(string[] arr, HTMLEncodeLevel encodeLevel)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < arr.Length; i++)
            {
                sb.Append("{");
                if (i == arr.Length - 1)
                {
                    sb.Append(string.Format("\"{0}\":\"{1}\"", "n", HtmlEncode(arr[i], encodeLevel))).Append("}");
                }
                else
                {
                    sb.Append(string.Format("\"{0}\":\"{1}\"", "n", HtmlEncode(arr[i], encodeLevel))).Append("},");
                }
            }
            sb.Append("]");

            return sb.ToString();
        }

        /// <summary> 
        /// 追加Json属性 默认轻度编码
        /// </summary> 
        /// <param name="Json"></param>
        /// <param name="diction">要追加的键值对</param>
        /// <returns>Json字符串</returns> 
        public static string Append2Json(string Json, Dictionary<string, string> diction)
        {
            return Append2Json(Json, diction, HTMLEncodeLevel.Light);
        }

        /// <summary> 
        /// 追加Json属性
        /// </summary> 
        /// <param name="Json"></param>
        /// <param name="diction">要追加的键值对</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns>Json字符串</returns> 
        public static string Append2Json(string Json, Dictionary<string, string> diction, HTMLEncodeLevel encodeLevel)
        {
            var sb = new StringBuilder(Json);
            sb.Replace('}', ',', sb.Length - 1, 1);

            //加上其他键值
            foreach (var i in diction)
            {
                sb.Append(string.Format("\"{0}\":\"{1}\",", i.Key, HtmlEncode(i.Value, encodeLevel)));
            }
            //去年最后一项后面的逗号
            sb.Remove(sb.Length - 1, 1);

            //结束标记
            sb.Append("}");
            return sb.ToString();
        }


        /// <summary> 
        /// Dictionary转换为Json(单对象) {"dic.key1":"dic.value1", "dic.key2":"dic.value2"}
        /// </summary> 
        /// <param name="style"></param>
        /// <param name="dic">要追加的键值对</param>
        /// <returns>Json字符串</returns> 
        public static string Dic2Json(Dic2JsonStyle style, Dictionary<string, string> dic)
        {
            if (style == Dic2JsonStyle.SingleItem)
                return Dic2JsonSingleItem(dic, HTMLEncodeLevel.Light);

            return Dic2JsonMultiterm(dic, HTMLEncodeLevel.Light);
        }
        public enum Dic2JsonStyle
        {
            SingleItem, Multiterm
        }

        public static string Dic2Json(Dictionary<string, string> dic)
        {
            return Dic2JsonMultiterm(dic, HTMLEncodeLevel.Light);
        }

        /// <summary> 
        /// Dictionary转换为Json(多对象) [{"key":"dic.key"}, {"value":"dic.value"}]
        /// </summary> 
        /// <param name="dic">要追加的键值对</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns>Json字符串</returns> 
        private static string Dic2JsonSingleItem(Dictionary<string, string> dic, HTMLEncodeLevel encodeLevel)
        {
            bool hasData = false;
            var sb = new StringBuilder();

            if (dic.Count > 0)
            {
                hasData = true;
                sb.Append("{");
                //循环结果集,拼接成Json
                foreach (var kv in dic)
                {
                    //遍历列
                    //获取当前列的名称和值,拼接到JSON,值要加"''"
                    sb.Append(string.Format("\"{0}\":\"{1}\",", kv.Key, HtmlEncode(kv.Value, encodeLevel)));
                }
                //去年最后一项后面的逗号
                sb.Remove(sb.Length - 1, 1);

                //结束标记
                sb.Append("}");
            }

            return hasData ? sb.ToString() : "{}";
        }
        /// <summary> 
        /// Dictionary转换为Json 
        /// </summary> 
        /// <param name="dic">要追加的键值对</param>
        /// <param name="encodeLevel">编码等级</param>
        /// <returns>Json字符串</returns> 
        private static string Dic2JsonMultiterm(Dictionary<string, string> dic, HTMLEncodeLevel encodeLevel)
        {
            bool hasData = false;
            var sb = new StringBuilder();

            if (dic.Count > 0)
            {
                hasData = true;
                sb.Append("[");
                //循环结果集,拼接成Json
                foreach (var kv in dic)
                {
                    //遍历列
                    //获取当前列的名称和值,拼接到JSON,值要加"''"
                    sb.Append("{\"key\":\"" + kv.Key + "\",\"value\":\"" + HtmlEncode(kv.Value, encodeLevel) + "\"},");
                }
                //去年最后一项后面的逗号
                sb.Remove(sb.Length - 1, 1);

                //结束标记
                sb.Append("]");
            }

            return hasData ? sb.ToString() : "{}";
        }

        /// <summary> 
        /// DataSet转换为Json 
        /// </summary> 
        /// <param name="dataSet">DataSet对象</param> 
        /// <returns>Json字符串</returns> 
        public static string DataSet2Json(DataSet dataSet)
        {
            string jsonString = "{";
            foreach (DataTable table in dataSet.Tables)
            {
                jsonString += "\"" + table.TableName + "\":" + DataTable2Json(table, table.TableName) + ",";
            }
            jsonString = jsonString.TrimEnd(',');
            return jsonString + "}";
        }

        /// <summary>
        /// 编码过滤特殊字符
        /// </summary>
        /// <param name="str">要编码的字符串</param>
        /// <param name="level">编码等级</param>
        /// <returns></returns>
        public static string HtmlEncode(string str, HTMLEncodeLevel level)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            switch (level)
            {
                case HTMLEncodeLevel.None:
                    return str;
                case HTMLEncodeLevel.Light:
                    return HtmlEncode_Light(str);
                case HTMLEncodeLevel.Medium:
                    return HtmlEncode_Medium(str);
                //case HTMLEncodeLevel.Heavy:
                //    return HtmlEncode_Heavy(str);
            }
            return str;
        }

        /// <summary>
        /// 轻度编码 过滤特殊字符
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string HtmlEncode_Light(string s)
        {
            StringBuilder result = new StringBuilder(s);
            //result.Replace("&", "&amp;");
            result.Replace("\"", "&quot;");
            result.Replace("\\", "&#92;");
            result.Replace("’", "&#39;");
            //result.Replace("  ", "&nbsp;&nbsp;");
            return result.ToString();

            //var result = new StringBuilder();
            //for (int i = 0; i < s.Length; i++)
            //{
            //    char c = s.ToCharArray()[i];
            //    switch (c)
            //    {
            //        case '\"':
            //            result.Append("\\\""); break;
            //        case '\\':
            //            result.Append("\\\\"); break;
            //        case '/':
            //            result.Append("\\/"); break;
            //        case '\b':
            //            result.Append("\\b"); break;
            //        case '\f':
            //            result.Append("\\f"); break;
            //        case '\n':
            //            result.Append("\\n"); break;
            //        case '\r':
            //            result.Append("\\r"); break;
            //        case '\t':
            //            result.Append("\\t"); break;
            //        default:
            //            result.Append(c); break;
            //    }
            //}
            //return result.ToString();
        }

        /// <summary>
        /// 中度度编码 过滤特殊字符
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string HtmlEncode_Medium(string s)
        {
            StringBuilder result = new StringBuilder(s);
            result.Replace("&", "&amp;");
            result.Replace("\"", "&quot;");
            result.Replace("<", "&lt;");
            result.Replace(">", "&gt;");
            result.Replace(" ", "&nbsp;");
            result.Replace("’", "&#39;");
            result.Replace(((char)13).ToString(), "<br />");
            result.Replace("\r\n", "<br />");
            result.Replace("\r", "<br />");
            result.Replace("\n", "<br />");
            result.Replace("\\", "&#92;");
            return result.ToString();
        }

        ///// <summary>
        ///// 重度编码 过滤特殊字符
        ///// </summary>
        ///// <param name="s"></param>
        ///// <returns></returns>
        //private static string HtmlEncode_Heavy(string s)
        //{
        //    return AntiXss.HtmlEncode(s);
        //}

        /// <summary>
        /// 格式化字符型、日期型、布尔型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string StringFormat(string str, Type type)
        {
            if (type == typeof(string))
            {
                str = HtmlEncode_Light(str);
                str = "\"" + str + "\"";
            }
            else if (type == typeof(DateTime))
            {
                str = "\"" + str + "\"";
            }
            else if (type == typeof(bool))
            {
                str = str.ToLower();
            }
            return str;
        }

        /// <summary>
        /// 将json转换为DataTable
        /// </summary>
        /// <param name="strJson">得到的json</param>
        /// <returns></returns>
        public static DataTable JsonToDataTable(string strJson)
        {
            try
            {
                //转换json格式
                strJson = strJson.Replace(",\"", "^^\"").Replace("\":", "\"&&").ToString();
                //取出表名   
                var rg = new Regex(@"(?<={)[^:]+(?=:\[)", RegexOptions.IgnoreCase);
                //string strName = rg.Match(strJson).Value;
                DataTable tb = null;
                //去除表名   
                strJson = strJson.Substring(strJson.IndexOf("[") + 1);
                strJson = strJson.Substring(0, strJson.IndexOf("]"));

                //获取数据   
                rg = new Regex(@"(?<={)[^}]+(?=})");
                MatchCollection mc = rg.Matches(strJson);
                for (int i = 0; i < mc.Count; i++)
                {
                    string strRow = mc[i].Value;

                    string[] strRows  = Regex.Split(strRow, "\\^\\^");

                    //创建表   
                    if (tb == null)
                    {
                        tb = new DataTable();
                        //tb.TableName = strName;
                        foreach (string str in strRows)
                        {
                            var dc = new DataColumn();
                            string[] strCell = Regex.Split(str, "\\&\\&");
                            if (strCell[0].Substring(0, 1) == "\"")
                            {
                                int a = strCell[0].Length;
                                dc.ColumnName = strCell[0].Substring(1, a - 2);
                            }
                            else
                            {
                                dc.ColumnName = strCell[0];
                            }
                            tb.Columns.Add(dc);
                        }
                        tb.AcceptChanges();
                    }

                    //增加内容   
                    DataRow dr = tb.NewRow();
                    for (int r = 0; r < strRows.Length; r++)
                    {
                        dr[r] = Regex.Split(strRows[r], "\\&\\&")[1].Trim().Replace("，", ",").Replace("：", ":").Replace("\"", "");
                    }
                    tb.Rows.Add(dr);
                    tb.AcceptChanges();
                }

                return tb;
            }
            catch (Exception)
            {

                return null;
            }
          
        }
    }
}
