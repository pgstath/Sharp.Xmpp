using System;

namespace XMPPEngineer.Extensions
{
    /// <summary>
    /// Provides information about the music to which a user is listening.
    /// </summary>
    [Serializable]
    public class TuneInformation
    {
        /// <summary>
        /// The artist or performer of the song or piece.
        /// </summary>
        public string Artist
        {
            get;
            private set;
        }

        /// <summary>
        /// The duration of the song or piece in seconds.
        /// </summary>
        public int Length
        {
            get;
            private set;
        }

        /// <summary>
        /// The user's rating of the song or piece, from 1 (lowest) to
        /// 10 (highest).
        /// </summary>
        public int Rating
        {
            get;
            private set;
        }

        /// <summary>
        /// The collection (e.g., album) or other source (e.g., a band website
        /// that hosts streams or audio files).
        /// </summary>
        public string Source
        {
            get;
            private set;
        }

        /// <summary>
        /// The title of the song or piece.
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// A unique identifier for the tune; e.g., the track number within
        /// a collection.
        /// </summary>
        public string Track
        {
            get;
            private set;
        }

        /// <summary>
        /// A URI or URL pointing to information about the song, collection,
        /// or artist.
        /// </summary>
        public string Uri
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the TuneInformation class.
        /// </summary>
        /// <param name="title">The title of the song or piece.</param>
        /// <param name="artist">The artist or performer of the song or piece.</param>
        /// <param name="track"> A unique identifier for the tune; e.g., the track
        /// number within a collection.</param>
        /// <param name="length">The duration of the song or piece in
        /// seconds.</param>
        /// <param name="rating">The user's rating of the song or piece, from
        /// 1 (lowest) to 10 (highest).</param>
        /// <param name="source">The collection (e.g., album) or other source
        /// (e.g., a band website that hosts streams or audio files).</param>
        /// <param name="uri">A URI or URL pointing to information about the
        /// song, collection, or artist.</param>
        public TuneInformation(string title = null, string artist = null,
            string track = null, int length = 0, int rating = 0,
            string source = null, string uri = null)
        {
            length.ThrowIfOutOfRange(0, Int16.MaxValue);
            rating.ThrowIfOutOfRange(0, 10);
            Title = title;
            Artist = artist;
            Track = track;
            Length = length;
            Rating = rating;
            Source = source;
            Uri = uri;
        }
    }
}