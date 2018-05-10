using System.Drawing.Imaging;

namespace TiffImage
{
    public class TiffEncodingFormats
    {
        private static readonly EncoderParameter MultiFrame = new EncoderParameter( Encoder.SaveFlag, (long)EncoderValue.MultiFrame );
        private static readonly EncoderParameter FrameDimensionPage = new EncoderParameter( Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage );
        private static readonly EncoderParameter Flush = new EncoderParameter( Encoder.SaveFlag, (long)EncoderValue.Flush );
        private static readonly EncoderParameter LastFrame = new EncoderParameter( Encoder.SaveFlag, (long)EncoderValue.LastFrame );
        private static readonly EncoderParameter Bpp24 = new EncoderParameter( Encoder.ColorDepth, (long)24 );
        private static readonly EncoderParameter Bpp1 = new EncoderParameter( Encoder.ColorDepth, (long)8 );

        /// <summary>
        /// Black and white 1 bit per pixel multitiff format
        /// </summary>
        public static TiffEncodingFormat Tiff1Bpp
        {
            get
            {
                if( tiff1Bpp == null )
                {
                    var compression = new EncoderParameter( Encoder.Compression, (long)EncoderValue.CompressionCCITT4 );

                    tiff1Bpp = new TiffEncodingFormat( 2, 2, 3 );

                    tiff1Bpp.FirstPage.Param[ 0 ] = MultiFrame;
                    tiff1Bpp.FirstPage.Param[ 1 ] = compression;

                    tiff1Bpp.XPage.Param[ 0 ] = FrameDimensionPage;
                    tiff1Bpp.XPage.Param[ 1 ] = compression;

                    tiff1Bpp.LastPage.Param[ 0 ] = Flush;
                    tiff1Bpp.LastPage.Param[ 1 ] = LastFrame;
                    tiff1Bpp.LastPage.Param[ 2 ] = compression;
                }

                return tiff1Bpp;
            }
        }
        private static TiffEncodingFormat tiff1Bpp = null;

        /// <summary>
        /// Color 24 bit per pixel multitiff format
        /// </summary>
        public static TiffEncodingFormat Tiff24Bpp
        {
            get
            {
                if( tiff24Bpp == null )
                {
                    tiff24Bpp = new TiffEncodingFormat( 2, 2, 3 );

                    tiff24Bpp.FirstPage.Param[ 0 ] = MultiFrame;
                    tiff24Bpp.FirstPage.Param[ 1 ] = Bpp24;

                    tiff24Bpp.XPage.Param[ 0 ] = FrameDimensionPage;
                    tiff24Bpp.XPage.Param[ 1 ] = Bpp24;

                    tiff24Bpp.LastPage.Param[ 0 ] = Flush;
                    tiff24Bpp.LastPage.Param[ 1 ] = LastFrame;
                    tiff24Bpp.LastPage.Param[ 2 ] = Bpp24;
                }

                return tiff24Bpp;
            }
        }
        private static TiffEncodingFormat tiff24Bpp = null;


        /// <summary>
        /// Color LZW compressed multitiff format
        /// </summary>
        public static TiffEncodingFormat TiffRegular
        {
            get
            {
                if( tiffRegular == null )
                {
                    var compression = new EncoderParameter( Encoder.Compression, (long)EncoderValue.CompressionLZW );

                    tiffRegular = new TiffEncodingFormat( 2, 2, 3 );

                    TiffRegular.FirstPage.Param[ 0 ] = MultiFrame;
                    TiffRegular.FirstPage.Param[ 1 ] = compression;

                    TiffRegular.XPage.Param[ 0 ] = FrameDimensionPage;
                    TiffRegular.XPage.Param[ 1 ] = compression;

                    TiffRegular.LastPage.Param[ 0 ] = Flush;
                    TiffRegular.LastPage.Param[ 1 ] = LastFrame;
                    tiffRegular.LastPage.Param[ 2 ] = compression;
                }
                return tiffRegular;
            }
        }
        private static TiffEncodingFormat tiffRegular = null;
    }

    public class TiffEncodingFormat
    {
        public EncoderParameters FirstPage { get; set; }
        public EncoderParameters XPage { get; set; }
        public EncoderParameters LastPage { get; set; }

        /// <summary>
        /// Initialize 3 EncoderParamteres arrays of <paramref name="paramsPerEncoder"/> fixed capacity
        /// </summary>
        /// <param name="paramsPerEncoder">Number of encoder parameters that every EncoderParameters array can hold</param>
        public TiffEncodingFormat( int paramsPerEncoder )
        {
            this.FirstPage = new EncoderParameters( paramsPerEncoder );
            this.XPage = new EncoderParameters( paramsPerEncoder );
            this.LastPage = new EncoderParameters( paramsPerEncoder );
        }

        /// <summary>
        /// Initialize 3 EncoderParamteres arrays, each one of fixed capacity. You must specify each EncoderParameters's capacity.
        /// </summary>
        /// <param name="firstPageParamsNum">Number of encoder parameters you want to use to encode first tiff page</param>
        /// <param name="xPageParamsNum">Number of encoder parameters you want to use to encode any tiff page but the first and the last one</param>
        /// <param name="lastPageParamsNum">Number of encoder parameters you want to use to encode last tiff page</param>
        public TiffEncodingFormat( int firstPageParamsNum, int xPageParamsNum, int lastPageParamsNum )
        {
            this.FirstPage = new EncoderParameters( firstPageParamsNum );
            this.XPage = new EncoderParameters( xPageParamsNum );
            this.LastPage = new EncoderParameters( lastPageParamsNum );
        }
    }
}