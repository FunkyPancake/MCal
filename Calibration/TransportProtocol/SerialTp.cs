using System.IO.Ports;
using System.Text;
using Serilog;

namespace CalProtocol.TransportProtocol;

public class SerialTp : ITransportProtocol {
    private readonly ILogger _logger;
    private SerialPort _serialPort;
    private byte _frameCounter = 0;

    public SerialTp(ILogger logger) {
        _logger = logger;
    }

    public void SendCommand() {
        throw new NotImplementedException();
    }

    public void Init(int timeout) {
        _serialPort = new SerialPort("COM7");
        _serialPort.Open();
    }

    public TpStatus Query(ReadOnlySpan<byte> command, int responseLength, out byte[] response) {
        var request = new[] {
            (byte) ((command.Length & 0xff00) >> 8), (byte) (command.Length & 0xff), _frameCounter
        };
        request = request.Concat(command.ToArray()).ToArray();
        _logger.Debug("Request: {data}",LogRaw(request));
        _serialPort.Write(Encoding.ASCII.GetString(request));

        var task = Task.Run(() => ReadBytes(responseLength));
        task.Wait();
        response = task.Result.ToArray();
        _logger.Debug("Response: {data}",LogRaw(request));

        return TpStatus.Ok;
    }

    private IReadOnlyList<byte> ReadBytes(int bytesToRead) {
        var buffer = new byte[bytesToRead+3];
        _serialPort.Read(buffer, 0, bytesToRead);
        return buffer;
    }
    
    private static string LogRaw(IReadOnlyCollection<byte> data) 
    {
        var str = new StringBuilder(data.Count * 4 + 1);
        foreach (var b in data) {
            str.Append($"0x{b:x2} ");
        }

        return str.ToString();
    }
}