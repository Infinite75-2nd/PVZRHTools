namespace ToolMod;

using System;
using System.Threading.Tasks;

/// <summary>Common surface for the desktop named-pipe and Android file transports.</summary>
public interface IToolSync : IDisposable
{
    event EventHandler Connected;
    event EventHandler<string> MessageReceived;
    event EventHandler Disconnected;
    event EventHandler<Exception> ErrorOccurred;

    void Start();
    void Stop();
    Task SendAsync(string message);
}
