using System;
using Octgn.Library.Localization;

namespace Octgn.Library.Exceptions
{
    public abstract class DeckException : Exception
    {
        protected DeckException(string message)
            : base(message)
        {
        }

        protected DeckException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class FileNotReadableException : DeckException
    {
        public FileNotReadableException(Exception inner)
            : base(
                string.Format(L.D.Exception__FileNotReadableException_Format, inner.Message), inner)
        {
        }
    }

    public sealed class WrongGameException : DeckException
    {
        public readonly Guid ActualGameId;
        public readonly string ExpectedGame;

        public WrongGameException(Guid actual, string expected)
            : base(
                string.Format(L.D.Exception__WrongGameException_Format, actual, expected))
        {
            this.ActualGameId = actual;
            this.ExpectedGame = expected;
        }
    }

    public sealed class InvalidFileFormatException : DeckException
    {
        public InvalidFileFormatException()
            : base(L.D.Exception__InvalidFileFormatException)
        {
        }

        public InvalidFileFormatException(string message)
            : base(message)
        {
        }
    }

    public sealed class UnknownCardException : DeckException
    {
        public readonly string CardId;
        public readonly string CardName;

        public UnknownCardException(string id, string name)
            : base(
                string.Format(L.D.Exception__UnknownCardException_Format, id ?? "?",
                              name ?? "?"))
        {
            this.CardId = id;
            this.CardName = name;
        }
    }

    public sealed class UnknownGameException : DeckException
    {
        public UnknownGameException(Guid gameId)
            : base(string.Format(L.D.Exception__UnknownGameException_Format, gameId))
        {
        }
    }
}