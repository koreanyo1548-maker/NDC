using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ThirdParty
{
    public static class LocalSaveManager
    {
        private const string SaveFileName = "save.json";
        private const string NicknameKey = "LocalNickname";

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static void SaveAll(Dictionary<string, string> data)
        {
            string json = JsonConvert.SerializeObject(data);
            string tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, json);
            File.Copy(tmp, SavePath, overwrite: true);
            File.Delete(tmp);
        }

        public static void SaveKeys(Dictionary<string, string> updates)
        {
            var data = LoadAll() ?? new Dictionary<string, string>();
            foreach (var kv in updates)
                data[kv.Key] = kv.Value;
            SaveAll(data);
        }

        public static Dictionary<string, string> LoadAll()
        {
            if (!File.Exists(SavePath)) return null;
            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static bool HasSave() => File.Exists(SavePath);

        public static void RemoveKeys(System.Collections.Generic.IEnumerable<string> keys)
        {
            var data = LoadAll();
            if (data == null) return;
            foreach (var key in keys)
                data.Remove(key);
            SaveAll(data);
        }

        public static void DeleteAll()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        // KST 기준 — 기존 코드가 전부 AddHours(9) 가정이므로 동일 규칙 유지
        public static DateTime Now() => DateTime.UtcNow.AddHours(9);

        public static string GetOrCreateNickname()
        {
            string saved = PlayerPrefs.GetString(NicknameKey, string.Empty);
            if (!string.IsNullOrEmpty(saved)) return saved;
            string generated = "Player_" + UnityEngine.Random.Range(10000, 99999);
            PlayerPrefs.SetString(NicknameKey, generated);
            PlayerPrefs.Save();
            return generated;
        }

        public static void SetNickname(string name)
        {
            PlayerPrefs.SetString(NicknameKey, name);
            PlayerPrefs.Save();
        }
    }
}
