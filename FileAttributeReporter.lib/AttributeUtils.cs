using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileAttributeReporter.lib.Types;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FileAttributeReporter.lib
{
    public static class AttributeUtils
    {
        private static MachineType GetDllMachineType(string dllPath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).
            //
            var fileStream = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fileStream);
            fileStream.Seek(0x3c, SeekOrigin.Begin);
            var peOffset = reader.ReadInt32();
            fileStream.Seek(peOffset, SeekOrigin.Begin);
            var peHead = reader.ReadUInt32();

            if (peHead != 0x00004550)
            {
                // "PE\0\0", little-endian
                throw new Exception("Can't find PE header");
            }   

            var machineType = (MachineType)reader.ReadUInt16();
            reader.Close();
            fileStream.Close();
            return machineType;
        }

        private static bool DllIs64Bit(string dllPath)
        {
            var machineType = GetDllMachineType(dllPath);
            return machineType == MachineType.IMAGE_FILE_MACHINE_AMD64 ||
                   machineType == MachineType.IMAGE_FILE_MACHINE_IA64;
        }

        private static bool DllIs32Bit(string dllPath)
        {
            var machineType = GetDllMachineType(dllPath);
            return machineType == MachineType.IMAGE_FILE_MACHINE_I386;
        }

        private static bool IsAssembly(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Try to read CLI metadata from the PE file.
            using var peReader = new PEReader(fs);

            if (!peReader.HasMetadata)
            {
                return false; // File does not have CLI metadata.
            }

            // Check that file has an assembly manifest.
            var reader = peReader.GetMetadataReader();
            return reader.IsAssembly;
        }

        public static BinaryArchitecture GetBinaryArchitecture(string fullBinaryPath)
        {
            if (DllIs64Bit(fullBinaryPath) && !IsAssembly(fullBinaryPath))
            {
                return BinaryArchitecture.Native64;
            }

            if (DllIs32Bit(fullBinaryPath) && !IsAssembly(fullBinaryPath))
            {
                return BinaryArchitecture.Native32Bit;
            }

            if (IsAssembly(fullBinaryPath))
            {
                return BinaryArchitecture.ManagedDotNet;
            }

            return BinaryArchitecture.Unknown;
        }

        public static FileData GetFileAttributes(string fullPath)
        {
            var file = new FileInfo(fullPath);

            var fileName = file.Name;
            var filePath = file.Directory?.FullName;
            var machineName = Environment.MachineName;
            var lastModified = File.GetLastWriteTime(fullPath);
            var architecture = GetBinaryArchitecture(fullPath);
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(fullPath);
            var fileData = new FileData(Name:fileName, Path: filePath, MachineName: machineName, LastModDateTime: lastModified, Architecture: architecture, FileVersion: fileVersionInfo.FileVersion);

            return fileData;
        }

        public static IEnumerable<string> GetFiles(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption).Where(file => reSearchPattern.IsMatch(Path.GetExtension(file)));
        }

        public static List<FileData> GetFilesInDirectoryAttributes(string fullPath, bool recursive)
        {
            var files = GetFiles(fullPath, @"\.exe|\.dll", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var allData = files.Select(GetFileAttributes).ToList();
            return allData;
        }

    }
}
