using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using TC_WinForms.WinForms.Diagram;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace TC_WinForms.Services
{
    public static class HighlightTextService
    {
        // Основной метод обновления содержимого
        public static void UpdateRichTextBoxContent(string text, RichTextBox textBox)
        {
            string currentText = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text;
            if (currentText == text) return;

            string[] phrases = { "Дополнение:", "ВАЖНО:" };
            var isNeedFormationg = phrases.Any(phrase => text.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0);

            if (isNeedFormationg)
            {
                ApplyFormattedText(text, textBox);
            }
            else
            {
                TextRange range = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
                range.Text = text;
            }
        }
        // Вспомогательные методы

        private static void ApplyFormattedText(string text, RichTextBox textBox)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph { TextAlignment = TextAlignment.Left, Margin = new Thickness(0) };

            string[] phrases = { "Дополнение:", "ВАЖНО:" };
            int startIndex = 0;

            while (startIndex < text.Length)
            {
                var foundPhrase = FindNextPhrase(text, startIndex, phrases);

                if (foundPhrase.Index != -1)
                {
                    if (foundPhrase.Index > startIndex)
                    {
                        paragraph.Inlines.Add(new Run(text.Substring(startIndex, foundPhrase.Index - startIndex)));
                    }
                    paragraph.Inlines.Add(new Bold(new Run(foundPhrase.Phrase)));
                    startIndex = foundPhrase.Index + foundPhrase.Phrase.Length;
                }
                else
                {
                    paragraph.Inlines.Add(new Run(text.Substring(startIndex)));
                    break;
                }
            }

            doc.Blocks.Add(paragraph);
            textBox.Document = doc;
        }

        private static (string Phrase, int Index) FindNextPhrase(string text, int startIndex, string[] phrases)
        {
            int minIndex = int.MaxValue;
            string foundPhrase = null;

            foreach (var phrase in phrases)
            {
                int index = text.IndexOf(phrase, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index >= 0 && index < minIndex)
                {
                    minIndex = index;
                    foundPhrase = phrase;
                }
            }

            return (foundPhrase, minIndex != int.MaxValue ? minIndex : -1);
        }

        //private static void RestoreCaretPosition((int Offset, bool WasAtEnd) caretInfo, RichTextBox textBox)
        //{
        //    if (caretInfo.WasAtEnd)
        //    {
        //        textBox.CaretPosition = textBox.Document.ContentEnd;
        //    }
        //    else
        //    {
        //        RestoreCaretPosition(caretInfo.Offset, textBox);
        //    }
        //}

        //public static void ApplyFormattingWithCaretPreservation(RichTextBox textBox, bool _isUpdatingFromCode, string _description, string Description, DiagramState _diagramState, Action<string> onPropertyChanged)
        //{

        //    string text = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text;
        //    if (string.IsNullOrEmpty(text)) return;

        //    _isUpdatingFromCode = true;
        //    try
        //    {
        //        // Сохраняем позицию каретки
        //        var caretInfo = (GetCaretOffset(textBox.CaretPosition, textBox),
        //            textBox.CaretPosition.CompareTo(textBox.Document.ContentEnd) == 0);

        //        // Обновляем содержимое
        //        UpdateRichTextBoxContent(text, textBox);

        //        // Восстанавливаем позицию
        //        RestoreCaretPosition(caretInfo, textBox);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error applying formatting: {ex.Message}");
        //        textBox.Document = new FlowDocument(new Paragraph(new Run(text)));
        //        textBox.CaretPosition = textBox.Document.ContentEnd;
        //    }
        //    finally
        //    {
        //        _isUpdatingFromCode = false;
        //    }

        //    if (_description != text)
        //    {
        //        _description = text;

        //        onPropertyChanged(nameof(Description));

        //        Description = text;
        //        _diagramState?.HasChanges();
        //    }
        //}
        //// Методы для работы с позицией каретки (оставляем без изменений)
        //private static int GetCaretOffset(TextPointer caretPosition, RichTextBox textBox)
        //{
        //    if (caretPosition == null || textBox.Document == null)
        //        return 0;

        //    TextPointer navigator = textBox.Document.ContentStart;
        //    int offset = 0;

        //    while (navigator != null && navigator.CompareTo(caretPosition) < 0)
        //    {
        //        if (navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
        //        {
        //            string textRun = navigator.GetTextInRun(LogicalDirection.Forward);
        //            if (textRun.Length > 0)
        //            {
        //                int charsToCaret = Math.Min(textRun.Length,
        //                    caretPosition.GetOffsetToPosition(navigator) <= 0 ? textRun.Length : caretPosition.GetOffsetToPosition(navigator));
        //                offset += charsToCaret;
        //                navigator = navigator.GetPositionAtOffset(charsToCaret);
        //            }
        //            else
        //            {
        //                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
        //            }
        //        }
        //        else
        //        {
        //            navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
        //        }
        //    }

        //    return offset;
        //}

        //private static void RestoreCaretPosition(int offset, RichTextBox textBox)
        //{
        //    if (textBox.Document == null || offset < 0)
        //        return;

        //    TextPointer navigator = textBox.Document.ContentStart;
        //    int currentOffset = 0;

        //    while (navigator != null && currentOffset < offset)
        //    {
        //        if (navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
        //        {
        //            string textRun = navigator.GetTextInRun(LogicalDirection.Forward);
        //            int charsToMove = Math.Min(textRun.Length, offset - currentOffset);
        //            currentOffset += charsToMove;
        //            navigator = navigator.GetPositionAtOffset(charsToMove);
        //        }
        //        else
        //        {
        //            navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
        //        }
        //    }

        //    textBox.CaretPosition = navigator ?? textBox.Document.ContentEnd;
        //}
    }
}
