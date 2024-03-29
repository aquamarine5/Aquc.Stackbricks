﻿using Aquc.Stackbricks.DataClass;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aquc.Stackbricks.Interop;

public class StackbricksInterop
{
    public FileInfo processFile;
    public StackbricksInterop(FileInfo file)
    {
        processFile= file;
    }
    public async Task<InteropDataClass<UpdateDataClass>> Update()
    {
        return await Execute<UpdateDataClass>(new string[] { "self","update", "--json", "--no-log" });
    }
    public async Task<InteropDataClass<T>> Execute<T>(string[] args)
        where T : IDataClass
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
        if (result[0] == ExceptionDataClass.ID)
            return new InteropDataClass<T>(default, JsonSerializer.Deserialize<ExceptionDataClass>(result[1]));
        else
            return new InteropDataClass<T>((T)JsonSerializer.Deserialize(result[1], type)!,default);
    }
}