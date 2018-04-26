﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator
{
    public static class TextSpanExtensions
    {
        //TODO: span starts or ends between \r and \n
        //TODO: move to Roslynator.CSharp
        public static LinePositionSpan ToLinePositionSpan(this TextSpan span, string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            int length = s.Length;

            if (span.Start + span.Length > length)
                throw new ArgumentOutOfRangeException(nameof(span), span, "");

            LinePosition start = LinePosition.Zero;

            start = GetLinePosition(0, span.Start);

            LinePosition end = GetLinePosition(span.Start, span.End);

            return new LinePositionSpan(start, end);

            LinePosition GetLinePosition(int startIndex, int endIndex)
            {
                int i = endIndex - 1;

                while (i >= startIndex)
                {
                    if (s[i] == '\r'
                        || s[i] == '\n')
                    {
                        int character = endIndex - i - 1;

                        int line = start.Line;

                        while (i >= startIndex)
                        {
                            switch (s[i])
                            {
                                case '\n':
                                    {
                                        if (i > startIndex
                                            && s[i - 1] == '\r')
                                        {
                                            i--;
                                        }

                                        line++;
                                        break;
                                    }
                                case '\r':
                                    {
                                        line++;
                                        break;
                                    }
                            }

                            i--;
                        }

                        return new LinePosition(line, character);
                    }

                    i--;
                }

                return new LinePosition(start.Line, start.Character + endIndex - startIndex);
            }
        }
    }
}
