using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 品项单位信息
    /// </summary>
    class O_dish_unit
    {
        /// <summary>
        /// 主键 ID NO
        /// </summary>
        public int DuId { get; set; }

        /// <summary>
        /// 品项类型名称 NO
        /// </summary>
        public string DishUnit { get; set; }

        /// <summary>
        /// 是否有效 1-有效，0-无效 NO
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 速记码 YES
        /// </summary>
        public string SNo { get; set; }

        /// <summary>
        /// 更新时间 YES
        /// </summary>
        public string UTime { get; set; }
    }
}
