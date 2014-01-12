namespace Octgn.Core.Play
{
    using System.Windows.Media;

    public interface IPlayPlayer
    {
		/// <summary>
		/// Identifier
		/// </summary>
        byte Id{ get; }

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