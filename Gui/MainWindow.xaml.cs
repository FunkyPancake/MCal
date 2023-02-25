using System;
using System.Windows;
using System.Windows.Input;
using CalProtocol;
using CalProtocol.TransportProtocol;
using Serilog;

namespace CanUpdaterGui {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static RoutedCommand DisconnectCmd = new();
        public static RoutedCommand ProgramCmd = new();
        public static RoutedCommand ConnectCmd = new();
        
        private readonly ILogger _logger;
        private readonly CalibrationProtocol _calProtocol;
        private readonly CalibrationConfig _calConfig;

        public MainWindow() {
            var sink = new InMemorySink();
            sink.NewLogEvent += UpdateText!;
            _logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink)
                .CreateLogger();
            var tp = new SerialTp(_logger);
            _calProtocol = new CalibrationProtocol(_logger, tp);
            _calConfig = new CalibrationConfig(_logger);
            InitializeComponent();
            LoggerTextBox.Text += "available channels" + "\n";
            foreach (var channel in tp.GetAvailableChannels()) {
                LoggerTextBox.Text += channel + "\n";
            }
            
        }

        private void UpdateText(object sender, EventArgs e) {
            if (((InMemorySink) sender).Events.TryDequeue(out var text))
                LoggerTextBox.Text += text + "\n";
        }

        private async void ConnectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            await _calProtocol.Connect();
        }

        private void DisconnectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            _calProtocol.Disconnect();
        }

        private async void ProgramCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            await _calProtocol.Program();
        }

        private async void LoadCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            var filePath = SelectFilePath();
            _calConfig.Load(filePath);
        }

        private string SelectFilePath() {
            throw new NotImplementedException();
        }
    }
}