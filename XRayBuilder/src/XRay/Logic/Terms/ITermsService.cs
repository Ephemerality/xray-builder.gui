using System.Collections.Generic;
using System.Data.Common;
using XRayBuilderGUI.XRay.Artifacts;

namespace XRayBuilderGUI.XRay.Logic.Terms
{
    public interface ITermsService
    {
        /// <summary>
        /// Extract terms from the given db.
        /// </summary>
        /// <param name="xrayDb">Connection to any db containing the proper dataset.</param>
        /// <param name="singleUse">If set, will close the connection when complete.</param>
        IEnumerable<Term> ExtractTermsNew(DbConnection xrayDb, bool singleUse);

        /// <summary>
        /// Extract terms from the old JSON X-Ray format
        /// </summary>
        IEnumerable<Term> ExtractTermsOld(string path);
    }
}