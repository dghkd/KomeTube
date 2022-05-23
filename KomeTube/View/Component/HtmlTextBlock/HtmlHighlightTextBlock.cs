using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using HtmlTextBlock;
using System.IO;

namespace HtmlTextBlock
{
    public class HtmlHighlightTextBlock : TextBlock
    {
        public string Highlight
        {
            get { return (string)GetValue(HighlightProperty); }
            set { SetValue(HighlightProperty, value); }
        }


        public static readonly DependencyProperty HighlightProperty =
        DependencyProperty.Register("Highlight", typeof(string), typeof(HtmlHighlightTextBlock), new UIPropertyMetadata(""));


        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(string),
                typeof(HtmlHighlightTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(OnHtmlChanged)));

        public string Html { get { return (string)GetValue(HtmlProperty); } set { SetValue(HtmlProperty, value); } }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        private void Parse(string html)
        {
            if (!String.IsNullOrEmpty(Highlight))
            {
                int idx = html.IndexOf(Highlight, StringComparison.InvariantCultureIgnoreCase);
                while (idx != -1)
                {
                    html = String.Format("{0}[b]{1}[/b]{2}",
                        html.Substring(0, idx), html.Substring(idx, Highlight.Length), html.Substring(idx + Highlight.Length));
                    idx = html.IndexOf(Highlight, idx + 7 + Highlight.Length, StringComparison.InvariantCultureIgnoreCase);
                }
            }                

            Inlines.Clear();
            HtmlTagTree tree = new HtmlTagTree();
            HtmlParser parser = new HtmlParser(tree); //output
            parser.Parse(new StringReader(html));     //input

            HtmlUpdater updater = new HtmlUpdater(this); //output
            updater.Update(tree);
        }

        public static void OnHtmlChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlHighlightTextBlock sender = (HtmlHighlightTextBlock)s;
            sender.Parse((string)e.NewValue);
        }

        public HtmlHighlightTextBlock()
        {

        }

    }
}
