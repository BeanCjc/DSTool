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
using Dapper;
using System.IO;
using DSTool.RequestEntity;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading;
using System.Data.SqlClient;
using System.IO;

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
        static System.Threading.Timer timerBase;
        static System.Threading.Timer timerOrder;
        static System.Threading.Timer timerRecordLog;

        private void MSTool_Load(object sender, EventArgs e)
        {
            label1.Text = @"首次同步请严格按照如下顺序进行同步:

1.品牌,区域->门店

2.品牌->餐段

3.品牌->部门

4.品牌->部门->品项类型,品项单位->品项

5.订单

程序启动后半小时将启动自动同步,基础数据将在每条的凌晨1点到2点之间同步两次,订单数据5分钟同步一次.";
            var currentDirectory = Directory.GetCurrentDirectory();
            var watcher = new FileSystemWatcher(currentDirectory, "DSTool.exe.config")
            {
                EnableRaisingEvents = true

            };
            watcher.Changed += (obj, fileSystemArgs) =>
            {
                ConfigInfo = ConfigInfo.CreateInstance;
                InitData();

            };//监听配置文件,刷新数据库默认数据(品牌,区域,部门)

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
            var selectBrandSql = @"select table_name from information_schema.tables where TABLE_NAME='brand_default'";
            var createBrandSql = @"CREATE TABLE [dbo].[brand_default](
	                                            [brand_name] [varchar](50) NOT NULL,
	                                            [brand_status] [int] NOT NULL,
	                                            [brand_subject] [varchar](50) NOT NULL,
	                                            [brand_posid] [varchar](50) NOT NULL,
	                                            [brand_seq] [int] NOT NULL,
	                                            [brand_utime] [datetime] NULL,
	                                            [brand_memo] [varchar](50) NULL,
	                                            [createtime] [datetime] NULL,
	                                            [lastupdatetime] [datetime] NULL) ;
                                            ALTER TABLE [dbo].[brand_default] ADD  CONSTRAINT [PK_brand_default] PRIMARY KEY(brand_posid) 
                                            ALTER TABLE [dbo].[brand_default] ADD  CONSTRAINT [DF_brand_default_brand_utime]  DEFAULT (getdate()) FOR [brand_utime];
                                            ALTER TABLE [dbo].[brand_default] ADD  CONSTRAINT [DF_brand_default_createtime]  DEFAULT (getdate()) FOR [createtime];
                                            ALTER TABLE [dbo].[brand_default] ADD  CONSTRAINT [DF_brand_default_lastupdatetime]  DEFAULT (getdate()) FOR [lastupdatetime];";

            //area_faid
            var selectAreaSql = @"select table_name from information_schema.tables where TABLE_NAME='area_default'";
            var createAreaSql = @" CREATE TABLE [dbo].[area_default](
	                                            [area_name] [varchar](50) NOT NULL,
	                                            [area_level] [int] NULL,
	                                            [area_faid] [varchar](50) NULL,
	                                            [area_seq] [int] NULL,
	                                            [area_status] [int] NOT NULL,
	                                            [area_subject] [varchar](50) NOT NULL,
	                                            [area_posid] [varchar](50) NOT NULL,
	                                            [createtime] [datetime] NULL,
	                                            [lastupdatetime] [datetime] NULL) ;
                                            ALTER TABLE [dbo].[area_default] ADD CONSTRAINT [PK_area_default] PRIMARY KEY(area_posid);
                                            ALTER TABLE [dbo].[area_default] ADD  CONSTRAINT [DF_area_default_area_level]  DEFAULT ((1)) FOR [area_level];
                                            ALTER TABLE [dbo].[area_default] ADD  CONSTRAINT [DF_area_default_area_faid]  DEFAULT ((-1)) FOR [area_faid];
                                            ALTER TABLE [dbo].[area_default] ADD  CONSTRAINT [DF_area_default_createtime]  DEFAULT (getdate()) FOR [createtime];
                                            ALTER TABLE [dbo].[area_default] ADD  CONSTRAINT [DF_area_default_lastupdatetime]  DEFAULT (getdate()) FOR [lastupdatetime];";

            var selectDeptSql = @"select table_name from information_schema.tables where TABLE_NAME='dept_default'";
            var createDeptSql = @" CREATE TABLE [dbo].[dept_default](
	                                            [dept_alias] [varchar](50) NOT NULL,
	                                            [dept_name] [varchar](50) NOT NULL,
	                                            [dept_bid] [varchar](50) NOT NULL,
	                                            [dept_seq] [int] NOT NULL,
	                                            [dept_status] [int] NOT NULL,
	                                            [dept_sno] [varchar](50) NOT NULL,
	                                            [dept_sid] [varchar](50) NULL,
	                                            [dept_posid] [varchar](50) NOT NULL,
	                                            [createtime] [datetime] NULL,
	                                            [lastupdatetime] [datetime] NULL);
                                            ALTER TABLE [dbo].[dept_default] ADD CONSTRAINT [PK_dept_default] PRIMARY KEY(dept_posid);
                                            ALTER TABLE [dbo].[dept_default] ADD  CONSTRAINT [DF_dept_default_createtime]  DEFAULT (getdate()) FOR [createtime];
                                            ALTER TABLE [dbo].[dept_default] ADD  CONSTRAINT [DF_dept_default_lastupdatetime]  DEFAULT (getdate()) FOR [lastupdatetime];";

            var selectSyncInfoSql = @"select table_name from information_schema.tables where table_name='syncInfo'";
            var createSyncInfoSql = @"CREATE TABLE [dbo].[syncinfo](
	                                    [tablename] [varchar](50) NOT NULL,
	                                    [lastupdatetime] [datetime] NOT NULL,
	                                    [issynced] [bit] NOT NULL,
                                        [idlist] [varchar](8000) NOT NULL);
                                    ALTER TABLE [dbo].[syncinfo] ADD  CONSTRAINT [PK_syncinfo] PRIMARY KEY(tablename);
                                    ALTER TABLE [dbo].[syncinfo] ADD  CONSTRAINT [DF_syncinfo_lastupdatetime]  DEFAULT ('1900-1-1 00:00:00') FOR [lastupdatetime];
                                    ALTER TABLE [dbo].[syncinfo] ADD  CONSTRAINT [DF_syncinfo_issynced]  DEFAULT ((0)) FOR [issynced];
                                    ALTER TABLE [dbo].[syncinfo] ADD  CONSTRAINT [DF_syncinfo_idlist]  DEFAULT ('') FOR [idlist];";

            var selectSyncFailDataSql = @"select table_name from information_schema.tables where table_name='syncfaildata'";
            var createSyncFailDataSql = @"CREATE TABLE [dbo].[syncfaildata](
	                                        [tablename] [varchar](50) NOT NULL,
	                                        [idlist] [varchar](500) NOT NULL,
	                                        [failtype] [int] NOT NULL,
	                                        [failmessage] [varchar](500) NULL,
	                                        [createtime] [datetime] NOT NULL);
                                        ALTER TABLE [dbo].[syncfaildata] ADD  CONSTRAINT [DF_syncfaildata_failtype]  DEFAULT ((0)) FOR [failtype];
                                        ALTER TABLE [dbo].[syncfaildata] ADD  CONSTRAINT [DF_syncfaildata_createdatiem]  DEFAULT (getdate()) FOR [createtime];";
            #endregion

            //建表 
            using (var db = new SqlConnection(connectionString))
            {
                var brand = db.ExecuteScalar(selectBrandSql)?.ToString();
                if (brand == null)
                {
                    var result = db.Execute(createBrandSql);
                }
                var area = db.ExecuteScalar(selectAreaSql)?.ToString();
                if (area == null)
                {
                    db.Execute(createAreaSql);
                }
                var dept = db.ExecuteScalar(selectDeptSql)?.ToString();
                if (dept == null)
                {
                    db.Execute(createDeptSql);
                }
                var syncInfo = db.ExecuteScalar(selectSyncInfoSql)?.ToString();
                if (syncInfo == null)
                {
                    db.Execute(createSyncInfoSql);
                }
                var syncFailData = db.ExecuteScalar(selectSyncFailDataSql)?.ToString();
                if (syncFailData == null)
                {
                    db.Execute(createSyncFailDataSql);
                }
            }

            SetButtonEnable();
            //初始化数据
            InitData();

            #region 添加两个定时器,半小时后启动,基础定时器每日凌晨1点到两点之间同步两次,订单定时器5分钟同步一次
            timerBase = new System.Threading.Timer(o =>
               {
                   if (DateTime.Now.Hour == 1)
                   {
                       btn_Brand_Click(null, null);
                       btn_Area_Click(null, null);
                       btn_Dept_Click(null, null);
                       btn_Store_Click(null, null);
                       btn_MealTime_Click(null, null);
                       btn_DishType_Click(null, null);
                       btn_Unit_Click(null, null);
                       btn_Dish_Click(null, null);
                   }
               }, null, 300000, 1800000);

            timerOrder = new System.Threading.Timer(o =>
              {
                  btn_OrderMain_Click(null, null);
              }, null, 60000, 60000);
            timerRecordLog = new System.Threading.Timer(o =>
              {
                  if (DateTime.Now.Hour == 1)
                  {
                      //记录日志
                      Directory.CreateDirectory(currentDirectory + "\\Log");
                      var fileName = currentDirectory + $"\\Log\\{DateTime.Now.ToString("yyyyMMdd") + ".txt"}";
                      if (!File.Exists(fileName))
                      {
                          var fs = File.Create(fileName);
                          fs.Close();
                      }
                      if (File.Exists(fileName))
                      {
                          var content = "";
                          rtxt_message.Invoke(new Action(() => content = rtxt_message.Text.Replace("\n", "\r\n")));
                          var sw = new StreamWriter(fileName, true, Encoding.UTF8);
                          sw.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\r\n" + content);
                          sw.Close();
                      }
                      rtxt_message.Invoke(new Action(() => rtxt_message.Clear()));
                  }
              }, null, 1800000, 3600000);
            #endregion
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
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品牌数据同步中,请耐心等待......\r\n"));
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
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品牌无数据同步\r\n"));
                        brand_sync = false;
                        return;
                    }
                    bool flag = true;
                    //int addCount = 0, updateCount = 0, failCount = 0;

                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length != 1)
                            {
                                SyncFailData.DeleteFailData("brand_default", item.IdList);
                                continue;
                            }
                            var itemData = Brand_default.GetById(Convert.ToInt32(idList[0]));
                            if (itemData == null)
                            {
                                SyncFailData.DeleteFailData("brand_default", idList[0]);
                                continue;
                            }
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
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品牌数据新增失败,原因:{result_add?.Msg}\r\n"));
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
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品牌数据修改失败,原因:{result_update?.Msg}\r\n"));
                                }
                                else
                                {
                                    SyncFailData.DeleteFailData("brand_default", item.IdList);
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
                        if (item.CreateTime == item.LastUpdateTime || item.CreateTime >= getLastSyncInfo.LastUpdateTime)
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_addbrand, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品牌数据新增失败,原因:{result_add?.Msg}\r\n"));
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
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品牌数据修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "brand_default", IdList = data.CbId.ToString(), FailType = 0, FailMessage = result_update?.Msg });
                                flag = flag && false;
                            }
                        }
                    }

                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("brand_default");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品牌数据同步成功\r\n"));
                    }


                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInBrand:{ex.Message}\r\n"));
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
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n区域数据同步中,请耐心等待......\r\n"));
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
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n区域无数据同步\r\n"));
                        area_sync = false;
                        return;
                    }

                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length != 1)
                            {
                                SyncFailData.DeleteFailData("area_default", item.IdList);
                                continue;
                            }
                            var itemData = Area_default.GetById(Convert.ToInt32(idList[0]));
                            if (itemData == null)
                            {
                                SyncFailData.DeleteFailData("area_default", idList[0]);
                                continue;
                            }
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
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 区域数据新增失败,原因:{result_add?.Msg}\r\n"));
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
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 区域数据修改失败,原因:{result_update?.Msg}\r\n"));
                                }
                                else
                                {
                                    SyncFailData.DeleteFailData("area_default", item.IdList);
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
                        if (item.CreateTime == item.LastUpdateTime || item.CreateTime >= getLastSyncInfo.LastUpdateTime)
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_addarea, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 区域数据新增失败,原因:{result_add?.Msg}\r\n"));
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
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 区域数据修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "area_default", IdList = data.CaId.ToString(), FailType = 0, FailMessage = result_update?.Msg });
                                flag = flag && false;
                            }
                        }
                    }

                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("area_default");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n区域数据同步成功\r\n"));
                    }
                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInArea:{ex.Message}\r\n"));
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
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n门店数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("store");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"store not in syncinfo\r\n"));
                        store_sync = false;
                        return;
                    }
                    var getData = DA_FD.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    var getFailedData = SyncFailData.GetListByTableName("store");
                    var idListForAdd = getLastSyncInfo.IdList?.Trim() ?? "";
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n门店无数据同步\r\n"));
                        store_sync = false;
                        return;
                    }
                    int addCount = 0, updateCount = 0, failCount = 0;
                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length != 1)
                            {
                                SyncFailData.DeleteFailData("store", item.IdList);
                                continue;
                            }
                            var itemData = DA_FD.GetById(Convert.ToInt32(idList[0]));
                            if (itemData == null)
                            {
                                SyncFailData.DeleteFailData("store", idList[0]);
                                continue;
                            }
                            var data = new Sls_shop()
                            {
                                ShopName = itemData.FD.Trim(),
                                ShopCode = itemData.FDDM.Trim(),
                                Subject = itemData.FDDM.Trim(),
                                Status = itemData.QYBJ == 1 ? 1 : -1,
                                AId = ConfigInfo.Area_posid,
                                BId = ConfigInfo.Brand_posid,
                                SType = 4,
                                //SlsId="2",
                                ShopAdd = itemData.DZ ?? "",
                                CsId = itemData.FDNM.ToString()
                            };
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            if (getLastSyncInfo.IdList.Contains(itemData.FDNM.ToString().Trim()))
                            {
                                var result_update = Common.Post(ConfigInfo.Apiurl_editstore, paramData);
                                if (result_update == null || !result_update.Success)
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 门店数据(storeid:{itemData.FDNM})修改失败,原因:{result_update?.Msg}\r\n"));
                                    failCount++;
                                    continue;
                                }
                                updateCount++;
                            }
                            else
                            {
                                var result_add = Common.Post(ConfigInfo.Apiurl_addstore, paramData);
                                if (result_add == null || !result_add.Success)
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 门店数据(storeid:{itemData.FDNM})新增失败,原因:{result_add?.Msg}\r\n"));
                                    failCount++;
                                    continue;
                                }
                                if (string.IsNullOrEmpty(idListForAdd))
                                {
                                    idListForAdd += itemData.FDNM.ToString().Trim();
                                }
                                else
                                {
                                    idListForAdd += ";" + itemData.FDNM.ToString().Trim();
                                }
                                addCount++;
                            }
                        }
                    }

                    //同步数据
                    foreach (var item in getData)
                    {
                        if (getLastSyncInfo.IdList.Contains(item.FDNM.ToString().Trim()) && getLastSyncInfo.LastUpdateTime > item.XGSJ)
                        {
                            continue;
                        }
                        var data = new Sls_shop()
                        {
                            ShopName = item.FD.Trim(),
                            ShopCode = item.FDDM.Trim(),
                            Subject = item.FDDM.Trim(),
                            Status = item.QYBJ == 1 ? 1 : -1,
                            AId = ConfigInfo.Area_posid,
                            BId = ConfigInfo.Brand_posid,
                            SType = 4,
                            //SlsId="2",
                            ShopAdd = item.DZ ?? "",
                            CsId = item.FDNM.ToString()
                        };
                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                        if (getLastSyncInfo.IdList.Contains(item.FDNM.ToString().Trim()))
                        {
                            var result_update = Common.Post(ConfigInfo.Apiurl_editstore, paramData);
                            if (result_update == null || !result_update.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 门店数据(storeid:{item.FDNM})修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData()
                                {
                                    TableName = "store",
                                    IdList = item.FDNM.ToString().Trim(),
                                    FailType = 1,
                                    FailMessage = result_update?.Msg
                                });
                                failCount++;
                                continue;
                            }
                            updateCount++;
                        }
                        else
                        {
                            var result_add = Common.Post(ConfigInfo.Apiurl_addstore, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 门店数据(storeid:{item.FDNM})新增失败,原因:{result_add?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData()
                                {
                                    TableName = "store",
                                    IdList = item.FDNM.ToString().Trim(),
                                    FailType = 0,
                                    FailMessage = result_add?.Msg
                                });
                                failCount++;
                                continue;
                            }
                            if (string.IsNullOrEmpty(idListForAdd))
                            {
                                idListForAdd += item.FDNM.ToString().Trim();
                            }
                            else
                            {
                                idListForAdd += ";" + item.FDNM.ToString().Trim();
                            }
                            addCount++;
                        }
                    }

                    if (addCount == 0 && updateCount == 0 && failCount == 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n门店无数据同步\r\n"));
                        store_sync = false;
                        return;
                    }
                    else
                    {
                        var successMessage = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n门店数据同步成功";
                        if (addCount > 0)
                        {
                            successMessage += $",新增了{addCount}条数据";
                        }
                        if (updateCount > 0)
                        {
                            successMessage += $",修改了{updateCount}条数据";
                        }
                        if (failCount > 0)
                        {
                            successMessage += $",失败了{failCount}条数据";
                        }
                        SyncInfo.UpdateByTableName("store", idList: idListForAdd);
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += successMessage + $".\r\n"));
                    }
                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInStore:{ex.Message}\r\n"));
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
                    return;
                }
                meal_time_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n餐段数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("meal_time");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"meal_time not in syncinfo\r\n"));
                        meal_time_sync = false;
                        return;
                    }
                    if (getLastSyncInfo.IsSynced)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 餐段已经同步过了且只同步一次\r\n"));
                        meal_time_sync = false;
                        return;
                    }
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
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 餐段数据新增失败,原因:{result_add?.Msg}\r\n"));
                    }
                    else
                    {
                        SyncInfo.UpdateByTableName("meal_time", 1);
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n餐段数据同步成功,新增了1条数据\r\n"));
                    }
                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInMeal_Time:{ex.Message}\r\n"));
                }
                finally
                {
                    meal_time_sync = false;
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
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n部门数据同步中,请耐心等待......\r\n"));
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
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n部门无数据同步\r\n"));
                        dept_sync = false;
                        return;
                    }
                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length != 1)
                            {
                                SyncFailData.DeleteFailData("Dept_default", item.IdList);
                                continue;
                            }
                            var itemData = Dept_default.GetById(Convert.ToInt32(idList[0]));
                            if (itemData == null)
                            {
                                SyncFailData.DeleteFailData("Dept_default", idList[0]);
                                continue;
                            }
                            var data = new C_department()
                            {
                                Department = itemData.Dept_name,
                                Alias = itemData.Dept_alias,
                                Status = itemData.Dept_status,
                                Seq = itemData.Dept_seq,
                                Sno = itemData.Dept_sno,
                                BId = itemData.Dept_bid,
                                Sid = itemData.Dept_sid,
                                CdmId = itemData.Dept_posid
                            };
                            if (item.FailType == 0)
                            {
                                var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                                if (result_add == null || !result_add.Success)
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 部门数据新增失败,原因:{result_add?.Msg}\r\n"));
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
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 部门数据修改失败,原因:{result_update?.Msg}\r\n"));
                                }
                                else
                                {
                                    SyncFailData.DeleteFailData("dept_default", item.IdList);
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
                            Seq = item.Dept_seq,
                            Sno = item.Dept_sno,
                            BId = item.Dept_bid,
                            Sid = item.Dept_sid,
                            CdmId = item.Dept_posid
                        };
                        if (item.CreateTime == item.LastUpdateTime || item.CreateTime >= getLastSyncInfo.LastUpdateTime)
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_adddept, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 部门数据新增失败,原因:{result_add?.Msg}\r\n"));
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
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 部门数据修改失败,原因:{result_update?.Msg}\r\n"));
                                SyncFailData.InsertFailDate(new SyncFailData() { TableName = "dept_default", IdList = data.CdmId.ToString(), FailType = 0, FailMessage = result_update?.Msg });
                                flag = flag && false;
                            }
                        }
                    }

                    if (flag)
                    {
                        SyncInfo.UpdateByTableName("dept_default");
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n部门数据同步成功\r\n"));
                    }

                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInDept:{ex.Message}\r\n"));
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
                    return;
                }
                dish_type_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品项类型数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("dish_type");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dish_type not in syncinfo\r\n"));
                        dish_type_sync = false;
                        return;
                    }
                    var getData = DA_SPLB.GetListByLastTime(getLastSyncInfo.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    var getFailedData = SyncFailData.GetListByTableName("dish_type");
                    var now = DateTime.Now;
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品项类型无数据同步\r\n"));
                        dish_type_sync = false;
                        return;
                    }
                    int addCount = 0, /*updateCoutnt = 0,*/ failCount = 0;
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
                        //var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                        //var result_update = Common.Post(ConfigInfo.Apiurl_editdishtype, paramData_update);
                        //if (result_update == null || !result_update.Success)
                        //{
                        //    failCount++;
                        //    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 品项类型数据修改失败,原因:{result_update?.Msg}\r\n"));
                        //}
                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                        var result_add = Common.Post(ConfigInfo.Apiurl_adddishtype, paramData);
                        if (result_add == null || !result_add.Success)
                        {
                            failCount++;
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品项类型数据新增失败(第{failCount}条),原因:{result_add?.Msg}\r\n"));
                            continue;
                        }
                        addCount++;
                    }
                    SyncInfo.UpdateByTableName("dish_type");
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品项类型数据同步成功,本次新增{addCount}条数据,失败{failCount}条\r\n"));

                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInDish_Type:{ex.Message}\r\n"));
                }
                finally
                {
                    dish_type_sync = false;
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
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n单位数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("unit");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"unit not in syncinfo\r\n"));
                        unit_sync = false;
                        return;
                    }
                    //if (getLastSyncInfo.IsSynced)
                    //{
                    //    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"对不起! 单位已经同步过了且只同步一次\r\n"));
                    //    unit_sync = false;
                    //    return;
                    //}
                    var getData = DA_JLDW.GetList();
                    var getFailedData = SyncFailData.GetListByTableName("unit");
                    var now = DateTime.Now;
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 单位无数据同步\r\n"));
                        unit_sync = false;
                        return;
                    }
                    bool flag = true;
                    int addCount = 0/*, updateCount = 0, failCount = 0*/;
                    foreach (var item in getData)
                    {
                        var data = new O_dish_unit()
                        {
                            DuId = item.JLDWNM,
                            DishUnit = item.JLDW.Trim(),
                            UTime = now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                        var result_add = Common.Post(ConfigInfo.Apiurl_addunit, paramData);
                        if (result_add == null || !result_add.Success)
                        {
                            rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 单位数据更新失败,原因:{result_add?.Msg}\r\n"));
                            SyncFailData.InsertFailDate(new SyncFailData() { TableName = "unit", IdList = data.DuId.ToString(), FailType = 0, FailMessage = result_add?.Msg });
                            flag = flag && false;
                            continue;
                        }
                        addCount++;
                    }
                    SyncInfo.UpdateByTableName("unit", 1);
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n单位数据同步成功,更新了{addCount}条数据\r\n"));

                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInUnit:{ex.Message}\r\n"));
                }
                finally
                {
                    unit_sync = false;
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
                    return;
                }
                dish_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品项数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("dish");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"dish not in syncinfo\r\n"));
                        dish_sync = false;
                        return;
                    }
                    var getFailedData = SyncFailData.GetListByTableName("dish");
                    var getData = DA_SP.GetListByLastTime(getLastSyncInfo.LastUpdateTime);
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品项无数据同步\r\n"));
                        dish_sync = false;
                        return;
                    }

                    int addCount = 0, updateCount = 0, failCount = 0;
                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length != 1)
                            {
                                SyncFailData.DeleteFailData("dish", item.IdList);
                                continue;
                            }
                            var itemData = DA_SP.GetById(idList[0].Trim());
                            if (itemData == null)
                            {
                                SyncFailData.DeleteFailData("dish", idList[0]);
                                continue;
                            }
                            var data = new DishInfo()
                            {
                                Dish = new O_dish()
                                {
                                    DId = itemData.SPNM,
                                    DkId = itemData.SPXLNM,
                                    SNo = itemData.SPZJM,
                                    BId = ConfigInfo.Brand_posid,
                                    CTime = Common.IntToDateTime(itemData.JDRQ).ToString("yyyy-MM-dd HH:mm:ss"),
                                    DmId = ConfigInfo.Dept_posid,
                                    Dish = itemData.SP,
                                    Alias = itemData.SPZJM,
                                    Status = itemData.QYBJ == 1 ? 1 : 2,
                                    Seq = itemData.XH
                                },
                                MenuDish = new List<O_menudish>()
                                            { new O_menudish()
                                                {
                                                    DId=itemData.SPNM.ToString(),
                                                    DuId=DA_JLDW.GetIdByUnitName(itemData.JLDW?.Trim()),
                                                    Price=itemData.LSJ
                                                }
                                            }
                            };
                            if (item.FailType == 0)
                            {
                                var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                                var result_add = Common.Post(ConfigInfo.Apiurl_adddish, paramData);
                                if (result_add == null || !result_add.Success)
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品项数据新增失败(第{failCount + 1}条失败),原因:{result_add?.Msg}\r\n"));
                                    failCount++;
                                    continue;
                                }
                                else
                                {
                                    SyncFailData.DeleteFailData("dish", item.IdList);
                                    addCount++;
                                }
                            }
                            else
                            {
                                var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                                var result_update = Common.Post(ConfigInfo.Apiurl_editdish, paramData_update);
                                if (result_update == null || !result_update.Success)
                                {
                                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品项数据修改失败(第{failCount + 1}条失败),原因:{result_update?.Msg}\r\n"));
                                    failCount++;
                                    continue;
                                }
                                else
                                {
                                    SyncFailData.DeleteFailData("dish", item.IdList);
                                    updateCount++;
                                }
                            }
                        }
                    }

                    //同步数据
                    foreach (var item in getData)
                    {
                        var data = new DishInfo()
                        {
                            Dish = new O_dish()
                            {
                                DId = item.SPNM,
                                DkId = item.SPXLNM,
                                SNo = item.SPZJM,
                                BId = ConfigInfo.Brand_posid,
                                CTime = Common.IntToDateTime(item.JDRQ).ToString("yyyy-MM-dd HH:mm:ss"),
                                DmId = ConfigInfo.Dept_posid,
                                Dish = item.SP,
                                Alias = item.SPZJM,
                                Status = item.QYBJ == 1 ? 1 : 2,
                                Seq = item.XH
                            },
                            MenuDish = new List<O_menudish>()
                                { new O_menudish()
                                    {
                                        DId=item.SPNM.ToString(),
                                        DuId=DA_JLDW.GetIdByUnitName(item.JLDW?.Trim()),
                                        Price=item.LSJ
                                    }
                                }
                        };

                        if (item.XGRQ.ToString("yyyyMMdd").CompareTo(Common.IntToDateTime(item.JDRQ).ToString("yyyyMMdd")) > 0)
                        {
                            var paramData_update = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_update = Common.Post(ConfigInfo.Apiurl_editdish, paramData_update);
                            if (result_update == null || !result_update.Success)
                            {
                                SyncFailData.InsertFailDate(new SyncFailData()
                                {
                                    TableName = "dish",
                                    IdList = item.SPNM.ToString(),
                                    FailType = 1,
                                    FailMessage = result_update?.Msg
                                });
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品项数据修改失败(第{failCount + 1}条失败),原因:{result_update?.Msg}\r\n"));
                                failCount++;
                                continue;
                            }
                            updateCount++;
                        }
                        else
                        {
                            var paramData = $"arr={JsonConvert.SerializeObject(data)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_adddish, paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                SyncFailData.InsertFailDate(new SyncFailData()
                                {
                                    TableName = "dish",
                                    IdList = item.SPNM.ToString(),
                                    FailType = 0,
                                    FailMessage = result_add?.Msg
                                });
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 品项数据新增失败(第{failCount + 1}条失败),原因:{result_add?.Msg}\r\n"));
                                failCount++;
                                continue;
                            }
                            addCount++;
                        }
                    }

                    var successMessage = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n品项数据同步成功";
                    if (addCount > 0)
                    {
                        successMessage += $",新增了{addCount}条数据";
                    }
                    if (updateCount > 0)
                    {
                        successMessage += $",修改了{updateCount}";
                    }
                    if (failCount > 0)
                    {
                        successMessage += $",失败了{failCount}条数据";
                    }

                    SyncInfo.UpdateByTableName("dish");
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += successMessage + $".\r\n"));
                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInDish:{ex.Message}\r\n"));
                }
                finally
                {
                    dish_sync = false;
                }
            });
        }

        bool order_sync = false;//true:订单 正在同步中
        private void btn_OrderMain_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (order_sync)
                {
                    MessageBox.Show("对不起! 订单数据同步中,请勿重复点击!", "Tips");
                    return;
                }
                order_sync = true;
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n订单数据同步中,请耐心等待......\r\n"));
                try
                {
                    var getLastSyncInfo = SyncInfo.GetInfoByTableName("order");
                    if (getLastSyncInfo == null)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"order not in syncinfo\r\n"));
                        order_sync = false;
                        return;
                    }
                    var getFailedData = SyncFailData.GetListByTableName("order");
                    var getData = XS_PZ_ZB.GetListByLastTime(getLastSyncInfo.LastUpdateTime);
                    if (getFailedData.Count <= 0 && getData.Count <= 0)
                    {
                        rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n订单无数据同步\r\n"));
                        order_sync = false;
                        return;
                    }

                    //同步以往失败的数据
                    if (getFailedData.Count > 0)
                    {
                        var idLists = new List<DeleteObject>();
                        var failDataLists = new List<OrderInfoList>();
                        foreach (var item in getFailedData)
                        {
                            var idList = item.IdList.Split(';');
                            if (idList.Length != 1)
                            {
                                SyncFailData.DeleteFailData("order", item.IdList);
                                continue;
                            }
                            var failDataInfo = XS_PZ_ZB.GetById(idList[0]);
                            if (failDataInfo == null)
                            {
                                SyncFailData.DeleteFailData("order", item.IdList);
                                continue;
                            }
                            var dataTemp = OrderInfo.GetData(failDataInfo, ConfigInfo.Dept_posid);

                            var ddd = failDataLists.FirstOrDefault(t => t.Sid == failDataInfo.FDNM);
                            if (ddd != null)
                            {
                                ddd.OrderInfos.Add(dataTemp);
                                ddd.IdLists.Add(new DeleteObject() { TableName = "order", IdList = item.IdList });
                            }
                            else
                            {
                                failDataLists.Add(new OrderInfoList()
                                {
                                    Sid = failDataInfo.FDNM,
                                    Date = Common.IntToDateTime(failDataInfo.CZRQ_XS, failDataInfo.CZSJ_XS).ToString("yyyy-MM-dd"),
                                    OrderInfos = new List<OrderInfo>() { dataTemp },
                                    IdLists = new List<DeleteObject>() { new DeleteObject() { TableName = "order", IdList = item.IdList } }
                                });
                            }
                        }
                        foreach (var failDatas in failDataLists)
                        {
                            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                            var paramDataOld = $"arr={JsonConvert.SerializeObject(failDatas.OrderInfos, timeConverter)}";
                            var result_Old_add = Common.Post(ConfigInfo.Apiurl_addordermain + $"&sid={failDatas.Sid}&day={failDatas.Date}", paramDataOld);
                            if (result_Old_add == null || !result_Old_add.Success)
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 订单数据(门店id:{failDatas.Sid} 日期:{failDatas.Date}(历史失败的数据))新增失败(共{failDatas.OrderInfos.Count}条失败),原因:{result_Old_add?.Msg}\r\n"));
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n订单数据(门店id:{failDatas.Sid} 日期:{failDatas.Date}(历史失败的数据))同步成功新增{failDatas.OrderInfos.Count}条订单数据\r\n"));
                                SyncFailData.DeleteFailDatas("order", failDatas.IdLists);
                            }
                        }
                    }

                    //同步数据
                    if (getData.Count > 0)
                    {
                        var datas = OrderInfo.GetListData(getData, ConfigInfo.Dept_posid);
                        foreach (var data in datas)
                        {


                            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                            var paramData = $"arr={JsonConvert.SerializeObject(data.OrderInfos, timeConverter)}";
                            var result_add = Common.Post(ConfigInfo.Apiurl_addordermain + $"&sid={data.Sid}&day={data.Date}", paramData);
                            if (result_add == null || !result_add.Success)
                            {
                                var failDataList = new List<SyncFailData>();
                                var date = DateTime.Now;
                                foreach (var item in data.OrderInfos)
                                {
                                    failDataList.Add(new SyncFailData()
                                    {
                                        TableName = "order",
                                        IdList = item.O_Order.OId,
                                        FailType = 0,
                                        FailMessage = result_add?.Msg,
                                        CreateTime = date
                                    });
                                }
                                SyncFailData.InsertFailDates(failDataList);
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n对不起! 订单数据(门店id:{data.Sid} 日期:{data.Date})新增失败(共{data.OrderInfos.Count}条),原因:{result_add?.Msg}\r\n"));
                            }
                            else
                            {
                                rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\n订单数据(门店id:{data.Sid} 日期:{data.Date})同步成功新增{data.OrderInfos.Count}条订单数据\r\n"));
                            }
                        }
                        SyncInfo.UpdateByTableName("order");
                    }

                }
                catch (Exception ex)
                {
                    rtxt_message.Invoke(new Action(() => rtxt_message.Text += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")}\r\nExceptionInfoInOrder:{ex.Message}\r\n"));
                }
                finally
                {
                    order_sync = false;
                }
            });
        }

        private void btn_OrderDetail_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                MessageBox.Show("Test");
                rtxt_message.Invoke(new Action(() => rtxt_message.Text += "test"));
            }
            );

        }

        private void btn_User_Click(object sender, EventArgs e)
        {

        }

        private void btn_Payway_Click(object sender, EventArgs e)
        {

        }

        private void rtxt_message_TextChanged(object sender, EventArgs e)
        {
            //自动滚动滚动条
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
            var brand_status = Convert.ToInt32(ConfigInfo.Brand_status);
            var brand_subject = ConfigInfo.Brand_subject;
            var brand_posid = ConfigInfo.Brand_posid;
            var brand_seq = ConfigInfo.Brand_seq;

            var area_name = ConfigInfo.Area_name;
            var area_level = Convert.ToInt32(ConfigInfo.Area_level);
            var area_faid = ConfigInfo.Area_faid;
            var area_seq = ConfigInfo.Area_seq;
            var area_status = Convert.ToInt32(ConfigInfo.Area_status);
            var area_subject = ConfigInfo.Area_subject;
            var area_posid = ConfigInfo.Area_posid;

            var dept_name = ConfigInfo.Dept_name;
            var dept_alias = ConfigInfo.Dept_alias;
            var dept_status = Convert.ToInt32(ConfigInfo.Dept_status);
            var dept_sequence = Convert.ToInt32(ConfigInfo.Dept_sequence);
            var dept_subject = ConfigInfo.Dept_subject;
            var dept_brand = ConfigInfo.Dept_brand;
            var dept_sid = ConfigInfo.Dept_sid;
            var dept_posid = ConfigInfo.Dept_posid;

            bool brandFlag = false, areaFlag = false, deptFlag = false;
            if (!string.IsNullOrEmpty(brand_name) && !string.IsNullOrEmpty(ConfigInfo.Brand_status) && !string.IsNullOrEmpty(brand_subject) && !string.IsNullOrEmpty(brand_posid) && !string.IsNullOrEmpty(brand_seq.ToString()))
            {
                brandFlag = true;
            }
            if (!string.IsNullOrEmpty(area_name) && !string.IsNullOrEmpty(ConfigInfo.Area_level) && !string.IsNullOrEmpty(ConfigInfo.Area_status) && !string.IsNullOrEmpty(area_subject) && !string.IsNullOrEmpty(area_posid))
            {
                areaFlag = true;
            }
            if (!string.IsNullOrEmpty(dept_name) && !string.IsNullOrEmpty(dept_alias) && !string.IsNullOrEmpty(ConfigInfo.Dept_status) && !string.IsNullOrEmpty(ConfigInfo.Dept_sequence) && !string.IsNullOrEmpty(dept_subject) && !string.IsNullOrEmpty(dept_brand.ToString()) && !string.IsNullOrEmpty(dept_posid.ToString()))
            {
                deptFlag = true;
            }
            if (brandFlag || areaFlag || deptFlag)
            {
                #region Sql语句
                var selectBrandSql = @"select top 1 brand_name,brand_status,brand_subject,brand_posid from brand_default where brand_posid=@brand_posid";
                var insertBrandSql = @"insert into brand_default(brand_name,brand_status,brand_subject,brand_posid,brand_seq,createtime,lastupdatetime) values(@brand_name,@brand_status,@brand_subject,@brand_posid,@brand_seq,@createtime,@lastupdatetime)";
                var updateBranSql = @"update brand_default set brand_name=@brand_name,brand_status=@brand_status,brand_subject=@brand_subject,brand_seq=@brand_seq,lastupdatetime=@lastupdatetime where brand_posid=@brand_posid";
                var paramBrand = new DynamicParameters();
                var paramBrandSelect = new DynamicParameters();
                var paramBrandUpdate = new DynamicParameters();
                paramBrandSelect.Add("brand_posid", brand_posid);
                var dateTime = DateTime.Now;
                paramBrand.Add("brand_name", brand_name);
                paramBrand.Add("brand_status", brand_status);
                paramBrand.Add("brand_subject", brand_subject);
                paramBrand.Add("brand_posid", brand_posid);
                paramBrand.Add("brand_seq", brand_seq);
                paramBrand.Add("createtime", dateTime);
                paramBrand.Add("lastupdatetime", dateTime);

                paramBrandUpdate.Add("brand_name", brand_name);
                paramBrandUpdate.Add("brand_status", brand_status);
                paramBrandUpdate.Add("brand_subject", brand_subject);
                paramBrandUpdate.Add("brand_posid", brand_posid);
                paramBrandUpdate.Add("brand_seq", brand_seq);
                paramBrandUpdate.Add("lastupdatetime", dateTime);

                var selectAreaSql = @"select top 1 area_name,area_level,area_faid,area_seq,area_status,area_subject,area_posid from area_default where area_posid=@area_posid";
                var insertAreaSql = @"insert into area_default(area_name,area_level,area_faid,area_seq,area_status,area_subject,area_posid,createtime,lastupdatetime) values(@area_name,@area_level,@area_faid,@area_seq,@area_status,@area_subject,@area_posid,@createtime,@lastupdatetime)";
                var updateAreaSql = @"update area_default set area_name=@area_name,area_level=@area_level,area_faid=@area_faid,area_seq=@area_seq,area_status=@area_status,area_subject=@area_subject,lastupdatetime=@lastupdatetime where area_posid=@area_posid";
                var paramArea = new DynamicParameters();
                var paramAreaSelect = new DynamicParameters();
                var paramAreaUpdate = new DynamicParameters();
                paramAreaSelect.Add("area_posid", area_posid);
                paramArea.Add("area_name", area_name);
                paramArea.Add("area_level", area_level);
                paramArea.Add("area_faid", area_faid);
                paramArea.Add("area_seq", area_seq);
                paramArea.Add("area_status", area_status);
                paramArea.Add("area_subject", area_subject);
                paramArea.Add("area_posid", area_posid);
                paramArea.Add("createtime", dateTime);
                paramArea.Add("lastupdatetime", dateTime);

                paramAreaUpdate.Add("area_name", area_name);
                paramAreaUpdate.Add("area_level", area_level);
                paramAreaUpdate.Add("area_faid", area_faid);
                paramAreaUpdate.Add("area_seq", area_seq);
                paramAreaUpdate.Add("area_status", area_status);
                paramAreaUpdate.Add("area_subject", area_subject);
                paramAreaUpdate.Add("area_posid", area_posid);
                paramAreaUpdate.Add("lastupdatetime", dateTime);

                var selectDeptSql = @"select top 1 dept_name,dept_alias,dept_status,dept_seq,dept_sno,dept_bid,dept_posid from dept_default where dept_posid=@dept_posid";
                var insertDeptSql = @"insert into dept_default(dept_name,dept_alias,dept_status,dept_seq,dept_sno,dept_bid,dept_sid,dept_posid,createtime,lastupdatetime) values(@dept_name,@dept_alias,@dept_status,@dept_seq,@dept_sno,@dept_bid,@dept_sid,@dept_posid,@createtime,@lastupdatetime)";
                var updateDeptSql = @"update dept_default set dept_name=@dept_name,dept_alias=@dept_alias,dept_status=@dept_status,dept_seq=@dept_seq,dept_sno=@dept_sno,dept_bid=@dept_bid,dept_sid=@dept_sid,lastupdatetime=@lastupdatetime where dept_posid=@dept_posid";
                var paramDept = new DynamicParameters();
                var paramDeptSelect = new DynamicParameters();
                var paramDeptUpdate = new DynamicParameters();
                paramDeptSelect.Add("dept_posid", dept_posid);
                paramDept.Add("dept_name", dept_name);
                paramDept.Add("dept_alias", dept_alias);
                paramDept.Add("dept_status", dept_status);
                paramDept.Add("dept_seq", dept_sequence);
                paramDept.Add("dept_sno", dept_subject);
                paramDept.Add("dept_bid", dept_brand);
                paramDept.Add("dept_sid", dept_sid);
                paramDept.Add("dept_posid", dept_posid);
                paramDept.Add("createtime", dateTime);
                paramDept.Add("lastupdatetime", dateTime);

                paramDeptUpdate.Add("dept_name", dept_name);
                paramDeptUpdate.Add("dept_alias", dept_alias);
                paramDeptUpdate.Add("dept_status", dept_status);
                paramDeptUpdate.Add("dept_seq", dept_sequence);
                paramDeptUpdate.Add("dept_sno", dept_subject);
                paramDeptUpdate.Add("dept_bid", dept_brand);
                paramDeptUpdate.Add("dept_sid", dept_sid);
                paramDeptUpdate.Add("dept_posid", dept_posid);
                paramDeptUpdate.Add("lastupdatetime", dateTime);

                var selectSyncInfoBrand = @"select tablename from syncinfo where tablename='brand_default'";
                var selectSyncInfoArea = @"select tablename from syncinfo where tablename='area_default'";
                var selectSyncInfoDept = @"select tablename from syncinfo where tablename='dept_default'";
                var selectSyncInfoStore = @"select tablename from syncinfo where tablename='store'";
                var selectSyncInfoDish_type = @"select tablename from syncinfo where tablename='dish_type'";
                var selectSyncInfoUnit = @"select tablename from syncinfo where tablename='unit'";
                var selectSyncInfoDish = @"select tablename from syncinfo where tablename='dish'";
                var selectSyncInfoMeal_time = @"select tablename from syncinfo where tablename='meal_time'";
                var selectSyncInfoOrder = @"select tablename from syncinfo where tablename='order'";
                var insertSyncInfoBrand = @"insert into syncinfo(tablename,lastupdatetime) values('brand_default',@lastupdatetime);";
                var insertSyncInfoArea = @"insert into syncinfo(tablename,lastupdatetime) values('area_default',@lastupdatetime);";
                var insertSyncInfoDept = @"insert into syncinfo(tablename,lastupdatetime) values('dept_default',@lastupdatetime);";
                var insertSyncInfoMeal_time = @"insert into syncinfo(tablename,lastupdatetime) values('meal_time',@lastupdatetime);";
                var insertSyncInfoDish_type = @"insert into syncinfo(tablename,lastupdatetime) values('dish_type',@lastupdatetime);";
                var insertSyncInfoStore = @"insert into syncinfo(tablename,lastupdatetime) values('store',@lastupdatetime);";
                var insertSyncInfoDish = @"insert into syncinfo(tablename,lastupdatetime) values('dish',@lastupdatetime);";
                var insertSyncInfoUnit = @"insert into syncinfo(tablename,lastupdatetime) values('unit',@lastupdatetime);";
                var insertSyncInfoOrder = @"insert into syncinfo(tablename,lastupdatetime) values('order',@lastupdatetime);";
                var paramDateTime = new DynamicParameters();
                paramDateTime.Add("lastupdatetime", ConfigInfo.StartTime);
                #endregion

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
                            else if (brandInfo.Brand_name != brand_name || brandInfo.Brand_Seq != brand_seq || brandInfo.Brand_status != brand_status || brandInfo.Brand_subject != brand_subject)
                            {
                                db.Execute(updateBranSql, paramBrandUpdate);
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
                            else if (areaInfo.Area_name != area_name || areaInfo.Area_level != area_level || areaInfo.Area_status != area_status || areaInfo.Area_Seq != area_seq || areaInfo.Area_faid != area_faid || areaInfo.Area_subject != area_subject)
                            {
                                db.Execute(updateAreaSql, paramAreaUpdate);
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
                            else if (deptInfo.Dept_name != dept_name || deptInfo.Dept_alias != dept_alias || deptInfo.Dept_bid != dept_brand || deptInfo.Dept_seq != dept_sequence || deptInfo.Dept_sid != dept_sid || deptInfo.Dept_sno != dept_subject || deptInfo.Dept_status != dept_status)
                            {
                                db.Execute(updateDeptSql, paramDeptUpdate);
                            }
                        }
                        else
                        {
                            btn_Dept.Enabled = false;
                        }

                        #region 插入基本信息
                        if (db.ExecuteScalar(selectSyncInfoBrand)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoBrand, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoArea)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoArea, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoDept)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoDept, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoStore)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoStore, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoMeal_time)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoMeal_time, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoUnit)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoUnit, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoDish_type)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoDish_type, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoDish)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoDish, paramDateTime);
                        }
                        if (db.ExecuteScalar(selectSyncInfoOrder)?.ToString() == null)
                        {
                            db.Execute(insertSyncInfoOrder, paramDateTime);
                        }
                        #endregion
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
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                var updateLastSyncSql = $"update syncinfo set lastupdatetime=@lastupdatetime where tablename=@tablename";
                var paramUpdate = new DynamicParameters();
                paramUpdate.Add("lastupdatetime", DateTime.Now);
                paramUpdate.Add("tablename", tableName);
                return db.Execute(updateLastSyncSql, paramUpdate);
            }
        }

        #endregion

        private void MSTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void display_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void exit_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("确定要退出吗？", "Tips", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                notifyIcon1.Visible = false;
                Close();
                Dispose();
                Application.Exit();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                display_Click(null, null);
            }
        }
    }
}
