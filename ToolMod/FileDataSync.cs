namespace ToolMod;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Android/file-based replacement for <see cref="DataSync"/>.
///
/// The launcher UI and the plugin live in the same app sandbox, so instead of a
/// Windows named pipe we use the BepInEx config directory:
///
///   {config}/toolmod_inbox/   UI drops one &lt;n&gt;.json file per command; plugin
///                             reads and deletes it, then raises MessageReceived.
///   {config}/toolmod_outbox.jsonl
///                             plugin appends one JSON object per line; the UI
///                             tails the file for InitData / replies.
///
/// The wire format is the exact same <c>SyncData</c> JSON used by the desktop
/// named pipe, so no command names or payloads change.
/// </summary>
public sealed class FileDataSync : IToolSync
{
    private readonly string _inboxDir;
    private readonly string _outboxFile;
    private readonly object _outLock = new();
    private CancellationTokenSource _cts;
    private Task _pollTask;
    private bool _isRunning;
    private bool _disposed;
    private int _lastOutLength;

    public event EventHandler Connected;
    public event EventHandler<string> MessageReceived;
    public event EventHandler Disconnected;
    public event EventHandler<Exception> ErrorOccurred;

    public FileDataSync(string configDir)
    {
        if (string.IsNullOrEmpty(configDir)) throw new ArgumentNullException(nameof(configDir));
        _inboxDir = Path.Combine(configDir, "toolmod_inbox");
        _outboxFile = Path.Combine(configDir, "toolmod_outbox.jsonl");
    }

    public void Start()
    {
        if (_isRunning) return;
        Directory.CreateDirectory(_inboxDir);
        _isRunning = true;
        _cts = new CancellationTokenSource();
        _pollTask = Task.Run(() => PollLoopAsync(_cts.Token));
        Connected?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        try { _cts?.Cancel(); } catch { }
        try { _pollTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Append one message line to the outbox (thread safe).</summary>
    public Task SendAsync(string message)
    {
        if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
        lock (_outLock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_outboxFile)!);
            using var stream = new FileStream(_outboxFile, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.WriteLine(message);
            writer.Flush();
        }
        return Task.CompletedTask;
    }

    private async Task PollLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(_inboxDir, "*.json")
                             .OrderBy(f => f, StringComparer.Ordinal))
                {
                    string text;
                    try { text = File.ReadAllText(file); }
                    catch (IOException) { continue; }
                    try { File.Delete(file); } catch { }

                    foreach (var line in text.Split('\n'))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.Length > 0) MessageReceived?.Invoke(this, trimmed);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, ex);
            }

            try { await Task.Delay(50, token); }
            catch (TaskCanceledException) { break; }
        }
    }

    /// <summary>Truncate the outbox (called when a fresh UI session starts).</summary>
    public void ResetOutbox()
    {
        lock (_outLock)
        {
            try { if (File.Exists(_outboxFile)) File.Delete(_outboxFile); } catch { }
            _lastOutLength = 0;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _cts?.Dispose();
    }
}
