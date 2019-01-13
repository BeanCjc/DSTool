using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    class O_menudish
    {
        /// <summary>
        /// 品项ID
        /// </summary>
        public string DId { get; set; }

        /// <summary>
        /// 品项单位ID
        /// </summary>
        public string DuId { get; set; }

        /// <summary>
        /// 品项做法ID
        /// </summary>
        public string DpId { get; set; }

        /// <summary>
        /// 菜品价格
        /// </summary>
        public decimal Price { get; set; }
    }
}
