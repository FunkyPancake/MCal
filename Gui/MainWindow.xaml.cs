using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using CalProtocol;
using CalProtocol.Config;
using CalProtocol.TransportProtocol;
using Serilog;

namespace CanUpdaterGui {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static readonly RoutedCommand DisconnectCmd = new();
        public static readonly RoutedCommand ProgramCmd = new();
        public static readonly RoutedCommand ConnectCmd = new();
        public static readonly RoutedCommand LoadCmd = new();

        private readonly ILogger _logger;
        private readonly Calibration _calibration;


        public MainWindow() {
            var pathToConfig = Path.Combine(System.Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MCal");
            if (!Directory.Exists(pathToConfig)) {
                Directory.CreateDirectory(pathToConfig);
            }

            var sink = new InMemorySink();
            sink.NewLogEvent += UpdateText!;
            _logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink)
                .CreateLogger();
            _calibration = new Calibration(_logger);
            InitializeComponent();
            _calibration.UpdateTpConfiguration(new TpConfig() {
                ConnectionType = TpConfig.TpType.SerialPort,
                SerialConfig = new SerialTpConfig() {
                    ComPort = "COM7",
                    Baudrate = 460800,
                    CommunicationTimeout = 50000
                }
            });
        }

        private void UpdateText(object sender, EventArgs e) {
            if (((InMemorySink) sender).Events.TryDequeue(out var text))
                LoggerTextBox.Text += text + "\n";
        }

        private async void ConnectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            await _calibration.Connect();
        }

        private void DisconnectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
        }

        private async void ProgramCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            await _calibration.ReadMemory(0x20000090, 8);
        }

        private async void LoadCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            var filePath = SelectFilePath();
            if (_calibration.IsConnected) {
            }

            var result = await _calibration.Load(filePath);
            var treeNode = new TreeViewItem {
                Header = result.Name,
                IsExpanded = true
            };
            CalTree.Items.Add(treeNode);
            BuildCalTreeView(treeNode, result);
        }

        private static void BuildCalTreeView(ItemsControl treeNode, XElement element) {
            if (element.HasElements) {
                foreach (var child in element.Elements()) {
                    var childTreeNode = new TreeViewItem {
                        //Get First attribute where it is equal to value
                        Header = child?.Attributes().First(s => s.Name == "Name").Value,
                        //Automatically expand elements
                        IsExpanded = true,
                    };
                    treeNode.Items.Add(childTreeNode);
                    BuildCalTreeView(childTreeNode, child!);
                }
            }
            else {
                var childTreeNode = new TreeViewItem {
                    //Get First attribute where it is equal to value
                    Header = element.Attribute("Name")!.Value,
                    //Automatically expand elements
                    IsExpanded = true,
                    Tag = element.Attribute("id")!.Value
                };
                treeNode.Items.Add(childTreeNode);
            }
        }


        private string SelectFilePath() {
            return "./schema.xml";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void OnKeyDownHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                _logger.Information("New value: {value}", NumberTextBox.Text);
                await _calibration.Program(uint.Parse(NumberTextBox.Text));
            }
        }

        private void CalTree_OnKeyDown(object sender, KeyEventArgs e) {
            var s = sender as TreeView;
            var item = s!.SelectedItem as TreeViewItem;
            if (e.Key == Key.Return && item!.Tag != null)
                _logger.Debug("Node selected:{name}, id: {id}", item.Header, item.Tag.ToString());
        }
    }
}