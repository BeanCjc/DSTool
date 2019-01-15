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
    class Dept_default
    {
        /// <summary>
        /// 部门名称
        /// </summary>
        public string Dept_name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Dept_alias { get; set; }

        /// <summary>
        /// 状态 1:有效 2:无效
        /// </summary>
        public int Dept_status { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Dept_seq { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Dept_sno { get; set; }

        /// <summary>
        /// 品牌ID
        /// </summary>
        public string Dept_bid { get; set; }

        /// <summary>
        /// 门店ID
        /// </summary>
        public string Dept_sid { get; set; }

        /// <summary>
        /// 主键ID
        /// </summary>
        public string Dept_posid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public static Dept_default GetById(int id)
        {
            var sql = @"select dept_name,dept_alias,dept_status,dept_seq,dept_sno,dept_bid,dept_sid,dept_posid,createtime,lastupdatetime from dept_default where lastupdatetime>=@lastupdatetime";
            var param = new DynamicParameters();
            param.Add("lastupdatetime", id);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Dept_default>(sql, param).FirstOrDefault();
            }
        }

        public static List<Dept_default> GetListByLastTime(string lastTime)
        {
            var sql = @"select dept_name,dept_alias,dept_status,dept_seq,dept_sno,dept_bid,dept_sid,dept_posid,createtime,lastupdatetime from dept_default where lastupdatetime>=@lastupdatetime";
            var param = new DynamicParameters();
            param.Add("lastupdatetime", lastTime);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Dept_default>(sql, param).ToList();
            }
        }
    }
}
