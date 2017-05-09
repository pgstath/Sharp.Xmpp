using XMPPEngineer.Extensions.Dataforms;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// A callback method which is invoked during the registration process to
    /// let the user fill out the required fields needed to complete the
    /// registration.
    /// </summary>
    /// <param name="form">A request form containing the fields to be filled
    /// out.</param>
    /// <returns>An initialized instance of the SubmitForm class containing the
    /// filled-out fields.</returns>
    public delegate SubmitForm RegistrationCallback(RequestForm form);
}