using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public static class PostfixExtensions {
    public static string FormatForPostfix(this string format, params object[] values) {
        return string.Format(format, values.Select(PostfixInterpreter.Format ).Cast<object>().ToArray());
    }
    public static void WritePostfix(this TextWriter writer, string format, params object[] values) {
        writer.Write(format.FormatForPostfix(values));
    }
}