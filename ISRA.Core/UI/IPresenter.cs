using System.Windows.Forms;
using ISRA.Core.Domain;

namespace ISRA.Core.UI
{
    /// <summary>
    /// Interface for presenters following MVP (Model-View-Presenter) pattern.
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// Executes the main analysis operation.
        /// </summary>
        void Analyze();
    }

    /// <summary>
    /// Interface for presenters with generic analysis result type.
    /// </summary>
    /// <typeparam name="TReport">Type of analysis report produced</typeparam>
    public interface IPresenter<TReport>
    {
        /// <summary>
        /// Executes the main analysis operation.
        /// </summary>
        /// <returns>Analysis report</returns>
        TReport Analyze();
    }
}
