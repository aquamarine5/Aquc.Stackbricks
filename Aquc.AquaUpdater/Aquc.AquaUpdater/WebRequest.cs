using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Aquc.AquaUpdater.Net;

public static class WebRequestExtention
{
    public static WebResponse SendGet(this HttpWebRequest webRequest) => webRequest.SendRequest("GET");
    public static WebResponse SendPost(this HttpWebRequest webRequest, string body = "") => webRequest.AddText(body).SendRequest("POST");
    public static WebResponse SendPut(this HttpWebRequest webRequest, string body = "") => webRequest.AddText(body).SendRequest("PUT");
    public static WebResponse SendRequest(this HttpWebRequest webRequest, string method)
    {
        webRequest.Method = method;
        return webRequest.GetResponse();
    }
}
public static class WebRequestStreamExtension
{
    public static HttpWebRequest AddText(this HttpWebRequest webRequest, string text, string encoding = "UTF-8")
    {
        webRequest.Method = "POST"; // See: https://github.com/awesomehhhhh/Aesc.AwesomeKits/issues/8
        if (text == "") return webRequest;
        var stream = webRequest.GetRequestStream();
        var bytes = Encoding.GetEncoding(encoding).GetBytes(text);
        stream.Write(bytes);
        return webRequest;
    }
    public static HttpWebRequest AddText(this HttpWebRequest webRequest, string text, Encoding encoding)
        => webRequest.AddText(text, encoding.BodyName);
    public static HttpWebRequest AddFormdata(this HttpWebRequest webRequest,MultipartFormDataContent multipartFormData,string boundary)
    {
        webRequest.Method = "POST"; // See: https://github.com/awesomehhhhh/Aesc.AwesomeKits/issues/8
        webRequest.ContentType = $"multipart/form-data; boundary={boundary}";
        multipartFormData.CopyToAsync(webRequest.GetRequestStream());
        return webRequest;
    }
    public static HttpWebRequest AddFile(this HttpWebRequest webRequest, string filePath)
    {
        webRequest.Method = "POST"; // See: https://github.com/awesomehhhhh/Aesc.AwesomeKits/issues/8
        var file = new FileInfo(filePath);
        webRequest.ContentLength = file.Length;
        var stream = webRequest.GetRequestStream();
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var fileBuffer = new byte[1024];
        int totalSize = 0;
        int size = fileStream.Read(fileBuffer, 0, fileBuffer.Length);
        while (size > 0)
        {
            totalSize += size;
            stream.Write(fileBuffer, 0, size);
            size = fileStream.Read(fileBuffer, 0, fileBuffer.Length);
        }
        fileStream.Close();
        return webRequest;
    }
}
public static class WebResponseExtension
{
    public static string ReadText(this WebResponse webResponse)
    {
        StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
        string result = streamReader.ReadToEnd();
        streamReader.Close();
        return result;
    }
    public static JObject ReadJsonObject(this WebResponse webResponse) 
        => JObject.Parse(webResponse.ReadText());

    public static void WriteToFile(this WebResponse webResponse, string filePath)
    {
        Stream stream = webResponse.GetResponseStream();
        if (File.Exists(filePath))
            File.Delete(filePath);
        Console.WriteLine(filePath);
        FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        byte[] byteSuffer = new byte[1024];
        int totalSize = 0;
        int size = stream.Read(byteSuffer, 0, byteSuffer.Length);
        while (size > 0)
        {
            totalSize += size;
            fileStream.Write(byteSuffer, 0, size);
            size = stream.Read(byteSuffer, 0, byteSuffer.Length);
        }
        fileStream.Flush();
        fileStream.Close();
    }
}
