using Newtonsoft.Json;

namespace TC_WinForms.DataProcessing
{
    internal static class DataJsonSerializer
    {
        public static void Serialize<T>(T obj, string path)
        {
            string json = JsonConvert.SerializeObject(obj);
            File.WriteAllText(path, json);
        }
        public static T Deserialize<T>(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
