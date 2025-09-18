using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Controllers
{
    public static class TaskFileController
    {
        public const string DBQUERYSTEP = "DBQueryStep";
        public const string APIENDPOINTSTEP = "ApiEndpointStep";
        private static string fileContent = String.Empty;
        private static Entities.TaskExecution? taskExecution = null;

        public static void WriteLogFile(string textContent)
        {

            using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Logs\Log" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt", true))
            {
                writer.WriteLine(textContent);
                writer.Close();
            }
        }

        public static bool Loadfile(string path)
        {
            bool returnValue = false;
            try
            {
                fileContent = System.IO.File.ReadAllText(path);
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                taskExecution = JsonConvert.DeserializeObject<TaskExecuter.Entities.TaskExecution>(fileContent, settings);
                returnValue = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                returnValue = false;
            }
            finally
            {
            }
            return returnValue;

        }

        public static bool CreateOrAccessLogTable()
        {
            //Por defecto la tabla no se pudo crear ni acceder.
            bool returnValue = false;
            LogDBController logDBController = new LogDBController();
            var logDbConnection = taskExecution.LogDbConnection;

            //Verificamos si la tabla existe
            if (!logDBController.VerifyLogTable(logDbConnection))
            {
                if (logDBController.CreateLogTable(logDbConnection))
                    returnValue = true;
            }
            //Verificamos si tiene campos de log agregados en ultima version
            if (!logDBController.VerifyLogFields(logDbConnection))
            {
                if (logDBController.AddLogFields(logDbConnection))
                    returnValue = true;
            }
            else
                returnValue = true;

            return returnValue;
        }

        public static void WriteLog(Entities.LogDBEntity logDBEntity)
        {
            LogDBController logDBController = new LogDBController();
            var logDbConnection = taskExecution.LogDbConnection;

            logDBController.WriteLogTable(logDbConnection, logDBEntity, taskExecution.Name);

        }

        public static void Executesteps()
        {
            if (taskExecution != null)
            {
                foreach (Entities.Step item in taskExecution.Steps)
                {
                    switch (item.Type)
                    {
                        case DBQUERYSTEP:
                            var dbQuerycontroller = new DBQuerycontroller();
                            var dbQueryStep = (Entities.DBQueryStep)item;
                            var dbConnection = taskExecution.DbConnections.Find(x => x.Name.Contains(((Entities.DBQueryStep)item).DbConnectionName));
                            var previousDbQueryStepItem = taskExecution.Steps.Find(x => x.Name.Contains(dbQueryStep.PreviousStepReference));
                            if (previousDbQueryStepItem.ReturnResult == null)
                            {
                                previousDbQueryStepItem.ReturnResult = new List<Dictionary<string, object?>>();
                                dbQuerycontroller.ExecuteStep(dbQueryStep, previousDbQueryStepItem.ReturnResult.FirstOrDefault(), dbConnection);
                            }
                            else
                            {
                                //item.ReturnResult = new List<Dictionary<string, object?>>();
                                if (previousDbQueryStepItem.Type == DBQUERYSTEP)
                                {
                                    if (previousDbQueryStepItem.ReturnResult.Count > 0)
                                    {
                                        foreach (var resultPair in previousDbQueryStepItem.ReturnResult[0])
                                        {
                                            if (resultPair.Key == item.PreviousStepReference)
                                            {
                                                foreach (var result in (IEnumerable<Dictionary<string, object>>)resultPair.Value)
                                                {
                                                    Entities.DBQueryStep aux = new Entities.DBQueryStep();
                                                    aux = dbQuerycontroller.ExecuteStep(dbQueryStep, result, dbConnection);
                                                    //item.ReturnResult.Add(aux.ReturnResult.FirstOrDefault());
                                                    dbQuerycontroller = new DBQuerycontroller();
                                                }
                                            }
                                        }

                                    }

                                }
                                else
                                {
                                    foreach (var resultPair in previousDbQueryStepItem.ReturnResult)
                                    {
                                        Entities.DBQueryStep aux = new Entities.DBQueryStep();
                                        aux = dbQuerycontroller.ExecuteStep(dbQueryStep, resultPair, dbConnection);
                                        item.ReturnResult.Add(aux.ReturnResult.FirstOrDefault());
                                        dbQuerycontroller = new DBQuerycontroller();
                                    }
                                }

                            }


                            break;

                        case APIENDPOINTSTEP:

                            item.ReturnResult = new List<Dictionary<string, object?>>();
                            var apiEndpointcontroller = new ApiEndpointController();
                            var apiEndpointStep = (Entities.ApiEndpointStep)item;

                            var previousApiEndpointStepItem = taskExecution.Steps.Find(x => x.Name.Contains(apiEndpointStep.PreviousStepReference));


                            //Si existen resultados de algun paso previo los recorro, caso contrario uso los del corriente
                            if (previousApiEndpointStepItem.ReturnResult.Count > 0)
                            {
                                foreach (var resultPair in previousApiEndpointStepItem.ReturnResult)
                                {
                                    var _jsonData = (IEnumerable<Dictionary<string, object>>)resultPair.Where(x => x.Key == previousApiEndpointStepItem.Name).FirstOrDefault().Value;

                                    foreach (var row in _jsonData)
                                    {
                                        Entities.ApiEndpointStep stepApi = new Entities.ApiEndpointStep();

                                        //Ejecutamos la request al endpoint
                                        stepApi = apiEndpointcontroller.ExecuteApiEndpointStep(apiEndpointStep, row);


                                        var auxResultPair = new Dictionary<string, object?>();
                                        auxResultPair.Add("HttpStatusResponse", stepApi.Response?.StatusCode);

                                        foreach (var results in stepApi.RouteVariables)
                                        {
                                            auxResultPair.Add(results.Value, row.Where(x => x.Key == results.Value).FirstOrDefault().Value);
                                        }

                                        //KT 17/8/2022: Para que pase como parametro de dbquerystep posterior cualquier valor de campo del body

                                        foreach (var field in row)
                                        {
                                            if (!auxResultPair.ContainsKey(field.Key)) {
                                                auxResultPair.Add(field.Key, field.Value);
                                            }
                                            
                                        }

                                        item.ReturnResult.Add(auxResultPair);

                                    }
                                }
                            }
                            else
                            {
                                Entities.ApiEndpointStep stepApi = new Entities.ApiEndpointStep();

                                //Dictionary<string, object>? routeVariables = new Dictionary<string, object>();

                                //Ejecutamos la request al endpoint
                                stepApi = apiEndpointcontroller.ExecuteApiEndpointStep(apiEndpointStep, null);


                                var auxResultPair = new Dictionary<string, object?>();
                                auxResultPair.Add("HttpStatusResponse", stepApi.Response?.StatusCode ?? HttpStatusCode.RequestTimeout);

                                foreach (var results in stepApi.RouteVariables)
                                {
                                    auxResultPair.Add(results.Value, apiEndpointStep.RouteVariables.Where(x => x.Name == results.Value).FirstOrDefault().Value);
                                }

                                item.ReturnResult.Add(auxResultPair);

                            }





                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}
