﻿using System;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;

#if !NO_INLINE
using System.Runtime.CompilerServices;
#endif

namespace WeCantSpell.Hunspell.Infrastructure
{
    static class HunspellTextFunctions
    {
        public static bool IsReverseSubset(string s1, string s2)
        {
#if DEBUG
            if (s1 == null) throw new ArgumentNullException(nameof(s1));
            if (s2 == null) throw new ArgumentNullException(nameof(s2));
#endif
            if (s2.Length < s1.Length)
            {
                return false;
            }

            for (int index1 = 0, index2 = s2.Length - 1; index1 < s1.Length; index1++, index2--)
            {
                if (s1[index1] != '.' && s1[index1] != s2[index2])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSubset(string s1, string s2)
        {
            if (s1.Length > s2.Length)
            {
                return false;
            }

            for (var i = 0; i < s1.Length; i++)
            {
                if (s1[i] != '.' && s1[i] != s2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSubset(string s1, ReadOnlySpan<char> s2)
        {
            if (s1.Length > s2.Length)
            {
                return false;
            }

            for (var i = 0; i < s1.Length; i++)
            {
                if (s1[i] != '.' && s1[i] != s2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsNumericWord(ReadOnlySpan<char> word)
        {
            byte state = 0; // 0 = begin, 1 = number, 2 = separator
            for (var i = 0; i < word.Length; i++)
            {
                var c = word[i];
                if (char.IsNumber(c))
                {
                    state = 1;
                }
                else if (c == ',' || c == '.' || c == '-')
                {
                    if (state != 1)
                    {
                        return false;
                    }

                    state = 2;
                }
                else
                {
                    return false;
                }
            }

            return state == 1;
        }

        public static int CountMatchingFromLeft(ReadOnlySpan<char> text, char character)
        {
            var count = 0;
            for (; count < text.Length && text[count] == character; count++) ;

            return count;
        }

        public static int CountMatchingFromRight(ReadOnlySpan<char> text, char character)
        {
            var lastIndex = text.Length - 1;
            var searchIndex = lastIndex;
            for (; searchIndex >= 0 && text[searchIndex] == character; searchIndex--) ;

            return lastIndex - searchIndex;
        }

#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool MyIsAlpha(char ch) => ch < 128 || char.IsLetter(ch);

#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CharIsNotNeutral(char c, TextInfo textInfo) =>
            (c < 127 || textInfo.ToUpper(c) != c) && char.IsLower(c);

        public static string RemoveChars(this string @this, CharacterSet chars)
        {
#if DEBUG
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            if (chars == null) throw new ArgumentNullException(nameof(chars));
#endif

            if (@this.Length == 0 || chars.IsEmpty)
            {
                return @this;
            }

            var thisSpan = @this.AsSpan();
            var index = thisSpan.IndexOfAny(chars);
            if (index < 0)
            {
                return @this;
            }

            var lastIndex = thisSpan.Length - 1;
            if (index == lastIndex)
            {
                return @this.Substring(0, lastIndex);
            }

            var builder = StringBuilderPool.Get(lastIndex);
            builder.Append(thisSpan.Slice(0, index));
            index++;
            for (; index < thisSpan.Length; index++)
            {
                ref readonly var c = ref thisSpan[index];
                if (!chars.Contains(c))
                {
                    builder.Append(c);
                }
            }

            return StringBuilderPool.GetStringAndReturn(builder);
        }

        public static string MakeInitCap(string s, TextInfo textInfo)
        {
#if DEBUG
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif
            if (s.Length == 0)
            {
                return s;
            }

            var actualFirstLetter = s[0];
            var expectedFirstLetter = textInfo.ToUpper(actualFirstLetter);
            if (expectedFirstLetter == actualFirstLetter)
            {
                return s;
            }

            if (s.Length == 1)
            {
                return expectedFirstLetter.ToString();
            }

            var builder = StringBuilderPool.Get(s);
            builder[0] = expectedFirstLetter;
            return StringBuilderPool.GetStringAndReturn(builder);
        }

        public static string MakeInitCap(ReadOnlySpan<char> s, TextInfo textInfo)
        {
#if DEBUG
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif
            if (s.IsEmpty)
            {
                return string.Empty;
            }

            var actualFirstLetter = s[0];
            var expectedFirstLetter = textInfo.ToUpper(actualFirstLetter);
            if (expectedFirstLetter == actualFirstLetter)
            {
                return s.ToString();
            }

            if (s.Length == 1)
            {
                return expectedFirstLetter.ToString();
            }

            var builder = StringBuilderPool.Get(s);
            builder[0] = expectedFirstLetter;
            return StringBuilderPool.GetStringAndReturn(builder);
        }

        public static ReadOnlyMemory<char> MakeInitCap(ReadOnlyMemory<char> s, TextInfo textInfo)
        {
#if DEBUG
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif
            if (s.IsEmpty)
            {
                return ReadOnlyMemory<char>.Empty;
            }

            var actualFirstLetter = s.Span[0];
            var expectedFirstLetter = textInfo.ToUpper(actualFirstLetter);
            if (expectedFirstLetter == actualFirstLetter)
            {
                return s;
            }

            if (s.Length == 1)
            {
                return expectedFirstLetter.ToString().AsMemory();
            }

            var builder = StringBuilderPool.Get(s);
            builder[0] = expectedFirstLetter;
            return StringBuilderPool.GetStringAndReturn(builder).AsMemory();
        }

        /// <summary>
        /// Convert to all little.
        /// </summary>
        public static string MakeAllSmall(string s, TextInfo textInfo)
        {
#if DEBUG
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif
            return textInfo.ToLower(s);
        }

        /// <summary>
        /// Convert to all little.
        /// </summary>
        public static Memory<char> MakeAllSmall(ReadOnlyMemory<char> s, CultureInfo culture)
        {
#if DEBUG
            if (culture == null) throw new ArgumentNullException(nameof(culture));
#endif
            var buffer = new char[s.Length].AsMemory();
            s.Span.ToLower(buffer.Span, culture);
            return buffer;
        }

        /// <summary>
        /// Convert to all little.
        /// </summary>
        public static Span<char> MakeAllSmall(ReadOnlySpan<char> s, CultureInfo culture)
        {
#if DEBUG
            if (culture == null) throw new ArgumentNullException(nameof(culture));
#endif
            var buffer = new char[s.Length].AsSpan();
            s.ToLower(buffer, culture);
            return buffer;
        }

        public static string MakeInitSmall(ReadOnlySpan<char> s, TextInfo textInfo)
        {
#if DEBUG
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif

            if (s.Length == 0)
            {
                return string.Empty;
            }

            var actualFirstLetter = s[0];
            var expectedFirstLetter = textInfo.ToLower(actualFirstLetter);
            if (expectedFirstLetter == actualFirstLetter)
            {
                return s.ToString();
            }

            if (s.Length == 1)
            {
                return expectedFirstLetter.ToString();
            }

            var builder = StringBuilderPool.Get(s);
            builder[0] = expectedFirstLetter;
            return StringBuilderPool.GetStringAndReturn(builder);
        }

        public static ReadOnlyMemory<char> MakeInitSmall(ReadOnlyMemory<char> s, TextInfo textInfo)
        {
#if DEBUG
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif

            if (s.Length == 0)
            {
                return ReadOnlyMemory<char>.Empty;
            }

            var actualFirstLetter = s.Span[0];
            var expectedFirstLetter = textInfo.ToLower(actualFirstLetter);
            if (expectedFirstLetter == actualFirstLetter)
            {
                return s;
            }

            if (s.Length == 1)
            {
                return expectedFirstLetter.ToString().AsMemory();
            }

            var builder = StringBuilderPool.Get(s);
            builder[0] = expectedFirstLetter;
            return StringBuilderPool.GetStringAndReturn(builder).AsMemory();
        }

        public static string MakeAllCap(string s, TextInfo textInfo)
        {
#if DEBUG
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (textInfo == null) throw new ArgumentNullException(nameof(textInfo));
#endif
            return textInfo.ToUpper(s);
        }

        public static Span<char> MakeAllCap(ReadOnlySpan<char> s, CultureInfo culture)
        {
#if DEBUG
            if (culture == null) throw new ArgumentNullException(nameof(culture));
#endif
            var buffer = new char[s.Length].AsSpan();
            s.ToUpper(buffer, culture);
            return buffer;
        }

        public static ReadOnlySpan<char> MakeTitleCase(ReadOnlySpan<char> s, CultureInfo cultureInfo)
        {
#if DEBUG
            if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));
#endif

            if (s.IsEmpty)
            {
                return s;
            }

            var buffer = new char[s.Length];
            s.Slice(0, 1).ToUpper(buffer.AsSpan(0, 1), cultureInfo);
            if (s.Length > 1)
            {
                s.Slice(1).ToLower(buffer.AsSpan(1), cultureInfo);
            }
            return new ReadOnlySpan<char>(buffer);
        }

        public static ReadOnlyMemory<char> MakeTitleCase(ReadOnlyMemory<char> s, CultureInfo cultureInfo)
        {
#if DEBUG
            if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));
#endif

            if (s.IsEmpty)
            {
                return s;
            }

            var span = s.Span;
            var buffer = new char[span.Length];
            span.Slice(0, 1).ToUpper(buffer.AsSpan(0, 1), cultureInfo);
            if (span.Length > 1)
            {
                span.Slice(1).ToLower(buffer.AsSpan(1), cultureInfo);
            }
            return buffer.AsMemory();
        }

        public static ReadOnlySpan<char> ReDecodeConvertedStringAsUtf8(ReadOnlySpan<char> decoded, Encoding encoding)
        {
            if (Encoding.UTF8.Equals(encoding))
            {
                return decoded;
            }

            var encodedBytes = encoding.GetBytes(decoded.ToArray());
            return Encoding.UTF8.GetString(encodedBytes, 0, encodedBytes.Length).AsSpan();
        }

        public static ReadOnlyMemory<char> ReDecodeConvertedStringAsUtf8(ReadOnlyMemory<char> decoded, Encoding encoding)
        {
            if (Encoding.UTF8.Equals(encoding))
            {
                return decoded;
            }

            var encodedBytes = encoding.GetBytes(decoded.ToArray());
            return Encoding.UTF8.GetString(encodedBytes, 0, encodedBytes.Length).AsMemory();
        }

        public static CapitalizationType GetCapitalizationType(ReadOnlySpan<char> word, TextInfo textInfo)
        {
            if (word.IsEmpty)
            {
                return CapitalizationType.None;
            }

            var hasFoundMoreCaps = false;
            var firstIsUpper = false;
            var hasLower = false;

            for (int i = 0; i < word.Length; i++)
            {
                ref readonly var c = ref word[i];

                if (!hasFoundMoreCaps && char.IsUpper(c))
                {
                    if (i == 0)
                    {
                        firstIsUpper = true;
                    }
                    else
                    {
                        hasFoundMoreCaps = true;
                    }

                    if (hasLower)
                    {
                        break;
                    }
                }
                else if (!hasLower && CharIsNotNeutral(c, textInfo))
                {
                    hasLower = true;
                    if (hasFoundMoreCaps)
                    {
                        break;
                    }
                }
            }

            if (firstIsUpper)
            {
                if (!hasFoundMoreCaps)
                {
                    return CapitalizationType.Init;
                }
                if (!hasLower)
                {
                    return CapitalizationType.All;
                }

                return CapitalizationType.HuhInit;
            }
            else
            {
                if (!hasFoundMoreCaps)
                {
                    return CapitalizationType.None;
                }
                if (!hasLower)
                {
                    return CapitalizationType.All;
                }

                return CapitalizationType.Huh;
            }
        }

        public static bool TestTripleLetters(ReadOnlySpan<char> word, int i) =>
            // test triple letters
            i <= 0
            ||
            i >= word.Length
            ||
            word[i - 1] != word[i]
            ||
            (
                (i < 2 || word[i - 1] != word[i - 2])
                &&
                (i + 1 >= word.Length || word[i - 1] != word[i + 1]) // may be word[i+1] == '\0'
            );

        public static bool TestSimpleDoubleLetter(ReadOnlySpan<char> word, int i) => word[i - 1] == word[i - 2];
    }
}
