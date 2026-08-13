using System.Collections.Generic;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Trackability criteria for Accusite constellation validation.
    /// A measurement point is OK if at least one tracking plane is satisfied.
    /// Only emitters visible from ALL 3 cameras count (same rule as Stars).
    /// </summary>
    public static class ConstellationCriteriaEngine
    {
        /// <summary>
        /// Result of criteria evaluation for a single measurement point.
        /// </summary>
        public class CriteriaResult
        {
            public bool IsOk { get; set; }
            public string SatisfiedPlane { get; set; } // null if NOK
            public PlaneResult PlaneA { get; set; }
            public PlaneResult PlaneB { get; set; }
            public PlaneResult PlaneC { get; set; }
            public PlaneResult PlaneD { get; set; }
            public string Label { get; set; }
        }

        public class PlaneResult
        {
            public string PlaneName { get; set; }
            public bool IsSatisfied { get; set; }
            public Dictionary<string, int> GroupCounts { get; set; }
            public string Label { get; set; }
        }

        // ── Plane definitions ─────────────────────────────────────
        //
        // Plane A (primary 1): max(NAUO1,NAUO2)≥3  AND  NAUO3≥2  AND  NAUO4≥2
        // Plane B (primary 2): max(NAUO6,NAUO8)≥3  AND  NAUO5≥2  AND  NAUO7≥2
        // Plane C (secondary 1): NAUO2≥2  AND  NAUO4≥2  AND  NAUO7≥2  AND  NAUO8≥2
        // Plane D (secondary 2): NAUO1≥2  AND  NAUO3≥2  AND  NAUO5≥2  AND  NAUO6≥2

        /// <summary>
        /// Evaluates all four tracking planes against the visibility result.
        /// Returns OK if at least one plane is satisfied.
        /// </summary>
        public static CriteriaResult Evaluate(ConstellationVisibilityResult visibility)
        {
            // Build group → visible-from-all-cameras count map
            var counts = BuildFullyVisibleCounts(visibility);

            var planeA = EvaluatePlaneA(counts);
            var planeB = EvaluatePlaneB(counts);
            var planeC = EvaluatePlaneC(counts);
            var planeD = EvaluatePlaneD(counts);

            string satisfiedPlane = null;
            if (planeA.IsSatisfied) satisfiedPlane = "A";
            else if (planeB.IsSatisfied) satisfiedPlane = "B";
            else if (planeC.IsSatisfied) satisfiedPlane = "C";
            else if (planeD.IsSatisfied) satisfiedPlane = "D";

            bool isOk = satisfiedPlane != null;

            return new CriteriaResult
            {
                IsOk = isOk,
                SatisfiedPlane = satisfiedPlane,
                PlaneA = planeA,
                PlaneB = planeB,
                PlaneC = planeC,
                PlaneD = planeD,
                Label = isOk
                    ? string.Format("OK (Plane {0})", satisfiedPlane)
                    : "NOK — no tracking plane satisfied"
            };
        }

        // ── Private: count emitters visible from ALL 3 cameras ────

        /// <summary>
        /// An emitter counts only if ALL 3 cameras see it.
        /// Returns group → fully-visible-emitter-count.
        /// </summary>
        private static Dictionary<string, int> BuildFullyVisibleCounts(
            ConstellationVisibilityResult visibility)
        {
            var counts = new Dictionary<string, int>();
            int totalCameras = 3;

            foreach (var emitter in visibility.VisibleEmitters)
            {
                if (emitter.VisibleFromCameras.Count < totalCameras)
                    continue; // not visible from all cameras

                if (!counts.ContainsKey(emitter.Group))
                    counts[emitter.Group] = 0;
                counts[emitter.Group]++;
            }

            return counts;
        }

        private static int Count(Dictionary<string, int> counts, string group)
        {
            int val;
            return counts.TryGetValue(group, out val) ? val : 0;
        }

        // ── Plane evaluators ──────────────────────────────────────

        private static PlaneResult EvaluatePlaneA(Dictionary<string, int> counts)
        {
            // max(NAUO1, NAUO2) >= 3  AND  NAUO3 >= 2  AND  NAUO4 >= 2
            int nauo1 = Count(counts, "NAUO1");
            int nauo2 = Count(counts, "NAUO2");
            int nauo3 = Count(counts, "NAUO3");
            int nauo4 = Count(counts, "NAUO4");

            bool apexOk = (nauo1 >= 3 || nauo2 >= 3);
            bool ok = apexOk && nauo3 >= 2 && nauo4 >= 2;

            return new PlaneResult
            {
                PlaneName = "A (Primary 1)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO1", nauo1}, {"NAUO2", nauo2},
                      {"NAUO3", nauo3}, {"NAUO4", nauo4} },
                Label = string.Format(
                    "A: apex({0},{1})≥3={2} | NAUO3({3})≥2={4} | NAUO4({5})≥2={6} → {7}",
                    nauo1, nauo2, apexOk,
                    nauo3, nauo3 >= 2,
                    nauo4, nauo4 >= 2,
                    ok ? "OK" : "NOK")
            };
        }

        private static PlaneResult EvaluatePlaneB(Dictionary<string, int> counts)
        {
            // max(NAUO6, NAUO8) >= 3  AND  NAUO5 >= 2  AND  NAUO7 >= 2
            int nauo5 = Count(counts, "NAUO5");
            int nauo6 = Count(counts, "NAUO6");
            int nauo7 = Count(counts, "NAUO7");
            int nauo8 = Count(counts, "NAUO8");

            bool apexOk = (nauo6 >= 3 || nauo8 >= 3);
            bool ok = apexOk && nauo5 >= 2 && nauo7 >= 2;

            return new PlaneResult
            {
                PlaneName = "B (Primary 2)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO5", nauo5}, {"NAUO6", nauo6},
                      {"NAUO7", nauo7}, {"NAUO8", nauo8} },
                Label = string.Format(
                    "B: apex({0},{1})≥3={2} | NAUO5({3})≥2={4} | NAUO7({5})≥2={6} → {7}",
                    nauo6, nauo8, apexOk,
                    nauo5, nauo5 >= 2,
                    nauo7, nauo7 >= 2,
                    ok ? "OK" : "NOK")
            };
        }

        private static PlaneResult EvaluatePlaneC(Dictionary<string, int> counts)
        {
            // NAUO2≥2  AND  NAUO4≥2  AND  NAUO7≥2  AND  NAUO8≥2
            int nauo2 = Count(counts, "NAUO2");
            int nauo4 = Count(counts, "NAUO4");
            int nauo7 = Count(counts, "NAUO7");
            int nauo8 = Count(counts, "NAUO8");

            bool ok = nauo2 >= 2 && nauo4 >= 2 && nauo7 >= 2 && nauo8 >= 2;

            return new PlaneResult
            {
                PlaneName = "C (Secondary 1)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO2", nauo2}, {"NAUO4", nauo4},
                      {"NAUO7", nauo7}, {"NAUO8", nauo8} },
                Label = string.Format(
                    "C: NAUO2({0})≥2={1} | NAUO4({2})≥2={3} | NAUO7({4})≥2={5} | NAUO8({6})≥2={7} → {8}",
                    nauo2, nauo2 >= 2,
                    nauo4, nauo4 >= 2,
                    nauo7, nauo7 >= 2,
                    nauo8, nauo8 >= 2,
                    ok ? "OK" : "NOK")
            };
        }

        private static PlaneResult EvaluatePlaneD(Dictionary<string, int> counts)
        {
            // NAUO1≥2  AND  NAUO3≥2  AND  NAUO5≥2  AND  NAUO6≥2
            int nauo1 = Count(counts, "NAUO1");
            int nauo3 = Count(counts, "NAUO3");
            int nauo5 = Count(counts, "NAUO5");
            int nauo6 = Count(counts, "NAUO6");

            bool ok = nauo1 >= 2 && nauo3 >= 2 && nauo5 >= 2 && nauo6 >= 2;

            return new PlaneResult
            {
                PlaneName = "D (Secondary 2)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO1", nauo1}, {"NAUO3", nauo3},
                      {"NAUO5", nauo5}, {"NAUO6", nauo6} },
                Label = string.Format(
                    "D: NAUO1({0})≥2={1} | NAUO3({2})≥2={3} | NAUO5({4})≥2={5} | NAUO6({6})≥2={7} → {8}",
                    nauo1, nauo1 >= 2,
                    nauo3, nauo3 >= 2,
                    nauo5, nauo5 >= 2,
                    nauo6, nauo6 >= 2,
                    ok ? "OK" : "NOK")
            };
        }
    }
}