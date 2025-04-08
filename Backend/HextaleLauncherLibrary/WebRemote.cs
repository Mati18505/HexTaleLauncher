using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HexTaleLauncherLibrary
{
    class WebRemote
    {
        public Uri uri { get; private set; }
        string rootDir;
        HttpClient client;

        public event EventHandler? DataReceivedHandler = null;
        public class ResponseData : EventArgs
        {
            public string Data {get; set;}
        }

        public WebRemote(Uri uri, string rootDir)
        {
            this.uri = uri;
            this.rootDir = rootDir;
            client = new();
        }

        public string DownloadFileText(string fileName)
        {
            var fileUri = GetFullUri(fileName);
            var task = Task.Run(() => client.GetStringAsync(fileUri));
            task.Wait();
            return task.Result;
        }

        public async Task<string> DownloadFileTextAsync(string fileName)
        {
            var fileUri = GetFullUri(fileName);
            var responseText = await client.GetStringAsync(fileUri);
            DataReceivedHandler?.Invoke(this, new ResponseData { Data = responseText });
            return responseText;
        }

        public void DownloadFile(string remoteFilePath, string localFilePath)
        {
            var fileUri = GetFullUri(remoteFilePath);

            var ms = new MemoryStream();

            HttpResponseMessage response;
            var task = Task.Run(() => client.GetAsync(fileUri));
            task.Wait();
            response = task.Result;
            response.Content.CopyToAsync(ms);

            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
            FilesManager.SaveToFile(localFilePath, ms);
        }

        public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
        {
            string text = await DownloadFileTextAsync(remoteFilePath);
            MemoryStream ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(text);
            sw.Flush();
            FilesManager.SaveToFile(localFilePath, ms);
            DataReceivedHandler?.Invoke(this, new());
        }

        private string GetAbsolutePath(string fileName)
        {
            return Path.Combine(rootDir, fileName);
        }

        public Uri GetFullUri(string fileName) 
        { 
            return new Uri(uri, GetAbsolutePath(fileName));
        }
    }
}
