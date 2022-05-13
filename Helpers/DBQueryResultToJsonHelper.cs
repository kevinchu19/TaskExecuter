using Newtonsoft.Json;
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
                    var obj = JsonConvert.DeserializeObject<object>((string)reader[col]);
                    result.Add(col, obj);

                }
                catch
                {
                    result.Add(col, reader[col]);
                }
            return result;
        }
    }
}
