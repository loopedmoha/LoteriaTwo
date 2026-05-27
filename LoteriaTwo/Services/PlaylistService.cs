using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services
{
    public sealed class PlaylistData
    {
        public string                              Nombre    { get; }
        public ObservableCollection<PlaylistItem> Elementos { get; } = new();
        public ObservableCollection<PlaylistItem> Logos     { get; } = new();

        public PlaylistData(string nombre) => Nombre = nombre;
    }

    public class PlaylistService
    {
        public static readonly PlaylistService Instancia = new();

        public PlaylistData[] Playlists { get; } =
        {
            new("Playlist 1"),
            new("Playlist 2"),
            new("Playlist 3"),
            new("Playlist 4"),
        };

        public int          IndicePlaylistActiva { get; set; } = 0;
        public PlaylistData Activa               => Playlists[IndicePlaylistActiva];

        private PlaylistService() { }

        public void AgregarElemento(Elemento el)
            => Activa.Elementos.Add(new PlaylistItem { ElementoId = el.Id, Tipo = el.Tipo, Nombre = el.ToPlaylistNombre() });

        public void AgregarLogo(string nombre, TipoElemento tipo)
            => Activa.Logos.Add(new PlaylistItem { Tipo = tipo, Nombre = nombre });

        // ── Save / Load ───────────────────────────────────────────────────────

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        public void Guardar(string ruta)
        {
            var dto = Playlists.Select(p => new PlaylistFileDto
            {
                Nombre    = p.Nombre,
                Elementos = p.Elementos.Select(i =>
                {
                    var el = ElementoRepository.Instancia.Get(i.ElementoId);
                    return new PlaylistItemDto
                    {
                        ElementoId    = i.ElementoId.ToString(),
                        Tipo          = i.Tipo.ToString(),
                        Nombre        = i.Nombre,
                        Datos         = el?.Datos         ?? new(),
                        DatosQuiniela = el?.DatosQuiniela,
                    };
                }).ToList(),
                Logos = p.Logos.Select(i => new PlaylistItemDto { Tipo = i.Tipo.ToString(), Nombre = i.Nombre }).ToList(),
            }).ToList();

            File.WriteAllText(ruta, JsonSerializer.Serialize(dto, _jsonOpts));
        }

        public bool Cargar(string ruta)
        {
            var json = File.ReadAllText(ruta);
            var dtos = JsonSerializer.Deserialize<List<PlaylistFileDto>>(json, _jsonOpts);
            if (dtos is null) return false;

            for (int i = 0; i < Math.Min(dtos.Count, Playlists.Length); i++)
            {
                Playlists[i].Elementos.Clear();
                Playlists[i].Logos.Clear();
                foreach (var it in dtos[i].Elementos)
                {
                    if (!Enum.TryParse<TipoElemento>(it.Tipo, out var tipo)) continue;
                    var elId = Guid.TryParse(it.ElementoId, out var g) ? g : Guid.NewGuid();
                    var el = new Elemento { Id = elId, Tipo = tipo, Datos = it.Datos ?? new(), DatosQuiniela = it.DatosQuiniela };
                    ElementoRepository.Instancia.Add(el);
                    Playlists[i].Elementos.Add(new PlaylistItem { ElementoId = elId, Tipo = tipo, Nombre = it.Nombre });
                }
                foreach (var it in dtos[i].Logos)
                    if (Enum.TryParse<TipoElemento>(it.Tipo, out var tipo))
                        Playlists[i].Logos.Add(new PlaylistItem { Tipo = tipo, Nombre = it.Nombre });
            }
            return true;
        }
    }

    // ── DTOs de serialización (solo usados internamente) ──────────────────────

    internal class PlaylistItemDto
    {
        public string                         ElementoId    { get; set; } = string.Empty;
        public string                         Tipo          { get; set; } = string.Empty;
        public string                         Nombre        { get; set; } = string.Empty;
        public Dictionary<string, string>?    Datos         { get; set; }
        public Quiniela?                      DatosQuiniela { get; set; }
    }

    internal class PlaylistFileDto
    {
        public string                 Nombre    { get; set; } = string.Empty;
        public List<PlaylistItemDto>  Elementos { get; set; } = new();
        public List<PlaylistItemDto>  Logos     { get; set; } = new();
    }
}
