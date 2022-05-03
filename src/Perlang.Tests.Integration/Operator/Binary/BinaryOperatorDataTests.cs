using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Formatting;
using Perlang.Interpreter.Typing;
using Xunit;
using static Perlang.Tests.Integration.Operator.Binary.BinaryOperatorData;

namespace Perlang.Tests.Integration.Operator.Binary;

/// <summary>
/// Test which ensures that the <see cref="BinaryOperatorData"/> has test data for all valid type combinations, for all
/// kinds of binary operators.
/// </summary>
public class BinaryOperatorDataTests
{
    public BinaryOperatorDataTests()
    {
        Formatter.AddFormatter(new HashSetFormatter());
    }

    [Fact]
    void Greater_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Greater, Greater_unsupported_types, "Greater");
    }

    [Fact]
    void GreaterEqual_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(GreaterEqual, GreaterEqual_unsupported_types, "GreaterEqual");
    }

    [Fact]
    void Less_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Less, Less_unsupported_types, "Less");
    }

    [Fact]
    void LessEqual_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(LessEqual, LessEqual_unsupported_types, "LessEqual");
    }

    [Fact]
    void NotEqual_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(NotEqual, ImmutableList<object[]>.Empty, "NotEqual");
    }

    [Fact]
    void Equal_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Equal, ImmutableList<object[]>.Empty, "Equal");
    }

    [Fact]
    void Subtraction_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Subtraction_result, Subtraction_unsupported_types, "Subtraction");
    }

    [Fact]
    void SubtractionAssignment_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(SubtractionAssignment_result, SubtractionAssignment_unsupported_types, "SubtractionAssignment");
    }

    [Fact]
    void Addition_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Addition_result, Addition_unsupported_types, "Addition");
    }

    [Fact]
    void AdditionAssignment_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(AdditionAssignment_result, AdditionAssignment_unsupported_types, "AdditionAssignment");
    }

    [Fact]
    void Division_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Division_result, Division_unsupported_types, "Division");
    }

    [Fact]
    void Multiplication_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Multiplication_result, Multiplication_unsupported_types, "Multiplication");
    }

    [Fact]
    void Exponential_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Exponential_result, Exponential_unsupported_types, "Exponential");
    }

    [Fact]
    void Modulo_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(Modulo_result, Modulo_unsupported_types, "Modulo");
    }

    [Fact]
    void ShiftLeft_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(ShiftLeft_result, ShiftLeft_unsupported_types, "ShiftLeft");
    }

    [Fact]
    void ShiftRight_has_test_data_for_all_supported_types()
    {
        EnsureAllTypesAreHandled(ShiftRight_result, ShiftRight_unsupported_types, "ShiftRight");
    }

    private static void EnsureAllTypesAreHandled(IEnumerable<object[]> supportedResults, IEnumerable<object[]> unsupportedTypes, string operatorName)
    {
        var data = supportedResults.Concat(unsupportedTypes);

        var knownTypes = TypeCoercer.SignedIntegerLengthByType.Keys
            .Concat(TypeCoercer.UnsignedIntegerLengthByType.Keys)
            .Concat(TypeCoercer.FloatIntegerLengthByType.Keys).ToList();

        var seenTypeCombinations = new HashSet<(Type, Type)>();

        foreach (object[] objects in data)
        {
            Type leftType = EvalHelper.Eval((string)objects[0]).GetType();
            Type rightType = EvalHelper.Eval((string)objects[1]).GetType();

            seenTypeCombinations.Add((leftType, rightType));
        }

        var unhandledTypeCombinations = new HashSet<(Type LeftType, Type RightType)>();

        foreach (Type knownLeftType in knownTypes)
        {
            foreach (Type knownRightType in knownTypes)
            {
                (Type, Type) knownTypesTuple = (knownLeftType, knownRightType);

                if (!seenTypeCombinations.Contains(knownTypesTuple))
                {
                    unhandledTypeCombinations.Add(knownTypesTuple);
                }
            }
        }

        unhandledTypeCombinations.Should()
            .BeEmpty($"'{operatorName}' operator test data contains all possible type combinations");
    }

    /// <summary>
    /// Custom formatter which emits newlines after each element, which is helpful when the output contains many lines.
    /// </summary>
    private sealed class HashSetFormatter : IValueFormatter
    {
        /// <summary>
        /// Indicates whether the current <see cref="IValueFormatter"/> can handle the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value for which to create a <see cref="string"/>.</param>
        /// <returns>
        /// <c>true</c> if the current <see cref="IValueFormatter"/> can handle the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(object value)
        {
            return value is HashSet<(Type, Type)>;
        }

        public void Format(object value, FormattedObjectGraph formattedGraph, FormattingContext context, FormatChild formatChild)
        {
            var list = ((HashSet<(Type, Type)>)value).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                (Type, Type) obj = list[i];

                if (i == 0)
                {
                    formattedGraph.AddLine(":");
                }

                if (i != list.Count - 1)
                {
                    formattedGraph.AddLine(" - " + obj);
                }
                else
                {
                    // Last line. Fluent Assertions will append a period character at this point, so let's just use
                    // AddFragment to avoid a superfluous newline in the output.
                    formattedGraph.AddFragment(" - " + obj);
                }
            }

            if (list.Count == 0)
            {
                formattedGraph.AddFragment("{empty}");
            }
        }
    }
}
