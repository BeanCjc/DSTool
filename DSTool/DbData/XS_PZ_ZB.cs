using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DSTool.DbData
{
    /// <summary>
    /// 销售凭证主表
    /// </summary>
    class XS_PZ_ZB
    {
        /// <summary>
        /// 交易号 Char(17)3位分店内码+日期8(20080101)+6位流水号(000001)=17位 PK NO
        /// </summary>
        public string JYH { get; set; }

        /// <summary>
        /// 流水号，用于叫菜,每天重新开始，从1开始 NO
        /// </summary>
        public int LSH { get; set; }

        /// <summary>
        /// 操作日期 NO
        /// </summary>
        public int CZRQ { get; set; }

        /// <summary>
        /// 操作时间 NO
        /// </summary>
        public int CZSJ { get; set; }

        /// <summary>
        /// 操作员内码 FK NO
        /// </summary>
        public int CZYNM { get; set; }

        /// <summary>
        /// 收银机内码 FK YES
        /// </summary>
        public int SYJNM { get; set; }

        /// <summary>
        /// 班次内码 FK NO
        /// </summary>
        public int BCNM { get; set; }

        /// <summary>
        /// 分店内码 FK NO
        /// </summary>
        public int FDNM { get; set; }

        /// <summary>
        /// 销售日期(结帐时候写) default 0 NO
        /// </summary>
        public int CZRQ_XS { get; set; }

        /// <summary>
        /// 销售时间 default 0 NO
        /// </summary>
        public int CZSJ_XS { get; set; }

        /// <summary>
        /// 操作员内码销售,先保存点单人，结帐时回写 FK NO
        /// </summary>
        public int CZYNM_XS { get; set; }

        /// <summary>
        /// 销售日期 NO
        /// </summary>
        public int XSRQ { get; set; }

        /// <summary>
        /// 人数  default 0 NO
        /// </summary>
        public int RS { get; set; }

        /// <summary>
        /// 卡内码 FK YES
        /// </summary>
        public int KNM { get; set; }

        /// <summary>
        /// 桌台内码 FK YES
        /// </summary>
        public int ZTNM { get; set; }

        /// <summary>
        /// 销售金额 default 0 NO
        /// </summary>
        public decimal XSJE { get; set; }

        /// <summary>
        /// 销售优惠 default 0 NO
        /// </summary>
        public decimal XSYH { get; set; }

        /// <summary>
        /// 交易取消标记 0否1是 NO
        /// </summary>
        public int JYQXBJ { get; set; }

        /// <summary>
        /// 反结帐标记,实际用于预订标记，蛋糕预订 1已提货，2未提货，3已取消 NO
        /// </summary>
        public int FJZBJ { get; set; }

        /// <summary>
        /// 转台信息,开台时要同时判断这个为null的才是挂帐记录 YES
        /// </summary>
        public string ZTXX { get; set; }

        /// <summary>
        /// 服务员内码 FK YES
        /// </summary>
        public int FWYNM { get; set; }

        /// <summary>
        /// 现金金额 default 0 NO
        /// </summary>
        public decimal XJJE { get; set; }

        /// <summary>
        /// 储值卡金额 default 0 NO
        /// </summary>
        public decimal CZKJE { get; set; }

        /// <summary>
        /// 抹零金额 default 0 NO
        /// </summary>
        public decimal MLJE { get; set; }

        /// <summary>
        /// 优惠券金额 default 0 NO
        /// </summary>
        public decimal YHQJE { get; set; }

        /// <summary>
        /// 积分付款金额 default 0 NO
        /// </summary>
        public decimal QTJE { get; set; }

        /// <summary>
        /// 免单金额 default 0 NO
        /// </summary>
        public decimal MDJE { get; set; }

        /// <summary>
        /// 实际用于商品预订的预收金额合计 default 0 NO
        /// </summary>
        public decimal BCJE { get; set; }

        /// <summary>
        /// 日结日期 default 0 NO
        /// </summary>
        public int RJRQ { get; set; }

        /// <summary>
        /// 发送日期 default 0 NO
        /// </summary>
        public int FSRQ { get; set; }

        /// <summary>
        /// 接收日期 default 0 NO
        /// </summary>
        public int JSRQ { get; set; }

        /// <summary>
        /// 签单金额 default 0 NO
        /// </summary>
        public decimal QDJE { get; set; }

        /// <summary>
        /// 包厢金额 default 0 NO
        /// </summary>
        public decimal BXJE { get; set; }

        /// <summary>
        /// 信用卡金额 default 0 NO
        /// </summary>
        public decimal JE1 { get; set; }

        /// <summary>
        /// 其他付款 default 0 NO
        /// </summary>
        public decimal JE2 { get; set; }

        /// <summary>
        /// 其他付款2 default 0 NO
        /// </summary>
        public decimal QTFK2 { get; set; }

        /// <summary>
        /// 来电通客户 YES
        /// </summary>
        public int LDTKH { get; set; }

        /// <summary>
        /// 反结账标记 default 0 NO
        /// </summary>
        public int FJZBJTRUE { get; set; }

        /// <summary>
        /// 备注 YES
        /// </summary>
        public string BZ { get; set; }

        /// <summary>
        /// 手工单号 YES
        /// </summary>
        public string SGDH { get; set; }

        /// <summary>
        /// 领餐号 YES
        /// </summary>
        public string LCH { get; set; }

        /// <summary>
        /// 桌台名称 YES
        /// </summary>
        public string ZT { get; set; }

        /// <summary>
        /// 员工餐 default 0 NO
        /// </summary>
        public decimal QTFK3 { get; set; }

        /// <summary>
        /// 其他付款4 default 0 NO
        /// </summary>
        public decimal QTFK4 { get; set; }

        /// <summary>
        /// 其他付款5 default 0 NO
        /// </summary>
        public decimal QTFK5 { get; set; }

        /// <summary>
        /// 订单项
        /// </summary>
        public List<XS_PZ> Order_Items { get; set; }

        public static XS_PZ_ZB GetById(string id)
        {
            var sql = @" SELECT JYH,CZRQ,CZSJ,FDNM,CZRQ_XS,CZSJ_XS,XSRQ,RS,ZTNM,XSJE,XSYH,
                                MLJE,/*XJJE,CZKJE, YHQJE,QTJE,MDJE,QDJE,JE1,*/BCJE,RJRQ,BXJE,FJZBJTRUE,BZ,ZT 
                           FROM XS_PZ_ZB
                          WHERE JYH=@JYH";
            var param = new DynamicParameters();
            param.Add("JYH", id);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<XS_PZ_ZB>(sql, param).FirstOrDefault();
            }

        }
        public static List<XS_PZ_ZB> GetListByLastTime(DateTime lastTime)
        {
            var sql = @" SELECT JYH,CZRQ,CZSJ,FDNM,CZRQ_XS,CZSJ_XS,XSRQ,RS,ZTNM,XSJE,XSYH,
                                MLJE,/*XJJE,CZKJE, YHQJE,QTJE,MDJE,QDJE,JE1,*/BCJE,RJRQ,BXJE,FJZBJTRUE,BZ,ZT 
                           FROM XS_PZ_ZB
                          WHERE (CZRQ>@CZRQ OR (CZRQ=@CZRQ AND CZSJ>=@CZSJ))
                            AND (CZRQ_XS>@CZRQ_XS OR (CZRQ_XS=@CZRQ_XS AND CZSJ_XS>=@CZSJ_XS))";
            var param = new DynamicParameters();
            param.Add("CZRQ", lastTime.ToString("yyyyMMdd"));
            param.Add("CZSJ", lastTime.ToString("HHmmss"));
            param.Add("CZRQ_XS", lastTime.ToString("yyyyMMdd"));
            param.Add("CZSJ_XS", lastTime.ToString("HHmmss"));
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<XS_PZ_ZB>(sql, param).ToList();
            }
        }
    }
}
