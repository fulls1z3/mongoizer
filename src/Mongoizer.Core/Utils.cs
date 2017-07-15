using System;
using System.Collections;

namespace Mongoizer.Core {
    internal static class Utils {
        public const string ARGUMENT_NULL_MESSAGE = "The argument '{0}' cannot be null.";
        public const string ARGUMENT_EMPTY_MESSAGE = "The argument '{0}' cannot be null or all whitespace.";
        public const string ARGUMENT_EMPTY_LIST_MESSAGE = "The argument '{0}' cannot be null and must have at least one item.";

        public static bool HasItems(this IEnumerable source) => source != null && source.GetEnumerator().MoveNext();
    }

    public static class Extensions {
        public static bool EqualsMongoDateTime(this DateTime source, DateTime value) => Math.Abs((source - value).TotalMilliseconds) < 1000;
    }
}
