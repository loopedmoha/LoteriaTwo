using System;
using System.IO;
using System.Text.Json;

namespace LoteriaTwo.Config
{
    public enum ModoEstudio { Prado, Torre }

    public class AppConfig
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public string BrainstormDB  { get; set; } = "LoteriasTotal/LoteriaApuestas";
        public string PradoIP       { get; set; } = "127.0.0.1";
        public string TorreIP1      { get; set; } = "127.0.0.1";
        public string TorreIP2      { get; set; } = "127.0.0.1";
        public string FotosShare    { get; set; } = @"\\172.28.51.61\FotosLoteria";
        public string FotosUser     { get; set; } = "Administrador";
        public string FotosPassword { get; set; } = "Auto1041";

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
