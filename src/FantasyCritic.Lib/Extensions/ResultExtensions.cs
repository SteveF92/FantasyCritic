using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Extensions;
public static class ResultExtensions
{
    public static T? ToNullable<T>(this Result<T> result) where T : class
    {
        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    //public static T ToNullable<T>(Result<T> result) where T : struct
    //{
    //    if (result.IsFailure)
    //    {
    //        return default(T);
    //    }

    //    return result.Value;
    //}
}
