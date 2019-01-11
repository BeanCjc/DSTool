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
    class Brand_default
    {
        public string Brand_name { get; set; }
        public int Brand_status { get; set; }
        public string Brand_subject { get; set; }
        public int Brand_posid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public static Brand_default GetById(int id)
        {
            var sql = @"select brand_posid,brand_name,brang_status,brand_subject,createtime,lastupdatetime from brand_default where posid=@posid";
            var param = new DynamicParameters();
            param.Add("posid", id);
            using (var db=new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Brand_default>(sql, param).FirstOrDefault();
            }
        }

        public static List<Brand_default> GetListByLastTime(string lastTime)
        {
            var sql = @"select brand_name,brand_status,brand_subject,brand_posid,createtime,lastupdatetime from brand_default where lastupdatetime>=@lastupdatetime";
            var param = new DynamicParameters();
            param.Add("lastupdatetime", lastTime);
            using (var db=new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<Brand_default>(sql, param).ToList();
            }
        }
    }
}
