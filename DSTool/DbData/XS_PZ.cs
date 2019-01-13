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
    /// 销售凭证，套餐明细销售数量也记录在本表，商品组成不记录，只减库存) 交易号和结帐单号均为8位年月日6位流水号
    /// </summary>
    class XS_PZ
    {
        /// <summary>
        /// 交易号 Char(17)3位分店内码+日期8(20080101)+6位流水号(000001)=17位 PK FK NO
        /// </summary>
        public string JYH { get; set; }

        /// <summary>
        /// 序号 PK NO
        /// </summary>
        public int XH { get; set; }

        /// <summary>
        /// 挂帐标记(0为已结帐，1为挂帐) NO
        /// </summary>
        public int GZBJ { get; set; }

        /// <summary>
        /// 送厨标记，有这个标记就可以用存储过程来判断哪些需要插入送厨表 default 0 NO
        /// </summary>
        public int SCBJ { get; set; }

        /// <summary>
        /// 商品内码 FK NO
        /// </summary>
        public int SPNM { get; set; }

        /// <summary>
        /// 所属套餐内码(如果不是null就是套餐明细商品) FK YES
        /// </summary>
        public int SPNM_SSTC { get; set; }

        /// <summary>
        /// 套餐明细行数，方便定位套餐的明细 YES
        /// </summary>
        public int TCMXHS { get; set; }

        /// <summary>
        /// 赠品行数 default 0 NO
        /// </summary>
        public int ZPHS { get; set; }

        /// <summary>
        /// 赠品标记（本处增加该标记是保留历史记录，防止商品的赠品标记被修改导致数据看上去不一致）促销政策产生的赠品该标记为1，手工的为2，这样不满足促销条件时可以找到促销赠品删除 0非，1是 NO
        /// </summary>
        public int ZPBJ { get; set; }

        /// <summary>
        /// 赠品所属商品内码，删除商品或者退货的时候要把消费该商品促销赠送的商品同时处理 YES
        /// </summary>
        public int ZPSSSPNM { get; set; }

        /// <summary>
        /// 销售数量 default 0 NO
        /// </summary>
        public decimal XSSL { get; set; }

        /// <summary>
        /// 套餐内销售数量 default 0 NO
        /// </summary>
        public decimal TCXSSL { get; set; }

        /// <summary>
        /// 赠品数量(统计赠品销售数量) default 0 NO
        /// </summary>
        public decimal ZPXSSL { get; set; }

        /// <summary>
        /// 促销销售数量 default 0 NO
        /// </summary>
        public decimal CXXSSL { get; set; }

        /// <summary>
        /// 零售价 default 0 NO
        /// </summary>
        public decimal LSJ { get; set; }

        /// <summary>
        /// 促销价（临时用来保存促销价格，取消会员卡时销售价采用促销价，不用再次检索或者刷入会员卡是和促销价比哪个最低） default 0 NO
        /// </summary>
        public decimal CXJ { get; set; }

        /// <summary>
        /// 销售价 default 0 NO
        /// </summary>
        public decimal XSJ { get; set; }

        /// <summary>
        /// 折扣率 default 1 NO
        /// </summary>
        public decimal ZKL { get; set; }

        /// <summary>
        /// 销售金额 default 0 NO
        /// </summary>
        public decimal XSJE { get; set; }

        /// <summary>
        /// 销售优惠 default 0 NO
        /// </summary>
        public decimal XSYH { get; set; }

        /// <summary>
        /// 成本金额 default 0 NO
        /// </summary>
        public decimal CBJE { get; set; }

        /// <summary>
        /// 综合成本金额（手工指定） default 0 NO
        /// </summary>
        public decimal ZHCBJE { get; set; }

        /// <summary>
        /// 口味 YES
        /// </summary>
        public string KW { get; set; }

        /// <summary>
        /// 授权人 YES
        /// </summary>
        public string SQR { get; set; }

        /// <summary>
        /// 服务员提成金额 default 0 NO
        /// </summary>
        public decimal FWYTCJE { get; set; }

        /// <summary>
        /// 退货原因 YES
        /// </summary>
        public string THYY { get; set; }

        /// <summary>
        /// 点单批次 NO
        /// </summary>
        public int DDPC { get; set; }

        /// <summary>
        /// 口味加价金额 NO
        /// </summary>
        public decimal JJJE { get; set; }

        /// <summary>
        /// 套餐替换加价金额 NO
        /// </summary>
        public decimal TCJJJE { get; set; }

        /// <summary>
        /// 点菜宝流水号 YES
        /// </summary>
        public string DCBLSH { get; set; }

        /// <summary>
        /// 起菜方式 NO
        /// </summary>
        public int QCFS { get; set; }

        /// <summary>
        /// 续单标记 NO
        /// </summary>
        public int XDBJ { get; set; }

        /// <summary>
        /// 规格 YES
        /// </summary>
        public string GG { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string JLDW { get; set; }

        public static List<XS_PZ> GetListByOrderId(string orderId)
        {
            var result = new List<XS_PZ>();
            if (string.IsNullOrEmpty(orderId))
            {
                return result;
            }
            var sql = @"SELECT A.JYH,A.XH,A.GZBJ,A.SPNM,A.SPNM_SSTC,A.ZPBJ,A.ZPSSSPNM,A.XSSL,A.ZPXSSL,A.LSJ,A.CXXSSL,A.CXJ,A.XSJ,A.ZKL,A.XSJE,A.XSYH,A.CBJE,B.JLDW
                          FROM XS_PZ A
                    INNER JOIN DA_SP B 
                            ON A.SPNM=B.SPNM
                         WHERE JYH=@JYS";
            var param = new DynamicParameters();
            param.Add("JYH", orderId);
            using (var db=new SqlConnection(ConfigInfo.ConnectionString))
            {
                result= db.Query<XS_PZ>(sql, param).ToList();
                return result;
            }
        }
    }
}
