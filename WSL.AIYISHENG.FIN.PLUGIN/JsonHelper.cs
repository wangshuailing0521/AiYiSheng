using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSL.AIYISHENG.FIN.PLUGIN
{
	public static class JsonHelper
	{
		// Token: 0x06000039 RID: 57 RVA: 0x00002D28 File Offset: 0x00000F28
		public static string ToJSON(this object o)
		{
			bool flag = o == null;
			string result;
			if (flag)
			{
				result = null;
			}
			else
			{
				result = JsonConvert.SerializeObject(o);
			}
			return result;
		}

        // Token: 0x0600003A RID: 58 RVA: 0x00002D50 File Offset: 0x00000F50
        public static T FromJSON<T>(this string input)
        {
            T result;

            result = JsonConvert.DeserializeObject<T>(input);

            return result;
        }
    }
}
