using System;
using Xunit;

namespace FileAttributeReporter.lib.Test
{
    public class BinaryArchitectureTest
    {
        private const string testPath = "TODO";

        [Fact]
        public void GivenBinaries_CheckArchitectureIsCorrect()
        {
            var result = AttributeUtils.GetBinaryArchitecture(testPath);
          
        }

    }
}
