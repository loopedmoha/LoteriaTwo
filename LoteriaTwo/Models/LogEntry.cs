using System;

namespace LoteriaTwo.Models
{
    public enum LogNivel { Info, Accion, Cambio, Conexion, Error }

    public class LogEntry
    {
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public LogNivel Nivel     { get; init; }
        public string   Fuente    { get; init; } = string.Empty;
        public string   Mensaje   { get; init; } = string.Empty;
    }
}
