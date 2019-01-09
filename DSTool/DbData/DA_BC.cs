using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    /// <summary>
    /// 班次档案
    /// </summary>
    class DA_BC
    {
        /// <summary>
        /// 班次内码 PK NO
        /// </summary>
        public int BCNM { get; set; }

        /// <summary>
        /// 分店内码 PK FK NO
        /// </summary>
        public int FDNM { get; set; }

        /// <summary>
        /// 班次名称 NO
        /// </summary>
        public string BC { get; set; }

        /// <summary>
        /// 启用标记 1启用，0不启用 NO
        /// </summary>
        public int QYBJ { get; set; }
    }
}
