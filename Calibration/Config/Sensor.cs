namespace CalProtocol.Config;

public class Sensor {
    private IUnitGroup _unitGroup;
    private uint valueRaw;

    public dynamic Value { get; }

    public Sensor(IUnitGroup unitGroup) {
        _unitGroup = unitGroup;
    }

    public override string ToString() {
        return base.ToString();
    }
}