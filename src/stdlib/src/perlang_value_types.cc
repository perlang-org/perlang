#include "perlang_type.h"
#include "perlang_value_types.h"

#include <memory>

perlang::PerlangType perlang::PerlangValueTypes::Int32 = perlang::PerlangType("perlang.Int32");
perlang::PerlangType perlang::PerlangValueTypes::Int64 = perlang::PerlangType("perlang.Int64");
perlang::PerlangType perlang::PerlangValueTypes::BigInt = perlang::PerlangType("perlang.BigInt");
perlang::PerlangType perlang::PerlangValueTypes::UInt32 = perlang::PerlangType("perlang.UInt32");
perlang::PerlangType perlang::PerlangValueTypes::UInt64 = perlang::PerlangType("perlang.UInt64");
perlang::PerlangType perlang::PerlangValueTypes::Float = perlang::PerlangType("perlang.Float");
perlang::PerlangType perlang::PerlangValueTypes::Double = perlang::PerlangType("perlang.Double");
perlang::PerlangType perlang::PerlangValueTypes::Bool = perlang::PerlangType("perlang.Bool");

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_Int32()
{
    return std::make_unique<perlang::PerlangType>(Int32);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_Int64()
{
    return std::make_unique<perlang::PerlangType>(Int64);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_BigInt()
{
    return std::make_unique<perlang::PerlangType>(BigInt);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_UInt32()
{
    return std::make_unique<perlang::PerlangType>(UInt32);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_UInt64()
{
    return std::make_unique<perlang::PerlangType>(UInt64);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_Float()
{
    return std::make_unique<perlang::PerlangType>(Float);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_Double()
{
    return std::make_unique<perlang::PerlangType>(Double);
}

[[maybe_unused]]
[[nodiscard]]
std::unique_ptr<perlang::PerlangType> perlang::PerlangValueTypes::get_type_perlang_Bool()
{
    return std::make_unique<perlang::PerlangType>(Bool);
}
