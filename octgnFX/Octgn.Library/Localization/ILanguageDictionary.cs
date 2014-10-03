// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octgn.Library.Localization
{
    public interface ILanguageDictionary
    {
        string LanguageCode { get; }
        string Exception__FileNotReadableException_Format { get; }
        string Exception__WrongGameException_Format { get; }
        string Exception__InvalidFileFormatException { get; }
        string Exception__UnknownCardException_Format { get; }
        string Exception__UnknownGameException_Format { get; }
        string Exception__InvalidProxyDefinition { get; }
        string Exception__CorruptDatabase { get; }
        string Exception__BrokenGameContactDev_Format { get; }
        string Exception__CanNotFindDirectoryGameDefBroken_Format { get; }
        string Exception__CanNotSaveDeckPathTooLong_Format { get; }
        string Exception__CanNotSaveDeckIOError_Format { get; }
        string Exception__CanNotSaveDeckUnspecified_Format { get; }
        string Exception__CanNotLoadDeckGameNotInstalled_Format { get; }
        string Exception__CanNotLoadDeckUnspecified_Format { get; }
        string Exception__CanNotLoadDeckCardNotInstalled_Format { get; }
        string Exception__CanNotLoadDeckCorrupt_Format { get; }
        string Exception__CanNotLoadDeckFileNotFound_Format { get; }
        string Exception__CanNotLoadDeckIOError_Format { get; }
        string Exception__CanNotCreateDeckMissingCardProperty { get; }
        string Exception__CanNotInstallGameTryRestart_Format { get; }
        string Exception__CanNotInstallo8cInvalid_Format { get; }
        string Exception__CanNotExtract_Format { get; }
        string Exception__CanNotUploadText { get; }
        string Exception__FileIsInvalid_Format { get; }

        string ServerMessage__UnknownBinaryMessage { get; }
        string ServerMessage__FailedToSendHelloMessage { get; }
        string ServerMessage__SpectatorsNotAllowed { get; }
        string ServerMessage__SpectatorsMuted { get; }
        string ServerMessage__CantJoinWasKicked { get; }
        string ServerMessage__SayHelloOnlyOnce { get; }
        string ServerMessage__IncorrectPassword { get; }
        string ServerMessage__InvalidGame_Format { get; }
        string ServerMessage__IncompatibleGameVersion_Format { get; }
        string ServerMessage__IncompatibleOctgnClient_Format { get; }
        string ServerMessage__GameStartedNotAcceptingNewPlayers { get; }
        string ServerMessage__CanNotReconnectFirstTimeConnecting { get; }
        string ServerMessage__PublicKeyDoesNotMatch { get; }
        string ServerMessage__CallDepreciated { get; }
        string ServerMessage__CanNotBootPlayerDoesNotExist { get; }
        string ServerMessage__CanNotBootNotHost { get; }
        string ServerMessage__PlayerKicked { get; }

        string UpdateMessage__CheckingGame_Format { get; }
        string UpdateMessage__UpdatingGame_Format { get; }
    }

    public class EnglishLanguageDictionary : ILanguageDictionary
    {
        public string LanguageCode
        {
            get { return "en-US"; }
        }

        public string Exception__FileNotReadableException_Format
        {
            get { return "OCTGN is unable to read the file. Internal error is:\r\n\r\n{0}"; }
        }

        public string Exception__WrongGameException_Format
        {
            get { return "This deck was made for the game with id = '{0}', which isn't '{1}'."; }
        }

        public string Exception__InvalidFileFormatException
        {
            get { return "The file format appears to be invalid or corrupted."; }
        }

        public string Exception__UnknownCardException_Format
        {
            get { return "OCTGN doesn't know this card:\r\nCard id = {0}\r\nCard name = \"{1}\""; }
        }

        public string Exception__UnknownGameException_Format
        {
            get { return "The game with id = {0} is not installed."; }
        }

        public string Exception__InvalidProxyDefinition
        {
            get { return "There is an invalid proxy definition. Please contact the game developer and let them know."; }
        }

        public string Exception__CorruptDatabase
        {
            get{ return "There is something wrong with your database. This may be caused by a broken game. If you are unable to uninstall the game, please let us know.";}
        }

        public string Exception__BrokenGameContactDev_Format
        {
            get { return "The game {0} you are trying to install/update/play is broken. Please contact the game developer."; }
        }

        public string Exception__CanNotFindDirectoryGameDefBroken_Format
        {
            get { return "Can not find directory {0}. This ususally means something is wrong with your game definition."; }
        }

        public string Exception__CanNotSaveDeckPathTooLong_Format
        {
            get { return "Could not save deck to {0}, the file path would be too long."; }
        }

        public string Exception__CanNotSaveDeckIOError_Format
        {
            get { return "Could not save deck to {0}, {1}"; }
        }

        public string Exception__CanNotSaveDeckUnspecified_Format
        {
            get { return "Could not save deck to {0}, there was an unspecified problem."; }
        }

        public string Exception__CanNotLoadDeckGameNotInstalled_Format
        {
            get { return "Could not load deck from {0}, you do not have the associated game installed."; }
        }

        public string Exception__CanNotLoadDeckUnspecified_Format
        {
            get { return "Could not load deck from {0}, there was an unspecified problem."; }
        }

        public string Exception__CanNotLoadDeckCardNotInstalled_Format
        {
            get { return "Problem loading deck {0}. The card with id: {1} and name: {2} is not installed."; }
        }

        public string Exception__CanNotLoadDeckCorrupt_Format
        {
            get { return "The deck {0} is corrupt."; }
        }

        public string Exception__CanNotLoadDeckFileNotFound_Format
        {
            get { return "Could not load deck from {0}, could not find the file."; }
        }

        public string Exception__CanNotLoadDeckIOError_Format
        {
            get { return "Could not load deck from {0}, {1}"; }
        }

        public string Exception__CanNotCreateDeckMissingCardProperty
        {
            get { return "The game you are trying to make a deck for has a missing property on a card. Please contact the game developer and let them know."; }
        }

        public string Exception__CanNotInstallGameTryRestart_Format
        {
            get { return "Game {0} could not be installed. Please restart your computer and try again"; }
        }

        public string Exception__CanNotInstallo8cInvalid_Format
        {
            get { return "The file {0} is invalid or broken. Please contact the game developer."; }
        }

        public string Exception__CanNotExtract_Format
        {
            get { return "Error extracting {0} to {1}\n{2}"; }
        }

        public string Exception__CanNotUploadText
        {
            get { return "There was an error uploading the text."; }
        }

        public string Exception__FileIsInvalid_Format
        {
            get { return "The file {0} is invalid."; }
        }

        public string ServerMessage__UnknownBinaryMessage
        {
            get { return "[Server Parser] Unknown message: "; }
        }

        public string ServerMessage__FailedToSendHelloMessage
        {
            get { return "You must shake hands. No one likes an anti social connection."; }
        }

        public string ServerMessage__SpectatorsNotAllowed
        {
            get { return "The game has started and doesn't allow spectators"; }
        }

        public string ServerMessage__SpectatorsMuted
        {
            get { return "You cannot chat, the host has muted specators."; }
        }

        public string ServerMessage__CantJoinWasKicked
        {
            get { return "You can't join this game, you were kicked."; }
        }

        public string ServerMessage__SayHelloOnlyOnce
        {
            get { return "[Hello]You may say hello only once."; }
        }

        public string ServerMessage__IncorrectPassword
        {
            get { return "The password you entered was incorrect."; }
        }

        public string ServerMessage__InvalidGame_Format
        {
            get { return "Invalid game selected. This server is hosting the game {0}."; }
        }

        public string ServerMessage__IncompatibleGameVersion_Format
        {
            get { return "Incompatible game version. This server is hosting game version {0}."; }
        }

        public string ServerMessage__IncompatibleOctgnClient_Format
        {
            get { return "Your version of OCTGN isn't compatible with this game server. This server is accepting {0} or greater clients only. Your current version is {1}. You should update."; }
        }

        public string ServerMessage__GameStartedNotAcceptingNewPlayers
        {
            get { return "This game is already started and is no longer accepting new players."; }
        }

        public string ServerMessage__CanNotReconnectFirstTimeConnecting
        {
            get { return "You can't reconnect, because you've never connected in the first place."; }
        }

        public string ServerMessage__PublicKeyDoesNotMatch
        {
            get { return "The public key you sent does not match the one on record."; }
        }

        public string ServerMessage__CallDepreciated
        {
            get { return "Call [{0}] is deprecated"; }
        }

        public string ServerMessage__CanNotBootPlayerDoesNotExist
        {
            get { return "[Boot] {0} cannot boot player because they don't exist."; }
        }

        public string ServerMessage__CanNotBootNotHost
        {
            get { return "[Boot] {0} cannot boot {1} because they are not the host."; }
        }

        public string ServerMessage__PlayerKicked
        {
            get { return "Player {0} was kicked: {1}"; }
        }

        public string UpdateMessage__CheckingGame_Format
        {
            get { return "Checking for updates for game {0}"; }
        }

        public string UpdateMessage__UpdatingGame_Format
        {
            get { return "Updating {0} from {1} to {2}"; }
        }
    }
}