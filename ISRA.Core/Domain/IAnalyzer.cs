namespace ISRA.Core.Domain
{
    /// <summary>
    /// Generic analyzer interface for performing complete analysis operations.
    /// </summary>
    /// <typeparam name="TInput">Type of input data to analyze</typeparam>
    /// <typeparam name="TReport">Type of analysis report to produce</typeparam>
    public interface IAnalyzer<TInput, TReport>
    {
        /// <summary>
        /// Performs a complete analysis of the input data.
        /// </summary>
        /// <param name="input">Input data to analyze</param>
        /// <returns>Analysis report containing all validation results</returns>
        TReport Analyze(TInput input);
    }
}
