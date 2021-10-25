using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using FileAttributeReporter.lib.Types;

namespace FileAttributeReporter.lib.OutputReporters
{
    public class ExcelReporter
    {
        public ReportValidation Report(ReportResult result, string outputPath)
        {
            var startRow = 2;
            var startCol = 2;
            var now = DateTime.Now;

            // Create workbook
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("File Attributes");

            // Generate title and formats
            OutputTitle(worksheet, startRow, startCol);
            SetColumnFormats(worksheet, startCol);

            // Generate results
            var i = 1;
            foreach (var fileData in result.AllFileData)
            {
                OutputRow(worksheet, startRow + i, startCol, fileData, now);
                ++i;
            }

            // Style into table and size to fit
            var range = worksheet.Range(startRow, startCol, startRow + i - 1, startCol + 6);
            var table = range.CreateTable();
            table.Theme = XLTableTheme.TableStyleLight9;
            worksheet.Columns().AdjustToContents();  // Adjust column width
            worksheet.Rows().AdjustToContents();

            // Output file
            var directory = string.IsNullOrEmpty(outputPath) ? Directory.GetCurrentDirectory() : outputPath;
            var outputFile = Path.Combine(directory, $"file_attributes_output_{now:dd_MM_yyyy_HH_mm_ss}.xlsx");
            workbook.SaveAs(outputFile);

            return new ReportValidation(Success: true, Reason: $"File generated at: {outputFile}");
        }

        private void OutputTitle(IXLWorksheet workSheet, int row, int col)
        {
            workSheet.Cell(row, col++).Value = "ReportGenerationTime";
            workSheet.Cell(row, col++).Value = "MachineName";
            workSheet.Cell(row, col++).Value = "ParentDirectory";
            workSheet.Cell(row, col++).Value = "BinaryName";
            workSheet.Cell(row, col++).Value = "BinaryArchitecture";
            workSheet.Cell(row, col++).Value = "FileVersion";
            workSheet.Cell(row, col++).Value = "LastModDateTime";
        }

        private void SetColumnFormats(IXLWorksheet workSheet, int col)
        {
            workSheet.Column(col++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            workSheet.Column(col).CellsUsed().SetDataType(XLDataType.Text);
            workSheet.Column(col++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            workSheet.Column(col++).CellsUsed().SetDataType(XLDataType.Text);

            workSheet.Column(col).CellsUsed().SetDataType(XLDataType.Text);
            workSheet.Column(col++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            workSheet.Column(col).CellsUsed().SetDataType(XLDataType.Text);
            workSheet.Column(col++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            workSheet.Column(col).CellsUsed().SetDataType(XLDataType.Text);
            workSheet.Column(col++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            workSheet.Column(col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private void OutputRow(IXLWorksheet workSheet, int row, int col, FileData fileData, DateTime now)
        {
            // ReportGenerationTime
            workSheet.Cell(row, col).Value = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            workSheet.Cell(row, col++).SetDataType(XLDataType.DateTime);

            // MachineName
            workSheet.Cell(row, col++).Value = fileData.MachineName;

            // ParentDirectory
            workSheet.Cell(row, col++).Value = fileData.Path;

            // BinaryName
            workSheet.Cell(row, col++).Value = fileData.Name;

            // BinaryArchitecture
            workSheet.Cell(row, col++).Value = fileData.Architecture.ToString();
            
            // FileVersion
            workSheet.Cell(row, col++).Value = fileData.FileVersion;

            // LastModDateTime
            workSheet.Cell(row, col).Value = fileData.LastModDateTime;
            workSheet.Cell(row, col).SetDataType(XLDataType.DateTime);
        }
    }
}
