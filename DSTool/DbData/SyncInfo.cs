using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSTool.DbData
{
    class SyncInfo
    {
        public string TableName { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public bool IsSynced { get; set; }

    }
}
