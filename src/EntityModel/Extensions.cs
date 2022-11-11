using System;
using System.Runtime.CompilerServices;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public static class Extensions
    {
        public static string ToSlugString(this string input) =>
            input.ToLowerInvariant().Replace(" ", "-");

    }
}

