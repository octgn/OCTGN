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

        #region Number Format Validation Tests

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithBinaryNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleBinaryValue";
            var args = "0b1010, 0B1111, 0b0";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithHexNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleHexValue";
            var args = "0xFF, 0x123ABC, 0X0";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithOctalNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleOctalValue";
            var args = "0o755, 0O123, 0o0";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithMixedNumberFormats_ShouldSucceed()
        {
            // Arrange
            var function = "handleMixedNumbers";
            var args = "42, 0xFF, 0b1010, 0o755, 3.14, 1.23e-4";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithLargeNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleLargeNumbers";
            var args = "0xFFFFFFFF, 0b11111111111111111111111111111111, 0o37777777777";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_FunctionWithEdgeCaseNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleEdgeCases";
            var args = "0b1, 0x1, 0o1, -0xFF, 0B1010";

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

        #region Dictionary Valid Use Cases

        [Test]
        public void ExecuteFunctionSecureNoFormat_SimpleDictionary_ShouldSucceed()
        {
            // Arrange
            var function = "processGameData";
            var args = "{\"action\": \"move_card\", \"player_id\": 1}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithStringKeys_ShouldSucceed()
        {
            // Arrange
            var function = "handleGameEvent";
            var args = "{\"event_type\": \"card_played\", \"message\": \"Player played a card\", \"timestamp\": \"2023-01-01\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithNumericKeys_ShouldSucceed()
        {
            // Arrange
            var function = "updatePlayerStats";
            var args = "{1: \"Player One\", 2: \"Player Two\", 3: \"Player Three\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithMixedKeyTypes_ShouldSucceed()
        {
            // Arrange
            var function = "configureMixedSettings";
            var args = "{\"setting_name\": \"auto_save\", 1: True, 2: False, \"timeout\": 30}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithOctgnObjects_ShouldSucceed()
        {
            // Arrange
            var function = "mapPlayersToCards";
            var args = "{Player(1): Card(100), Player(2): Card(101), Player(3): Card(102)}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithArrayValues_ShouldSucceed()
        {
            // Arrange
            var function = "organizePlayerHands";
            var args = "{\"player1\": [Card(1), Card(2), Card(3)], \"player2\": [Card(4), Card(5)]}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithNestedDictionaries_ShouldSucceed()
        {
            // Arrange
            var function = "processNestedGameData";
            var args = "{\"players\": {\"player1\": {\"name\": \"Alice\", \"score\": 100}, \"player2\": {\"name\": \"Bob\", \"score\": 85}}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithBooleanValues_ShouldSucceed()
        {
            // Arrange
            var function = "setGameFlags";
            var args = "{\"multiplayer\": True, \"sandbox_mode\": False, \"debug_enabled\": True}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithNullValues_ShouldSucceed()
        {
            // Arrange
            var function = "handleOptionalData";
            var args = "{\"optional_field1\": None, \"required_field\": \"value\", \"optional_field2\": None}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithNumericValues_ShouldSucceed()
        {
            // Arrange
            var function = "updateScores";
            var args = "{\"player1\": 100, \"player2\": 85.5, \"player3\": -10, \"bonus\": 1.5e2}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_EmptyDictionary_ShouldSucceed()
        {
            // Arrange
            var function = "initializeEmptyData";
            var args = "{}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithCounters_ShouldSucceed()
        {
            // Arrange
            var function = "manageCounters";
            var args = "{\"life\": Counter(1, \"Life\", Player(1)), \"poison\": Counter(2, \"Poison\", Player(1))}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithPiles_ShouldSucceed()
        {
            // Arrange
            var function = "organizePiles";
            var args = "{\"deck\": Pile(1, \"Deck\", Player(1)), \"hand\": Pile(2, \"Hand\", Player(1)), \"graveyard\": Pile(3, \"Graveyard\", Player(1))}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_ComplexNestedDictionary_ShouldSucceed()
        {
            // Arrange
            var function = "processComplexGameState";
            var args = "{\"game\": {\"phase\": \"main\", \"turn\": 5}, \"players\": {1: {\"name\": \"Alice\", \"hand\": [Card(1), Card(2)], \"life\": 20}, 2: {\"name\": \"Bob\", \"hand\": [Card(3)], \"life\": 18}}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithMixedNesting_ShouldSucceed()
        {
            // Arrange
            var function = "handleMixedData";
            var args = "{\"simple\": \"value\", \"array\": [1, 2, 3], \"nested_dict\": {\"inner\": \"data\"}, \"mixed_array\": [{\"dict_in_array\": \"value\"}, \"string_in_array\"]}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithSpecialCharactersInStrings_ShouldSucceed()
        {
            // Arrange
            var function = "handleSpecialText";
            var args = "{\"message\": \"Player's turn! @#$%^&*()\", \"unicode\": \"Ñice gâme! 🎲🃏\", \"symbols\": \"<>=+-*/\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithEscapedQuotes_ShouldSucceed()
        {
            // Arrange
            var function = "handleQuotedText";
            var args = "{\"quote1\": \"He said \\\"Hello world!\\\"\", \"quote2\": 'She replied \\'Hi there!\\''}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithWhitespace_ShouldSucceed()
        {
            // Arrange
            var function = "handleSpacedData";
            var args = "{ \"key1\" : \"value1\" , \"key2\" : \"value2\" , \"key3\" : { \"nested\" : \"data\" } }";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithVariableNames_ShouldSucceed()
        {
            // Arrange
            var function = "processVariables";
            var args = "{\"card_variable\": card, \"player_variable\": me, \"table_variable\": table}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryGameEventData_ShouldSucceed()
        {
            // Arrange
            var function = "logGameEvent";
            var args = "{\"event\": \"card_draw\", \"player\": Player(1), \"card\": Card(42), \"source\": Pile(5, \"Deck\", Player(1)), \"timestamp\": \"2023-01-01T12:00:00\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryGameConfiguration_ShouldSucceed()
        {
            // Arrange
            var function = "configureGame";
            var args = "{\"max_players\": 4, \"time_limit\": None, \"allow_spectators\": True, \"game_mode\": \"standard\", \"extensions\": [\"expansion1\", \"expansion2\"]}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryPlayerInventory_ShouldSucceed()
        {
            // Arrange
            var function = "updateInventory";
            var args = "{\"hand\": [Card(1), Card(2), Card(3)], \"battlefield\": [Card(10), Card(11)], \"graveyard\": [Card(20)], \"library\": [Card(30), Card(31), Card(32), Card(33)]}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryScoreTracking_ShouldSucceed()
        {
            // Arrange
            var function = "trackScores";
            var args = "{\"current_scores\": {Player(1): 15, Player(2): 12}, \"score_history\": [[10, 8], [13, 10], [15, 12]], \"bonus_points\": {\"combo\": 5, \"speed\": 2}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryGamePhases_ShouldSucceed()
        {
            // Arrange
            var function = "managePhases";
            var args = "{\"current_phase\": \"main1\", \"available_phases\": [\"untap\", \"upkeep\", \"draw\", \"main1\", \"combat\", \"main2\", \"end\"], \"phase_data\": {\"combat\": {\"attackers\": [], \"blockers\": []}}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryCardDatabase_ShouldSucceed()
        {
            // Arrange
            var function = "queryCardDatabase";
            var args = "{\"search_criteria\": {\"type\": \"creature\", \"cost\": {\"min\": 1, \"max\": 3}, \"colors\": [\"red\", \"white\"]}, \"result_format\": \"detailed\", \"limit\": 50}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryNetworkMessage_ShouldSucceed()
        {
            // Arrange
            var function = "sendNetworkMessage";
            var args = "{\"message_type\": \"game_action\", \"sender\": Player(1), \"action\": \"play_card\", \"target\": Player(2), \"data\": {\"card_id\": Card(42), \"position\": {\"x\": 100, \"y\": 200}}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryAIDecision_ShouldSucceed()
        {
            // Arrange
            var function = "makeAIDecision";
            var args = "{\"decision_type\": \"card_selection\", \"available_options\": [Card(1), Card(2), Card(3)], \"decision_factors\": {\"board_state\": \"favorable\", \"hand_size\": 5, \"opponent_life\": 15}, \"confidence\": 0.85}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryTournamentData_ShouldSucceed()
        {
            // Arrange
            var function = "recordTournamentData";
            var args = "{\"match_id\": \"match_001\", \"round\": 3, \"players\": [Player(1), Player(2)], \"results\": {Player(1): \"win\", Player(2): \"loss\"}, \"match_data\": {\"duration_minutes\": 45, \"turns_taken\": 12}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryCustomProperties_ShouldSucceed()
        {
            // Arrange
            var function = "setCustomProperties";
            var args = "{\"custom_field_1\": \"custom_value_1\", \"custom_field_2\": 42, \"custom_field_3\": True, \"custom_nested\": {\"sub_field_1\": \"sub_value_1\", \"sub_field_2\": [1, 2, 3]}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryStatistics_ShouldSucceed()
        {
            // Arrange
            var function = "calculateStatistics";
            var args = "{\"game_stats\": {\"total_games\": 150, \"wins\": 87, \"losses\": 63}, \"card_stats\": {\"most_played\": Card(42), \"win_rate_with_card\": 0.73}, \"time_stats\": {\"average_game_time\": 28.5, \"fastest_game\": 12.3}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryVeryDeepNesting_ShouldSucceed()
        {
            // Arrange
            var function = "processDeepStructure";
            var args = "{\"level1\": {\"level2\": {\"level3\": {\"level4\": {\"level5\": {\"deep_value\": \"reached_the_bottom\", \"deep_array\": [1, 2, 3], \"deep_object\": Player(1)}}}}}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryMassiveStructure_ShouldSucceed()
        {
            // Arrange
            var function = "handleMassiveData";
            var args = "{\"players\": {\"p1\": {\"hand\": [Card(1), Card(2)], \"life\": 20}, \"p2\": {\"hand\": [Card(3)], \"life\": 18}}, \"board\": {\"creatures\": [Card(10), Card(11)], \"artifacts\": [Card(20)]}, \"game_state\": {\"phase\": \"main\", \"turn\": 5, \"stack\": []}, \"metadata\": {\"start_time\": \"2023-01-01\", \"format\": \"standard\"}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryAlternatingStructures_ShouldSucceed()
        {
            // Arrange
            var function = "processAlternatingData";
            var args = "{\"mixed_data\": [{\"dict_in_array\": {\"nested_dict\": [\"array_in_dict\", {\"dict_in_array_in_dict\": \"deep_nesting\"}]}}, {\"another_dict\": [1, 2, {\"more_nesting\": \"value\"}]}]}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryAllDataTypes_ShouldSucceed()
        {
            // Arrange
            var function = "handleAllTypes";
            var args = "{\"string\": \"text\", \"integer\": 42, \"float\": 3.14, \"boolean_true\": True, \"boolean_false\": False, \"null_value\": None, \"player\": Player(1), \"card\": Card(100), \"counter\": Counter(1, \"Life\", Player(1)), \"pile\": Pile(5, \"Deck\", Player(1)), \"array\": [1, 2, 3], \"nested_dict\": {\"inner\": \"value\"}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryKeyVariations_ShouldSucceed()
        {
            // Arrange
            var function = "testKeyVariations";
            var args = "{\"string_key\": \"value1\", 'single_quote_key': \"value2\", 42: \"numeric_key\", 3.14: \"float_key\", True: \"boolean_key\", Player(1): \"object_key\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryValueVariations_ShouldSucceed()
        {
            // Arrange
            var function = "testValueVariations";
            var args = "{\"str_val\": \"string\", \"int_val\": 42, \"float_val\": 3.14, \"bool_val\": True, \"null_val\": None, \"player_val\": Player(1), \"card_val\": Card(100), \"array_val\": [1, 2, 3], \"dict_val\": {\"nested\": \"value\"}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryRealWorldCardGame_ShouldSucceed()
        {
            // Arrange - Realistic card game scenario
            var function = "executeMagicTheGatheringTurn";
            var args = "{\"phase\": \"main1\", \"active_player\": Player(1), \"priority_player\": Player(1), \"battlefield\": {Player(1): [Card(101), Card(102)], Player(2): [Card(201)]}, \"hands\": {Player(1): [Card(111), Card(112), Card(113)], Player(2): [Card(211), Card(212)]}, \"life_totals\": {Player(1): 18, Player(2): 15}, \"mana_pools\": {Player(1): {\"white\": 2, \"blue\": 1, \"colorless\": 0}, Player(2): {\"red\": 1, \"green\": 2}}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryRealWorldBoardGame_ShouldSucceed()
        {
            // Arrange - Realistic board game scenario
            var function = "updateBoardGameState";
            var args = "{\"board_positions\": {Player(1): {\"x\": 5, \"y\": 3}, Player(2): {\"x\": 8, \"y\": 7}}, \"player_resources\": {Player(1): {\"gold\": 150, \"wood\": 20, \"stone\": 15}, Player(2): {\"gold\": 120, \"wood\": 25, \"stone\": 10}}, \"building_placements\": {\"settlements\": [{\"player\": Player(1), \"position\": {\"x\": 5, \"y\": 3}}, {\"player\": Player(2), \"position\": {\"x\": 8, \"y\": 7}}], \"roads\": [{\"player\": Player(1), \"from\": {\"x\": 5, \"y\": 3}, \"to\": {\"x\": 6, \"y\": 3}}]}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryRealWorldRPG_ShouldSucceed()
        {
            // Arrange - Realistic RPG scenario
            var function = "processRPGAction";
            var args = "{\"character\": Player(1), \"action_type\": \"attack\", \"target\": Player(2), \"weapon\": Card(50), \"combat_stats\": {\"attack_roll\": 15, \"damage_roll\": 8, \"critical_hit\": False}, \"character_stats\": {\"hp\": 45, \"max_hp\": 60, \"armor_class\": 16, \"spell_slots\": {\"level_1\": 3, \"level_2\": 2}}, \"environmental_factors\": {\"terrain\": \"forest\", \"weather\": \"clear\", \"visibility\": \"normal\"}}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Dictionary Edge Cases and Boundary Tests

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithTrailingCommas_ShouldSucceed()
        {
            // Arrange
            var function = "handleTrailingCommas";
            var args = "{\"key1\": \"value1\", \"key2\": \"value2\", }";

            // Act & Assert - Should not throw for valid Python syntax
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithUnderscoreKeys_ShouldSucceed()
        {
            // Arrange
            var function = "handleUnderscoreKeys";
            var args = "{\"_private_key\": \"value\", \"public_key_\": \"value\", \"_both_sides_\": \"value\", \"single_underscore\": \"safe\"}";

            // Act & Assert - Should not throw for single underscores
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithLongKeys_ShouldSucceed()
        {
            // Arrange
            var function = "handleLongKeys";
            var args = "{\"this_is_a_very_long_key_name_that_should_still_be_valid_in_python_dictionaries\": \"value\", \"another_extremely_long_key_name_for_testing_purposes_and_validation\": \"another_value\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithNumericStringKeys_ShouldSucceed()
        {
            // Arrange
            var function = "handleNumericStringKeys";
            var args = "{\"123\": \"numeric_string_key\", \"456.789\": \"float_string_key\", \"-123\": \"negative_string_key\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithComplexFloats_ShouldSucceed()
        {
            // Arrange
            var function = "handleComplexNumbers";
            var args = "{\"pi\": 3.14159265359, \"e\": 2.71828182846, \"large\": 1.23e10, \"small\": 4.56e-8, \"negative\": -9.87654321}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithHexNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleHexNumbers";
            var args = "{\"color_red\": 0xFF0000, \"color_green\": 0x00FF00, \"color_blue\": 0x0000FF, \"mask\": 0xDEADBEEF}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithOctalNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleOctalNumbers";
            var args = "{\"permissions\": 0o755, \"mask\": 0o644, \"special\": 0o777}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithBinaryNumbers_ShouldSucceed()
        {
            // Arrange
            var function = "handleBinaryNumbers";
            var args = "{\"flags\": 0b1010, \"mask\": 0b1111, \"pattern\": 0b10101010}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithMixedQuoteStyles_ShouldSucceed()
        {
            // Arrange
            var function = "handleMixedQuotes";
            var args = "{\"double_quote_key\": \"double_quote_value\", 'single_quote_key': 'single_quote_value', \"mixed_key\": 'mixed_value', 'another_mixed': \"another_mixed_value\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_DictionaryWithTabsAndNewlines_ShouldSucceed()
        {
            // Arrange
            var function = "handleWhitespaceInStrings";
            var args = "{\"tab_content\": \"line1\\tline2\", \"newline_content\": \"line1\\nline2\", \"mixed_whitespace\": \"tab\\there\\nnewline\\rreturn\"}";

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Custom Test Cases

        [Test]
        public void ExecuteFunctionSecureNoFormat_RealWorldComplexDictionary_ShouldSucceed()
        {
            // Arrange - Real use case with complex dictionary structure
            var function = "whisper";
            var args = "{\"player\":Player(1), \"cards\":[Card(65565)], \"fromGroups\":[table], \"toGroups\":[table], \"indexs\":[3], \"xs\":[-560], \"ys\":[-295], \"highlights\":[\"#8a2be2\"], \"markers\":[\"{}\"], \"faceups\":[False], \"filters\":[\"None\"], \"alternates\":[\"\"]}";

            // Act & Assert - Should not throw for real-world dictionary usage
            Assert.DoesNotThrow(() => _engine.ExecuteFunctionSecureNoFormat(function, args));
        }

        #endregion

        #region Security Validation Tests

        [Test]
        public void ExecuteFunctionSecureNoFormat_WebFunctions_AreBlockedFromRemoteCall()
        {
            // This test verifies that the actual web functions are properly blocked when called via remoteCall mechanism
            var webFunctions = new[]
            {
                "webPost",
                "webRead"
            };

            foreach (var webFunction in webFunctions)
            {
                // Act & Assert - web functions should be blocked
                Assert.Throws<ScriptSecurityException>(() =>
                    _engine.ExecuteFunctionSecureNoFormat(webFunction, "\"http://example.com\""),
                    $"Web function '{webFunction}' should be blocked from remote execution");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_NonWebFunctions_AreAllowedFromRemoteCall()
        {
            // This test verifies that legitimate non-web functions still work
            var legitimateFunctions = new[]
            {
                "whisper",
                "notify", 
                "playSound",
                "drawCard",
                "setActivePlayer",
                "customGameFunction"
            };

            foreach (var function in legitimateFunctions)
            {
                // Act & Assert - legitimate functions should NOT throw
                Assert.DoesNotThrow(() =>
                    _engine.ExecuteFunctionSecureNoFormat(function, "Player(1)"),
                    $"Legitimate function '{function}' should not be blocked");
            }
        }

        [Test]
        public void ExecuteFunctionSecureNoFormat_WebPrefixedNonWebFunctions_AreAllowedFromRemoteCall()
        {
            // This test verifies that functions starting with "web" but not actual web functions are allowed
            // This ensures we're not being overly broad in our blocking
            var webPrefixedFunctions = new[]
            {
                "website",
                "webGet", 
                "webApi",
                "webSocket",
                "webCustomFunction"
            };

            foreach (var function in webPrefixedFunctions)
            {
                // Act & Assert - non-web functions starting with "web" should NOT throw
                Assert.DoesNotThrow(() =>
                    _engine.ExecuteFunctionSecureNoFormat(function, "\"test\""),
                    $"Function '{function}' starting with 'web' but not actual web function should not be blocked");
            }
        }

        #endregion
    }
}
