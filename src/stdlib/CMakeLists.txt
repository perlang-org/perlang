cmake_minimum_required(VERSION 3.0)
project(stdlib VERSION 0.1.0)

set(headers
        src/bigint.hpp
        src/stdlib.hpp
        src/double-conversion/utils.h
)

add_library(
        stdlib
        src/bigint.cpp
        src/Base64.cpp
        src/print.cpp
)

# Enable C++ 17 support. This is required on e.g. AppleClang 13
set_property(TARGET stdlib PROPERTY CXX_STANDARD 17)

target_include_directories(
        stdlib PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
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
