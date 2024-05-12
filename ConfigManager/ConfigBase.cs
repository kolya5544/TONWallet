using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TONWallet.ConfigManager
{
    public abstract class ConfigBase<T> where T : ConfigBase<T>, new()
    {
        private string filename;
        private static string FolderPath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TONWallet");
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".tonwallet");
            }
        }

        public static T Load(string filename)
        {
            var filePath = Path.Combine(FolderPath, filename);
            Directory.CreateDirectory(FolderPath);
            if (File.Exists(filePath))
            {
                var tmp = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                tmp.filename = filePath;
                return tmp;
            }
            else
            {
                T cfg = new T();
                cfg.filename = filePath;
                cfg.Save();
                return cfg;
            }
        }

        public void Save()
        {
            var filePath = Path.Combine(FolderPath, filename);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
