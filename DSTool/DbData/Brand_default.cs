using System;
using System.Collections.Generic;
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
            var sql = @"select brand_posid,brand_name,brang_status,brand_subject,createtime,lastupdatetime from brand_default where posid=?posid";
            var param = new DynamicParameters();
            param.Add("?posid", id);
            using (var db=new MySqlConnection(ConfigInfo.Mysql_connectionstring))
            {
                return db.Query<Brand_default>(sql, param).FirstOrDefault();
            }
        }
    }
}
