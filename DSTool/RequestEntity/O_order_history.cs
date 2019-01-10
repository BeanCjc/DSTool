using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 订单信息
    /// </summary>
    class O_order_history
    {
        /// <summary>
        /// 订单ID 主键 NO
        /// </summary>
        public int OId { get; set; }

        /// <summary>
        /// 订单状态（0无效,1待核算,2已算单,3已结单,4退菜取消 Default 1 传2 NO
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 就餐人数 Default 1 NO
        /// </summary>
        public int People { get; set; }

        /// <summary>
        /// 已付款金额 Default 0.0 NO
        /// </summary>
        public decimal PayedAmount { get; set; }

        /// <summary>
        /// 订单有效收入 Default 0.0 NO
        /// </summary>
        public decimal RealRecieve { get; set; }

        /// <summary>
        /// 应收 Default 0.0 NO
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// 实收(包含实收品项,实收服务费,实收包间费) Default 0.0 NO
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 应收品项 Default 0.0 NO
        /// </summary>
        public decimal TotalDish { get; set; }

        /// <summary>
        /// 实收品项 Default 0.0 NO
        /// </summary>
        public decimal CostDish { get; set; }

        /// <summary>
        /// 应收服务费 Default 0.0 NO
        /// </summary>
        public decimal TotalService { get; set; }

        /// <summary>
        /// 实收服务费 Default 0.0 NO
        /// </summary>
        public decimal CostService { get; set; }

        /// <summary>
        /// 开台时间 NO
        /// </summary>
        public DateTime NewTime { get; set; }

        /// <summary>
        /// 结账时间 YES
        /// </summary>
        public DateTime CheckoutTime { get; set; }

        /// <summary>
        /// 备注 YES
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 餐段市别ID YES
        /// </summary>
        public int BrId { get; set; }

        /// <summary>
        /// 订单桌台数 YES
        /// </summary>
        public int TableNum { get; set; }

        /// <summary>
        /// 就餐方式(1堂食2打包3外卖) Default 1 NO
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 门店ID sls_shop,外键 NO
        /// </summary>
        public int SId { get; set; }

        /// <summary>
        /// 支付合计 YES
        /// </summary>
        public decimal PayMoney { get; set; }

        /// <summary>
        /// 额外折让 Default '0.00' YES
        /// </summary>
        public decimal ExtraDiscount { get; set; }

        /// <summary>
        /// 折扣率 Default '0.00' YES
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// 桌台 YES
        /// </summary>
        public string Tables { get; set; }

        /// <summary>
        /// 原订单号 YES
        /// </summary>
        public int CancelOriginoId { get; set; }

        /// <summary>
        /// 反结账单号 YES
        /// </summary>
        public int CancelantioId { get; set; }

        /// <summary>
        /// 新单号 YES
        /// </summary>
        public int CancelnewoId { get; set; }

        /// <summary>
        /// 成本额 Default '0.00' YES
        /// </summary>
        public decimal CostMoney { get; set; }

        /// <summary>
        /// 手动抹零 YES
        /// </summary>
        public decimal MaLing { get; set; }

        /// <summary>
        /// 系统抹零 YES
        /// </summary>
        public decimal Rounding { get; set; }

        /// <summary>
        /// 标记问题订单 NO
        /// </summary>
        public int Flag { get; set; }
    }
}
