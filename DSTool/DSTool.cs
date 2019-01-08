using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DSTool.DbData;
using System.Configuration;
using MySql.Data.MySqlClient;
using Dapper;
using System.IO;
using DSTool.RequestEntity;
using System.Net.Http;
using System.Net;

namespace DSTool
{
    public partial class MSTool : Form
    {
        public MSTool()
        {
            InitializeComponent();
        }
        static ConfigInfo ConfigInfo = ConfigInfo.CreateInstance;
        static string connectionString = ConfigInfo.Mysql_connectionstring;
        static string default_tablename_brand = Common.GetAppConfig("brand_table")?.ToString();
        static string default_tablename_area = Common.GetAppConfig("area_table")?.ToString();
        static string default_tablename_dept = Common.GetAppConfig("dept_table")?.ToString();

        private void MSTool_Load(object sender, EventArgs e)
        {
            var fileName = Directory.GetCurrentDirectory();
            var watcher = new FileSystemWatcher(fileName, "DSTool.exe.config")
            {
                EnableRaisingEvents = true

            };
            watcher.Changed += (obj, fileSystemArgs) =>
            {
                ConfigInfo = ConfigInfo.CreateInstance;

            };//监听配置文件

            //1.检测是否可以访问智慧POS所在服务器和数据库
            //2.页面初始化需要创建三张表(品牌表,区域表,部门表),并填充默认数据到智慧POS数据库
            //3.根据配置文件获取对应的API接口，若对应的URL为空则将对应的按钮置灰，三张表的数据存在则置灰同步品牌、区域、部门三个按钮

            #region 第一步 访问数据库
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("请配置数据库连接字符串", "ErrorInfo");
                return;
            }
            else
            {
                try
                {
                    using (var db = new MySqlConnection(connectionString))
                    {
                        var testConnection = db.ExecuteScalar("select 1")?.ToString();
                        if (testConnection == null)
                        {
                            throw new Exception("连接数据库失败！");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误信息" + ex.Message, "ErrorInfo");
                    return;
                }
            }
            #endregion

            #region sql语句
            var selectBrandSql = @"select table_name from information_schema.tables where /* table_schema='wiseposdb' and */ TABLE_NAME=?tablename";
            var createBrandSql = @"CREATE TABLE `brand_default` (
                                                              `brand_name` varchar(255) DEFAULT NULL COMMENT '品牌名称',
                                                              `brand_status` varchar(255) DEFAULT NULL COMMENT '状态 1:有效 0:无效',
                                                              `brand_subject` varchar(255) DEFAULT NULL COMMENT '编码',
                                                              `brand_posid` varchar(255) DEFAULT NULL COMMENT 'pos ID'
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            var paramBrand = new DynamicParameters();
            paramBrand.Add("?tablename", ConfigInfo.Brand_table);


            var selectAreaSql = @"select table_name from information_schema.tables where table_schema='wiseposdb' and TABLE_NAME=?tablename";
            var createAreaSql = @"CREATE TABLE `area_default` (
                                                              `area_name` varchar(255) DEFAULT NULL COMMENT '区域名称',
                                                              `area_level` varchar(255) DEFAULT NULL COMMENT '区域层级',
                                                              `area_status` varchar(255) DEFAULT NULL COMMENT '状态 1:有效 0:无效',
                                                              `area_subject` varchar(255) DEFAULT NULL COMMENT '编码',
                                                              `area_posid` varchar(255) DEFAULT NULL COMMENT 'pos id'
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            var paramArea = new DynamicParameters();
            paramArea.Add("?tablename", ConfigInfo.Area_table);

            var selectDeptSql = @"select table_name from information_schema.tables where table_schema='wiseposdb' and TABLE_NAME=?tablename";
            var createDeptSql = @"CREATE TABLE `dept_default` (
                                                              `dept_name` varchar(255) DEFAULT NULL COMMENT '部门名称',
                                                              `dept_alias` varchar(255) DEFAULT NULL COMMENT '部门别名',
                                                              `dept_status` varchar(255) DEFAULT NULL COMMENT '状态 1:有效 0:无效',
                                                              `dept_sequence` varchar(255) DEFAULT NULL COMMENT '部门顺序',
                                                              `dept_subject` varchar(255) DEFAULT NULL COMMENT '编码',
                                                              `dept_brand` varchar(255) DEFAULT NULL COMMENT '品牌',
                                                              `dept_posid` varchar(255) DEFAULT NULL COMMENT 'pos id'
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            var paramDept = new DynamicParameters();
            paramDept.Add("?tablename", ConfigInfo.Dept_table);
            #endregion

            //建表 
            using (var db = new MySqlConnection(connectionString))
            {
                var brand = db.ExecuteScalar(selectBrandSql, paramBrand)?.ToString();
                if (brand == null)
                {
                    var result = db.Execute(createBrandSql, null, null, null, CommandType.Text);
                }
                var area = db.ExecuteScalar(selectAreaSql, paramArea)?.ToString();
                if (area == null)
                {
                    db.Execute(createAreaSql, null, null, null, CommandType.Text);
                }
                var dept = db.ExecuteScalar(selectDeptSql, paramDept)?.ToString();
                if (dept == null)
                {
                    db.Execute(createDeptSql, null, null, null, CommandType.Text);
                }
            }

            SetButtonEnable();
            //初始化数据
            InitData();
        }

        /// <summary>
        /// 设置是否启用按钮
        /// </summary>
        void SetButtonEnable()
        {
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addbrand))
            {
                btn_Brand.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addarea))
            {
                btn_Area.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addmealtime))
            {
                btn_MealTime.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_adddept))
            {
                btn_Dept.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addstore))
            {
                btn_Store.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addunit))
            {
                btn_Unit.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_adddishtype))
            {
                btn_DishType.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_adddish))
            {
                btn_Dish.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addpayway))
            {
                btn_Payway.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_adduser))
            {
                btn_User.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addordermain))
            {
                btn_OrderMain.Enabled = false;
            }
            if (string.IsNullOrEmpty(ConfigInfo.Apiurl_addorderdetail))
            {
                btn_OrderDetail.Enabled = false;
            }
        }


        void InitData()
        {
            var brand_name = ConfigInfo.Brand_name;
            var brand_status = ConfigInfo.Brand_status;
            var brand_subject = ConfigInfo.Brand_subject;
            var brand_posid = ConfigInfo.Brand_posid;

            var area_name = ConfigInfo.Area_name;
            var area_level = ConfigInfo.Area_level;
            var area_status = ConfigInfo.Area_status;
            var area_subject = ConfigInfo.Area_subject;
            var area_posid = ConfigInfo.Area_posid;

            var dept_name = ConfigInfo.Dept_name;
            var dept_alias = ConfigInfo.Dept_alias;
            var dept_status = ConfigInfo.Dept_status;
            var dept_sequence = ConfigInfo.Dept_sequence;
            var dept_subject = ConfigInfo.Dept_subject;
            var dept_brand = ConfigInfo.Dept_brand;
            var dept_posid = ConfigInfo.Dept_posid;

            bool brandFlag = false, areaFlag = false, deptFlag = false;
            if (!string.IsNullOrEmpty(brand_name) && !string.IsNullOrEmpty(brand_status) && !string.IsNullOrEmpty(brand_subject) && !string.IsNullOrEmpty(brand_posid))
            {
                brandFlag = true;
            }
            if (!string.IsNullOrEmpty(area_name) && !string.IsNullOrEmpty(area_level) && !string.IsNullOrEmpty(area_status) && !string.IsNullOrEmpty(area_subject) && !string.IsNullOrEmpty(area_posid))
            {
                areaFlag = true;
            }
            if (!string.IsNullOrEmpty(dept_name) && !string.IsNullOrEmpty(dept_alias) && !string.IsNullOrEmpty(dept_status) && !string.IsNullOrEmpty(dept_sequence) && !string.IsNullOrEmpty(dept_subject) && !string.IsNullOrEmpty(dept_brand) && !string.IsNullOrEmpty(dept_posid))
            {
                deptFlag = true;
            }
            if (brandFlag || areaFlag || deptFlag)
            {
                var selectBrandSql = @"select brand_name,brand_status,brand_subject,brand_posid from brand_default where brand_name=?brand_name and brand_status=?brand_status and brand_subject=?brand_subject and brand_posid=?brand_posid limit 1";
                var insertBrandSql = @"insert into brand_default(brand_name,brand_status,brand_subject,brand_posid) values(?brand_name,?brand_status,?brand_subject,?brand_posid)";
                var paramBrand = new DynamicParameters();
                paramBrand.Add("?brand_name", brand_name);
                paramBrand.Add("?brand_status", brand_status);
                paramBrand.Add("?brand_subject", brand_subject);
                paramBrand.Add("?brand_posid", brand_posid);

                var selectAreaSql = @"select area_name,area_level,area_status,area_subject,area_posid from area_default where area_name=?area_name and area_level=?area_level and area_status=?area_status and area_subject=?area_subject and area_posid=?area_posid limit 1";
                var insertAreaSql = @"insert into area_default(area_name,area_level,area_status,area_subject,area_posid) values(?area_name,?area_level,?area_status,?area_subject,?area_posid)";
                var paramArea = new DynamicParameters();
                paramArea.Add("?area_name", area_name);
                paramArea.Add("?area_level", area_level);
                paramArea.Add("?area_status", area_status);
                paramArea.Add("?area_subject", area_subject);
                paramArea.Add("?area_posid", area_posid);

                var selectDeptSql = @"select dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid from dept_default where dept_name=?dept_name and dept_alias=?dept_alias and dept_status=?dept_status and dept_sequence=?dept_sequence and dept_subject=?dept_subject and dept_brand=?dept_brand and dept_posid=?dept_posid limit 1";
                var insertDeptSql = @"insert into dept_default(dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid) values(?dept_name,?dept_alias,?dept_status,?dept_sequence,?dept_subject,?dept_brand,?dept_posid)";
                var paramDept = new DynamicParameters();
                paramDept.Add("?dept_name", dept_name);
                paramDept.Add("?dept_alias", dept_alias);
                paramDept.Add("?dept_status", dept_status);
                paramDept.Add("?dept_sequence", dept_sequence);
                paramDept.Add("?dept_subject", dept_subject);
                paramDept.Add("?dept_brand", dept_brand);
                paramDept.Add("?dept_posid", dept_posid);
                try
                {

                    using (var db = new MySqlConnection(connectionString))
                    {
                        if (brandFlag)
                        {
                            var brandInfo = db.QueryFirstOrDefault<Brand_default>(selectBrandSql, paramBrand);
                            if (brandInfo == null)
                            {
                                db.Execute(insertBrandSql, paramBrand);
                                //品牌数据初始化完毕并允许同步品牌数据
                                btn_Brand.Enabled = true;
                            }
                        }
                        else
                        {
                            btn_Brand.Enabled = false;
                        }

                        if (areaFlag)
                        {
                            var areaInfo = db.QueryFirstOrDefault<Area_default>(selectAreaSql, paramArea);
                            if (areaInfo == null)
                            {
                                db.Execute(insertAreaSql, paramArea);
                                //区域数据初始化完毕并允许同步区域数据
                                btn_Area.Enabled = true;
                            }
                        }
                        else
                        {
                            btn_Area.Enabled = false;
                        }

                        if (deptFlag)
                        {
                            var deptInfo = db.QueryFirstOrDefault<Dept_default>(selectDeptSql, paramDept);
                            if (deptInfo == null)
                            {
                                db.Execute(insertDeptSql, paramDept);
                                //部门数据初始化完毕并允许同步部门数据
                                btn_Dept.Enabled = true;
                            }
                        }
                        else
                        {
                            btn_Dept.Enabled = false;
                        }
                    }

                }
                catch (Exception ex)
                {
                    rtxt_message.Text += ex.Message + "\r\n";
                    //
                }

            }
            else
            {
                //配置文件的初始化数据有问题,按钮不可用
                btn_Brand.Enabled = false;
                btn_Area.Enabled = false;
                btn_Dept.Enabled = false;
            }
        }

        bool brand_sync = false;//true 为正在同步中
        private void btn_Brand_Click(object sender, EventArgs e)
        {
            //防止重复点击同步
            //有没有数据可同步，有则整理成list用API传送
            //写同步记录
            if (brand_sync)
            {
                MessageBox.Show("数据同步中，请勿重复点击!", "Tips");
            }
            else
            {
                var getLastSyncInfoSql = @"select tablename,lastupdatetime from syncinfo where tablename='brand_defalut' limit 1";
                var getDataSql = @"select brand_name,brand_status,brand_subject,brand_posid from brand_default where lastdatetime>?lastdatetime";
                var param = new DynamicParameters();
                using (var db = new MySqlConnection(ConfigInfo.Mysql_connectionstring))
                {
                    var getLastSyncInfo = db.QueryFirstOrDefault<SyncInfo>(getLastSyncInfoSql);
                    if (getLastSyncInfo != null)
                    {

                        param.Add("?lastdatetime", getLastSyncInfo.LastUpdateTime);
                        var getData = db.Query<Brand_default>(getDataSql, param).ToList();
                        var dataList = new List<Sls_brand>();
                        foreach (var item in getData)
                        {
                            var data = new Sls_brand() { Brandname = item.Brand_name, Status = item.Brand_status, Subject = item.Brand_subject, Cbid = item.Brand_posid };
                            dataList.Add(data);
                        }
                        if (getData.Count > 0)
                        {
                            //请求接口同步数据
                            var request = WebRequest.CreateHttp(ConfigInfo.Apiurl_addbrand);
                            request.Method = "POST";
                            var response = request.GetResponse();

                            var updateLastSyncSql = @"update syncinfo set lastupdatetime=?lastupdatetime";
                            var paramUpdate = new DynamicParameters();
                            paramUpdate.Add("?lastupdatetime", DateTime.Now);
                            db.Execute(updateLastSyncSql, paramUpdate);

                        }
                    }
                    else
                    {
                        rtxt_message.Text += "brand_default not in syncinfo\r\n";
                    }
                }
                //同步完成将brand_sync置为false
                brand_sync = false;
            }
        }

        private void btn_Area_Click(object sender, EventArgs e)
        {

        }

        private void btn_Dept_Click(object sender, EventArgs e)
        {

        }

        private void btn_Store_Click(object sender, EventArgs e)
        {

        }

        private void btn_Unit_Click(object sender, EventArgs e)
        {

        }

        private void btn_DishType_Click(object sender, EventArgs e)
        {

        }

        private void btn_Dish_Click(object sender, EventArgs e)
        {

        }

        private void btn_OrderMain_Click(object sender, EventArgs e)
        {

        }

        private void btn_OrderDetail_Click(object sender, EventArgs e)
        {

        }

        private void btn_MealTime_Click(object sender, EventArgs e)
        {

        }

        private void btn_User_Click(object sender, EventArgs e)
        {

        }

        private void btn_Payway_Click(object sender, EventArgs e)
        {

        }
    }
}
