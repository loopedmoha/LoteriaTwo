using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services
{
    public class LogService
    {
        public static readonly LogService Instancia = new();

        private readonly List<LogEntry> _entradas = new();
        private readonly object _lock = new();
        private readonly string _rutaArchivo;

        public event Action? Updated;

        private LogService()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LoteriaTwo", "Logs");
            Directory.CreateDirectory(dir);
            _rutaArchivo = Path.Combine(dir, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        }

        public void Registrar(LogNivel nivel, string fuente, string mensaje)
        {
            var entry = new LogEntry { Nivel = nivel, Fuente = fuente, Mensaje = mensaje };
            lock (_lock)
            {
                _entradas.Add(entry);
                EscribirLinea(entry);
            }
            Updated?.Invoke();
        }

        public IReadOnlyList<LogEntry> GetAll()
        {
            lock (_lock)
                return _entradas.ToList().AsReadOnly();
        }

        public void Clear()
        {
            lock (_lock)
                _entradas.Clear();
            Updated?.Invoke();
        }

        public string RutaArchivo => _rutaArchivo;

        private void EscribirLinea(LogEntry e)
        {
            try
            {
                File.AppendAllText(_rutaArchivo,
                    $"{e.Timestamp:HH:mm:ss.fff}  {e.Nivel,-9}  {e.Fuente,-20}  {e.Mensaje}{Environment.NewLine}");
            }
            catch { /* no bloquear la app si el disco falla */ }
        }
    }
}
