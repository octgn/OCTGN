using System;
using System.Windows.Media;

namespace Octgn.Core.Play
{
    public interface IPlayPlayer
    {
        /// <summary>
        /// Identifier
        /// </summary>
        Guid Id { get; }

		/// <summary>
		/// Nickname
		/// </summary>
        string Name{ get; }

		/// <summary>
		/// Player Color
		/// </summary>
        Color Color { get; }

		/// <summary>
		/// Player State
		/// </summary>
        PlayerState State { get; }
    }
}