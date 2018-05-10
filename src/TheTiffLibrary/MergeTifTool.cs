using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TiffImage
{
    public class MergeTifTool
    {
        private const int LIMITED_FILE_SIZE_MB = 10;
        public static void MergeFilesInFolder(string folder, string outPut)
        {
            List<InMemoryTiff> tifList = SplitFilesToMemory(folder);
            Dictionary<int, List<InMemoryTiff>> temp = MergeTiffFromMemories(tifList);
            SaveTiffs(temp, outPut);
        }

        public static List<InMemoryTiff> SplitFilesToMemory(string folder)
        {
            DirectoryInfo directory = new DirectoryInfo(folder);
            List<string> filePaths = new List<string>();
            List<FileInfo> tifFiles = directory.GetFiles("*.tif", SearchOption.AllDirectories).OrderByDescending(o => o.LastWriteTimeUtc).ToList();

            #region make the remark tif file is first file
            FileInfo remarkFile = directory.GetFiles("*.tif").FirstOrDefault();

            int tifFileIndex = tifFiles.FindIndex(0, tifFiles.Count, A => { return A.FullName.Equals(remarkFile.FullName); });

            tifFiles.RemoveAt(tifFileIndex);

            List<FileInfo> foreachTifFiles = new List<FileInfo>();
            foreachTifFiles.Add(remarkFile);
            foreachTifFiles.AddRange(tifFiles);
            #endregion
            foreach (var temp in foreachTifFiles)
            {
                filePaths.Add(temp.FullName);
            }
            return SplitFilesToMemory(filePaths);
        }

        public static List<InMemoryTiff> SplitFilesToMemory(List<string> filePaths)
        {
            Func<string, IEnumerable<InMemoryTiff>> splitToMemories = (filePath) =>
            {
                FileSystemTiff tiffFiles = new FileSystemTiff(filePath);
                return tiffFiles.Split();
            };

            var resultDic = new Dictionary<int, List<InMemoryTiff>>();

            var fileTaskArray = new System.Threading.Tasks.Task[filePaths.Count];


            for (int i = 0; i < filePaths.Count; i++)
            {
                var filePath = filePaths[i];
                var index = i;
                fileTaskArray[i] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    resultDic.Add(index, splitToMemories(filePath).ToList());
                });

            }

            System.Threading.Tasks.Task.WaitAll(fileTaskArray);

            var result = new List<InMemoryTiff>();

            for (int i = 0; i < resultDic.Keys.Count; i++)
            {
                result.AddRange(resultDic[i]);
            }

            return result;
        }

        public static Dictionary<int, List<InMemoryTiff>> MergeTiffFromMemories(IEnumerable<InMemoryTiff> memories)
        {
            long tempFileSize = 0;

            var splitDic = new Dictionary<int, List<InMemoryTiff>>();
            int splitIndex = 0;
            splitDic.Add(splitIndex, new List<InMemoryTiff>());

            foreach (var tempMemory in memories)
            {
                long imgLegnth;
                using (var stream = new MemoryStream())
                {
                    tempMemory.Image.Save(stream, ImageFormat.Tiff);
                    imgLegnth = stream.Length;
                }

                if (IsOverload(tempFileSize + imgLegnth))
                {
                    splitIndex = splitIndex + 1;
                    splitDic.Add(splitIndex, new List<InMemoryTiff>() { tempMemory });
                    tempFileSize = imgLegnth;
                }
                else
                {
                    tempFileSize = tempFileSize + imgLegnth;
                    splitDic[splitIndex].Add(tempMemory);
                }
            }
            return splitDic;
        }


        public static void SaveTiffs(Dictionary<int, List<InMemoryTiff>> splitDic, string folder)
        {
            foreach (var key in splitDic.Keys)
            {
                string filePath = string.Format("{0}_{1}.tif", DateTime.Now.ToString("yyyyMMddHHmmss"), key);
                TiffEncodingFormat tiffEncodingFormat = new TiffEncodingFormat(splitDic[key].Count);
                InMemoryTiff tempInMemoryTiff = InMemoryTiff.Merge(tiffEncodingFormat, splitDic[key].ToArray());
                tempInMemoryTiff.SaveTo(folder + filePath);
                tempInMemoryTiff.SaveTo(TiffEncodingFormats.Tiff1Bpp, folder + filePath);
                tempInMemoryTiff.Dispose();
            }
        }

        private static bool IsOverload(long byteLength)
        {
            long limitedLength = LIMITED_FILE_SIZE_MB * 1024 * 1024;
            return limitedLength < byteLength;
        }
    }

}