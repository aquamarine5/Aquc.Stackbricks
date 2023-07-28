using Aquc.Stackbricks.DataClass;
using System.Diagnostics;
using System.Text.Json;

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
        return await Execute<UpdateDataClass>(new string[] { "self","update", "--json", "--no-log" });
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
        var output = await process.StandardOutput.ReadToEndAsync();
        var result=output.Split(DataClassManager.SPLIT_KEY);
        Console.WriteLine(output);
        var type = DataClassManager.ParseID(result[0]);
        return JsonSerializer.Deserialize(result[1], type);
    }
    public async Task<T> Execute<T>(string[] args)
        where T : IDataClass
    {
        return (T)(await Execute(args))!;
    }
}