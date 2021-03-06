﻿using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Sql.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL array ANY or ALL expression.
    /// </summary>
    /// <example>
    /// 1 = ANY ('{0,1,2}'), 'cat' LIKE ANY ('{a%,b%,c%}')
    /// </example>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/functions-comparisons.html
    /// </remarks>
    public class ArrayAnyAllExpression : Expression, IEquatable<ArrayAnyAllExpression>
    {
        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type => typeof(bool);

        /// <summary>
        /// The value to test against the <see cref="Array"/>.
        /// </summary>
        [NotNull]
        public virtual Expression Operand { get; }

        /// <summary>
        /// The array of values or patterns to test for the <see cref="Operand"/>.
        /// </summary>
        [NotNull]
        public virtual Expression Array { get; }

        /// <summary>
        /// The operator.
        /// </summary>
        [NotNull]
        public virtual string Operator { get; }

        /// <summary>
        /// The comparison type.
        /// </summary>
        public virtual ArrayComparisonType ArrayComparisonType { get; }

        /// <summary>
        /// Constructs a <see cref="ArrayAnyAllExpression"/>.
        /// </summary>
        /// <param name="arrayComparisonType">The comparison type.</param>
        /// <param name="operatorSymbol">The operator symbol to the array expression.</param>
        /// <param name="operand">The value to find.</param>
        /// <param name="array">The array to search.</param>
        /// <exception cref="ArgumentNullException" />
        public ArrayAnyAllExpression(
            ArrayComparisonType arrayComparisonType,
            [NotNull] string operatorSymbol,
            [NotNull] Expression operand,
            [NotNull] Expression array)
        {
            ArrayComparisonType = arrayComparisonType;
            Operator = Check.NotNull(operatorSymbol, nameof(operatorSymbol));
            Operand = Check.NotNull(operand, nameof(operand));
            Array = Check.NotNull(array, nameof(array));
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitArrayAnyAll(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = visitor.Visit(Operand) ?? Operand;
            var array = visitor.Visit(Array) ?? Array;

            return
                operand != Operand || array != Array
                    ? new ArrayAnyAllExpression(ArrayComparisonType, Operator, operand, array)
                    : this;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is ArrayAnyAllExpression likeAnyExpression && Equals(likeAnyExpression);

        /// <inheritdoc />
        public bool Equals(ArrayAnyAllExpression other)
            => Operand.Equals(other?.Operand) &&
               Operator.Equals(other?.Operator) &&
               ArrayComparisonType.Equals(other?.ArrayComparisonType) &&
               Array.Equals(other?.Array);

        /// <inheritdoc />
        public override int GetHashCode()
            => unchecked((397 * Operand.GetHashCode()) ^
                         (397 * Operator.GetHashCode()) ^
                         (397 * ArrayComparisonType.GetHashCode()) ^
                         (397 * Array.GetHashCode()));

        /// <inheritdoc />
        public override string ToString() => $"{Operand} {Operator} {ArrayComparisonType.ToString()} ({Array})";
    }

    /// <summary>
    /// Represents whether an array comparison is ANY or ALL.
    /// </summary>
    public enum ArrayComparisonType
    {
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Represents an ANY array comparison.
        /// </summary>
        ANY,

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Represents an ALL array comparison.
        /// </summary>
        ALL
    }
}
