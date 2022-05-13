using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Entities
{
    public class TaskExecution
    {

        public string Name { get; set; } = String.Empty;
        public List<Step> Steps { get; set; } = new List<Step> { };
        public List<DbConnection> DbConnections { get; set; } = new List<DbConnection> { };
        public LogDbConnection LogDbConnection { get; set; } = new LogDbConnection();
        public Dictionary<String, String> ExternalParameters { get; set; } = new Dictionary<String, String>();

    }
}
