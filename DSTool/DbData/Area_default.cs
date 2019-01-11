using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace DSTool.DbData
{
    class Area_default
    {
        public string Area_name { get; set; }
        public int Area_level { get; set; }
        public int? Area_faid { get; set; }
        public int Area_Seq { get; set; }
        public int Area_status { get; set; }
        public string Area_subject { get; set; }
        public int Area_posid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public static Area_default GetById(int id)
        {
            var sql = @"select area_name,area_level,area_faid,area_seq,area_status,area_subject,area_posid,createtime,lastupdatetime from area_default where area_posid=@area_posid";
            var param = new DynamicParameters();
            param.Add("area_posid", id);
            using (var db=new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Area_default>(sql, param).FirstOrDefault();
            }
        }

        public static List<Area_default> GetListByLastTime(string lastTime)
        {
            var sql= @"select area_name,area_level,area_faid,area_seq,area_status,area_subject,area_posid,createtime,lastupdatetime from area_default where lastupdatetime>=@lastupdatetime";
            var param = new DynamicParameters();
            param.Add("lastupdatetime", lastTime);
            using (var  db=new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Area_default>(sql, param).ToList();
            }
        }
    }
}
