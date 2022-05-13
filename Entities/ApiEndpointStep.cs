using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace TaskExecuter.Entities
{
    public class ApiEndpointStep :Step
    {
        internal HttpWebResponse? Response { get; set; }
        private Dictionary<String, String> ResponseResults { get; set; } = new Dictionary<String, String>();
        public String Url { get; set; } = String.Empty;
        public String Verb { get; set; } = String.Empty;
        public List<ApiEndpointHeader> Headers { get; set; } = new List<ApiEndpointHeader>();
        public List<ApiEndpointParam> RouteVariables { get; set; } = new List<ApiEndpointParam>();


        public ApiEndpointStep()
        {
            base.Type = "ApiEndpointStep";
        }
    }
    public class ApiEndpointHeader
    {
        public String Name { get; set; } = String.Empty;
        public String Value { get; set; } = String.Empty;
    }
    public class ApiEndpointParam
    {
        public String Name { get; set; } = String.Empty;
        public String Value { get; set; } = String.Empty;
    }
}
