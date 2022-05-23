using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cofe.Core.Utils
{
    public interface IParamParser : ICofeService
    {
        /// <summary>
        /// Uses to serialize the properties to string.
        /// </summary>
        IPropertySerializer Serializer { get; }

        /// <summary>
        /// Convert a dictionary contining key value pairs to ParamString.
        /// </summary>
        /// <param name="param">An array of key and value pairs</param>
        /// <returns>Param String containing both key and value, e.g. key:"value"</returns>
        string DictionaryToString(Dictionary<string, string> paramDic);

        /// <summary>
        /// Take a Paramstring and return a dictionary containing 
        /// the paramstring's key and value.
        /// </summary>
        /// <param name="input"></param>        
        /// <param name="paramDic"></param>        
        Dictionary<string, string> StringToDictionary(string paramString);
    }

}
