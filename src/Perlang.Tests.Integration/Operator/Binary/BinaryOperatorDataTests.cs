using System;
using System.Collections.Concurrent;
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
    void Greater_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Greater, Greater_unsupported_types, "Greater");
    }

    [Fact]
    void Greater_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Greater, "Greater");
    }

    [Fact]
    void GreaterEqual_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(GreaterEqual, GreaterEqual_unsupported_types, "GreaterEqual");
    }

    [Fact]
    void GreaterEqual_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(GreaterEqual, "GreaterEqual");
    }

    [Fact]
    void Less_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Less, Less_unsupported_types, "Less");
    }

    [Fact]
    void Less_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Less, "Less");
    }

    [Fact]
    void LessEqual_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(LessEqual, LessEqual_unsupported_types, "LessEqual");
    }

    [Fact]
    void LessEqual_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(LessEqual, "LessEqual");
    }

    [Fact]
    void NotEqual_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(NotEqual, ImmutableList<object[]>.Empty, "NotEqual");
    }

    [Fact]
    void NotEqual_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(NotEqual, "NotEqual");
    }

    [Fact]
    void Equal_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Equal, ImmutableList<object[]>.Empty, "Equal");
    }

    [Fact]
    void Equal_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Equal, "Equal");
    }

    [Fact]
    void Subtraction_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Subtraction_result, Subtraction_unsupported_types, "Subtraction");
    }

    [Fact]
    void Subtraction_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Subtraction_result, "Subtraction");
    }

    [Fact]
    void SubtractionAssignment_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(SubtractionAssignment_result, SubtractionAssignment_unsupported_types, "SubtractionAssignment");
    }

    [Fact]
    void SubtractionAssignment_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(SubtractionAssignment_result, "SubtractionAssignment");
    }

    [Fact]
    void Addition_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Addition_result, Addition_unsupported_types, "Addition");
    }

    [Fact]
    void Addition_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Addition_result, "Addition");
    }

    [Fact]
    void AdditionAssignment_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(AdditionAssignment_result, AdditionAssignment_unsupported_types, "AdditionAssignment");
    }

    [Fact]
    void AdditionAssignment_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(AdditionAssignment_result, "AdditionAssignment");
    }

    [Fact]
    void Division_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Division_result, Division_unsupported_types, "Division");
    }

    [Fact]
    void Division_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Division_result, "Division");
    }

    [Fact]
    void Multiplication_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Multiplication_result, Multiplication_unsupported_types, "Multiplication");
    }

    [Fact]
    void Multiplication_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Multiplication_result, "Multiplication");
    }

    [Fact]
    void Exponential_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Exponential_result, Exponential_unsupported_types, "Exponential");
    }

    // EnsureAllPrimitiveTypePairsAreHandled deliberately does not test for the exponential operator, since it only
    // supports a limited subset of operand types.

    [Fact]
    void Modulo_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(Modulo_result, Modulo_unsupported_types, "Modulo");
    }

    [Fact]
    void Modulo_has_test_data_for_all_primitive_type_pairs()
    {
        EnsureAllPrimitiveTypePairsAreHandled(Modulo_result, "Modulo");
    }

    [Fact]
    void ShiftLeft_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(ShiftLeft_result, ShiftLeft_unsupported_types, "ShiftLeft");
    }

    [Fact]
    void ShiftRight_has_test_data_for_all_supported_primitive_types()
    {
        EnsureAllPrimitiveTypesAreHandled(ShiftRight_result, ShiftRight_unsupported_types, "ShiftRight");
    }

    // ShiftLeft and ShiftRight deliberately not tested with EnsureAllPrimitiveTypePairsAreHandled

    private static readonly ConcurrentDictionary<string, Type> TypesForEvaluatedValues = new();

    private static void EnsureAllPrimitiveTypesAreHandled(IEnumerable<object[]> supportedResults, IEnumerable<object[]> unsupportedTypes, string operatorName)
    {
        var knownPrimitiveTypes = TypeCoercer.SignedIntegerLengthByType.Keys
            .Concat(TypeCoercer.UnsignedIntegerLengthByType.Keys)
            .Concat(TypeCoercer.FloatIntegerLengthByType.Keys)
            .ToImmutableList();

        var data = supportedResults.Concat(unsupportedTypes);

        var seenTypeCombinations = new HashSet<(Type, Type)>();

        foreach (object[] objects in data)
        {
            string o1 = (string)objects[0];
            string o2 = (string)objects[1];

            if (!TypesForEvaluatedValues.ContainsKey(o1))
            {
                TypesForEvaluatedValues[o1] = EvalHelper.Eval(o1).GetType();
            }

            if (!TypesForEvaluatedValues.ContainsKey(o2))
            {
                TypesForEvaluatedValues[o2] = EvalHelper.Eval(o2).GetType();
            }

            Type leftType = TypesForEvaluatedValues[o1];
            Type rightType = TypesForEvaluatedValues[o2];

            seenTypeCombinations.Add((leftType, rightType));
        }

        var unhandledTypeCombinations = new HashSet<(Type LeftType, Type RightType)>();

        foreach (Type knownLeftType in knownPrimitiveTypes)
        {
            foreach (Type knownRightType in knownPrimitiveTypes)
            {
                (Type, Type) knownTypesTuple = (knownLeftType, knownRightType);

                if (!seenTypeCombinations.Contains(knownTypesTuple))
                {
                    unhandledTypeCombinations.Add(knownTypesTuple);
                }
            }
        }

        unhandledTypeCombinations.Should()
            .BeEmpty($"'{operatorName}' operator test data contains all possible primitive type combinations");
    }

    private static void EnsureAllPrimitiveTypePairsAreHandled(IEnumerable<object[]> supportedResults, string operatorName)
    {
        // "Type pair" is to be interpreted in this context as `int + int`, `long + long` etc.

        var knownPrimitiveTypes = TypeCoercer.SignedIntegerLengthByType.Keys
            .Concat(TypeCoercer.UnsignedIntegerLengthByType.Keys)
            .Concat(TypeCoercer.FloatIntegerLengthByType.Keys)
            .ToImmutableList();

        var supportedTypeCombinations = new HashSet<(Type, Type)>();

        foreach (object[] objects in supportedResults)
        {
            string o1 = (string)objects[0];
            string o2 = (string)objects[1];

            if (!TypesForEvaluatedValues.ContainsKey(o1))
            {
                TypesForEvaluatedValues[o1] = EvalHelper.Eval(o1).GetType();
            }

            if (!TypesForEvaluatedValues.ContainsKey(o2))
            {
                TypesForEvaluatedValues[o2] = EvalHelper.Eval(o2).GetType();
            }

            Type leftType = TypesForEvaluatedValues[o1];
            Type rightType = TypesForEvaluatedValues[o2];

            supportedTypeCombinations.Add((leftType, rightType));
        }

        // This check ensures that all supported builtin types (as defined by knownTypes) where left+right operands are
        // the same are present in `supportedResults`, i.e. that `int + int`, `uint * uint`. This serves as a bit of a
        // "safety net" to ensure that we have not forgot to implement a particular binary operator when adding a new
        // type.
        //
        // If this assertion fails, make sure to go through ALL the test data for the binary operator in question, to
        // ensure that other type combinations which are "expected to work" have the expected semantics!
        foreach (Type knownType in knownPrimitiveTypes)
        {
            supportedTypeCombinations.Should().Contain((knownType, knownType), $"'{operatorName}' should handle all primitive type pairs (e.g. `int + int`)");
        }
    }

    /// <summary>
    /// Custom formatter which emits newlines after each element, which is helpful when the output contains many lines.
    /// </summary>
    private sealed class HashSetFormatter : IValueFormatter
    {
        private const int MaxElementsDisplayed = 10;

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

            for (int i = 0; i < Math.Min(MaxElementsDisplayed, list.Count); i++)
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

            if (list.Count > MaxElementsDisplayed)
            {
                formattedGraph.AddFragment(" - (...)");
            }

            if (list.Count == 0)
            {
                formattedGraph.AddFragment("{empty}");
            }
        }
    }
}
