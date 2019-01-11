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
using Newtonsoft.Json;
using System.Threading;
using System.Data.SqlClient;

namespace DSTool
{
    public partial class MSTool : Form
    {
        public MSTool()
        {
            InitializeComponent();
        }
        static ConfigInfo ConfigInfo = ConfigInfo.CreateInstance;
        static string connectionString = ConfigInfo.ConnectionString;
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
                    using (var db = new SqlConnection(connectionString))
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
            var selectBrandSql = @"select table_name from information_schema.tables where TABLE_NAME=@tablename";
            var createBrandSql = @"CREATE TABLE [dbo].[brand_default](
	                                            [brand_name] [varchar](50) NOT NULL,
	                                            [brand_status] [nchar](10) NOT NULL,
	                                            [brand_subject] [nchar](10) NOT NULL,
	                                            [brand_posid] [int] NOT NULL,
	                                            [seq] [int] NULL,
	                                            [utime] [datetime] NULL,
	                                            [memo] [varchar](50) NULL,
	                                            [createtime] [datetime] NULL,
	                                            [lastupdatetime] [datetime] NULL,
                                             CONSTRAINT [PK_brand_default] PRIMARY KEY CLUSTERED 
                                            (
	                                            [brand_posid] ASC
                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                            ) ON [PRIMARY]";
            var paramBrand = new DynamicParameters();
            paramBrand.Add("tablename", ConfigInfo.Brand_table);

            //area_faid
            var selectAreaSql = @"select table_name from information_schema.tables where TABLE_NAME=@tablename";
            var createAreaSql = @" CREATE TABLE [dbo].[area_default](
	                                            [area_name] [varchar](50) NOT NULL,
	                                            [area_level] [int] NOT NULL,
	                                            [area_faid] [int] NULL,
	                                            [area_seq] [int] NULL,
	                                            [area_status] [int] NOT NULL,
	                                            [area_subject] [varchar](50) NOT NULL,
	                                            [area_posid] [int] NOT NULL,
	                                            [createtime] [datetime] NULL,
	                                            [lastupdatetime] [datetime] NULL,
                                             CONSTRAINT [PK_area_default] PRIMARY KEY CLUSTERED 
                                            (
	                                            [area_posid] ASC
                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                            ) ON [PRIMARY]";
            var paramArea = new DynamicParameters();
            paramArea.Add("tablename", ConfigInfo.Area_table);

            var selectDeptSql = @"select table_name from information_schema.tables where TABLE_NAME=@tablename";
            var createDeptSql = @" CREATE TABLE [dbo].[dept_default](
	                                            [dept_alias] [varchar](50) NOT NULL,
	                                            [dept_name] [int] NOT NULL,
	                                            [dept_brand] [int] NOT NULL,
	                                            [dept_sequence] [int] NOT NULL,
	                                            [dept_status] [int] NOT NULL,
	                                            [dept_subject] [varchar](50) NOT NULL,
	                                            [dept_posid] [int] NOT NULL,
	                                            [createtime] [datetime] NULL,
	                                            [lastupdatetime] [datetime] NULL,
                                             CONSTRAINT [PK_dept_default] PRIMARY KEY CLUSTERED 
                                            (
	                                            [dept_posid] ASC
                                            )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                                            ) ON [PRIMARY]";
            var paramDept = new DynamicParameters();
            paramDept.Add("tablename", ConfigInfo.Dept_table);
            #endregion

            //建表 
            using (var db = new SqlConnection(connectionString))
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

        bool brand_sync = false;//true:品牌 正在同步中
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
                    return;
                }

                brand_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品牌数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("brand_default");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"brand_default not in syncinfo\r\n"));
                        brand_sync = false;
                        return;
                    }
                    var getData = Brand_default.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    var getFailedData = SyncFailData.GetListByTableName("brand_default");
                    //获取待同步的数据,包含新增和修改,不含删除,以及之前同步失败的数据
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品牌无数据同步\r\n"));
                        brand_sync = false;
                        return;
                    }
                    bool flag = true;

                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length == 1)
                            {
                                var itemData = Brand_default.GetById(Convert.ToInt32(idList[0]));
                                if (itemData != null)
                                {
                                    var data = new Sls_brand()
                                    {
                                        BrandName = itemData.Brand_name,
                                        Status = itemData.Brand_status,
                                        Subject = itemData.Brand_subject,
                                        CbId = itemData.Brand_posid
                                    };
                                    if (item.FailType == 0)
                                    {
                                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_addbrand, paramData);
                                        if (result_add == null || !result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品牌数据新增失败,原因:{result_add?.Msg}\r\n"));
                                        }
                                        else
                                        {
                                            SyncFailData.DeleteFailData("brand_default", item.IdList);
                                        }
                                    }
                                    else
                                    {
                                        var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editbrand, paramData_update);
                                        if (result_update == null || !result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品牌数据修改失败,原因:{result_update?.Msg}\r\n"));
                                        }
                                        else
                                        {
                                            SyncFailData.DeleteFailData("brand_default", item.IdList);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //同步数据
                    foreach (var item in getData)
                    {
                        var data = new Sls_brand()
                        {
                            BrandName = item.Brand_name,
                            Status = item.Brand_status,
                            Subject = item.Brand_subject,
                            CbId = item.Brand_posid
                        };
                        if (item.CreateTime == item.LastUpdateTime)
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_addbrand, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品牌数据新增失败,原因:{result_add?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "brand_default", IdList = data.CbId.ToString(), FailType = 0, FailMessage = result_add?.Msg });
                                flag = flag && false;
                            }
                        }
                        else
                        {
                            var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_update = Common.Post(ConfigInfo.Apiurl_editbrand, paramData_update);
                            if (result_update == null || !result_update.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品牌数据修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "brand_default", IdList = data.CbId.ToString(), FailType = 0, FailMessage = result_update?.Msg });
                                flag = flag && false;
                            }
                        }
                    }

                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("brand_default");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品牌数据同步成功\r\n"));
                    }


                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInBrand:{ex.Message}\r\n"));
                }
                finally
                {
                    brand_sync = false;
                }
            });
        }

        bool area_sync = false;//true:区域 正在同步中
        private void btn_Area_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (area_sync)
                {
                    MessageBox.Show("对不起! 区域数据同步中,请勿重复点击!", "Tips");
                    return;
                }
                area_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"区域数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("area_default");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"area_default not in syncinfo\r\n"));
                        area_sync = false;
                        return;
                    }
                    var getData = Area_default.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    var getFailedData = SyncFailData.GetListByTableName("area_default");
                    if (getFailedData.Count > 0 || getData.Count > 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"区域无数据同步\r\n"));
                        area_sync = false;
                        return;
                    }

                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length == 1)
                            {
                                var itemData = Area_default.GetById(Convert.ToInt32(idList[0]));
                                if (itemData != null)
                                {
                                    var data = new Sls_area()
                                    {
                                        AreaName = itemData.Area_name,
                                        AreaLevel = itemData.Area_level,
                                        FaId = itemData.Area_faid,
                                        Seq = itemData.Area_Seq,
                                        Status = itemData.Area_status,
                                        Subject = itemData.Area_subject,
                                        CaId = itemData.Area_posid
                                    };
                                    if (item.FailType == 0)
                                    {
                                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_addarea, paramData);
                                        if (result_add == null || !result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 区域数据新增失败,原因:{result_add?.Msg}\r\n"));
                                        }
                                        else
                                        {
                                            SyncFailData.DeleteFailData("area_default", item.IdList);
                                        }
                                    }
                                    else
                                    {
                                        var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editarea, paramData_update);
                                        if (result_update == null || !result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 区域数据修改失败,原因:{result_update?.Msg}\r\n"));
                                        }
                                        else
                                        {
                                            SyncFailData.DeleteFailData("area_default", item.IdList);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag = true;
                    //同步数据
                    foreach (var item in getData)
                    {
                        var data = new Sls_area()
                        {
                            AreaName = item.Area_name,
                            AreaLevel = item.Area_level,
                            FaId = item.Area_faid,
                            Seq = item.Area_Seq,
                            Status = item.Area_status,
                            Subject = item.Area_subject,
                            CaId = item.Area_posid
                        };
                        if (item.CreateTime == item.LastUpdateTime)
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_addarea, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 区域数据新增失败,原因:{result_add?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "area_default", IdList = data.CaId.ToString(), FailType = 0, FailMessage = result_add?.Msg });
                                flag = flag && false;
                            }
                        }
                        else
                        {
                            var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_update = Common.Post(ConfigInfo.Apiurl_editarea, paramData_update);
                            if (result_update == null || !result_update.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 区域数据修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "area_default", IdList = data.CaId.ToString(), FailType = 0, FailMessage = result_update?.Msg });
                                flag = flag && false;
                            }
                        }
                    }

                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("area_default");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"区域数据同步成功\r\n"));
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
            });
        }

        bool store_sync = false;//true:门店 正在同步中
        private void btn_Store_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (store_sync)
                {
                    MessageBox.Show("对不起! 门店数据同步中,请勿重复点击!", "Tips");
                    return;
                }
                store_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"门店数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("store");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"store not in syncinfo\r\n"));
                        store_sync = false;
                        return;
                    }
                    var getData = DA_FD.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyyMMdd"));
                    var getFailedData = SyncFailData.GetListByTableName("store");
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"门店无数据同步\r\n"));
                        store_sync = false;
                        return;
                    }
                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {

                    }
                    bool flag = true;
                    //同步数据
                    foreach (var item in getData)
                    {
                        var data = new Sls_shop()
                        {
                            ShopName = item.FD,
                            ShopCode = item.FDDM,
                            Subject = item.FDDM,
                            Status = item.QYBJ == 1 ? 1 : -1,
                            AId = ConfigInfo.Area_posid,
                            BId = ConfigInfo.Brand_posid,
                            SType = 4,
                            ShopAdd = item.DZ,
                            CsId = item.FDNM
                        };
                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                        var result_add = Common.Post(ConfigInfo.Apiurl_addstore, paramData);
                        if (result_add == null || !result_add.Success)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 门店数据(storeid:{item.FDNM})新增失败,原因:{result_add?.Msg}\r\n"));
                            SyncFailData.InsertFailDate(new SyncFailData() { TableName = "store", IdList = data.CsId.ToString(), FailType = 0, FailMessage = result_add?.Msg });
                            flag = false && true;
                        }
                    }
                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("store");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"门店数据同步成功\r\n"));
                    }
                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInStore:{ex.Message}\r\n"));
                }
                finally
                {
                    store_sync = false;
                }
            });
        }

        bool meal_time_sync = false;//true:餐段 正在同步中
        private void btn_MealTime_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (meal_time_sync)
                {
                    MessageBox.Show("对不起! 餐段数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    meal_time_sync = true;
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"餐段数据同步中,请耐心等待......\r\n"));
                    using (var db = new SqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = SyncInfo.GetInfoByTableName("meal_time");
                            if (getLastSyncInfo != null)
                            {
                                if (!getLastSyncInfo.IsSynced)
                                {
                                    var start = new DateTime(2019, 1, 1, 0, 0, 0);
                                    var end = new DateTime(2019, 1, 1, 23, 59, 59);
                                    var now = DateTime.Now;
                                    var data = new O_business_range()
                                    {
                                        Businessrange = "全天",
                                        StartTime = start,
                                        ShowStartTime = start,
                                        ShowEndTime = end,
                                        Status = 1,
                                        BId = ConfigInfo.Brand_posid.ToString(),
                                        CbrId = 1
                                    };
                                    var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                    var result_add = Common.Post(ConfigInfo.Apiurl_addmealtime, paramData);
                                    if (result_add == null || !result_add.Success)
                                    {
                                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 餐段数据新增失败,原因:{result_add?.Msg}\r\n"));
                                    }
                                    else
                                    {
                                        SyncInfo.UpdateByTableName("meal_time", 1);
                                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"餐段数据同步成功\r\n"));
                                    }
                                    //rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"餐段无数据同步\r\n"));
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 餐段已经同步过了且只同步一次\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"meal_time not in syncinfo\r\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInMeal_Time:{ex.Message}\r\n"));
                        }
                        finally
                        {
                            meal_time_sync = false;
                        }
                    }
                }
            });
        }

        bool dept_sync = false;//true: 部门 正在同步中
        private void btn_Dept_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (dept_sync)
                {
                    MessageBox.Show("对不起! 部门数据同步中,请勿重复点击!", "Tips");
                    return;
                }
                dept_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"部门数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("dept_default");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dept_default not in syncinfo\r\n"));

                    }
                    var getData = Dept_default.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    var getFailedData = SyncFailData.GetListByTableName("dept_default");
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"部门无数据同步\r\n"));
                        dept_sync = false;
                        return;
                    }
                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length == 1)
                            {
                                var itemData = Dept_default.GetById(Convert.ToInt32(idList[0]));
                                if (itemData != null)
                                {
                                    var data = new C_department()
                                    {
                                        Department = itemData.Dept_name,
                                        Alias = itemData.Dept_alias,
                                        Status = itemData.Dept_status,
                                        Seq = itemData.Dept_sequence,
                                        Sno = itemData.Dept_subject,
                                        BId = itemData.Dept_brand,
                                        CdmId = itemData.Dept_posid
                                    };
                                    if (item.FailType == 0)
                                    {
                                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                                        if (result_add == null || !result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 部门数据新增失败,原因:{result_add?.Msg}\r\n"));
                                        }
                                        else
                                        {
                                            SyncFailData.DeleteFailData("dept_default", item.IdList);
                                        }
                                    }
                                    else
                                    {
                                        var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editdept, paramData_update);
                                        if (result_update == null || !result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 部门数据修改失败,原因:{result_update?.Msg}\r\n"));
                                        }
                                        else
                                        {
                                            SyncFailData.DeleteFailData("dept_default", item.IdList);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool flag = true;
                    //同步数据
                    foreach (var item in getData)
                    {
                        var data = new C_department()
                        {
                            Department = item.Dept_name,
                            Alias = item.Dept_alias,
                            Status = item.Dept_status,
                            Seq = item.Dept_sequence,
                            Sno = item.Dept_subject,
                            BId = item.Dept_brand,
                            CdmId = item.Dept_posid
                        };
                        if (item.CreateTime == item.LastUpdateTime)
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 部门数据新增失败,原因:{result_add?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "dept_default", IdList = data.CdmId.ToString(), FailType = 0, FailMessage = result_add?.Msg });
                                flag = flag && false;
                            }
                        }
                        else
                        {
                            var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_update = Common.Post(ConfigInfo.Apiurl_editdept, paramData_update);
                            if (result_update == null || !result_update.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 部门数据修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "dept_default", IdList = data.CdmId.ToString(), FailType = 0, FailMessage = result_update?.Msg });
                                flag = flag && false;
                            }
                        }
                    }

                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("dept_default");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"部门数据同步成功\r\n"));
                    }

                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInDept:{ex.Message}\r\n"));
                }
                finally
                {
                    dept_sync = false;
                }
            });
        }

        bool dish_type_sync = false;//true: 品项类型 正在同步中
        private void btn_DishType_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (dish_type_sync)
                {
                    MessageBox.Show("对不起! 品项类型数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    dish_type_sync = true;
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品项类型数据同步中,请耐心等待......\r\n"));
                    using (var db = new SqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = SyncInfo.GetInfoByTableName("dish_type");
                            if (getLastSyncInfo != null)
                            {
                                var getData = DA_SPLB.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                var getFailedData = SyncFailData.GetListByTableName("dish_type");
                                var now = DateTime.Now;
                                if (getFailedData.Count > 0 || getData.Count > 0)
                                {
                                    foreach (var item in getData)
                                    {
                                        var data = new O_dish_kind()
                                        {
                                            DkId = item.SPLBNM,
                                            PdkId = item.SSLBNM,
                                            DishKind = item.SPLB,
                                            Valid = item.QYBJ,
                                            SNo = item.SPLBDM,
                                            BId = ConfigInfo.Brand_posid,
                                            UTime = now.ToString("yyyy-MM-dd HH:mm:ss"),
                                            DmId = ConfigInfo.Dept_posid
                                        };
                                        var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editdishtype, paramData_update);
                                        if (result_update == null || !result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品项类型数据修改失败,原因:{result_update?.Msg}\r\n"));
                                        }

                                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_adddishtype, paramData);
                                        if (result_add == null || !result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品项类型数据新增失败,原因:{result_add?.Msg}\r\n"));
                                        }
                                    }
                                    SyncInfo.UpdateByTableName("dish_type");
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品项类型数据同步成功\r\n"));
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品项类型无数据同步\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dish_type not in syncinfo\r\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInDish_Type:{ex.Message}\r\n"));
                        }
                        finally
                        {
                            dish_type_sync = false;
                        }
                    }
                }
            });
        }

        bool unit_sync = false;//true:单位 正在同步中
        private void btn_Unit_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (unit_sync)
                {
                    MessageBox.Show("对不起! 单位数据同步中,请勿重复点击!", "Tips");
                    return;
                }
                unit_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"单位数据同步中,请耐心等待......\r\n"));
                using (var db = new SqlConnection(ConfigInfo.Mysql_connectionstring))
                {
                    try
                    {
                        var getLastSyncInfo = SyncInfo.GetInfoByTableName("unit");
                        if (getLastSyncInfo == null)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"unit not in syncinfo\r\n"));
                            unit_sync = false;
                            return;
                        }
                        if (getLastSyncInfo.IsSynced)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 单位已经同步过了且只同步一次\r\n"));
                            unit_sync = false;
                            return;
                        }
                        var getData = DA_JLDW.GetList();
                        var getFailedData = SyncFailData.GetListByTableName("unit");
                        var now = DateTime.Now;
                        if (getFailedData.Count <= 0 && getData.Count <= 0)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 单位已经同步过了且只同步一次\r\n"));
                            unit_sync = false;
                            return;
                        }
                        bool flag = true;
                        foreach (var item in getData)
                        {
                            var data = new O_dish_unit()
                            {
                                DuId = item.JLDWNM,
                                DishUnit = item.JLDW,
                                UTime = now.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_addunit, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 单位数据新增失败,原因:{result_add?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "unit", IdList = data.DuId.ToString(), FailType = 0, FailMessage = result_add?.Msg });
                                flag = flag && false;
                            }
                        }
                        SyncInfo.UpdateByTableName("unit", 1);
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"单位数据同步成功\r\n"));

                    }
                    catch (Exception ex)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInUnit:{ex.Message}\r\n"));
                    }
                    finally
                    {
                        unit_sync = false;
                    }
                }
            });
        }

        bool dish_sync = false;//true:品项 正在同步中
        private void btn_Dish_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (dish_sync)
                {
                    MessageBox.Show("对不起! 品项数据同步中,请勿重复点击!", "Tips");
                }
                else
                {
                    dish_sync = true;
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品项数据同步中,请耐心等待......\r\n"));
                    using (var db = new SqlConnection(ConfigInfo.Mysql_connectionstring))
                    {
                        try
                        {
                            var getLastSyncInfo = SyncInfo.GetInfoByTableName("dish");
                            if (getLastSyncInfo != null)
                            {
                                var getFailedData = SyncFailData.GetListByTableName("dish_type");
                                var getData = DA_SP.GetListByLastTime(getLastSyncInfo.LastUpdateTime);
                                if (getFailedData.Count > 0 || getData.Count > 0)
                                {
                                    //同步以往失败的数据
                                    if (getFailedData.Count > 0)
                                    {

                                    }

                                    //同步数据
                                    foreach (var item in getData)
                                    {
                                        var data = new O_dish()
                                        {
                                            DId = item.SPNM,
                                            DkId = item.SPXLNM,
                                            SNo = item.SP_E,
                                            BId = ConfigInfo.Brand_posid,
                                            CTime = item.JDRQ.ToString(),
                                            DmId = ConfigInfo.Dept_posid,
                                            Dish = item.SP,
                                            Alias = item.SPZJM,
                                            Status = item.QYBJ == 1 ? 1 : 2,
                                            Seq = item.XH
                                        };
                                        var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_update = Common.Post(ConfigInfo.Apiurl_editdish, paramData_update);
                                        if (result_update == null || !result_update.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品项数据修改失败,原因:{result_update?.Msg}\r\n"));
                                        }

                                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                        var result_add = Common.Post(ConfigInfo.Apiurl_adddish, paramData);
                                        if (result_add == null || !result_add.Success)
                                        {
                                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品项数据新增失败,原因:{result_add?.Msg}\r\n"));
                                        }
                                    }

                                    SyncInfo.UpdateByTableName("dish");
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品项数据同步成功\r\n"));
                                }
                                else
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"品项无数据同步\r\n"));
                                }
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dish not in syncinfo\r\n"));
                            }
                        }
                        catch (Exception ex)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"ExceptionInfoInDish:{ex.Message}\r\n"));
                        }
                        finally
                        {
                            dish_sync = false;
                        }
                    }
                }
            });
        }

        private void btn_OrderMain_Click(object sender, EventArgs e)
        {

        }

        private void btn_OrderDetail_Click(object sender, EventArgs e)
        {

        }

        private void btn_User_Click(object sender, EventArgs e)
        {

        }

        private void btn_Payway_Click(object sender, EventArgs e)
        {

        }

        private void rtxt_message_TextChanged(object sender, EventArgs e)
        {
            rtxt_message.SelectionStart = rtxt_message.Text.Length;
            rtxt_message.ScrollToCaret();
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

        /// <summary>
        /// 数据初始化
        /// </summary>
        void InitData()
        {
            var brand_name = ConfigInfo.Brand_name;
            var brand_status = ConfigInfo.Brand_status;
            var brand_subject = ConfigInfo.Brand_subject;
            var brand_posid = ConfigInfo.Brand_posid;

            var area_name = ConfigInfo.Area_name;
            var area_level = ConfigInfo.Area_level;
            var area_faid = ConfigInfo.Area_faid;
            var area_seq = ConfigInfo.Area_seq;
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
            if (!string.IsNullOrEmpty(brand_name) && !string.IsNullOrEmpty(brand_status) && !string.IsNullOrEmpty(brand_subject) && !string.IsNullOrEmpty(brand_posid.ToString()))
            {
                brandFlag = true;
            }
            if (!string.IsNullOrEmpty(area_name) && !string.IsNullOrEmpty(area_level) && !string.IsNullOrEmpty(area_status) && !string.IsNullOrEmpty(area_subject) && !string.IsNullOrEmpty(area_posid.ToString()))
            {
                areaFlag = true;
            }
            if (!string.IsNullOrEmpty(dept_name) && !string.IsNullOrEmpty(dept_alias) && !string.IsNullOrEmpty(dept_status) && !string.IsNullOrEmpty(dept_sequence) && !string.IsNullOrEmpty(dept_subject) && !string.IsNullOrEmpty(dept_brand.ToString()) && !string.IsNullOrEmpty(dept_posid.ToString()))
            {
                deptFlag = true;
            }
            if (brandFlag || areaFlag || deptFlag)
            {
                var selectBrandSql = @"select top 1 brand_name,brand_status,brand_subject,brand_posid from brand_default where brand_name=@brand_name and brand_status=@brand_status and brand_subject=@brand_subject and brand_posid=@brand_posid";
                var insertBrandSql = @"insert into brand_default(brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime) values(@brand_name,@brand_status,@brand_subject,@brand_posid,@createtime,@lastupdatetime)";
                var paramBrand = new DynamicParameters();
                var dateTime = DateTime.Now;
                paramBrand.Add("brand_name", brand_name);
                paramBrand.Add("brand_status", brand_status);
                paramBrand.Add("brand_subject", brand_subject);
                paramBrand.Add("brand_posid", brand_posid);
                paramBrand.Add("createtime", dateTime);
                paramBrand.Add("lastupdatetime", dateTime);

                var selectAreaSql = @"select top 1 area_name,area_level,area_faid,area_seq,area_status,area_subject,area_posid from area_default where area_name=@area_name and area_level=@area_level and area_status=@area_status and area_subject=@area_subject and area_posid=@area_posid";
                var insertAreaSql = @"insert into area_default(area_name,area_level,area_faid,area_seq,area_status,area_subject,area_posid,createtime,lastupdatetime) values(@area_name,@area_level,@area_faid,@area_seq,@area_status,@area_subject,@area_posid,@createtime,@lastupdatetime)";
                var paramArea = new DynamicParameters();
                paramArea.Add("area_name", area_name);
                paramArea.Add("area_level", area_level);
                paramArea.Add("area_faid", area_faid);
                paramArea.Add("area_seq", area_seq);
                paramArea.Add("area_status", area_status);
                paramArea.Add("area_subject", area_subject);
                paramArea.Add("area_posid", area_posid);
                paramArea.Add("createtime", dateTime);
                paramArea.Add("lastupdatetime", dateTime);

                var selectDeptSql = @"select top 1 dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid from dept_default where dept_name=@dept_name and dept_alias=@dept_alias and dept_status=@dept_status and dept_sequence=@dept_sequence and dept_subject=@dept_subject and dept_brand=@dept_brand and dept_posid=@dept_posid";
                var insertDeptSql = @"insert into dept_default(dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid,createtime,lastupdatetime) values(@dept_name,@dept_alias,@dept_status,@dept_sequence,@dept_subject,@dept_brand,@dept_posid,@createtime,@lastupdatetime)";
                var paramDept = new DynamicParameters();
                paramDept.Add("dept_name", dept_name);
                paramDept.Add("dept_alias", dept_alias);
                paramDept.Add("dept_status", dept_status);
                paramDept.Add("dept_sequence", dept_sequence);
                paramDept.Add("dept_subject", dept_subject);
                paramDept.Add("dept_brand", dept_brand);
                paramDept.Add("dept_posid", dept_posid);
                paramDept.Add("createtime", dateTime);
                paramDept.Add("lastupdatetime", dateTime);

                try
                {

                    using (var db = new SqlConnection(connectionString))
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

        #region 提取函数
        private void ExecteDataSync(List<object> dataList_update, List<object> dataList_add, string tips, string tableName)
        {
            bool updateFlag = true;
            if (dataList_update.Count > 0)
            {
                var paramData_update = $"arr={JsonConvert.SerializeObject(dataList_add)}";
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
                var paramData = $"arr={JsonConvert.SerializeObject(dataList_add)}";
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
            using (var db = new SqlConnection(ConfigInfo.Mysql_connectionstring))
            {
                var updateLastSyncSql = $"update syncinfo set lastupdatetime=@lastupdatetime where tablename=@tablename";
                var paramUpdate = new DynamicParameters();
                paramUpdate.Add("lastupdatetime", DateTime.Now);
                paramUpdate.Add("tablename", tableName);
                return db.Execute(updateLastSyncSql, paramUpdate);
            }
        }

        #endregion

    }
}
