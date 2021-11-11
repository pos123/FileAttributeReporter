namespace FileAttributeReporter.lib;

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

    private static BinaryArchitecture GetAssemblyBinaryArchitecture(string assemblyFile)
    {
        var assembly = System.Reflection.AssemblyName.GetAssemblyName(assemblyFile);
        if (assembly.ProcessorArchitecture == System.Reflection.ProcessorArchitecture.Amd64)
        {
            return BinaryArchitecture.ManagedDotNet64;
        }
        else if (assembly.ProcessorArchitecture == System.Reflection.ProcessorArchitecture.X86)
        {
            return BinaryArchitecture.ManagedDotNet32;
        }
        return BinaryArchitecture.ManagedDotNet;
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
            return GetAssemblyBinaryArchitecture(fullBinaryPath);
        }

        return BinaryArchitecture.Unknown;
    }

    public static FileData GetFileAttributes(string fullPath, Action<string> progress)
    {
        var file = new FileInfo(fullPath);

        try
        {
            var fileName = file.Name;
            var filePath = file.Directory?.FullName;
            var machineName = Environment.MachineName;
            var lastModified = File.GetLastWriteTime(fullPath);
            var architecture = GetBinaryArchitecture(fullPath);
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(fullPath);
            var fileData = new FileData(Name: fileName, Path: filePath, MachineName: machineName, LastModDateTime: lastModified, Architecture: architecture, FileVersion: fileVersionInfo.FileVersion);
            return fileData;
        }
        catch (Exception ex)
        {
            progress?.Invoke($"Failed to get data for {file.Name} : {ex.Message}");
            throw;
        }
    }

    public static IEnumerable<string> GetFiles(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly, Action<string> progress = null)
    {
        progress?.Invoke($"Searching for file(s) at {path} ...");
        var reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
        return Directory.EnumerateFiles(path, "*", searchOption).Where(file => reSearchPattern.IsMatch(Path.GetExtension(file)));
    }

    public static List<FileData> GetFilesInDirectoryAttributes(string fullPath, bool recursive, Action<string> reporter)
    {
        var files = GetFiles(fullPath, @"\.exe|\.dll", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, reporter);
        var allData = files.Select(x =>
        {
            reporter?.Invoke($"Getting file information for : {x}");
            return GetFileAttributes(x, reporter);
        }).ToList();

        return allData;
    }

}

