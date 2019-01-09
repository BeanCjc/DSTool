using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    /// <summary>
    /// 班次管理日志
    /// </summary>
    class XS_BCGL
    {
        /// <summary>
        /// 开始日期 PK NO
        /// </summary>
        public int KSRQ { get; set; }

        /// <summary>
        /// 开始时间 PK NO
        /// </summary>
        public int KSSJ { get; set; }

        /// <summary>
        /// 开始人 PK NO
        /// </summary>
        public int CZYNM_KS { get; set; }

        /// <summary>
        /// 班次内码 NO
        /// </summary>
        public int BCNM { get; set; }

        /// <summary>
        /// 班次状态 0未班结，1已班结 NO
        /// </summary>
        public int BCZT { get; set; }

        /// <summary>
        /// 销售日期 NO
        /// </summary>
        public int XSRQ { get; set; }

        /// <summary>
        /// 结束日期 default 0 NO
        /// </summary>
        public int JSRQ { get; set; }

        /// <summary>
        /// 结束时间 default 0 NO
        /// </summary>
        public int JSSJ { get; set; }

        /// <summary>
        /// 结束人 YES
        /// </summary>
        public int CZYNM_JS { get; set; }

        /// <summary>
        /// 日结日期 default 0 NO
        /// </summary>
        public int RJRQ { get; set; }
    }
}
