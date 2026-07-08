using System;
using System.Diagnostics;
using System.IO;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services
{
    public class RemoteShareService
    {
        public static readonly RemoteShareService Instancia = new();

        private string _share = string.Empty;

        private RemoteShareService() { }

        public void Inicializar(string share, string user, string password)
        {
            _share = share;
        }

        public bool Configurado => !string.IsNullOrEmpty(_share);

        // Copia una imagen a la carpeta remota manteniendo el nombre original.
        public string CopiarImagen(string localPath)
            => Copiar(localPath, Path.GetFileName(localPath));

        public string RutaDecimoBrainstorm
            => Path.Combine(_share, "Decimo.jpg").Replace('\\', '/');

        public void CopiarDecimo(string localPath)
            => Copiar(localPath, "Decimo.jpg");

        private string Copiar(string localPath, string destFileName)
        {
            if (!Directory.Exists(_share))
                Directory.CreateDirectory(_share);

            string remotePath = Path.Combine(_share, destFileName);
            File.Copy(localPath, remotePath, overwrite: true);

            // Brainstorm requiere barras "/" en las rutas, no "\"
            string brainstormPath = remotePath.Replace('\\', '/');
            Debug.WriteLine($"[RemoteShare] Copiado → {brainstormPath}");
            return brainstormPath;
        }
    }
}
