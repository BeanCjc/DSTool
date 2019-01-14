using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 品项信息
    /// </summary>
    class O_dish
    {
        /// <summary>
        /// ID NO
        /// </summary>
        public int DId { get; set; }

        /// <summary>
        /// 品项类型ID o_dish_kind,外键	No
        /// </summary>
        public int DkId { get; set; }

        /// <summary>
        /// 速记码 NO
        /// </summary>
        public string SNo { get; set; }

        /// <summary>
        /// 品牌ID sls_brand,外键 No
        /// </summary>
        public string BId { get; set; }

        /// <summary>
        /// 添加时间 YES
        /// </summary>
        public string CTime { get; set; }

        /// <summary>
        /// 出品部门ID c_department,外键 NO
        /// </summary>
        public int DmId { get; set; }

        /// <summary>
        /// 品项 NO
        /// </summary>
        public string Dish { get; set; }

        /// <summary>
        /// 拼音首字母 NO
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 状态 1-有效，2-无效 NO
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 顺序 默认为1 NO
        /// </summary>
        public int Seq { get; set; } = 1;

        /// <summary>
        /// 品项属性 1-普通品项 2-套餐 3-自助餐 NO
        /// </summary>
        public int DishType { get; set; } = 1;

        /// <summary>
        /// 是否自动出库 默认1 NO
        /// </summary>
        public int BAutoDeliver { get; set; } = 1;

        /// <summary>
        /// 是否主料 默认0  1-是 0-否 NO
        /// </summary>
        public int BMain { get; set; } = 0;

        /// <summary>
        /// 打折条件 NO
        /// </summary>
        public int DiscountType { get; set; } = 1;
    }
}
