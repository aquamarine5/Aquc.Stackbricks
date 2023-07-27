using Aquc.Stackbricks.DataClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;

public class DataClassParser
{
    public static string ParseDataClass<T>(T data)
        where T : IDataClass
    {
        var str = DataClassManager.ParseType<T>() + DataClassManager.SPLIT_KEY + JsonConvert.SerializeObject(data);
        return str;
    }
    public static void ParseDataClassPrintin<T>(T data)
        where T : IDataClass
    {
        Console.WriteLine(ParseDataClass(data));
    }
    
    public static UpdateDataClass ParseUpdateDC(StackbricksUpdatePackage updatePackage, bool isProgram)
    {
        if (updatePackage.isZip)
            return new UpdateDataClass(isProgram, true, updatePackage.updateMessage.version.ToString(), updatePackage.file);
        else
            return new UpdateDataClass(isProgram, true, updatePackage.updateMessage.version.ToString(), updatePackage.file, updatePackage.depressedDir.FullName);
    }

    public static UpdateDataClass ParseUpdateDC(StackbricksUpdateMessage updateMessage, bool isProgram)
    {
        return new UpdateDataClass(isProgram, false, updateMessage.version.ToString(), string.Empty);
    }
    public static CheckDataClass ParseCheckDC(StackbricksUpdateMessage updateMessage, bool isProgram)
    {
        return new CheckDataClass(isProgram, updateMessage.NeedUpdate(), updateMessage.version.ToString());
    }
    public static CheckDownloadDataClass ParseCheckDownloadDC(StackbricksUpdatePackage updatePackage, bool isProgram)
    {
        return ParseUpdateDC(updatePackage, isProgram);
    }
    public static InstallDataClass ParseInstallDC(bool isProgram)
    {
        return new InstallDataClass(isProgram);
    }
}
