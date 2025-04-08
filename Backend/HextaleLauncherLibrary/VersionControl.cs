using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HexTaleLauncherLibrary
{
    public class VersionControl
    {
        public struct Version
        {
            public Dictionary<string, string> hashes = new();

            public Version()
            {
            }
            public Version(List<string> versionString)
            {
                foreach (var line in versionString)
                {
                    if (!line.Contains(" - "))
                        continue;

                    string[] splitedLine = line.Split(" - ");
                    hashes.Add(splitedLine[0], splitedLine[1]);
                }
            }

            //https://stackoverflow.com/questions/3804367/testing-for-equality-between-dictionaries-in-c-sharp
            public static bool CompareX<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
            {
                if (dict1 == dict2) return true;
                if ((dict1 == null) || (dict2 == null)) return false;
                if (dict1.Count != dict2.Count) return false;

                var valueComparer = EqualityComparer<TValue>.Default;

                foreach (var kvp in dict1)
                {
                    TValue value2;
                    if (!dict2.TryGetValue(kvp.Key, out value2)) return false;
                    if (!valueComparer.Equals(kvp.Value, value2)) return false;
                }
                return true;
            }

            public static bool Equal(Version first, Version second)
            {
                return CompareX(first.hashes, second.hashes);
            }
        }

        public Version vc { get; }
        private string vcFolder;

        public VersionControl(List<string> vc, string? vcFolder = null)
        {
            this.vc = new Version(vc);
            this.vcFolder = vcFolder;
        }
        
        public VersionControl(Version vc, string? vcFolder = null)
        {
            this.vc = vc;
            this.vcFolder = vcFolder;
        }

        string CalculateMD5OfFile(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = FilesManager.ReadFile(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static string CalculateMD5(string text)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public void CalculateVersionControl()
        {
            if(vcFolder == null)
            {
                throw new Exception("Calculating version of null directory!");
            }
            string rootDir = Directory.GetCurrentDirectory();
            string vcPath = Path.Combine(rootDir, vcFolder);

            if (!Directory.Exists(vcPath))
                throw new Exception("Game folder (" + vcPath + ") doesn't exist! Check config file.");

            foreach (string file in Directory.EnumerateFiles(vcPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(vcPath, file);
                vc.hashes.Remove(relativePath);
                vc.hashes.Add(relativePath, CalculateMD5OfFile(file));
            }
        }

        public List<string> GetFilePathsToDownload(VersionControl remoteVersionControl)
        {
            Version online = remoteVersionControl.vc;
            Version local = vc;

            List<string> toDownload = new();

            foreach (var (path, remoteHash) in online.hashes)
            {
                if (local.hashes.TryGetValue(path, out String? localHash))
                {
                    if (remoteHash != localHash)
                    {
                        toDownload.Add(path);
                    }
                }
                else
                    toDownload.Add(path);
            }

            return toDownload;
        }

        public List<string> GetFilePathsToRemove(VersionControl remoteVersionControl)
        {
            Version online = remoteVersionControl.vc;
            Version local = vc;

            List<string> toRemove = new();

            foreach (var (path, hash) in local.hashes)
            {
                if (!online.hashes.ContainsKey(path))
                    toRemove.Add(path);
            }
            return toRemove;
        }

        public void Clear()
        {
            vc.hashes.Clear();
        }

        public bool Empty()
        {
            return vc.hashes.Count == 0;
        }

        public void AddToVersionControl(string absoluteFilePath)
        {
            string rootDir = Directory.GetCurrentDirectory();
            string vcPath = Path.Combine(rootDir, vcFolder);

            if (!Directory.Exists(vcPath))
                throw new Exception("Game folder (" + vcPath + ") doesn't exist! Check config file.");

            string relativePath = Path.GetRelativePath(vcPath,   absoluteFilePath);

            vc.hashes.Remove(relativePath);
            vc.hashes.Add(relativePath, CalculateMD5OfFile(absoluteFilePath));
        }

        public void DeleteFromVersionControl(string localFilePath)
        {
            vc.hashes.Remove(localFilePath);
        }

        public static bool Equal(VersionControl localVC, VersionControl remoteVC)
        {
            var localVersion = localVC.vc;
            var remoteVersion = remoteVC.vc;
            return Version.Equal(localVersion, remoteVersion);
        }

        public void WriteToFile(string filePath)
        {
            List<string> result = new List<string>();

            foreach (KeyValuePair<string, string> el in vc.hashes)
                result.Add(el.Key + " - " + el.Value);

            File.WriteAllLines(filePath, result);
        }
    }
}
