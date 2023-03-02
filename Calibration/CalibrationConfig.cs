using System.Xml;
using System.Xml.Linq;
using CalProtocol.Config;
using Serilog;

namespace CalProtocol;

public class CalibrationConfig {
    private readonly ILogger _logger;
    private Dictionary<Guid,IUnitGroup> _unitGroups = new();
    public readonly List<Variable> Variables = new();
    public CalibrationConfig(ILogger logger) {
        _logger = logger;
    }

    public async Task<bool> Load(string filePath) {
        if (!File.Exists(filePath) || Path.GetExtension(filePath) != ConfigExtension) {
            _logger.Error("Invalid file path,{path}", filePath);
            return false;
        }

        using var streamReader = new StreamReader(filePath);
        var settings = new XmlReaderSettings() {
            Async = true,
            IgnoreWhitespace = true,
            IgnoreComments = true
        };
        var reader = XmlReader.Create(streamReader,settings);
        var tree = await XElement.LoadAsync(reader, LoadOptions.None, CancellationToken.None);
        foreach (var element in tree.Element("UnitGroups")?.Elements()!) {
            var id = Guid.Parse(element!.Attribute("id")!.Value);
            _unitGroups.Add(id,new UnitGroup(id,"texst sample"));
        }
        foreach (var element in tree?.Element("Variables")?.Elements()!) {
            var id = Guid.Parse(element.Attribute("id")!.Value);
            var tmp = element.Element("UnitGroup")!.Attribute("id")!.Value;
            var unitGroupId = Guid.Parse(tmp);
            Variables.Add(new Variable(id,_unitGroups[unitGroupId]));
        }

        var calItems = tree.Element("Calibration").Elements();
        foreach (var calItem in calItems) {
            // calItem.ResolveGuid();
        }
        
        return true;
    }

    private const string ConfigExtension = ".xml";
}