using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Utils
{
    public class DbUserParser<T, U> where T : DbUserModel<T, U>, new()
    {
        public Dictionary<U, T> GetDataAsDictionary(List<T> obj)
        {
            return obj.ToDictionary(data => data.Id);
        }
        
//         public Dictionary<U, T> GetDataAsDictionary(string fileName)
//         {
// #if UNITY_EDITOR
//             if (fileName == null || fileName.Equals(string.Empty))
//             {
//                 var initials = new T().GetInitials();
//                 return initials.ToDictionary(data => data.Id);
//             }
//
//             var fileContents = Resources.Load<TextAsset>("User/" + fileName);
//             if (fileContents == null)
//             {
//                 var initials = new T().GetInitials();
//                 return initials.ToDictionary(data => data.Id);
//             }
//             else
//             {
//                 // var rs = new MemoryStream(fileContents.bytes);
//                 // var deserializer = new BinaryFormatter();
//                 var rs = fileContents.text;
//                 var obj = JsonConvert.DeserializeObject<List<T>>(rs);
//                 return obj.ToDictionary(data => data.Id);
//             }
//
// #else
//
//             if (fileName == null || fileName.Equals(string.Empty))
//             {
//                 var initials = new T().GetInitials();
//                 return initials.ToDictionary(data => data.Id);
//             }
//             if (!File.Exists(Application.persistentDataPath + fileName))
//             {
//                     var initials = new T().GetInitials();
//                     return initials.ToDictionary(data => data.Id);
//             }
//             var fileContents = File.ReadAllText(Application.persistentDataPath + fileName);
//             var obj = JsonConvert.DeserializeObject<List<T>>(fileContents);
//             return obj.ToDictionary(data => data.Id);
// #endif
//         }
    }
}