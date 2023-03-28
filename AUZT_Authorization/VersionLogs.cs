using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AUZT_Authorization
{
   public class VersionLogs
    {
        /// <summary>
        /// 版本
        /// </summary>
       public string Edition { set; get; }

        /// <summary>
        /// 更新时间
        /// </summary>
       public DateTime UpdateTime { set; get; }

        /// <summary>
        /// 更新内容
        /// </summary>
       public List<string> UpdateContent { set; get; }

    }
}
