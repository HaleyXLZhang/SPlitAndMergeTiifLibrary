using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using TiffImage;

namespace TiffImage
{
    /// <summary>
    /// Overload facilities
    /// </summary>
    public abstract partial class AbstractTiff : IDisposable
    {
        #region Merge Overloads

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Merge( params Image[] images )
        {
            this.Append( (IEnumerable<Image>)( images ) );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static FileSystemTiff Merge( TiffEncodingFormat format, string outFile, IEnumerable<Image> images )
        {
            return InMemoryTiff.MergeToFile( format, outFile, images );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static FileSystemTiff Merge( TiffEncodingFormat format, string outFile, params Image[] images )
        {
            return Merge( format, outFile, images.Select( image => image ) );
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public static InMemoryTiff Merge( TiffEncodingFormat format, IEnumerable<Image> images )
        {
            var tiff = new InMemoryTiff( images.First() );
            tiff.Append( images.Skip( 1 ) );

            return tiff;
        }

        /// <summary>
        /// Merge (multi-page) TIFF images
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        /// <returns></returns>
        public static InMemoryTiff Merge( TiffEncodingFormat format, params Image[] images )
        {
            return Merge( format, images.Select( image => image ) );
        }

        #endregion

        #region MergeToFile Overloads

        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public static FileSystemTiff MergeToFile( TiffEncodingFormat format, string outfile, IEnumerable<Image> images )
        {
            return new InMemoryTiff( images.First() ).MergeToFile( outfile, images.Skip( 1 ) );
        }

        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff MergeToFile( string outFile, params Image[] images )
        {
            return this.MergeToFile( outFile, images.Select( image => image ) );
        }

        #endregion
    }
}
