using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    /// <summary>
    /// 收银机档案
    /// </summary>
    class DA_SYJ
    {
        /// <summary>
        /// 收银机内码 PK ID NO
        /// </summary>
        public int SYJNM { get; set; }

        /// <summary>
        /// 收银机名称(报表中显示) UNIQUEKEY NO
        /// </summary>
        public string SYJ { get; set; }

        /// <summary>
        /// 计算机名 UNIQUEKEY NO
        /// </summary>
        public string JSJM { get; set; }

        /// <summary>
        /// 登陆方式（手工输入密码或者刷卡登陆不需密码） 0手工，1刷卡 NO
        /// </summary>
        public int DLFS { get; set; }

        /// <summary>
        /// 屏蔽桌面 0否，1是 NO
        /// </summary>
        public int PBZM { get; set; }

        /// <summary>
        /// 小票打印方式 0调驱动，1直接 NO
        /// </summary>
        public int DYFS { get; set; }

        /// <summary>
        /// 小票打印机名 YES
        /// </summary>
        public string DYJM { get; set; }

        /// <summary>
        /// 打印端口 NO
        /// </summary>
        public string DYDK { get; set; }

        /// <summary>
        /// 小票份数 default 0 No
        /// </summary>
        public int XPFS { get; set; }

        /// <summary>
        /// 充值小票份数 default 0 NO
        /// </summary>
        public int CZXPFS { get; set; }

        /// <summary>
        /// 打印大类合计 0否，1是 NO
        /// </summary>
        public int DYDLHJ { get; set; }

        /// <summary>
        /// 打印宽度 57，75，80，100表示套打 NO
        /// </summary>
        public int DYKD { get; set; }

        /// <summary>
        /// 开钱箱 0否1接打印机，2接钱箱卡 NO
        /// </summary>
        public int KQX { get; set; }

        /// <summary>
        /// 客显端口号 1com1，2com2，3com3，4com4，5com5，6com6 NO
        /// </summary>
        public int KXDKH { get; set; }

        /// <summary>
        /// 客显型号 NO
        /// </summary>
        public int KXXH { get; set; }

        /// <summary>
        /// IC卡端口号 NO
        /// </summary>
        public int ICDKH { get; set; }

        /// <summary>
        /// IC卡类型 NO
        /// </summary>
        public int ICLX { get; set; }

        /// <summary>
        /// 注册码 YES
        /// </summary>
        public string ZCM { get; set; }

        /// <summary>
        /// 手工输入桌台 1是，不需建区域桌台档案，只是送厨用，2，使用桌台档案 NO
        /// </summary>
        public int SGSRZT { get; set; }

        /// <summary>
        /// 允许结帐 0否，1是 NO
        /// </summary>
        public int YXJZ { get; set; }

        /// <summary>
        /// 条码打印机名（用于珍珠奶茶） YES
        /// </summary>
        public string TMDYJM { get; set; }

        /// <summary>
        /// 条码打印份数 default 0 NO
        /// </summary>
        public int TMDYFS { get; set; }

        /// <summary>
        /// 默认输入方式 0磁卡，1商品条码 NO
        /// </summary>
        public int MRSRFS { get; set; }

        /// <summary>
        /// 带切刀 0否，1是 NO
        /// </summary>
        public int DQD { get; set; }

        /// <summary>
        /// 启用标记 1是，0否 NO
        /// </summary>
        public int QYBJ { get; set; }

        /// <summary>
        /// 必须输入桌台 0否1是 NO
        /// </summary>
        public int BXSRZT { get; set; }

        /// <summary>
        /// 结账打印机名2，可以同时打印的另一个打印机 YES
        /// </summary>
        public string DYJM2 { get; set; }
    }
}
