namespace CanDriver;

public struct CanFrame {
    public uint Id { get; init; }
    public byte Dlc { get; set; }
    public byte[] Payload { get; set; }
    public ulong Timestamp { get; init; }
}

public enum Baudrate {
    Baud125K,
    Baud250K,
    Baud500K,
    Baud1000K
}