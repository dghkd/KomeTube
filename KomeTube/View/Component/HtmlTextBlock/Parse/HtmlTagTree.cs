using System;
using System.Collections.Generic;

namespace HtmlTextBlock
{
    /// <summary>
    /// Represent owner of all HtmlTag.
    /// </summary>
    public class HtmlTagTree : HtmlTagNode
    {
        public HtmlTagTree() : base(null, null)
        {
            isRoot = true;
            tag = new HtmlTag("master", "");
        }

        public override bool CanAdd(HtmlTag aTag)
        {
            return true;
        }

        private static string printNode(Int32 level, HtmlTagNode node)
        {
            string spacing = " ";
            string retVal = "";
            for (int i = 0; i < level; i++)
                spacing += "  ";

            retVal += spacing + node.ToString() + '\r' + '\n';

            foreach (HtmlTagNode subnode in node)
                retVal += HtmlTagTree.printNode(level + 1, subnode);

            return retVal;
        }

        public override string ToString()
        {
            string retVal = "";

            foreach (HtmlTagNode subnode in this)
                retVal += HtmlTagTree.printNode(0, subnode);

            return retVal;
        }
    }
}