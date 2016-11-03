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
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class NpgsqlSqlTranslatingExpressionVisitorFactory : ISqlTranslatingExpressionVisitorFactory
    {
        readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        readonly IExpressionFragmentTranslator _compositeExpressionFragmentTranslator;
        readonly IMethodCallTranslator _methodCallTranslator;
        readonly IMemberTranslator _memberTranslator;
        readonly IRelationalTypeMapper _relationalTypeMapper;

        /// <summary>
        ///     Creates a new instance of <see cref="SqlTranslatingExpressionVisitorFactory" />.
        /// </summary>
        /// <param name="relationalAnnotationProvider"> The relational annotation provider. </param>
        /// <param name="compositeExpressionFragmentTranslator"> The composite expression fragment translator. </param>
        /// <param name="methodCallTranslator"> The method call translator. </param>
        /// <param name="memberTranslator"> The member translator. </param>
        /// <param name="relationalTypeMapper"> The relational type mapper. </param>
        public NpgsqlSqlTranslatingExpressionVisitorFactory(
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IExpressionFragmentTranslator compositeExpressionFragmentTranslator,
            [NotNull] IMethodCallTranslator methodCallTranslator,
            [NotNull] IMemberTranslator memberTranslator,
            [NotNull] IRelationalTypeMapper relationalTypeMapper)
        {
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(compositeExpressionFragmentTranslator, nameof(compositeExpressionFragmentTranslator));
            Check.NotNull(methodCallTranslator, nameof(methodCallTranslator));
            Check.NotNull(memberTranslator, nameof(memberTranslator));
            Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper));

            _relationalAnnotationProvider = relationalAnnotationProvider;
            _compositeExpressionFragmentTranslator = compositeExpressionFragmentTranslator;
            _methodCallTranslator = methodCallTranslator;
            _memberTranslator = memberTranslator;
            _relationalTypeMapper = relationalTypeMapper;
        }

        /// <summary>
        ///     Creates a new NpgsqlSqlTranslatingExpressionVisitor.
        /// </summary>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="targetSelectExpression"> The target select expression. </param>
        /// <param name="topLevelPredicate"> The top level predicate. </param>
        /// <param name="bindParentQueries"> true to bind parent queries. </param>
        /// <param name="inProjection"> true if we are translating a projection. </param>
        /// <returns>
        ///     An NpgsqlSqlTranslatingExpressionVisitor.
        /// </returns>
        public virtual SqlTranslatingExpressionVisitor Create(
            RelationalQueryModelVisitor queryModelVisitor,
            SelectExpression targetSelectExpression = null,
            Expression topLevelPredicate = null,
            bool bindParentQueries = false,
            bool inProjection = false)
            => new NpgsqlSqlTranslatingExpressionVisitor(
                _relationalAnnotationProvider,
                _compositeExpressionFragmentTranslator,
                _methodCallTranslator,
                _memberTranslator,
                _relationalTypeMapper,
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                targetSelectExpression,
                topLevelPredicate,
                bindParentQueries,
                inProjection);
    }
}
