﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp
{
    public static class SymbolExtensions
    {
        private static SymbolDisplayFormat DefaultSymbolDisplayFormat { get; } = new SymbolDisplayFormat(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

        #region INamespaceOrTypeSymbol
        public static TypeSyntax ToTypeSyntax(this INamespaceOrTypeSymbol namespaceOrTypeSymbol, SymbolDisplayFormat format = null)
        {
            if (namespaceOrTypeSymbol == null)
                throw new ArgumentNullException(nameof(namespaceOrTypeSymbol));

            if (namespaceOrTypeSymbol.IsType)
            {
                return ToTypeSyntax((ITypeSymbol)namespaceOrTypeSymbol, format);
            }
            else
            {
                return ToTypeSyntax((INamespaceSymbol)namespaceOrTypeSymbol, format);
            }
        }

        public static TypeSyntax ToMinimalTypeSyntax(this INamespaceOrTypeSymbol namespaceOrTypeSymbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            if (namespaceOrTypeSymbol == null)
                throw new ArgumentNullException(nameof(namespaceOrTypeSymbol));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (namespaceOrTypeSymbol.IsType)
            {
                return ToMinimalTypeSyntax((ITypeSymbol)namespaceOrTypeSymbol, semanticModel, position, format);
            }
            else
            {
                return ToMinimalTypeSyntax((INamespaceSymbol)namespaceOrTypeSymbol, semanticModel, position, format);
            }
        }
        #endregion INamespaceOrTypeSymbol

        #region INamespaceSymbol
        public static TypeSyntax ToTypeSyntax(this INamespaceSymbol namespaceSymbol, SymbolDisplayFormat format = null)
        {
            if (namespaceSymbol == null)
                throw new ArgumentNullException(nameof(namespaceSymbol));

            ThrowIfExplicitDeclarationIsNotSupported(namespaceSymbol);

            return ParseTypeName(namespaceSymbol.ToDisplayString(format ?? DefaultSymbolDisplayFormat));
        }

        public static TypeSyntax ToMinimalTypeSyntax(this INamespaceSymbol namespaceSymbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            if (namespaceSymbol == null)
                throw new ArgumentNullException(nameof(namespaceSymbol));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            ThrowIfExplicitDeclarationIsNotSupported(namespaceSymbol);

            return ParseTypeName(namespaceSymbol.ToMinimalDisplayString(semanticModel, position, format ?? DefaultSymbolDisplayFormat));
        }

        private static void ThrowIfExplicitDeclarationIsNotSupported(INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol.NamespaceKind != NamespaceKind.Module)
                throw new ArgumentException($"Namespace '{namespaceSymbol.ToDisplayString()}' does not support explicit declaration.", nameof(namespaceSymbol));
        }
        #endregion INamespaceSymbol

        #region IParameterSymbol
        internal static ExpressionSyntax GetDefaultValueMinimalSyntax(this IParameterSymbol parameterSymbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            if (parameterSymbol == null)
                throw new ArgumentNullException(nameof(parameterSymbol));

            if (!parameterSymbol.HasExplicitDefaultValue)
                throw new ArgumentException("Parameter does not specify default value.", nameof(parameterSymbol));

            object value = parameterSymbol.ExplicitDefaultValue;

            ITypeSymbol typeSymbol = parameterSymbol.Type;

            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                if (value == null)
                    return NullLiteralExpression();

                IFieldSymbol fieldSymbol = typeSymbol.FindFieldWithConstantValue(value);

                TypeSyntax type = typeSymbol.ToMinimalTypeSyntax(semanticModel, position, format);

                if (fieldSymbol != null)
                {
                    return SimpleMemberAccessExpression(type, IdentifierName(fieldSymbol.Name));
                }
                else
                {
                    return CastExpression(type, LiteralExpression(value));
                }
            }

            if (value == null
                && !typeSymbol.IsReferenceOrNullableType())
            {
                return DefaultExpression(typeSymbol.ToMinimalTypeSyntax(semanticModel, position, format));
            }

            return LiteralExpression(value);
        }
        #endregion IParameterSymbol

        #region ITypeSymbol
        public static TypeSyntax ToTypeSyntax(this ITypeSymbol typeSymbol, SymbolDisplayFormat format = null)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            ThrowIfExplicitDeclarationIsNotSupported(typeSymbol);

            return ParseTypeName(typeSymbol.ToDisplayString(format ?? DefaultSymbolDisplayFormat));
        }

        public static TypeSyntax ToMinimalTypeSyntax(this ITypeSymbol typeSymbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            ThrowIfExplicitDeclarationIsNotSupported(typeSymbol);

            return ParseTypeName(typeSymbol.ToMinimalDisplayString(semanticModel, position, format ?? DefaultSymbolDisplayFormat));
        }

        private static void ThrowIfExplicitDeclarationIsNotSupported(ITypeSymbol typeSymbol)
        {
            if (!typeSymbol.SupportsExplicitDeclaration())
                throw new ArgumentException($"Type '{typeSymbol.ToDisplayString()}' does not support explicit declaration.", nameof(typeSymbol));
        }

        public static ExpressionSyntax GetDefaultValueSyntax(this ITypeSymbol typeSymbol, TypeSyntax type)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return GetDefaultValueSyntaxImpl(typeSymbol, type, default(SemanticModel), -1, default(SymbolDisplayFormat));
        }

        public static ExpressionSyntax GetDefaultValueSyntax(this ITypeSymbol typeSymbol, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            return GetDefaultValueSyntaxImpl(typeSymbol, default(TypeSyntax), semanticModel, position, format);
        }

        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/default-values-table
        private static ExpressionSyntax GetDefaultValueSyntaxImpl(ITypeSymbol typeSymbol, TypeSyntax type, SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
        {
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return FalseLiteralExpression();
                case SpecialType.System_Char:
                    return CharacterLiteralExpression('\0');
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return NumericLiteralExpression(0);
            }

            if (typeSymbol.IsConstructedFrom(SpecialType.System_Nullable_T))
                return NullLiteralExpression();

            if (typeSymbol.BaseType?.SpecialType == SpecialType.System_Enum)
            {
                IFieldSymbol fieldSymbol = typeSymbol.FindFieldWithConstantValue(0);

                if (fieldSymbol != null)
                {
                    type = type ?? (typeSymbol.ToMinimalTypeSyntax(semanticModel, position, format));

                    Debug.Assert(type != null);

                    return SimpleMemberAccessExpression(type, IdentifierName(fieldSymbol.Name));
                }
                else
                {
                    return NumericLiteralExpression(0);
                }
            }

            if (typeSymbol.IsReferenceType)
                return NullLiteralExpression();

            type = type ?? (typeSymbol.ToMinimalTypeSyntax(semanticModel, position, format));

            Debug.Assert(type != null);

            return DefaultExpression(type);
        }

        public static bool SupportsConstantValue(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Decimal:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_String:
                    return true;
                default:
                    return typeSymbol.TypeKind == TypeKind.Enum;
            }
        }

        internal static bool SupportsPrefixOrPostfixUnaryOperator(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return CSharpFacts.SupportsPrefixOrPostfixUnaryOperator(typeSymbol.SpecialType)
                || typeSymbol.TypeKind == TypeKind.Enum;
        }
        #endregion ITypeSymbol
    }
}
