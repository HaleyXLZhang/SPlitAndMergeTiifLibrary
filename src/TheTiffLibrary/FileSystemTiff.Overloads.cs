using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FrameworkExtensions;

namespace TiffImage
{
    public partial class FileSystemTiff : AbstractTiff
    {  
        #region Merge Overloads
        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Append( params string[] images )
        {
            this.Append( images.Select( image => image ) );
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Append( IEnumerable<string> images )
        {
            var imgs = images.Select( str => new FileSystemTiff( str ) );

            this.Append( imgs.Select( img => img.Image ) );
            this.Save();

            imgs.Foreach( img => img.Dispose() );
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Append( params FileSystemTiff[] images )
        {
            this.Append( images.Select( image => image.Image ) );
            this.Save();
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Append( IEnumerable<FileSystemTiff> images )
        {
            this.Append( images.Select( image => image.Image ) );
            this.Save();
        }

        public static FileSystemTiff Merge( TiffEncodingFormat format, string outFile, IEnumerable<FileSystemTiff> images )
        {
            return Merge( format, outFile, images.Select( image => image.Image ) );
        }

        public static FileSystemTiff Merge( TiffEncodingFormat format, string outFile, IEnumerable<string> images )
        {
            //we receive path to images: we have to load, do our task and dispose them

            var frames = images.Select( image => new FileSystemTiff( image ) );
            var newImg = Merge( format, outFile, frames.Select( frame => frame.image ) );

            frames.Foreach( frame => frame.Dispose() );
            return newImg;
        }

        public static FileSystemTiff Merge( TiffEncodingFormat format, string outFile, params FileSystemTiff[] images )
        {
            return Merge( format, outFile, images.Select( image => image.Image ) );
        }

        public static FileSystemTiff Merge( TiffEncodingFormat format, string outFile, params string[] images )
        {
            //we receive path to images: we have to load, do our task and dispose them

            var frames = images.Select( image => new FileSystemTiff( image ) );
            var newImg = Merge( format, outFile, frames.Select( frame => frame.Image ) );

            frames.Foreach( frame => frame.Dispose() );
            return newImg;
        }
        #endregion

        #region SplitToFile Overloads
        public IEnumerable<FileSystemTiff> SplitToFile( string outputFile, NewFilesNamingOptions namingOptions )
        {
            string directory = Path.GetDirectoryName( outputFile );
            string fileName = Path.GetFileName( outputFile );

            return this.SplitToFile( directory, fileName, namingOptions );
        }
        #endregion
    }
}
