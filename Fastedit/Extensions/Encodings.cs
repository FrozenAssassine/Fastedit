using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using UnicodeEncoding = System.Text.UnicodeEncoding;

namespace Fastedit.Extensions
{
    public class Encodings
    {
        public static readonly List<string> AllEncodingNames = new List<string>
        {
            "ANSI", "UTF-7", "UTF-8", "UTF-8-BOM", "UTF-16 BE", "UTF-16 LE","UTF-16 BE BOM", "UTF-16 LE BOM", "UTF-32 BE", "UTF-32 LE","UTF-32 BE BOM", "UTF-32 LE BOM"
        };
        
        private static Encoding _systemDefaultANSIEncoding;
        private static Encoding _currentCultureANSIEncoding;

        public static Encoding DetectTextEncoding(byte[] b, out string text, int taster = 1000)
        {
            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) { text = Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4); return Encoding.GetEncoding("utf-32BE"); }  // UTF-32, big-endian 
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) { text = Encoding.UTF32.GetString(b, 4, b.Length - 4); return Encoding.UTF32; }    // UTF-32, little-endian
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) { text = Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2); return Encoding.BigEndianUnicode; }     // UTF-16, big-endian
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) { text = Encoding.Unicode.GetString(b, 2, b.Length - 2); return Encoding.Unicode; }              // UTF-16, little-endian
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) { text = Encoding.UTF8.GetString(b, 3, b.Length - 3); return Encoding.UTF8; } // UTF-8
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76) { text = Encoding.UTF7.GetString(b, 3, b.Length - 3); return Encoding.UTF7; } // UTF-7

            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > b.Length) taster = b.Length;    // Taster size can't be bigger than the filesize obviously.

            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4)
            {
                if (b[i] <= 0x7F) { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] < 0xE0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0) { i += 2; utf8 = true; continue; }
                if (b[i] >= 0xE0 && b[i] < 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0) { i += 3; utf8 = true; continue; }
                if (b[i] >= 0xF0 && b[i] < 0xF5 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0) { i += 4; utf8 = true; continue; }
                utf8 = false; break;
            }
            if (utf8 == true)
            {
                text = Encoding.UTF8.GetString(b);
                return Encoding.UTF8;
            }

            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.BigEndianUnicode.GetString(b); return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.Unicode.GetString(b); return Encoding.Unicode; } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++)
            {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') && (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') && (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') && (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') && (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') && (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') && (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    )
                {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8; else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    int oldn = n;
                    while (n < taster && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z')))
                    { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try
                    {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        text = Encoding.GetEncoding(internalEnc).GetString(b);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            text = Encoding.Default.GetString(b);
            return Encoding.Default;
        }
        public static Encoding StringToEncoding(string name)
        {
            switch (name)
            {
                case "ANSI":
                    if (TryGetSystemDefaultANSIEncoding(out var systemDefaultANSIEncoding)) return systemDefaultANSIEncoding;
                    else return TryGetCurrentCultureANSIEncoding(out var currentCultureANSIEncoding) ? currentCultureANSIEncoding : new UTF8Encoding(false);
                case "UTF-7":
                    return new UTF7Encoding();
                case "UTF-8":
                    return new UTF8Encoding(false);
                case "UTF-8-BOM":
                    return new UTF8Encoding(true);
                case "UTF-16 BE BOM":
                    return new UnicodeEncoding(true, true);
                case "UTF-16 LE BOM":
                    return new UnicodeEncoding(false, true);
                case "UTF-16 BE":
                    return new UnicodeEncoding(true, false);
                case "UTF-16 LE":
                    return new UnicodeEncoding(false, false);
                case "UTF-32 BE BOM":
                    return new UTF32Encoding(true, true);
                case "UTF-32 LE BOM":
                    return new UTF32Encoding(false, true);
                case "UTF-32 BE":
                    return new UTF32Encoding(true, false);
                case "UTF-32 LE":
                    return new UTF32Encoding(false, false);
                default:
                    return Encoding.Default;
            }
        }
        public static Encoding IntToEncoding(int index)
        {
            switch (index)
            {
                case 11:
                    if (TryGetSystemDefaultANSIEncoding(out var systemDefaultANSIEncoding)) return systemDefaultANSIEncoding;
                    else return TryGetCurrentCultureANSIEncoding(out var currentCultureANSIEncoding) ? currentCultureANSIEncoding : new UTF8Encoding(false);
                case 0:
                    return new UTF7Encoding();
                case 1:
                    return new UTF8Encoding(false);
                case 2:
                    return new UTF8Encoding(true);
                case 3:
                    return new UnicodeEncoding(true, true);
                case 4:
                    return new UnicodeEncoding(false, true);
                case 5:
                    return new UnicodeEncoding(true, false);
                case 6:
                    return new UnicodeEncoding(false, false);
                case 7:
                    return new UTF32Encoding(true, true);
                case 8:
                    return new UTF32Encoding(false, true);
                case 9:
                    return new UTF32Encoding(true, false);
                case 10:
                    return new UTF32Encoding(false, false);
                default:
                    return Encoding.Default;
            }
        }
        public static string EncodingToString(Encoding encoding)
        {
            string encodingName;

            switch (encoding)
            {
                case UTF7Encoding _:
                    encodingName = "UTF-7";
                    break;
                case UTF8Encoding _ when Equals(encoding, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)):
                    encodingName = "UTF-8-BOM";
                    break;
                case UTF8Encoding _ when Equals(encoding, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)):
                    encodingName = "UTF-8";
                    break;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: true, byteOrderMark: true)):
                    encodingName = "UTF-16 BE BOM";
                    break;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: false, byteOrderMark: true)):
                    encodingName = "UTF-16 LE BOM";
                    break;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: true, byteOrderMark: false)):
                    encodingName = "UTF-16 BE";
                    break;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: false, byteOrderMark: false)):
                    encodingName = "UTF-16 LE";
                    break;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: true, byteOrderMark: true)):
                    encodingName = "UTF-32 BE BOM";
                    break;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: false, byteOrderMark: true)):
                    encodingName = "UTF-32 LE BOM";
                    break;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: true, byteOrderMark: false)):
                    encodingName = "UTF-32 BE";
                    break;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: false, byteOrderMark: false)):
                    encodingName = "UTF-32 LE";
                    break;
                default:
                    encodingName = "ANSI";
                    break;
            }

            return encodingName;
        }
        public static int EncodingToInt(Encoding encoding)
        {
            switch (encoding)
            {
                case UTF7Encoding _:
                    return 0;
                case UTF8Encoding _ when Equals(encoding, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)):
                    return 1;
                case UTF8Encoding _ when Equals(encoding, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)):
                    return 2;                
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: true, byteOrderMark: true)):
                    return 3;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: false, byteOrderMark: true)):
                    return 4;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: true, byteOrderMark: false)):
                    return 5;
                case UnicodeEncoding _ when Equals(encoding, new UnicodeEncoding(bigEndian: false, byteOrderMark: false)):
                    return 6;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: true, byteOrderMark: true)):
                    return 7;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: false, byteOrderMark: true)):
                    return 8;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: true, byteOrderMark: false)):
                    return 9;
                case UTF32Encoding _ when Equals(encoding, new UTF32Encoding(bigEndian: false, byteOrderMark: false)):
                    return 10;
                default:
                    return 11;
            }
        }
        public static bool TryGetSystemDefaultANSIEncoding(out Encoding encoding)
        {
            try
            {
                if (_systemDefaultANSIEncoding != null)
                {
                    encoding = _systemDefaultANSIEncoding;
                    return true;
                }
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                encoding = Encoding.GetEncoding(0);
                _systemDefaultANSIEncoding = encoding;
                return true;
            }
            catch
            {
            }

            encoding = null;
            return false;
        }
        public static bool TryGetCurrentCultureANSIEncoding(out Encoding encoding)
        {
            try
            {
                if (_currentCultureANSIEncoding != null)
                {
                    encoding = _currentCultureANSIEncoding;
                    return true;
                }
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                encoding = Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage);
                _currentCultureANSIEncoding = encoding;
                return true;
            }
            catch
            {
            }

            encoding = null;
            return false;
        }
    }
}
