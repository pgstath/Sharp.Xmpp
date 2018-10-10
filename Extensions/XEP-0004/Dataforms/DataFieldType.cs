namespace XMPPEngineer.Extensions.Dataforms
{
    /// <summary>
    /// Defines possible values for the different types of data-fields.
    /// </summary>
    public enum DataFieldType
    {
        /// <summary>
        /// The field provides an either-or choice between two options.
        /// </summary>
        Boolean,

        /// <summary>
        /// The field is intended for data description rather than data gathering.
        /// </summary>
        Fixed,

        /// <summary>
        /// The field is not shown to the form-submitting entity, but instead is
        /// returned with the form.
        /// </summary>
        Hidden,

        /// <summary>
        /// The field enables an entity to gather or provide multiple Jabber IDs.
        /// </summary>
        JidMulti,

        /// <summary>
        /// The field enables an entity to gather or provide a single Jabber ID.
        /// </summary>
        JidSingle,

        /// <summary>
        /// The field enables an entity to gather or provide one or more options
        /// from among many.
        /// </summary>
        ListMulti,

        /// <summary>
        /// The field enables an entity to gather or provide one option from among
        /// many.
        /// </summary>
        ListSingle,

        /// <summary>
        /// The field enables an entity to gather or provide multiple lines of text.
        /// </summary>
        TextMulti,

        /// <summary>
        /// The field enables an entity to gather or provide a single line or word of
        /// text, which may be shown in an interface.
        /// </summary>
        TextSingle,

        /// <summary>
        /// The field enables an entity to gather or provide a single line or word of
        /// text, which shall be obscured in an interface (e.g., with multiple
        /// instances of the asterisk character).
        /// </summary>
        TextPrivate
    }
}