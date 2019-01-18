using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace DSTool.DbData
{
    /// <summary>
    /// 计量单位档案
    /// </summary>
    class DA_JLDW
    {
        /// <summary>
        /// 计量单位内码 PK，ID NO
        /// </summary>
        public int JLDWNM { get; set; }

        /// <summary>
        /// 计量单位 UNIQUEKEY NO
        /// </summary>
        public string JLDW { get; set; }

        public static DA_JLDW GetById(int id)
        {
            var sql = @"select JLDWNM,JLDW from DA_JLDW where JLDWNM=@JLDWNM";
            var param = new DynamicParameters();
            param.Add("JLDWNM", id);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_JLDW>(sql, param).FirstOrDefault();
            }
        }

        public static string GetIdByUnitName(string UnitName)
        {
            if (string.IsNullOrEmpty(UnitName))
            {
                return GetIdByUnitName("份");
            }
            var sql = @"select JLDWNM from DA_JLDW where JLDW=@JLDW";
            var param = new DynamicParameters();
            param.Add("JLDW", UnitName);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.ExecuteScalar(sql, param)?.ToString() ?? GetIdByUnitName("份");
            }
        }
        public static List<DA_JLDW> GetList()
        {
            var sql = @"select JLDWNM,JLDW from DA_JLDW";
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_JLDW>(sql).ToList();
            }
        }
    }
}
