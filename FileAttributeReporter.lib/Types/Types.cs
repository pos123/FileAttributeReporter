namespace FileAttributeReporter.lib.Types;

public record Parameters(string Path, SearchMode ParameterSearchMode, ResultType ParameterResultType, string OutDetails, Action<string> Progress);
public record FileData(string Name, string Path, string MachineName, DateTime LastModDateTime, BinaryArchitecture Architecture, string FileVersion);
public record ReportValidation(bool Success, string Message);
public record ReportResult(ReportValidation ReportValidation, List<FileData> AllFileData);

