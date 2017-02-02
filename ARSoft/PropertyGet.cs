using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Sharp.Xmpp.ARSoft
{

    static class PropertyGet
    {
        /// <summary>
        /// Get PropertyName like "nameof()"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> e)
        {
            var memberEx = (MemberExpression)e.Body;
            return memberEx.Member.Name;
        }
    }
}
