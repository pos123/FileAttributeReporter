using System;
using System.Collections.Generic;
using System.IO;
using FileAttributeReporter.lib.OutputReporters;
using FileAttributeReporter.lib.Types;
using static System.String;

namespace FileAttributeReporter.lib
{
    public static class Reporter
    {
        public static ReportResult RunReport(Parameters parameters)
        {
            try
            {
                return parameters.ParameterSearchMode == SearchMode.File
                    ? new ReportResult(ReportValidation: new(true, Empty), AllFileData: new List<FileData> { AttributeUtils.GetFileAttributes(parameters.Path) })
                    : new ReportResult(ReportValidation: new(true, Empty), AllFileData: AttributeUtils.GetFilesInDirectoryAttributes(parameters.Path, parameters.ParameterSearchMode == SearchMode.DirectoryRecurse));
            }
            catch (Exception e)
            {
                return new ReportResult(new ReportValidation(false, $"{e.Message}"), null);
            }
        }

        public static ReportValidation OutputResults(ReportResult result, Parameters parameters)
        {
            try
            {
                return new ExcelReporter().Report(result, parameters.OutDetails);
            }
            catch (Exception e)
            {
                return new ReportValidation(false, $"failed to generate report: ${e.Message}");
            }
            
        }

        public static ReportValidation Validate(Parameters parameters)
        {
            if (parameters.ParameterSearchMode == SearchMode.Unknown)
            {
                return new ReportValidation(false, "invalid search mode");
            }
            
            if (IsNullOrEmpty(parameters.Path))
            {
                return new ReportValidation(false, "path to search is not specified");
            }

            if (parameters.ParameterSearchMode == SearchMode.File && !File.Exists(parameters.Path))
            {
                return new ReportValidation(false, "path to search does not exist");
            }

            if ((parameters.ParameterSearchMode == SearchMode.Directory || parameters.ParameterSearchMode == SearchMode.DirectoryRecurse) && !Directory.Exists(parameters.Path))
            {
                return new ReportValidation(false, "path is not a directory but directory search requested");
            }

            if ((parameters.ParameterResultType == ResultType.CsvFile || parameters.ParameterResultType == ResultType.ExcelFile) && !IsNullOrEmpty(parameters.OutDetails))
            {
                if (!Directory.Exists(parameters.OutDetails))
                {
                    return new ReportValidation(false, "the directory specified for the output does not exist");
                }
            }

            return new ReportValidation(true, Empty);
        }

    }
}
