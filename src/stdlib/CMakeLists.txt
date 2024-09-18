cmake_minimum_required(VERSION 3.5)
project(stdlib VERSION 0.1.0)

# This is needed to get CMake to use -fPIC. Without this, linking the Perlang stdlib into a .so shared library will fail
# with relocation errors.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

set(headers
        src/ascii_string.h
        src/bigint.h
        src/int_array.h
        src/internal/string_utils.h
        src/perlang_stdlib.h
        src/perlang_string.h
        src/string_array.h
        src/utf8_string.h
        src/double-conversion/utils.h
)

set(libtommath_headers
        src/libtommath/tommath.h
)

add_library(
        stdlib
        src/ascii_string.cc
        src/Base64.cc
        src/bigint.cc
        src/bigint_mod.cc
        src/bigint_pow.cc
        src/int_array.cc
        src/print.cc
        src/string_array.cc
        src/utf8_string.cc

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
)

# Enable C++ 17 support. This is required on e.g. AppleClang 13
set_property(TARGET stdlib PROPERTY CXX_STANDARD 17)

target_include_directories(
        stdlib PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
)

target_compile_options(
        stdlib PRIVATE -Wall -Wextra -Werror

        # TODO: Remove this. We have some current issues with %lld (long long int vs int64_t) that needs to be
        # TODO: resolved for this. The problem is IIRC that Linux and macOS treated this differently.
        -Wno-error=format
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
        FILES ${libtommath_headers}
        DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}/libtommath"
)

# The testing code is not yet ready for macOS, since it uses a different linker which doesn't support --wrap. We'll
# live with only building/running the tests on other platforms for now.
if (NOT ${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    enable_testing()
    include(CTest)

    set(CCTEST_SRC
            test/print.cc
            test/double-conversion/cctest.cc
    )

    add_executable(
            cctest
            ${CCTEST_SRC}
    )

    target_include_directories(
            cctest
            PUBLIC $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
    )

    target_link_libraries(cctest stdlib)

    # We enable wrapping for fwrite(), to be able to capture its output in tests. The reason why we don't do this on
    # macOS is because it does not use GNU ld, so the --wrap linker option isn't available.
    target_link_options(cctest PRIVATE -Wl,--wrap=fwrite)

    add_test(NAME test_fast_dtoa
            WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
            COMMAND $<TARGET_FILE:cctest> test-fast-dtoa)

    add_test(NAME test_print
            WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
            COMMAND $<TARGET_FILE:cctest> print)
endif()
