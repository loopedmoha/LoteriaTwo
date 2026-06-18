using System;
using System.Text.Json.Serialization;

namespace LoteriaTwo.Models
{
    public class PlaylistItem
    {
        public Guid         Id         { get; init; } = Guid.NewGuid();
        public Guid         ElementoId { get; set; }
        public TipoElemento Tipo       { get; set; }
        public string       Nombre     { get; set; } = string.Empty;

        [JsonIgnore]
        public Func<Elemento?>? BuildActual { get; set; }
    }
}
