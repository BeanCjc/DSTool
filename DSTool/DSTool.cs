﻿using System;
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
using Newtonsoft.Json;

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
                                                              `brand_posid` varchar(255) DEFAULT NULL COMMENT 'pos ID',
                                                              `createtime` datetime(6) DEFAULT NULL COMMENT '创建时间',
                                                              `lastupdatetime` datetime(6) DEFAULT NULL COMMENT '修改时间'
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
            var paramBrand = new DynamicParameters();
            paramBrand.Add("?tablename", ConfigInfo.Brand_table);


            var selectAreaSql = @"select table_name from information_schema.tables where table_schema='wiseposdb' and TABLE_NAME=?tablename";
            var createAreaSql = @"CREATE TABLE `area_default` (
                                                              `area_name` varchar(255) DEFAULT NULL COMMENT '区域名称',
                                                              `area_level` varchar(255) DEFAULT NULL COMMENT '区域层级',
                                                              `area_status` varchar(255) DEFAULT NULL COMMENT '状态 1:有效 0:无效',
                                                              `area_subject` varchar(255) DEFAULT NULL COMMENT '编码',
                                                              `area_posid` varchar(255) DEFAULT NULL COMMENT 'pos id',
                                                              `createtime` datetime(6) DEFAULT NULL COMMENT '创建时间',
                                                              `lastupdatetime` datetime(6) DEFAULT NULL COMMENT '修改时间'
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
                                                              `dept_posid` varchar(255) DEFAULT NULL COMMENT 'pos id',
                                                              `createtime` datetime(6) DEFAULT NULL COMMENT '创建时间',
                                                              `lastupdatetime` datetime(6) DEFAULT NULL COMMENT '修改时间'
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
                var insertBrandSql = @"insert into brand_default(brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime) values(?brand_name,?brand_status,?brand_subject,?brand_posid,?createtime,?lastupdatetime)";
                var paramBrand = new DynamicParameters();
                var dateTime = DateTime.Now;
                paramBrand.Add("?brand_name", brand_name);
                paramBrand.Add("?brand_status", brand_status);
                paramBrand.Add("?brand_subject", brand_subject);
                paramBrand.Add("?brand_posid", brand_posid);
                paramBrand.Add("?createtime", dateTime);
                paramBrand.Add("?lastupdatetime", dateTime);

                var selectAreaSql = @"select area_name,area_level,area_status,area_subject,area_posid from area_default where area_name=?area_name and area_level=?area_level and area_status=?area_status and area_subject=?area_subject and area_posid=?area_posid limit 1";
                var insertAreaSql = @"insert into area_default(area_name,area_level,area_status,area_subject,area_posid,createtime,lastupdatetime) values(?area_name,?area_level,?area_status,?area_subject,?area_posid,?createtime,?lastupdatetime)";
                var paramArea = new DynamicParameters();
                paramArea.Add("?area_name", area_name);
                paramArea.Add("?area_level", area_level);
                paramArea.Add("?area_status", area_status);
                paramArea.Add("?area_subject", area_subject);
                paramArea.Add("?area_posid", area_posid);
                paramArea.Add("?createtime", dateTime);
                paramArea.Add("?lastupdatetime", dateTime);

                var selectDeptSql = @"select dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid from dept_default where dept_name=?dept_name and dept_alias=?dept_alias and dept_status=?dept_status and dept_sequence=?dept_sequence and dept_subject=?dept_subject and dept_brand=?dept_brand and dept_posid=?dept_posid limit 1";
                var insertDeptSql = @"insert into dept_default(dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid,createtime,lastupdatetime) values(?dept_name,?dept_alias,?dept_status,?dept_sequence,?dept_subject,?dept_brand,?dept_posid,?createtime,?lastupdatetime)";
                var paramDept = new DynamicParameters();
                paramDept.Add("?dept_name", dept_name);
                paramDept.Add("?dept_alias", dept_alias);
                paramDept.Add("?dept_status", dept_status);
                paramDept.Add("?dept_sequence", dept_sequence);
                paramDept.Add("?dept_subject", dept_subject);
                paramDept.Add("?dept_brand", dept_brand);
                paramDept.Add("?dept_posid", dept_posid);
                paramDept.Add("?createtime", dateTime);
                paramDept.Add("?lastupdatetime", dateTime);

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

        bool brand_sync = false;//true:正在同步中
        private void btn_Brand_Click(object sender, EventArgs e)
        {
            //防止重复点击同步
            //有没有数据可同步，有则整理成list用API传送
            //写同步记录
            Task.Run(() =>
            {
                //先不管线程安全了,这样也能跑起来
                if (brand_sync)
                {
                    MessageBox.Show("对不起! 品牌数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品牌数据同步中,请耐心等待......\r\n"));
                    var getLastSyncInfoSql = @"select tablename,lastupdatetime from syncinfo where tablename='brand_defalut' limit 1";
                    var getDataSql = @"select brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime from brand_default where lastupdatetime>=?lastupdatetime";
                    var param = new DynamicParameters();
                    using (var db = new MySqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = db.QueryFirstOrDefault<SyncInfo>(getLastSyncInfoSql);
                            if (getLastSyncInfo != null)
                            {
                                param.Add("?lastupdatetime", getLastSyncInfo.LastUpdateTime);
                                var getData = db.Query<Brand_default>(getDataSql, param).ToList();
                                //获取待同步的数据,包含新增和修改,不含删除
                                var dataList_add = new List<Sls_brand>();
                                var dataList_update = new List<Sls_brand>();
                                foreach (var item in getData)
                                {
                                    var data = new Sls_brand() { BrandName = item.Brand_name, Status = item.Brand_status, Subject = item.Brand_subject, CbId = item.Brand_posid };
                                    if (item.CreateTime == item.LastUpdateTime)
                                    {
                                        dataList_add.Add(data);
                                    }
                                    else
                                    {
                                        dataList_update.Add(data);
                                    }
                                }
                                if (dataList_add.Count > 0 || dataList_update.Count > 0)
                                {
                                    //先发起修改的请求，再发起新增的请求，目的分为三种情况.
                                    //情况1:修改失败,不执行新增的请求;
                                    //情况二:修改成功，新增的请求失败,不写更新记录表,下次将再次修改数据并添加数据;
                                    //情况三:修改和新增都成功,那就更新更新记录表

                                    //请求接口同步数据 edit
                                    bool updateFlag = true;
                                    if (dataList_update.Count > 0)
                                    {
                                        var paramData_update = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editbrand, paramData_update);
                                        if (result_update == null || result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品牌数据修改失败,原因:{result_update?.Msg}\r\n"));
                                            //rtxt_message.Text += $"品牌数据修改失败，原因:{result_update?.Msg}\r\n";
                                            updateFlag = false;
                                        }
                                    }

                                    bool addFlag = true;
                                    if (dataList_add.Count > 0 && updateFlag)
                                    {
                                        //请求接口同步数据 add
                                        //var url = ConfigInfo.Apiurl_addbrand + "?do=brand&type=add&apikey=sign";
                                        var paramData = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_addbrand, paramData);
                                        if (result_add == null || result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品牌数据新增失败,原因:{result_add?.Msg}\r\n"));
                                            //rtxt_message.Text += $"品牌数据新增失败，原因:{result_add?.Msg}\r\n";
                                            addFlag = false;
                                        }
                                    }

                                    //数据同步成功,写更新记录表,若此次没有数据同步也将更新该表,下次就不用从原来的时间再次同步(一般不会有这种情况)
                                    if (updateFlag && addFlag)
                                    {
                                        var updateLastSyncSql = @"update syncinfo set lastupdatetime=?lastupdatetime where tablename='brand_default'";
                                        var paramUpdate = new DynamicParameters();
                                        paramUpdate.Add("?lastupdatetime", DateTime.Now);
                                        db.Execute(updateLastSyncSql, paramUpdate);
                                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品牌数据同步成功\r\n"));
                                        //rtxt_message.Text += $"品牌数据同步成功\r\n";
                                    }
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品牌无数据同步\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"brand_default not in syncinfo\r\n"));
                                //rtxt_message.Text += "brand_default not in syncinfo\r\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInBrand:{ex.Message}\r\n"));
                            //rtxt_message.Text += $"ExceptionInfoInBrand:{ex.Message}\r\n";
                        }
                        finally
                        {
                            brand_sync = false;
                        }
                    }
                }
            });
        }

        bool area_sync = false;//true:正在同步中
        private void btn_Area_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (area_sync)
                {
                    MessageBox.Show("对不起! 区域数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"区域数据同步中,请耐心等待......\r\n"));
                    var getLastSyncInfoSql = @"select tablename,lastupdatetime from syncinfo where tablename='area_defalut' limit 1";
                    var getDataSql = @"select brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime from area_default where lastupdatetime>=?lastupdatetime";
                    var param = new DynamicParameters();
                    using (var db = new MySqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = db.QueryFirstOrDefault<SyncInfo>(getLastSyncInfoSql);
                            if (getLastSyncInfo != null)
                            {
                                param.Add("?lastupdatetime", getLastSyncInfo.LastUpdateTime);
                                var getData = db.Query<Area_default>(getDataSql, param).ToList();
                                var dataList_add = new List<Sls_area>();
                                var dataList_update = new List<Sls_area>();
                                foreach (var item in getData)
                                {
                                    var data = new Sls_area() { AreaName = item.Area_name, AreaLevel = item.Area_level, Status = item.Area_status, Subject = item.Area_subject, CaId = item.Area_posid };
                                    if (item.CreateTime == item.LastUpdateTime)
                                    {
                                        dataList_add.Add(data);
                                    }
                                    else
                                    {
                                        dataList_update.Add(data);
                                    }
                                }
                                if (dataList_add.Count > 0 || dataList_update.Count > 0)
                                {
                                    bool updateFlag = true;
                                    if (dataList_update.Count > 0)
                                    {
                                        var paramData_update = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editarea, paramData_update);
                                        if (result_update == null || result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 区域数据修改失败,原因:{result_update?.Msg}\r\n"));
                                            updateFlag = false;
                                        }
                                    }

                                    bool addFlag = true;
                                    if (dataList_add.Count > 0 && updateFlag)
                                    {
                                        var paramData = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_addarea, paramData);
                                        if (result_add == null || result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 区域数据新增失败,原因:{result_add?.Msg}\r\n"));
                                            addFlag = false;
                                        }
                                    }
                                    if (updateFlag && addFlag)
                                    {
                                        var updateLastSyncSql = @"update syncinfo set lastupdatetime=?lastupdatetime where tablename='area_default'";
                                        var paramUpdate = new DynamicParameters();
                                        paramUpdate.Add("?lastupdatetime", DateTime.Now);
                                        db.Execute(updateLastSyncSql, paramUpdate);
                                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"区域数据同步成功\r\n"));
                                    }
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"区域无数据同步\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"area_default not in syncinfo\r\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInArea:{ex.Message}\r\n"));
                        }
                        finally
                        {
                            area_sync = false;
                        }
                    }
                }
            });
        }

        bool dept_sync = false;//true:正在同步中
        private void btn_Dept_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (dept_sync)
                {
                    MessageBox.Show("对不起! 部门数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"部门数据同步中,请耐心等待......\r\n"));
                    var getLastSyncInfoSql = @"select tablename,lastupdatetime from syncinfo where tablename='dept_defalut' limit 1";
                    var getDataSql = @"select brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime from dept_default where lastupdatetime>=?lastupdatetime";
                    var param = new DynamicParameters();
                    using (var db = new MySqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = db.QueryFirstOrDefault<SyncInfo>(getLastSyncInfoSql);
                            if (getLastSyncInfo != null)
                            {
                                param.Add("?lastupdatetime", getLastSyncInfo.LastUpdateTime);
                                var getData = db.Query<Dept_default>(getDataSql, param).ToList();
                                var dataList_add = new List<C_department>();
                                var dataList_update = new List<C_department>();
                                foreach (var item in getData)
                                {
                                    var data = new C_department() { Department = item.Dept_name, Alias = item.Dept_alias, Status = item.Dept_status, Seq = item.Dept_sequence, Sno = item.Dept_subject, BId = item.Dept_brand, CdmId = item.Dept_posid };
                                    if (item.CreateTime == item.LastUpdateTime)
                                    {
                                        dataList_add.Add(data);
                                    }
                                    else
                                    {
                                        dataList_update.Add(data);
                                    }
                                }
                                if (dataList_add.Count > 0 || dataList_update.Count > 0)
                                {
                                    bool updateFlag = true;
                                    if (dataList_update.Count > 0)
                                    {
                                        var paramData_update = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editdept, paramData_update);
                                        if (result_update == null || result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 部门数据修改失败,原因:{result_update?.Msg}\r\n"));
                                            updateFlag = false;
                                        }
                                    }

                                    bool addFlag = true;
                                    if (dataList_add.Count > 0 && updateFlag)
                                    {
                                        var paramData = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                                        if (result_add == null || result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 部门数据新增失败,原因:{result_add?.Msg}\r\n"));
                                            addFlag = false;
                                        }
                                    }
                                    if (updateFlag && addFlag)
                                    {
                                        var updateLastSyncSql = @"update syncinfo set lastupdatetime=?lastupdatetime where tablename='dept_default'";
                                        var paramUpdate = new DynamicParameters();
                                        paramUpdate.Add("?lastupdatetime", DateTime.Now);
                                        db.Execute(updateLastSyncSql, paramUpdate);
                                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"部门数据同步成功\r\n"));
                                    }
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"部门无数据同步\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dept_default not in syncinfo\r\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInArea:{ex.Message}\r\n"));
                        }
                        finally
                        {
                            dept_sync = false;
                        }
                    }
                }
            });
        }

        private void btn_Store_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (dept_sync)
                {
                    MessageBox.Show("对不起! 门店数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"门店数据同步中,请耐心等待......\r\n"));
                    var getLastSyncInfoSql = @"select tablename,lastupdatetime from syncinfo where tablename='store' limit 1";
                    var getDataSql = @"select brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime from dept_default where lastupdatetime>=?lastupdatetime";
                    var param = new DynamicParameters();
                    using (var db = new MySqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = db.QueryFirstOrDefault<SyncInfo>(getLastSyncInfoSql);
                            if (getLastSyncInfo != null)
                            {
                                param.Add("?lastupdatetime", getLastSyncInfo.LastUpdateTime);
                                var getData = db.Query<Dept_default>(getDataSql, param).ToList();
                                var dataList_add = new List<C_department>();
                                var dataList_update = new List<C_department>();
                                foreach (var item in getData)
                                {
                                    var data = new C_department() { Department = item.Dept_name, Alias = item.Dept_alias, Status = item.Dept_status, Seq = item.Dept_sequence, Sno = item.Dept_subject, BId = item.Dept_brand, CdmId = item.Dept_posid };
                                    if (item.CreateTime == item.LastUpdateTime)
                                    {
                                        dataList_add.Add(data);
                                    }
                                    else
                                    {
                                        dataList_update.Add(data);
                                    }
                                }
                                if (dataList_add.Count > 0 || dataList_update.Count > 0)
                                {
                                    bool updateFlag = true;
                                    if (dataList_update.Count > 0)
                                    {
                                        var paramData_update = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editdept, paramData_update);
                                        if (result_update == null || result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 门店数据修改失败,原因:{result_update?.Msg}\r\n"));
                                            updateFlag = false;
                                        }
                                    }

                                    bool addFlag = true;
                                    if (dataList_add.Count > 0 && updateFlag)
                                    {
                                        var paramData = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                                        if (result_add == null || result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 门店数据新增失败,原因:{result_add?.Msg}\r\n"));
                                            addFlag = false;
                                        }
                                    }
                                    if (updateFlag && addFlag)
                                    {
                                        var updateLastSyncSql = @"update syncinfo set lastupdatetime=?lastupdatetime where tablename='dept_default'";
                                        var paramUpdate = new DynamicParameters();
                                        paramUpdate.Add("?lastupdatetime", DateTime.Now);
                                        db.Execute(updateLastSyncSql, paramUpdate);
                                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"门店数据同步成功\r\n"));
                                    }
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"门店无数据同步\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dept_default not in syncinfo\r\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInArea:{ex.Message}\r\n"));
                        }
                        finally
                        {
                            dept_sync = false;
                        }
                    }
                }
            });
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

        private void ExecteDataSync(List<object> dataList_update, List<object> dataList_add, string tips, string tableName)
        {
            bool updateFlag = true;
            if (dataList_update.Count > 0)
            {
                var paramData_update = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                var result_update = Common.Post(ConfigInfo.Apiurl_editdept, paramData_update);
                if (result_update == null || result_update.Success)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! {tips}数据修改失败,原因:{result_update?.Msg}\r\n"));
                    updateFlag = false;
                }
            }

            bool addFlag = true;
            if (dataList_add.Count > 0 && updateFlag)
            {
                var paramData = $"arr:{JsonConvert.SerializeObject(dataList_add)}";
                var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                if (result_add == null || result_add.Success)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! {tips}数据新增失败,原因:{result_add?.Msg}\r\n"));
                    addFlag = false;
                }
            }
            if (updateFlag && addFlag)
            {
                var res = UpdateSyncInfo(tableName);
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{tips}数据同步成功\r\n"));
            }
        }

        private int UpdateSyncInfo(string tableName)
        {
            using (var db = new MySqlConnection(ConfigInfo.Mysql_connectionstring))
            {
                var updateLastSyncSql = $"update syncinfo set lastupdatetime=?lastupdatetime where tablename=?tablename";
                var paramUpdate = new DynamicParameters();
                paramUpdate.Add("?lastupdatetime", DateTime.Now);
                paramUpdate.Add("?tablename", tableName);
                return db.Execute(updateLastSyncSql, paramUpdate);
            }
        }
    }
}
