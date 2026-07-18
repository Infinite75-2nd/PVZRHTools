using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PVZRHTools.Utils;
using ToolData;

namespace PVZRHTools.Services;

public class DataSyncService : IDisposable, IDataSyncService
{
    private readonly string _serverName;
    private readonly string _pipeName;
    private NamedPipeClientStream _pipeStream;
    private StreamReader _reader;
    private StreamWriter _writer;
    private CancellationTokenSource _cts;
    private Task _clientTask;
    private bool _disposed;
    private bool _locked = false;

    /// <summary>连接成功</summary>
    public event EventHandler Connected;

    /// <summary>收到消息</summary>
    public event EventHandler<SyncData> MessageReceived;

    /// <summary>断开连接</summary>
    public event EventHandler Disconnected;

    /// <summary>发生错误</summary>
    public event EventHandler<Exception> ErrorOccurred;

    /// <param name="pipeName">管道名称</param>
    /// <param name="serverName">服务器名称，本地为"."</param>
    public DataSyncService(string serverName = ".")
    {
        _pipeName = Strings.PipeName;
        _serverName = serverName ?? throw new ArgumentNullException(nameof(serverName));
    }

    /// <summary>
    /// 连接到服务端（异步）
    /// </summary>
    public async Task ConnectAsync()
    {
        if (_clientTask is { IsCompleted: false })
            throw new InvalidOperationException("客户端已在运行");

        _cts = new CancellationTokenSource();
        _clientTask = Task.Run(() => RunClientAsync(_cts.Token));
        await Task.CompletedTask; // 让调用方异步等待连接完成？但内部会触发Connected事件，可让调用方自行等待事件
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        _cts?.Cancel();
        try
        {
            _pipeStream?.Dispose();
        }
        catch
        {
        }
    }

    /// <summary>
    /// 发送消息（异步）
    /// </summary>
    public async Task SendMessageAsync(string message)
    {
        if (_writer == null)
            throw new InvalidOperationException("未连接");

        if (_locked) return;

        await _writer.WriteLineAsync(message);
        await _writer.FlushAsync();
    }

    public async Task SendCommand(SyncData data) =>
        await SendMessageAsync(JsonSerializer.Serialize(data, JsonSGC.Default.SyncData));

    public void Lock(bool locked) => _locked = locked;

    private async Task RunClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            _pipeStream = new NamedPipeClientStream(
                _serverName,
                _pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await _pipeStream.ConnectAsync(cancellationToken);
            _reader = new StreamReader(_pipeStream);
            _writer = new StreamWriter(_pipeStream) { AutoFlush = true };
            OnConnected();

            // 循环接收消息
            string message;
            while (!cancellationToken.IsCancellationRequested &&
                   (message = await _reader.ReadLineAsync()) != null)
            {
                try
                {
                    //Locator.Current.GetService<INotificationService>().NotificationManager.Show(message);
                    SyncData syncData = JsonSerializer.Deserialize(message, JsonSGC.Default.SyncData);
                    OnMessageReceived(syncData);
                }
                catch
                {
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常停止
        }
        catch (Exception ex)
        {
            OnErrorOccurred(ex);
        }
        finally
        {
            Cleanup();
            OnDisconnected();
        }
    }

    private void Cleanup()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _pipeStream?.Dispose();
        _reader = null;
        _writer = null;
        _pipeStream = null;
    }

    private void OnConnected() => Connected?.Invoke(this, EventArgs.Empty);
    private void OnMessageReceived(SyncData data) => MessageReceived?.Invoke(this, data);
    private void OnDisconnected() => Disconnected?.Invoke(this, EventArgs.Empty);
    private void OnErrorOccurred(Exception ex) => ErrorOccurred?.Invoke(this, ex);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Disconnect();
        _cts?.Dispose();
        Cleanup();
    }
}

public interface IDataSyncService
{
    public event EventHandler Connected;
    public event EventHandler<SyncData> MessageReceived;
    public event EventHandler Disconnected;
    public event EventHandler<Exception> ErrorOccurred;
    public Task ConnectAsync();
    public Task SendCommand(SyncData data);
    public void Lock(bool locked);
}