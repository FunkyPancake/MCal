using System.Text;
using System.Xml.Linq;
using CalProtocol.TransportProtocol;
using Serilog;

namespace CalProtocol;

public class Calibration {
    private readonly ILogger _logger;
    private readonly CalibrationConfig _calibrationConfig;
    private CalibrationProtocol _calibrationProtocol;
    private ITransportProtocol _transportProtocol;

    public Calibration(ILogger logger) {
        _logger = logger;
        _calibrationConfig = new CalibrationConfig(logger);
    }

    public bool IsConnected { get; }

    public void UpdateTpConfiguration(TpConfig config) {
        switch (config.ConnectionType) {
            case TpConfig.TpType.SerialPort:
                _transportProtocol = new SerialTp(_logger,config.SerialConfig);
                break;
            case TpConfig.TpType.Can:
                break;
            case TpConfig.TpType.Ethernet:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<bool> Connect() {
        _calibrationProtocol = new CalibrationProtocol(_logger, _transportProtocol);
        await _calibrationProtocol.Connect();
        return true;
    }

    public async void Disconnect() {
    }

    public async Task<XElement> Load(string filePath) {
        await _calibrationConfig.Load(filePath);
        return _calibrationConfig.GetCalTree();
    }

    public async void New() {
    }

    public async Task Program(uint value) {
        var response = await _calibrationProtocol.WriteMemory(0x20000094,BitConverter.GetBytes(value) );
    }

    public async Task ReadMemory(uint address, uint size) {
        var response = await _calibrationProtocol.ReadMemory(address, size);
        _logger.Debug("memory values",LogRaw(response.Item2));
    }
    private static string LogRaw(IReadOnlyCollection<byte> data) {
        var str = new StringBuilder(data.Count * 4 + 1);
        foreach (var b in data) {
            str.Append($"0x{b:x2} ");
        }

        return str.ToString();
    }
}

public class TpConfig {
    public enum TpType {
        SerialPort,
        Can,
        Ethernet
    }
    public TpType ConnectionType { get; init; }
    public SerialTpConfig SerialConfig;
    public Dictionary<string, string> CanConfig;
    public Dictionary<string, string> EthernetConfig;
}