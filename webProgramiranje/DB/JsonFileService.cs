using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace webProgramiranje.DB
{
    public class JsonFileService<T>
    {
        private readonly string _filePath;

        public JsonFileService(string filePath)
        {
            _filePath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/") + filePath;
        }

        public IEnumerable<T> ReadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return new List<T>();
            }

            var content = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<IEnumerable<T>>(content);
        }

        public void WriteToFile(IEnumerable<T> data)
        {
            var content = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(_filePath, content);
        }
    }
}