using System;
using System.Collections.Generic;
using System.Diagnostics;
using CommandLine;
using FileAttributeReporter.lib;
using FileAttributeReporter.lib.Types;

namespace Report.Console
{
    class Program
    {
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

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
        }

        static void RunOptions(Options opts)
        {
            var searchMode = SearchMode.Unknown;
            if (opts.Mode == "file")
            {
                searchMode = SearchMode.File;
            }
            else if (opts.Mode == "directory")
            {
                searchMode = SearchMode.Directory;
            }
            else if (opts.Mode == "directoryAll")
            {
                searchMode = SearchMode.DirectoryRecurse;
            }
            
            var parameters = new Parameters(Path: opts.InputPath, ParameterSearchMode: searchMode,
                ParameterResultType: ResultType.ExcelFile, opts.OutputPath, (progress) =>
                {
                    OutputMessage(progress, opts.SilentMode);
                });

            var validationResult = Reporter.Validate(parameters);
            if (!validationResult.Success)
            {
                OutputMessage($"Failed to parse: {validationResult.Message}", opts.SilentMode);
            }

            var reportResult = Reporter.RunReport(parameters);
            if (!reportResult.ReportValidation.Success)
            {
                OutputMessage($"Failed to generate report: {validationResult.Message}", opts.SilentMode);
            }
            else
            {
                var result = Reporter.OutputResults(reportResult, parameters);
                OutputMessage(!result.Success ? $"Failed to output results report: {result.Message}" : $"Successfully output results report at: {result.Message}", opts.SilentMode);
                if (result.Success)
                {
                    DisplayFileInExplorer(result.Message);
                }
            }
            
            OutputMessage("Press any key to exit", opts.SilentMode);

            if (!opts.SilentMode)
            {
                System.Console.ReadKey();
            }
        }

        static void DisplayFileInExplorer(string fullFilePath)
        {
            string args = string.Format("/e, /select, \"{0}\"", fullFilePath);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "explorer";
            info.Arguments = args;
            Process.Start(info);
        }

        static void OutputMessage(string message, bool silentMode)
        {
            if (!silentMode)
            {
                System.Console.WriteLine(message);
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
            {
                System.Console.WriteLine(err.ToString());
            }

            System.Console.ReadKey();
        }
    }
}
