using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoteriaTwo.Config;
using LoteriaTwo.Models;

namespace LoteriaTwo.Services;

public class Signals
{
    private static readonly Signals _instancia = new();
    public static Signals GetInstance() => _instancia;

    private UnrealConfig? _config;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };

    private Signals() { }

    public void Inicializar(UnrealConfig config) => _config = config;

    // ── API pública ────────────────────────────────────────────────────────────

    public async Task EnviarFuncionAsync(string nivel, string funcion,
        Dictionary<string, object>? parameters = null)
    {
        if (_config is null) return;
        var nivelObj = _config.UnrealSettings.ListaNiveles.FirstOrDefault(n => n.Nivel == nivel);
        if (nivelObj is null || !nivelObj.Funciones.TryGetValue(funcion, out var funcId)) return;

        var port  = _config.SocketOptions.Port;
        var tasks = _config.SocketOptions.IPs
            .Select(ip => CallPresetFunctionAsync(ip, port, nivelObj.RemoteControl, funcId, parameters));
        await Task.WhenAll(tasks);
    }

    public async Task EnviarPropiedadAsync(string nivel, string propiedad, object valor)
    {
        if (_config is null) return;
        var nivelObj = _config.UnrealSettings.ListaNiveles.FirstOrDefault(n => n.Nivel == nivel);
        if (nivelObj is null || !nivelObj.Propiedades.TryGetValue(propiedad, out var fieldId)) return;

        // Construir JSON con tipo correcto para evitar boxing/unboxing en la serialización
        string json = BuildPropertyJson(valor);

        var port = _config.SocketOptions.Port;
        var tasks = _config.SocketOptions.IPs
            .Select(ip => PutPropertyAsync(ip, port, nivelObj.RemoteControl, fieldId, json));
        await Task.WhenAll(tasks);
    }

    public async Task<bool> PingIpAsync(string ip, string port)
    {
        try
        {
            var response = await _http.GetAsync($"http://{ip}:{port}/remote/presets");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── HTTP ───────────────────────────────────────────────────────────────────

    // Construye el JSON de propiedad con el tipo adecuado, evitando boxing
    private static string BuildPropertyJson(object valor)
    {
        string valueJson = valor switch
        {
            bool   b => b ? "true" : "false",
            int    i => i.ToString(System.Globalization.CultureInfo.InvariantCulture),
            float  f => f.ToString("G9",  System.Globalization.CultureInfo.InvariantCulture),
            double d => d.ToString("G17", System.Globalization.CultureInfo.InvariantCulture),
            string s => JsonSerializer.Serialize(s),
            _        => JsonSerializer.Serialize(valor),
        };
        return $"{{\"PropertyValue\":{valueJson},\"GenerateTransaction\":true}}";
    }

    // PUT /remote/preset/{presetName}/property/{fieldId}
    private Task<bool> PutPropertyAsync(
        string ip, string port, string presetName, string fieldId, string json)
    {
        var url = $"http://{ip}:{port}/remote/preset/{Uri.EscapeDataString(presetName)}/property/{fieldId}";
        return PutAsync(url, json);
    }

    // PUT /remote/preset/{presetName}/function/{funcId}
    private Task<bool> CallPresetFunctionAsync(
        string ip, string port, string presetName, string funcId,
        Dictionary<string, object>? parameters = null)
    {
        var body = new { Parameters = parameters ?? new Dictionary<string, object>(), GenerateTransaction = true };
        var json = JsonSerializer.Serialize(body);
        var url  = $"http://{ip}:{port}/remote/preset/{Uri.EscapeDataString(presetName)}/function/{funcId}";
        return PutAsync(url, json);
    }

    private async Task<bool> PutAsync(string url, string json)
    {
        try
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            LogService.Instancia.Registrar(LogNivel.Info, "Unreal", $"PUT {url}  body={json}");

            var response = await _http.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                LogService.Instancia.Registrar(LogNivel.Conexion, "Unreal", $"OK {(int)response.StatusCode} ← {url}");
                return true;
            }

            var msg = await response.Content.ReadAsStringAsync();
            LogService.Instancia.Registrar(LogNivel.Error, "Unreal",
                $"PUT {url} [{(int)response.StatusCode}]: {msg}");
            return false;
        }
        catch (TaskCanceledException)
        {
            LogService.Instancia.Registrar(LogNivel.Error, "Unreal", $"Timeout: {url}");
            return false;
        }
        catch (Exception ex)
        {
            LogService.Instancia.Registrar(LogNivel.Error, "Unreal", $"Error: {ex.Message}");
            return false;
        }
    }
}
