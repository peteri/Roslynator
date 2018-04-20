// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Xunit;

namespace Roslynator.Tests
{
    public static class CodeFixVerifier
    {
        public static void VerifyNoCodeFix(
            string source,
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            string language)
        {
            Document document = WorkspaceUtility.CreateDocument(source, language);

            DiagnosticUtility.VerifyNoCompilerError(document);

            foreach (Diagnostic diagnostic in DiagnosticUtility.GetSortedDiagnostics(document, analyzer))
            {
                List<CodeAction> actions = null;

                var context = new CodeFixContext(
                    document,
                    diagnostic,
                    (a, _) => (actions ?? (actions = new List<CodeAction>())).Add(a),
                    CancellationToken.None);

                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                Assert.True(actions == null, $"Expected no code fix, actual: {actions.Count}.");
            }
        }

        public static void VerifyFix(
            string source,
            string newSource,
            DiagnosticAnalyzer analyzer,
            CodeFixProvider codeFixProvider,
            string language,
            bool allowNewCompilerDiagnostics = false)
        {
            Assert.True(codeFixProvider.FixableDiagnosticIds.Intersect(analyzer.SupportedDiagnostics.Select(f => f.Id)).Any(), $"Code fix provider '{codeFixProvider.GetType().Name}' cannot fix any diagnostic supported by analyzer '{analyzer}'.");

            Document document = WorkspaceUtility.CreateDocument(source, language);

            Diagnostic[] analyzerDiagnostics = DiagnosticUtility.GetSortedDiagnostics(document, analyzer);

            ImmutableArray<Diagnostic> compilerDiagnostics = DiagnosticUtility.GetCompilerDiagnostics(document);

            while (analyzerDiagnostics.Length > 0)
            {
                List<CodeAction> actions = null;

                var context = new CodeFixContext(
                    document,
                    analyzerDiagnostics[0],
                    (a, _) => (actions ?? (actions = new List<CodeAction>())).Add(a),
                    CancellationToken.None);

                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (actions == null)
                    break;

                document = WorkspaceUtility.ApplyCodeAction(document, actions[0]);

                analyzerDiagnostics = DiagnosticUtility.GetSortedDiagnostics(document, analyzer);

                IEnumerable<Diagnostic> newCompilerDiagnostics = DiagnosticUtility.GetNewDiagnostics(compilerDiagnostics, DiagnosticUtility.GetCompilerDiagnostics(document));

                if (!allowNewCompilerDiagnostics
                    && newCompilerDiagnostics.Any())
                {
                    document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                    newCompilerDiagnostics = DiagnosticUtility.GetNewDiagnostics(compilerDiagnostics, DiagnosticUtility.GetCompilerDiagnostics(document));

                    Assert.True(false,
                        $"Fix introduced new compiler diagnostics\r\n\r\nDiagnostics:\r\n{newCompilerDiagnostics.ToMultilineString()}\r\n\r\nNew document:\r\n{document.ToFullString()}\r\n");
                }
            }

            string actual = WorkspaceUtility.GetSimplifiedAndFormattedText(document);

            Assert.Equal(newSource, actual);
        }
    }
}
