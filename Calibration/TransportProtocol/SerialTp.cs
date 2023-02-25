using System.IO.Ports;
using System.Text;
using Serilog;

namespace CalProtocol.TransportProtocol;

public class SerialTp : ITransportProtocol, IDisposable {
    private readonly ILogger _logger;
    private SerialPort? _serialPort;
    private byte _rxFrameCounter;
    private byte _txFrameCounter;
    private int _timeout;

    public SerialTp(ILogger logger) {
        _logger = logger;
    }

    public IEnumerable<int> GetAvailableChannels() {
        return SerialPort.GetPortNames().Select(portName => int.Parse(portName[3..]));
    }

    public void Connect(int channel, int timeout) {
        _timeout = timeout;

        if (_serialPort is not null && _serialPort.IsOpen) {
            return;
        }

        try {
            _serialPort = new SerialPort($"COM{channel}") {
                Encoding = Encoding.UTF8,
                BaudRate = 460800,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                RtsEnable = false,
                DtrEnable = false
            };
            _serialPort.Open();
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }
        catch (IOException) {
            _logger.Error("Invalid channel, {channel} doesn't exist or busy", channel);
        }
    }

    public void Disconnect() {
        _serialPort?.Close();
    }

    public async Task<(TpStatus Status, byte[] Data)> Query(byte[] command, int responseLength) {
        if (_serialPort is null || !_serialPort.IsOpen) {
            _logger.Error("Interface not connected.");
            return (TpStatus.NotConnected, Array.Empty<byte>());
        }

        var request = new byte[command.Length + 3];
        new[] {
            (byte) ((command.Length & 0xff00) >> 8), (byte) (command.Length & 0xff), _txFrameCounter
        }.CopyTo(request, 0);
        command.CopyTo(request, 3);
        var bytesToRead = responseLength + 3;

        _logger.Debug("Request: {data}", LogRaw(request));
        _serialPort.Write(request, 0, request.Length);
        _txFrameCounter++;

        var response = new byte[bytesToRead];
        var task = ReadBytesAsync(response, bytesToRead);
        var timeoutTask = Task.Delay(_timeout);
        if (await Task.WhenAny(task, timeoutTask) == timeoutTask) {
            _logger.Error("Communication timeout");
            return (TpStatus.Timeout, Array.Empty<byte>());
        }

        if (task.Result != bytesToRead) {
            _logger.Error("Rx Data to short, expected:{expected}, received:{bytes}", bytesToRead, task.Result);
        }


        _logger.Debug("Response: {data}", LogRaw(response));
        if (response[2] != _rxFrameCounter) {
            _rxFrameCounter++;
            _logger.Error("Communication error - missing rx frames.");
            return (TpStatus.InvalidMsgCounter, Array.Empty<byte>());
        }

        _rxFrameCounter++;
        return (TpStatus.Ok, response[3..]);
    }

    public event EventHandler? OnNewAsyncMessage;

    private async Task<int> ReadBytesAsync(byte[] buffer, int bytesToRead) {
        var bytesRead = 0;
        while (bytesRead < bytesToRead) {
            var read = await _serialPort!.BaseStream.ReadAsync(buffer.AsMemory(bytesRead, bytesToRead - bytesRead));
            bytesRead += read;
        }

        return bytesRead;
    }

    private static string LogRaw(IReadOnlyCollection<byte> data) {
        var str = new StringBuilder(data.Count * 4 + 1);
        foreach (var b in data) {
            str.Append($"0x{b:x2} ");
        }

        return str.ToString();
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }
}