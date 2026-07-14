using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services
{
    public class BrainstormService
    {
        public static readonly BrainstormService Instancia = new();

        private BrainstormConnection[] _conns = [];
        private string _bd = "LoteriasTotal/LoteriaApuestas";

        private const double D = 0.2;

        private BrainstormService() { }

        public void Inicializar(BrainstormConnection[] conns, string bd)
        {
            _conns = conns;
            _bd = bd;
        }

        // ── Command helpers ───────────────────────────────────────────────────

        private string Set(string obj, string prop, string val, double delay = 0)
            => delay == 0
                ? $"itemset('<{_bd}>{obj}','{prop}','{val}');"
                : $"itemgo('<{_bd}>{obj}','{prop}','{val}',0,{delay.ToString("0.00", CultureInfo.InvariantCulture)});";

        private string Set(string obj, string prop, bool val, double delay = 0)
            => delay == 0
                ? $"itemset('<{_bd}>{obj}','{prop}',{val.ToString().ToLower()});"
                : $"itemgo('<{_bd}>{obj}','{prop}',{val.ToString().ToLower()},0,{delay.ToString("0.00", CultureInfo.InvariantCulture)});";

        private string Run(string obj, double delay = 0)
            => delay == 0
                ? $"itemset('<{_bd}>{obj}','EVENT_RUN');"
                : $"itemgo('<{_bd}>{obj}','EVENT_RUN',0,{delay.ToString("0.00", CultureInfo.InvariantCulture)});";

        private string Itemgo(string obj, string prop, string val, double duration, double delay)
            => $"itemgo('<{_bd}>{obj}','{prop}','{val}',{duration.ToString("0.00", CultureInfo.InvariantCulture)},{delay.ToString("0.00", CultureInfo.InvariantCulture)});";

        private string Itemgo(string obj, string prop, bool val, double duration, double delay)
            => $"itemgo('<{_bd}>{obj}','{prop}',{val.ToString().ToLower()},{duration.ToString("0.00", CultureInfo.InvariantCulture)},{delay.ToString("0.00", CultureInfo.InvariantCulture)});";

        // ── Send ──────────────────────────────────────────────────────────────

        public bool Enviar(string cmd)
        {
            if (_conns.Length == 0 || string.IsNullOrEmpty(cmd)) return false;
            Debug.WriteLine($"[IPF] >>> {cmd}");
            bool ok = true;
            foreach (var conn in _conns)
                ok &= conn.Send(cmd);
            var nivel = ok ? LogNivel.Accion : LogNivel.Error;
            LogService.Instancia.Registrar(nivel, "IPF", ok ? cmd : $"ERROR al enviar: {cmd}");
            return ok;
        }

        // ── PUBLIC API ────────────────────────────────────────────────────────

        public bool ModoQuiniela  { get; set; }
        public bool FaldonActivo  => _faldonActivo;

        private Elemento? _elementoActivo;
        private bool      _faldonActivo;
        private bool      _fondoActivo;

        public bool Entra(Elemento el)
        {
            bool esExento = el.Tipo is TipoElemento.Rotulo or TipoElemento.EuromillonesMosca;
            var cmd = new StringBuilder();
            if (_faldonActivo)
            {
                cmd.Append(Run("SaleFaldon"));
                if (!esExento)
                {
                    cmd.Append(Run($"Fondo/{FondoColor(el)}"));
                    cmd.Append(Run("Fondo/Entra"));
                }
                if (_elementoActivo is not null)
                    cmd.Append(BuildSale(_elementoActivo));
                cmd.Append(BuildContenido(el));
                _faldonActivo = false;
            }
            else
            {
                if (_elementoActivo is not null)
                    cmd.Append(BuildSale(_elementoActivo));
                cmd.Append(BuildEntra(el));
            }
            var ok = Enviar(cmd.ToString());
            if (ok)
            {
                _elementoActivo = el;
                if (!esExento) _fondoActivo = true;
            }
            return ok;
        }

        public bool Sale(Elemento el)
        {
            var ok = Enviar(BuildSale(el));
            if (ok) _elementoActivo = null;
            return ok;
        }

        public bool SaleActivo()
        {
            if (_elementoActivo is null) return false;
            return Sale(_elementoActivo);
        }
        public bool EntraFondo()
        {
            var ok = Enviar(Run("Fondo/Entra"));
            if (ok) _fondoActivo = true;
            return ok;
        }
        public bool SaleFondo()
        {
            var ok = Enviar(Run("Fondo/Sale"));
            if (ok) _fondoActivo = false;
            return ok;
        }
        public string CambiarFondo(string color) => Run($"Fondo/{color}");
        public bool EnviarEvento(string obj) => Enviar(Run(obj));
        public bool EnviarTexFile(string obj, string path)
            => Enviar($"itemset('<{_bd}>{obj}','TEX_FILE','{path}');");

        public bool SetBoteCantidad(string cantidad)
            => Enviar(Set("Premiados/BoteCantidad", "TEXT_STRING", $"{cantidad}€"));

        public bool EnviarCifrasFaldones(string[] numeros)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < numeros.Length; i++)
                sb.Append(Set($"cifra_Faldones_{(i + 1):D2}", "TEXT_STRING", numeros[i], D));
            return Enviar(sb.ToString());
        }

        public bool EnviarNumerosPremiado(string[] numeros)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < numeros.Length; i++)
                sb.Append(Set($"Premiados/NumerosPremiado/{(i + 1):D2}", "TEXT_STRING", numeros[i], D));
            return Enviar(sb.ToString());
        }

        public bool EntraFaldon(Elemento el, string? jokerNumero = null)
        {
            var ok = Enviar(BuildEntraFaldon(el, jokerNumero));
            if (ok) { _faldonActivo = true; _fondoActivo = false; }
            return ok;
        }

        public bool SaleFaldon()
        {
            var ok = Enviar(Run("SaleFaldon"));
            if (ok) _faldonActivo = false;
            return ok;
        }

        public bool EntraJoker()
        {
            var sb = new StringBuilder();
            sb.Append(Run("Faldon_Joker"));
            sb.Append(Run("Fondo/Sale"));
            sb.Append(Run("EntraFaldon", 0.1));
            var ok = Enviar(sb.ToString());
            if (ok) { _faldonActivo = true; _fondoActivo = false; }
            return ok;
        }
        public bool MostrarJoker(string numero)
        {
            var sb = new StringBuilder();
            sb.Append(Set("HD/Premiados/Joker",               "OBJ_CULL",    false));
            sb.Append(Set("HD_PantallaPlato/Premiados/Joker", "OBJ_CULL",    false));
            sb.Append(Set("Premiados/JokerNumero",            "TEXT_STRING", numero));
            sb.Append(Run("Premiados/Tipos/Primitiva"));
            return Enviar(sb.ToString());
        }

        public bool EntraFaldonCifra(int pos, string valor)
        {
            var sb = new StringBuilder();
            sb.Append(Set($"cifra_Faldones_{pos:D2}", "TEXT_STRING", valor));
            sb.Append(Run($"EntraFaldon/cifra{pos:D2}"));
            return Enviar(sb.ToString());
        }

        public bool EntraLogos(string logoNombre)
        {
            var sb = new StringBuilder();
            sb.Append(Set("HD/SorteosYBotes/LogoSorteo",               "OBJ_OVERMAT", $"Logo{logoNombre}"));
            sb.Append(Set("HD_PantallaPlato/SorteosYBotes/LogoSorteo", "OBJ_OVERMAT", $"Logo{logoNombre}"));
            sb.Append(Run("SorteosYBotes/LogoSorteo/Entra"));
            return Enviar(sb.ToString());
        }

        public bool SaleLogos() => Enviar(Run("SorteosYBotes/LogoSorteo/Sale"));

        public bool NextLogo(string logoNombre) => Enviar(CambiarLogo(logoNombre));
        public bool SyncLogo(string logoNombre) => Enviar(CambiarLogo(logoNombre));

        private string FondoColor(Elemento el)
        {
            if (ModoQuiniela) return "Rojo";
            if (el.Tipo == TipoElemento.Premiado)
            {
                return el["Juego"].ToUpperInvariant() switch
                {
                    "EL GORDO"                         => "Gordo",
                    "PRIMITIVA"                        => "Primitiva",
                    "BONOLOTO"                         => "Bonoloto",
                    "EUROMILLONES" or "EUROMILLONES M" => "Euromillones",
                    "LOTOTURF"                         => "Lototurf",
                    _                                  => "Azul"
                };
            }
            return "Azul";
        }

        // ── ENTRA builders ────────────────────────────────────────────────────

        private string BuildEntra(Elemento el)
        {
            if (el.Tipo is TipoElemento.Quiniela or TipoElemento.Pleno15)
                ModoQuiniela = true;
            if (el.Tipo is TipoElemento.Rotulo or TipoElemento.EuromillonesMosca)
                return BuildContenido(el);
            return Run($"Fondo/{FondoColor(el)}") + BuildContenido(el);
        }

        private string BuildContenido(Elemento el)
        {
            if (el.Tipo is TipoElemento.Quiniela or TipoElemento.Pleno15)
                ModoQuiniela = true;
            return el.Tipo switch
            {
                TipoElemento.Logo              => EntraLogo(el),
                TipoElemento.Bote              => EntraBote(el),
                TipoElemento.Premiado          => EntraPremiado(el),
                TipoElemento.ElMillon          => EntraMillon(el),
                TipoElemento.EuromillonesMosca => EntraEuromillonesMosca(el),
                TipoElemento.Eurodreams        => EntraEurodreams(el),
                TipoElemento.PrimerPremio      => EntraPremioLoteria(el),
                TipoElemento.PremioEspecial    => EntraPremioLoteria(el),
                TipoElemento.SegundoPremio     => EntraPremioLoteria(el),
                TipoElemento.TercerPremio      => EntraPremioLoteria(el),
                TipoElemento.Quiniela          => EntraQuiniela(el),
                TipoElemento.Pleno15           => EntraPleno15(el),
                TipoElemento.Rotulo            => EntraRotulo(el),
                TipoElemento.LogoCiudades      => EntraMapa(el),
                TipoElemento.Imagen            => EntraImagen(el),
                TipoElemento.Web               => Run("CartonWeb/Entra", 0.1),
                _                              => string.Empty,
            };
        }

        private string BuildSale(Elemento el) => el.Tipo switch
        {
            TipoElemento.Logo              => Run("SorteosYBotes/LogoSorteo/Sale"),
            TipoElemento.Bote              => Run("SorteosYBotes/Bote/Sale"),
            TipoElemento.Premiado          => Run("Premiados/Sale"),
            TipoElemento.ElMillon          => Run("ElMillon/Sale"),
            TipoElemento.EuromillonesMosca => Run("MoscaEuroMillones/Sale"),
            TipoElemento.Eurodreams        => Run("SaleEurodreams"),
            TipoElemento.PrimerPremio      => Run("LoteriaPremio/Sale"),
            TipoElemento.PremioEspecial    => Run("LoteriaPremio/Sale"),
            TipoElemento.SegundoPremio     => Run("LoteriaPremio/Sale"),
            TipoElemento.TercerPremio      => Run("LoteriaPremio/Sale"),
            TipoElemento.Quiniela          => Run("Quiniela/QuinielaResultados/Sale"),
            TipoElemento.Pleno15           => Run("Quiniela/QuinielaPleno/Sale"),
            TipoElemento.Rotulo            => Run("SaleRotulo"),
            TipoElemento.LogoCiudades      => Run("Mapa/Sale"),
            TipoElemento.Imagen            => Run("Imagen/Sale"),
            TipoElemento.Web               => Run("CartonWeb/Sale"),
            _ => string.Empty,
        };

        // ── FALDÓN ────────────────────────────────────────────────────────────

        private string BuildEntraFaldon(Elemento el, string? jokerNumero = null)
        {
            var juego  = el["Juego"].ToUpperInvariant();
            var nums   = el["Numeros"].Split(',');
            var extras = el["Extras"].Split(','); // sin RemoveEmptyEntries para preservar índices
            var sb = new StringBuilder();

            var tipoEvento = juego switch
            {
                "BONOLOTO"       => "Faldon_Bonoloto",
                "PRIMITIVA"      => "Faldon_Primitiva",
                "EL GORDO"       => "Faldon_Gordo",
                "EUROMILLONES M" => "Faldon_Euromillon",
                "LOTOTURF"       => "Faldon_Lototurf",
                _                => string.Empty
            };
            if (string.IsNullOrEmpty(tipoEvento)) return string.Empty;

            if (jokerNumero is not null)
            {
                sb.Append(Set("HD/Premiados/Joker",               "OBJ_CULL",    false));
                sb.Append(Set("HD_PantallaPlato/Premiados/Joker", "OBJ_CULL",    false));
                sb.Append(Set("Premiados/JokerNumero",            "TEXT_STRING", jokerNumero));
                sb.Append(Run("Premiados/Tipos/Primitiva"));
            }

            sb.Append(Run("Fondo/Sale"));
            sb.Append(Run(tipoEvento));

            // Números principales — después del evento para que no los sobreescriba
            for (int i = 0; i < nums.Length; i++)
                if (!string.IsNullOrWhiteSpace(nums[i]))
                    sb.Append(Set($"cifra_Faldones_0{i + 1}", "TEXT_STRING", nums[i].Trim()));

            // Extras con C/R
            switch (juego)
            {
                case "BONOLOTO":
                case "PRIMITIVA":
                case "LOTOTURF":
                    var e0 = extras.Length > 0 ? extras[0].Trim() : string.Empty;
                    var e1 = extras.Length > 1 ? extras[1].Trim() : string.Empty;
                    if (e0.Length > 0) sb.Append(Set("cifra_Faldones_07", "TEXT_STRING", $"C{e0}"));
                    if (e1.Length > 0) sb.Append(Set("cifra_Faldones_08", "TEXT_STRING", $"R{e1}"));
                    break;
                case "EL GORDO":
                    var eg0 = extras.Length > 0 ? extras[0].Trim() : string.Empty;
                    if (eg0.Length > 0) sb.Append(Set("cifra_Faldones_06", "TEXT_STRING", eg0));
                    break;
                case "EUROMILLONES M":
                    var em0 = extras.Length > 0 ? extras[0].Trim() : string.Empty;
                    var em1 = extras.Length > 1 ? extras[1].Trim() : string.Empty;
                    if (em0.Length > 0) sb.Append(Set("cifra_Faldones_06", "TEXT_STRING", em0));
                    if (em1.Length > 0) sb.Append(Set("cifra_Faldones_07", "TEXT_STRING", em1));
                    break;
            }

            sb.Append(Run("EntraFaldon", 0.1));
            return sb.ToString();
        }

        // ── LOGO ──────────────────────────────────────────────────────────────

        private static readonly string[] LogoPaths =
        {
            "HD/SorteosYBotes/LogoSorteo",
            "HD_PantallaPlato/SorteosYBotes/LogoSorteo",
            "9_16/SorteosYBotes/LogoSorteo",
        };

        private string CambiarLogo(string logo)
        {
            var sb = new StringBuilder();
            foreach (var path in LogoPaths)
            {
                sb.Append(Itemgo(path, "OBJ_CULL",    true,          0.1, 0.0));
                sb.Append(Itemgo(path, "OBJ_OVERMAT", $"Logo{logo}", 0.0, 0.5));
                sb.Append(Itemgo(path, "OBJ_CULL",    false,         0.1, 0.6));
            }
            return sb.ToString();
        }

        private string EntraLogo(Elemento el)
        {
            var logo = el["Logo"];
            var sb = new StringBuilder();
            sb.Append(Set("HD/SorteosYBotes/LogoSorteo",               "OBJ_OVERMAT", $"Logo{logo}"));
            sb.Append(Set("HD_PantallaPlato/SorteosYBotes/LogoSorteo", "OBJ_OVERMAT", $"Logo{logo}"));
            sb.Append(Run("SorteosYBotes/LogoSorteo/Entra"));
            return sb.ToString();
        }

        // ── BOTE ──────────────────────────────────────────────────────────────

        private string EntraBote(Elemento el)
        {
            var juego    = el["Juego"];
            var cantidad = el["Cantidad"];
            var fecha    = el["Fecha"].Replace('/', '-');
            var logo     = JuegoALogo(juego);

            var sb = new StringBuilder();
            sb.Append(Set("SorteosYBotes/BoteCantidad", "TEXT_STRING", $"{cantidad}€", D));
            sb.Append(Set("SorteosYBotes/Fecha", "TEXT_STRING", fecha, D));
            sb.Append(Set($"HD/SorteosYBotes/Bote/Logo",               "OBJ_OVERMAT", $"Logo{logo}", D));
            sb.Append(Set($"HD_PantallaPlato/SorteosYBotes/Bote/Logo", "OBJ_OVERMAT", $"Logo{logo}", D));
            sb.Append(Set($"9_16/SorteosYBotes/LogoSorteo",             "OBJ_OVERMAT", $"Logo{logo}", D));
            var tipoBote = juego.Equals("LOTERIA", StringComparison.OrdinalIgnoreCase)
                ? "SorteosYBotes/Bote/PremioLoteriaNacional"
                : "SorteosYBotes/Bote/Botes";
            sb.Append(Run(tipoBote, D));
            sb.Append(Run("SorteosYBotes/Bote/Entra", 0.1));
            return sb.ToString();
        }

        // ── PREMIADO ──────────────────────────────────────────────────────────

        private string EntraPremiado(Elemento el)
        {
            var juego   = el["Juego"];
            var numeros = el["Numeros"].Split(',');
            var extras  = el["Extras"].Split(',');
            var fecha   = el["Fecha"].Replace('/', '-');
            var bote    = el["Bote"] == "True";
            var logo    = PremiadoALogo(juego);

            var sb = new StringBuilder();

            bool tieneSeisNumeros = numeros.Length > 5;
            sb.Append(Set($"HD/Premiados/Nums/06",                 "OBJ_CULL", !tieneSeisNumeros, tieneSeisNumeros ? D : 0));
            sb.Append(Set($"HD_PantallaPlato/Premiados/Nums/06",   "OBJ_CULL", !tieneSeisNumeros, tieneSeisNumeros ? D : 0));
            sb.Append(Set($"HD/Premiados/Nums/Numeros/06",         "OBJ_CULL", !tieneSeisNumeros, tieneSeisNumeros ? D : 0));
            sb.Append(Set($"HD_PantallaPlato/Premiados/Nums/Numeros/06", "OBJ_CULL", !tieneSeisNumeros, tieneSeisNumeros ? D : 0));

            sb.Append(Set("HD/Premiados/LogoYFecha/Logo",             "OBJ_OVERMAT", $"Logo{logo}", D));
            sb.Append(Set("HD_PantallaPlato/Premiados/LogoYFecha/Logo", "OBJ_OVERMAT", $"Logo{logo}", D));

            for (int i = 0; i < numeros.Length; i++)
                sb.Append(Set($"Premiados/NumerosPremiado/{(i + 1):D2}", "TEXT_STRING", numeros[i].Trim(), D));

            string juegoBajo = juego.ToLower();
            if (juegoBajo == "eurodreams")
            {
                sb.Append(Set("Premiados/FechaPremiado", "TEXT_STRING", fecha, D));
            }
            else
            {
                string c = extras.Length > 0 ? extras[0].Trim() : "";
                string r = extras.Length > 1 ? extras[1].Trim() : "";

                sb.Append(Set("Premiados/Txt/Txt01", "TEXT_STRING", juegoBajo == "el gordo" ? "Nº Clave " : "C", D));
                sb.Append(Set("Premiados/Txt/Txt01Num1", "TEXT_STRING", c, D));
                if (juegoBajo != "el gordo")
                {
                    sb.Append(Set("Premiados/Txt/Txt02", "TEXT_STRING", "R", D));
                    sb.Append(Set("Premiados/Txt/Txt02Num1", "TEXT_STRING", r, D));
                }
                else
                {
                    sb.Append(Set("Premiados/Txt/Txt02", "TEXT_STRING", "", D));
                    sb.Append(Set("Premiados/Txt/Txt02Num1", "TEXT_STRING", $" {c}", D));
                }
                sb.Append(Set("Premiados/FechaPremiado", "TEXT_STRING", fecha, D));
            }

            sb.Append(Set("Premiados/BoteCantidad", "TEXT_STRING", bote ? $"{el["BoteCantidad"]}€" : "", D));

            var tipoPremiados = juegoBajo switch
            {
                "euromillones m" => "Premiados/Tipos/Euromillones",
                "el gordo"       => "Premiados/Tipos/ElGordo",
                "primitiva"      => "Premiados/Tipos/Primitiva",
                "eurodreams"     => "Premiados/Tipos/Eurodreams",
                _                => "Premiados/Tipos/Premiados",
            };
            sb.Append(Run(tipoPremiados, D));

            sb.Append(Set("HD/Premiados/Bote",              "OBJ_CULL", !bote, D));
            sb.Append(Set("HD_PantallaPlato/Premiados/Bote","OBJ_CULL", !bote, D));
            sb.Append(Set("9_16/Premiados/Bote",            "OBJ_CULL", !bote, D));

            sb.Append(Run("Premiados/Entra", 0.3));

            sb.Append(Set("HD/Premiados/Bote",              "OBJ_CULL", !bote, D));
            sb.Append(Set("HD_PantallaPlato/Premiados/Bote","OBJ_CULL", !bote, D));
            sb.Append(Set("9_16/Premiados/Bote",            "OBJ_CULL", !bote, D));
            return sb.ToString();
        }

        // ── EL MILLÓN ─────────────────────────────────────────────────────────

        private string EntraMillon(Elemento el)
        {
            var fecha = el["Fecha"];
            var fechaConDia = fecha;
            if (DateTime.TryParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            {
                var dia = dt.ToString("dddd", new CultureInfo("es-ES"));
                fechaConDia = $"{fecha}\\n{char.ToUpper(dia[0])}{dia[1..]}";
            }

            var sb = new StringBuilder();
            sb.Append(Set("ElMillon/TxtElMillon",   "TEXT_STRING", el["Numero"].ToUpper(), D));
            sb.Append(Set("ElMillon/FechaElMillon", "TEXT_STRING", fechaConDia, D));
            sb.Append(Run("Fondo/Entra", D));
            sb.Append(Run("ElMillon/Entra", 0.3));
            return sb.ToString();
        }

        // ── EUROMILLONES MOSCA ────────────────────────────────────────────────

        private string EntraEuromillonesMosca(Elemento el)
        {
            var sb = new StringBuilder();
            sb.Append(Set("FechaMoscaEuromillones", "TEXT_STRING", el["Numero"], 0.3));
            sb.Append(Run("Fondo/EntraPantalla", 0.3));
            sb.Append(Run("MoscaEuroMillones/Entra", 0.4));
            return sb.ToString();
        }

        // ── EURODREAMS ────────────────────────────────────────────────────────

        private string EntraEurodreams(Elemento el)
        {
            var sb = new StringBuilder();
            sb.Append(Set("Eurodreams/Dia", "TEXT_STRING", el["DiaSemana"], D));
            sb.Append(Set("Eurodreams/Fecha", "TEXT_STRING", $"{el["Dia"]}-{el["Mes"]}", D));
            sb.Append(Run("EntraEurodreams"));
            return sb.ToString();
        }

        public bool ProximoSorteoEurodreams(string dia, string mes)
        {
            var sb = new StringBuilder();
            sb.Append(Set("Eurodreams/Dia", "TEXT_STRING", dia, D));
            sb.Append(Set("Eurodreams/Mes", "TEXT_STRING", mes, D));
            sb.Append(Run("Eurodreams/ProximoSorteo"));
            return Enviar(sb.ToString());
        }

        public bool EnviarPremiadosEurodreams(string[] numeros, string otro)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < numeros.Length; i++)
                sb.Append(Set($"Premiados/NumerosPremiado/{(i + 1):D2}", "TEXT_STRING", numeros[i], D));
            sb.Append(Set("Premiados/Txt/Txt01Num1", "TEXT_STRING", otro, D));
            sb.Append(Run("Eurodreams/EntraNumeros"));
            return Enviar(sb.ToString());
        }

        // ── DÉCIMOS / PREMIOS LOTERÍA ─────────────────────────────────────────

        private string EntraPremioLoteria(Elemento el)
        {
            var sb = new StringBuilder();

            string tipoPremio = el.Tipo switch
            {
                TipoElemento.PrimerPremio   => "PRIMER PREMIO",
                TipoElemento.SegundoPremio  => "SEGUNDO PREMIO",
                TipoElemento.TercerPremio   => "TERCER PREMIO",
                _                           => "PREMIO ESPECIAL",
            };

            sb.Append(Set("LoteriaPremio/TipoPremio",   "TEXT_STRING", tipoPremio, D));
            sb.Append(Set("LoteriaPremio/NumeroPremio", "TEXT_STRING", el["Numero"], D));
            sb.Append(Set("LoteriaPremio/CantidadPremio", "TEXT_STRING", $"{el["Cantidad"]}€", D));
            sb.Append(Set("LoteriaPremio/FechaPremio",  "TEXT_STRING", el["Fecha"].Replace('/', '-'), D));

            if (el.Tipo == TipoElemento.PremioEspecial)
            {
                sb.Append(Set("LoteriaPremio/SeriePremio",    "TEXT_STRING", el["Serie"], D));
                sb.Append(Set("LoteriaPremio/FraccionPremio", "TEXT_STRING", el["Fraccion"], D));
                sb.Append(Set("LoteriaPremio/ReintegroPremio", "TEXT_STRING", "", D));
                sb.Append(Set("LoteriaPremio/R", "TEXT_STRING", "", D));
            }
            else if (el.Tipo == TipoElemento.PrimerPremio)
            {
                sb.Append(Set("LoteriaPremio/SeriePremio",    "TEXT_STRING", "", D));
                sb.Append(Set("LoteriaPremio/FraccionPremio", "TEXT_STRING", "", D));

                bool reintegro = el["ReintegroPremio"] == "True";
                if (reintegro)
                {
                    string rein = $"{el["Reintegro1"]} - {el["Reintegro2"]} - {el["Reintegro3"]}";
                    sb.Append(Set("LoteriaPremio/ReintegroPremio", "TEXT_STRING", rein, D));
                    sb.Append(Set("LoteriaPremio/R", "TEXT_STRING", "R", D));
                }
                else
                {
                    sb.Append(Set("LoteriaPremio/ReintegroPremio", "TEXT_STRING", "", D));
                    sb.Append(Set("LoteriaPremio/R", "TEXT_STRING", "", D));
                }
            }
            else
            {
                sb.Append(Set("LoteriaPremio/SeriePremio",    "TEXT_STRING", "", D));
                sb.Append(Set("LoteriaPremio/FraccionPremio", "TEXT_STRING", "", D));
                sb.Append(Set("LoteriaPremio/ReintegroPremio", "TEXT_STRING", "", D));
                sb.Append(Set("LoteriaPremio/R", "TEXT_STRING", "", D));
            }

            string layoutId = el.Tipo switch
            {
                TipoElemento.PrimerPremio   => "Loteria1",
                TipoElemento.SegundoPremio  => "Loteria2",
                TipoElemento.TercerPremio   => "Loteria3",
                _                           => "LoteriaPremioEspecial",
            };
            sb.Append(Run($"ColocacionesObjetosTotal/{layoutId}"));
            sb.Append(Run("LoteriaPremio/Entra", 0.1));
            return sb.ToString();
        }

        // ── QUINIELA ──────────────────────────────────────────────────────────

        private string EntraQuiniela(Elemento el)
        {
            var sb = new StringBuilder();
            var q = el.DatosQuiniela;
            if (q is null) return Run("Quiniela/QuinielaResultados/Entra", 0.1);

            sb.Append(Set("Quiniela/JornadaQuiniela", "TEXT_STRING", $"{q.Jornada}ª", 1));
            sb.Append(Set("Quiniela/FechaQuiniela",   "TEXT_STRING", q.Fecha, 1));

            for (int i = 0; i < q.Partidos.Length; i++)
            {
                var p   = q.Partidos[i];
                var sfx = i < 9 ? $"0{i + 1}" : $"{i + 1}";
                sb.Append(Set($"Quiniela/EquipoLocal/{sfx}",          "TEXT_STRING", p.EquipoLocal, 1));
                sb.Append(Set($"Quiniela/EquipoVisitante/{sfx}",      "TEXT_STRING", p.EquipoVisitante, 1));
                var signo = Partido.Signo(p.Resultado, i == 14);
                sb.Append(Set($"Quiniela/ResultadoQuiniela/{sfx}",    "TEXT_STRING", signo, 1));
                sb.Append(Set($"Quiniela/Resultado/{sfx}",            "TEXT_STRING", p.Resultado, 1));
            }

            sb.Append(Run("Quiniela/QuinielaResultados/Entra", 0.1));
            return sb.ToString();
        }

        private string EntraPleno15(Elemento el)
        {
            var sb = new StringBuilder();
            var q = el.DatosQuiniela;
            string acertantes = q?.AcertantesPleno ?? el["AcertantesPleno"];
            string bote       = q?.BotePleno       ?? el["BotePleno"];
            string jornada    = q?.Jornada         ?? el["Jornada"];
            string fecha      = q?.Fecha           ?? el["Fecha"];

            string textoAcertantes = acertantes == "1" ? "1 Acertante" : $"{acertantes} Acertantes";
            sb.Append(Set("Quiniela/PlenoQuiniela/Acertantes",   "TEXT_STRING", textoAcertantes, 1));
            sb.Append(Set("Quiniela/PlenoQuiniela/PremioCantidad","TEXT_STRING", $"{bote}€", 1));
            sb.Append(Set("Quiniela/JornadaQuiniela",            "TEXT_STRING", $"{jornada}ª", 1));
            sb.Append(Set("Quiniela/FechaQuiniela",              "TEXT_STRING", fecha, 1));
            sb.Append(Run("Quiniela/QuinielaPleno/Entra"));
            return sb.ToString();
        }

        // ── RÓTULO ────────────────────────────────────────────────────────────

        private string EntraRotulo(Elemento el)
        {
            var sb  = new StringBuilder();
            var tipo = el["Tipo"];
            var l1a  = el["Linea1Primera"];
            var l1b  = el["Linea1Segunda"];
            var l2   = el["Linea2"];

            bool esLaSuerte = tipo.Contains("Suerte", StringComparison.OrdinalIgnoreCase);
            bool doble = !string.IsNullOrEmpty(l2);

            if (!doble)
            {
                sb.Append(Set("Rotulos_Txt_01", "TEXT_STRING", l1a, D));
                sb.Append(Run(esLaSuerte ? "Rotulos_LaSuerte" : "Rotulos_Loterias", D));
                sb.Append(Run("Rotulos_1L", D));
            }
            else
            {
                sb.Append(Set("Rotulos_Txt_01", "TEXT_STRING", l1a, D));
                sb.Append(Set("Rotulos_Txt_02", "TEXT_STRING", l1b.Length > 0 ? l1b : l2, D));
                sb.Append(Run(esLaSuerte ? "Rotulos_LaSuerte" : "Rotulos_Loterias", D));
                sb.Append(Run("Rotulos_2L_1caja", D));
            }

            sb.Append(Run("EntraRotulo", D));
            return sb.ToString();
        }

        // ── MAPA CIUDADES ─────────────────────────────────────────────────────

        private string EntraMapa(Elemento el)
        {
            var sb = new StringBuilder();

            string[] comunidades = { el["Comunidad1"], el["Comunidad2"], el["Comunidad3"], el["Comunidad4"], el["Comunidad5"], el["Comunidad6"] };
            foreach (var c in comunidades)
            {
                if (string.IsNullOrEmpty(c)) continue;
                var key = ComunidadAKey(c);
                if (string.IsNullOrEmpty(key)) continue;
                sb.Append(Set($"HD/Mapa/Mapas_ComunidadesAutonomas/Mapa_{key}", "OBJ_CULL", false, 1));
                sb.Append(Set($"HD_PantallaPlato/Mapa/Mapas_ComunidadesAutonomas/Mapa_{key}", "OBJ_CULL", false, 1));
            }

            sb.Append(Set("Mapa/FechaMapa", "TEXT_STRING", el["Fecha"].Replace('/', '-'), 1));
            sb.Append(Set("Mapa/Txt01Mapa", "TEXT_STRING", el["Texto1"], 1));
            sb.Append(Set("Mapa/Txt02Mapa", "TEXT_STRING", el["Texto2"], 1));

            string[] ciudades = { el["Ciudad1"], el["Ciudad2"], el["Ciudad3"], el["Ciudad4"], el["Ciudad5"], el["Ciudad6"] };
            for (int i = 0; i < ciudades.Length; i++)
                sb.Append(Set($"Mapa/Ciudades/Ciudades0{i + 1}", "TEXT_STRING", ciudades[i], 1));

            string logo = el["Logo"];
            sb.Append(Set("HD/Mapa/LogoYFecha/Logo", "OBJ_OVERMAT",
                string.IsNullOrEmpty(logo) ? "LogoGenericoLAE" : $"Logo{logo}", 1));
            sb.Append(Set("HD_PantallaPlato/Mapa/LogoYFecha/Logo", "OBJ_OVERMAT",
                string.IsNullOrEmpty(logo) ? "LogoGenericoLAE" : $"Logo{logo}", 1));

            sb.Append(Run("Mapa/Ocultar"));
            sb.Append(Run("Mapa/Entra", 0.3));
            return sb.ToString();
        }

        // ── IMAGEN ────────────────────────────────────────────────────────────

        private string EntraImagen(Elemento el)
        {
            var sb = new StringBuilder();
            var foto = el["Foto"];
            var ruta = foto switch
            {
                "Foto 1"     => el["RutaFoto1"],
                "Foto 2"     => el["RutaFoto2"],
                "Foto 3"     => el["RutaFoto3"],
                "Foto 4"     => el["RutaFoto4"],
                "Foto 5"     => el["RutaFoto5"],
                "Video vivo" => string.Empty,
                _            => string.Empty,
            };

            if (foto == "Video vivo")
            {
                sb.Append(Run("Imagen/TipoMedia"));
            }
            else if (!string.IsNullOrEmpty(ruta))
            {
                sb.Append(Run("Imagen/TipoImagen"));
                sb.Append(Set("Imagen", "TEX_FILE", ruta, 0.4));
            }
            sb.Append(Run("Imagen/Entra", 0.1));
            return sb.ToString();
        }

        // ── Lookup helpers ────────────────────────────────────────────────────

        private static string JuegoALogo(string juego) => juego switch
        {
            "BONOLOTO"     => "Bonoloto",
            "QUINIGOL"     => "Quinigol",
            "EUROMILLONES" => "Euromillones",
            "LOTERIA"      => "LoteriaNacional",
            "PRIMITIVA"    => "Primitiva",
            "QUINIELA"     => "Quiniela",
            "EL GORDO"     => "El Gordo",
            "LOTOTURF"     => "Lototurf",
            "JOKER"        => "Joker",
            "Eurodreams"   => "Eurodreams",
            _              => juego,
        };

        private static string PremiadoALogo(string juego) => juego switch
        {
            "BONOLOTO"       => "Bonoloto",
            "EUROMILLONES M" => "EuromillonesMillon",
            "PRIMITIVA"      => "Primitiva",
            "EL GORDO"       => "El Gordo",
            "LOTOTURF"       => "Lototurf",
            "EURODREAMS"     => "Eurodreams",
            _                => juego,
        };

        private static string ComunidadAKey(string c) => c switch
        {
            "Andalucia"        => "Andalucia",
            "Aragon"           => "Aragon",
            "Asturias"         => "Asturias",
            "Baleares"         => "Baleares",
            "Canarias"         => "Canarias",
            "Cantabria"        => "Cantabria",
            "Castilla Y Leon"  => "CastillaYLeon",
            "Castilla La Mancha" => "CastillaLaMancha",
            "Catalunia"        => "Catalunia",
            "Extremadura"      => "Extremadura",
            "Galicia"          => "Galicia",
            "Madrid"           => "Madrid",
            "Murcia"           => "Murcia",
            "Navarra"          => "Navarra",
            "Pais Vasco"       => "PaisVasco",
            "Rioja"            => "Rioja",
            "Valencia"         => "Valencia",
            "Ceuta"            => "Ceuta",
            "Melilla"          => "Melilla",
            _                  => string.Empty,
        };
    }
}
