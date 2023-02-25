namespace CalProtocol.TransportProtocol;

public interface ITransportProtocol {
    IEnumerable<int> GetAvailableChannels();
    void Connect(int channel, int timeout);
    void Disconnect();
    Task<(TpStatus Status, byte[] Data)> Query(byte[] command, int responseLength) ;
}

public enum TpStatus {
    Ok,
    NotConnected,
    Timeout,
    DeviceError,
    InvalidLength,
    InvalidMsgCounter,
    
}