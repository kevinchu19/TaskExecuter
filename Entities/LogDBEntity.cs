using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Entities
{
    public class LogDBEntity
    {
        public DateTime ExecutionDate { get; set; }
        public string StepName { get; set; }
        public string StepType { get; set; }
        public string StepOrder { get; set; }
        public string StoredProcedure { get; set; }
        public string Parameters { get; set; }
        public string Endpoint { get; set; }
        public string JsonRequestEndpoint { get; set; }
        public string Resultset { get; set; }  
        public string StatusCode { get; set; } 

        public LogDBEntity()
        {
            ExecutionDate = DateTime.Now;
            StepName = "";
            StepType = "";
            StepOrder = "";
            StoredProcedure = "";
            Parameters = "";
            Endpoint = "";
            Resultset = "";
            StatusCode = "";
            JsonRequestEndpoint = "";

        }
    }
}
