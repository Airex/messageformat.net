﻿// CharHelper.cs
// - MessageFormat
// -- Jeffijoe.MessageFormat
// 
// Author: Jeff Hansen <jeff@jeffijoe.com>
// Copyright © 2014.
namespace Jeffijoe.MessageFormat.Helpers
{
    /// <summary>
    /// Char helper
    /// </summary>
    internal static class CharHelper
    {
        private static readonly char[] Alphanumberic = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        

        /// <summary>
        /// Determines whether the specified character is alpha numeric.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns></returns>
         internal static bool IsAlphaNumeric(this char c)
         {
             foreach (var chr in Alphanumberic)
             {
                 if (chr == c)
                     return true;
             }
             return false;
         }

         /// <summary>
         /// Determines whether the specified char is whitespace (space, tab, carriage return, line feed).
         /// </summary>
         /// <param name="c">The c.</param>
         /// <returns></returns>
        internal static bool IsWhitespace(this char c)
        {
            return (c == ' ' || c == '\r' || c == '\n' || c == '\t');
        }
    }
}