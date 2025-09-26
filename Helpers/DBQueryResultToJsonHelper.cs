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
                try
                {
                    var jsonString = (string)reader[col];

                    // Corregir booleanos que están como strings en el JSON
                    jsonString = jsonString.Replace("\"true\"", "true")
                                           .Replace("\"false\"", "false");

                    // Limpiar strings que tienen comillas dobles escapadas innecesarias
                    // Patrón: "campo":"\"valor\"" -> "campo":"valor"
                    jsonString = System.Text.RegularExpressions.Regex.Replace(
                        jsonString,
                        @"""(\w+)"":""\\""([^""]*?)\\""""",
                        @"""$1"":""$2"""
                    );

                    // Limpiar arrays que están como strings escapados
                    jsonString = System.Text.RegularExpressions.Regex.Replace(
                        jsonString,
                        @"""(\w+)"":""\[([^\]]*)\]""",
                        match => {
                            var fieldName = match.Groups[1].Value;
                            var arrayContent = match.Groups[2].Value.Replace(@"\""", @"""");
                            return $@"""{fieldName}"":[{arrayContent}]";
                        }
                    );

                    var obj = JsonConvert.DeserializeObject<object>(jsonString);
                    result.Add(col, obj);
                }
                catch
                {
                    var value = reader[col];

                    // Manejar DBNull
                    if (value == DBNull.Value || value == null)
                    {
                        result.Add(col, null);
                        continue;
                    }

                    // Convertir a string y verificar si es booleano
                    string stringValue = value.ToString().Trim();

                    if (stringValue.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("1", StringComparison.Ordinal))
                    {
                        result.Add(col, true);
                    }
                    else if (stringValue.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                             stringValue.Equals("0", StringComparison.Ordinal))
                    {
                        result.Add(col, false);
                    }
                    else
                    {
                        result.Add(col, value);
                    }
                }
            return result;
        }
    }
}
