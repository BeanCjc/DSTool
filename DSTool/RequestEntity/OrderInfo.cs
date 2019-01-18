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

        /// <summary>
        /// 日结
        /// </summary>
        public int DayDone { get; set; } = 1;

        public static OrderInfo GetData(XS_PZ_ZB main, int deptId)
        {
            if (main == null || main.JYH?.Trim() == null /*|| !int.TryParse(main.JYH, out int oid)*/)
            {
                return new OrderInfo() { O_Order = new O_order_history(), O_Order_Item = new List<O_order_item_history>() };
            }
            var orderItemList = XS_PZ.GetListByOrderId(main.JYH.Trim());
            var orderItem = new List<O_order_item_history>();
            var sid = main.FDNM;
            foreach (var detail in orderItemList)
            {
                orderItem.Add(new O_order_item_history
                {
                    OiId = detail.JYH.Trim() + detail.XH.ToString(),//必须字段
                    ItemStatus = 2,//必须字段
                    OiType = 1,//必须字段
                    Price = detail.XSJ,//必须字段
                    Amount = detail.XSSL,//必须字段
                    OiTotal = detail.XSJE,//必须字段
                    OiCost = detail.XSJE,//必须字段
                    DId = detail.SPNM,//必须字段
                    DuId = Convert.ToInt32(DA_JLDW.GetIdByUnitName(detail.JLDW)),//必须字段
                    DmId = deptId,//必须字段
                    OId = detail.JYH.Trim(),//必须字段
                    SId = sid//必须字段
                             //可选字段

                });
            }
            return new OrderInfo()
            {
                O_Order = new O_order_history()
                {
                    OId = main.JYH.Trim(),//必须字段
                    OrderStatus = 3,//必须字段
                    People = main.RS,//必须字段
                    Total = main.XSJE,//必须字段
                    Cost = main.XSJE,//必须字段
                    NewTime = Common.IntToDateTime(main.CZRQ, main.CZSJ),//必须字段
                    CheckoutTime = Common.IntToDateTime(main.CZRQ_XS, main.CZSJ_XS),//必须字段
                    OrderType = "1",//必须字段
                    SId = sid//必须字段
                             //可选字段

                },
                O_Order_Item = orderItem
            };
        }

        /// <summary>
        /// 按门店进行分类集合
        /// </summary>
        /// <param name="mains"></param>
        /// <param name="deptId"></param>
        /// <returns></returns>
        public static List<OrderInfoList> GetListData(List<XS_PZ_ZB> mains, int deptId)
        {
            var result = new List<OrderInfoList>();
            foreach (var main in mains)
            {
                var orderItemList = XS_PZ.GetListByOrderId(main.JYH.Trim());
                var orderItem = new List<O_order_item_history>();
                var sid = main.FDNM;
                foreach (var detail in orderItemList)
                {
                    orderItem.Add(new O_order_item_history
                    {
                        OiId = detail.JYH.Trim() + detail.XH.ToString(),//必须字段
                        ItemStatus = 2,//必须字段
                        OiType = 1,//必须字段
                        Price = detail.XSJ,//必须字段
                        Amount = detail.XSSL,//必须字段
                        OiTotal = detail.XSJE,//必须字段
                        OiCost = detail.XSJE,//必须字段
                        DId = detail.SPNM,//必须字段
                        DuId = Convert.ToInt32(DA_JLDW.GetIdByUnitName(detail.JLDW)),//必须字段
                        DmId = deptId,//必须字段
                        OId = detail.JYH.Trim(),//必须字段
                        SId = sid//必须字段
                                 //可选字段

                    });
                }
                var ddd = result.FirstOrDefault(t => t.Sid == sid&&t.Date== Common.IntToDateTime(main.CZRQ, main.CZSJ).ToString("yyyy-MM-dd"));
                if (ddd != null)
                {
                    ddd.OrderInfos.Add(new OrderInfo()
                    {
                        O_Order = new O_order_history()
                        {
                            OId = main.JYH.Trim(),//必须字段
                            OrderStatus = 3,//必须字段
                            People = main.RS,//必须字段
                            Total = main.XSJE,//必须字段
                            Cost = main.XSJE,//必须字段
                            NewTime = Common.IntToDateTime(main.CZRQ, main.CZSJ),//必须字段
                            CheckoutTime = Common.IntToDateTime(main.CZRQ_XS, main.CZSJ_XS),//必须字段
                            OrderType = "1",//必须字段
                            SId = sid//必须字段
                                     //可选字段

                        },
                        O_Order_Item = orderItem
                    });
                }
                else
                {
                    result.Add(new OrderInfoList()
                    {
                        Sid = sid,
                        Date= Common.IntToDateTime(main.CZRQ_XS, main.CZSJ_XS).ToString("yyyy-MM-dd"),
                        OrderInfos = new List<OrderInfo>()
                        {
                            new OrderInfo()
                            {
                                O_Order = new O_order_history()
                                {
                                    OId = main.JYH.Trim(),//必须字段
                                    OrderStatus = 3,//必须字段
                                    People = main.RS,//必须字段
                                    Total = main.XSJE,//必须字段
                                    Cost = main.XSJE,//必须字段
                                    NewTime = Common.IntToDateTime(main.CZRQ, main.CZSJ),//必须字段
                                    CheckoutTime = Common.IntToDateTime(main.CZRQ_XS, main.CZSJ_XS),//必须字段
                                    OrderType = "1",//必须字段
                                    SId = sid//必须字段
                                             //可选字段

                                },
                                O_Order_Item = orderItem
                            }
                        }
                    });
                }
            }
            return result;
        }
    }
    class OrderInfoList
    {
        public int Sid { get; set; }

        public string Date { get; set; }

        public List<OrderInfo> OrderInfos { get; set; }
        
        public List<DeleteObject> IdLists { get; set; }
    }
}
