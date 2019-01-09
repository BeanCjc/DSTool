using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 餐段信息
    /// </summary>
    class O_business_range
    {
        /// <summary>
        /// 名称 NO
        /// </summary>
        public string Businessrange { get; set; }

        /// <summary>
        /// 开始时间 NO
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 起始时间 NO
        /// </summary>
        public DateTime ShowStartTime { get; set; }

        /// <summary>
        /// 结束时间 NO
        /// </summary>
        public DateTime ShowEndTime { get; set; }

        /// <summary>
        /// 状态 有效-1 无效-0 NO
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 品牌 sls_brand,外键 NO
        /// </summary>
        public string BId { get; set; }

        /// <summary>
        /// 门店id sls_shop,外键,如果餐段和品牌绑定该字段不用传,餐段和门店绑定该字段必须传 YES
        /// </summary>
        public int SId { get; set; }

        /// <summary>
        /// pos主键 NO
        /// </summary>
        public int CbrId { get; set; }






    }
}
