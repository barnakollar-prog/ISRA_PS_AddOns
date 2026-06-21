using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.Stars
{
    /// <summary>
    /// Interface for AccuSite star components.
    /// </summary>
    public interface IStar
    {
        /// <summary>
        /// Returns all emitter definitions for this star type.
        /// </summary>
        EmitterData[] GetEmitters();

        /// <summary>
        /// Returns the LED center world position for a given star location.
        /// </summary>
        TxVector GetLedWorldPosition(ITxLocatableObject starLoc);

        /// <summary>
        /// Returns the world position of a specific emitter.
        /// </summary>
        TxVector GetEmitterWorldPosition(ITxLocatableObject starLoc, EmitterData emitter);

        /// <summary>
        /// Returns the world Z vector (direction) of a specific emitter.
        /// </summary>
        TxVector GetEmitterWorldZVector(ITxLocatableObject starLoc, EmitterData emitter);
    }

    /// <summary>
    /// Represents an LED emitter on a star with position and orientation.
    /// </summary>
    public class EmitterData
    {
        /// <summary>Emitter name (e.g., "Emitter_1")</summary>
        public string Name { get; set; }

        /// <summary>Emitter position relative to star origin (mm)</summary>
        public TxVector Position { get; set; }

        /// <summary>Emitter Z-axis direction vector in star local coordinate system</summary>
        public TxVector ZVector { get; set; }
    }
}
