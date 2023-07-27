namespace Aquc.Stackbricks.DataClass;

public class UpdateDataClass : CheckDownloadDataClass, IDataClass
{
    public new string DCID => ID;

    public new const string ID = "stbks.dc.update";

    public UpdateDataClass(bool isProgram, bool needUpdate, string version, string filePath, string depressedDir, bool isDirectory = true)
        : base(isProgram, needUpdate, version, filePath, depressedDir, isDirectory)
    {
    }

    public UpdateDataClass(bool isProgram, bool needUpdate, string version, string filePath, bool isDirectory = false)
        : base(isProgram, needUpdate, version, filePath, isDirectory)
    {
    }
}
public class CheckDataClass : IDataClass
{

    public string DCID => ID;

    public bool IsProgram { get; }

    public const string ID = "stbks.dc.check";
    public bool needUpdate;
    public string version;
    public CheckDataClass(bool isProgram, bool needUpdate, string version)
    {
        this.needUpdate = needUpdate;
        this.version = version;
        IsProgram = isProgram;
    }
}
public class CheckDownloadDataClass : CheckDataClass, IDataClass
{

    public new string DCID => ID;

    public new const string ID = "stbks.dc.checkdl";
    public string filePath;
    public string depressedDir;
    public bool isDirectory;
    public CheckDownloadDataClass(bool isProgram, bool needUpdate, string version, string filePath, string depressedDir, bool isDirectory = true)
        : base(isProgram, needUpdate, version)
    {
        this.filePath = filePath;
        this.depressedDir = depressedDir;
        this.isDirectory = isDirectory;

    }
    public CheckDownloadDataClass(bool isProgram, bool needUpdate, string version, string filePath, bool isDirectory = false)
        : base(isProgram, needUpdate, version)
    {
        this.needUpdate = needUpdate;
        this.version = version;
        this.filePath = filePath;
        this.isDirectory = isDirectory;
        depressedDir = string.Empty;

    }
}
public class InstallDataClass : IDataClass
{

    public string DCID => ID;

    public bool IsProgram { get; }

    public const string ID = "stbks.dc.install";
    public InstallDataClass(bool isProgram)
    {
        IsProgram = isProgram;
    }
}
public class ReadLastDateTimeDataClass : IDataClass
{
    public DateTime lastCheckTime;
    public DateTime lastUpdateTime;

    public DateTime lastSelfCheckTime;
    public DateTime lastSelfUpdateTime;

    public string DCID => ID;

    public bool IsProgram => false;

    public const string ID = "stbks.dc.readlasttime";
    public ReadLastDateTimeDataClass(DateTime lastCheckTime, DateTime lastUpdateTime, DateTime lastSelfCheckTime, DateTime lastSelfUpdateTime)
    {
        this.lastCheckTime = lastCheckTime;
        this.lastUpdateTime = lastUpdateTime;
        this.lastSelfCheckTime = lastSelfCheckTime;
        this.lastSelfUpdateTime = lastSelfUpdateTime;
    }
}
public interface IDataClass
{
    public string DCID { get; }
    public bool IsProgram { get; }
}
public class DataClassManager
{
    public const string SPLIT_KEY = "&&&";

    public readonly static Dictionary<Type, string> matchDictToID = new()
    {
        {typeof(UpdateDataClass),UpdateDataClass.ID },
        {typeof(CheckDataClass),CheckDataClass.ID },
        {typeof(CheckDownloadDataClass),CheckDownloadDataClass.ID },
        {typeof(InstallDataClass),InstallDataClass.ID },
    };
    public readonly static Dictionary<string,Type> matchDictToType = new()
    {
        {UpdateDataClass.ID,typeof(UpdateDataClass) },
        {CheckDataClass.ID , typeof(CheckDataClass) },
        {CheckDownloadDataClass.ID , typeof(CheckDownloadDataClass) },
        {InstallDataClass.ID , typeof(InstallDataClass) },
    };
    public static string ParseType<T>()
        where T : IDataClass
    {
        return matchDictToID[typeof(T)];
    }
    public static Type ParseID(string id)
    {
        return matchDictToType[id];
    }
}