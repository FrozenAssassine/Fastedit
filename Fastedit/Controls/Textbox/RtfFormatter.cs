using ColorCode.Common;
using ColorCode.Parsing;
using ColorCode.Styling;
using ColorCode;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Fastedit.Controls.Textbox
{
    public class RtfFormatter  //: CodeColorizerBase
    {
    }
}
        /*private readonly Dictionary<string, int> _colorTableIndexes = new Dictionary<string, int>();
        private readonly string _colorTable;
        private TextControlBox textbox = null;
        
        Stopwatch stopWatch = new Stopwatch();
        private void ShowTimeElapsed(string text)
        {
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            Debug.WriteLine(text + ": " + elapsedTime);
        }
        private void StartWatch()
        {
            stopWatch.Start();
        }

        public RtfFormatter(TextControlBox textbox, StyleDictionary style = null, ILanguageParser languageParser = null) : base(style, languageParser)
        {
            _colorTable = PrepareColorTable();
            this.textbox = textbox;
        }
        private TextWriter Writer { get; set; }

        public string GetRtfString(string sourceCode, ILanguage language)
        {
            var buffer = new StringBuilder();
            using (var writer = new StringWriter(buffer))
            {
                Writer = writer;
                WriteHeader();
                StartWatch();
                languageParser.Parse(sourceCode, language, Write);
                ShowTimeElapsed("Parse");
                writer.Flush();
            }
            return buffer.ToString();
        }
        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            var styleInsertions = new List<TextInsertion>();

            if (scopes.Count == 0)
            {
                scopes.Add(new Scope(ScopeName.PlainText, 0, parsedSourceCode.Length));
            }

            for (int i = 0; i < scopes.Count; i++)
            {
                GetStyleInsertionsForCapturedStyle(scopes[i], styleInsertions);
            }


            styleInsertions.SortStable((x, y) => x.Index.CompareTo(y.Index));

            int offset = 0;

            for (int i = 0; i < styleInsertions.Count; i++)
            {
                TextInsertion styleInsertion = styleInsertions[i];
                var text = parsedSourceCode.Substring(offset, styleInsertion.Index - offset);
                Writer.Write(RtfEncodeText(text));

                BuildControlWordsForCapturedStyle(styleInsertion.Scope);

                if (string.IsNullOrEmpty(styleInsertion.Text))
                {
                    BuildControlWordsForCapturedStyle(styleInsertion.Scope);
                }
                else
                {
                    Writer.Write(styleInsertion.Text);
                }

                offset = styleInsertion.Index;
            }

            Writer.Write(RtfEncodeText(parsedSourceCode.Substring(offset)));
        }
        private string RtfEncodeText(string text)
        {
            text = text.Replace("\\", "\\\\", StringComparison.Ordinal);
            text = text.Replace("{", @"\{", StringComparison.Ordinal);
            text = text.Replace("}", @"\}", StringComparison.Ordinal);
            text = text.Replace("\r\n", "\\par\r\n", StringComparison.Ordinal);
            //text = Regex.Replace(text, "\\\\", "\\\\", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //text = Regex.Replace(text, @"\{", @"\{", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //text = Regex.Replace(text, @"\}", @"\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //text = Regex.Replace(text, @"\\r\n", "\\par\r\n", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return text;
        }
        private void WriteHeader()
        {
            int size = textbox.GetFontSizeFromRTF();
            Writer.WriteLine(@"{\rtf1\fbidis\ansi\ansicpg1252\deff0\nouicompat\deflang1031{\fonttbl{\f0\fnil " + textbox.FontFamily.Source + ";}}");
            Writer.Write(_colorTable);
            Writer.Write(@"{\*\generator Riched20 10.0.19041}\viewkind4\uc1\pard\tx720\cf1\f0\fs" + size + @"\lang1033");
        }
        private string PrepareColorTable()
        {
            var colorTable = new StringBuilder();
            var index = 1;

            colorTable.Append(@"{\colortbl ;");

            for (int i = 0; i < Styles.Count; i++)
            {
                var style = Styles[i];
                if (style.Foreground == null)
                {
                    continue;
                }

                var color = HexToColor(style.Foreground);
                colorTable.Append($@"\red{color.R}\green{color.G}\blue{color.B};");
                _colorTableIndexes[style.ScopeName] = index++;
            }
            colorTable.AppendLine("}");

            return colorTable.ToString();
        }
        public static Color HexToColor(string hexString)
        {
            if (hexString.IndexOf('#') != -1)
            {
                hexString = Regex.Replace(hexString, "#", string.Empty);
            }

            byte r, g, b = 0;

            r = (byte)int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            g = (byte)int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            b = (byte)int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            return Color.FromArgb(255, r, g, b);
        }
        private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<TextInsertion> styleInsertions)
        {
            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index,
                Scope = scope
            });

            for (int i = 0; i < scope.Children.Count; i++)
            {
                GetStyleInsertionsForCapturedStyle(scope.Children[i], styleInsertions);
            }

            styleInsertions.Add(new TextInsertion
            {
                Index = scope.Index + scope.Length,
                Text = GetEndTag(scope)
            });
        }
        private string GetEndTag(Scope scope)
        {
            var end = string.Empty;
            var style = Styles[scope.Name];

            if (style.Bold)
            {
                end += "\\b0";
            }

            if (style.Italic)
            {
                end += "\\i0";
            }

            return end;
        }
        private void BuildControlWordsForCapturedStyle(Scope scope)
        {
            if (scope == null)
            {
                return;
            }

            var style = Styles[scope.Name];

            if (style.Foreground != null)
            {
                var index = _colorTableIndexes[style.ScopeName];
                Writer.Write($@"\cf{index} ");
            }
            else
            {
                Writer.Write("\\cf0 ");
            }

            if (style.Bold)
            {
                Writer.Write("\\b ");
            }

            if (style.Italic)
            {
                Writer.Write("\\i ");
            }
        }

    }

    public class CS_Language : ILanguage
    {
        private readonly IEnumerable<string> Variables = new string[] { "string", "var", "bool", "int", "double", "float", "void" };
        private readonly IEnumerable<string> Attributes = new string[] { "const", "readonly", "class", "sealed", "partial", "namespace", "while", "foreach", "for", "if", "else", "true", "false", "new", "private", "public", "protected", "override", "void" };

        //private readonly LanguageRule _singleLineComment;
        private readonly LanguageRule _multiLineComment;
        private readonly LanguageRule _number;
        private readonly LanguageRule _attributes;
        private readonly LanguageRule _singleQuotes;
        private readonly LanguageRule _doubleQuotes;
        private readonly LanguageRule _backTicks;
        private readonly LanguageRule _multiLineString;
        private readonly LanguageRule _variables;
        private readonly LanguageRule _variablenames;

        public CS_Language()
        {
            //_singleLineComment = new LanguageRule(
            //    @"(//.*?)\r?$",
            //    new Dictionary<int, string>
            //    {
            //        [1] = ScopeName.Comment
            //    }
            //);
            _multiLineComment = new LanguageRule(
        */
          //      @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
            /*    new Dictionary<int, string>
                {
                    [0] = ScopeName.Comment
                }
            );

            _number = new LanguageRule(
                @"\b(?:0x[a-f0-9]+|(?:\d(?:_\d+)*\d*(?:\.\d*)?|\.\d\+)(?:e[+\-]?\d+)?)\b",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Number
                }
            );

            _attributes = new LanguageRule(
                $"(?i)\\b({string.Join('|', Attributes)})\\b",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Attribute
                }
            );

            _variables = new LanguageRule(
                $"(?i)\\b({string.Join('|', Variables)})\\b",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.Attribute
                }
            );

            _variablenames = new LanguageRule(
                $"(?<=({string.Join('|', Variables)}) ).*(?= =)",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.TypeVariable
                }
            );

            _singleQuotes = new LanguageRule(
                @"'[^\n]*?'",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );

            _doubleQuotes = new LanguageRule(
                @"""[^\n]*?""",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );

            _backTicks = new LanguageRule(
                @"`[^\n]*?`",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );

            _multiLineString = new LanguageRule(
                @"(?s)(\""\""\"")(.*?)(\""\""\"")",
                new Dictionary<int, string>
                {
                    [0] = ScopeName.String
                }
            );
        }

        public string Id => "CS";

        public string Name => Id;

        public string CssClassName => Id;

        public string FirstLinePattern => null;

        public IList<LanguageRule> Rules => new List<LanguageRule>
        {
            _multiLineComment,
            //_singleLineComment,
            _number,
            _attributes,
            _multiLineString,
            _singleQuotes,
            _doubleQuotes,
            _backTicks,
            _variables,
            _variablenames
        };

        public bool HasAlias(string lang)
        {
            return false;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public static class SyntaxHighlightingColorCodes
    {
        private const string StringContent = "#d69d85";
        private const string Bluish = "#3586ff";
        private const string Type = "#569cd6";
        private const string Greenish = "#32ff47";
        private const string VariableName = "#dcdcaa";
        private const string Orangeish = "#ffb24c";
        private const string RedButDarker = "#b91843";
        private const string BlueButDarker = "#006ab7";
        private const string GreenButDarker = "#00b200";
        private const string OrangeABitDarker = "#ff652d";
        private const string CyanTinyBitDarker = "#00b1b7";
        private const string CommentGreyDarkest = "#888ea6";
        private const string White = "#ffffff";
        private const string Black = "#000000";

        public static StyleDictionary Light = new StyleDictionary
        {
            new ColorCode.Styling.Style(ScopeName.Comment)
            {
                Foreground = CommentGreyDarkest
            },
            new Style(ScopeName.String)
            {
                Foreground = RedButDarker
            },
            new Style(ScopeName.Attribute)
            {
                Foreground = CyanTinyBitDarker
            },
            new ColorCode.Styling.Style(ScopeName.Number)
            {
                Foreground = OrangeABitDarker
            },
            new Style(ScopeName.PseudoKeyword)
            {
                Foreground = BlueButDarker
            },
            new Style(ScopeName.Keyword)
            {
                Foreground = GreenButDarker
            },
            new Style(ScopeName.PlainText)
            {
                Foreground = Black
            }
        };
        public static StyleDictionary Dark = new StyleDictionary
        {
            new Style(ScopeName.Comment)
            {
                Foreground = CommentGreyDarkest
            },
            new Style(ScopeName.String)
            {
                Foreground = StringContent
            },
            new Style(ScopeName.Attribute)
            {
                Foreground = Type
            },
            new Style(ScopeName.Number)
            {
                Foreground = Orangeish
            },
            new Style(ScopeName.PseudoKeyword)
            {
                Foreground = Bluish
            },
            new Style(ScopeName.Keyword)
            {
                Foreground = Greenish
            },
            new Style(ScopeName.PlainText)
            {
                Foreground = White
            },
            new Style(ScopeName.TypeVariable)
            {
                Foreground = VariableName
            },
            new Style(ScopeName.Intrinsic)
            {
                Foreground = White,
                Italic = true,
            },
            new Style(ScopeName.MarkdownBold)
            {
                Foreground = White,
                Bold = true,
            }
        };
    }
*/
