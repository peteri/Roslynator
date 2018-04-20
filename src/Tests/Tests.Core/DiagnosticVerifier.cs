// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Roslynator.Tests
{
    public static class DiagnosticVerifier
    {
        public static void VerifyNoDiagnostic(
            string source,
            DiagnosticDescriptor descriptor,
            DiagnosticAnalyzer analyzer,
            string language)
        {
            VerifyNoDiagnostic(new string[] { source }, descriptor, analyzer, language);
        }

        public static void VerifyNoDiagnostic(
            IEnumerable<string> sources,
            DiagnosticDescriptor descriptor,
            DiagnosticAnalyzer analyzer,
            string language)
        {
            Assert.True(analyzer.SupportedDiagnostics.IndexOf(descriptor, DiagnosticDescriptorComparer.IdOrdinal) != -1,
                $"Diagnostic \"{descriptor.Id}\" is not supported by analyzer \"{analyzer.GetType().Name}\"");

            IEnumerable<Document> documents = WorkspaceUtility.CreateDocuments(sources, language);

            DiagnosticUtility.VerifyNoCompilerError(documents);

            Diagnostic[] diagnostics = DiagnosticUtility.GetSortedDiagnostics(documents, analyzer);

            Assert.True(diagnostics.Length == 0 || diagnostics.All(f => !string.Equals(f.Id, descriptor.Id, StringComparison.Ordinal)),
                    $"No diagnostic expected\r\n\r\nDiagnostics:\r\n{string.Join("\r\n", diagnostics.Where(f => string.Equals(f.Id, descriptor.Id, StringComparison.Ordinal)))}\r\n");
        }

        public static void VerifyDiagnostic(
            string source,
            DiagnosticAnalyzer analyzer,
            string language,
            params Diagnostic[] expectedDiagnostics)
        {
            VerifyDiagnostic(new string[] { source }, analyzer, language, expectedDiagnostics);
        }

        public static void VerifyDiagnostic(
            IEnumerable<string> sources,
            DiagnosticAnalyzer analyzer,
            string language,
            params Diagnostic[] expectedDiagnostics)
        {
            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = analyzer.SupportedDiagnostics;

            foreach (Diagnostic diagnostic in expectedDiagnostics)
            {
                Assert.True(supportedDiagnostics.IndexOf(diagnostic.Descriptor, DiagnosticDescriptorComparer.IdOrdinal) != -1,
                    $"Diagnostic \"{diagnostic.Descriptor.Id}\" is not supported by analyzer \"{analyzer.GetType().Name}\"");
            }

            Diagnostic[] diagnostics = DiagnosticUtility.GetSortedDiagnostics(sources, analyzer, language);

            if (diagnostics.Length > 0
                && analyzer.SupportedDiagnostics.Length > 1)
            {
                diagnostics = diagnostics
                    .Where(diagnostic => expectedDiagnostics.Any(expectedDiagnostic => DiagnosticComparer.IdOrdinal.Equals(diagnostic, expectedDiagnostic)))
                    .ToArray();
            }

            VerifyDiagnostics(diagnostics, expectedDiagnostics);
        }

        private static void VerifyDiagnostics(
            Diagnostic[] actual,
            Diagnostic[] expected)
        {
            int expectedCount = expected.Length;
            int actualCount = actual.Length;

            Assert.True(expectedCount == actualCount,
                $"Mismatch between number of diagnostics returned, expected: {expectedCount} actual: {actualCount}\r\n\r\nDiagnostics:\r\n{string.Join<Diagnostic>("\r\n", actual)}\r\n");

            for (int i = 0; i < expectedCount; i++)
                VerifyDiagnostic(actual[i], expected[i]);
        }

        private static void VerifyDiagnostic(Diagnostic actual, Diagnostic expected)
        {
            Assert.True(actual.Id == expected.Descriptor.Id,
                $"Expected diagnostic id to be \"{expected.Descriptor.Id}\" was \"{actual.Id}\"\r\n\r\nDiagnostic:\r\n{actual}\r\n");

            VerifyLocation(actual, actual.Location, expected.Location);

            VerifyAdditionalLocations(actual, actual.AdditionalLocations, expected.AdditionalLocations);
        }

        private static void VerifyAdditionalLocations(
            Diagnostic diagnostic,
            IReadOnlyList<Location> actual,
            IReadOnlyList<Location> expected)
        {
            int actualCount = actual.Count;
            int expectedCount = expected.Count;

            Assert.True(actualCount == expectedCount,
                $"Expected {expectedCount} additional locations but got {actualCount} for Diagnostic:\r\n{diagnostic}\r\n");

            for (int j = 0; j < actualCount; j++)
                VerifyLocation(diagnostic, actual[j], expected[j]);
        }

        private static void VerifyLocation(
            Diagnostic diagnostic,
            Location actual,
            Location expected)
        {
            VerifyFileLinePositionSpan(diagnostic, actual.GetLineSpan(), expected.GetLineSpan());
        }

        private static void VerifyFileLinePositionSpan(
            Diagnostic diagnostic,
            FileLinePositionSpan actual,
            FileLinePositionSpan expected)
        {
            Assert.True(actual.Path == expected.Path,
                $"Expected diagnostic to be in file \"{expected.Path}\" was actually in file \"{actual.Path}\"\r\n\r\nDiagnostic:\r\n{diagnostic}\r\n");

            VerifyLinePosition(diagnostic, actual.StartLinePosition, expected.StartLinePosition, "start");

            VerifyLinePosition(diagnostic, actual.EndLinePosition, expected.EndLinePosition, "end");
        }

        private static void VerifyLinePosition(
            Diagnostic diagnostic,
            LinePosition actual,
            LinePosition expected,
            string name)
        {
            int actualLine = actual.Line;
            int expectedLine = expected.Line;

            Assert.True(actualLine == expectedLine,
                $"Expected diagnostic to {name} on line \"{expectedLine}\" actually {name}s on line \"{actualLine}\"\r\n\r\nDiagnostic:\r\n{diagnostic}\r\n");

            int actualCharacter = actual.Character;
            int expectedCharacter = expected.Character;

            Assert.True(actualCharacter == expectedCharacter,
                $"Expected diagnostic to {name} at column \"{expectedCharacter}\" actually {name}s at column \"{actualCharacter}\"\r\n\r\nDiagnostic:\r\n{diagnostic}\r\n");
        }
    }
}
