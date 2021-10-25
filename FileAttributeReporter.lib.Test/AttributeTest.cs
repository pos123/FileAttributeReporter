using System;
using Xunit;

namespace FileAttributeReporter.lib.Test
{
    public class AttributeTest
    {
        private const string testPath = "TODO";

        [Fact]
        public void GivenFileName_CheckAttributesAreCorrect()
        {
            var fileAttributes = AttributeUtils.GetFileAttributes(testPath);
        }
    }
}
