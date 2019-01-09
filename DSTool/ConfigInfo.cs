using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DSTool
{
    public class ConfigInfo
    {
        private static ConfigInfo Instance;
        private ConfigInfo()
        {

        }
        public static ConfigInfo CreateInstance
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new ConfigInfo();
                    Instance.Refresh(Instance);
                }
                return Instance.Refresh(Instance);
            }
        }

        private ConfigInfo Refresh(ConfigInfo configInfo)
        {
            configInfo.Apiurl_addbrand = Common.GetAppConfig("apiurl_addbrand")?.ToString();
            configInfo.Apiurl_addarea = Common.GetAppConfig("apiurl_addarea")?.ToString();
            configInfo.Apiurl_addmealtime = Common.GetAppConfig("apiurl_addmealtime")?.ToString();
            configInfo.Apiurl_adddept = Common.GetAppConfig("apiurl_adddept")?.ToString();
            configInfo.Apiurl_addstore = Common.GetAppConfig("apiurl_addstore")?.ToString();
            configInfo.Apiurl_addunit = Common.GetAppConfig("apiurl_addunit")?.ToString();
            configInfo.Apiurl_adddishtype = Common.GetAppConfig("apiurl_adddishtype")?.ToString();
            configInfo.Apiurl_adddish = Common.GetAppConfig("apiurl_adddish")?.ToString();
            configInfo.Apiurl_addpayway = Common.GetAppConfig("apiurl_addpayway")?.ToString();
            configInfo.Apiurl_adduser = Common.GetAppConfig("apiurl_adduser")?.ToString();
            configInfo.Apiurl_addordermain = Common.GetAppConfig("apiurl_addordermain")?.ToString();
            configInfo.Apiurl_addorderdetail = Common.GetAppConfig("apiurl_addorderdetail")?.ToString();

            configInfo.Brand_table = Common.GetAppConfig("brand_table")?.ToString();
            configInfo.Brand_name = Common.GetAppConfig("brand_name")?.ToString();
            configInfo.Brand_status = Common.GetAppConfig("brand_status")?.ToString();
            configInfo.Brand_subject = Common.GetAppConfig("brand_subject")?.ToString();
            configInfo.Brand_posid = Common.GetAppConfig("brand_posid")?.ToString();

            configInfo.Area_table = Common.GetAppConfig("area_table")?.ToString();
            configInfo.Area_name = Common.GetAppConfig("area_name")?.ToString();
            configInfo.Area_level = Common.GetAppConfig("area_level")?.ToString();
            configInfo.Area_status = Common.GetAppConfig("area_status")?.ToString();
            configInfo.Area_subject = Common.GetAppConfig("area_subject")?.ToString();
            configInfo.Area_posid = Common.GetAppConfig("area_posid")?.ToString();

            configInfo.Dept_table = Common.GetAppConfig("dept_table")?.ToString();
            configInfo.Dept_name = Common.GetAppConfig("dept_name")?.ToString();
            configInfo.Dept_alias = Common.GetAppConfig("dept_alias")?.ToString();
            configInfo.Dept_status = Common.GetAppConfig("dept_status")?.ToString();
            configInfo.Dept_sequence = Common.GetAppConfig("dept_sequence")?.ToString();
            configInfo.Dept_subject = Common.GetAppConfig("dept_subject")?.ToString();
            configInfo.Dept_brand = Common.GetAppConfig("dept_brand")?.ToString();
            configInfo.Dept_posid = Common.GetAppConfig("dept_posid")?.ToString();

            configInfo.Mysql_connectionstring = ConfigurationManager.ConnectionStrings["mysql_connectionstring"]?.ToString();
            configInfo.Sqlserver_connectionstring = ConfigurationManager.ConnectionStrings["sqlserver_connectionstring"]?.ToString();
            return configInfo;
        }
        public string Apiurl_addbrand { get; set; }
        public string Apiurl_addarea { get; set; }
        public string Apiurl_addmealtime { get; set; }
        public string Apiurl_adddept { get; set; }
        public string Apiurl_addstore { get; set; }
        public string Apiurl_addunit { get; set; }
        public string Apiurl_adddishtype { get; set; }
        public string Apiurl_adddish { get; set; }
        public string Apiurl_addpayway { get; set; }
        public string Apiurl_adduser { get; set; }
        public string Apiurl_addordermain { get; set; }
        public string Apiurl_addorderdetail { get; set; }

        public string Apiurl_editbrand { get; set; }
        public string Apiurl_editarea { get; set; }
        public string Apiurl_editmealtime { get; set; }
        public string Apiurl_editdept { get; set; }
        public string Apiurl_editstore { get; set; }
        public string Apiurl_editunit { get; set; }
        public string Apiurl_editdishtype { get; set; }
        public string Apiurl_editdish { get; set; }
        public string Apiurl_editpayway { get; set; }
        public string Apiurl_edituser { get; set; }
        public string Apiurl_editordermain { get; set; }
        public string Apiurl_editorderdetail { get; set; }

        public string Brand_table { get; set; }
        public string Brand_name { get; set; }
        public string Brand_status { get; set; }
        public string Brand_subject { get; set; }
        public string Brand_posid { get; set; }

        public string Area_table { get; set; }
        public string Area_name { get; set; }
        public string Area_level { get; set; }
        public string Area_status { get; set; }
        public string Area_subject { get; set; }
        public string Area_posid { get; set; }

        public string Dept_table { get; set; }
        public string Dept_name { get; set; }
        public string Dept_alias { get; set; }
        public string Dept_status { get; set; }
        public string Dept_sequence { get; set; }
        public string Dept_subject { get; set; }
        public string Dept_brand { get; set; }
        public string Dept_posid { get; set; }

        public string Mysql_connectionstring { get; set; }
        public string Sqlserver_connectionstring { get; set; }
    }
}
