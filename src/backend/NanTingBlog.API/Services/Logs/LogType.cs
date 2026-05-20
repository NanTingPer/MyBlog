// Copyright 2013-2015 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NanTingBlog.API.Services.Logs;

/// <summary>
/// An optional static entry point for logging that can be easily referenced
/// by different parts of an application. To configure the <see cref="Log"/>
/// set the Logger static property to a logger instance.
/// </summary>
/// <example>
/// Log.Logger = new LoggerConfiguration()
///     .WithConsoleSink()
///     .CreateLogger();
///
/// var thing = "World";
/// Log.Logger.Information("Hello, {Thing}!", thing);
/// </example>
/// <remarks>
/// The methods on <see cref="Log"/> (and its dynamic sibling <see cref="Serilog.ILogger"/>) are guaranteed
/// never to throw exceptions. Methods on all other types may.
/// </remarks>
public abstract class BaseLog
{
    private Serilog.ILogger _logger;

    /// <summary>
    /// 日志对象
    /// </summary>
    protected Serilog.ILogger Logger => _logger;

    /// <summary> </summary>
    public static event Action<string, object?[]?>? FatalEvent;

    /// <summary> </summary>
    public static event Action<Exception?, string, object?[]?>? FatalExEvent;

    /// <summary> </summary>
    public static event Action<string, object?[]?>? VerboseEvent;

    /// <summary> </summary>
    public static event Action<Exception?, string, object?[]?>? VerboseExEvent;

    /// <summary> </summary>
    public static event Action<string, object?[]?>? DebugEvent;

    /// <summary> </summary>
    public static event Action<Exception?, string, object?[]?>? DebugExEvent;

    /// <summary> </summary>
    public static event Action<string, object?[]?>? InformationEvent;

    /// <summary> </summary>
    public static event Action<Exception?, string, object?[]?>? InformationExEvent;

    /// <summary> </summary>
    public static event Action<string, object?[]?>? WarningEvent;

    /// <summary> </summary>
    public static event Action<Exception?, string, object?[]?>? WarningExEvent;

    /// <summary> </summary>
    public static event Action<string, object?[]?>? ErrorEvent;

    /// <summary> </summary>
    public static event Action<Exception?, string, object?[]?>? ErrorExEvent;

    /// <summary> </summary>
    public static event Action<LogEvent>? WriteLogEventEvent;

    /// <summary> </summary>
    public static event Action<LogEventLevel, string>? WriteMessageEvent;

    /// <summary> </summary>
    public static event Action<LogEventLevel, string, object?>? WriteMessageTEvent;

    /// <summary> </summary>
    public static event Action<LogEventLevel, string, object?, object?>? WriteMessageT0T1Event;

    /// <summary> </summary>
    public static event Action<LogEventLevel, string, object?, object?, object?>? WriteMessageT0T1T2Event;

    /// <summary> </summary>
    public static event Action<LogEventLevel, string, object?[]?>? WriteMessageParamsEvent;

    /// <summary> </summary>
    public static event Action<LogEventLevel, Exception?, string>? WriteExMessageEvent;

    /// <summary> </summary>
    public static event Action<LogEventLevel, Exception?, string, object?>? WriteExMessageTEvent;

    /// <summary> </summary>
    public static event Action<LogEventLevel, Exception?, string, object?, object?>? WriteExMessageT0T1Event;

    /// <summary> </summary>
    public static event Action<LogEventLevel, Exception?, string, object?, object?, object?>? WriteExMessageT0T1T2Event;

    /// <summary> </summary>
    public static event Action<LogEventLevel, Exception?, string, object?[]?>? WriteExMessageParamsEvent;

    /// <summary>
    /// 初始化Log
    /// </summary>
    protected abstract Serilog.ILogger CreateLogger();

    /// <summary>
    /// </summary>
    public BaseLog()
    {
        _logger = CreateLogger();
        ArgumentNullException.ThrowIfNull(_logger);
    }

    /// <summary>
    /// Resets <see cref="Logger"/> to the default and disposes the original if possible
    /// </summary>
    public void CloseAndFlush()
    {
        var logger = Interlocked.Exchange(ref _logger, Serilog.Core.Logger.None);

        (logger as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Create a logger that enriches log events via the provided enrichers.
    /// </summary>
    /// <param name="enricher">Enricher that applies in the context.</param>
    /// <returns>A logger that will enrich log events as specified.</returns>
    public Serilog.ILogger ForContext(ILogEventEnricher enricher)
    {
        return Logger.ForContext(enricher);
    }

    /// <summary>
    /// Create a logger that enriches log events via the provided enrichers.
    /// </summary>
    /// <param name="enrichers">Enrichers that apply in the context.</param>
    /// <returns>A logger that will enrich log events as specified.</returns>
    public Serilog.ILogger ForContext(ILogEventEnricher[] enrichers)
    {
        return Logger.ForContext(enrichers);
    }

    /// <summary>
    /// Create a logger that enriches log events with the specified property.
    /// </summary>
    /// <returns>A logger that will enrich log events as specified.</returns>
    public Serilog.ILogger ForContext(string propertyName, object? value, bool destructureObjects = false)
    {
        return Logger.ForContext(propertyName, value, destructureObjects);
    }

    /// <summary>
    /// Create a logger that marks log events as being from the specified
    /// source type.
    /// </summary>
    /// <typeparam name="TSource">Type generating log messages in the context.</typeparam>
    /// <returns>A logger that will enrich log events as specified.</returns>
    public Serilog.ILogger ForContext<TSource>() => Logger.ForContext<TSource>();

    /// <summary>
    /// Create a logger that marks log events as being from the specified
    /// source type.
    /// </summary>
    /// <param name="source">Type generating log messages in the context.</param>
    /// <returns>A logger that will enrich log events as specified.</returns>
    public Serilog.ILogger ForContext(Type source) => Logger.ForContext(source);

    /// <summary>
    /// Write an event to the log.
    /// </summary>
    /// <param name="logEvent">The event to write.</param>
    public void Write(LogEvent logEvent)
    {
        WriteLogEventEvent?.Invoke(logEvent);
        Logger.Write(logEvent);
    }

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write(LogEventLevel level, string messageTemplate)
    {
        WriteMessageEvent?.Invoke(level, messageTemplate);
        Logger.Write(level, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write<T>(LogEventLevel level, string messageTemplate, T propertyValue)
    {
        WriteMessageTEvent?.Invoke(level, messageTemplate, propertyValue);
        Logger.Write(level, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write<T0, T1>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        WriteMessageT0T1Event?.Invoke(level, messageTemplate, propertyValue0, propertyValue1);
        Logger.Write(level, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write<T0, T1, T2>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        WriteMessageT0T1T2Event?.Invoke(level, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        Logger.Write(level, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write(LogEventLevel level, string messageTemplate, params object?[]? propertyValues)
    {
        WriteMessageParamsEvent?.Invoke(level, messageTemplate, propertyValues);
        Logger.Write(level, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the specified level and associated exception.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write(LogEventLevel level, Exception? exception, string messageTemplate)
    {
        WriteExMessageEvent?.Invoke(level, exception, messageTemplate);
        Logger.Write(level, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the specified level and associated exception.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write<T>(LogEventLevel level, Exception? exception, string messageTemplate, T propertyValue)
    {
        WriteExMessageTEvent?.Invoke(level, exception, messageTemplate, propertyValue);
        Logger.Write(level, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the specified level and associated exception.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write<T0, T1>(LogEventLevel level, Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        WriteExMessageT0T1Event?.Invoke(level, exception, messageTemplate, propertyValue0, propertyValue1);
        Logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the specified level and associated exception.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write<T0, T1, T2>(LogEventLevel level, Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        WriteExMessageT0T1T2Event?.Invoke(level, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        Logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the specified level and associated exception.
    /// </summary>
    /// <param name="level">The level of the event.</param>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        WriteExMessageParamsEvent?.Invoke(level, exception, messageTemplate, propertyValues);
        Logger.Write(level, exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Determine if events at the specified level will be passed through
    /// to the log sinks.
    /// </summary>
    /// <param name="level">Level to check.</param>
    /// <returns><see langword="true"/> if the level is enabled; otherwise, <see langword="false"/>.</returns>
    public bool IsEnabled(LogEventLevel level) => Logger.IsEnabled(level);

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose(string messageTemplate)
    {
        Write(LogEventLevel.Verbose, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose<T>(string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Verbose, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Verbose, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Verbose, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose(string messageTemplate, params object?[]? propertyValues)
    {
        VerboseEvent?.Invoke(messageTemplate, propertyValues);
        Logger.Verbose(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose(Exception? exception, string messageTemplate)
    {
        Write(LogEventLevel.Verbose, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Verbose, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Verbose, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Verbose, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Verbose"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into space, wondering where this comet came from.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Verbose(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        VerboseExEvent?.Invoke(exception, messageTemplate, propertyValues);
        Logger.Verbose(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(string messageTemplate)
    {
        Write(LogEventLevel.Debug, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug<T>(string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Debug, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Debug, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Debug, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug("Starting up at {StartedAt}.", DateTime.Now);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(string messageTemplate, params object?[]? propertyValues)
    {
        DebugEvent?.Invoke(messageTemplate, propertyValues);
        Logger.Debug(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Debug(ex, "Swallowing a mundane exception.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(Exception? exception, string messageTemplate)
    {
        Write(LogEventLevel.Debug, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug(ex, "Swallowing a mundane exception.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Debug, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug(ex, "Swallowing a mundane exception.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Debug, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug(ex, "Swallowing a mundane exception.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Debug, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Debug"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Debug(ex, "Swallowing a mundane exception.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        DebugExEvent?.Invoke(exception, messageTemplate, propertyValues);
        Logger.Debug(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information(string messageTemplate)
    {
        Write(LogEventLevel.Information, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T>(string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Information, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Information, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information("Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information(string messageTemplate, params object?[]? propertyValues)
    {
        InformationEvent?.Invoke(messageTemplate, propertyValues);
        Logger.Information(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information(Exception? exception, string messageTemplate)
    {
        Write(LogEventLevel.Information, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Information, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Information, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Information, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Information"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Information(ex, "Processed {RecordCount} records in {TimeMS}.", records.Length, sw.ElapsedMilliseconds);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        InformationExEvent?.Invoke(exception, messageTemplate, propertyValues);
        Logger.Information(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(string messageTemplate)
    {
        Write(LogEventLevel.Warning, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning<T>(string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Warning, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Warning, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Warning, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning("Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(string messageTemplate, params object?[]? propertyValues)
    {
        WarningEvent?.Invoke(messageTemplate, propertyValues);
        Logger.Warning(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(Exception? exception, string messageTemplate)
    {
        Write(LogEventLevel.Warning, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Warning, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Warning, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Warning, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Warning"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Warning(ex, "Skipped {SkipCount} records.", skippedRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        WarningExEvent?.Invoke(exception, messageTemplate, propertyValues);
        Logger.Warning(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(string messageTemplate)
    {
        Write(LogEventLevel.Error, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error<T>(string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Error, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error("Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(string messageTemplate, params object?[]? propertyValues)
    {
        ErrorEvent?.Invoke(messageTemplate, propertyValues);
        Logger.Error(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(Exception? exception, string messageTemplate)
    {
        Write(LogEventLevel.Error, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Error, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Error, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Error, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Error"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Error(ex, "Failed {ErrorCount} records.", brokenRecords.Length);
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        ErrorExEvent?.Invoke(exception, messageTemplate, propertyValues);
        Logger.Error(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Fatal("Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(string messageTemplate)
    {
        Write(LogEventLevel.Fatal, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal("Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal<T>(string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Fatal, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal("Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Fatal, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal("Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Fatal, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal("Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(string messageTemplate, params object?[]? propertyValues)
    {
        FatalEvent?.Invoke(messageTemplate, propertyValues);
        Logger.Fatal(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Fatal(ex, "Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(Exception? exception, string messageTemplate)
    {
        Write(LogEventLevel.Fatal, exception, messageTemplate);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal(ex, "Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Write(LogEventLevel.Fatal, exception, messageTemplate, propertyValue);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal(ex, "Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Write(LogEventLevel.Fatal, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValue0">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue1">Object positionally formatted into the message template.</param>
    /// <param name="propertyValue2">Object positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal(ex, "Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Write(LogEventLevel.Fatal, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary>
    /// Write a log event with the <see cref="LogEventLevel.Fatal"/> level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Fatal(ex, "Process terminating.");
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Fatal(exception, messageTemplate, propertyValues);
        FatalExEvent?.Invoke(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Uses configured scalar conversion and destructuring rules to bind a set of properties to a
    /// message template. Returns false if the template or values are invalid (<summary>ILogger</summary>
    /// methods never throw exceptions).
    /// </summary>
    /// <param name="messageTemplate">Message template describing an event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <param name="parsedTemplate">The internal representation of the template, which may be used to
    /// render the <paramref name="boundProperties"/> as text.</param>
    /// <param name="boundProperties">Captured properties from the template and <paramref name="propertyValues"/>.</param>
    /// <example>
    /// MessageTemplate template;
    /// IEnumerable&lt;LogEventProperty&gt; properties>;
    /// if (Log.BindMessageTemplate("Hello, {Name}!", new[] { "World" }, out template, out properties)
    /// {
    ///     var propsByName = properties.ToDictionary(p => p.Name, p => p.Value);
    ///     Console.WriteLine(template.Render(propsByName, null));
    ///     // -> "Hello, World!"
    /// }
    /// </example>
    [MessageTemplateFormatMethod("messageTemplate")]
    public bool BindMessageTemplate(string messageTemplate, object?[] propertyValues,
        [NotNullWhen(true)] out MessageTemplate? parsedTemplate, [NotNullWhen(true)] out IEnumerable<LogEventProperty>? boundProperties)
    {
        return Logger.BindMessageTemplate(messageTemplate, propertyValues, out parsedTemplate, out boundProperties);
    }

    /// <summary>
    /// Uses configured scalar conversion and destructuring rules to bind a property value to its captured
    /// representation.
    /// </summary>
    /// <param name="propertyName">The name of the property. Must be non-empty.</param>
    /// <param name="value">The property value.</param>
    /// <param name="destructureObjects">If <see langword="true"/>, the value will be serialized as a structured
    /// object if possible; if <see langword="false"/>, the object will be recorded as a scalar or simple array.</param>
    /// <param name="property">The resulting property.</param>
    /// <returns>True if the property could be bound, otherwise false (<summary>ILogger</summary>
    /// methods never throw exceptions).</returns>
    public bool BindProperty(string propertyName, object? value, bool destructureObjects, [NotNullWhen(true)] out LogEventProperty? property)
    {
        return Logger.BindProperty(propertyName, value, destructureObjects, out property);
    }
}

/// <summary>
/// 聚合日志
/// </summary>
public static class GlobalLog
{
    [ModuleInitializer]
    internal static void Init() { }

    static GlobalLog()
    {
        var logRootDir = Path.Combine(AppContext.BaseDirectory, "logs");
        _logger = new LoggerConfiguration()
            .WriteTo.File(path: Path.Combine(logRootDir, "log_json_formatter.log"), formatter: new JsonFormatter(), rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .WriteTo.File(path: Path.Combine(logRootDir, "log.log"), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .CreateLogger();

        BaseLog.FatalEvent += Fatal;
        BaseLog.FatalExEvent += Fatal;
        BaseLog.VerboseEvent += Verbose;
        BaseLog.VerboseExEvent += Verbose;
        BaseLog.DebugEvent += Debug;
        BaseLog.DebugExEvent += Debug;
        BaseLog.InformationEvent += Information;
        BaseLog.InformationExEvent += Information;
        BaseLog.WarningEvent += Warning;
        BaseLog.WarningExEvent += Warning;
        BaseLog.ErrorEvent += Error;
        BaseLog.ErrorExEvent += Error;
        BaseLog.WriteLogEventEvent += Write;
        BaseLog.WriteMessageEvent += Write;
        BaseLog.WriteMessageTEvent += Write;
        BaseLog.WriteMessageT0T1Event += Write;
        BaseLog.WriteMessageT0T1T2Event += Write;
        BaseLog.WriteMessageParamsEvent += Write;
        BaseLog.WriteExMessageEvent += Write;
        BaseLog.WriteExMessageTEvent += Write;
        BaseLog.WriteExMessageT0T1Event += Write;
        BaseLog.WriteExMessageT0T1T2Event += Write;
        BaseLog.WriteExMessageParamsEvent += Write;
    }

    private static Serilog.ILogger _logger;

    /// <summary>
    /// 日志
    /// </summary>
    public static Serilog.ILogger Logger
    {
        get => _logger;
        set
        {
            if(value != null) {
                _logger = value;
                return;
            }
            ArgumentNullException.ThrowIfNull(value);
        }
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal(string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Fatal(messageTemplate, propertyValues);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Fatal(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Fatal(exception, messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Verbose(string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Verbose(messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Verbose(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Verbose(exception, messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Debug(string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Debug(messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Debug(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Debug(exception, messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Information(string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Information(messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Information(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Information(exception, messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Warning(string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Warning(messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Warning(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Warning(exception, messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error(string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Error(messageTemplate, propertyValues);
    }

    /// <summary></summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Error(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Error(exception, messageTemplate, propertyValues);
    }

    /// <summary> </summary>
    public static void Write(LogEvent logEvent)
    {
        Logger.Write(logEvent);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write(LogEventLevel level, string messageTemplate)
    {
        Logger.Write(level, messageTemplate);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write<T>(LogEventLevel level, string messageTemplate, T propertyValue)
    {
        Logger.Write(level, messageTemplate, propertyValue);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write<T0, T1>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Logger.Write(level, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write<T0, T1, T2>(LogEventLevel level, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Logger.Write(level, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write(LogEventLevel level, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Write(level, messageTemplate, propertyValues);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write(LogEventLevel level, Exception? exception, string messageTemplate)
    {
        Logger.Write(level, exception, messageTemplate);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write<T>(LogEventLevel level, Exception? exception, string messageTemplate, T propertyValue)
    {
        Logger.Write(level, exception, messageTemplate, propertyValue);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write<T0, T1>(LogEventLevel level, Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write<T0, T1, T2>(LogEventLevel level, Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Logger.Write(level, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    /// <summary> </summary>
    [MessageTemplateFormatMethod("messageTemplate")]
    public static void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Logger.Write(level, exception, messageTemplate, propertyValues);
    }
}