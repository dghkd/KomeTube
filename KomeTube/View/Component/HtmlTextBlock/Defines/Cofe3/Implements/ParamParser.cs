using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cofe.Core.Utils
{
    //[Export(typeof(ICofeService))]
    //[ServicePriority(ServicePriorityAttribute.DefaultPriority_COFE)]
    public class ParamParser : IParamParser
    {
        #region Constructor

        public ParamParser(IPropertySerializer serializer)
        {
            Serializer = serializer;
        }

        //public ParamParser()
        //    : this(new ParamStringSerializer(false))
        //{
        //}

        #endregion Constructor

        #region Methods

        public string DictionaryToString(Dictionary<string, string> paramDic)
        {
            return Serializer.PropertyToString(
                from p in paramDic.Keys
                select new Tuple<string, string>(p, paramDic[p])
                );
        }

        public Dictionary<string, string> StringToDictionary(string paramString)
        {
            Dictionary<string, string> retDic = new Dictionary<string, string>();

            foreach (var tup in Serializer.StringToProperty(paramString))
                if (!retDic.ContainsKey(tup.Item1))
                    retDic.Add(tup.Item1, tup.Item2);
                else retDic[tup.Item1] = tup.Item2;

            return retDic;
        }

        #endregion Methods

        #region Data

        #endregion Data



        #region Public Properties

        public IPropertySerializer Serializer { get; private set; }

        #endregion Public Properties
    }
}