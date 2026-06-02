using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LoteriaTwo.Config
{
    public class UnrealConfig
    {
        public SocketOptionsUnreal SocketOptions  { get; set; } = new();
        public UnrealSettings      UnrealSettings { get; set; } = new();

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static UnrealConfig Load(ModoEstudio modo)
        {
            var filename = modo == ModoEstudio.Prado ? "unreal_prado.json" : "unreal_torre.json";
            var path     = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            return Load(path);
        }

        public static UnrealConfig Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<UnrealConfig>(json, _opts) ?? new();
                }
            }
            catch { }
            return new();
        }
    }

    public class SocketOptionsUnreal
    {
        public List<string> IPs  { get; set; } = new();
        public string       Port { get; set; } = "30010";
    }

    public class UnrealSettings
    {
        public string            IDRemoteControl   { get; set; } = string.Empty;
        public List<NivelUnreal> ListaNiveles      { get; set; } = new();

        [JsonPropertyName("ListaClima")]
        public Dictionary<string, string> ListaClima        { get; set; } = new();

        [JsonPropertyName("HorasPredefinidas")]
        public Dictionary<string, string> HorasPredefinidas { get; set; } = new();

        [JsonPropertyName("FasesLunares")]
        public Dictionary<string, double> FasesLunares      { get; set; } = new();

        public IntensidadesUnreal Intensidades { get; set; } = new();
    }

    public class NivelUnreal
    {
        public string                     Nivel         { get; set; } = string.Empty;
        public string                     RemoteControl { get; set; } = string.Empty;
        public Dictionary<string, string> Funciones     { get; set; } = new();
        public Dictionary<string, string> Propiedades   { get; set; } = new();
    }

    public class IntensidadesUnreal
    {
        public double LunaDespejado { get; set; } = 0.15;
        public double LunaNublado   { get; set; } = 5.0;
        public double SolDespejado  { get; set; } = 5.0;
        public double SolNublado    { get; set; } = 15.0;
    }
}
