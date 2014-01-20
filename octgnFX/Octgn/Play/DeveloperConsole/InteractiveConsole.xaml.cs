namespace Octgn.Play.DeveloperConsole
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    using Microsoft.Scripting.Hosting;

    using Octgn.Core;
    using Octgn.Extentions;
    using Octgn.Scripting;

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
            this.InitializeComponent();
            if (this.IsInDesignMode()) return;
            Program.GameEngine.ComposeParts(this);
            this._scope = this._scriptEngine.CreateScope(Path.Combine(Prefs.DataDirectory, "GameDatabase", Program.GameEngine.Definition.Id.ToString()));

            this.Loaded += (s, a) => this.prompt.Focus();
        }

        private void PromptKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                int position = this.prompt.CaretIndex;
                this.prompt.Text = this.prompt.Text.Insert(position, "    ");
                this.prompt.CaretIndex = position + 4;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (e.Key == Key.Up && this._commandHistoryLoc != 0)
                {
                    this._commandHistoryLoc -= 1;
                    this.prompt.Text = this._commandHistory[this._commandHistoryLoc];
                    this.prompt.CaretIndex = this.prompt.Text.Length;
                }
                else if (e.Key == Key.Down && this._commandHistoryLoc < this._commandHistory.Count - 1 && this._commandHistory.Count != 0)
                {
                    this._commandHistoryLoc += 1;
                    this.prompt.Text = this._commandHistory[this._commandHistoryLoc];
                    this.prompt.CaretIndex = this.prompt.Text.Length;
                }
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter) return;
            e.Handled = true;

            string input = this.prompt.Text;

            if (input.EndsWith("\\")) // \ is the line continuation character in Python
            {
                this.PromptNewIndentedLine();
                return;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                this.prompt.Text = "";
                return;
            }

            if (this.prompt.CaretIndex < input.Length)
                // Don't try to execute the script if the caret isn't at the end of the input
            {
                this.PromptNewIndentedLine();
                return;
            }

            this._commandHistory.Add(input);
            this._commandHistoryLoc = this._commandHistory.Count;
            this.prompt.IsEnabled = false;
            if (this._scriptEngine.TryExecuteInteractiveCode(input, this._scope, this.ExecutionCompleted)) return;
            this.prompt.IsEnabled = true;
            this.PromptNewIndentedLine();
        }

        private void ExecutionCompleted(ExecutionResult result)
        {
            this.PrintCommand(this.prompt.Text);
            this.PrintResult(result.Output);
            this.PrintError(result.Error);
            this.prompt.Text = "";
            this.prompt.IsEnabled = true;
        }

        private static readonly string[] Terminators = new[] {"pass", "break", "continue", "return"};

        private void PromptNewIndentedLine()
        {
            int position = this.prompt.CaretIndex;
            string insert = "\n" + GetNewIndentation(this.prompt.Text, position);
            this.prompt.Text = this.prompt.Text.Insert(position, insert);
            this.prompt.CaretIndex = position + insert.Length;
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
            this.results.Inlines.Add(new Run("\n>>> " + text));
            this.scroller.ScrollToBottom();
        }

        private void PrintResult(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            this.results.Inlines.Add(new Run("\n" + text.Trim()) {Foreground = Brushes.DarkBlue});
            this.scroller.ScrollToBottom();
        }

        private void PrintError(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            this.results.Inlines.Add(new Run("\n" + text) {Foreground = Brushes.DarkRed});
            this.scroller.ScrollToBottom();
        }
    }
}