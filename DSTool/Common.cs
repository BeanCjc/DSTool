using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using DSTool.RequestEntity;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography;

namespace DSTool
{
    class Common
    {
        public static string GetAppConfig(string strKey)
        {
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = ConfigurationManager.OpenExeConfiguration(file);
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == strKey)
                {
                    return config.AppSettings.Settings[strKey].Value.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// POST请求,用于add和edit
        /// </summary>
        /// <param name="ulr">post请求URL</param>
        /// <param name="paramData">请求参数</param>
        /// <returns>返回请求的返回或者异常信息</returns>
        /// <exception cref="不抛异常,若存在异常信息,将体现在返回结果中"></exception>
        public static ResponseResult Post(string ulr, string paramData)
        {
            ResponseResult responseResult = null;
            if (string.IsNullOrEmpty(ulr))
            {
                return new ResponseResult() { Success = false, Msg = "url为空", Data = null };
            }
            if (!ulr.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
            {
                return new ResponseResult() { Success = false,  Msg = "url格式不正确", Data = null };
            }
            try
            {
                var request = WebRequest.CreateHttp(ulr);
                var front = GetTimeStamps();
                var apipwd = "@acewill";
                MD5 mD5 = MD5.Create();
                var sign = Encoding.UTF8.GetString(mD5.ComputeHash(Encoding.UTF8.GetBytes(front + apipwd)));
                var requestData = $"front:{front}&sign:{sign}";
                if (!string.IsNullOrEmpty(paramData))
                {
                    requestData += $"&{paramData}";
                }
                var body = Encoding.UTF8.GetBytes(requestData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Accept = "application/json";//只接收json的返回
                //request.Timeout = 5000;//超时时间5s
                request.ContentLength = body.Length;
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    var responseJson = sr.ReadToEnd();
                    responseResult = JsonConvert.DeserializeObject<ResponseResult>(responseJson);
                    if (responseResult == null)
                    {
                        return new ResponseResult() { Success = false, Msg = "返回空对象", Data = null };
                    }
                }
                response.Close();//关闭连接
                return responseResult;
            }
            catch (Exception ex)
            {
                return new ResponseResult() { Success = false, Msg = ex.Message, Data = null };
            }
        }

        /// <summary>
        /// 获取当前时间的时间戳(13位)
        /// </summary>
        /// <returns>当前时间戳</returns>
        public static string GetTimeStamps()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)).TotalMilliseconds).ToString();
        }

        #region +ExcuteNonQueryTransaction 增、删、改同步操作
        /// <summary>
        /// 增、删、改同步操作
        ///  </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="connection">链接字符串</param>
        /// <param name="cmd">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="flag">true存储过程，false sql语句</param>
        /// <returns>int</returns>
        public static int ExcuteNonQueryTransaction<T>(string connection, string cmd, DynamicParameters param, bool flag = true) where T : class, new()
        {
            int result = 0;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                if (flag)
                {
                    result = con.Execute(cmd, param, null, null, CommandType.StoredProcedure);
                }
                else
                {
                    result = con.Execute(cmd, param, null, null, CommandType.Text);
                }
            }
            return result;
        }
        #endregion

        #region +ExcuteNonQueryAsync 增、删、改异步操作
        /// <summary>
        /// 增、删、改异步操作
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="connection">链接字符串</param>
        /// <param name="cmd">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="flag">true存储过程，false sql语句</param>
        /// <returns>int</returns>
        public static async Task<int> ExcuteNonQueryAsync<T>(string connection, string cmd, DynamicParameters param, bool flag = true) where T : class, new()
        {
            int result = 0;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                if (flag)
                {
                    result = await con.ExecuteAsync(cmd, param, null, null, CommandType.StoredProcedure);
                }
                else
                {
                    result = await con.ExecuteAsync(cmd, param, null, null, CommandType.Text);
                }
            }
            return result;
        }
        #endregion

        #region +ExecuteScalar 同步查询操作
        /// <summary>
        /// 同步查询操作
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="connection">连接字符串</param>
        /// <param name="cmd">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="flag">true存储过程，false sql语句</param>
        /// <returns>object</returns>
        public static object ExecuteScalar<T>(string connection, string cmd, DynamicParameters param, bool flag = true) where T : class, new()
        {
            object result = null;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                if (flag)
                {
                    result = con.ExecuteScalar(cmd, param, null, null, CommandType.StoredProcedure);
                }
                else
                {
                    result = con.ExecuteScalar(cmd, param, null, null, CommandType.Text);
                }
            }
            return result;
        }
        #endregion

        #region +ExecuteScalarAsync 异步查询操作
        /// <summary>
        /// 异步查询操作
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="connection">连接字符串</param>
        /// <param name="cmd">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="flag">true存储过程，false sql语句</param>
        /// <returns>object</returns>
        public static async Task<object> ExecuteScalarAsync<T>(string connection, string cmd, DynamicParameters param, bool flag = true) where T : class, new()
        {
            object result = null;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                if (flag)
                {
                    result = await con.ExecuteScalarAsync(cmd, param, null, null, CommandType.StoredProcedure);
                }
                else
                {
                    result = con.ExecuteScalarAsync(cmd, param, null, null, CommandType.Text);
                }
            }
            return result;
        }
        #endregion

        #region +FindToList  同步查询数据集合
        /// <summary>
        /// 同步查询数据集合
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="connection">连接字符串</param>
        /// <param name="cmd">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="flag">true存储过程，false sql语句</param>
        /// <returns>t</returns>
        public static IList<T> FindToList<T>(string connection, string cmd, DynamicParameters param, bool flag = true) where T : class, new()
        {
            IDataReader dataReader = null;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                if (flag)
                {
                    dataReader = con.ExecuteReader(cmd, param, null, null, CommandType.StoredProcedure);
                }
                else
                {
                    dataReader = con.ExecuteReader(cmd, param, null, null, CommandType.Text);
                }
                if (dataReader == null) return null;
                Type type = typeof(T);
                List<T> tlist = new List<T>();
                while (dataReader.Read())
                {
                    T t = new T();
                    foreach (var item in type.GetProperties())
                    {
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            //属性名与查询出来的列名比较
                            if (item.Name.ToLower() != dataReader.GetName(i).ToLower()) continue;
                            var kvalue = dataReader[item.Name];
                            if (kvalue == DBNull.Value) continue;
                            item.SetValue(t, kvalue, null);
                            break;
                        }
                    }
                    if (tlist != null) tlist.Add(t);
                }
                return tlist;
            }
        }
        #endregion

        #region +FindToListAsync  异步查询数据集合
        /// <summary>
        /// 异步查询数据集合
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="connection">连接字符串</param>
        /// <param name="cmd">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="flag">true存储过程，false sql语句</param>
        /// <returns>t</returns>
        public static async Task<IList<T>> FindToListAsync<T>(string connection, string cmd, DynamicParameters param, bool flag = true) where T : class, new()
        {
            IDataReader dataReader = null;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                if (flag)
                {
                    dataReader = await con.ExecuteReaderAsync(cmd, param, null, null, CommandType.StoredProcedure);
                }
                else
                {
                    dataReader = await con.ExecuteReaderAsync(cmd, param, null, null, CommandType.Text);
                }
                if (dataReader == null) return null;
                Type type = typeof(T);
                List<T> tlist = new List<T>();
                while (dataReader.Read())
                {
                    T t = new T();
                    foreach (var item in type.GetProperties())
                    {
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            //属性名与查询出来的列名比较
                            if (item.Name.ToLower() != dataReader.GetName(i).ToLower()) continue;
                            var kvalue = dataReader[item.Name];
                            if (kvalue == DBNull.Value) continue;
                            item.SetValue(t, kvalue, null);
                            break;
                        }
                    }
                    if (tlist != null) tlist.Add(t);
                }
                return tlist;
            }
        }
        #endregion
    }
}
