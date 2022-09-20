using System;
using System.Collections.Generic;
using MonoMod.ModInterop;
using RandomizerMod.Logging;

namespace MoreDoors.Imports
{
    internal static class CondensedSpoilerLogger
    {
        [ModImportName("CondensedSpoilerLogger")]
        private static class CondensedSpoilerLoggerImport
        {
            public static Action<string, Func<LogArguments, bool>, List<string>> AddCategory = null;
        }
        static CondensedSpoilerLogger()
        {
            typeof(CondensedSpoilerLoggerImport).ModInterop();
        }
        public static void AddCategory(string categoryName, Func<LogArguments, bool> test, List<string> entries)
            => CondensedSpoilerLoggerImport.AddCategory?.Invoke(categoryName, test, entries);
    }
}