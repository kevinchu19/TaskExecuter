using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Entities
{
    public class DBQueryStep: Step
    {
        public String Sentence { get; set; } = String.Empty;
        public String DbConnectionName { get; set; } = String.Empty;
        public Dictionary<String, String> QueryParameters { get; set; } = new Dictionary<String, String>();
        public System.Data.IDataReader? DataResult { get; set; }
        
        public DBQueryStep()
        {
            base.Type = "DBQueryStep";
        }
    }
}
