using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.SensorHolders
{
    /// <summary>
    /// Interface for AccuSite sensor holder components.
    /// All positions/orientations are relative to the sensor component's self origin.
    /// </summary>
    public interface ISensorHolder
    {
        /// <summary>Returns all emitter definitions for this holder type.</summary>
        SensorEmitterData[] GetEmitters();

        /// <summary>Returns the world position of a specific emitter.</summary>
        TxVector GetEmitterWorldPosition(ITxLocatableObject holderLoc, SensorEmitterData emitter);

        /// <summary>Returns the world Z vector (emission direction) of a specific emitter.</summary>
        TxVector GetEmitterWorldZVector(ITxLocatableObject holderLoc, SensorEmitterData emitter);
    }

    /// <summary>
    /// Represents a single LED emitter on a sensor holder.
    /// Position and Z vector are in the sensor component's local coordinate system.
    /// </summary>
    public class SensorEmitterData
    {
        /// <summary>PS component frame name (e.g. "NAUO98_frame")</summary>
        public string Name { get; set; }

        /// <summary>Emitter group identifier (e.g. "NAUO1")</summary>
        public string Group { get; set; }

        /// <summary>Emitter position relative to sensor self origin [mm]</summary>
        public TxVector Position { get; set; }

        /// <summary>
        /// Emitter Z-axis direction in sensor local coordinate system.
        /// Computed from Rx/Ry/Rz Euler angles — same convention as Stars.
        /// </summary>
        public TxVector ZVector { get; set; }
    }
}