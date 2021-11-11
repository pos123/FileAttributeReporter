namespace Report.Console;

public class Options
{
    [Option('m', "search mode -can be \"file\", \"directory\" or \"directoryAll\"", Required = true, HelpText = "choose to report on single file, directory or directory recursively")]
    public string Mode { get; set; }

    [Option('i', "the full path to the directory or file for the search", Required = true, HelpText = "This is the full path of the directory or file to search")]
    public string InputPath { get; set; }

    [Option('o', "the full path to output directory for the report", Required = false, HelpText = "This is the full path of the directory where the report will be output - path must exist if you use it")]
    public string OutputPath { get; set; }

    [Option('s', "silent mode", Required = false, HelpText = "use this if you want to run this in silent mode")]
    public bool SilentMode { get; set; }
}

