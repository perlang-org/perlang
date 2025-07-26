#include <memory>

namespace perlang
{
    class PerlangValueTypes
    {
     private:
        // Declared in .cc companion file
        static PerlangType Int32;
        static PerlangType Int64;
        static PerlangType BigInt;
        static PerlangType UInt32;
        static PerlangType UInt64;
        static PerlangType Float;
        static PerlangType Double;
        static PerlangType Bool;

     public:
        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_Int32();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_Int64();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_BigInt();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_UInt32();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_UInt64();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_Float();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_Double();

        [[maybe_unused]]
        [[nodiscard]]
        static std::unique_ptr<PerlangType> get_type_perlang_Bool();
    };
};
