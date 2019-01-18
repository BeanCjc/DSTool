using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    /// <summary>
    /// 商品类别档案
    /// </summary>
    class DA_SPLB
    {
        /// <summary>
        /// 商品类别内码 PK NO
        /// </summary>
        public int SPLBNM { get; set; }

        /// <summary>
        /// 商品类别代码 UNIQUEINDEX NO
        /// </summary>
        public string SPLBDM { get; set; }

        /// <summary>
        /// 序号 NO
        /// </summary>
        public int XH { get; set; }

        /// <summary>
        /// 商品类别名称 UNIQUE KEY NO
        /// </summary>
        public string SPLB { get; set; }

        /// <summary>
        /// 最低折扣 default 0 NO
        /// </summary>
        public decimal ZDZK { get; set; }

        /// <summary>
        /// 会员折扣 default 1 NO
        /// </summary>
        public decimal HYZK { get; set; }

        /// <summary>
        /// 折扣开放标记 1是，0否 NO
        /// </summary>
        public int ZKKFBJ { get; set; }

        /// <summary>
        /// 价格开放标记 0否，1是 NO
        /// </summary>
        public int JGKFBJ { get; set; }

        /// <summary>
        /// 参与积分标记 1是，0 NO
        /// </summary>
        public int CYJFBJ { get; set; }

        /// <summary>
        /// 所属类别内码 FK，self NO
        /// </summary>
        public int SSLBNM { get; set; }

        /// <summary>
        /// 记库存标记 0否,1是 NO
        /// </summary>
        public int JKCBJ { get; set; }

        /// <summary>
        /// 允许赠送标记 0不,1是 NO
        /// </summary>
        public int YXZSBJ { get; set; }

        /// <summary>
        /// 厨房内码 FK NO
        /// </summary>
        public int CFNM { get; set; }

        /// <summary>
        /// 厨房内码2 FK NO
        /// </summary>
        public int CFNM2 { get; set; }

        /// <summary>
        /// 原材料标记（是的话在销售的时候不显示 0否1是 NO
        /// </summary>
        public int YCLBJ { get; set; }

        /// <summary>
        /// 允许销售标记 1是0否 NO
        /// </summary>
        public int YXXSBJ { get; set; }

        /// <summary>
        /// 启用标记 1启用，0不启用 NO
        /// </summary>
        public int QYBJ { get; set; }

        /// <summary>
        /// 修改日期 NO
        /// </summary>
        public DateTime XGRQ { get; set; }

        /// <summary>
        /// 发送日期 NO
        /// </summary>
        public DateTime FSRQ { get; set; }

        /// <summary>
        /// 接收日期 NO
        /// </summary>
        public DateTime JSRQ { get; set; }

        /// <summary>
        /// 是否打印（小类使用） 0不打印，1打印 NO
        /// </summary>
        public int SFDY { get; set; }

        /// <summary>
        /// 是否选择款式 0否，1是 NO
        /// </summary>
        public int XZKS { get; set; }

        /// <summary>
        /// 提成比例 default 0 NO
        /// </summary>
        public decimal TCBL { get; set; }

        /// <summary>
        /// 提成金额 default 0 NO
        /// </summary>
        public decimal TCJE { get; set; }

        /// <summary>
        /// 时价标记 default 0 NO
        /// </summary>
        public int SJBJ { get; set; }

        /// <summary>
        /// 背景色 NO
        /// </summary>
        public int BACKCOLOR { get; set; }

        /// <summary>
        /// 点菜宝编码 YES
        /// </summary>
        public string DCBBM { get; set; }

        /// <summary>
        /// 自助点餐 default 1 NO
        /// </summary>
        public int ZZDC { get; set; }

        public static DA_SPLB GetById(int id)
        {
            var sql = @"select SPLBNM,SPLB,SSLBNM,QYBJ,SPLBDM from DA_SPLB where SPLBNM=@SPLBNM and yclbj<>1";
            var param = new DynamicParameters();
            param.Add("SPLBNM", id);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_SPLB>(sql, param).FirstOrDefault();
            }
        }

        public static List<DA_SPLB> GetListByLastTime(string lastTime)
        {
            var sql = @"select SPLBNM,SPLB,SSLBNM,QYBJ,SPLBDM from DA_SPLB where XGRQ>=@lasttime and yclbj<>1";
            var param = new DynamicParameters();
            param.Add("lasttime", lastTime);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_SPLB>(sql, param).ToList();
            }
        }
    }
}
