using System;
using System.Collections.Generic;
using System.Linq;

namespace LoteriaTwo.Models
{
    public class ElementoRepository
    {
        public static readonly ElementoRepository Instancia = new();

        private readonly List<Elemento> _elementos = new();

        private ElementoRepository() { }

        public void Add(Elemento elemento)      => _elementos.Add(elemento);
        public IReadOnlyList<Elemento> GetAll() => _elementos.AsReadOnly();
        public Elemento? Get(Guid id)           => _elementos.FirstOrDefault(e => e.Id == id);

        public bool Update(Elemento elemento)
        {
            int idx = _elementos.FindIndex(e => e.Id == elemento.Id);
            if (idx < 0) return false;
            _elementos[idx] = elemento;
            return true;
        }

        public bool Delete(Guid id)
        {
            int idx = _elementos.FindIndex(e => e.Id == id);
            if (idx < 0) return false;
            _elementos.RemoveAt(idx);
            return true;
        }

        public void Clear() => _elementos.Clear();
    }
}
