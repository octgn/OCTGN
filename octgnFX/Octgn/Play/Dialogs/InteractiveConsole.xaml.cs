using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Scripting.Hosting;
using Octgn.Scripting;

namespace Octgn.Play.Dialogs
{
    using System.IO;

    using Octgn.Core;

    public partial class InteractiveConsole
    {
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import] private Engine _scriptEngine;

#pragma warning restore 649

        private readonly ScriptScope _scope;
        private readonly List<string> _commandHistory = new List<string>();
        private int _commandHistoryLoc = 0;

        public InteractiveConsole()
        {
            InitializeComponent();
            Program.GameEngine.ComposeParts(this);
            _scope = _scriptEngine.CreateScope(Path.Combine(Prefs.DataDirectory, "GameDatabase", Program.GameEngine.Definition.Id.ToString()));

            Loaded += (s, a) => prompt.Focus();
        }

        private void PromptKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                int position = prompt.CaretIndex;
                prompt.Text = prompt.Text.Insert(position, "    ");
                prompt.CaretIndex = position + 4;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (e.Key == Key.Up && _commandHistoryLoc != 0)
                {
                    _commandHistoryLoc -= 1;
                    prompt.Text = _commandHistory[_commandHistoryLoc];
                    prompt.CaretIndex = prompt.Text.Length;
                }
                else if (e.Key == Key.Down && _commandHistoryLoc < _commandHistory.Count - 1 && _commandHistory.Count != 0)
                {
                    _commandHistoryLoc += 1;
                    prompt.Text = _commandHistory[_commandHistoryLoc];
                    prompt.CaretIndex = prompt.Text.Length;
                }
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter) return;
            e.Handled = true;

            string input = prompt.Text;

            if (input.EndsWith("\\")) // \ is the line continuation character in Python
            {
                PromptNewIndentedLine();
                return;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                prompt.Text = "";
                return;
            }

            if (prompt.CaretIndex < input.Length)
                // Don't try to execute the script if the caret isn't at the end of the input
            {
                PromptNewIndentedLine();
                return;
            }

            _commandHistory.Add(input);
            _commandHistoryLoc = _commandHistory.Count;
            prompt.IsEnabled = false;
            if (_scriptEngine.TryExecuteInteractiveCode(input, _scope, ExecutionCompleted)) return;
            prompt.IsEnabled = true;
            PromptNewIndentedLine();
        }

        private void ExecutionCompleted(ExecutionResult result)
        {
            PrintCommand(prompt.Text);
            PrintResult(result.Output);
            PrintError(result.Error);
            prompt.Text = "";
            prompt.IsEnabled = true;
        }

        private static readonly string[] Terminators = new[] {"pass", "break", "continue", "return"};

        private void PromptNewIndentedLine()
        {
            int position = prompt.CaretIndex;
            string insert = "\n" + GetNewIndentation(prompt.Text, position);
            prompt.Text = prompt.Text.Insert(position, insert);
            prompt.CaretIndex = position + insert.Length;
        }

        private static string GetNewIndentation(string input, int position)
        {
            string indent = "    ";
            // Find the first indented line (if any), to find out how wide the indentation is (usually four spaces)
            Match match = Regex.Match(input, "^\\s+", RegexOptions.Multiline);
            if (match.Success)
                indent = match.Value;

            // Find the indentation of the current line
            if (position < input.Length && input[position] == '\n') position--;
            int lineStart = input.LastIndexOf("\n", position, StringComparison.Ordinal) + 1;
            string curLine = input.Substring(lineStart, position - lineStart);
            string newIndent = "";
            match = Regex.Match(curLine, "^\\s+");
            if (match.Success) newIndent = match.Value;

            // Check if the indentation shoud be modified
            curLine = curLine.Trim();
            if (curLine.EndsWith(":")) newIndent += indent;
            else if (Terminators.Any(t => curLine.StartsWith(t)))
                newIndent = new string(' ', Math.Max(0, newIndent.Length - indent.Length));

            return newIndent;
        }

        private void PrintCommand(string text)
        {
            text = text.Trim().Replace("\n", "\n... ");
            results.Inlines.Add(new Run("\n>>> " + text));
            scroller.ScrollToBottom();
        }

        private void PrintResult(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            results.Inlines.Add(new Run("\n" + text.Trim()) {Foreground = Brushes.DarkBlue});
            scroller.ScrollToBottom();
        }

        private void PrintError(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            results.Inlines.Add(new Run("\n" + text) {Foreground = Brushes.DarkRed});
            scroller.ScrollToBottom();
        }
    }
}