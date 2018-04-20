// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.CSharp;
using Roslynator.CSharp.CodeFixes;
using Xunit;
using static Roslynator.Tests.CSharpCompilerCodeFixVerifier;

namespace Roslynator.CodeFixes.Tests
{
    public static class CSTests
    {
        private const string DiagnosticId = CompilerDiagnosticIdentifiers.OperatorCannotBeAppliedToOperands;

        private static CodeFixProvider CodeFixProvider { get; }

        private const string SourceTemplate = @"
using System.Collections.Generic;
using System.Linq;

class C
{
    void M()
    {
    }
}
";

        //[Fact]
        public static void TestCodeFix()
        {
            VerifyCodeFix(
@"
",
@"
",
                diagnosticId: DiagnosticId,
                codeFixProvider: CodeFixProvider,
                equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Theory]
        //[InlineData("", "")]
        public static void TestCodeFix2(string fixableCode, string fixedCode)
        {
            VerifyCodeFix(
                SourceTemplate,
                fixableCode,
                fixedCode,
                diagnosticId: DiagnosticId,
                codeFixProvider: CodeFixProvider,
                equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }

        //[Fact]
        public static void TestNoCodeFix()
        {
            VerifyNoCodeFix(
@"
",
                codeFixProvider: CodeFixProvider,
                equivalenceKey: EquivalenceKey.Create(DiagnosticId));
        }
    }
}
