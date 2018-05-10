using System.Drawing;
using System.IO;
using System;

namespace TiffImage
{
    /// <summary>
    /// Represent an image that exists only in memory.
    /// </summary>
    public partial class InMemoryTiff : AbstractTiff
    {
        #region Properties & Variables
        public override Image Image
        {
            get
            {
                if( image == null && this.MemoryStream != null )
                    image = Image.FromStream( this.MemoryStream, true, false );

                return image;
            }

            protected set { image = value; }
        }
        #endregion

        #region Contructors
        public InMemoryTiff( Image image )
            : base()
        {
            this.Image = image;
        }

        public InMemoryTiff( Stream stream )
            : base()
        {
            this.MemoryStream = (MemoryStream)stream;
        }
        #endregion
    }
}