cmake_minimum_required(VERSION 3.10)
project(stdlib VERSION 0.1.0)

Include(FetchContent)

FetchContent_Declare(
        Catch2
        GIT_REPOSITORY https://github.com/catchorg/Catch2.git
        GIT_TAG        v3.7.1
)

FetchContent_MakeAvailable(Catch2)

# This is needed to get CMake to use -fPIC. Without this, linking the Perlang stdlib into a .so shared library will fail
# with relocation errors.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

set(headers
        src/ascii_string.h
        src/bigint.h
        src/bigint_array.h
        src/bool_array.h
        src/char_array.h
        src/double_array.h
        src/float_array.h
        src/int_array.h
        src/integer.h
        src/long_array.h
        src/object.h
        src/object_array.h
        src/perlang_char.h
        src/perlang_stdlib.h
        src/perlang_string.h
        src/perlang_type.h
        src/perlang_value_types.h
        src/string_array.h
        src/uint_array.h
        src/ulong_array.h
        src/utf8_string.h
        src/utf16_string.h
)

set(collections_headers
        src/collections/mutable_string_hash_set.h
        src/collections/string_hash_set.h
)

set(exceptions_headers
        src/exceptions/null_pointer_exception.h
        src/exceptions/illegal_state_exception.h
)

set(io_headers
        src/io/file.h
)

set(libtommath_headers
        src/libtommath/tommath.h
)

set(text_headers
        src/text/string_builder.h
        src/text/string_hashing.h
)

set(tsl_headers
        src/tsl/ordered_hash.h
        src/tsl/ordered_map.h
        src/tsl/ordered_set.h
)

add_library(
        stdlib
        src/ascii_string.cc
        src/Base64.cc
        src/bigint.cc
        src/bigint_array.cc
        src/bigint_mod.cc
        src/bigint_pow.cc
        src/bool_array.cc
        src/char_array.cc
        src/double_array.cc
        src/float_array.cc
        src/int_array.cc
        src/integer.cc
        src/long_array.cc
        src/object.cc
        src/object_array.cc
        src/perlang_char.cc
        src/perlang_string.cc
        src/perlang_type.cc
        src/perlang_value_types.cc
        src/print.cc
        src/string_array.cc
        src/uint_array.cc
        src/ulong_array.cc
        src/utf8_string.cc
        src/utf16_string.cc

        src/collections/mutable_string_hash_set.cc
        src/collections/string_hash_set.cc

        src/io/file.cc

        src/libtommath/bn_cutoffs.c
        src/libtommath/bn_mp_add.c
        src/libtommath/bn_mp_clamp.c
        src/libtommath/bn_mp_clear.c
        src/libtommath/bn_mp_clear_multi.c
        src/libtommath/bn_mp_cmp.c
        src/libtommath/bn_mp_cmp_mag.c
        src/libtommath/bn_mp_copy.c
        src/libtommath/bn_mp_count_bits.c
        src/libtommath/bn_mp_div.c
        src/libtommath/bn_mp_div_d.c
        src/libtommath/bn_mp_div_2.c
        src/libtommath/bn_mp_div_2d.c
        src/libtommath/bn_mp_div_3.c
        src/libtommath/bn_mp_exch.c
        src/libtommath/bn_mp_expt_u32.c
        src/libtommath/bn_mp_grow.c
        src/libtommath/bn_mp_init.c
        src/libtommath/bn_mp_init_copy.c
        src/libtommath/bn_mp_init_multi.c
        src/libtommath/bn_mp_init_size.c
        src/libtommath/bn_mp_lshd.c
        src/libtommath/bn_mp_mod.c
        src/libtommath/bn_mp_mod_2d.c
        src/libtommath/bn_mp_mul.c
        src/libtommath/bn_mp_mul_d.c
        src/libtommath/bn_mp_mul_2.c
        src/libtommath/bn_mp_mul_2d.c
        src/libtommath/bn_mp_neg.c
        src/libtommath/bn_mp_radix_size.c
        src/libtommath/bn_mp_radix_smap.c
        src/libtommath/bn_mp_rshd.c
        src/libtommath/bn_mp_error_to_string.c
        src/libtommath/bn_mp_to_radix.c
        src/libtommath/bn_mp_set.c
        src/libtommath/bn_mp_set_u64.c
        src/libtommath/bn_mp_sqr.c
        src/libtommath/bn_mp_sub.c
        src/libtommath/bn_mp_zero.c
        src/libtommath/bn_s_mp_add.c
        src/libtommath/bn_s_mp_balance_mul.c
        src/libtommath/bn_s_mp_karatsuba_mul.c
        src/libtommath/bn_s_mp_karatsuba_sqr.c
        src/libtommath/bn_s_mp_mul_digs.c
        src/libtommath/bn_s_mp_mul_digs_fast.c
        src/libtommath/bn_s_mp_sqr.c
        src/libtommath/bn_s_mp_sqr_fast.c
        src/libtommath/bn_s_mp_reverse.c
        src/libtommath/bn_s_mp_sub.c
        src/libtommath/bn_s_mp_toom_mul.c
        src/libtommath/bn_s_mp_toom_sqr.c

        src/text/string_builder.cc
        src/perlang_value_types.h
)

# Enable C++ 17 support. This is required on e.g. AppleClang 13
# TODO: Upgrade this to C++ 20, so we could use things like std::format
set_property(TARGET stdlib PROPERTY CXX_STANDARD 17)

target_include_directories(
        stdlib PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/src>
)

# Need to put this in a separate variable and call separate_arguments() on it, to handle multiple warning flags in the
# CXXFLAGS.
set(EXTRA_CXXFLAGS $ENV{CXXFLAGS})
separate_arguments(EXTRA_CXXFLAGS)

target_compile_options(
        stdlib PRIVATE -Wall -Wextra -Werror -ggdb -fno-delete-null-pointer-checks

        # Needed so that our top-level Makefile can add extra flags for particular platforms if necessary.
        ${EXTRA_CXXFLAGS}
)

include(GNUInstallDirs)

install(
        TARGETS stdlib
        LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}"
        ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
        RUNTIME DESTINATION "${CMAKE_INSTALL_BINDIR}"
        INCLUDES DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}"
)

install(
        FILES ${headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}"
)

install(
        FILES ${collections_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/collections"
)

install(
        FILES ${exceptions_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/exceptions"
)

install(
        FILES ${io_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/io"
)

install(
        FILES ${libtommath_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/libtommath"
)

install(
        FILES ${text_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/text"
)

install(
        FILES ${tsl_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/tsl"
)

# The testing code is not yet ready for macOS, since it uses a different linker which doesn't support --wrap.
if (NOT ${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    set(TEST_SRC
            test/ascii_string.cc
            test/int_array_tests.cc
            test/perlang_char.cc
            test/perlang_string.cc
            test/print.cc
            test/string_array_tests.cc
            test/string_builder.cc
            test/utf8_string.cc
            test/utf16_string.cc
    )

    add_executable(
            tests
            ${TEST_SRC}
    )

    target_include_directories(
            tests
            PUBLIC $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/src>
    )

    target_link_libraries(tests PRIVATE stdlib)
    target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)

    # We enable wrapping for fwrite(), to be able to capture its output in tests.
    target_link_options(tests PRIVATE -Wl,--wrap=fwrite)
endif()
