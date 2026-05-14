using Tecnomatix.Engineering;

namespace LedVisibilityAddon
{
    /// <summary>
    /// Defines the LED geometry of star component 515-0139.
    /// LED center offset is relative to the component's self origin.
    /// </summary>
    public class star_515_0139
    {
        // LED center offset from self origin (mm)
        public static readonly TxVector LedCenterOffset = new TxVector(0, 0, 4);

        /// <summary>
        /// Returns the LED center position in world coordinates.
        /// </summary>
        public static TxVector GetLedWorldPosition(ITxLocatableObject starComponent)
        {
            TxTransformation worldTx = starComponent.AbsoluteLocation;
            return worldTx * LedCenterOffset;
        }
    }
}