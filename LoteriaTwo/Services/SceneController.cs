using System.Linq;
using System.Threading.Tasks;
using LoteriaTwo.Config;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services;

public class SceneController
{
    public static readonly SceneController Instancia = new();

    private readonly Signals _signals = Signals.GetInstance();
    private UnrealConfig?    _config;
    private string?          _currentLevel   = "Loterias";
    private bool             _pantallaIn     = true;
    private bool             _lucesIn        = true;
    private bool             _edificiosDiaIn = true;
    private bool             _ndiIn          = true;

    private SceneController() { }

    public UnrealConfig? Config      => _config;
    public string?       NivelActivo { get => _currentLevel; set => _currentLevel = value; }

    public void Inicializar(UnrealConfig config)
    {
        _config       = config;
        _signals.Inicializar(config);
        _currentLevel = config.UnrealSettings.ListaNiveles.Count > 0
            ? config.UnrealSettings.ListaNiveles[0].Nivel
            : "Loterias";

        var ips  = string.Join(", ", config.SocketOptions.IPs);
        var port = config.SocketOptions.Port;
        LogService.Instancia.Registrar(LogNivel.Info, "Unreal",
            $"Remote Control → {config.SocketOptions.IPs.Count} IP(s) [{ips}] :{port}, nivel: {_currentLevel}");
    }

    public async Task<(int Ok, int Total)> PingAllAsync()
    {
        if (_config is null || _config.SocketOptions.IPs.Count == 0) return (0, 0);
        var port    = _config.SocketOptions.Port;
        var results = await Task.WhenAll(
            _config.SocketOptions.IPs.Select(ip => _signals.PingIpAsync(ip, port)));
        int ok = results.Count(r => r);
        LogService.Instancia.Registrar(
            ok == results.Length ? LogNivel.Conexion : LogNivel.Error,
            "Unreal", $"Ping: {ok}/{results.Length} IPs responden");
        return (ok, results.Length);
    }

    public void ChangeLevel(string level)
    {
        _currentLevel = level;
        _pantallaIn   = true;
    }

    // ── Toggle ─────────────────────────────────────────────────────────────────

    public async void ActivarPantalla()
    {
        string funcion = _pantallaIn ? "BajarPantalla" : "SubirPantalla";
        _pantallaIn    = !_pantallaIn;
        await _signals.EnviarFuncionAsync(_currentLevel!, funcion);
    }

    public async void CambiarLuces()
    {
        string funcion = _lucesIn ? "ApagarLuces" : "EncenderLuces";
        _lucesIn       = !_lucesIn;
        await _signals.EnviarFuncionAsync(_currentLevel!, funcion);
    }

    public async void CambiarNDI()
    {
        string funcion = _ndiIn ? "PantallaSDI" : "PantallaNDI";
        await _signals.EnviarFuncionAsync(_currentLevel!, funcion);
        if (_currentLevel == "LaSuerteEnTusManos")
        {
            funcion = _ndiIn ? "PantallaSDI16-9" : "PantallaNDI16-9";
            await _signals.EnviarFuncionAsync(_currentLevel!, funcion);
        }
        _ndiIn = !_ndiIn;
    }

    public async void CambiarEdificios()
    {
        string funcion  = _edificiosDiaIn ? "EdificiosDia" : "EdificiosNoche";
        _edificiosDiaIn = !_edificiosDiaIn;
        await _signals.EnviarFuncionAsync(_currentLevel!, funcion);
    }

    // ── Directos (para botones explícitos en UI) ────────────────────────────────

    public async void BajarPantalla() => await _signals.EnviarFuncionAsync(_currentLevel!, "BajarPantalla");
    public async void SubirPantalla() => await _signals.EnviarFuncionAsync(_currentLevel!, "SubirPantalla");
    public async void PantallaSDI()   => await _signals.EnviarFuncionAsync(_currentLevel!, "PantallaSDI");
    public async void PantallaNDI()   => await _signals.EnviarFuncionAsync(_currentLevel!, "PantallaNDI");

    // ── Propiedades ─────────────────────────────────────────────────────────────

    public async void CambiarHora(int hora)
        => await _signals.EnviarPropiedadAsync(_currentLevel!, "Hora", hora);

    public async void CambiarHoraAutomatica(bool activar)
        => await _signals.EnviarPropiedadAsync(_currentLevel!, "HoraAutom", activar);

    public async void CambiarDia(int dia, int mes, int anio)
    {
        await _signals.EnviarPropiedadAsync(_currentLevel!, "Dia", dia);
        await _signals.EnviarPropiedadAsync(_currentLevel!, "Mes", mes);
        await _signals.EnviarPropiedadAsync(_currentLevel!, "Año", anio);
    }

    public async void CambiarClima(string clima)
    {
        if (_config is null) return;
        var climaPath = _config.UnrealSettings.ListaClima[clima];
        await _signals.EnviarPropiedadAsync(_currentLevel!, "Clima", climaPath);
    }

    public async void CambiarFaseLunar(string fase)
    {
        if (_config is null) return;
        var faseInNum = _config.UnrealSettings.FasesLunares[fase];
        await _signals.EnviarPropiedadAsync(_currentLevel!, "FaseLunar", faseInNum);
    }

    public async void CambiarIntensidadLuna(float intensidad)
        => await _signals.EnviarPropiedadAsync(_currentLevel!, "IntensidadLuna", intensidad);

    public async void CambiarIntensidadSol(float intensidad)
        => await _signals.EnviarPropiedadAsync(_currentLevel!, "IntensidadSol", intensidad);
}
