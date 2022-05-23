using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cofe.Core.Utils
{
    public class HtmlAttributeStringSerializer : IPropertySerializer
    {
        #region Constructor

        #endregion Constructor

        #region Methods

        public string PropertyToString(IEnumerable<Tuple<string, string>> properties)
        {
            string retVal = "";
            foreach (var prop in properties)
                retVal += String.Format(" {0}=\"{1}\"", prop.Item1, prop.Item2);
            return retVal;
        }

        private static char quote = '\'';

        private static void locateNextVariable(ref string working, ref string varName, ref string varValue)
        {
            working = working.Trim();

            Int32 pos1 = working.IndexOf('=');
            if (pos1 != -1)
            {
                varName = working.Substring(0, pos1);
                Int32 j = working.IndexOf(quote);
                Int32 f1 = working.IndexOf(' ');
                Int32 f2 = working.IndexOf('=');
                if (f1 == -1) { f1 = f2 + 1; }

                if ((j == -1) || (j > f1))
                {
                    varValue = working.Substring(f2 + 1, working.Length - f2 - 1);
                    f1 = working.IndexOf(' ');
                    if (f1 == -1)
                    {
                        working = "";
                    }
                    else
                    {
                        working = working.Substring(f1 + 1, working.Length - f1 - 1);
                    }
                }
                else
                {
                    working = working.Substring(j + 1, working.Length - j - 1);
                    j = working.IndexOf(quote);
                    if (j != -1)
                    {
                        varValue = working.Substring(0, j);
                        working = working.Substring(j + 1, working.Length - j - 1);
                    }
                }
            }
            else
            {
                varName = working;
                varValue = "TRUE";
                working = "";
            }
        }

        public IEnumerable<Tuple<string, string>> StringToProperty(string propertyString)
        {
            string working = propertyString;
            string varName = "", varValue = "";
            while (working != "")
            {
                locateNextVariable(ref working, ref varName, ref varValue);
                yield return new Tuple<string, string>(varName, varValue);
            }
        }

        #endregion Methods

        #region Data

        #endregion Data

        #region Public Properties

        #endregion Public Properties
    }
}