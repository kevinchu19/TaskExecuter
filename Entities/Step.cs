using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Entities
{
    public class Step
    {
        public int ExecutionOrder { get; set; }
        public String Name { get; set; } = String.Empty;
        public String Type { get; set; } = String.Empty;
        public Dictionary<String, String> ExternalParameters { get; set; } = new Dictionary<String, String>();
        public List<Dictionary<String, object?>> ReturnResult { get; set; } = new List<Dictionary<String, object?>>();
        //public object? ReturnResult { get; set; }
        public String? PreviousStepReference { get; set; } = String.Empty;
    }
}
