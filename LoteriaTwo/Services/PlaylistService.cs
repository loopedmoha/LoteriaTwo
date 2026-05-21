using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        public void AgregarElemento(TipoElemento tipo, string nombre)
            => Activa.Elementos.Add(new PlaylistItem { Tipo = tipo, Nombre = nombre });

        public void AgregarLogo(string nombre, TipoElemento tipo)
            => Activa.Logos.Add(new PlaylistItem { Tipo = tipo, Nombre = nombre });

        // ── Save / Load ───────────────────────────────────────────────────────

        public void Guardar(string ruta)
        {
            var dto = Playlists.Select(p => new PlaylistFileDto
            {
                Nombre    = p.Nombre,
                Elementos = p.Elementos.Select(i => new PlaylistItemDto { Tipo = i.Tipo.ToString(), Nombre = i.Nombre }).ToList(),
                Logos     = p.Logos    .Select(i => new PlaylistItemDto { Tipo = i.Tipo.ToString(), Nombre = i.Nombre }).ToList(),
            }).ToList();

            File.WriteAllText(ruta, JsonSerializer.Serialize(dto,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        public bool Cargar(string ruta)
        {
            var json = File.ReadAllText(ruta);
            var dtos = JsonSerializer.Deserialize<List<PlaylistFileDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dtos is null) return false;

            for (int i = 0; i < Math.Min(dtos.Count, Playlists.Length); i++)
            {
                Playlists[i].Elementos.Clear();
                Playlists[i].Logos.Clear();
                foreach (var it in dtos[i].Elementos)
                    if (Enum.TryParse<TipoElemento>(it.Tipo, out var tipo))
                        Playlists[i].Elementos.Add(new PlaylistItem { Tipo = tipo, Nombre = it.Nombre });
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
        public string Tipo   { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    internal class PlaylistFileDto
    {
        public string                 Nombre    { get; set; } = string.Empty;
        public List<PlaylistItemDto>  Elementos { get; set; } = new();
        public List<PlaylistItemDto>  Logos     { get; set; } = new();
    }
}
