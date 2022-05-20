using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Controllers
{
    public class ApiEndpointController
    {
        public Entities.ApiEndpointStep ExecuteApiEndpointStep(Entities.ApiEndpointStep step, Dictionary<string, object>? result)
        {

            string originalURL = step.Url;
            //Reemplazo los parametros de la URL con los valores
            foreach (var item in step.RouteVariables)
            {
                if (result != null)
                {
                    if (result.Where(x => x.Key == item.Value).ToList().Count > 0)
                        step.Url = step.Url.Replace(item.Name, result.Where(x => x.Key == item.Value).FirstOrDefault().Value.ToString());
                }
                else
                    step.Url = step.Url.Replace(item.Name, item.Value.ToString());
            }

            // Api call
            Uri? requestUri = null;
            Uri.TryCreate(step.Url, UriKind.Absolute, out requestUri);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            request.ContentType = "application/json";
            request.Method = step.Verb;

            foreach (var item in step.Headers)
            {
                request.Headers.Add(item.Name, item.Value);

            }

            string jsonData = "";
            if (step.Verb == "POST" || step.Verb == "PUT")
            {
                if(result!=null)
                    jsonData = JsonConvert.SerializeObject(result);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                }
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                step.Response = response;
            }
            catch (WebException ex)
            {
                step.Response = (HttpWebResponse)ex.Response;
            }

            //guardamos log.
            saveLog(step, jsonData.ToString());
            step.Url = originalURL;

            return step;

        }

        public void saveLog(Entities.ApiEndpointStep pStep, string pJsonRequest)
        {
            string jsonResult = "";
            using (Stream responseStream = pStep.Response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                jsonResult = reader.ReadToEnd();
            }

            Entities.LogDBEntity logDBEntity = new Entities.LogDBEntity();

            logDBEntity.StepOrder = pStep.ExecutionOrder.ToString();
            logDBEntity.StepName = pStep.Name;
            logDBEntity.StepType = "Api Endpoint";
            logDBEntity.Endpoint = pStep.Url.ToString();
            logDBEntity.JsonRequestEndpoint = pJsonRequest;
            logDBEntity.Resultset = jsonResult.Replace("'","");
            logDBEntity.StatusCode = pStep.Response.StatusCode.ToString();

            TaskFileController.WriteLog(logDBEntity);
        }
    }
}
