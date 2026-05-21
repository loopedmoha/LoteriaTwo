using System;
using System.Collections.Generic;
using System.Linq;

namespace LoteriaTwo.Models
{
    public class LogoRepository
    {
        private static readonly string[] NombresConocidos =
        {
            "Bonoloto", "Primitiva", "El Gordo", "Euromillones", "EuromillonesMillon",
            "Eurodreams", "LotoTurf", "Quiniela", "Quinigol", "LoteriaNacional",
            "ApuestaHipica", "Joker", "ElMillon", "Deporte", "Cultura", "Sociedad",
            "GenericoLAE", "ProgLaSuerte", "Elige8"
        };

        public static readonly LogoRepository Instancia = new();

        private readonly List<Logo> _logos = new();

        private LogoRepository()
        {
            foreach (var nombre in NombresConocidos)
                _logos.Add(new Logo { Nombre = nombre });
        }

        // ── CRUD ─────────────────────────────────────────────────────────────

        public void Add(Logo logo)              => _logos.Add(logo);
        public IReadOnlyList<Logo> GetAll()     => _logos.AsReadOnly();
        public Logo? Get(Guid id)               => _logos.FirstOrDefault(l => l.Id == id);
        public Logo? GetByNombre(string nombre) => _logos.FirstOrDefault(l => l.Nombre == nombre);

        public Logo GetOrCreate(string nombre)
        {
            var logo = GetByNombre(nombre);
            if (logo is not null) return logo;
            logo = new Logo { Nombre = nombre };
            _logos.Add(logo);
            return logo;
        }

        public bool Update(Logo logo)
        {
            int idx = _logos.FindIndex(l => l.Id == logo.Id);
            if (idx < 0) return false;
            _logos[idx] = logo;
            return true;
        }

        public bool Delete(Guid id)
        {
            int idx = _logos.FindIndex(l => l.Id == id);
            if (idx < 0) return false;
            _logos.RemoveAt(idx);
            return true;
        }

        public void Clear() => _logos.Clear();
    }
}
