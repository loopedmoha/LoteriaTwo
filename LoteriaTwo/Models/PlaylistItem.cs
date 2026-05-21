using System;

namespace LoteriaTwo.Models
{
    public class PlaylistItem
    {
        public Guid         Id     { get; init; } = Guid.NewGuid();
        public TipoElemento Tipo   { get; set; }
        public string       Nombre { get; set; } = string.Empty;
    }
}
