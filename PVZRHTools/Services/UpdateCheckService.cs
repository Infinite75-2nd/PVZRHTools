using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using PVZRHTools.Utils;
using ToolData;

namespace PVZRHTools.Services;

public class UpdateCheckService(INotificationService notificationService)
{
    private const string UpdateUrl =
        "https://raw.githubusercontent.com/Infinite75-2nd/PVZRHTools/alpha-dev/update.json";

    public async Task CheckAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PVZRHTools-UpdateChecker/1.0");

        string? json = null;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                json = await httpClient.GetStringAsync(UpdateUrl, cts.Token);
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                try { await Task.Delay(100, cts.Token); }
                catch (OperationCanceledException) { break; }
            }
        }

        if (json == null) return;

        UpdateInfo? info;
        try
        {
            info = JsonSerializer.Deserialize(json, JsonSGC.Default.UpdateInfo);
        }
        catch
        {
            return;
        }

        if (IsRemoteNewer(Strings.GameVersion, info.Value.GameVersion) || IsRemoteNewer(Strings.ModifierVersion, info.Value.ModifierVersion))
        {
            NotifyUpdate(info.Value);
        }
    }

    private void NotifyUpdate(UpdateInfo info)
    {
        var version = $"{info.GameVersion}-{info.ModifierVersion}";
        Dispatcher.UIThread.Post(() =>
        {
            notificationService.NotificationManager?.Show(
                $"发现新版本 {version}",
                NotificationType.Information);
        });
    }

    private static bool IsRemoteNewer(string localVersion, string remoteVersion)
    {
        var (localBase, localPre) = ParseVersion(localVersion);
        var (remoteBase, remotePre) = ParseVersion(remoteVersion);

        if (!Version.TryParse(localBase, out var localVer)) return false;
        if (!Version.TryParse(remoteBase, out var remoteVer)) return false;

        int cmp = remoteVer.CompareTo(localVer);
        if (cmp != 0) return cmp > 0;

        if (string.IsNullOrEmpty(localPre) && string.IsNullOrEmpty(remotePre)) return false;
        if (string.IsNullOrEmpty(remotePre)) return true;
        if (string.IsNullOrEmpty(localPre)) return false;

        return string.Compare(remotePre, localPre, StringComparison.Ordinal) > 0;
    }

    private static (string Base, string PreRelease) ParseVersion(string version)
    {
        int dashIdx = version.IndexOf('-');
        return dashIdx >= 0
            ? (version[..dashIdx], version[(dashIdx + 1)..])
            : (version, "");
    }
}
