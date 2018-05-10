using System;
using System.IO;
using System.Globalization;

namespace TiffImage
{
    public class NewFilesNamingOptions
    {
        public string CounterPrefix { get; set; }
        public string CounterSuffix { get; set; }

        public int InitialCounterNumber { get; set; }
        public int NumberMinimunLength { get; set; }
        public int IncrementFactor { get; set; }

        public int CurrentCounterNumber { get; private set; }

        public NewFilesNamingOptions()
        {
            this.InitialCounterNumber = 1;
            this.CurrentCounterNumber = 1;
            this.NumberMinimunLength = 1;
            this.IncrementFactor = 1;
        }

        public override string ToString()
        {
            return this.ToString( "", "", this.InitialCounterNumber );
        }

        public string ToString( string fullFileName )
        {
            var newName = this.ToString( fullFileName, this.CurrentCounterNumber );
            this.CurrentCounterNumber += this.IncrementFactor;

            return newName;
        }

        public string ToString( string fullFileName, int number )
        {
            string fileName = Path.GetFileNameWithoutExtension( fullFileName );
            string fileExtension = Path.GetExtension( fullFileName );

            return this.ToString( fileName, fileExtension, number );
        }

        public string ToString( string fileName, string fileExtension, int number )
        {
            string formattedNumber = number.ToString( "D" + this.NumberMinimunLength, CultureInfo.CurrentCulture );
            return String.Format( CultureInfo.CurrentCulture, @"{0}{1}{2}{3}{4}", fileName, CounterPrefix, formattedNumber, CounterSuffix, fileExtension );
        }
    }
}
