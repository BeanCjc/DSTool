using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DSTool.DbData
{
    class Brand_default
    {
        /// <summary>
        /// 品牌名称
        /// </summary>
        public string Brand_name { get; set; }

        /// <summary>
        /// 状态 1:有效 0:无效
        /// </summary>
        public int Brand_status { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Brand_subject { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Brand_Seq { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Brand_Memo { get; set; }

        /// <summary>
        /// 主键ID
        /// </summary>
        public string Brand_posid { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间(该表数据的更新)
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 更新时间(更新到供应链)
        /// </summary>
        public DateTime UTime { get; set; }

        public static Brand_default GetById(int id)
        {
            var sql = @"select brand_posid,brand_name,brand_status,brand_subject,createtime,lastupdatetime from brand_default where posid=@posid";
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
