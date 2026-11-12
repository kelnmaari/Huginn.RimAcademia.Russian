using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Huginn.RimAcademia.Russian
{
    [StaticConstructorOnStartup]
    internal static class RussianLocalization
    {
        private const string PatchId = "huginn.codex.childprotection";
        private static readonly Dictionary<string, string> Keys;

        static RussianLocalization()
        {
            var localization = AccessTools.TypeByName("ChildProtection.L");
            if (localization == null)
                return;

            var method = AccessTools.Method(localization, "Text", new[] { typeof(string) });
            var field = AccessTools.Field(localization, "keys");
            Keys = field == null ? null : field.GetValue(null) as Dictionary<string, string>;
            if (method == null || Keys == null)
            {
                Log.Warning("[Rim Academia RU] The original localization hook could not be found.");
                return;
            }

            var prefix = typeof(RussianLocalization).GetMethod("TranslateRussian", BindingFlags.NonPublic | BindingFlags.Static);
            new Harmony(PatchId).Patch(method, prefix: new HarmonyMethod(prefix));
        }

        private static bool TranslateRussian(string __0, ref string __result)
        {
            var language = LanguageDatabase.activeLanguage;
            if (language == null || string.IsNullOrEmpty(__0) ||
                !(string.Equals(language.folderName, "Russian", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(language.folderName, "Russian (Русский)", StringComparison.OrdinalIgnoreCase)))
                return true;

            // The original uses a Korean subject particle to join some sentence fragments.
            if (__0 == "이 ")
            {
                __result = " ";
                return false;
            }

            string key;
            if (!Keys.TryGetValue(__0, out key) || !key.CanTranslate())
                return true;

            __result = key.Translate().ToString();
            return false;
        }
    }
}