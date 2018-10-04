using System.Collections.Generic;
using Newtonsoft.Json;

namespace Manisero.Navvy.Reporting.Utils
{
    internal static class ObjectUtils
    {
        public static IEnumerable<TValue> ToEnumerable<TValue>(
            this TValue value)
        {
            yield return value;
        }

        public static string ToJson(
            this object value)
            => JsonConvert.SerializeObject(value);
    }
}
