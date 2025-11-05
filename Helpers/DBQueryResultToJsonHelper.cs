using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TaskExecuter.Helpers
{
    public static  class DBQueryResultToJsonHelper
    {
        public static IEnumerable<Dictionary<string, object>> Serialize(System.Data.IDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));
            List<object> resultado = new List<object>();
            while (reader.Read())
            {
                results.Add(SerializeRow(cols, reader));
                //resultado.Add()
            }

            return results;
        }

        private static Dictionary<string, object> SerializeRow(IEnumerable<string> cols,
                                  System.Data.IDataReader reader)
        {
            var result = new Dictionary<string, object>();

            foreach (var col in cols)
            {
                var value = reader[col];

                // Manejar DBNull
                if (value == DBNull.Value || value == null)
                {
                    result.Add(col, null);
                    continue;
                }

                // Intentar deserializar como JSON
                if (TryDeserializeJson(value, out object jsonObject))
                {
                    result.Add(col, jsonObject);
                    continue;
                }

                // No es JSON, procesar según el tipo de dato del reader
                result.Add(col, ConvertToTypedValue(value, reader.GetFieldType(reader.GetOrdinal(col))));
            }

            return result;
        }

        private static bool TryDeserializeJson(object value, out object jsonObject)
        {
            jsonObject = null;

            if (!(value is string stringValue))
                return false;

            stringValue = stringValue.Trim();

            // Verificar si parece JSON (empieza con { o [)
            if (!stringValue.StartsWith("{") && !stringValue.StartsWith("["))
                return false;

            try
            {
                // Primero verificar si todo el JSON está envuelto en comillas (como string escapado)
                if (stringValue.StartsWith("\"") && stringValue.EndsWith("\"") && stringValue.Length > 2)
                {
                    stringValue = stringValue.Substring(1, stringValue.Length - 2);
                    stringValue = stringValue.Replace("\\\"", "\"");
                }

                // Corregir booleanos que están como strings en el JSON
                stringValue = stringValue.Replace("\"true\"", "true")
                                         .Replace("\"false\"", "false");

                // Limpiar strings que tienen comillas dobles escapadas innecesarias
                stringValue = System.Text.RegularExpressions.Regex.Replace(
                    stringValue,
                    @"""(\w+)"":""\\""([^""]*?)\\""""",
                    @"""$1"":""$2"""
                );

                // Limpiar arrays que están como strings escapados
                stringValue = System.Text.RegularExpressions.Regex.Replace(
                    stringValue,
                    @"""(\w+)"":""\[([^\]]*)\]""",
                    match => {
                        var fieldName = match.Groups[1].Value;
                        var arrayContent = match.Groups[2].Value.Replace(@"\""", @"""");
                        return $@"""{fieldName}"":[{arrayContent}]";
                    }
                );

                jsonObject = JsonConvert.DeserializeObject<object>(stringValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object ConvertToTypedValue(object value, Type fieldType)
        {
            string stringValue = value.ToString().Trim();

            // Limpiar comillas literales que vienen en el texto (ej: "10" -> 10)
            if (stringValue.StartsWith("\"") && stringValue.EndsWith("\"") && stringValue.Length > 2)
            {
                stringValue = stringValue.Substring(1, stringValue.Length - 2);
            }

            // Verificar booleanos en formato string
            if (stringValue.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (stringValue.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Si el tipo del campo es string, devolver como string sin conversión
            if (fieldType == typeof(string))
            {
                return stringValue;
            }

            // Si el tipo del campo ya es numérico, devolverlo tal cual
            if (fieldType == typeof(int) || fieldType == typeof(long) ||
                fieldType == typeof(short) || fieldType == typeof(byte))
            {
                return value;
            }

            if (fieldType == typeof(decimal) || fieldType == typeof(double) ||
                fieldType == typeof(float))
            {
                return value;
            }

            // Si el tipo del campo es booleano, devolverlo tal cual
            if (fieldType == typeof(bool))
            {
                return value;
            }

            // Para otros tipos, evaluar el contenido del string

            

            // Intentar parsear como número solo si el tipo NO es string
            if (decimal.TryParse(stringValue, out decimal decimalValue))
            {
                // Si es entero, devolverlo como int
                if (decimalValue == Math.Floor(decimalValue) &&
                    decimalValue >= int.MinValue &&
                    decimalValue <= int.MaxValue)
                {
                    return (int)decimalValue;
                }
                // Si no, como decimal
                return decimalValue;
            }

            // Si nada de lo anterior aplica, devolver como string
            return stringValue;
        }
    }
}
