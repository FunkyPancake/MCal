using System.Text;
using Serilog;

namespace CalProtocol;

public enum CmdStatus {
    Ok,
}

public class CalibrationProtocol {
    private readonly ILogger _logger;
    private readonly ITransportProtocol _tp;

    public CalibrationProtocol(ILogger logger, ITransportProtocol tp) {
        _logger = logger;
        _tp = tp;
    }

    public bool ConnectionStatus { get; private set; }

    public CmdStatus Connect() {
        _tp.Init(5000);
        var status = _tp.Query(BuildCommand(Command.Connect, Array.Empty<byte>()), 10, out var response);
        if (status != TpStatus.Ok) {
            ConnectionStatus = false;
            _logger.Error("");
            return CmdStatus.Ok;
        }
        ConnectionStatus = true;
        return CmdStatus.Ok;
    }

    public CmdStatus Disconnect() {
        ConnectionStatus = false;
        return (CmdStatus) 0;
    }

    public CmdStatus Reset() {
        var status = _tp.Query(BuildCommand(Command.Connect, Array.Empty<byte>()), 10, out var response);
        return (CmdStatus) status;
    }

    public CmdStatus ReadMemory(uint addr, uint size, out byte[] data) {
        data = Array.Empty<byte>();
        _tp.SendCommand();
        return (CmdStatus) 0;
    }

    public CmdStatus WriteMemory(uint addr, byte[] data) {
        return (CmdStatus) 0;
    }

    public CmdStatus Program() {
        return (CmdStatus) 0;
    }

    public CmdStatus ConfigureCyclicReadBlock(int readFrequency, int size, Tuple<uint, uint>[] blockDesc) {
        return (CmdStatus) 0;
    }

    public CmdStatus StartCyclicRead() {
        return (CmdStatus) 0;
    }

    public CmdStatus StopCyclicRead() {
        return (CmdStatus) 0;
    }

    public CmdStatus ClearReadBlockConfig() {
        return (CmdStatus) 0;
    }

    public CmdStatus UpdateSoftware() {
        return (CmdStatus) 0;
    }

    public CmdStatus GetControlBlock() {
        return (CmdStatus) 0;
    }

    private CmdStatus ProcessCommand() {
        return (CmdStatus) 0;
    }

    protected void OnCyclicDataRead() {
    }

    private ReadOnlySpan<byte> BuildCommand(Command command, byte[] payload) {
        var data = new byte[payload.Length+1];
        data[0] = (byte) command;
        payload.CopyTo(data,1);
        return data;
    }

    private CmdStatus ProcessCommand(Command cmd) {
        return 0;
    }
}

internal enum Command {
    Connect,
    Disconnect,
    Reset,
    GetControlBlock,
    ReadMemory,
    WriteMemory,
    Program,
    ConfigureCyclicRead,
    StartCyclicRead,
    StopCyclicRead,
    ClearCyclicRead,
    JumpToFbl
}