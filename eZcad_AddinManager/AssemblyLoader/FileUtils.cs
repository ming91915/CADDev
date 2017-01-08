// Decompiled with JetBrains decompiler
// Type: AddInManager.FileUtils
// Assembly: AddInManager, Version=2015.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5BDD7A83-FA69-4D91-8F2B-9F16E915F05A
// Assembly location: D:\Setups\Add-In Manager - Revit 2016\Add-In Manager\AddInManager.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace eZcad.AddinManager
{
    /// <summary> Revit AddinManager 中用来进行文件处理的类，用来参考，并不具有通用性 </summary>
    public static class FileUtils
    {
        private const string TempFolderName = "eZOfficeAddins";

        public static DateTime GetModifyTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        public static string CreateTempFolder(string prefix)
        {
            // 临时文件夹，用以复制临时程序集
            DirectoryInfo directoryInfo1 = new DirectoryInfo(Path.Combine(Path.GetTempPath(), TempFolderName));

            if (!directoryInfo1.Exists)
                directoryInfo1.Create();
            foreach (DirectoryInfo directoryInfo2 in directoryInfo1.GetDirectories())
            {
                try
                {
                    Directory.Delete(directoryInfo2.FullName, true);
                }
                catch (Exception ex)
                {
                }
            }
            string str = string.Format("{0:yyyyMMdd_HHmmss_ffff}", (object)DateTime.Now);
            DirectoryInfo directoryInfo3 = new DirectoryInfo(Path.Combine(directoryInfo1.FullName, prefix + str));
            directoryInfo3.Create();
            return directoryInfo3.FullName;
        }

        public static void SetWriteable(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, fileAttributes);
        }

        public static bool SameFile(string file1, string file2)
        {
            return 0 == string.Compare(file1.Trim(), file2.Trim(), true);
        }

        public static bool CreateFile(string filePath)
        {
            if (File.Exists(filePath))
                return true;
            try
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (new FileInfo(filePath).Create())
                    FileUtils.SetWriteable(filePath);
            }
            catch (Exception ex)
            {
                return false;
            }
            return File.Exists(filePath);
        }

        public static void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, fileAttributes);
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
            }
        }

        public static bool FileExistsInFolder(string filePath, string destFolder)
        {
            return File.Exists(Path.Combine(destFolder, Path.GetFileName(filePath)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destFolder"></param>
        /// <param name="onlyCopyRelated"></param>
        /// <param name="allCopiedFiles">方法结束后，此 List中包含有所有复制到的文件 </param>
        /// <returns></returns>
        public static string CopyFileToFolder(string sourceFilePath, string destFolder, bool onlyCopyRelated, List<FileInfo> allCopiedFiles)
        {
            if (!File.Exists(sourceFilePath))
                return (string)null;
            string directoryName = Path.GetDirectoryName(sourceFilePath);
            if (onlyCopyRelated)
            {
                string searchPattern = Path.GetFileNameWithoutExtension(sourceFilePath) + ".*";
                foreach (string str1 in Directory.GetFiles(directoryName, searchPattern, SearchOption.TopDirectoryOnly))
                {
                    string fileName = Path.GetFileName(str1);
                    string str2 = Path.Combine(destFolder, fileName);
                    if (FileUtils.CopyFile(str1, str2))
                    {
                        FileInfo fileInfo = new FileInfo(str2);
                        allCopiedFiles.Add(fileInfo);
                    }
                }
            }
            else
            {
                long folderSize = FileUtils.GetFolderSize(directoryName);
                if (folderSize > 50L)
                {
                    switch (FolderTooBigDialog.Show(directoryName, folderSize))
                    {
                        case DialogResult.Yes:
                            FileUtils.CopyDirectory(directoryName, destFolder, allCopiedFiles);
                            break;
                        case DialogResult.No:
                            FileUtils.CopyFileToFolder(sourceFilePath, destFolder, true, allCopiedFiles);
                            break;
                        default:
                            return (string)null;
                    }
                }
                else
                    FileUtils.CopyDirectory(directoryName, destFolder, allCopiedFiles);
            }
            string path = Path.Combine(destFolder, Path.GetFileName(sourceFilePath));
            if (File.Exists(path))
                return path;
            return (string)null;
        }

        public static bool CopyFile(string sourceFilename, string destinationFilename)
        {
            if (!File.Exists(sourceFilename))
                return false;
            FileAttributes fileAttributes1 = File.GetAttributes(sourceFilename) & ~FileAttributes.ReadOnly;
            File.SetAttributes(sourceFilename, fileAttributes1);
            if (File.Exists(destinationFilename))
            {
                FileAttributes fileAttributes2 = File.GetAttributes(destinationFilename) & ~FileAttributes.ReadOnly;
                File.SetAttributes(destinationFilename, fileAttributes2);
                File.Delete(destinationFilename);
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(destinationFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFilename));
                File.Copy(sourceFilename, destinationFilename, true);
            }
            catch (Exception ex)
            {
                return false;
            }
            return File.Exists(destinationFilename);
        }

        public static void CopyDirectory(string sourceDir, string desDir, List<FileInfo> allCopiedFiles)
        {
            try
            {
                foreach (string str1 in Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string str2 = str1.Replace(sourceDir, "");
                    string path = desDir + str2;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                foreach (string sourceFilename in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                {
                    string str1 = sourceFilename.Replace(sourceDir, "");
                    string str2 = desDir + str1;
                    if (!Directory.Exists(Path.GetDirectoryName(str2)))
                        Directory.CreateDirectory(Path.GetDirectoryName(str2));
                    if (FileUtils.CopyFile(sourceFilename, str2))
                        allCopiedFiles.Add(new FileInfo(str2));
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary> 返回文件夹内所有文件的总大小，单位为 MB  </summary>
        /// <param name="folderPath"></param>
        /// <returns> 文件夹内所有文件的总大小，单位为 MB </returns>
        public static long GetFolderSize(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            long num = 0L;
            foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                if (fileSystemInfo is FileInfo)
                    num += ((FileInfo)fileSystemInfo).Length;
                else
                    num += FileUtils.GetFolderSize(fileSystemInfo.FullName);
            }
            return num / 1024L / 1024L;
        }

        private class FolderTooBigDialog
        {
            public static DialogResult Show(string folderPath, long sizeInMB)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Folder [" + folderPath + "]");
                stringBuilder.AppendLine("is " + sizeInMB + "MB large.");
                stringBuilder.AppendLine("AddinManager will attempt to copy all the files to temp folder");
                stringBuilder.AppendLine("Select [Yes] to copy all the files to temp folder");
                stringBuilder.AppendLine("Select [No] to only copy test script DLL");
                string text = stringBuilder.ToString();
                return MessageBox.Show(text, "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
            }
        }
    }

}
