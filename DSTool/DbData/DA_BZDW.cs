using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    /// <summary>
    /// 包装单位档案
    /// </summary>
    class DA_BZDW
    {
        /// <summary>
        /// 包装单位内码 PK，ID NO
        /// </summary>
        public int BZDWNM { get; set; }

        /// <summary>
        /// 包装单位名称 UNIQUEKEY NO
        /// </summary>
        public string BZDW { get; set; }

        /// <summary>
        /// 共享标记 NO
        /// </summary>
        public int GXBJ { get; set; }

        /// <summary>
        /// 所属商品内码 YES
        /// </summary>
        public int SPNM { get; set; }

        /// <summary>
        /// 相当于基本单位的数量 NO
        /// </summary>
        public decimal SL { get; set; }
    }
}
