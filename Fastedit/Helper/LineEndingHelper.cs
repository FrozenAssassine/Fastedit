using System;
using TextControlBoxNS;

namespace Fastedit.Helper;

internal class LineEndingHelper
{
    public static string GetLineEndingString(LineEnding lineEnding)
    {
        return lineEnding switch
        {
            LineEnding.CRLF => "\r\n",
            LineEnding.LF => "\n",
            LineEnding.CR => "\r",
            _ => Environment.NewLine
        };
    }

}
