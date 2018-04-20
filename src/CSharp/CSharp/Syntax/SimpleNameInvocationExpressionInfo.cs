// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.Syntax.SyntaxInfoHelpers;

namespace Roslynator.CSharp.Syntax
{
    //TODO: make public
    /// <summary>
    /// Provides information about simple name invocation expression.
    /// </summary>
    internal readonly struct SimpleNameInvocationExpressionInfo : IEquatable<SimpleNameInvocationExpressionInfo>
    {
        private SimpleNameInvocationExpressionInfo(
            InvocationExpressionSyntax invocationExpression,
            SimpleNameSyntax name)
        {
            InvocationExpression = invocationExpression;
            Name = name;
        }

        private static SimpleNameInvocationExpressionInfo Default { get; } = new SimpleNameInvocationExpressionInfo();

        /// <summary>
        /// The invocation expression.
        /// </summary>
        public InvocationExpressionSyntax InvocationExpression { get; }

        /// <summary>
        /// The simple name.
        /// </summary>
        public SimpleNameSyntax Name { get; }

        /// <summary>
        /// The argumet list.
        /// </summary>
        public ArgumentListSyntax ArgumentList
        {
            get { return InvocationExpression?.ArgumentList; }
        }

        /// <summary>
        /// The list of the arguments.
        /// </summary>
        public SeparatedSyntaxList<ArgumentSyntax> Arguments
        {
            get { return InvocationExpression?.ArgumentList.Arguments ?? default(SeparatedSyntaxList<ArgumentSyntax>); }
        }

        /// <summary>
        /// The name of the member being invoked.
        /// </summary>
        public string NameText
        {
            get { return Name?.Identifier.ValueText; }
        }

        /// <summary>
        /// Determines whether this struct was initialized with an actual syntax.
        /// </summary>
        public bool Success
        {
            get { return InvocationExpression != null; }
        }

        internal static SimpleNameInvocationExpressionInfo Create(
            SyntaxNode node,
            bool walkDownParentheses = true,
            bool allowMissing = false)
        {
            return CreateImpl(
                Walk(node, walkDownParentheses) as InvocationExpressionSyntax,
                allowMissing);
        }

        internal static SimpleNameInvocationExpressionInfo Create(
            InvocationExpressionSyntax invocationExpression,
            bool allowMissing = false)
        {
            return CreateImpl(invocationExpression, allowMissing);
        }

        private static SimpleNameInvocationExpressionInfo CreateImpl(
            InvocationExpressionSyntax invocationExpression,
            bool allowMissing = false)
        {
            if (!(invocationExpression?.Expression is SimpleNameSyntax name))
                return Default;

            if (!Check(name, allowMissing))
                return Default;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            if (argumentList == null)
                return Default;

            return new SimpleNameInvocationExpressionInfo(invocationExpression, name);
        }

        /// <summary>
        /// Returns the string representation of the underlying syntax, not including its leading and trailing trivia.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return InvocationExpression?.ToString() ?? "";
        }

        /// <summary>
        /// Determines whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            return obj is SimpleNameInvocationExpressionInfo other && Equals(other);
        }

        /// <summary>
        /// Determines whether this instance is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(SimpleNameInvocationExpressionInfo other)
        {
            return EqualityComparer<InvocationExpressionSyntax>.Default.Equals(InvocationExpression, other.InvocationExpression);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return EqualityComparer<InvocationExpressionSyntax>.Default.GetHashCode(InvocationExpression);
        }

        public static bool operator ==(SimpleNameInvocationExpressionInfo info1, SimpleNameInvocationExpressionInfo info2)
        {
            return info1.Equals(info2);
        }

        public static bool operator !=(SimpleNameInvocationExpressionInfo info1, SimpleNameInvocationExpressionInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
