using Cofe.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HtmlTextBlock
{
    /// <summary>
    /// Represent an element Tag in Html code
    /// </summary>
    public class HtmlTag
    {
        private static IParamParser HtmlAttributeParser = new ParamParser(new HtmlAttributeStringSerializer());

        private string name;                                     //HtmlTag name without <>
        private Dictionary<string, string> variables = new Dictionary<string, string>();     //Variable List and values

        ///<summary> Gets HtmlTag ID in BuiltInTags. (without <>) </summary>
        internal int ID
        { get { return Defines.BuiltinTags.ToList().FindIndex(tagInfo => tagInfo.Html.Equals(name.TrimStart('/'))); } }

        ///<summary> Gets HtmlTag Level in BuiltInTags. (without <>) </summary>
        internal Int32 Level
        { get { if (ID == -1) return 0; else return Defines.BuiltinTags[ID].tagLevel; } }

        internal bool IsEndTag
        { get { return ((name.IndexOf('/') == 0) || (variables.ContainsKey("/"))); } }

        ///<summary> Gets HtmlTag name. (without <>) </summary>
        public string Name
        { get { return name; } }

        ///<summary> Gets variable value. </summary>
        public string this[string key]
        { get { return variables[key]; } }

        ///<summary> Gets whether variable list contains the specified key. </summary>
        public bool Contains(string key)
        { return variables.ContainsKey(key); }

        ///<summary> Returns the string representation of the value of this instance.  </summary>
		public override string ToString()
        {
            return String.Format("<{0}> : {1}", name, variables.ToString());
        }

        /// <summary>
        /// Initialite procedure, can be used by child tags.
        /// </summary>
        protected void init(string aName, Dictionary<string, string> aVariables)
        {
            name = aName.ToLower();
            if (aVariables == null)
                variables = new Dictionary<string, string>();
            else
                variables = aVariables;
        }

        ///<summary> Constructor. </summary>
        public HtmlTag(string aName, string aVarString)
        {
            init(aName, HtmlAttributeParser.StringToDictionary(aVarString));
        }

        public HtmlTag(string aText)
        {
            Dictionary<string, string> aList = new Dictionary<string, string>();
            aList.Add("value", aText);
            init("text", aList);
        }
    }
}