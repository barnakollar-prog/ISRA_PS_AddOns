namespace ISRA.Core.Domain
{
    /// <summary>
    /// Generic validator interface for performing validation checks.
    /// </summary>
    /// <typeparam name="TInput">Type of input data to validate</typeparam>
    /// <typeparam name="TResult">Type of validation result</typeparam>
    public interface IValidator<TInput, TResult> where TResult : IValidationResult
    {
        /// <summary>
        /// Name of the validator/criterion being checked.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Performs the validation check.
        /// </summary>
        /// <param name="input">Input data to validate</param>
        /// <returns>Validation result</returns>
        TResult Validate(TInput input);
    }
}
