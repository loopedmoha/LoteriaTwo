using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services
{
    public class FormStateService
    {
        public static readonly FormStateService Instancia = new();

        public static readonly string RutaFormulario = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LoteriaTwo", "formulario.json");

        private readonly Dictionary<string,
            (Func<Dictionary<string, string>> Leer,
             Action<Dictionary<string, string>> Escribir)> _secciones = new();

        private FormStateService() { }

        public void RegistrarSeccion(
            string nombre,
            Func<Dictionary<string, string>> leer,
            Action<Dictionary<string, string>> escribir)
        {
            _secciones[nombre] = (leer, escribir);
        }

        public void Guardar()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RutaFormulario)!);
            var estado = _secciones.ToDictionary(kv => kv.Key, kv => kv.Value.Leer());
            File.WriteAllText(RutaFormulario, JsonSerializer.Serialize(estado,
                new JsonSerializerOptions { WriteIndented = true }));
            LogService.Instancia.Registrar(LogNivel.Accion, "Formulario", "Guardado → formulario.json");
        }

        public void Limpiar()
        {
            var empty = new Dictionary<string, string>();
            foreach (var kv in _secciones)
                kv.Value.Escribir(empty);
            LogService.Instancia.Registrar(LogNivel.Accion, "Formulario", "Formulario limpiado");
        }

        public bool Cargar()
        {
            if (!File.Exists(RutaFormulario)) return false;
            var json = File.ReadAllText(RutaFormulario);
            var estado = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (estado is null) return false;
            foreach (var kv in estado.Where(kv => _secciones.ContainsKey(kv.Key)))
                _secciones[kv.Key].Escribir(kv.Value);
            LogService.Instancia.Registrar(LogNivel.Accion, "Formulario", "Cargado ← formulario.json");
            return true;
        }
    }
}
