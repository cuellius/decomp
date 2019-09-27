using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;
using Decomp.Windows.HtmlConverter;

namespace Decomp.Windows
{
    public static class ControlsMethods
    {
        public static void Refresh(this UIElement uiElement) => uiElement.Dispatcher?.Invoke(DispatcherPriority.Render, new Action(delegate { }));

        public static void SetContent(this ContentControl button, object content) => button.Dispatcher?.Invoke(() => button.Content = content);

        public static void SetText(this TextBlock textBlock, string text) => textBlock.Dispatcher?.Invoke(() => textBlock.Text = text);
        
        public static bool IsChecked(this ToggleButton toogleButton)
        {
            var @checked = false;
            toogleButton.Dispatcher?.Invoke(() => @checked = toogleButton.IsChecked ?? false);
            return @checked;
        }

        public static void SetText(this RichTextBox t, string text)
        {
            t.Document.Blocks.Clear();
            t.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
        
        public static void SetHtml(this RichTextBox t, string html)
        {
            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, true);
            var flowDocument = XamlReader.Parse(xaml) as FlowDocument;
            flowDocument.HyperlinksSubscriptions();
            t.Document = flowDocument;
        }

        private static void HyperlinksSubscriptions(this DependencyObject flowDocument)
        {
            if (flowDocument == null) return;
            var hyperLinks = GetVisualChildren(flowDocument).OfType<Hyperlink>().ToList();
            foreach (var hyperlink in hyperLinks)
            {
                hyperlink.IsEnabled = true;
                hyperlink.RequestNavigate += (s, e) =>
                {
                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                    e.Handled = true;
                };
            }
        }

        private static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisualChildren(child)) yield return descendants;
            }
        }
    }
}
