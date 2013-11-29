using System.Collections.Generic;

namespace nQuant
{
    public class LookupData
    {
        public LookupData()
        {
            Lookups = new List<Lookup>();
        }

        public List<Lookup> Lookups { get; private set; }
    }
}