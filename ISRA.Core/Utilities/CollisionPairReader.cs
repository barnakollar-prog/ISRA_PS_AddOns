using System.Collections.Generic;
using Tecnomatix.Engineering;

namespace ISRA.Core.Utilities
{
    /// <summary>
    /// Reads collision pairs defined in the PS Collision Viewer.
    /// Source: TxCollisionRoot.Pairs / TxCollisionPair.FirstList + SecondList
    /// </summary>
    public static class CollisionPairReader
    {
        /// <summary>
        /// Returns all active collision pairs defined in the current PS document.
        /// </summary>
        public static List<TxCollisionPair> GetActivePairs()
        {
            var result = new List<TxCollisionPair>();
            var pairs = TxApplication.ActiveDocument.CollisionRoot.Pairs;

            foreach (ITxObject obj in pairs)
            {
                var pair = obj as TxCollisionPair;
                if (pair != null && pair.Active)
                    result.Add(pair);
            }

            return result;
        }

        /// <summary>
        /// Returns all collision pairs (active and inactive).
        /// </summary>
        public static List<TxCollisionPair> GetAllPairs()
        {
            var result = new List<TxCollisionPair>();
            var pairs = TxApplication.ActiveDocument.CollisionRoot.Pairs;

            foreach (ITxObject obj in pairs)
            {
                var pair = obj as TxCollisionPair;
                if (pair != null)
                    result.Add(pair);
            }

            return result;
        }

        /// <summary>
        /// Checks collision using a predefined PS collision pair.
        /// Returns true if collision detected.
        /// </summary>
        public static bool CheckCollisionWithPair(TxCollisionPair pair)
        {
            if (pair == null || !pair.Active) return false;

            var queryParams = new TxCollisionQueryParams
            {
                Mode = TxCollisionQueryParams.TxCollisionQueryMode.All,
                StopQueryAfterFirstCollision = true,
                ReportLevel = TxCollisionQueryParams.TxCollisionReportLevel.ComponentLevel
            };

            return TxApplication.ActiveDocument.CollisionRoot
                .HasCollidingObjectsFromLists(
                    pair.FirstList, pair.SecondList, queryParams);
        }
    }
}