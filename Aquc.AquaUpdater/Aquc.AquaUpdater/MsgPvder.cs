using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Aquc.AquaUpdater.Pvder;
using Microsoft.Extensions.DependencyInjection;

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
public class ProviderController
{
    private readonly IServiceProvider serviceDescriptors;
    public ProviderController(IServiceProvider serviceDescriptors)
    {
        this.serviceDescriptors = serviceDescriptors;
    }
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
        => new List<string> { "aliyunfilepvder", "huang1111filepvder" }.Contains(identity);
    public static IUpdateFilesProvider GetFilesProvider(string identity)
    {
        return (identity?.ToLower()) switch
        {
            "aliyunfilepvder" => new AliyundrivePvder(),
            "huang1111filepvder"=>new Huang1111Pvder(),
            null => null,
            _ => null,
        };
    }
}
