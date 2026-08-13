using System;
using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.SensorHolders
{
    /// <summary>
    /// Sensor holder perc_01-03944-10.
    /// TCP (from self origin): X=264.50, Y=-0.14, Z=269.09, Rx=0, Ry=90, Rz=0
    /// Constellation: 8 groups (NAUO1–NAUO8), 5 emitters each = 40 LEDs total.
    /// </summary>
    public class SensorHolder_Perc_01_03944_10 : ISensorHolder
    {
        public SensorEmitterData[] GetEmitters()
        {
            return new SensorEmitterData[]
            {
                // --- NAUO1 ---
                E("NAUO1", "NAUO98_frame",    35.93,  -58.00,  350.79,    0.00,  36.00,    0.00),
                E("NAUO1", "NAUO101_frame",   42.38,  -58.00,  330.94,  180.00,  72.00,  180.00),
                E("NAUO1", "NAUO103_frame",   34.53,  -72.36,  341.64,  -48.04,  63.92, -108.90),
                E("NAUO1", "NAUO18_frame",    25.65,  -72.45,  326.12, -136.81,  36.28,  147.78),
                E("NAUO1", "NAUO102_frame",   16.16,  -72.77,  341.90,   47.02,  62.62,  -74.49),

                // --- NAUO2 ---
                E("NAUO2", "NAUO16_frame",   -35.93,  -58.00,  350.79,    0.00, -36.00,    0.00),
                E("NAUO2", "NAUO88_frame",   -42.38,  -58.00,  330.94,  180.00, -72.00,  180.00),
                E("NAUO2", "NAUO86_frame",   -34.53,  -72.36,  341.64,  -48.04, -63.92,  108.90),
                E("NAUO2", "NAUO15_frame",   -25.65,  -72.45,  326.12, -136.81, -36.28, -147.78),
                E("NAUO2", "NAUO87_frame",   -16.16,  -72.77,  341.90,   47.02, -62.62,   74.49),

                // --- NAUO3 ---
                E("NAUO3", "NAUO104_frame",  113.43,  -58.00,  137.86,    0.00,  36.00,    0.00),
                E("NAUO3", "NAUO105_frame",  119.88,  -58.00,  118.01,  180.00,  72.00,  180.00),
                E("NAUO3", "NAUO100_frame",  112.03,  -72.36,  128.71,  -48.04,  63.92, -108.90),
                E("NAUO3", "NAUO17_frame",   103.15,  -72.45,  113.19, -136.81,  36.28,  147.78),
                E("NAUO3", "NAUO99_frame",    93.66,  -72.77,  128.97,   47.02,  62.62,  -74.49),

                // --- NAUO4 ---
                E("NAUO4", "NAUO14_frame",  -113.43,  -58.00,  137.86,    0.00, -36.00,    0.00),
                E("NAUO4", "NAUO85_frame",  -119.88,  -58.00,  118.01,  180.00, -72.00,  180.00),
                E("NAUO4", "NAUO84_frame",  -112.03,  -72.36,  128.71,  -48.04, -63.92,  108.90),
                E("NAUO4", "NAUO83_frame",  -103.15,  -72.45,  113.19, -136.81, -36.28, -147.78),
                E("NAUO4", "NAUO13_frame",   -93.66,  -72.77,  128.97,   47.02, -62.62,   74.49),

                // --- NAUO5 ---
                E("NAUO5", "NAUO97_frame",   112.03,   72.36,  128.71,  -48.04, -63.92,  -71.10),
                E("NAUO5", "NAUO34_frame",    93.66,   72.77,  128.97,   47.02, -62.62, -105.51),
                E("NAUO5", "NAUO96_frame",   103.15,   72.45,  113.19, -136.81, -36.28,   32.22),
                E("NAUO5", "NAUO95_frame",   119.88,   58.00,  118.01,  180.00, -72.00,    0.00),
                E("NAUO5", "NAUO94_frame",   113.43,   58.00,  137.86,    0.00, -36.00,  180.00),

                // --- NAUO6 ---
                E("NAUO6", "NAUO93_frame",    16.16,   72.77,  341.90,   47.02, -62.62, -105.51),
                E("NAUO6", "NAUO92_frame",    34.53,   72.36,  341.64,  -48.04, -63.92,  -71.10),
                E("NAUO6", "NAUO90_frame",    42.38,   58.00,  330.94,  180.00, -72.00,    0.00),
                E("NAUO6", "NAUO91_frame",    25.65,   72.45,  326.12, -136.81, -36.28,   32.22),
                E("NAUO6", "NAUO89_frame",    35.93,   58.00,  350.79,    0.00, -36.00,  180.00),

                // --- NAUO7 ---
                E("NAUO7", "NAUO107_frame", -113.43,   58.00,  137.86,    0.00,  36.00,  180.00),
                E("NAUO7", "NAUO115_frame", -112.03,   72.36,  128.71,  -48.04,  63.92,   71.10),
                E("NAUO7", "NAUO112_frame", -119.88,   58.00,  118.01,  180.00,  72.00,    0.00),
                E("NAUO7", "NAUO113_frame", -103.15,   72.45,  113.19, -136.81,  36.28,  -32.22),
                E("NAUO7", "NAUO114_frame",  -93.66,   72.77,  128.97,   47.02,  62.62,  105.51),

                // --- NAUO8 ---
                E("NAUO8", "NAUO110_frame",  -16.16,   72.77,  341.90,   47.02,  62.62,  105.51),
                E("NAUO8", "NAUO106_frame",  -35.93,   58.00,  350.79,    0.00,  36.00,  180.00),
                E("NAUO8", "NAUO108_frame",  -42.38,   58.00,  330.94,  180.00,  72.00,    0.00),
                E("NAUO8", "NAUO111_frame",  -34.53,   72.36,  341.64,  -48.04,  63.92,   71.10),
                E("NAUO8", "NAUO109_frame",  -25.65,   72.45,  326.12, -136.81,  36.28,  -32.22),
            };
        }

        public TxVector GetEmitterWorldPosition(
            ITxLocatableObject holderLoc, SensorEmitterData emitter)
        {
            TxTransformation holderWorld = holderLoc.AbsoluteLocation;
            return holderWorld.Transform(emitter.Position);
        }

        public TxVector GetEmitterWorldZVector(
            ITxLocatableObject holderLoc, SensorEmitterData emitter)
        {
            TxTransformation w = holderLoc.AbsoluteLocation;
            TxVector z = emitter.ZVector;
            return new TxVector(
                w[0, 0] * z.X + w[0, 1] * z.Y + w[0, 2] * z.Z,
                w[1, 0] * z.X + w[1, 1] * z.Y + w[1, 2] * z.Z,
                w[2, 0] * z.X + w[2, 1] * z.Y + w[2, 2] * z.Z);
        }

        // ── Private helper ────────────────────────────────────────

        private static SensorEmitterData E(
            string group, string name,
            double x, double y, double z,
            double rx, double ry, double rz)
        {
            return new SensorEmitterData
            {
                Group = group,
                Name = name,
                Position = new TxVector(x, y, z),
                ZVector = ComputeZVector(rx, ry, rz)
            };
        }

        private static TxVector ComputeZVector(double rx, double ry, double rz)
        {
            double rxR = rx * Math.PI / 180.0;
            double ryR = ry * Math.PI / 180.0;
            double rzR = rz * Math.PI / 180.0;

            double x = 0, y = 0, z = 1;

            double y1 = y * Math.Cos(rxR) - z * Math.Sin(rxR);
            double z1 = y * Math.Sin(rxR) + z * Math.Cos(rxR);
            y = y1; z = z1;

            double x2 = x * Math.Cos(ryR) + z * Math.Sin(ryR);
            double z2 = -x * Math.Sin(ryR) + z * Math.Cos(ryR);
            x = x2; z = z2;

            double x3 = x * Math.Cos(rzR) - y * Math.Sin(rzR);
            double y3 = x * Math.Sin(rzR) + y * Math.Cos(rzR);
            x = x3; y = y3;

            return new TxVector(x, y, z);
        }
    }

    /// <summary>
    /// Static wrapper for backward compatibility and simple access.
    /// </summary>
    public static class perc_01_03944_10
    {
        private static readonly SensorHolder_Perc_01_03944_10 _instance
            = new SensorHolder_Perc_01_03944_10();

        public static SensorEmitterData[] GetEmitters() => _instance.GetEmitters();

        public static TxVector GetEmitterWorldPosition(
            ITxLocatableObject holderLoc, SensorEmitterData emitter)
            => _instance.GetEmitterWorldPosition(holderLoc, emitter);

        public static TxVector GetEmitterWorldZVector(
            ITxLocatableObject holderLoc, SensorEmitterData emitter)
            => _instance.GetEmitterWorldZVector(holderLoc, emitter);
    }
}