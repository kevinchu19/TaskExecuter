// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using TaskExecuter.Controllers;
using TaskExecuter.Entities;


//Leemos parametros
var parameters = Environment.GetCommandLineArgs();

//Obtenemos ruta del archivo
string Filepath = Array.Find(parameters, s => s.StartsWith("Filepath"));

try
{
    Filepath = Filepath.Replace("Filepath=", "");

}
catch(Exception ex)
{
    Console.WriteLine("Debe indicar el parametro Filepath con la ruta del archivo .json");
    return -1;
}

if (Filepath.Length > 0)
{
    //Logueamos el inicio de trabajo en un archivo de log.
    string firstLog = "\n";
    firstLog += "Execution time: " + DateTime.Now.TimeOfDay.ToString();
    firstLog += "\n";
    firstLog += "Task JSON path: " + Filepath;
    firstLog += "\n";
    TaskFileController.WriteLogFile(firstLog);


    if (TaskFileController.Loadfile(Filepath))
    {
        //Tratamos de crear la tabla de logueo en la base de datos indicada
        if(TaskFileController.CreateOrAccessLogTable())
        {
            //Si todo sale OK, ejecutamos los pasos.
            TaskFileController.Executesteps();
        }
        else
        {
            TaskFileController.WriteLogFile("No se pudo crear/acceder la tabla de logueo, se detiene la ejecucion.");
            Console.WriteLine("No se pudo crear/acceder la tabla de logueo, se detiene la ejecucion.");
        }
    }
    else
    {
        //Si no se encontro el archivo .json se loguea el error en el archivo. 
        string secondLog = "NO SE ENCONTRO EL ARCHIVO: " + Filepath;
        secondLog+="\n";
        TaskFileController.WriteLogFile(secondLog);
    }
}

return 0;
