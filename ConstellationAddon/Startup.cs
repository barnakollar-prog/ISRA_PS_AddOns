using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;

namespace ConstellationAddon
{
    public class ConstellationCommand : TxButtonCommand
    {
        public override string Category => "ISRA";
        public override string Name => "Constellation Validator";
        public override string Bitmap
        {
            get { return "perc_01_03944_10_16x16.bmp"; }
        }

        public override string LargeBitmap
        {
            get { return "perc_01_03944_10_32x32.png"; }
        }

        public override void Execute(object cmdParams)
        {
            var form = new ConstellationForm();
            form.Show();
        }
    }
}