using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 品项类型信息
    /// </summary>
    class O_dish_kind
    {
        /// <summary>
        /// 主键ID NO
        /// </summary>
        public int DkId { get; set; }

        /// <summary>
        /// 父级品项类型 o_dish_kind,外键,如果为空传null YES
        /// </summary>
        public int PdkId { get; set; }

        /// <summary>
        /// 品项类型名称 NO
        /// </summary>
        public string DishKind { get; set; }

        /// <summary>
        /// 是否有效 1-有效，0-无效 NO
        /// </summary>
        public int Valid { get; set; }

        /// <summary>
        /// 速记码 NO
        /// </summary>
        public string SNo { get; set; }

        /// <summary>
        /// 品牌ID sls_brand,外键 NO
        /// </summary>
        public int BId { get; set; }

        /// <summary>
        /// 更新时间 YES
        /// </summary>
        public string UTime { get; set; }

        /// <summary>
        /// 出品部门ID c_department,外键,如果为空传null YES
        /// </summary>
        public int DmId { get; set; }
    }
}
