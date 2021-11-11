namespace FileAttributeReporter.lib.Test;

public class ReportTests
{
    private const string testPath = "TODO";

    [Fact]
    public void GivenReport_ShouldGenerateExcelFile()
    {
        var parameters = new Parameters(
            Path: testPath,
            ParameterSearchMode: SearchMode.File, ParameterResultType: ResultType.ExcelFile,
            string.Empty, null);

        var validationResult = Reporter.Validate(parameters);
        Assert.True(validationResult.Success);

        var reportResult = Reporter.RunReport(parameters);
        Reporter.OutputResults(reportResult, parameters);
    }

}

