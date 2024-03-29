namespace CalProtocol.TransportProtocol;

public interface ITransportProtocol {
    IEnumerable<int> GetAvailableChannels();
    bool Connect();
    void Disconnect();
    Task<(TpStatus Status, byte[] Data)> Query(byte[] command, int responseLength) ;
    
    event EventHandler? OnNewAsyncMessage;
}

public enum TpStatus {
    Ok,
    NotConnected,
    Timeout,
    DeviceError,
    InvalidLength,
    InvalidMsgCounter,
    
}