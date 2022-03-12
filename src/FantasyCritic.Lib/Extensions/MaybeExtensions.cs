using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Extensions
{
    public static class MaybeExtensions
    {
        public static Maybe<T> ToMaybe<T>(this Result<T> result) where T : class
        {
            if (result.IsSuccess)
            {
                return Maybe<T>.From(result.Value);
            }

            return Maybe<T>.None;
        }

        public static Maybe<string> ToMaybe(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Maybe<string>.None;
            }

            return value;
        }
    }
}
