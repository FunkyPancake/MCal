﻿using System.Text;
using CalProtocol.TransportProtocol;
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

    public async Task<CmdStatus> Connect() {
        _tp.Connect();
        var status = await _tp.Query(BuildCommand(Command.Connect), 1);
        if (status.Status != TpStatus.Ok) {
            ConnectionStatus = false;
            _logger.Error("");
            return CmdStatus.Ok;
        }
        ConnectionStatus = true;
        return CmdStatus.Ok;
    }

    public async Task<CmdStatus> Program() {
        await _tp.Query(BuildCommand(Command.Program), 3);
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> Disconnect() {
        await _tp.Query(BuildCommand(Command.Disconnect), 1);
        _tp.Disconnect();
        ConnectionStatus = false;
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> Reset() {
        var status = await _tp.Query(BuildCommand(Command.Reset), 1);
        return (CmdStatus) status.Status;
    }

    public async Task<(CmdStatus, byte[])> ReadMemory(uint addr, uint size) {
        var addressBytes = GetAddressBytes(addr);
        var sizeBytes = GetSizeBytes((ushort) size);
        var payload = addressBytes.Concat(sizeBytes).ToArray();
        var status =
            await _tp.Query(
                BuildCommand(Command.ReadMemory, payload), (byte)size+1);

        return ((CmdStatus, byte[])) (status.Status, status.Data);
    }

    public async Task<CmdStatus> WriteMemory(uint addr, byte[] data) {
        var addressBytes = GetAddressBytes(addr);
        var sizeBytes = GetSizeBytes((ushort) data.Length);
        var payload = addressBytes.Concat(sizeBytes).Concat(data).ToArray();
        var status =
            await _tp.Query(
                BuildCommand(Command.WriteMemory, payload),1);
        
        return (CmdStatus) status.Status;
    }

    public async Task<CmdStatus> ConfigureCyclicReadBlock(int readFrequency, int size, Tuple<uint, uint>[] blockDesc) {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> StartCyclicRead() {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> StopCyclicRead() {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> ClearReadBlockConfig() {
        return (CmdStatus) 0;
    }

    public async Task<CmdStatus> UpdateSoftware() {
        var status = _tp.Query(BuildCommand(Command.JumpToFbl), 1);
        return (CmdStatus) status.Status;
    }

    public async Task<CmdStatus> GetControlBlock() {
        return (CmdStatus) 0;
    }

    private async Task<CmdStatus> ProcessCommand() {
        return (CmdStatus) 0;
    }

    protected void OnCyclicDataRead() {
    }

    private byte[] BuildCommand(Command command, byte[]? payload = null) {
        if (payload == null) {
            return new[] {(byte) command};
        }

        var data = new byte[payload.Length + 1];
        data[0] = (byte) command;
        Buffer.BlockCopy(payload, 0, data, 1, payload.Length);
        return data;
    }

    private CmdStatus ProcessCommand(Command cmd) {
        return 0;
    }

    private byte[] GetAddressBytes(uint value) {
        return BitConverter.GetBytes(value);
    }

    private byte[] GetSizeBytes(ushort value) {
        return BitConverter.GetBytes(value);
    }
}

internal enum Command {
    Connect = 0xff,
    Disconnect = 0xfe,
    Reset,
    GetControlBlock,
    ReadMemory=0xf5,
    WriteMemory=0xf0,
    Program,
    ConfigureCyclicRead,
    StartCyclicRead,
    StopCyclicRead,
    ClearCyclicRead,
    JumpToFbl
}