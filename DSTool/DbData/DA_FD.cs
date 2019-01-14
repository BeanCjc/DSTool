using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace DSTool.DbData
{
    /// <summary>
    /// DA_FD(分店档案)分店内码范围从100到999，方便计算分店的自营内码
    /// </summary>
    class DA_FD
    {
        /// <summary>
        /// 分店内码0代表总部
        /// </summary>
        public int FDNM { get; set; }

        /// <summary>
        /// 分店代码
        /// </summary>
        public string FDDM { get; set; }

        /// <summary>
        /// 分店名称
        /// </summary>
        public string FD { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        public int CLRQ { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string DZ { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string DH { get; set; }

        /// <summary>
        /// 负责人
        /// </summary>
        public string FZR { get; set; }

        /// <summary>
        /// 负责人手机
        /// </summary>
        public string FZRSJ { get; set; }

        /// <summary>
        /// 是否使用总部会员卡
        /// </summary>
        public int SFSYZBHYK { get; set; }

        /// <summary>
        /// 是否用总部指导零售价覆盖分店价格
        /// </summary>
        public int LSJSFFG { get; set; }

        /// <summary>
        /// 总部商品是否允许修改价格
        /// </summary>
        public int ZBSPSFYXXGJG { get; set; }

        /// <summary>
        /// 是否允许设置自营商品
        /// </summary>
        public int SFYXZYSP { get; set; }

        /// <summary>
        /// 享受价格（直营配送价或者加盟配送价 0直营配送价，1加盟配送价，2-门店零售价，3批发价
        /// </summary>
        public int XSJG { get; set; }

        /// <summary>
        /// 管理员工 0否1是 管理的话分店不允许建立员工
        /// </summary>
        public int GLYG { get; set; }

        /// <summary>
        /// 总部管理促销 0否1是 管理的话分店不允许建促销
        /// </summary>
        public int GLCX { get; set; }

        /// <summary>
        /// 上传销售数据 0否，1是
        /// </summary>
        public int SCXSSJ { get; set; }

        /// <summary>
        /// 下载总部商品 0否，1是
        /// </summary>
        public int XZZBSP { get; set; }

        /// <summary>
        /// 仓库标记（总部为默认仓库，ckbj为0）0否，1是
        /// </summary>
        public int CKBJ { get; set; }

        /// <summary>
        /// 是否连接网络 0否1是
        /// </summary>
        public int SFLJWL { get; set; }

        /// <summary>
        /// 启用标记
        /// </summary>
        public int QYBJ { get; set; }

        /// <summary>
        /// 上传后分店数据保留天数
        /// </summary>
        public int SJBLTS { get; set; }

        /// <summary>
        /// 是否上传销售流水
        /// </summary>
        public int SFSCXSLS { get; set; }

        /// <summary>
        /// 是否覆盖指定成本价
        /// </summary>
        public int SFFGZHCBJ { get; set; }

        /// <summary>
        /// 所属区域内码
        /// </summary>
        public int QYNM { get; set; }

        /// <summary>
        /// 生产配方是否可见
        /// </summary>
        public int SCPFSFKJ { get; set; }

        /// <summary>
        /// 允许修改会员卡折扣率
        /// </summary>
        public int YXXGHYKZKL { get; set; }

        /// <summary>
        /// 启用充值规则
        /// </summary>
        public int QYCZGZ { get; set; }

        /// <summary>
        /// 充值方式
        /// </summary>
        public int CZFS { get; set; }

        /// <summary>
        /// 充值基数
        /// </summary>
        public int CZJS { get; set; }

        /// <summary>
        /// 充值赠送金额
        /// </summary>
        public int CZZSJE { get; set; }

        /// <summary>
        /// 启用积分
        /// </summary>
        public int QYJF { get; set; }

        /// <summary>
        /// 消费基数
        /// </summary>
        public decimal XFJS { get; set; }

        /// <summary>
        /// 单位消费基数产生的积分数
        /// </summary>
        public int JFS { get; set; }

        /// <summary>
        /// 积分付款金额参与积分
        /// </summary>
        public int JFFKJECYJF { get; set; }

        /// <summary>
        /// 赠券参与积分
        /// </summary>
        public int ZQCYJF { get; set; }

        /// <summary>
        /// 仅储值卡付款参与积分
        /// </summary>
        public int JCZKFKCYJF { get; set; }

        /// <summary>
        /// 覆盖收银按键设置
        /// </summary>
        public int FGSYKJ { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime XGSJ { get; set; }

        public static DA_FD GetById(int id)
        {
            var sql= @"select FDNM,FD,FDDM,QYBJ,QYNM,DZ,XGSJ from DA_FD where FDNM=@FDNM";
            var param = new DynamicParameters();
            param.Add("FDNM", id);
            using (var db=new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_FD>(sql, param).FirstOrDefault();
            }
        }

        public static List<DA_FD> GetListByLastTime(string lastTime)
        {
            var sql = @"select FDNM,FD,FDDM,QYBJ,QYNM,DZ,XGSJ from DA_FD where XGSJ>=@XGSJ";
            var param = new DynamicParameters();
            param.Add("XGSJ", lastTime);
            using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            {
                return db.Query<DA_FD>(sql, param).ToList();
            }
            //var sql = @"select FDNM,FD,FDDM,QYBJ,QYNM,DZ from DA_FD";
            //using (var db = new SqlConnection(ConfigInfo.ConnectionString))
            //{
            //    return db.Query<DA_FD>(sql).ToList();
            //}
        }
    }
}
