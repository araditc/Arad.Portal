//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Davood Ghashghaei <ghashghaei@arad-itc.org>
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0 
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  --------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class LinqHelper
    {
        #region public methods
        public static Expression<Func<T, bool>> Begin<T>(bool value = false)
        {
            if (value)
                return parameter => true;  //value cannot be used in place of true/false

            return parameter => false;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
           Expression<Func<T, bool>> right)
        {
            return CombineLambdas(left, right, ExpressionType.AndAlso);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            return CombineLambdas(left, right, ExpressionType.OrElse);
        }
        #endregion 

        #region private methods
        private static Expression<Func<T, bool>> CombineLambdas<T>(this Expression<Func<T, bool>> left,
           Expression<Func<T, bool>> right, ExpressionType expressionType)
        {
            //Remove expressions created with Begin<T>()
            if (IsExpressionBodyConstant(left))
                return (right);

            ParameterExpression p = left.Parameters[0];

            SubstituteParameterVisitor visitor = new SubstituteParameterVisitor();
            visitor.Sub[right.Parameters[0]] = p;

            Expression body = Expression.MakeBinary(expressionType, left.Body, visitor.Visit(right.Body));
            return Expression.Lambda<Func<T, bool>>(body, p);
        }

        private static bool IsExpressionBodyConstant<T>(Expression<Func<T, bool>> left)
        {
            return left.Body.NodeType == ExpressionType.Constant;
        }
        #endregion

        #region internal methods
        internal class SubstituteParameterVisitor : ExpressionVisitor
        {
            public Dictionary<Expression, Expression> Sub = new Dictionary<Expression, Expression>();

            protected override Expression VisitParameter(ParameterExpression node)
            {
                Expression newValue;
                if (Sub.TryGetValue(node, out newValue))
                {
                    return newValue;
                }
                return node;
            }
        }
        #endregion
    }
}
