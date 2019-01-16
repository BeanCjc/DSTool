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
    /// <summary>
    /// 商品档案
    /// </summary>
    class DA_SP
    {
        /// <summary>
        /// 商品内码1000001 PK NO
        /// </summary>
        public int SPNM { get; set; }

        /// <summary>
        /// 商品代码(3位类别代码加4位商品流水号) UNIQUEINDEX NO
        /// </summary>
        public string SPDM { get; set; }

        /// <summary>
        /// 序号 NO
        /// </summary>
        public int XH { get; set; }

        /// <summary>
        /// 商品条码 UNIQUEINDEX YES
        /// </summary>
        public string SPTM { get; set; }

        /// <summary>
        /// 商品助记码 INDEX YES
        /// </summary>
        public string SPZJM { get; set; }

        /// <summary>
        /// 商品名称 INDEX NO
        /// </summary>
        public string SP { get; set; }

        /// <summary>
        /// 计量单位 YES
        /// </summary>
        public string JLDW { get; set; }

        /// <summary>
        /// 规格 YES
        /// </summary>
        public string GGXH { get; set; }

        /// <summary>
        /// 产地 YES
        /// </summary>
        public string CD { get; set; }

        /// <summary>
        /// 商品大类内码 FK NO
        /// </summary>
        public int SPDLNM { get; set; }

        /// <summary>
        /// 商品小类内码 FK NO
        /// </summary>
        public int SPXLNM { get; set; }

        /// <summary>
        /// 零售价 NO
        /// </summary>
        public decimal LSJ { get; set; }

        /// <summary>
        /// 成本价(手工指定的综合成本价) default 0 NO
        /// </summary>
        public decimal CBJ { get; set; }

        /// <summary>
        /// 最后进价 default 0 NO
        /// </summary>
        public decimal ZHJJ { get; set; }

        /// <summary>
        /// 总部指导零售价 default 0 NO
        /// </summary>
        public decimal ZBZDLSJ { get; set; }

        /// <summary>
        /// 总部指导最低价 default 0 NO
        /// </summary>
        public decimal ZBZDZDJ { get; set; }

        /// <summary>
        /// 直营配送价 default 0 NO
        /// </summary>
        public decimal ZYPSJ { get; set; }

        /// <summary>
        /// 加盟配送价 default 0 NO
        /// </summary>
        public decimal JMPSJ { get; set; }

        /// <summary>
        /// 价格开放标记 NO
        /// </summary>
        public int JGKFBJ { get; set; }

        /// <summary>
        /// 折扣开放标记 NO
        /// </summary>
        public int ZKKFBJ { get; set; }

        /// <summary>
        /// 最低折扣 NO
        /// </summary>
        public decimal ZDZK { get; set; }

        /// <summary>
        /// 参与积分标记 NO
        /// </summary>
        public int CYJFBJ { get; set; }

        /// <summary>
        /// 允许赠送标记 0非，1是 NO
        /// </summary>
        public int YXZSBJ { get; set; }

        /// <summary>
        /// 兑换积分数 default 0 NO
        /// </summary>
        public int DHJFS { get; set; }

        /// <summary>
        /// 建档日期 NO
        /// </summary>
        public int JDRQ { get; set; }

        /// <summary>
        /// 记库存标记 0否1是2原材料 NO
        /// </summary>
        public int JKCBJ { get; set; }

        /// <summary>
        /// 套餐标记 0否，1是 NO
        /// </summary>
        public int TCBJ { get; set; }

        /// <summary>
        /// 统计方式 0按日1按周，2按月 NO
        /// </summary>
        public int TJFS { get; set; }

        /// <summary>
        /// 最高库存 default 0 NO
        /// </summary>
        public int ZGKC { get; set; }

        /// <summary>
        /// 最低库存 default 0 NO
        /// </summary>
        public int ZDKC { get; set; }

        /// <summary>
        /// 启用标记(修改启用标记提示库存数量不会被结转到下期) 1启用，0不启用 NO
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
        /// 提成比例 default 0 NO
        /// </summary>
        public decimal TCBL { get; set; }

        /// <summary>
        /// 提成金额 default 0 NO
        /// </summary>
        public decimal TCJE { get; set; }

        /// <summary>
        /// 是否送厨(用于允许类别下单个商品作为例外) default 1 NO
        /// </summary>
        public int SFSC { get; set; }

        /// <summary>
        /// 外包单位 NO
        /// </summary>
        public string WBDW { get; set; }

        /// <summary>
        /// 外包包装率 NO
        /// </summary>
        public decimal WBBZL { get; set; }

        /// <summary>
        /// 内包单位 NO
        /// </summary>
        public string NBDW { get; set; }

        /// <summary>
        /// 内包包装率 NO
        /// </summary>
        public decimal NBBZL { get; set; }

        /// <summary>
        /// 时价标记 default 0 NO
        /// </summary>
        public int SJBJ { get; set; }

        /// <summary>
        /// 商品英文名 YES
        /// </summary>
        public string SP_E { get; set; }

        /// <summary>
        /// 背景色 NO
        /// </summary>
        public int BACKCOLOR { get; set; }

        /// <summary>
        /// 特价标记 default 0 NO
        /// </summary>
        public int TJBJ { get; set; }

        /// <summary>
        /// 新品标记 default 0 NO
        /// </summary>
        public int XPBJ { get; set; }

        /// <summary>
        /// 自助点餐 default 1 NO
        /// </summary>
        public int ZZDC { get; set; }

        /// <summary>
        /// 重量 default 0 NO
        /// </summary>
        public decimal ZL { get; set; }

        /// <summary>
        /// 中止日期 NO
        /// </summary>
        public int ZZRQ { get; set; }

        /// <summary>
        /// 生效日期 NO
        /// </summary>
        public int SXRQ { get; set; }

        /// <summary>
        /// 审批标记，用于套餐需要审批下传 NO
        /// </summary>
        public int SPBJ { get; set; }

        /// <summary>
        /// 秤重标记 default 0 NO
        /// </summary>
        public int JZBJ { get; set; }

        /// <summary>
        /// 厨房内码 NO
        /// </summary>
        public int CFNM { get; set; }

        /// <summary>
        /// 厨房内码2 NO
        /// </summary>
        public int CFNM2 { get; set; }

        /// <summary>
        /// 起始时间 NO
        /// </summary>
        public int QSSJ { get; set; }

        /// <summary>
        /// 结束时间 NO
        /// </summary>
        public int JSSJ { get; set; }

        public static DA_SP GetById(string id)
        {

            var sql = @"select SPNM,SPXLNM,SP_E,JDRQ,SP,SPZJM,QYBJ,XH,LSJ,JLDW from DA_SP where SPNM=@SPNM and jkcbj<>2 and spnm>0";
            var param = new DynamicParameters();
            param.Add("SPNM", id);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_SP>(sql, param).FirstOrDefault();
            }
        }
        public static List<DA_SP> GetListByLastTime(DateTime lastTime)
        {

            var sql = @"select SPNM,SPXLNM,JDRQ,SP,SPZJM,QYBJ,XH,LSJ,JLDW from DA_SP where (JDRQ>=@JDRQ or XGRQ>=@XGRQ) and jkcbj<>2 and spnm>0";
            var param = new DynamicParameters();
            param.Add("JDRQ", lastTime.ToString("yyyyMMdd"));
            param.Add("XGRQ", lastTime.ToString("yyyy-MM-dd HH:mm:ss"));
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_SP>(sql, param).ToList();
            }
        }
    }
}
