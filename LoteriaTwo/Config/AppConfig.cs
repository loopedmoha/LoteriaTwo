using System;
using System.IO;
using System.Text.Json;

namespace LoteriaTwo.Config
{
    public class AppConfig
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public string BrainstormIP { get; set; } = "127.0.0.1";

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? CreateDefault();
                }
            }
            catch { }

            return CreateDefault();
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        private static AppConfig CreateDefault()
        {
            var cfg = new AppConfig();
            cfg.Save();
            return cfg;
        }
    }
}
