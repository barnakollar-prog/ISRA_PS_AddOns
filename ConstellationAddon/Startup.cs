using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;

namespace ConstellationAddon
{
    public class ConstellationCommand : TxButtonCommand
    {
        public override string Category => "ISRA";
        public override string Name => "Constellation Validator";
        public override string Bitmap => "";
        public override string LargeBitmap => "";

        public override void Execute(object cmdParams)
        {
            var form = new ConstellationForm();
            form.Show();
        }
    }
}