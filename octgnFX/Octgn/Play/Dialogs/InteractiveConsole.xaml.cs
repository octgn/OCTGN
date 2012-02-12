using System;
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
    public partial class InteractiveConsole
    {
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import] private Engine _scriptEngine;

#pragma warning restore 649

        private readonly ScriptScope _scope;

        public InteractiveConsole()
        {
            InitializeComponent();
            Program.Game.ComposeParts(this);
            _scope = _scriptEngine.CreateScope();

            Loaded += (s, a) => prompt.Focus();
        }

        private void PromptKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                var position = prompt.CaretIndex;
                prompt.Text = prompt.Text.Insert(position, "    ");
                prompt.CaretIndex = position + 4;
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter) return;
            e.Handled = true;

            var input = prompt.Text;

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
            var position = prompt.CaretIndex;
            var insert = "\n" + GetNewIndentation(prompt.Text, position);
            prompt.Text = prompt.Text.Insert(position, insert);
            prompt.CaretIndex = position + insert.Length;
        }

        private static string GetNewIndentation(string input, int position)
        {
            var indent = "    ";
            // Find the first indented line (if any), to find out how wide the indentation is (usually four spaces)
            var match = Regex.Match(input, "^\\s+", RegexOptions.Multiline);
            if (match.Success)
                indent = match.Value;

            // Find the indentation of the current line
            if (position < input.Length && input[position] == '\n') position--;
            var lineStart = input.LastIndexOf("\n", position, StringComparison.Ordinal) + 1;
            var curLine = input.Substring(lineStart, position - lineStart);
            var newIndent = "";
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