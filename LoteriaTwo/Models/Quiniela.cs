namespace LoteriaTwo.Models
{
    public class Partido
    {
        public string EquipoLocal { get; set; } = string.Empty;
        public string EquipoVisitante { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public bool Jugado { get; set; } = true;

        // Calcula el signo a partir del resultado. esPartido15 activa las reglas del Pleno al 15.
        public static string Signo(string resultado, bool esPartido15)
        {
            if (string.IsNullOrWhiteSpace(resultado)) return string.Empty;
            var p = resultado.Split('-');
            if (p.Length != 2 || !int.TryParse(p[0].Trim(), out int l) || !int.TryParse(p[1].Trim(), out int v))
                return string.Empty;

            if (!esPartido15)
                return l > v ? "1" : l == v ? "X" : "2";

            // Pleno al 15: 1 / 2 / 0 (empate 0-0) / M (resto de empates)
            if (l > v) return "1";
            if (l < v) return "2";
            return l == 0 ? "0" : "M";
        }
    }

    public class Quiniela
    {
        public string Fecha { get; set; } = string.Empty;
        public string Jornada { get; set; } = string.Empty;
        public Partido[] Partidos { get; set; } = new Partido[15];
        public string AcertantesPleno { get; set; } = string.Empty;
        public string BotePleno { get; set; } = string.Empty;

        public Quiniela()
        {
            for (int i = 0; i < 15; i++)
                Partidos[i] = new Partido();
        }
    }
}
