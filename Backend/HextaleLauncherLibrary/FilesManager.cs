using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTaleLauncherLibrary
{
    public class FileException : Exception
    {
        public enum Type { DiskOutOfSpace, FileRequiresElevation, FileNotFound, FileUsedByAnotherProcess };
        public required Type type { get; init; }
        public required string filePath { get; init; }

        public FileException() : base("")
        {}
        public FileException(Exception inner) : base("", inner)
        {}

        public override string Message => $"File exception: type: {type.ToString()}, filePath: {filePath}";
    }

    public static class FilesManager
    {

        public static void SaveToFile(string filePath, MemoryStream ms)
        {
            try
            {
                ms.Position = 0;

                using (FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.Write))
                {
                    ms.CopyTo(fs);
                    fs.Flush();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FileException(ex) { type = FileException.Type.FileRequiresElevation, filePath=filePath };
            }
            catch (IOException ex)
            {
                if (IsDiskFull(ex))
                    throw new FileException(ex) { type = FileException.Type.DiskOutOfSpace, filePath = filePath };
                else if (IsUsedByAnotherProcess(ex))
                    throw new FileException(ex) { type = FileException.Type.FileUsedByAnotherProcess, filePath = filePath };
                else
                    throw;
            }
        }

        public static MemoryStream ReadFile(string filePath)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    MemoryStream ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FileException(ex) { type = FileException.Type.FileRequiresElevation, filePath = filePath };
            }
            catch (FileNotFoundException ex)
            {
                throw new FileException(ex) { type = FileException.Type.FileNotFound, filePath = filePath };
            }
            catch (IOException ex)
            {
                if (IsDiskFull(ex))
                    throw new FileException(ex) { type = FileException.Type.DiskOutOfSpace, filePath = filePath };
                else if(IsUsedByAnotherProcess(ex))
                    throw new FileException(ex) { type = FileException.Type.FileUsedByAnotherProcess, filePath = filePath };
                else
                    throw;
            }
        }

        static bool IsDiskFull(Exception ex)
        {
            const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
            const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

            return ex.HResult == HR_ERROR_HANDLE_DISK_FULL
                || ex.HResult == HR_ERROR_DISK_FULL;
        }
        
        static bool IsUsedByAnotherProcess(Exception ex)
        {
            int errorCode = (int)(ex.HResult & 0x0000FFFF);

            const int ERROR_SHARING_VIOLATION = 32;
            const int ERROR_LOCK_VIOLATION = 33;

            return errorCode == ERROR_SHARING_VIOLATION
                || errorCode == ERROR_LOCK_VIOLATION;
        }
    }
}
