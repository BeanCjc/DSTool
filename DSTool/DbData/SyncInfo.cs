using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    class SyncInfo
    {
        public string TableName { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public bool IsSynced { get; set; }

        public string IdList { get; set; }

        public static SyncInfo GetInfoByTableName(string tableName)
        {
            var sql = @"select top 1 tablename,lastupdatetime,issynced,idlist from syncinfo where tablename=@tablename";
            var param = new DynamicParameters();
            param.Add("tablename", tableName);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<SyncInfo>(sql, param).FirstOrDefault();
            }
        }

        public static bool UpdateByTableName(string tableName, int isSynced = 0, string idList = "")
        {
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                var sql = @"update syncinfo set lastupdatetime=@lastupdatetime,issynced=@issynced,idlist=@idlist where tablename=@tablename";
                var param = new DynamicParameters();
                param.Add("issynced", isSynced);
                param.Add("lastupdatetime", DateTime.Now);
                param.Add("tablename", tableName);
                param.Add("idlist", idList);
                return db.Execute(sql, param) > 0;
            }
        }


    }
}
