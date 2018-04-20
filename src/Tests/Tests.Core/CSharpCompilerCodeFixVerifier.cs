// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Tests
{
    public static class CSharpCompilerCodeFixVerifier
    {
        public static void VerifyNoCodeFix(
            string source,
            CodeFixProvider codeFixProvider,
            string equivalenceKey = null)
        {
            CompilerCodeFixVerifier.VerifyNoCodeFix(
                source: source,
                codeFixProvider: codeFixProvider,
                language: LanguageNames.CSharp,
                equivalenceKey: equivalenceKey);
        }

        public static void VerifyCodeFix(
            string sourceTemplate,
            string fixableCode,
            string fixedCode,
            string diagnosticId,
            CodeFixProvider codeFixProvider,
            string equivalenceKey = null)
        {
            (string source, string newSource, TextSpan span) = TextUtility.GetMarkedSpan(sourceTemplate, fixableCode, fixedCode);

            VerifyCodeFix(
                source: source,
                newSource: newSource,
                diagnosticId: diagnosticId,
                codeFixProvider: codeFixProvider,
                equivalenceKey: equivalenceKey);
        }

        public static void VerifyCodeFix(
            string source,
            string newSource,
            string diagnosticId,
            CodeFixProvider codeFixProvider,
            string equivalenceKey = null)
        {
            CompilerCodeFixVerifier.VerifyCodeFix(
                source: source,
                newSource: newSource,
                diagnosticId: diagnosticId,
                codeFixProvider: codeFixProvider,
                language: LanguageNames.CSharp,
                equivalenceKey: equivalenceKey);
        }
    }
}
