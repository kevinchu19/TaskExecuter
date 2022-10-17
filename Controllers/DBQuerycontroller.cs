using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Controllers
{
    public class DBQuerycontroller
    {
        private IDbConnection? _connection;
        private IDbCommand? _dbCommand;
        private String? _adoNetAssemblyName;
        private String? _adoNetConnectionTypeName;
        private int _commandTimeout = 0;
        private String? _parameterPrefix;
        private Entities.DBQueryStep? _step;
        private Entities.DbConnection _dbConnection = null;

        public Entities.DBQueryStep ExecuteStep(Entities.DBQueryStep step, Dictionary<string, object?>? result, Entities.DbConnection dbConnection)
        {
            _step = step;
            _dbConnection = dbConnection;
            GetConnection();
            GetCommand(step.Sentence, _connection, 0);
            if (step.ReturnResult == null)
                step.ReturnResult = new List<Dictionary<string, object?>>();
            Dictionary<string, object?> resultDictionary = new System.Collections.Generic.Dictionary<string, object?>();
            resultDictionary.Add("DataReader", ExecuteReader(step.Sentence, null, CommandType.StoredProcedure, result));
            step.ReturnResult.Add(resultDictionary);

            saveLog(step, result);

            return step;
        }
        protected void GetConnection()
        {
            if (_connection == null)
            {
                getAssembliesNames();
                // Create a  connection of the type
                _connection = (IDbConnection)Activator.CreateInstance(_adoNetAssemblyName, _adoNetConnectionTypeName).Unwrap();
                // Retrieve the Connection String    
                _connection.ConnectionString = _dbConnection.ConnectionString;
                _connection.Open();
            }

        }
        protected IDbCommand GetCommand(string query, IDbConnection connection, int commandTimeOut, CommandType commandType = CommandType.Text, Dictionary<string, object?>? pResultPair = null)
        {
            getAssembliesNames();
            IDbCommand newCommand = connection.CreateCommand();
            newCommand.CommandText = query;
            newCommand.Connection = connection;
            newCommand.CommandType = commandType;
            newCommand.CommandTimeout = commandTimeOut;
            if (pResultPair == null)
            {
                foreach (var parameterValue in _step.QueryParameters)
                {
                    IDbDataParameter parameter = newCommand.CreateParameter();
                    parameter.ParameterName = parameterValue.Key;
                    parameter.Value = parameterValue.Value == null ? DBNull.Value : parameterValue.Value;
                    newCommand.Parameters.Add(parameter);
                }
            }
            else
            {
                foreach (var parameterValue in _step.QueryParameters)
                {
                    //KT 17/8/2022: Machea por nombre el parametro del stored procedure con valores de campos del previous step
                    if (pResultPair.Where(x=>x.Key == parameterValue.Key) != null)
                    {
                        IDbDataParameter parameter = newCommand.CreateParameter();
                        parameter.ParameterName = parameterValue.Key;
                        parameter.Value = parameterValue.Value == null ? DBNull.Value : pResultPair[parameterValue.Key.Replace("@","")];
                        newCommand.Parameters.Add(parameter);
                    }
                    
                }
            }
            return newCommand;
        }
        protected void getAssembliesNames()
        {
            _adoNetAssemblyName = _dbConnection.AdoNetAssemblyName;
            _adoNetConnectionTypeName = _dbConnection.AdoNetConnectionTypeName;
            _commandTimeout = _dbConnection.AdoNetCommandTimeOut;
            _parameterPrefix = _dbConnection.ParameterPrefix;

        }
        protected IDataReader ExecuteReader(String query, IDbTransaction transaction, CommandType commandType = CommandType.Text, Dictionary<string, object?>? pResultPair = null)
        {
            try
            {
                GetConnection();
                _dbCommand = GetCommand(query, _connection, _commandTimeout, commandType, pResultPair);
                return _dbCommand.ExecuteReader();
            }
            catch (Exception sqlExeption)
            {
                throw sqlExeption;
            }
        }

        public void saveLog(Entities.DBQueryStep pStep, Dictionary<string, object?>? pResult)
        {
            Entities.LogDBEntity logDBEntity = new Entities.LogDBEntity();

            logDBEntity.StepOrder = _step.ExecutionOrder.ToString();
            logDBEntity.StepName = _step.Name;
            logDBEntity.StoredProcedure = _step.Sentence;
            logDBEntity.StepType = "Database Query";


            if (pResult == null)
            {
                foreach (var parameterValue in _step.QueryParameters)
                {
                    logDBEntity.Parameters += "Parameter name: " + parameterValue.Key + " value: " + parameterValue.Value + " ";
                }
            }
            else
            {
                foreach (var parameterValue in _step.QueryParameters)
                {
                    //KT 17/8/2022: Machea por nombre el parametro del stored procedure con valores de campos del previous step
                    if (pResult.Where(x => x.Key == parameterValue.Key) != null)
                        logDBEntity.Parameters += "Parameter name: " + parameterValue.Key + " value: " + pResult[parameterValue.Key.Replace("@", "")] + " ";
                }
            }
            foreach (var item in pStep.ReturnResult)
            {
                var dataReader = item.Where(x => x.Key == "DataReader").FirstOrDefault().Value;
                if (dataReader != null)
                {
                    var ResultSet = Helpers.DBQueryResultToJsonHelper.Serialize((System.Data.IDataReader)dataReader);
                    var jsonResultSet = JsonConvert.SerializeObject(ResultSet);
                    item.Remove("DataReader");
                    item.Add(pStep.Name, ResultSet);
                    logDBEntity.Resultset = jsonResultSet;
                }
            }

            TaskFileController.WriteLog(logDBEntity);
        }
    }
}
