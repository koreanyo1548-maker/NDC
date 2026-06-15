using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Utils
{
    public class DbParser<T, U> where T: DbModel<T, U>, new()
    {
        /*
        public T GetPersistentData(string fileName)
        {
            try
            {
                if (!File.Exists(Application.persistentDataPath + fileName))
                    return default;
                var dataAsJson = SaveRepository.Decrypt(File.ReadAllText(Application.persistentDataPath + fileName));
                if (!string.IsNullOrEmpty(dataAsJson))
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new RecordConverter());

                    var loadedData = JsonConvert.DeserializeObject<T>(dataAsJson, settings);
                    return loadedData;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            return default;
        }
        */

        public Dictionary<U, T> GetDataAsDictionary(string fileName)
        {
            var fileContents = Resources.Load<TextAsset>("ExcelGenerated/" + fileName);
            var rs = new MemoryStream(fileContents.bytes);
            var deserializer = new BinaryFormatter();
            

            var obj = (DbMeta<T>) deserializer.Deserialize(rs);
            rs.Close();
            return obj.meta.ToDictionary(data => data.Id);
        }
    }

    [Serializable]
    public class DbMeta<T>
    {
        public T[] meta;
        
        public DbMeta (List<T> meta)
        {
            this.meta = meta.ToArray();
        }
    }
}