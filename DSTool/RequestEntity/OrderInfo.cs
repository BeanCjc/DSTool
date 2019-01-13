using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSTool.DbData;

namespace DSTool.RequestEntity
{
    class OrderInfo
    {
        /// <summary>
        /// 订单主信息
        /// </summary>
        public O_order_history O_Order { get; set; }

        /// <summary>
        /// 订单项
        /// </summary>
        public List<O_order_item_history> O_Order_Item { get; set; }

        public static OrderInfo GetData(XS_PZ_ZB main, int deptId)
        {
            if (main == null || main.JYH == null || !int.TryParse(main.JYH, out int oid))
            {
                return new OrderInfo() { O_Order = new O_order_history(), O_Order_Item = new List<O_order_item_history>() };
            }
            var orderItemList = XS_PZ.GetListByOrderId(main.JYH);
            var orderItem = new List<O_order_item_history>();
            var sid = main.FDNM;
            foreach (var detail in orderItemList)
            {
                orderItem.Add(new O_order_item_history
                {
                    OiId = detail.JYH.ToString() + detail.XH.ToString(),//必须字段
                    ItemStatus = 2,//必须字段
                    OiType = 1,//必须字段
                    Price = detail.XSJ,//必须字段
                    Amount = detail.XSSL,//必须字段
                    OiTotal = detail.XSJE,//必须字段
                    OiCost = detail.XSJE,//必须字段
                    DId = detail.SPNM,//必须字段
                    DuId = Convert.ToInt32(DA_JLDW.GetIdByUnitName(detail.JLDW)),//必须字段
                    DmId = deptId,//必须字段
                    OId = Convert.ToInt32(detail.JYH),//必须字段
                    SId = sid//必须字段
                             //可选字段

                });
            }
            return new OrderInfo()
            {
                O_Order = new O_order_history()
                {
                    OId = oid,//必须字段
                    OrderStatus = 3,//必须字段
                    People = main.RS,//必须字段
                    Total = main.XSJE,//必须字段
                    Cost = main.XSJE,//必须字段
                    NewTime = Convert.ToDateTime(main.CZRQ.ToString() + main.CZSJ.ToString()),//必须字段
                    CheckoutTime = Convert.ToDateTime(main.CZRQ_XS.ToString() + main.CZSJ_XS.ToString()),//必须字段
                    OrderType = "1",//必须字段
                    SId = sid//必须字段
                             //可选字段

                },
                O_Order_Item = orderItem
            };
        }
    }
}
