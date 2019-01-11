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
        public string Dept_name { get; set; }
        public string Dept_alias { get; set; }
        public int Dept_status { get; set; }
        public int Dept_sequence { get; set; }
        public string Dept_subject { get; set; }
        public int Dept_brand { get; set; }
        public int Dept_posid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public static Dept_default GetById(int id)
        {
            var sql = @"select dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid,createtime,lastupdatetime from dept_default where lastupdatetime>=@lastupdatetime";
            var param = new DynamicParameters();
            param.Add("lastupdatetime", id);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Dept_default>(sql, param).FirstOrDefault();
            }
        }

        public static List<Dept_default> GetListByLastTime(string lastTime)
        {
            var sql = @"select dept_name,dept_alias,dept_status,dept_sequence,dept_subject,dept_brand,dept_posid,createtime,lastupdatetime from dept_default where lastupdatetime>=@lastupdatetime";
            var param = new DynamicParameters();
            param.Add("lastupdatetime", lastTime);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Dept_default>(sql, param).ToList();
            }
        }
    }
}
