// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    public abstract class DiagnosticDescriptorComparer : IComparer<DiagnosticDescriptor>, IEqualityComparer<DiagnosticDescriptor>
    {
        private static readonly StringComparer _ordinalStringComparer = StringComparer.Ordinal;

        public static DiagnosticDescriptorComparer IdOrdinal { get; } = new DiagnosticDescriptorIdOrdinalComparer();

        public abstract int Compare(DiagnosticDescriptor x, DiagnosticDescriptor y);

        public abstract bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y);

        public abstract int GetHashCode(DiagnosticDescriptor obj);

        private class DiagnosticDescriptorIdOrdinalComparer : DiagnosticDescriptorComparer
        {
            public override int Compare(DiagnosticDescriptor x, DiagnosticDescriptor y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
            }

            public override bool Equals(DiagnosticDescriptor x, DiagnosticDescriptor y)
            {
                if (object.ReferenceEquals(x, y))
                    return true;

                if (x == null)
                    return false;

                if (y == null)
                    return false;

                return string.Equals(x.Id, y.Id, StringComparison.Ordinal);
            }

            public override int GetHashCode(DiagnosticDescriptor obj)
            {
                return _ordinalStringComparer.GetHashCode(obj?.Id);
            }
        }
    }
}
