using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 菜品信息
    /// </summary>
    class O_menu_dish
    {
        /// <summary>
        /// 菜品ID o_dish,外键 NO
        /// </summary>
        public int DId { get; set; }

        /// <summary>
        /// 品项单位ID o_dish_unit,外键 NO
        /// </summary>
        public int Duid { get; set; }

        /// <summary>
        /// 价格 NO
        /// </summary>
        public int Price { get; set; }
    }
}
