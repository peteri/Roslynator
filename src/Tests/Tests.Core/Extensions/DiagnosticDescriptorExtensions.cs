// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    public static class DiagnosticDescriptorExtensions
    {
        public static DiagnosticDescriptor WithIsEnabledByDefault(this DiagnosticDescriptor descriptor, bool isEnabledByDefault)
        {
            return new DiagnosticDescriptor(
                id: descriptor.Id,
                title: descriptor.Title,
                messageFormat: descriptor.MessageFormat,
                category: descriptor.Category,
                defaultSeverity: descriptor.DefaultSeverity,
                isEnabledByDefault: isEnabledByDefault,
                description: descriptor.Description,
                helpLinkUri: descriptor.HelpLinkUri,
                customTags: descriptor.CustomTags.ToArray());
        }
    }
}
