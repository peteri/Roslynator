// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    public static class Extensions
    {
        public static string ToMultilineString(this IEnumerable<Diagnostic> diagnostics)
        {
            return string.Join("\r\n", diagnostics.Select(d => d.ToString()));
        }

        public static string ToFullString(this Document document)
        {
            return document.GetSyntaxRootAsync().Result.ToFullString();
        }
    }
}
