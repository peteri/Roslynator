// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Tests
{
    public static class CSharpCodeRefactoringVerifier
    {
        public static void VerifyNoCodeRefactoring(
            string source,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey = null)
        {
            (string source2, List<TextSpan> spans) = TextUtility.GetMarkedSpans(source);

            CodeRefactoringVerifier.VerifyNoCodeRefactoring(
                source: source2,
                spans: spans,
                codeRefactoringProvider: codeRefactoringProvider,
                language: LanguageNames.CSharp,
                equivalenceKey: equivalenceKey);
        }

        public static void VerifyNoCodeRefactoring(
            string source,
            TextSpan span,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey = null)
        {
            CodeRefactoringVerifier.VerifyNoCodeRefactoring(
                source: source,
                span: span,
                codeRefactoringProvider: codeRefactoringProvider,
                language: LanguageNames.CSharp,
                equivalenceKey: equivalenceKey);
        }

        public static void VerifyCodeRefactoring(
            string source,
            string newSource,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey = null,
            bool allowNewCompilerDiagnostics = false)
        {
            (string source2, List<TextSpan> spans) = TextUtility.GetMarkedSpans(source);

            VerifyCodeRefactoring(
                source: source2,
                newSource: newSource,
                spans: spans,
                codeRefactoringProvider: codeRefactoringProvider,
                equivalenceKey: equivalenceKey,
                allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);
        }

        public static void VerifyCodeRefactoring(
            string sourceTemplate,
            string fixableCode,
            string fixedCode,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey = null,
            bool allowNewCompilerDiagnostics = false)
        {
            (string source, string newSource, TextSpan span) = TextUtility.GetMarkedSpan(sourceTemplate, fixableCode, fixedCode);

            VerifyCodeRefactoring(
                source: source,
                newSource: newSource,
                span: span,
                codeRefactoringProvider: codeRefactoringProvider,
                equivalenceKey: equivalenceKey,
                allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);
        }

        public static void VerifyCodeRefactoring(
            string source,
            string newSource,
            TextSpan span,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey = null,
            bool allowNewCompilerDiagnostics = false)
        {
            CodeRefactoringVerifier.VerifyCodeRefactoring(
                source: source,
                newSource: newSource,
                span: span,
                codeRefactoringProvider: codeRefactoringProvider,
                language: LanguageNames.CSharp,
                equivalenceKey: equivalenceKey,
                allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);
        }

        public static void VerifyCodeRefactoring(
            string source,
            string newSource,
            IEnumerable<TextSpan> spans,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey = null,
            bool allowNewCompilerDiagnostics = false)
        {
            CodeRefactoringVerifier.VerifyCodeRefactoring(
                source: source,
                newSource: newSource,
                spans: spans,
                codeRefactoringProvider: codeRefactoringProvider,
                language: LanguageNames.CSharp,
                equivalenceKey: equivalenceKey,
                allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);
        }
    }
}
