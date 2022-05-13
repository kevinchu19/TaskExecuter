using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Entities
{
    public class DbConnection
    {
        public String Name { get; set; } = String.Empty;
        public String Driver { get; set; } = String.Empty;
        public String ConnectionString { get; set; } = String.Empty;
        public String AdoNetAssemblyName { get; set; } = String.Empty;
        public String AdoNetConnectionTypeName { get; set; } = String.Empty;
        public String AdoNetCommandTypeName { get; set; } = String.Empty;
        public int AdoNetCommandTimeOut { get; set; } = 0;
        public String ParameterPrefix { get; set; } = String.Empty;
        public String PkFunction { get; set; } = String.Empty;
    }
}
