using System;
using System.Collections.Generic;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services
{
    public class LiveDataService
    {
        public static readonly LiveDataService Instancia = new();

        private readonly Dictionary<TipoElemento, Func<string>> _proveedores = new();

        private LiveDataService() { }

        public void Registrar(TipoElemento tipo, Func<string> proveedor)
            => _proveedores[tipo] = proveedor;

        public string GetSnapshot(TipoElemento tipo)
            => _proveedores.TryGetValue(tipo, out var fn) ? fn() : string.Empty;
    }
}
