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

        public string ToPlaylistNombre()
        {
            switch (Tipo)
            {
                case TipoElemento.Web:            return "Web";
                case TipoElemento.Rotulo:         { var v = this["Tipo"];     return v.Length > 0 ? $"Rótulo – {v}"     : "Rótulo"; }
                case TipoElemento.LogoCiudades:   { var v = this["Logo"];     return v.Length > 0 ? $"Ciudades – {v}"   : "Ciudades"; }
                case TipoElemento.PrimerPremio:   return "1er Premio";
                case TipoElemento.SegundoPremio:  return "2º Premio";
                case TipoElemento.TercerPremio:   return "3er Premio";
                case TipoElemento.PremioEspecial: return "Premio Especial";
                case TipoElemento.Quiniela:       { var v = DatosQuiniela?.Jornada ?? string.Empty; return v.Length > 0 ? $"Quiniela J.{v}" : "Quiniela"; }
                case TipoElemento.Pleno15:        return "Pleno 15";
                case TipoElemento.Logo:           { var v = this["Logo"];     return v.Length > 0 ? $"Logo – {v}"       : "Logo"; }
                case TipoElemento.Bote:           { var v = this["Juego"];    return v.Length > 0 ? $"Bote – {v}"       : "Bote"; }
                case TipoElemento.Premiado:       { var v = this["Juego"];    return v.Length > 0 ? $"Premiado – {v}"   : "Premiado"; }
                case TipoElemento.ElMillon:       return "El Millón";
                case TipoElemento.EuromillonesMosca: return "Euromillones M.";
                case TipoElemento.Eurodreams:     { var v = this["DiaSemana"]; return v.Length > 0 ? $"Eurodreams – {v}" : "Eurodreams"; }

                case TipoElemento.Imagen:
                    var fotoVal = this["Foto"];
                    if (fotoVal == "Colas") return "Cola Video";
                    var imgNum = fotoVal switch
                    {
                        "Foto 1" => "1", "Foto 2" => "2", "Foto 3" => "3",
                        "Foto 4" => "4", "Foto 5" => "5",
                        _        => string.Empty,
                    };
                    var logoKey = fotoVal switch
                    {
                        "Foto 1"     => "LogoFoto1",
                        "Foto 2"     => "LogoFoto2",
                        "Foto 3"     => "LogoFoto3",
                        "Foto 4"     => "LogoFoto4",
                        "Foto 5"     => "LogoFoto5",
                        "Video vivo" => "LogoFotoVideo",
                        _            => string.Empty,
                    };
                    var logoNombre = logoKey.Length > 0 ? this[logoKey] : string.Empty;
                    if (imgNum.Length > 0)
                        return logoNombre.Length > 0 ? $"Imagen {imgNum} – {logoNombre}" : $"Imagen {imgNum}";
                    return "Imagen";

                default: return Tipo.ToString();
            }
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
