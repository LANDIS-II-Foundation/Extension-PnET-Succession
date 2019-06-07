//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Srinivas S., Robert M. Scheller

using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class DoublePixel : Pixel
    {
        public Band<double> MapCode = "The numeric code for each raster cell";

        public DoublePixel()
        {
            SetBands(MapCode);
        }
    }
}
