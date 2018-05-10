using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FrameworkExtensions;
using System.Linq;

namespace TiffImage
{
    /// <summary>
    /// Represents an image stored on file system.
    /// </summary>
    public partial class FileSystemTiff : AbstractTiff, IDisposable
    {
        #region Properties & Variables
        public FileInfo FileInfo { get; private set; }

        private FileStream fileStream = null;
        private FileStream FileStream
        {
            get
            {
                return fileStream = fileStream ?? new FileStream( this.FileInfo.FullName, FileMode.Open, FileAccess.Read );
            }
        }

        public override Image Image
        {
            get
            {
                return base.image = image ?? Image.FromStream( this.FileStream, true, false );
            }

            protected set { image = value; }
        }
        #endregion

        public FileSystemTiff( string file )
            : base()
        {
            this.FileInfo = new FileInfo( file );
        }

        /// <summary>
        /// Split a multi-page TIFF file into multiple single page TIFF files. 
        /// Each image is saved to disk applying default naming options:
        /// [initial number: 1 padded to 3 chars, counter prefix: '_PAGE']
        /// </summary>
        /// <param name="targetDirectory">The directory where images are stored</param>
        /// <returns>A collection of lazy loading in-memory tiff images</returns>
        public IEnumerable<FileSystemTiff> SplitToFile( string targetDirectory )
        {
            var namingOptions = new NewFilesNamingOptions()
            {
                InitialCounterNumber = 1,
                NumberMinimunLength = 3,
                CounterPrefix = "_PAGE",
                CounterSuffix = ""
            };

            return this.SplitToFile( targetDirectory, this.FileInfo.Name, namingOptions );
        }

        public FileSystemTiff AppendToFile( string outFile, params string[] images )
        {
            if( outFile == this.FileInfo.FullName )
            {
                this.Append( images );
                return this;
            }
            else
            {
                using( var fileStream = new FileStream( outFile, FileMode.Create ) )
                {
                    var memImgs = images.Select( image => new FileSystemTiff( image ) );
                    this.Append( memImgs.Select( image => image.Image ), fileStream );

                    memImgs.Foreach( image => image.Dispose() );
                }

                return new FileSystemTiff( outFile );
            }
        }

        public static int GetPageCount( string file )
        {
            using( var img = new FileSystemTiff( file ) )
            {
                return img.Image.GetFrameCount( FrameDimension.Page );
            }
        }

        new public void RemovePages( params int[] pageIndexes )
        {
            base.RemovePages( pageIndexes );
            this.Save();
        }

        /// <summary>
        /// Saves changes made to this image permanently.
        /// </summary>
        public void Save()
        {
            Directory.CreateDirectory( this.FileInfo.Directory.FullName );

            if( this.image != null )
                this.image.Dispose();

            if( this.fileStream != null )
                this.fileStream.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();

            if( this.MemoryStream != null )
            {
                File.WriteAllBytes( this.FileInfo.FullName, this.MemoryStream.GetBuffer() );
                this.MemoryStream.Dispose();
            }

            this.Image = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
        }

        public override void Dispose()
        {
            // Call dispose on variables, not on properties here! 
            // If property is null, a call will reload resources just do dispose them.

            if( this.fileStream != null )
                this.fileStream.Dispose();

            base.Dispose();
            GC.SuppressFinalize( this );
        }
    }
}
