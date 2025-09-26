using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskExecuter.Controllers
{
    public class LogDBController
    {
        private IDbConnection? _connection;
        private IDbCommand? _dbCommand;
        private String? _adoNetAssemblyName;
        private String? _adoNetConnectionTypeName;
        private int _commandTimeout = 0;
        private Entities.LogDbConnection _logDbConnection = null;

        public bool VerifyLogTable(Entities.LogDbConnection logDbConnection)
        {
            //Por defecto la tabla no existe.
            bool resultado = false;

            _logDbConnection = logDbConnection;
            GetConnection();

            if (_connection.State == ConnectionState.Open)
            {
                String comandoVerificacion = "SELECT " +
                                                "TABLE_NAME " +
                                             "FROM "+ _logDbConnection.DatabaseName + ".INFORMATION_SCHEMA.TABLES " +
                                             "WHERE TABLE_SCHEMA = '" + _logDbConnection.Schema + "'" +
                                             " AND TABLE_NAME = '" + _logDbConnection.TableName + "'";

                try
                {
                    object? registro;
                    IDbCommand newCommand = _connection.CreateCommand();
                    newCommand.CommandText = comandoVerificacion;
                    newCommand.Connection = _connection;
                    newCommand.CommandType = CommandType.Text;
                    registro = newCommand.ExecuteScalar();
                    if (registro != null)
                        resultado = true;
                }
                catch (Exception ex)
                {
                    TaskFileController.WriteLogFile(ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
            }
            return resultado;
        }

        public bool VerifyLogFields(Entities.LogDbConnection logDbConnection) // Verifica existencia del campo ExecutionDateTimeFinish en la tabla
        {
            //Por defecto el campo no existe.
            bool resultado = false;
            _logDbConnection = logDbConnection;
            GetConnection();
            if (_connection.State == ConnectionState.Open)
            {
                String comandoVerificacion = "SELECT " +
                                                "COLUMN_NAME " +
                                             "FROM " + _logDbConnection.DatabaseName + ".INFORMATION_SCHEMA.COLUMNS " +
                                             "WHERE TABLE_SCHEMA = '" + _logDbConnection.Schema + "'" +
                                             " AND TABLE_NAME = '" + _logDbConnection.TableName + "'" +
                                             " AND COLUMN_NAME = 'ExecutionDateTimeFinish'";
                try
                {
                    object? registro;
                    IDbCommand newCommand = _connection.CreateCommand();
                    newCommand.CommandText = comandoVerificacion;
                    newCommand.Connection = _connection;
                    newCommand.CommandType = CommandType.Text;
                    registro = newCommand.ExecuteScalar();
                    if (registro != null)
                        resultado = true;
                }
                catch (Exception ex)
                {
                    TaskFileController.WriteLogFile(ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
            }
            return resultado;
        }


        public bool CreateLogTable(Entities.LogDbConnection logDbConnection)
        {
            bool resultado = false;

            _logDbConnection = logDbConnection;
            GetConnection();

            if (_connection.State == ConnectionState.Open)
            {
                String comandoCreacion = "CREATE TABLE " + _logDbConnection.DatabaseName + "." + _logDbConnection.Schema + "." + _logDbConnection.TableName + "(" +
                                        "[LogID] [bigint] IDENTITY(1,1) NOT NULL," +
                                        "[ExecutionDateTime] [datetime] NOT NULL," +
                                        "[TaskName] [varchar](100) NOT NULL," +
                                        "[StepName] [varchar](100) NOT NULL," +
                                        "[StepType] [nchar](100) NOT NULL," +
                                        "[StepOrder] [varchar](10) NULL," +
                                        "[StoredProcedure] [varchar](500) NULL," +
                                        "[Parameters] [varchar](max) NULL," +
                                        "[Endpoint] [varchar](500) NULL," +
                                        "[JsonRequestEndpoint] [varchar](max) NULL," +
                                        "[Resultset] [varchar](max) NULL," +
                                        "[StatusCode] [varchar](max) NULL," +
                                        "[ExecutionDateTimeFinish] [datetime] NULL," +
                                        ")";
                try
                {
                    IDbCommand newCommand = _connection.CreateCommand();
                    newCommand.CommandText = comandoCreacion;
                    newCommand.Connection = _connection;
                    newCommand.CommandType = CommandType.Text;
                    newCommand.ExecuteNonQuery();
                    resultado = true;
                }
                catch (Exception ex)
                {
                    resultado = false;
                    TaskFileController.WriteLogFile(ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
            }
            return resultado;
        }

        public bool AddLogFields(Entities.LogDbConnection logDbConnection)
        {
            bool resultado = false;

         
         
            _logDbConnection = logDbConnection;
            GetConnection();
            if (_connection.State == ConnectionState.Open)
            {
                String comandoAlter = "ALTER TABLE " + _logDbConnection.DatabaseName + "." + _logDbConnection.Schema + "." + _logDbConnection.TableName +
                                        " ADD [ExecutionDateTimeFinish] [datetime] NULL";

                try
                {
                    IDbCommand newCommand = _connection.CreateCommand();
                    newCommand.CommandText = comandoAlter;
                    newCommand.Connection = _connection;
                    newCommand.CommandType = CommandType.Text;
                    newCommand.ExecuteNonQuery();
                    resultado = true;
                }
                catch (Exception ex)
                {
                    resultado = false;
                    TaskFileController.WriteLogFile(ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
    
        }

            return resultado;
        }

        public bool WriteLogTable (Entities.LogDbConnection logDbConnection, Entities.LogDBEntity logDBEntity, string taskName)
        {
            //Por defecto no se pudo escribir en la tabla.
            bool resultado = false;

            _logDbConnection = logDbConnection;
            GetConnection();

            logDBEntity.Resultset = logDBEntity.Resultset == null ? "" : logDBEntity.Resultset;


            if (_connection.State == ConnectionState.Open)
            {
                String comandoInsercion = "INSERT INTO " + _logDbConnection.DatabaseName + "." + _logDbConnection.Schema + "." + _logDbConnection.TableName + " (" +
                                        "[ExecutionDateTime]," +
                                        "[ExecutionDateTimeFinish]," +
                                        "[TaskName]," +
                                        "[StepName]," +
                                        "[StepType]," +
                                        "[StepOrder]," +
                                        "[StoredProcedure]," +
                                        "[Parameters]," +
                                        "[Endpoint]," +
                                        "[JsonRequestEndpoint]," +
                                        "[Resultset]," +
                                        "[StatusCode]" +
                                        ") " +
                                        "SELECT " +
                                        "CONVERT(DATETIME,'" + logDBEntity.ExecutionDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',120) AS ExecutionDateTime, " +
                                        "CONVERT(DATETIME,'" + logDBEntity.ExecutionDateFinish.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',120) AS ExecutionDateTimeFinish, " +
                                        "'" + taskName + "'" + " AS TaskName, " +
                                        "'" + logDBEntity.StepName.Replace("'","''") + "'" + " AS StepName, "+
                                        "'" + logDBEntity.StepType.Replace("'", "''") + "'" + " AS StepType, " +
                                        "'" + logDBEntity.StepOrder.Replace("'", "''") + "'" + "  AS StepOrder, " +
                                        "'" + logDBEntity.StoredProcedure.Replace("'", "''") + "'" + " AS StoredProcedure, " +
                                        "'" + logDBEntity.Parameters.Replace("'", "''") + "'" + " AS Parameters, " +
                                        "'" + logDBEntity.Endpoint.Replace("'", "''") + "'" + " AS Endpoint, " +
                                        "'" + logDBEntity.JsonRequestEndpoint.Replace("'", "''") + "'" + " AS JsonRequestEndpoint, " +
                                        "'" + logDBEntity.Resultset.Replace("'", "''") + "'" + " AS Resultset, " +
                                        "'" + logDBEntity.StatusCode.Replace("'", "''") + "'" + " AS StatusCode ";
                try
                {
                    IDbCommand newCommand = _connection.CreateCommand();
                    newCommand.CommandText = comandoInsercion;
                    newCommand.Connection = _connection;
                    newCommand.CommandType = CommandType.Text;
                    newCommand.ExecuteNonQuery();
                    resultado = true;
                }
                catch (Exception ex)
                {
                    resultado = false;
                    TaskFileController.WriteLogFile(ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
            }
            return resultado;
        }

        protected void GetConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                getAssembliesNames();
                // Create a  connection of the type
                _connection = (IDbConnection)Activator.CreateInstance(_adoNetAssemblyName, _adoNetConnectionTypeName).Unwrap();
                // Retrieve the Connection String    
                _connection.ConnectionString = _logDbConnection.ConnectionString;

                try
                {
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    TaskFileController.WriteLogFile(ex.Message);
                }
            }

        }
        protected void getAssembliesNames()
        {
            _adoNetAssemblyName = _logDbConnection.AdoNetAssemblyName;
            _adoNetConnectionTypeName = _logDbConnection.AdoNetConnectionTypeName;
            _commandTimeout = _logDbConnection.AdoNetCommandTimeOut;

        }
    }
}
