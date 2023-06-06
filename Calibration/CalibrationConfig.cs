using System.Xml;
using System.Xml.Linq;
using CalProtocol.Config;
using CalProtocol.Config.CalItems;
using Serilog;

namespace CalProtocol;

public class CalibrationConfig {
    private const string ConfigExtension = ".xml";
    private readonly ILogger _logger;
    private readonly Dictionary<Guid, Variable> _variables = new();
    private readonly Dictionary<Guid, IUnitGroup> _unitGroups = new();
    private readonly Dictionary<Guid, IUnitGroup> _cal = new();
    private XElement _calTree = new("Calibration");

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
        var reader = XmlReader.Create(streamReader, settings);
        var tree = await XElement.LoadAsync(reader, LoadOptions.None, CancellationToken.None);

        foreach (var element in tree.Element("UnitGroups")?.Elements()!) {
            ParseUnitGroup(element);
        }

        foreach (var element in tree?.Element("Variables")?.Elements()!) {
            ParseVariable(element);
        }

        foreach (var element in tree.Element("Calibration")?.Elements()!) {
            ParseCalItem(element);
            BuildTree(_calTree, element);
        }


        return true;
    }

    private static void BuildTree(XContainer parent, XElement element) {
        if (element.Name == "CalGroup") {
            var name = element.Attribute("Name")!.Value;
            var node = new XElement("CalGroup", new XAttribute("Name", name));
            parent.Add(node);
            var subElements = element.Elements().Where(x => x.Name == "CalGroup" || x.Name == "CalItem");
            foreach (var subElement in subElements) {
                BuildTree(node, subElement);
            }
        }
        else {
            if (element.Name != "CalItem")
                return;
            var name = element.Element("LongName")!.Value;
            var id = element.Attribute("id")!;
            var node = new XElement("CalItem", new XAttribute("Name", name),id);
            parent.Add(node);
        }
    }


    public XElement GetCalTree() {
        return _calTree;
    }

    public ICalItem GetCalById() {
        return new Axis();
    }

    private void ParseCalItem(XElement element) {
    }

    private void ParseVariable(XElement element) {
        var id = Guid.Parse(element.Attribute("id")!.Value);
        var tmp = element.Element("UnitGroup")!.Attribute("id")!.Value;
        var unitGroupId = Guid.Parse(tmp);
        _variables.Add(id,new Variable(id, _unitGroups[unitGroupId]));
    }

    private void ParseUnitGroup(XElement element) {
        var id = Guid.Parse(element!.Attribute("id")!.Value);

        _unitGroups.Add(id, new UnitGroup(id, "texst sample"));
    }
}