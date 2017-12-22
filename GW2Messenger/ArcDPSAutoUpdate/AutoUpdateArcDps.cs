using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace ArcDPSAutoUpdate
{
    public class AutoUpdateArcDps
    {

        #region file statuse
        public enum FileStatus
        {
            None = 0,
            UpToDate,
            OutDated,
            Missing
        }

        public static FileStatus IsLocalArcDpsUpToDate(string gw2Directory)
        {
            // check if local file exists
            if (!File.Exists(gw2Directory + @"\d3d9.dll"))
                return FileStatus.Missing;

            // get remote md5
            string remoteMd5;
            var webRequest = WebRequest.Create(@"https://www.deltaconnected.com/arcdps/x64/d3d9.dll.md5sum");
            try
            {
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    if (content is null)
                        return FileStatus.None;

                    using (var reader = new StreamReader(content))
                    {
                        // only has 1 line to read
                        var s = reader.ReadLine();
                        if (s is null) return FileStatus.None;

                        remoteMd5 = s.Split(' ')[0];
                   }
                }
            }
            catch
            {
                return FileStatus.None;
            }

            // get local md5
            var localMd5 = ComputeMd5(gw2Directory + @"\d3d9.dll");

            return localMd5 == remoteMd5 ? FileStatus.UpToDate : FileStatus.OutDated;
        }

        public static FileStatus IsLocalBuildTempUpToDate(string gw2Directory)
        {
            // check if local file exists
            if (!File.Exists(gw2Directory + @"\d3d9_arcdps_buildtemplates.dll"))
                return FileStatus.Missing;

            // get remote md5
            var remoteFile = Path.GetTempPath() + @"\d3d9_arcdps_buildtemplates.dll";
                
            if (!DownloadFile(@"https://www.deltaconnected.com/arcdps/x64/buildtemplates/d3d9_arcdps_buildtemplates.dll", remoteFile))
                return FileStatus.None;
            var remoteMd5 = ComputeMd5(remoteFile);

            // get local md5
            var localMd5 = ComputeMd5(gw2Directory + @"\d3d9_arcdps_buildtemplates.dll");

            return localMd5 == remoteMd5 ? FileStatus.UpToDate: FileStatus.OutDated;
        }
        #endregion

        #region webdownloads
        public static Exception DownloadArcDps(string gw2Directory)
        {
            return DownloadFileWithBackup(
                @"https://www.deltaconnected.com/arcdps/x64/d3d9.dll",
                gw2Directory + @"\d3d9.dll");
        }
        
        public static Exception DownloadBuildTemp(string gw2Directory)
        {
            return DownloadFileWithBackup(
                @"https://www.deltaconnected.com/arcdps/x64/buildtemplates/d3d9_arcdps_buildtemplates.dll",
                gw2Directory + @"\d3d9_arcdps_buildtemplates.dll");
        }

        private static Exception DownloadFileWithBackup(string url, string filename)
        {
            var backupFile = filename + ".bak";

            // rename existing file
            if (File.Exists(filename))
                try
                {
                    File.Copy(filename, backupFile);
                }
                catch (Exception e)
                {
                    return e;
                }

            // if download fails, revert renaming
            if (!DownloadFile(url, filename))
            {
                File.Copy(backupFile, filename);
                File.Delete(backupFile);
                return new WebException(
                    "Failed to download file. The remote server hosting the file does not respond.");
            }

            // if success, delete backup file.
            File.Delete(backupFile);
            return null;
        }

        private static bool DownloadFile(string url, string filename)
        {
            var webRequest = WebRequest.Create(url);
            using (var response = webRequest.GetResponse())
            {
                var remoteStream = response.GetResponseStream();
                if (remoteStream == null)
                    return false;

                // Create the local file
                using (var localStream = File.Create(filename))
                {
                    // Allocate a 1k buffer
                    var buffer = new byte[1024];
                    int bytesRead;

                    // Read and store data
                    do
                    {
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                        localStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead > 0);
                }
            }

            return true;
        }
        #endregion

        #region miscellaneous
        private static string ComputeMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var s = md5.ComputeHash(stream);
                    return string.Join("", s.Select(x=> x.ToString("X"))).ToLower();
                }
            }
        }

        public static bool IsGw2Running()
        {
            if (Process.GetProcessesByName("Gw2-64.exe").Length > 0)
                return true;
            return Process.GetProcessesByName("Gw2.exe").Length > 0;
        }
        #endregion
    }
}
