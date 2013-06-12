using System;
using System.Collections.Generic;

namespace nQuant
{
    class ColorData
    {
        public ColorData(int dataGranularity)
        {
            dataGranularity++;
            Moments = new ColorMoment[dataGranularity, dataGranularity, dataGranularity, dataGranularity];
        }

        public ColorMoment[, , ,] Moments { get; private set; }
    }
}