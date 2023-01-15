using System;
using System.Collections.Generic;
using System.Text;
using Aquc.AquaUpdater.Pvder;

namespace Aquc.AquaUpdater;

public interface IUpdateMessageProvider
{
    public string Identity { get; }
    UpdateMessage GetUpdateMessage(UpdateSubscription updateSubscription);
}
public interface IUpdateFilesProvider
{
    public string Identity { get; }
    UpdatePackage DownloadPackage(UpdateMessage updateMessage);
}
public class Provider
{
    public static IUpdateMessageProvider GetMessageProvider(string identity)
    {
        return (identity?.ToLower()) switch
        {
            "bilibilimsgpvder" => new BiliCommitMsgPvder(),
            null => null,
            _ => null,
        };
    }
    public static bool ContainInMessageProvider(string identity)
        => new List<string> { "bilibilimsgpvder" }.Contains(identity);
    public static bool ContainInFileProvider(string identity)
        => new List<string> { "aliyunfilepvder" }.Contains(identity);
    public static IUpdateFilesProvider GetFilesProvider(string identity)
    {
        return (identity?.ToLower()) switch
        {
            "aliyunfilepvder" => new AliyundrivePvder(),
            null => null,
            _ => null,
        };
    }
}
