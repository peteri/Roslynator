// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Roslynator.Tests
{
    public static class CodeRefactoringVerifier
    {
        public static void VerifyNoCodeRefactoring(
            string source,
            IEnumerable<TextSpan> spans,
            CodeRefactoringProvider codeRefactoringProvider,
            string language,
            string equivalenceKey = null)
        {
            foreach (TextSpan span in spans)
            {
                VerifyNoCodeRefactoring(
                    source: source,
                    span: span,
                    codeRefactoringProvider: codeRefactoringProvider,
                    language: language,
                    equivalenceKey: equivalenceKey);
            }
        }

        public static void VerifyNoCodeRefactoring(
            string source,
            TextSpan span,
            CodeRefactoringProvider codeRefactoringProvider,
            string language,
            string equivalenceKey = null)
        {
            Document document = WorkspaceUtility.CreateDocument(source, language);

            DiagnosticUtility.VerifyNoCompilerError(document);

            List<CodeAction> actions = null;

            var context = new CodeRefactoringContext(
                document,
                span,
                codeAction =>
                {
                    if (equivalenceKey == null
                        || string.Equals(codeAction.EquivalenceKey, equivalenceKey, StringComparison.Ordinal))
                    {
                        (actions ?? (actions = new List<CodeAction>())).Add(codeAction);
                    }
                },
                CancellationToken.None);

            codeRefactoringProvider.ComputeRefactoringsAsync(context).Wait();

            Assert.True(actions == null, $"Expected no code refactoring, actual: {actions?.Count ?? 0}");
        }

        public static void VerifyCodeRefactoring(
            string source,
            string newSource,
            IEnumerable<TextSpan> spans,
            CodeRefactoringProvider codeRefactoringProvider,
            string language,
            string equivalenceKey = null,
            bool allowNewCompilerDiagnostics = false)
        {
            Document document = WorkspaceUtility.CreateDocument(source, language);

            foreach (TextSpan span in spans.OrderByDescending(f => f.Start))
            {
                document = VerifyCodeRefactoring(
                    document: document,
                    span: span,
                    codeRefactoringProvider: codeRefactoringProvider,
                    equivalenceKey: equivalenceKey,
                    allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);
            }

            string actual = WorkspaceUtility.GetSimplifiedAndFormattedText(document);

            Assert.Equal(newSource, actual);
        }

        public static void VerifyCodeRefactoring(
            string source,
            string newSource,
            TextSpan span,
            CodeRefactoringProvider codeRefactoringProvider,
            string language,
            string equivalenceKey = null,
            bool allowNewCompilerDiagnostics = false)
        {
            Document document = WorkspaceUtility.CreateDocument(source, language);

            document = VerifyCodeRefactoring(
                document: document,
                span: span,
                codeRefactoringProvider: codeRefactoringProvider,
                equivalenceKey: equivalenceKey,
                allowNewCompilerDiagnostics: allowNewCompilerDiagnostics);

            string actual = WorkspaceUtility.GetSimplifiedAndFormattedText(document);

            Assert.Equal(newSource, actual);
        }

        private static Document VerifyCodeRefactoring(
            Document document,
            TextSpan span,
            CodeRefactoringProvider codeRefactoringProvider,
            string equivalenceKey,
            bool allowNewCompilerDiagnostics)
        {
            ImmutableArray<Diagnostic> compilerDiagnostics = DiagnosticUtility.GetCompilerDiagnostics(document);

            List<CodeAction> actions = null;

            var context = new CodeRefactoringContext(
                document,
                span,
                a =>
                {
                    if (equivalenceKey == null
                        || string.Equals(a.EquivalenceKey, equivalenceKey, StringComparison.Ordinal))
                    {
                        (actions ?? (actions = new List<CodeAction>())).Add(a);
                    }
                },
                CancellationToken.None);

            codeRefactoringProvider.ComputeRefactoringsAsync(context).Wait();

            Assert.True(actions != null, "No code refactoring has been registered.");

            document = WorkspaceUtility.ApplyCodeAction(document, actions[0]);

            IEnumerable<Diagnostic> newCompilerDiagnostics = DiagnosticUtility.GetNewDiagnostics(compilerDiagnostics, DiagnosticUtility.GetCompilerDiagnostics(document));

            if (!allowNewCompilerDiagnostics
                && newCompilerDiagnostics.Any())
            {
                document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                newCompilerDiagnostics = DiagnosticUtility.GetNewDiagnostics(compilerDiagnostics, DiagnosticUtility.GetCompilerDiagnostics(document));

                Assert.True(false,
                    $"Fix introduced new compiler diagnostics\r\n\r\nDiagnostics:\r\n{newCompilerDiagnostics.ToMultilineString()}\r\n\r\nNew document:\r\n{document.ToFullString()}\r\n");
            }

            return document;
        }
    }
}
