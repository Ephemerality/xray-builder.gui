using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Ephemerality.Unpack.Util
{
    public static class GuidUtil
    {
        /// <summary>
        /// Process GUID. If in decimal form, convert to hex.
        /// </summary>
        [CanBeNull]
        public static string ConvertGuid(string guid)
        {
            if (guid == null)
                return null;

            if (Regex.IsMatch(guid, "/[a-zA-Z]/", RegexOptions.Compiled))
                guid = guid.ToUpper();
            else
            {
                long.TryParse(guid, out var guidDec);
                guid = guidDec.ToString("X");
            }

            if (guid == "0")
                throw new ArgumentException("An error occurred while converting the GUID.");

            return guid;
        }
    }
}