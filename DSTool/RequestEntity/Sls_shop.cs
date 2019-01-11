using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    class Sls_shop
    {
        /// <summary>
        /// 名称 NO
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 门店代码 YES
        /// </summary>
        public string ShopCode { get; set; }

        /// <summary>
        /// 门店编码 NO
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 状态 1-有效  -1-无效 NO
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 区域id sls_area,外键 NO
        /// </summary>
        public int AId { get; set; }

        /// <summary>
        /// 品牌id sls_brand,外键,如果门店类型是配送中心传null YES
        /// </summary>
        public int? BId { get; set; }

        /// <summary>
        /// 类型 3-配送中心  4-直营店 5-加盟店  6-外销客户 NO
        /// </summary>
        public int SType { get; set; }

        /// <summary>
        /// 配送中心id sls_shop,外键,如果门店类型是配送中心传null YES
        /// </summary>
        public int? SlsId { get; set; }

        /// <summary>
        /// 顺序 YES
        /// </summary>
        public int Seq { get; set; }

        /// <summary>
        /// 地址 YES
        /// </summary>
        public string ShopAdd { get; set; }

        /// <summary>
        /// 是否是总部配送中心 默认为0-否,只有餐行健pos传该字段 NO
        /// </summary>
        public int BSetMailDelivery { get; set; } = 0;

        /// <summary>
        /// 是否由配送结算 0-否 1-是 默认为1-是,只有餐行健pos传该字段 NO
        /// </summary>
        public int BSetDelivery { get; set; } = 1;

        /// <summary>
        /// 开店时间 YES
        /// </summary>
        public DateTime? OpenTime { get; set; }

        /// <summary>
        /// 门店营业面积 YES
        /// </summary>
        public int? BusinessArea { get; set; }

        /// <summary>
        /// 门店桌台数 YES
        /// </summary>
        public int? SumTable { get; set; }

        /// <summary>
        /// 门店员工人数 YES
        /// </summary>
        public int? People { get; set; }

        /// <summary>
        /// pos主键 NO
        /// </summary>
        public int CsId { get; set; }
    }
}
