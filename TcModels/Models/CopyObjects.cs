using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcModels.Models.Interfaces;
using TcModels.Models.IntermediateTables;
using TcModels.Models.IntermediateTables;

namespace TcModels.Models
{
    public static class CopyObjects
    {
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            var c = JsonConvert.SerializeObject(source, Formatting.Indented, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            return JsonConvert.DeserializeObject<T>(c, deserializeSettings);
        }


        public static List<Staff_TC> CloneJson(this List<Staff_TC> source) 
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            List<Staff_TC> o = new List<Staff_TC>();
            foreach (var item in source)
            {
                item.IdAuto = 0;
                item.ParentId = 0;

                var c = JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                o.Add(JsonConvert.DeserializeObject<Staff_TC>(c, deserializeSettings));
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result

            return o;
        }

        public static List<T> CloneJsonIIntermediateTable<T>(this List<T> source) where T : IIntermediateTableIds
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            List<T> o = new List<T>();
            foreach (var item in source)
            {
                item.ParentId = 0;
                var c = JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                o.Add(JsonConvert.DeserializeObject<T>(c, deserializeSettings));
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result

            return o;
        }
    }
}

