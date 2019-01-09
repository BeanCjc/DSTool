using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.RequestEntity
{
    class ResponseResult
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
    }
}
