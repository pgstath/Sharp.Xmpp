namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Defines possible values for the different types of data-forms.
    /// </summary>
    public enum DataFormType
    {
        /// <summary>
        /// The form is a requesting form, i.e. the form-processing entity is
        /// asking the form-submitting entity to complete it.
        /// </summary>
        Form,

        /// <summary>
        /// The form-submitting entity is submitting data to the form-processing
        /// entity.
        /// </summary>
        Submit,

        /// <summary>
        /// The form-submitting entity wishes to signal cancellation of submission
        /// of data to the form-processing entity.
        /// </summary>
        Cancel,

        /// <summary>
        /// The form-processing entity is returning data (e.g., search results) to
        /// the form-submitting entity, or the data is a generic data set.
        /// </summary>
        Result
    }
}