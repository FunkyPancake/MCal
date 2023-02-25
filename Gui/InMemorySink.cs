using System;
using System.Collections.Concurrent;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace CanUpdaterGui;

internal class InMemorySink : ILogEventSink {
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

    public event EventHandler? NewLogEvent;
}