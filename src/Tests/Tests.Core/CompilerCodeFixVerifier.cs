// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit;

namespace Roslynator.Tests
{
    public static class CompilerCodeFixVerifier
    {
        public static void VerifyNoCodeFix(
            string source,
            CodeFixProvider codeFixProvider,
            string language,
            string equivalenceKey = null)
        {
            Document document = WorkspaceUtility.CreateDocument(source, language);

            foreach (Diagnostic compilerDiagnostic in DiagnosticUtility.GetCompilerDiagnostics(document))
            {
                var context = new CodeFixContext(
                    document,
                    compilerDiagnostic,
                    (a, _) => Assert.True(equivalenceKey != null && !string.Equals(a.EquivalenceKey, equivalenceKey, StringComparison.Ordinal), "Expected no code fix."),
                    CancellationToken.None);

                codeFixProvider.RegisterCodeFixesAsync(context).Wait();
            }
        }

        public static void VerifyCodeFix(
            string source,
            string newSource,
            string diagnosticId,
            CodeFixProvider codeFixProvider,
            string language,
            string equivalenceKey = null)
        {
            Assert.True(codeFixProvider.FixableDiagnosticIds.Contains(diagnosticId), $"Code fix provider '{codeFixProvider.GetType().Name}' cannot fix diagnostic '{diagnosticId}'.");

            Document document = WorkspaceUtility.CreateDocument(source, language);

            ImmutableArray<Diagnostic> compilerDiagnostics = DiagnosticUtility.GetCompilerDiagnostics(document);

            while (compilerDiagnostics.Length > 0)
            {
                Diagnostic diagnostic = null;

                foreach (Diagnostic compilerDiagnostic in compilerDiagnostics)
                {
                    if (string.Equals(compilerDiagnostic.Id, diagnosticId, StringComparison.Ordinal))
                    {
                        diagnostic = compilerDiagnostic;
                        break;
                    }
                }

                if (diagnostic == null)
                    break;

                List<CodeAction> actions = null;

                var context = new CodeFixContext(
                    document,
                    compilerDiagnostics.First(f => string.Equals(f.Id, diagnosticId, StringComparison.Ordinal)),
                    (a, _) =>
                    {
                        if (equivalenceKey == null
                            || string.Equals(a.EquivalenceKey, equivalenceKey, StringComparison.Ordinal))
                        {
                            (actions ?? (actions = new List<CodeAction>())).Add(a);
                        }
                    },
                    CancellationToken.None);

                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (actions == null)
                    break;

                document = WorkspaceUtility.ApplyCodeAction(document, actions[0]);

                compilerDiagnostics = DiagnosticUtility.GetCompilerDiagnostics(document);
            }

            string actual = WorkspaceUtility.GetSimplifiedAndFormattedText(document);

            Assert.Equal(newSource, actual);
        }
    }
}
