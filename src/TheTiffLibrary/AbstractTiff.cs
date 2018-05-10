using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TiffImage
{
    /// <summary>
    /// Represent an image that exists only in memory.
    /// </summary>
    public abstract partial class AbstractTiff : IDisposable
    {
        #region Properties & Variables

        public TiffEncodingFormat DefaultFormat;

        private static ImageCodecInfo codecInfo = null;
        public static ImageCodecInfo CodecInfo
        {
            get
            {
                return codecInfo ?? ImageCodecInfo.GetImageEncoders().First( ici => ici.MimeType == "image/tiff" );
            }
        }

        protected Image image = null;
        public abstract Image Image { get; protected set; }

        public int PageCount
        {
            get { return this.Image.GetFrameCount( FrameDimension.Page ); }
        }

        protected MemoryStream MemoryStream { get; set; }

        #endregion

        protected AbstractTiff()
        {
            this.DefaultFormat = TiffEncodingFormats.TiffRegular;
            this.MemoryStream = new MemoryStream();
        }

        /// <summary>
        /// Retrieve single pages/images from a multi-page TIFF image
        /// </summary>
        /// <param name="pageIndexes">Index of pages to retrieve</param>
        /// <returns>A collection of lazy loading in-memory tiff images</returns>
        public IEnumerable<InMemoryTiff> GetPages( params int[] pageIndexes )
        {
            return this.GetPages( pageIndexes.Select( index => index ), this.DefaultFormat.XPage );
        }

        /// <summary>
        /// Retrieve multiple single pages/images/frames from a multi-page TIFF image converting image in the specified format
        /// </summary>
        /// <param name="pageIndexes">Index of pages to retrieve</param>
        /// <param name="encoder">encoder format</param>
        /// <returns>A collection of lazy loading in-memory tiff images</returns>
        public IEnumerable<InMemoryTiff> GetPages( IEnumerable<int> pageIndexes, EncoderParameters encoder )
        {
            Guid objGuid = this.Image.FrameDimensionsList[ 0 ];
            FrameDimension objDimension = new FrameDimension( objGuid );

            foreach( var page in pageIndexes )
            {
                //You must keep the stream open for the lifetime of the image. DO NOT DISPOSE memoryStream.
                var memoryStream = new MemoryStream();

                this.Image.SelectActiveFrame( objDimension, page );
                this.Image.Save( memoryStream, AbstractTiff.CodecInfo, encoder );

                memoryStream.Capacity = (int)memoryStream.Length;
                yield return new InMemoryTiff( memoryStream );
            }
        }

        /// <summary>
        /// Split this multi-page TIFF image into multiple single page TIFF images
        /// </summary>
        /// <returns>A collection of lazy loading in-memory tiff images</returns>
        public IEnumerable<InMemoryTiff> Split()
        {
            return this.Split( this.DefaultFormat.XPage );
        }

        public IEnumerable<InMemoryTiff> Split( EncoderParameters encoder )
        {
            var pagesToRetrieve = Enumerable.Range( 0, this.PageCount );
            return this.GetPages( pagesToRetrieve, encoder );
        }

        /// <summary>
        /// Split a multi-page TIFF file into multiple single page TIFF files
        /// </summary>
        /// <param name="outputFile">The directory where images are stored</param>
        /// <param name="namingOptions">File sequence naming options</param>
        /// <returns>A collection of lazy loading in-memory tiff images</returns>
        public IEnumerable<FileSystemTiff> SplitToFile( string outDirectory, string outFileName, NewFilesNamingOptions namingOptions )
        {
            Directory.CreateDirectory( outDirectory );

            var imgPaths = new List<string>();
            foreach( var image in this.Split() )
            {
                string page = Path.Combine( outDirectory, namingOptions.ToString( outFileName ) );
                image.SaveTo( page );
                image.Dispose();

                imgPaths.Add( page );
            }

            return imgPaths.Select( imgPath => new FileSystemTiff( imgPath ) );
        }

        /// <summary>
        /// Append (multi-page) TIFF images to this (multi-page) TIFF image.
        /// For very big images consider using <seealso cref="MergeToFile"/>
        /// </summary>
        public void Append( IEnumerable<Image> images )
        {
            this.Append( images, this.MemoryStream = new MemoryStream() );

            //We now dispose the image and set it to null thus allowing the image to be lazily-reloaded the next time it is needed.
            this.image.Dispose();
            this.image = null;
        }

        /// <summary>
        /// Core append function
        /// </summary>
        /// <param name="images">Images to append to this instance image</param>
        /// <param name="dataBuffer">Open stream used to save data</param>
        /// <returns>Merged image</returns>
        protected Image Append( IEnumerable<Image> images, Stream dataBuffer )
        {
            var imgList = new List<Image>() { this.Image };
            return Merge( this.DefaultFormat, dataBuffer, imgList.Concat( images ) );
        }

        /// <summary>
        /// Core merge function
        /// </summary>
        /// <param name="images">Images to concatenae following provided order</param>
        /// <param name="dataBuffer">Open stream used to save data</param>
        /// <returns>Merged image</returns>
        protected static Image Merge( TiffEncodingFormat format, Stream dataBuffer, IEnumerable<Image> images )
        {
            var memoryImgs = images.Select( img => new InMemoryTiff( img ) );

            // Get all pages from each (multi)tiff image and flattens Ienumerable<IEnumerable<Image>> to IEnumerable<Image>
            var pages = memoryImgs.Select( img => img.Split() ).SelectMany( page => page );

            // First image
            Image newImage = pages.First().Image;
            newImage.Save( dataBuffer, CodecInfo, format.FirstPage );

            // Other images   
            var skipFirst = pages.Skip( 1 );
            foreach( var image in skipFirst )
            {
                newImage.SaveAdd( image.Image, format.XPage );
                image.Dispose();
            }

            // Last page
            newImage.SaveAdd( format.LastPage );

            return newImage;
        }

        protected static Image Merge( TiffEncodingFormat format, Stream dataBuffer, params Image[] images )
        {
            return Merge( format, dataBuffer, images.Select( image => image ) );
        }

        /// <summary>
        /// Merge this (multi-page) TIFF image with other (multi-page) TIFF images.
        /// Result is stored directly to file so this method is perfect for very big images.
        /// </summary>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff MergeToFile( string outFile, IEnumerable<Image> images )
        {
            var fileStream = new FileStream( outFile, FileMode.Create );
            this.Append( images, fileStream );

            return new FileSystemTiff( outFile );
        }

        /// <summary>
        /// Removes specified pages from current image
        /// </summary>
        /// <param name="pageIndexes"></param>
        public void RemovePages( params int[] pageIndexes )
        {
            var usefulPages = Enumerable.Range( 0, this.PageCount ).Where( i => !pageIndexes.Contains( i ) );

            var newImg = InMemoryTiff.Merge( this.DefaultFormat, this.GetPages( usefulPages, this.DefaultFormat.XPage ) );
            this.MemoryStream = newImg.MemoryStream;
            this.Image = newImg.Image;
        }

      

        /// <summary>
        /// Saves this image to file system
        /// </summary>
        /// <param name="destinationFile">Full path to output file. If file does not exists it is created, otherwise it is overwritten.</param>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff SaveTo( string destinationFile )
        {
            // The easiest and cheapest way to save an image to file is to load the image into a MemoryStream and save it to file.
            Directory.CreateDirectory( Path.GetDirectoryName( destinationFile ) );

            if( this.MemoryStream != null && this.MemoryStream.Capacity > 0 )
            {
                File.WriteAllBytes( destinationFile, this.MemoryStream.GetBuffer() );
                return new FileSystemTiff( destinationFile );
            }

            return this.SaveTo( this.DefaultFormat, destinationFile );
        }

        /// <summary>
        /// Saves this image to file system
        /// </summary>
        /// 
        /// <param name="destinationFile">Full path to output file. If file does not exists it is created, otherwise it is overwritten.</param>
        /// <returns>A reference to the new image</returns>
        public FileSystemTiff SaveTo( TiffEncodingFormat encoding, string destinationFile )
        {
            Merge( encoding, new FileStream( destinationFile, FileMode.Create ), this.Image );
            return new FileSystemTiff( destinationFile );
        }

        #region Apply
        public void RemoveImages( Func<Image, bool> where )
        {
            var usefulImages = this.Split().Select( tiff => tiff.Image ).Where( image => !where( image ) );

            var newImg = InMemoryTiff.Merge( this.DefaultFormat, usefulImages );
            this.MemoryStream = newImg.MemoryStream;
            this.Image = newImg.Image;
        }

        private IEnumerable<Image> performAction( Func<Image, Image> action, Func<Image, bool> where )
        {
            //this function lazily returns every image applying an action only on images matching a condition.
            foreach( var image in this.Split() )
                yield return where( image.Image ) ? action( image.Image ) : image.Image;
        }

        public void Apply( Func<Image, Image> action, Func<Image, bool> where )
        {
            var images = performAction( action, where );
            var res = InMemoryTiff.Merge( this.DefaultFormat, images );
            res.SaveTo( @"C:\test.tif" );
            this.Image = res.Image;
            this.MemoryStream = res.MemoryStream;
        }

        public void Apply( Func<Image, Image> action )
        {
            this.Apply( action, img => true );
        }
        #endregion

        /// <summary>
        /// Frees all resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Call dispose on variables (if available), not on properties here! 
            // If you call property and it is null, a call will reload resources just do dispose them.

            if( this.image != null )
                this.image.Dispose();

            if( this.MemoryStream != null )
                this.MemoryStream.Dispose();

            // IMPORTANT!
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.SuppressFinalize( this );
        }
    }
}
