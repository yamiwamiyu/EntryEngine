using System;
using System.Collections.Generic;
using System.Linq;
using EntryEngine.Serialize;

namespace EntryEngine
{
    public static class _LANGUAGE
    {
        public class LANGUAGE
        {
            private string language;
            private Dictionary<string, string> current;

            public string Language
            {
                get { return language; }
                set
                {
                    if (!languages.ContainsKey(value)) throw new ArgumentException(string.Format("Language table don't contains language \"{0}\".", value));
                    if (language == value) return;
                    language = value;
                    current = languages[language];
                }
            }
            public string this[string key]
            {
                get { return current[key]; }
            }
            public string this[string key, params object[] parameters]
            {
                get { return string.Format(current[key], parameters); }
            }

            internal LANGUAGE(string language)
            {
                this.Language = language;
            }
            internal LANGUAGE(string language, Dictionary<string, string> current)
            {
                this.language = language;
                this.current = current;
            }

            public string GetString(string key)
            {
                return current[key];
            }
            public string GetString(string key, params object[] param)
            {
                return string.Format(current[key], param);
            }
        }

        private static string language;
        private static Dictionary<string, Dictionary<string, string>> languages = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> current;

        public static string Language
        {
            get { return language; }
            set
            {
                if (!languages.ContainsKey(value)) throw new ArgumentException(string.Format("Language table don't contains language \"{0}\".", value));
                if (language == value) return;
                language = value;
                current = languages[language];
            }
        }
        public static IEnumerable<string> Languages
        {
            get { return languages.Keys; }
        }

        public static void Load(string csvContent, string language)
        {
            bool all = string.IsNullOrEmpty(language);
            StringTable table = new CSVReader(csvContent).ReadTable();
            for (int i = 1; i < table.ColumnCount; i++)
            {
                string column = table.GetColumn(i);
                if (!all && column != language) continue;

                Dictionary<string, string> lt = new Dictionary<string, string>();
                for (int j = 0; j < table.RowCount; j++) lt.Add(table[0, j], table[i, j]);
                languages[column] = lt;
            }
            if (all)
                Language = languages.Keys.FirstOrDefault();
            else
                Language = language;
        }
        public static string GetString(string key)
        {
            return current[key];
        }
        public static string GetString(string lang, string key)
        {
            return languages[lang][key];
        }
        public static string GetString(string key, params object[] param)
        {
            return string.Format(current[key], param);
        }
        public static string GetString(string lang, string key, params object[] param)
        {
            return string.Format(languages[lang][key], param);
        }
        public static LANGUAGE GetLanguage()
        {
            return new LANGUAGE(language, current);
        }
        public static LANGUAGE GetLanguage(string language)
        {
            return new LANGUAGE(language);
        }
        public static void NextLanguage()
        {
            var temp = languages.Keys.ToArray();
            int index = temp.IndexOf(language);
            if (index == -1 || index == temp.Length - 1)
                index = 0;
            else
                index++;
            Language = temp[index];
        }
        public static void PreviousLanguage()
        {
            var temp = languages.Keys.ToArray();
            int index = temp.IndexOf(language);
            if (index == -1 || index == 0)
                index = temp.Length - 1;
            else
                index--;
            Language = temp[index];
        }
    }
}
