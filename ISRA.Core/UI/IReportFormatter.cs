using System.Windows.Forms;
using ISRA.Core.Domain;

namespace ISRA.Core.UI
{
    /// <summary>
    /// Interface for report formatters that convert analysis results into UI elements.
    /// </summary>
    public interface IReportFormatter
    {
        /// <summary>
        /// Formats a validation result as a ListView item.
        /// </summary>
        ListViewItem FormatValidationResult(IValidationResult result);
    }

    /// <summary>
    /// Generic interface for report formatters.
    /// </summary>
    /// <typeparam name="TReport">Type of report to format</typeparam>
    public interface IReportFormatter<TReport>
    {
        /// <summary>
        /// Formats a complete analysis report for display.
        /// </summary>
        void FormatReport(TReport report, ListView targetListView);
    }
}
