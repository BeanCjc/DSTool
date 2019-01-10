using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 订单项
    /// </summary>
    class O_order_item_history
    {
        /// <summary>
        /// 订单项ID 主键 NO
        /// </summary>
        public int OiId { get; set; }

        /// <summary>
        /// 项状态(0-未确认 1-重量待调整 2-确认 3-退菜) Default 2 NO
        /// </summary>
        public int ItemStatus { get; set; }

        /// <summary>
        /// 单项属性 1是普通的 2 是宴席 3 是套餐 4 是套餐子项 Defalut 1 NO
        /// </summary>
        public int OiType { get; set; }

        /// <summary>
        /// 是否赠菜 Default 0 NO
        /// </summary>
        public int BGift { get; set; }

        /// <summary>
        /// 单项备注 NO
        /// </summary>
        public string OiMemo { get; set; }

        /// <summary>
        /// 单价 Default0.00 NO
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 数量 YES
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 单项应收 Default0.00 NO
        /// </summary>
        public decimal OiTotal { get; set; }

        /// <summary>
        /// 单项实收 Default0.00 NO
        /// </summary>
        public decimal OiCost { get; set; }

        /// <summary>
        /// 出库时间 YES
        /// </summary>
        public DateTime DeliverTime { get; set; }

        /// <summary>
        /// 品项ID o_dish,外键 NO
        /// </summary>
        public int DId { get; set; }

        /// <summary>
        /// 品项单位ID o_dish_unit,外键 YES
        /// </summary>
        public int DuId { get; set; }

        /// <summary>
        /// 出品部门ID c_department,外键 YES
        /// </summary>
        public int DmId { get; set; }

        /// <summary>
        /// 订单ID NO
        /// </summary>
        public int OId { get; set; }

        /// <summary>
        /// 门店ID sls_shop,外键 NO
        /// </summary>
        public int SId { get; set; }

        /// <summary>
        /// 订单项成本 Default 0.0 YES
        /// </summary>
        public decimal CostMoney { get; set; }

        /// <summary>
        /// 辅助单位 YES
        /// </summary>
        public int AssistDuId { get; set; }

        /// <summary>
        /// 辅助数量 Default 0.0 YES
        /// </summary>
        public decimal AssistAmount { get; set; }
    }
}
