using System;

namespace LoteriaTwo.Models
{
    public class Logo
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public DateTime CreadoEn { get; init; } = DateTime.Now;
    }
}
