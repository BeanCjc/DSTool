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
using System.Data.SqlClient;

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

        public static string MD5Encrypt(string str,int bit)
        {
            var md5 = MD5.Create();
            var byteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (var item in byteArray)
            {
                sb.Append(item.ToString("x2"));
            }
            if (bit==16)
            {
                return sb.ToString().Substring(8, 16);
            }
            return sb.ToString();
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
                var preMD5Str = front + apipwd;
                MD5 mD5 = MD5.Create();
                var sign = MD5Encrypt(preMD5Str, 32);
                var requestData = $"front={front}&sign={sign}";
                if (!string.IsNullOrEmpty(paramData))
                {
                    requestData += $"&{paramData.ToLower()}";
                }
                var body = Encoding.UTF8.GetBytes(requestData);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Accept = "application/json";//只接收json的返回
                //request.Timeout = 5000;//超时时间5s
                request.ContentLength = body.Length;
                request.GetRequestStream().Write(body, 0, body.Length);
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

    }
}
