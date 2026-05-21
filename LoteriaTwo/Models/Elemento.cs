using System;
using System.Collections.Generic;
using System.Linq;

namespace LoteriaTwo.Models
{
    public enum TipoElemento
    {
        // Lotería y Rótulos
        Web,
        Imagen,
        Rotulo,
        LogoCiudades,
        // Décimos
        PrimerPremio,
        PremioEspecial,
        SegundoPremio,
        TercerPremio,
        // Quiniela
        Quiniela,
        Pleno15,
        // Sorteos y Botes
        Logo,
        Bote,
        Premiado,
        ElMillon,
        EuromillonesMosca,
        Eurodreams,
    }

    public class Elemento
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public TipoElemento Tipo { get; set; }
        public Guid? LogoId { get; set; }
        public Dictionary<string, string> Datos { get; set; } = new();
        public Quiniela? DatosQuiniela { get; set; }
        public DateTime CreadoEn { get; init; } = DateTime.Now;

        public string this[string key]
        {
            get => Datos.TryGetValue(key, out var v) ? v : string.Empty;
            set => Datos[key] = value;
        }

        public string ToLogString()
        {
            if (DatosQuiniela is { } q)
                return $"Jornada={q.Jornada} Fecha={q.Fecha}";

            var partes = Datos
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Take(4)
                .Select(kv => $"{kv.Key}={kv.Value}");
            return string.Join("  ", partes);
        }
    }
}
