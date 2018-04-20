// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Tests
{
    public static class CSharpCodeFixVerifier
    {
        public static void VerifyNoCodeFix(
            string source,
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider)
        {
            CodeFixVerifier.VerifyNoCodeFix(
                source: source,
                analyzer: analyzer,
                codeFixProvider: codeFixProvider,
                language: LanguageNames.CSharp);
        }

        public static void VerifyCodeFix(
            string source,
            string newSource,
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            bool allowNewCompilerDiagnostics = false)
        {
            CodeFixVerifier.VerifyFix(
                source: source,
                newSource: newSource,
                analyzer: analyzer,
                codeFixProvider: codeFixProvider,
                language: LanguageNames.CSharp,
                allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);
        }
    }
}
