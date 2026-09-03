using System.Collections.Generic;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Trackability criteria for Accusite constellation validation.
    /// A measurement point is OK if at least one tracking plane is satisfied.
    /// Only emitters visible from ALL 3 cameras count.
    /// </summary>
    public static class ConstellationCriteriaEngine
    {
        public class CriteriaResult
        {
            public bool IsOk { get; set; }
            public string SatisfiedPlane { get; set; }
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
        // Plane A: NAUO3≥1 AND NAUO4≥1 AND (NAUO1≥1 OR NAUO2≥1)
        // Plane B: NAUO5≥1 AND NAUO7≥1 AND (NAUO6≥1 OR NAUO8≥1)
        // Plane C: at least 3 of {NAUO2, NAUO4, NAUO7, NAUO8} have ≥1
        // Plane D: at least 3 of {NAUO1, NAUO3, NAUO5, NAUO6} have ≥1

        public static CriteriaResult Evaluate(ConstellationVisibilityResult visibility)
        {
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

        private static Dictionary<string, int> BuildFullyVisibleCounts(
            ConstellationVisibilityResult visibility)
        {
            var counts = new Dictionary<string, int>();

            foreach (var emitter in visibility.VisibleEmitters)
            {
                if (emitter.VisibleFromCameras.Count < 3) continue;

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
            // NAUO3≥1 AND NAUO4≥1 AND (NAUO1≥1 OR NAUO2≥1)
            int nauo1 = Count(counts, "NAUO1");
            int nauo2 = Count(counts, "NAUO2");
            int nauo3 = Count(counts, "NAUO3");
            int nauo4 = Count(counts, "NAUO4");

            bool apexOk = nauo1 >= 1 || nauo2 >= 1;
            bool ok = nauo3 >= 1 && nauo4 >= 1 && apexOk;

            return new PlaneResult
            {
                PlaneName = "A (Primary 1)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO1", nauo1}, {"NAUO2", nauo2},
                      {"NAUO3", nauo3}, {"NAUO4", nauo4} },
                Label = string.Format(
                    "A: NAUO3({0})≥1={1} | NAUO4({2})≥1={3} | NAUO1({4})orNAUO2({5})≥1={6} → {7}",
                    nauo3, nauo3 >= 1,
                    nauo4, nauo4 >= 1,
                    nauo1, nauo2, apexOk,
                    ok ? "OK" : "NOK")
            };
        }

        private static PlaneResult EvaluatePlaneB(Dictionary<string, int> counts)
        {
            // NAUO5≥1 AND NAUO7≥1 AND (NAUO6≥1 OR NAUO8≥1)
            int nauo5 = Count(counts, "NAUO5");
            int nauo6 = Count(counts, "NAUO6");
            int nauo7 = Count(counts, "NAUO7");
            int nauo8 = Count(counts, "NAUO8");

            bool apexOk = nauo6 >= 1 || nauo8 >= 1;
            bool ok = nauo5 >= 1 && nauo7 >= 1 && apexOk;

            return new PlaneResult
            {
                PlaneName = "B (Primary 2)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO5", nauo5}, {"NAUO6", nauo6},
                      {"NAUO7", nauo7}, {"NAUO8", nauo8} },
                Label = string.Format(
                    "B: NAUO5({0})≥1={1} | NAUO7({2})≥1={3} | NAUO6({4})orNAUO8({5})≥1={6} → {7}",
                    nauo5, nauo5 >= 1,
                    nauo7, nauo7 >= 1,
                    nauo6, nauo8, apexOk,
                    ok ? "OK" : "NOK")
            };
        }

        private static PlaneResult EvaluatePlaneC(Dictionary<string, int> counts)
        {
            // At least 3 of {NAUO2, NAUO4, NAUO7, NAUO8} have ≥1
            int nauo2 = Count(counts, "NAUO2");
            int nauo4 = Count(counts, "NAUO4");
            int nauo7 = Count(counts, "NAUO7");
            int nauo8 = Count(counts, "NAUO8");

            int satisfiedGroups = 0;
            if (nauo2 >= 1) satisfiedGroups++;
            if (nauo4 >= 1) satisfiedGroups++;
            if (nauo7 >= 1) satisfiedGroups++;
            if (nauo8 >= 1) satisfiedGroups++;

            bool ok = satisfiedGroups >= 3;

            return new PlaneResult
            {
                PlaneName = "C (Secondary 1)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO2", nauo2}, {"NAUO4", nauo4},
                      {"NAUO7", nauo7}, {"NAUO8", nauo8} },
                Label = string.Format(
                    "C: {0}/4 groups satisfied (NAUO2={1},NAUO4={2},NAUO7={3},NAUO8={4}) → {5}",
                    satisfiedGroups, nauo2, nauo4, nauo7, nauo8,
                    ok ? "OK" : "NOK")
            };
        }

        private static PlaneResult EvaluatePlaneD(Dictionary<string, int> counts)
        {
            // At least 3 of {NAUO1, NAUO3, NAUO5, NAUO6} have ≥1
            int nauo1 = Count(counts, "NAUO1");
            int nauo3 = Count(counts, "NAUO3");
            int nauo5 = Count(counts, "NAUO5");
            int nauo6 = Count(counts, "NAUO6");

            int satisfiedGroups = 0;
            if (nauo1 >= 1) satisfiedGroups++;
            if (nauo3 >= 1) satisfiedGroups++;
            if (nauo5 >= 1) satisfiedGroups++;
            if (nauo6 >= 1) satisfiedGroups++;

            bool ok = satisfiedGroups >= 3;

            return new PlaneResult
            {
                PlaneName = "D (Secondary 2)",
                IsSatisfied = ok,
                GroupCounts = new Dictionary<string, int>
                    { {"NAUO1", nauo1}, {"NAUO3", nauo3},
                      {"NAUO5", nauo5}, {"NAUO6", nauo6} },
                Label = string.Format(
                    "D: {0}/4 groups satisfied (NAUO1={1},NAUO3={2},NAUO5={3},NAUO6={4}) → {5}",
                    satisfiedGroups, nauo1, nauo3, nauo5, nauo6,
                    ok ? "OK" : "NOK")
            };
        }
    }
}