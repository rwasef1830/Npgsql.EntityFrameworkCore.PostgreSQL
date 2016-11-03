using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class NpgsqlSqlTranslatingExpressionVisitor : SqlTranslatingExpressionVisitor
    {
        public NpgsqlSqlTranslatingExpressionVisitor([NotNull] IRelationalAnnotationProvider relationalAnnotationProvider, [NotNull] IExpressionFragmentTranslator compositeExpressionFragmentTranslator, [NotNull] IMethodCallTranslator methodCallTranslator, [NotNull] IMemberTranslator memberTranslator, [NotNull] IRelationalTypeMapper relationalTypeMapper, [NotNull] RelationalQueryModelVisitor queryModelVisitor, [CanBeNull] SelectExpression targetSelectExpression = null, [CanBeNull] Expression topLevelPredicate = null, bool bindParentQueries = false, bool inProjection = false)
            : base(relationalAnnotationProvider, compositeExpressionFragmentTranslator, methodCallTranslator, memberTranslator, relationalTypeMapper, queryModelVisitor, targetSelectExpression, topLevelPredicate, bindParentQueries, inProjection) {}

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var subQueryModel = expression.QueryModel;

            if (subQueryModel.IsIdentityQuery()
                && subQueryModel.ResultOperators.Count == 1
                && subQueryModel.ResultOperators.First() is CountResultOperator
                && subQueryModel.MainFromClause.FromExpression.Type.IsArray
                )
            {
                var fromExpression = subQueryModel.MainFromClause.FromExpression;

                var member = fromExpression as MemberExpression;
                if (member != null)
                {
                    var aliasExpression = VisitMember(member) as AliasExpression;

                    return aliasExpression != null
                        ? Expression.ArrayLength(aliasExpression)
                        : null;
                }
            }

            return base.VisitSubQuery(expression);
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var left = Visit(expression.Left);
                var right = Visit(expression.Right);

                // ReSharper disable once AssignNullToNotNullAttribute
                return left != null && right != null
                    ? Expression.MakeBinary(ExpressionType.ArrayIndex, left, right)
                    : null;
            }
            return base.VisitBinary(expression);
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            Console.WriteLine("Unary node type: " + expression.NodeType);
            if (expression.NodeType == ExpressionType.ArrayLength)
            {
                throw new NotSupportedException("FOO");
            }
            return base.VisitUnary(expression);
        }
    }
}
