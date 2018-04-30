// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.CSharp;
using Roslynator.CSharp.CodeFixes;
using Xunit;
using static Roslynator.Tests.CSharp.CSharpCompilerCodeFixVerifier;

namespace Roslynator.CodeFixes.Tests
{
    public static class CS1061TypeDoesNotContainDefinitionAndNoExtensionMethodCouldBeFoundTests
    {
        private const string DiagnosticId = CompilerDiagnosticIdentifiers.TypeDoesNotContainDefinitionAndNoExtensionMethodCouldBeFound;

        private static CodeFixProvider CodeFixProvider { get; } = new TypeDoesNotContainDefinitionCodeFixProvider();

        [Fact]
        public static void TestFix_RemoveAwaitKeyword()
        {
            VerifyFix(@"
using System.Threading.Tasks;

class C
{
    void M()
    {
        async Task<string> GetAsync()
        {
            return await Foo();
        }

        async Task DoAsync()
        {
            await Foo();
        }

        string Foo() => null;
    }
}
", @"
using System.Threading.Tasks;

class C
{
    void M()
    {
        async Task<string> GetAsync()
        {
            return Foo();
        }

        async Task DoAsync()
        {
            Foo();
        }

        string Foo() => null;
    }
}
", DiagnosticId, CodeFixProvider, EquivalenceKey.Create(DiagnosticId));
        }

        [Fact]
        public static void TestFix_LengthToCount()
        {
            VerifyFix(@"
using System.Collections.Generic;
using System.Collections.ObjectModel;

class C
{
    void M()
    {
        int i = 0;

        var list = new List<object>();
        var collection = new Collection<object>();

        i = list.Length;
        i = collection.Length;
        i = list?.Length;
        i = collection?.Length;
    }
}
", @"
using System.Collections.Generic;
using System.Collections.ObjectModel;

class C
{
    void M()
    {
        int i = 0;

        var list = new List<object>();
        var collection = new Collection<object>();

        i = list.Count;
        i = collection.Count;
        i = list?.Count;
        i = collection?.Count;
    }
}
", DiagnosticId, CodeFixProvider, EquivalenceKey.Create(DiagnosticId));
        }
    }
}
