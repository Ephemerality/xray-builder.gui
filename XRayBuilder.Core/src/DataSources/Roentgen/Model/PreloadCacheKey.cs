using System;

namespace XRayBuilder.Core.DataSources.Roentgen.Model
{
    public sealed class PreloadCacheKey : IEquatable<PreloadCacheKey>
    {
        public PreloadCacheKey(string asin, string regionTld)
        {
            Asin = asin;
            RegionTld = regionTld;
        }

        public string Asin { get; }
        public string RegionTld { get; }

        public bool Equals(PreloadCacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Asin == other.Asin && RegionTld == other.RegionTld;
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is PreloadCacheKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Asin != null ? Asin.GetHashCode() : 0) * 397) ^ (RegionTld != null ? RegionTld.GetHashCode() : 0);
            }
        }
    }
}