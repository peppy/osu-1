namespace Unosquare.Labs.LiteLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        private const string integer_affinity = "INTEGER";
        private const string numeric_affinity = "NUMERIC";
        private const string real_affinity = "REAL";
        private const string text_affinity = "TEXT";
        private const string date_time_affinity = "DATETIME";

        private static readonly Dictionary<Type, string> type_mappings = new Dictionary<Type, string>
        {
            {typeof(short), integer_affinity},
            {typeof(int), integer_affinity},
            {typeof(long), integer_affinity},
            {typeof(ushort), integer_affinity},
            {typeof(uint), integer_affinity},
            {typeof(ulong), integer_affinity},
            {typeof(byte), integer_affinity},
            {typeof(char), integer_affinity},
            {typeof(decimal), real_affinity},
            {typeof(bool), numeric_affinity},
            {typeof(DateTime), date_time_affinity},
        };

        /// <summary>
        /// Gets the type mapping.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns>A property type of the mapping.</returns>
        public static string GetTypeMapping(this Type propertyType) => type_mappings.ContainsKey(propertyType) ? type_mappings[propertyType] : text_affinity;

        /// <summary>
        /// Transform a DateTime to a SQLite UTC date.
        /// </summary>
        /// <param name="utcDate">The UTC date.</param>
        /// <returns>UTC DateTime.</returns>
        public static DateTime ToSqLiteUtcDate(this DateTime utcDate)
        {
            var startupDifference = (int)DateTime.UtcNow.Subtract(DateTime.Now).TotalHours;
            return utcDate.AddHours(startupDifference);
        }
    }
}
