using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace TiffImage
{
    /// <summary>
    /// Overload facilities
    /// </summary> 
    public partial class InMemoryTiff : AbstractTiff
    {
        #region Merge Overloads

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Merge( IEnumerable<Stream> imageStreams )
        {
            this.Append( imageStreams.Select( stream => new InMemoryTiff( stream ).Image ) );
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Merge( params Stream[] imageStreams )
        {
            this.Append( imageStreams.Select( stream => new InMemoryTiff( stream ).Image ) );
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Merge( params InMemoryTiff[] images )
        {
            this.Append( images.Select( tiff => tiff.Image ) );
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Merge( IEnumerable<InMemoryTiff> images )
        {
            this.Append( images.Select( image => image.Image ) );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static InMemoryTiff Merge( TiffEncodingFormat format, IEnumerable<InMemoryTiff> images )
        {
            return Merge( format, images.Select( tif => tif.Image ) );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static InMemoryTiff Merge( TiffEncodingFormat format, params InMemoryTiff[] tiffs )
        {
            return Merge( format, tiffs.Select( tiff => tiff.Image ) );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static InMemoryTiff Merge( TiffEncodingFormat format, IEnumerable<Stream> imageStreams )
        {
            return Merge( format, imageStreams.Select( stream => new InMemoryTiff( stream ).Image ) );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static InMemoryTiff Merge( TiffEncodingFormat format, params Stream[] imageStreams )
        {
            return Merge( format, imageStreams.Select( stream => new InMemoryTiff( stream ).Image ) );
        }
        #endregion

        #region MergeToFile Overloads
     
        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff MergeToFile( string outFile, params InMemoryTiff[] images )
        {
            return this.MergeToFile( outFile, images.Select( tiff => tiff.Image ) );
        }

        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff MergeToFile( string outFile, IEnumerable<InMemoryTiff> images )
        {
            return this.MergeToFile( outFile, images.Select( image => image.Image ) );
        }

        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff MergeToFile( string outFile, params Stream[] imageStreams )
        {
            return this.MergeToFile( outFile, imageStreams.Select( stream => new InMemoryTiff( stream ).Image ) );
        }

        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff MergeToFile( string outFile, IEnumerable<Stream> imageStreams )
        {
            return this.MergeToFile( outFile, imageStreams.Select( stream => new InMemoryTiff( stream ).Image ) );
        }
        #endregion
    }
}
