namespace XMPPEngineer.Extensions
{ 
    /// <summary>
    /// Describes whether a conference room is password protected.
    /// </summary>
    public enum RoomProtection
    {
        /// <summary>
        /// Not specified by the server.
        /// </summary>
        Undefined,

        /// <summary>
        /// A room that anyone is allowed to enter 
        /// without first providing the correct password
        /// </summary>
        Unsecured,

        /// <summary>
        /// A room that a user cannot enter
        /// without first providing the correct password.
        /// </summary>
        PasswordProtected
    }
}
