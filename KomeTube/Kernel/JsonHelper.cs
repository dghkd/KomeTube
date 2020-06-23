using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace KomeTube.Kernel
{
    public class JsonHelper
    {
        /// <summary>
        /// Try to get the json data value and not throw exception when there is not the key. Return default value  if there is not the key.
        /// </summary>
        /// <param name="jsonData">Raw json data.</param>
        /// <param name="key">Try string of json data key.</param>
        /// <param name="defaultValue">If there is not the key, return this value. </param>
        /// <returns>Return default value if there is not the key.</returns>
        public static object TryGetValue(dynamic jsonData, String key, object defaultValue = null)
        {
            object ret;
            try
            {
                ret = jsonData[key];
                if (ret == null)
                {
                    ret = defaultValue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Try get value error:{0}", ex.Message));
                return defaultValue;
            }
            return ret;
        }

        /// <summary>
        /// Try to get the json data value and not throw exception when there is not the key. Return default value  if there is not the key.
        /// </summary>
        /// <param name="jsonData">Raw json data.</param>
        /// <param name="idx">Try index of json data array.</param>
        /// <param name="defaultValue">If there is not the key, return this value. </param>
        /// <returns>Return default value if there is not the key.</returns>
        public static object TryGetValue(dynamic jsonData, int idx, object defaultValue = null)
        {
            object ret;
            try
            {
                ret = jsonData[idx];
                if (ret == null)
                {
                    ret = defaultValue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Try get value error:{0}", ex.Message));
                return defaultValue;
            }
            return ret;
        }

        public static object TryGetValueByXPath(dynamic jsonData, String xPath, object defaultValue = null)
        {
            object ret = jsonData;
            String[] keys = xPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (String k in keys)
            {
                int idx = -1;
                if (Int32.TryParse(k, out idx))
                {
                    ret = TryGetValue(ret, idx);
                }
                else
                {
                    ret = TryGetValue(ret, k);
                }

                if (ret == null)
                {
                    ret = defaultValue;
                    break;
                }
            }
            return ret;
        }
    }
}