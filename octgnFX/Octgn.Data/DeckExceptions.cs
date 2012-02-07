﻿using System;

namespace Octgn.Data
{
    public abstract class DeckException : Exception
    {
        public DeckException(string message)
            : base(message)
        { }

        public DeckException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public sealed class FileNotReadableException : DeckException
    {
        public FileNotReadableException(Exception inner)
            : base(string.Format("OCTGN is unable to read the file. Internal error is:\r\n\r\n{0}", inner.Message), inner)
        { }
    }

    public sealed class WrongGameException : DeckException
    {
        public readonly Guid ActualGameId;
        public readonly string ExpectedGame;

        public WrongGameException(Guid actual, string expected)
            : base(string.Format("This deck was made for the game with id = '{0}', which isn't '{1}'.", actual, expected))
        { ActualGameId = actual; ExpectedGame = expected; }
    }

    public sealed class InvalidFileFormatException : DeckException
    {
        public InvalidFileFormatException()
            : base("The file format appears to be invalid or corrupted.")
        { }

        public InvalidFileFormatException(string message)
            : base(message)
        { }
    }

    public sealed class UnknownCardException : DeckException
    {
        public readonly string CardId;
        public readonly string CardName;

        public UnknownCardException(string id, string name)
            : base(string.Format("OCTGN doesn't know this card:\r\nCard id = {0}\r\nCard name = \"{1}\"", id ?? "?", name ?? "?"))
        { CardId = id; CardName = name; }
    }

    public sealed class UnknownGameException : DeckException
    {
        public UnknownGameException(Guid gameId)
            : base(string.Format("The game with id = {0} is not installed.", gameId))
        { }
    }
}