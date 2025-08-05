using System;
using NUnit.Framework;
using Octgn.Scripting;
using Octgn.Core;
using Octgn.Library;
using Octgn.Library.Scripting;

namespace Octgn.Test.OctgnApp.Scripting
{
    [TestFixture]
    public class RemoteCallValidUseCasesTests
    {
        private Engine _engine;
        private MockFunctionValidator mockValidator;
        private MockCodeExecutor mockExecutor;
        private bool _originalSandboxingSetting;

        [SetUp]
        public void SetUp()
        {
            // Initialize Config.Instance if not already done
            lock (Config.Sync)
            {
                if (Config.Instance == null)
                {
                    Config.Instance = new Config();
                }
            }
            
            // Store original setting
            _originalSandboxingSetting = Prefs.EnableGameSandboxing;
            
            // Enable sandboxing for tests
            Prefs.EnableGameSandboxing = true;
            
            // Create mock implementations
            mockValidator = new MockFunctionValidator(functionName => 
            {
                // Allow all common OCTGN functions + any custom functions for valid use case testing
                var validFunctions = new[]
                {
                    "whisper", "notify", "playSound", "setActivePlayer", "nextTurn",
                    "mute", "remoteCall", "random", "rnd", "deckSize",
                    "choosePlayer", "getPlayers", "me", "players", "openPack",
                    "createCard", "moveCardTo", "moveCardToTable", "turnCard",
                    "rotateCard", "switchTo", "passiveTurn", "activeTurn",
                    "isActivePlayer", "isMyTurn", "update", "rng",
                    "addTokens", "removeTokens", "addMarker", "removeMarker",
                    // Additional functions for various test scenarios
                    "drawCard", "dealCards", "playCard", "rollDice", "updateScore",
                    "changeTurn", "setPhase", "setStop", "triggerEvent", "processGameEvent",
                    "executeCustomFunction", "processTable", "processPile", "processCounter",
                    "shuffleDeck", "dealHand", "foldCards", "betChips", "raiseStakes",
                    "castSpell", "attackCreature", "tapLand", "untapAll", "drawSevenCards",
                    "rollD20", "addExperiencePoints", "levelUp", "useAbility", "moveCharacter",
                    "checkInventory", "useItem", "restoreHealth", "boardSetup", "endGame"
                };
                
                // For valid use case tests, be permissive about function names
                return Array.IndexOf(validFunctions, functionName) >= 0 || 
                       functionName.StartsWith("custom") ||
                       functionName.StartsWith("game") ||
                       functionName.StartsWith("player") ||
                       functionName.StartsWith("card") ||
                       functionName.Length > 3; // Allow most reasonable function names
            });

            // Mock executor that just logs what would be executed
            mockExecutor = new MockCodeExecutor((functionName, arguments) =>
            {
                Console.WriteLine($"Mock execution: {functionName}({arguments})");
            });
            
            // Create a test engine with mocked dependencies
            _engine = new Engine(true, mockValidator, mockExecutor); // true = for testing
            _engine.SetupEngine(true);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                // Restore original setting
                Prefs.EnableGameSandboxing = _originalSandboxingSetting;
            }
            catch (Exception)
            {
                // Ignore exceptions during teardown to prevent masking the real test failure
            }
            
            // Clean up engine
            ((IDisposable)_engine)?.Dispose();
        }

        #region Communication Functions

        [Test]
        public void ExecuteFunctionSecureNoFormat_WhisperWithStringMessage_ShouldSucceed()
        {
            // Arrange
            var function = "whisper";
            var args = "\"Hello, this is a test message!\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_NotifyWithStringMessage_ShouldSucceed()
        {
            // Arrange
            var function = "notify";
            var args = "\"Game state updated\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_NotifyBarWithColorAndMessage_ShouldSucceed()
        {
            // Arrange
            var function = "notifyBar";
            var args = "\"Red\", \"Important message\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_PlaySoundWithName_ShouldSucceed()
        {
            // Arrange
            var function = "playSound";
            var args = "\"coin_flip\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Game State Functions

        [Test]
        public void ExecuteFunctionSecureNoFormat_NextTurnWithPlayer_ShouldSucceed()
        {
            // Arrange
            var function = "nextTurn";
            var args = "Player(2), False";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_NextTurnWithForceFlag_ShouldSucceed()
        {
            // Arrange
            var function = "nextTurn";
            var args = "None, True";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_SetActivePlayer_ShouldSucceed()
        {
            // Arrange
            var function = "setActivePlayer";
            var args = "Player(1)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_SetPhase_ShouldSucceed()
        {
            // Arrange
            var function = "setPhase";
            var args = "2, False";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_SetStop_ShouldSucceed()
        {
            // Arrange
            var function = "setStop";
            var args = "1, True";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Card and Game Object Manipulation

        [Test]
        public void ExecuteFunctionSecureNoFormat_CustomFunctionWithCard_ShouldSucceed()
        {
            // Arrange - Testing a custom game function that manipulates cards
            var function = "drawCard";
            var args = "Card(123), Player(2)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CustomFunctionWithMultipleCards_ShouldSucceed()
        {
            // Arrange
            var function = "dealCards";
            var args = "[Card(100), Card(101), Card(102)], Player(1)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CustomFunctionWithCounter_ShouldSucceed()
        {
            // Arrange
            var function = "updateCounter";
            var args = "Counter(1, \"Life\", Player(1)), 5";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CustomFunctionWithPile_ShouldSucceed()
        {
            // Arrange
            var function = "shufflePile";
            var args = "Pile(10, \"Deck\", Player(2))";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CustomFunctionWithTable_ShouldSucceed()
        {
            // Arrange
            var function = "clearTable";
            var args = "table";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Complex Argument Combinations

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithMixedArguments_ShouldSucceed()
        {
            // Arrange
            var function = "gameAction";
            var args = "Player(1), \"action_type\", 42, True, Card(999)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNestedList_ShouldSucceed()
        {
            // Arrange
            var function = "complexMove";
            var args = "[[Card(1), Card(2)], [Card(3), Card(4)]], Player(1)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithEmptyList_ShouldSucceed()
        {
            // Arrange
            var function = "processCards";
            var args = "[], Player(1)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNoneArgument_ShouldSucceed()
        {
            // Arrange
            var function = "resetGame";
            var args = "None, Player(1), True";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNumericValues_ShouldSucceed()
        {
            // Arrange
            var function = "setPosition";
            var args = "Card(123), 100.5, -50.25, 0";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNegativeNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "adjustScore";
            var args = "Player(1), -10, \"penalty\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithScientificNotation_ShouldSucceed()
        {
            // Arrange
            var function = "preciseCalculation";
            var args = "1.23e-4, 5.67e+8";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region String Argument Variations

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithSingleQuotedString_ShouldSucceed()
        {
            // Arrange
            var function = "announceMessage";
            var args = "'Game started!'";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithDoubleQuotedString_ShouldSucceed()
        {
            // Arrange
            var function = "announceMessage";
            var args = "\"Player wins!\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithEmptyString_ShouldSucceed()
        {
            // Arrange
            var function = "clearMessage";
            var args = "\"\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithStringContainingSpaces_ShouldSucceed()
        {
            // Arrange
            var function = "displayText";
            var args = "\"This is a longer message with spaces\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithStringContainingNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "showScore";
            var args = "\"Player scored 123 points!\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithStringContainingUnicode_ShouldSucceed()
        {
            // Arrange
            var function = "displayUnicode";
            var args = "\"Ñice gâme! 🎲\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Boolean Argument Variations

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithTrueBoolean_ShouldSucceed()
        {
            // Arrange
            var function = "setFlag";
            var args = "True";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithFalseBoolean_ShouldSucceed()
        {
            // Arrange
            var function = "setFlag";
            var args = "False";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithMixedCaseBooleans_ShouldSucceed()
        {
            // Arrange
            var function = "toggleOptions";
            var args = "true, FALSE, True, false";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Game-Specific Scenarios

        [Test]
        public void ExecuteFunctionSecureNoFormat_CardGameDealHand_ShouldSucceed()
        {
            // Arrange - Typical card game scenario: dealing a hand
            var function = "dealHand";
            var args = "Player(1), 7, Pile(5, \"Deck\", Player(1))";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_CardGamePlayCard_ShouldSucceed()
        {
            // Arrange - Playing a card from hand to table
            var function = "playCard";
            var args = "Card(42), table, Player(2)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DiceRollNotification_ShouldSucceed()
        {
            // Arrange - Notifying players of dice roll results
            var function = "announceDiceRoll";
            var args = "Player(1), [6, 4, 2], \"rolled for initiative\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ScoreUpdate_ShouldSucceed()
        {
            // Arrange - Updating player scores
            var function = "updateScore";
            var args = "Player(2), 15, \"combat victory\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_TurnPhaseChange_ShouldSucceed()
        {
            // Arrange - Changing turn phases
            var function = "changePhase";
            var args = "\"Main Phase\", Player(1), True";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_GameEventTrigger_ShouldSucceed()
        {
            // Arrange - Triggering game events
            var function = "triggerEvent";
            var args = "\"card_drawn\", Card(123), Player(2), [\"extra\", \"data\"]";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Edge Cases and Boundary Values

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithVeryLargeNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "processLargeValue";
            var args = "999999999999999, -999999999999999";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithZeroValues_ShouldSucceed()
        {
            // Arrange
            var function = "resetValues";
            var args = "0, 0.0, Player(0)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithMaxPlayerIDs_ShouldSucceed()
        {
            // Arrange - Testing with maximum typical player IDs
            var function = "playerAction";
            var args = "Player(255), Card(2147483647)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithLongValidString_ShouldSucceed()
        {
            // Arrange - Testing with a long but valid string
            var longMessage = "\"This is a very long message that might be used in a game to provide detailed explanations or story text to players during gameplay scenarios.\"";
            var function = "displayLongMessage";
            var args = longMessage;

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithManyArguments_ShouldSucceed()
        {
            // Arrange - Testing with many valid arguments
            var function = "complexGameAction";
            var args = "Player(1), Card(1), Card(2), Card(3), \"action\", True, 42, table, [1, 2, 3]";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Whitespace and Formatting Variations

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithExtraSpaces_ShouldSucceed()
        {
            // Arrange - Testing with extra spaces that should be handled gracefully
            var function = "testSpacing";
            var args = "  Player(1)  ,   \"test\"   ,  42  ";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithTabsInStrings_ShouldSucceed()
        {
            // Arrange - Testing with tabs in string content (should be escaped)
            var function = "displayFormatted";
            var args = "\"Column1\\tColumn2\\tColumn3\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Argument Parsing Edge Cases

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNestedParentheses_ShouldSucceed()
        {
            // Arrange - Testing nested parentheses in valid OCTGN objects
            var function = "nestedAction";
            var args = "Counter(1, \"Life\", Player(1)), Pile(2, \"Hand\", Player(1))";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNestedBrackets_ShouldSucceed()
        {
            // Arrange - Testing nested brackets in lists
            var function = "processNestedLists";
            var args = "[[Card(1), Card(2)], [Card(3)]], Player(1)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithSingleArgumentNoComma_ShouldSucceed()
        {
            // Arrange - Testing single argument (no commas)
            var function = "singleArg";
            var args = "Player(1)";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithNoArguments_ShouldSucceed()
        {
            // Arrange - Testing function with no arguments
            var function = "noArgs";
            var args = "";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Custom Game Function Names

        [Test]
        public void ExecuteFunctionSecureNoFormat_ValidCustomFunctionNames_ShouldSucceed()
        {
            // Test various valid Python identifier patterns for custom game functions
            var validFunctionNames = new[]
            {
                "drawCard",
                "draw_card", 
                "DrawCard",
                "DRAW_CARD",
                "drawCard123",
                "draw_card_from_deck",
                "_privateFunction",
                "__init__",
                "onCardPlayed",
                "OnCardPlayed",
                "handlePlayerAction",
                "game_event_handler",
                "processMove",
                "validateAction",
                "checkWinCondition",
                "updateGameState"
            };

            foreach (var functionName in validFunctionNames)
            {
                // Act & Assert - Should not throw for any valid function name
                Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(functionName, "Player(1)"),
                    $"Valid function name '{functionName}' should not be rejected");
            }
        }

        #endregion

        #region Real Game Scenario Simulations

        [Test]
        public void ExecuteFunctionSecureNoFormat_MagicTheGatheringLikeScenario_ShouldSucceed()
        {
            // Arrange - Simulating Magic: The Gathering like gameplay
            var function = "castSpell";
            var args = "Card(567), Player(1), [Card(100), Card(101)], \"Lightning Bolt\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_PokerLikeScenario_ShouldSucceed()
        {
            // Arrange - Simulating poker-like gameplay
            var function = "placeBet";
            var args = "Player(2), 50, \"raise\", Counter(1, \"Chips\", Player(2))";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_BoardGameLikeScenario_ShouldSucceed()
        {
            // Arrange - Simulating board game movement
            var function = "moveToken";
            var args = "Card(999), 5, \"forward\", Player(1), True";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_RPGLikeScenario_ShouldSucceed()
        {
            // Arrange - Simulating RPG-like actions
            var function = "rollDice";
            var args = "\"2d6+3\", Player(1), \"attack roll\"";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion
    }
}
