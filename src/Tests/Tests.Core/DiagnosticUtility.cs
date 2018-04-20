// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Roslynator
{
    public static class DiagnosticUtility
    {
        public static Diagnostic[] GetSortedDiagnostics(
            IEnumerable<string> sources,
            DiagnosticAnalyzer analyzer,
            string language)
        {
            IEnumerable<Document> documents = WorkspaceUtility.CreateDocuments(sources, language);

            return GetSortedDiagnostics(documents, analyzer);
        }

        public static Diagnostic[] GetSortedDiagnostics(
            Document document,
            DiagnosticAnalyzer analyzer)
        {
            Project project = document.Project;

            List<Diagnostic> diagnostics = null;

            SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

            foreach (Diagnostic diagnostic in GetDiagnostics(project, analyzer))
            {
                Location location = diagnostic.Location;

                Debug.Assert(location != Location.None && !location.IsInMetadata, location.ToString());

                (diagnostics ?? (diagnostics = new List<Diagnostic>())).Add(diagnostic);
            }

            if (diagnostics != null)
            {
                diagnostics.Sort(DiagnosticComparer.SpanStart);

                return diagnostics.ToArray();
            }

            return Array.Empty<Diagnostic>();
        }

        public static Diagnostic[] GetSortedDiagnostics(
            IEnumerable<Document> documents,
            DiagnosticAnalyzer analyzer)
        {
            Project project = documents.First().Project;

            List<Diagnostic> diagnostics = null;

            foreach (Diagnostic diagnostic in GetDiagnostics(project, analyzer))
            {
                Location location = diagnostic.Location;

                Debug.Assert(location != Location.None && !location.IsInMetadata, location.ToString());

                if (location == Location.None
                    || location.IsInMetadata)
                {
                    (diagnostics ?? (diagnostics = new List<Diagnostic>())).Add(diagnostic);
                }
                else
                {
                    foreach (Document document in documents)
                    {
                        SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

                        if (tree == location.SourceTree)
                        {
                            (diagnostics ?? (diagnostics = new List<Diagnostic>())).Add(diagnostic);
                        }
                    }
                }
            }

            if (diagnostics != null)
            {
                diagnostics.Sort(DiagnosticComparer.SpanStart);

                return diagnostics.ToArray();
            }

            return Array.Empty<Diagnostic>();
        }

        private static ImmutableArray<Diagnostic> GetDiagnostics(Project project, DiagnosticAnalyzer analyzer)
        {
            return GetDiagnosticsAsync(project, analyzer).Result;
        }

        private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Project project, DiagnosticAnalyzer analyzer)
        {
            Compilation compilation = await project.GetCompilationAsync().ConfigureAwait(false);

            //foreach (Diagnostic diagnostic in compilation.GetDiagnostics())
            //{
            //    if (diagnostic.Descriptor.DefaultSeverity == DiagnosticSeverity.Error
            //        && diagnostic.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.Compiler))
            //    {
            //        Debug.WriteLine(diagnostic.ToString());
            //    }
            //}

            compilation = EnableDiagnosticsDisabledByDefault(analyzer, compilation);

            CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

            return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
        }

        private static Compilation EnableDiagnosticsDisabledByDefault(DiagnosticAnalyzer analyzer, Compilation compilation)
        {
            foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics)
            {
                if (descriptor.IsEnabledByDefault)
                    continue;

                CompilationOptions compilationOptions = compilation.Options;
                ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions;

                specificDiagnosticOptions = specificDiagnosticOptions.Add(descriptor.Id, descriptor.DefaultSeverity.ToReportDiagnostic());
                CompilationOptions options = compilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions);

                compilation = compilation.WithOptions(options);
            }

            return compilation;
        }

        internal static IEnumerable<Diagnostic> GetNewDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            IEnumerable<Diagnostic> newDiagnostics)
        {
            using (IEnumerator<Diagnostic> enNew = newDiagnostics.OrderBy(f => f.Location.SourceSpan.Start).GetEnumerator())
            using (IEnumerator<Diagnostic> en = diagnostics.OrderBy(f => f.Location.SourceSpan.Start).GetEnumerator())
            {
                while (enNew.MoveNext())
                {
                    if (en.MoveNext())
                    {
                        if (en.Current.Id != enNew.Current.Id)
                            yield return enNew.Current;
                    }
                    else
                    {
                        yield return enNew.Current;

                        while (enNew.MoveNext())
                            yield return enNew.Current;

                        yield break;
                    }
                }
            }
        }

        public static ImmutableArray<Diagnostic> GetCompilerDiagnostics(Document document, CancellationToken cancellation = default(CancellationToken))
        {
            return document.GetSemanticModelAsync(cancellation).Result.GetDiagnostics();
        }

        public static void VerifyNoCompilerError(IEnumerable<Document> documents)
        {
            foreach (Document document in documents)
                VerifyNoCompilerError(document);
        }

        //TODO: move to DiagnosticVerifier
        public static void VerifyNoCompilerError(Document document)
        {
            ImmutableArray<Diagnostic> compilerDiagnostics = GetCompilerDiagnostics(document);

            if (compilerDiagnostics.Any(f => f.Severity == DiagnosticSeverity.Error))
            {
                compilerDiagnostics = compilerDiagnostics.Where(f => f.Severity == DiagnosticSeverity.Error).ToImmutableArray();

                Assert.True(false,
                    $"No compiler error expected\r\n\r\nDiagnostics:\r\n{compilerDiagnostics.ToMultilineString()}");
            }
        }
    }
}
