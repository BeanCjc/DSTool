using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    class DishInfo
    {
        /// <summary>
        /// 品项信息
        /// </summary>
        public O_dish Dish { get; set; }

        /// <summary>
        /// 品项下的菜品数组
        /// </summary>
        public List<O_menudish> MenuDish { get; set; }
    }
}
