using Serilog;

namespace CalProtocol;

public enum CmdStatus
{
}

public class Calibration
{
    private readonly ITransportProtocol _tp;

    Calibration(ILogger logger, ITransportProtocol tp)
    {
        _tp = tp;
    }

    public CmdStatus ReadMemory(uint addr, uint size, out byte[] data)
    {
        data = Array.Empty<byte>();
        return (CmdStatus) 0;
    }

    public CmdStatus WriteMemory(uint addr, byte[] data)
    {
        return (CmdStatus) 0;
    }

    public CmdStatus ProgramDataBlock(int id)
    {
        return (CmdStatus) 0;
    }

    public CmdStatus EraseDataBlock(int id)
    {
        return (CmdStatus) 0;
    }

    public CmdStatus ConfigureReadBlock(int readFrequency, int size, Tuple<uint, uint>[] blockDesc)
    {
        return (CmdStatus) 0;
    }

    public CmdStatus StartCyclicRead()
    {
        return (CmdStatus) 0;
    }

    public CmdStatus StopCyclicRead()
    {
        return (CmdStatus) 0;
    }

    public CmdStatus ClearReadBlockConfig()
    {
        return (CmdStatus) 0;
    }

    private CmdStatus ProcessCommand()
    {
        return (CmdStatus) 0;
    }

    protected void OnCyclicDataRead()
    {
    }
}