using System;

#if WINDOWSPLATFORM
using System.Drawing;
#endif

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Provides data for the AvatarChanged event.
    /// </summary>
    [Serializable]
    public class AvatarChangedEventArgs : EventArgs
    {
#if WINDOWSPLATFORM

		/// <summary>
		/// The JID of the XMPP entity that published the avatar information.
		/// </summary>
		public Jid Jid {
			get;
			private set;
		}

		/// <summary>
		/// True if the XMPP entity has cleared its avatar image.
		/// </summary>
		public bool Cleared {
			get {
				return Avatar == null;
			}
		}

		/// <summary>
		/// The SHA-1 hash of the XMPP entity's avatar image.
		/// </summary>
		public string Hash {
			get;
			private set;
		}

		/// <summary>
		/// The image representing the XMPP entity's avatar.
		/// </summary>
		public Image Avatar {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the AvatarChangedEventArgs class.
		/// </summary>
		/// <param name="jid">The JID of the XMPP entity that published the
		/// avatar information.</param>
		/// <param name="hash">The SHA-1 hash of the avatar image data.</param>
		/// <param name="avatar">The avatar information to include as part of
		/// the event.</param>
		/// <exception cref="ArgumentNullException">The jid parameter is
		/// null.</exception>
		public AvatarChangedEventArgs(Jid jid, string hash = null, Image avatar = null) {
			jid.ThrowIfNull("jid");
			Jid = jid;
			Hash = hash;
			Avatar = avatar;
		}
#endif
    }
}