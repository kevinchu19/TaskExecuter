using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TaskExecuter.Entities;

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
            request.Timeout = step.TimeoutInSecs * 1000;
            request.ContentType =step.ContentType != null? step.ContentType:"application/json";
            request.Method = step.Verb;


            foreach (var item in ResolveValues<ApiEndpointHeader>("headers",step.Headers, result))
            {
                request.Headers.Add(item.Name, item.Value);

            }
            //Para application/x-www-form-urlencoded
            var data = new Dictionary<string, string>();

            foreach (var item in ResolveValues<ApiEndpointFormKey>("formKeys",step.FormKeys,result))
            {
                //TaskFileController.WriteLogFile(item.Value);
                data.Add(item.Name, item.Value);
            }

  
            string content = step.ContentType == "application/x-www-form-urlencoded" ? string.Join("&", data.Select(x => $"{Uri.EscapeDataString(x.Key)}={x.Value}")):"";
            
            

            if (step.Verb == "POST" || step.Verb == "PUT" || step.Verb == "PATCH")
            {
                if(result!=null )
                    if(result.Count() > 0)
                        content = JsonConvert.SerializeObject(result);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(content);
                }
            }
            DateTime inicioRequest = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                stopwatch.Stop();
                step.Response = response;
            }
            catch (WebException ex)
            {
                stopwatch.Stop();
                step.Response = (HttpWebResponse)ex.Response;
            }

            TimeSpan duracion = stopwatch.Elapsed;
            DateTime finRequest = inicioRequest.AddMilliseconds(duracion.TotalMilliseconds);
            //guardamos log.
            saveLog(step, content.ToString(), inicioRequest,finRequest);
            step.Url = originalURL;

            return step;

        }

        //Para Headers variables y forms
        private IEnumerable<T> ResolveValues <T>(string valueToSearch,List<T> values, Dictionary<string, object>? result)
        {
            //Si el paso anterior contiene un campo $headers con una coleccion, toma el json de dicho campo para armar los headers
            object dynamicValues;
            if (result != null)
            {

                if (result.TryGetValue($"${valueToSearch}", out dynamicValues))
                {
                    result.Remove($"${valueToSearch}");
                    try
                    {
                        
                        return JsonConvert.DeserializeObject<IEnumerable<T>>(dynamicValues.ToString());
                    }
                    catch (Exception ex)
                    {
                        return values;
                    }
                
                };

            }
            return values;
        }

        public void saveLog(Entities.ApiEndpointStep pStep, string pJsonRequest, DateTime inicio, DateTime fin)
        {
            string jsonResult = "";
            string statusCode = "";

            if (pStep.Response != null)
            {
                using (Stream responseStream = pStep.Response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    jsonResult = reader.ReadToEnd();
                    statusCode = string.IsNullOrEmpty(pStep.Response.StatusCode.ToString()) ? "TimeOut" : pStep.Response.StatusCode.ToString();
                }
            }
            else
            {
                statusCode = "TimeOut";
            }
            
            jsonResult = jsonResult == null ? "" : jsonResult;
            Entities.LogDBEntity logDBEntity = new Entities.LogDBEntity();

            logDBEntity.StepOrder = pStep.ExecutionOrder.ToString();
            logDBEntity.StepName = pStep.Name;
            logDBEntity.StepType = "Api Endpoint";
            logDBEntity.Endpoint = pStep.Url.ToString();
            logDBEntity.JsonRequestEndpoint = pJsonRequest;
            logDBEntity.Resultset = jsonResult.Replace("'","");
            logDBEntity.StatusCode = statusCode;
            logDBEntity.ExecutionDate = inicio;
            logDBEntity.ExecutionDateFinish = fin;

            TaskFileController.WriteLog(logDBEntity);
        }
    }
}
