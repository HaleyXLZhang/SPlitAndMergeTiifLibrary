using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TiffImage.Tests
{
    [TestClass()]
    public class MergeTifToolTests
    {
        [TestMethod()]
        public void SplitFilesToMemoryTest()
        {
            var st = DateTime.Now;
            string documentTempPath = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug", "TESTFolder");

            List<InMemoryTiff> tifList = MergeTifTool.SplitFilesToMemory(documentTempPath);

            Dictionary<int, List<InMemoryTiff>> temp = MergeTifTool.MergeTiffFromMemories(tifList);

            var ed = DateTime.Now - st;

            MergeTifTool.SaveTiffs(temp, documentTempPath + @"\MergeResult\");
        }
    }
}