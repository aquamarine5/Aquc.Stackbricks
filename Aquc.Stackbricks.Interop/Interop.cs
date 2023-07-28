using Aquc.Stackbricks.DataClass;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Aquc.Stackbricks.Interop;

public class StackbricksInterop
{
    public FileInfo processFile;
    public StackbricksInterop(FileInfo file)
    {
        processFile= file;
    }
    public async Task<UpdateDataClass> Update()
    {
        return await Execute<UpdateDataClass>(new string[] { "update", "--json", "--no-log" });
    }
    public async Task<object?> Execute(string[] args)
    {
        var arg = string.Join(" ", args);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = processFile.Name,
                Arguments = arg,
                CreateNoWindow = true,
                WorkingDirectory = processFile.DirectoryName,
                RedirectStandardOutput= true,
            }
        };
        process.Start();
        await process.WaitForExitAsync();
        var result=(await process.StandardOutput.ReadToEndAsync()).Split(DataClassManager.SPLIT_KEY);
        Console.WriteLine(result[1]);
        var type = DataClassManager.ParseID(result[0]);
        return JsonConvert.DeserializeObject(result[1], type);
    }
    public async Task<T> Execute<T>(string[] args)
        where T : IDataClass
    {
        return (T)(await Execute(args))!;
    }
}