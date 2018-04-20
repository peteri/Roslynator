// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.Syntax.SyntaxInfoHelpers;

namespace Roslynator.CSharp.Syntax
{
    //TODO: make public
    //TODO: ThisQualifiedExpressionInfo, ExpressionQualifiedWithThisInfo
    /// <summary>
    /// Provides information about simple member access expression with "this" expression.
    /// </summary>
    internal readonly struct ThisMemberAccessExpressionInfo : IEquatable<ThisMemberAccessExpressionInfo>
    {
        private ThisMemberAccessExpressionInfo(MemberAccessExpressionSyntax memberAccessExpression)
        {
            MemberAccessExpression = memberAccessExpression;
        }

        private static ThisMemberAccessExpressionInfo Default { get; } = new ThisMemberAccessExpressionInfo();

        /// <summary>
        /// The member access expression.
        /// </summary>
        public MemberAccessExpressionSyntax MemberAccessExpression { get; }

        /// <summary>
        /// The expression that contains the member being invoked.
        /// </summary>
        public ThisExpressionSyntax ThisExpression
        {
            get { return (ThisExpressionSyntax)MemberAccessExpression?.Expression; }
        }

        /// <summary>
        /// The name of the member being invoked.
        /// </summary>
        public SimpleNameSyntax Name
        {
            get { return MemberAccessExpression?.Name; }
        }

        /// <summary>
        /// The operator in the member access expression.
        /// </summary>
        public SyntaxToken OperatorToken
        {
            get { return MemberAccessExpression?.OperatorToken ?? default(SyntaxToken); }
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
            get { return MemberAccessExpression != null; }
        }

        internal static ThisMemberAccessExpressionInfo Create(
            SyntaxNode node,
            bool walkDownParentheses = true,
            bool allowMissing = false)
        {
            return Create(
                Walk(node, walkDownParentheses) as MemberAccessExpressionSyntax,
                allowMissing);
        }

        internal static ThisMemberAccessExpressionInfo Create(
            MemberAccessExpressionSyntax memberAccessExpression,
            bool walkDownParentheses = true,
            bool allowMissing = false)
        {
            if (memberAccessExpression?.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return Default;

            if (Walk(memberAccessExpression.Expression, walkDownParentheses)?.Kind() != SyntaxKind.ThisExpression)
                return Default;

            if (!Check(memberAccessExpression.Name, allowMissing))
                return Default;

            return new ThisMemberAccessExpressionInfo(memberAccessExpression);
        }

        /// <summary>
        /// Returns the string representation of the underlying syntax, not including its leading and trailing trivia.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MemberAccessExpression?.ToString() ?? "";
        }

        /// <summary>
        /// Determines whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            return obj is ThisMemberAccessExpressionInfo other && Equals(other);
        }

        /// <summary>
        /// Determines whether this instance is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(ThisMemberAccessExpressionInfo other)
        {
            return EqualityComparer<MemberAccessExpressionSyntax>.Default.Equals(MemberAccessExpression, other.MemberAccessExpression);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return EqualityComparer<MemberAccessExpressionSyntax>.Default.GetHashCode(MemberAccessExpression);
        }

        public static bool operator ==(ThisMemberAccessExpressionInfo info1, ThisMemberAccessExpressionInfo info2)
        {
            return info1.Equals(info2);
        }

        public static bool operator !=(ThisMemberAccessExpressionInfo info1, ThisMemberAccessExpressionInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
