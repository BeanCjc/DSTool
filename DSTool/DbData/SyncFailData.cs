using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DSTool.DbData
{
    /// <summary>
    /// 同步失败数据记录
    /// </summary>
    class SyncFailData
    {
        /// <summary>
        /// 同步失败的表明
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 同步失败的表逐渐数据，联合逐渐用";"隔开
        /// </summary>
        public string IdList { get; set; }

        /// <summary>
        /// 失败类型，0:插入失败 1:更新失败 
        /// </summary>
        public int FailType { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailMessage { get; set; }

        /// <summary>
        /// 插入时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public static List<SyncFailData> GetListByTableName(string tableName)
        {
            var sql = @"select idlist from syncfaildata where tablename=@tablename";
            var param = new DynamicParameters();
            param.Add("tablename", tableName);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<SyncFailData>(sql, param).ToList();
            }

        }

        public static bool InsertFailDate(SyncFailData data)
        {
            var sql = @"insert into syncfaildata(tablename,idlist,failtype,failmessage,createtime) values(@tablename,@idlist,@failtype,@failmessage,@createtime)";
            var param = new DynamicParameters();
            param.Add("tablename", data.TableName);
            param.Add("idlist", data.IdList);
            param.Add("failtype", data.FailType);
            param.Add("failmessage", data.FailMessage);
            param.Add("createtime", DateTime.Now);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Execute(sql, param) > 0;
            }
        }
        public static bool InsertFailDates(List<SyncFailData> data)
        {
            var sql = @"insert into syncfaildata(tablename,idlist,failtype,failmessage,createtime) values(@tablename,@idlist,@failtype,@failmessage,@createtime)";
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Execute(sql, data) > 0;
            }
        }

        public static bool DeleteFailData(string tableName, string idList)
        {
            var sql = @"delete from syncfaildata where tablename=@tablename and idlist=@idlist";
            var param = new DynamicParameters();
            param.Add("tablename", tableName);
            param.Add("idlist", idList);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Execute(sql, param) > 0;
            }
        }
        public static bool DeleteFailDatas(string tableName, List<DeleteObject> idLists)
        {
            var sql = @"delete from syncfaildata where tablename=@tablename and idlist=@idlist";
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Execute(sql, idLists) > 0;
            }
        }
    }
    public class DeleteObject
    {
        public string TableName { get; set; }
        public string IdList { get; set; }
    }
}
