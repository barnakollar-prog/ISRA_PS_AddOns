using System;
using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.Stars
{
    /// <summary>
    /// Defines the geometry of star 515-0139.
    /// LED center offset: (0, 0, 4mm) from self origin.
    /// 4 emitters with individual positions and orientations.
    /// </summary>
    public class Star515_0139 : IStar
    {
        // ── LED center offset ─────────────────────────────────────
        private static readonly TxVector LedOffset = new TxVector(0, 0, 4);

        // ── IStar implementation ──────────────────────────────────

        /// <summary>
        /// Returns the 4 emitter definitions in star local coordinate system.
        /// </summary>
        public EmitterData[] GetEmitters()
        {
            return new EmitterData[]
            {
                new EmitterData
                {
                    Name     = "Emitter_1",
                    Position = new TxVector(-13.76, 0.00, 2.05),
                    ZVector  = ComputeZVector(0.00, -10.00, 0.00)
                },
                new EmitterData
                {
                    Name     = "Emitter_2",
                    Position = new TxVector(0.00, -13.76, 2.05),
                    ZVector  = ComputeZVector(10.00, 0.00, 0.00)
                },
                new EmitterData
                {
                    Name     = "Emitter_3",
                    Position = new TxVector(13.76, 0.00, 2.05),
                    ZVector  = ComputeZVector(0.00, 10.00, 0.00)
                },
                new EmitterData
                {
                    Name     = "Emitter_4",
                    Position = new TxVector(0.00, 13.76, 2.05),
                    ZVector  = ComputeZVector(-10.00, 0.00, 0.00)
                }
            };
        }

        /// <summary>
        /// Returns the LED world position from a star locatable object.
        /// </summary>
        public TxVector GetLedWorldPosition(ITxLocatableObject starLoc)
        {
            TxTransformation starWorld = starLoc.AbsoluteLocation;
            return starWorld.Transform(LedOffset);
        }

        /// <summary>
        /// Returns the world position of a specific emitter.
        /// </summary>
        public TxVector GetEmitterWorldPosition(
            ITxLocatableObject starLoc, EmitterData emitter)
        {
            TxTransformation starWorld = starLoc.AbsoluteLocation;
            return starWorld.Transform(emitter.Position);
        }

        /// <summary>
        /// Returns the world Z vector of a specific emitter.
        /// </summary>
        public TxVector GetEmitterWorldZVector(
            ITxLocatableObject starLoc, EmitterData emitter)
        {
            TxTransformation starWorld = starLoc.AbsoluteLocation;
            // Rotate the emitter local Z vector into world space
            TxVector worldZ = new TxVector(
                starWorld[0, 0] * emitter.ZVector.X +
                starWorld[0, 1] * emitter.ZVector.Y +
                starWorld[0, 2] * emitter.ZVector.Z,
                starWorld[1, 0] * emitter.ZVector.X +
                starWorld[1, 1] * emitter.ZVector.Y +
                starWorld[1, 2] * emitter.ZVector.Z,
                starWorld[2, 0] * emitter.ZVector.X +
                starWorld[2, 1] * emitter.ZVector.Y +
                starWorld[2, 2] * emitter.ZVector.Z);
            return worldZ;
        }

        // ── Private helpers ───────────────────────────────────────

        /// <summary>
        /// Computes the Z vector from Rx, Ry, Rz rotation angles (degrees).
        /// Applies rotations in order: Rx, Ry, Rz.
        /// The base Z vector is (0, 0, 1).
        /// </summary>
        private static TxVector ComputeZVector(double rx, double ry, double rz)
        {
            double rxR = rx * Math.PI / 180.0;
            double ryR = ry * Math.PI / 180.0;
            double rzR = rz * Math.PI / 180.0;

            // Start with Z = (0, 0, 1)
            double x = 0, y = 0, z = 1;

            // Rotate around X
            double y1 = y * Math.Cos(rxR) - z * Math.Sin(rxR);
            double z1 = y * Math.Sin(rxR) + z * Math.Cos(rxR);
            y = y1; z = z1;

            // Rotate around Y
            double x2 = x * Math.Cos(ryR) + z * Math.Sin(ryR);
            double z2 = -x * Math.Sin(ryR) + z * Math.Cos(ryR);
            x = x2; z = z2;

            // Rotate around Z
            double x3 = x * Math.Cos(rzR) - y * Math.Sin(rzR);
            double y3 = x * Math.Sin(rzR) + y * Math.Cos(rzR);
            x = x3; y = y3;

            return new TxVector(x, y, z);
        }
    }

    /// <summary>
    /// Backward compatibility: static wrapper for star_515_0139.
    /// Legacy code can continue using star_515_0139.GetEmitters() etc.
    /// </summary>
    public static class star_515_0139
    {
        private static readonly Star515_0139 _instance = new Star515_0139();

        public static EmitterData[] GetEmitters() => _instance.GetEmitters();

        public static TxVector GetLedWorldPosition(ITxLocatableObject starLoc)
            => _instance.GetLedWorldPosition(starLoc);

        public static TxVector GetEmitterWorldPosition(ITxLocatableObject starLoc, EmitterData emitter)
            => _instance.GetEmitterWorldPosition(starLoc, emitter);

        public static TxVector GetEmitterWorldZVector(ITxLocatableObject starLoc, EmitterData emitter)
            => _instance.GetEmitterWorldZVector(starLoc, emitter);
    }
}