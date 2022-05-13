using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Entities
{
    public class LogDbConnection
    {
        public String DatabaseName { get; set; } = String.Empty;
        public String Schema { get; set; } = String.Empty;
        public String TableName { get; set; } = String.Empty;
        public String ConnectionString { get; set; } = String.Empty;
        public String AdoNetAssemblyName { get; set; } = String.Empty;
        public String AdoNetConnectionTypeName { get; set; } = String.Empty;
        public String AdoNetCommandTypeName { get; set; } = String.Empty;
        public int AdoNetCommandTimeOut { get; set; } = 0;
    }
}
