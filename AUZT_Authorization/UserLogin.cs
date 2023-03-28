using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AUZT_Authorization
{
   public static class UserLogin
    {
        //哈希表，作为保存登录用户的队列
        private static Hashtable m_userList;

        //用户在线超时的时限(60分钟)
        private static TimeSpan m_tsSub = new TimeSpan(0, 60, 0);

        public static void Init()
        {
            m_userList = Hashtable.Synchronized(new Hashtable());
        }
        public static void Empty()
        {
            m_userList.Clear();
        }
        /// <summary>
        /// 登录信息在数据库中验证成功后调用
        /// </summary>
      
        /// <returns></returns>
        public static int AddUserToList(string keyName, int AUTOID, string USERID, string NAME, string DEPT, string STATUS)
        {
            int ret = 0;
            try
            {
                User hashElt = new User();
                hashElt.AUTOID = AUTOID;
                hashElt.USERID = USERID;
                hashElt.NAME = NAME;
                hashElt.DEPT = DEPT;
                hashElt.STATUS = STATUS;

                //如果用户已经登录过，则只更新登录流水号及操作时间，否则新加入队列
                lock (m_userList.SyncRoot)
                {
                    if (m_userList.ContainsKey(keyName))
                    {
                        m_userList[keyName] = hashElt;
                    }
                    else
                    {
                        m_userList.Add(keyName, hashElt);
                    }
                }
            }
            catch (Exception)
            {
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 获取登陆用户
        /// </summary>
        /// <returns></returns>
        public static Hashtable GetUserLogin()
        {
            lock (m_userList.SyncRoot)
            {
                lock (m_userList.SyncRoot)
                {
                    return m_userList;
                }
            }
        }
        /// <summary>
        ///验证用户是否登陆        
        /// </summary>
        /// <param name="userName">用户名</param>       
        /// <returns>
        /// 0:验证用户登录成功
        /// -1:验证用户登录失败
        /// -2:用户未登录
        /// -3:用户已重新登录或在别处登录
        /// </returns>
        public static int CheckUserLogin(string USERID)
        {
            int ret = 0;
            try
            {
                lock (m_userList.SyncRoot)
                {
                    if (m_userList.ContainsKey(USERID))
                    {
                        User hashElt = m_userList[USERID] as User;
                        if (hashElt.USERID.Equals(USERID))
                        {
                         
                            m_userList[USERID] = hashElt;
                            ret = 0;
                        }
                        else
                        {
                            ret = -3;
                        }
                    }
                    else
                    {
                        ret = -2;
                    }
                }
            }
            catch
            {

                ret = -1;
            }
            return ret;
        }
      
    }
}
