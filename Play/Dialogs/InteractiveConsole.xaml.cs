using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Scripting.Hosting;

namespace Octgn.Play.Dialogs
{
  public partial class InteractiveConsole : Window
  {
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

    [Import]
    private Scripting.Engine scriptEngine;
    
#pragma warning restore 649

    private ScriptScope scope;

    public InteractiveConsole()
    {
      InitializeComponent();
      Program.Game.ComposeParts(this);      
      scope = scriptEngine.CreateScope();

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

      if (e.Key != Key.Enter) return;
      e.Handled = true;

      string input = prompt.Text;

      if (input.EndsWith("\\")) // \ is the line continuation character in Python
      { PromptNewIndentedLine(); return; }

      if (string.IsNullOrWhiteSpace(input))
      { prompt.Text = ""; return; }

      if (prompt.CaretIndex < input.Length) // Don't try to execute the script if the caret isn't at the end of the input
      { PromptNewIndentedLine(); return; }

      prompt.IsEnabled = false;
      if (!scriptEngine.TryExecuteInteractiveCode(input, scope, ExecutionCompleted))
      {
        prompt.IsEnabled = true;
        PromptNewIndentedLine();
      }
    }

    private void ExecutionCompleted(Scripting.ExecutionResult result)
    {      
      PrintCommand(prompt.Text);      
      PrintResult(result.Output);
      PrintError(result.Error);
      prompt.Text = "";
      prompt.IsEnabled = true;
    }

    private readonly static string[] terminators = new string[]
    { "pass", "break", "continue", "return" };

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
      var match = Regex.Match(input, "^\\s+", RegexOptions.Multiline);
      if (match.Success)
        indent = match.Value;

      // Find the indentation of the current line
      if (position < input.Length && input[position] == '\n') position--;
      int lineStart = input.LastIndexOf("\n", position) + 1;
      string curLine = input.Substring(lineStart, position - lineStart);
      string newIndent = "";
      match = Regex.Match(curLine, "^\\s+");
      if (match.Success) newIndent = match.Value;

      // Check if the indentation shoud be modified
      curLine = curLine.Trim();
      if (curLine.EndsWith(":")) newIndent += indent;
      else if (terminators.Any(t => curLine.StartsWith(t)))
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
      results.Inlines.Add(new Run("\n" + text.Trim()) { Foreground = Brushes.DarkBlue });
      scroller.ScrollToBottom();
    }

    private void PrintError(string text)
    {
      if (string.IsNullOrWhiteSpace(text)) return;
      results.Inlines.Add(new Run("\n" + text) { Foreground = Brushes.DarkRed });
      scroller.ScrollToBottom();
    }
  }
}
