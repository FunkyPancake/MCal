using System;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CalProtocol;
using CalProtocol.TransportProtocol;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace CanUpdaterGui {
    public class CanFrame {
        public uint Id { get; set; }
        public byte[] Payload { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public FrameDirection Direction { get; set; }
    }

    public enum FrameDirection {
        Tx,
        Rx
    }

    class InMemorySink : ILogEventSink {
        readonly ITextFormatter _textFormatter =
            new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");

        public ConcurrentQueue<string> Events { get; } = new ConcurrentQueue<string>();

        public void Emit(LogEvent logEvent) {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            var renderSpace = new StringWriter();
            _textFormatter.Format(logEvent, renderSpace);
            Events.Enqueue(renderSpace.ToString());
            NewLogEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler NewLogEvent;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static RoutedCommand DisconnectCmd = new RoutedCommand();
        public static RoutedCommand ProgramCmd = new RoutedCommand();
        public static RoutedCommand ConnectCmd = new RoutedCommand();
        private readonly ILogger _logger;
        private CalibrationProtocol _calibrationProtocol;
        public MainWindow() {
            var sink = new InMemorySink();
            sink.NewLogEvent += UpdateText;
            _logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink)
                .CreateLogger();
            var tp = new SerialTp(_logger);
            _calibrationProtocol = new CalibrationProtocol(_logger,tp);
            InitializeComponent();
        }

        public CanFrame[] Frames { get; set; } = {
            new() {Id = 0x12341, Payload = new byte[] {1, 2, 3, 4, 5, 6, 7, 8}},
            new() {Id = 0x871, Payload = new byte[] {12}}
        };

        private void UpdateText(object sender, EventArgs e) {
            if (((InMemorySink) sender).Events.TryDequeue(out var text))
                LoggerTextBox.Text += text+"\n";
        }

        private async void ConnectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
           await _calibrationProtocol.Connect();
        }

        private void DisconnectCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            _calibrationProtocol.Disconnect();
        }

        private async void ProgramCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
           await _calibrationProtocol.Program();
        }
    }
}