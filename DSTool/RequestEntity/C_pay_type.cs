using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 支付方式信息 -22	商家优惠   -21支付宝补贴  -20罚款  -19赠券免找  -18支付宝  -17百度支付
    ///-16	微信支付   -15离线刷卡  -12	积分卡   -11储值卡   -9挂账     -8订金
    ///-5微信支付  -4定金 -3代金券差额  -2	会员  -1现金
    /// </summary>
    class C_pay_type
    {
        /// <summary>
        /// 支付方式名称 NO
        /// </summary>
        public string PayType { get; set; }

        /// <summary>
        /// 状态 1-有效,2-无效 NO
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string UTime { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 是否有效实收 1-有效实收,2-无效实收 YES
        /// </summary>
        public int BReal { get; set; }

        /// <summary>
        /// 顺序 NO
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// 是否添加到物流系统 1-是,0-否 NO
        /// </summary>
        public int BSetChain { get; set; }

        /// <summary>
        /// pos主键 NO
        /// </summary>
        public int CptId { get; set; }

    }
}
