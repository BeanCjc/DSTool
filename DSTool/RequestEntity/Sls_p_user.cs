using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    /// <summary>
    /// 用户信息
    /// </summary>
    class Sls_p_user
    {
        /// <summary>
        /// pos主键 pos用户表主键 NO
        /// </summary>
        public int CsuId { get; set; }

        /// <summary>
        /// 账号 唯一性 NO
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码 NO
        /// </summary>
        public int Pwd { get; set; }

        /// <summary>
        /// 状态 1-有效,2-无效 NO
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 名字 NO
        /// </summary>
        public string RName { get; set; }

        /// <summary>
        /// 别名 YES
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 注册时间 YES
        /// </summary>
        public string RegistTime { get; set; }

        /// <summary>
        /// 电话 YES
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 邮箱 YES
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 岗位id YES
        /// </summary>
        public string Rlids { get; set; }

        /// <summary>
        /// 接口 YES
        /// </summary>
        public string Fpk { get; set; }

        /// <summary>
        /// 更新时间 YES
        /// </summary>
        public string UpdateTime { get; set; }

        /// <summary>
        /// 初始密码 YES
        /// </summary>
        public string InitPwd { get; set; }

        /// <summary>
        /// 会员名 YES
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 管理品牌权限 YES
        /// </summary>
        public string BIds { get; set; }

        /// <summary>
        /// 操作类型 默认1(添加) YES
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 邮箱密码 YES
        /// </summary>
        public string EmailPwd { get; set; }

        /// <summary>
        /// 错误次数 超过五次会被锁定账号 YES
        /// </summary>
        public int ErrorTimes { get; set; }

        /// <summary>
        /// 是否锁定账户 默认为1，为0时账户被锁定 YES
        /// </summary>
        public int Locks { get; set; }

        /// <summary>
        /// 邮箱类型 1-163邮箱，2-阿里云邮箱 YES
        /// </summary>
        public int EmailType { get; set; }
    }
}
