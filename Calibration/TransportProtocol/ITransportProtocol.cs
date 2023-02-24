namespace CalProtocol;

public interface ITransportProtocol {
    void SendCommand();
    void Init(int timeout);
    TpStatus Query(ReadOnlySpan<byte> command, int responseLength, out byte[] response);
}

public enum TpStatus {
    Ok,
    Timeout
}