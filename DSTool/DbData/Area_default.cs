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
        /// <summary>
        /// 区域名称
        /// </summary>
        public string Area_name { get; set; }

        /// <summary>
        /// 区域等级,默认1
        /// </summary>
        public int Area_level { get; set; }

        /// <summary>
        /// 父级区域ID,默认写-1(供应链默认顶级数据)
        /// </summary>
        public string  Area_faid { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Area_Seq { get; set; }

        /// <summary>
        /// 状态 1:有效 0:无效
        /// </summary>
        public int Area_status { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Area_subject { get; set; }

        /// <summary>
        /// 主键ID
        /// </summary>
        public string Area_posid { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
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
