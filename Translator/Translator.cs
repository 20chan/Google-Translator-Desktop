// Copyright (c) 2015 Ravi Bhavnani
// License: Code Project Open License
// http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace RavSoft.GoogleTranslator
{
    /// <summary>
    /// Translates text using Google's online language tools.
    /// </summary>
    public class GTranslator
    {
        #region Properties

            /// <summary>
            /// Gets the supported languages.
            /// </summary>
            public static IEnumerable<string> Languages {
                get {
                    GTranslator.EnsureInitialized();
                    return GTranslator._languageModeMap.Keys.OrderBy (p => p);
                }
            }

            /// <summary>
            /// Gets the time taken to perform the translation.
            /// </summary>
            public TimeSpan TranslationTime {
                get;
                private set;
            }

            /// <summary>
            /// Gets the url used to speak the translation.
            /// </summary>
            /// <value>The url used to speak the translation.</value>
            public string TranslationSpeechUrl {
                get;
                private set;
            }

            /// <summary>
            /// Gets the error.
            /// </summary>
            public Exception Error {
                get;
                private set;
            }

        #endregion

        #region Public methods

            /// <summary>
            /// Translates the specified source text.
            /// </summary>
            /// <param name="sourceText">The source text.</param>
            /// <param name="sourceLanguage">The source language.</param>
            /// <param name="targetLanguage">The target language.</param>
            /// <returns>The translation.</returns>
            public string Translate
                (string sourceText,
                 string sourceLanguage,
                 string targetLanguage)
            {
                // Initialize
                this.Error = null;
                this.TranslationSpeechUrl = null;
                this.TranslationTime = TimeSpan.Zero;
                DateTime tmStart = DateTime.Now;
                string translation = string.Empty;

                try {
                    // Download translation
                    string url = string.Format ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
                                                GTranslator.LanguageEnumToIdentifier (sourceLanguage),
                                                GTranslator.LanguageEnumToIdentifier (targetLanguage),
                                                HttpUtility.UrlEncode (sourceText));
                    string outputFile = Path.GetTempFileName();
                    using (WebClient wc = new WebClient ()) {
                        wc.Headers.Add ("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                        wc.DownloadFile(url, outputFile);
                    }

                    // Get translated text
                    if (File.Exists (outputFile)) {

                        // Get phrase collection
                        string text = File.ReadAllText(outputFile);
                        int index = text.IndexOf (string.Format(",,\"{0}\"", GTranslator.LanguageEnumToIdentifier (sourceLanguage)));
                        if (index == -1) {
                            // Translation of single word
                            int startQuote = text.IndexOf('\"');
                            if (startQuote != -1) {
                                int endQuote = text.IndexOf('\"', startQuote + 1);
                                if (endQuote != -1) {
                                    translation = text.Substring(startQuote + 1, endQuote - startQuote - 1);
                                }
                            }
                        }
                        else {
                            // Translation of phrase
                            text = text.Substring(0, index);
                            text = text.Replace("],[", ",");
                            text = text.Replace("]", string.Empty);
                            text = text.Replace("[", string.Empty);
                            text = text.Replace("\",\"", "\"");

                            // Get translated phrases
                            string[] phrases = text.Split (new[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i=0; (i < phrases.Count()); i += 2) {
                                string translatedPhrase = phrases[i];
                                if (translatedPhrase.StartsWith(",,")) {
                                    i--;
                                    continue;
                                }
                                translation += translatedPhrase + "  ";
                            }
                        }

                        // Fix up translation
                        translation = translation.Trim();
                        translation = translation.Replace(" ?", "?");
                        translation = translation.Replace(" !", "!");
                        translation = translation.Replace(" ,", ",");
                        translation = translation.Replace(" .", ".");
                        translation = translation.Replace(" ;", ";");

                        // And translation speech URL
                        this.TranslationSpeechUrl = string.Format ("https://translate.googleapis.com/translate_tts?ie=UTF-8&q={0}&tl={1}&total=1&idx=0&textlen={2}&client=gtx",
                                                                   HttpUtility.UrlEncode (translation), GTranslator.LanguageEnumToIdentifier (targetLanguage), translation.Length);
                    }
                }
                catch (Exception ex) {
                    this.Error = ex;
                }

                // Return result
                this.TranslationTime = DateTime.Now - tmStart;
                return translation;
            }

        #endregion

        #region Private methods

            /// <summary>
            /// Converts a language to its identifier.
            /// </summary>
            /// <param name="language">The language."</param>
            /// <returns>The identifier or <see cref="string.Empty"/> if none.</returns>
            private static string LanguageEnumToIdentifier
                (string language)
            {
                string mode = string.Empty;
                GTranslator.EnsureInitialized();
                GTranslator._languageModeMap.TryGetValue (language, out mode);
                return mode;
            }

            /// <summary>
            /// Ensures the translator has been initialized.
            /// </summary>
            private static void EnsureInitialized()
            {
                if (GTranslator._languageModeMap == null) {
                    GTranslator._languageModeMap = new Dictionary<string,string>();
                    GTranslator._languageModeMap.Add ("Afrikaans",   "af");
                    GTranslator._languageModeMap.Add ("Albanian",    "sq");
                    GTranslator._languageModeMap.Add ("Arabic",      "ar");
                    GTranslator._languageModeMap.Add ("Armenian",    "hy");
                    GTranslator._languageModeMap.Add ("Azerbaijani", "az");
                    GTranslator._languageModeMap.Add ("Basque",      "eu");
                    GTranslator._languageModeMap.Add ("Belarusian",  "be");
                    GTranslator._languageModeMap.Add ("Bengali",     "bn");
                    GTranslator._languageModeMap.Add ("Bulgarian",   "bg");
                    GTranslator._languageModeMap.Add ("Catalan",     "ca");
                    GTranslator._languageModeMap.Add ("Chinese",     "zh-CN");
                    GTranslator._languageModeMap.Add ("Croatian",    "hr");
                    GTranslator._languageModeMap.Add ("Czech",       "cs");
                    GTranslator._languageModeMap.Add ("Danish",      "da");
                    GTranslator._languageModeMap.Add ("Dutch",       "nl");
                    GTranslator._languageModeMap.Add ("English",     "en");
                    GTranslator._languageModeMap.Add ("Esperanto",   "eo");
                    GTranslator._languageModeMap.Add ("Estonian",    "et");
                    GTranslator._languageModeMap.Add ("Filipino",    "tl");
                    GTranslator._languageModeMap.Add ("Finnish",     "fi");
                    GTranslator._languageModeMap.Add ("French",      "fr");
                    GTranslator._languageModeMap.Add ("Galician",    "gl");
                    GTranslator._languageModeMap.Add ("German",      "de");
                    GTranslator._languageModeMap.Add ("Georgian",    "ka");
                    GTranslator._languageModeMap.Add ("Greek",       "el");
                    GTranslator._languageModeMap.Add ("Haitian Creole",    "ht");
                    GTranslator._languageModeMap.Add ("Hebrew",      "iw");
                    GTranslator._languageModeMap.Add ("Hindi",       "hi");
                    GTranslator._languageModeMap.Add ("Hungarian",   "hu");
                    GTranslator._languageModeMap.Add ("Icelandic",   "is");
                    GTranslator._languageModeMap.Add ("Indonesian",  "id");
                    GTranslator._languageModeMap.Add ("Irish",       "ga");
                    GTranslator._languageModeMap.Add ("Italian",     "it");
                    GTranslator._languageModeMap.Add ("Japanese",    "ja");
                    GTranslator._languageModeMap.Add ("Korean",      "ko");
                    GTranslator._languageModeMap.Add ("Lao",         "lo");
                    GTranslator._languageModeMap.Add ("Latin",       "la");
                    GTranslator._languageModeMap.Add ("Latvian",     "lv");
                    GTranslator._languageModeMap.Add ("Lithuanian",  "lt");
                    GTranslator._languageModeMap.Add ("Macedonian",  "mk");
                    GTranslator._languageModeMap.Add ("Malay",       "ms");
                    GTranslator._languageModeMap.Add ("Maltese",     "mt");
                    GTranslator._languageModeMap.Add ("Norwegian",   "no");
                    GTranslator._languageModeMap.Add ("Persian",     "fa");
                    GTranslator._languageModeMap.Add ("Polish",      "pl");
                    GTranslator._languageModeMap.Add ("Portuguese",  "pt");
                    GTranslator._languageModeMap.Add ("Romanian",    "ro");
                    GTranslator._languageModeMap.Add ("Russian",     "ru");
                    GTranslator._languageModeMap.Add ("Serbian",     "sr");
                    GTranslator._languageModeMap.Add ("Slovak",      "sk");
                    GTranslator._languageModeMap.Add ("Slovenian",   "sl");
                    GTranslator._languageModeMap.Add ("Spanish",     "es");
                    GTranslator._languageModeMap.Add ("Swahili",     "sw");
                    GTranslator._languageModeMap.Add ("Swedish",     "sv");
                    GTranslator._languageModeMap.Add ("Tamil",       "ta");
                    GTranslator._languageModeMap.Add ("Telugu",      "te");
                    GTranslator._languageModeMap.Add ("Thai",        "th");
                    GTranslator._languageModeMap.Add ("Turkish",     "tr");
                    GTranslator._languageModeMap.Add ("Ukrainian",   "uk");
                    GTranslator._languageModeMap.Add ("Urdu",         "ur");
                    GTranslator._languageModeMap.Add ("Vietnamese",  "vi");
                    GTranslator._languageModeMap.Add ("Welsh",       "cy");
                    GTranslator._languageModeMap.Add ("Yiddish",     "yi");
                }
            }

        #endregion

        #region Fields

            /// <summary>
            /// The language to translation mode map.
            /// </summary>
            private static Dictionary<string, string> _languageModeMap;

        #endregion
    }
}
