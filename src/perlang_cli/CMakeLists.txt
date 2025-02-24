cmake_minimum_required(VERSION 3.5)
project(perlang_cli VERSION 0.1.0)

Include(FetchContent)

FetchContent_Declare(
        Catch2
        GIT_REPOSITORY https://github.com/catchorg/Catch2.git
        GIT_TAG        v3.7.1
)

FetchContent_MakeAvailable(Catch2)

add_library(
        perlang_cli SHARED
        src/mutable_string_token_type_dictionary.cc
        src/perlang_cli_preprocessed.cc
        src/stdlib_wrappers.cc
        src/string_token_type_dictionary.cc
)

# Enable C++ 17 support. This is required on e.g. AppleClang 13
set_property(TARGET perlang_cli PROPERTY CXX_STANDARD 17)

# Remove the lib prefix from the output file (libperlang_cli.so -> perlang_cli.so)
set_property(TARGET perlang_cli PROPERTY PREFIX "")

target_include_directories(
        perlang_cli
        PUBLIC $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
        PUBLIC $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>/../../lib/stdlib/include/
)

target_compile_options(
        perlang_cli PRIVATE -Wall -Wextra -Werror -ggdb
)

target_link_directories(
        perlang_cli
        PRIVATE "../../lib/stdlib/lib"
)

target_link_libraries(
        perlang_cli
        "stdlib"
)

include(GNUInstallDirs)

install(
        TARGETS perlang_cli
        LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}"
        ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
        RUNTIME DESTINATION "${CMAKE_INSTALL_BINDIR}"
        INCLUDES DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}"
)

set(TEST_SRC
        test/mutable_string_token_type_dictionary.cc
)

add_executable(
        tests
        ${TEST_SRC}
)

target_include_directories(
        tests
        PUBLIC $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/src>
)

target_link_directories(
        tests
        PRIVATE "../../lib/stdlib/lib"
)

target_link_libraries(tests PRIVATE perlang_cli)
target_link_libraries(tests PRIVATE Catch2::Catch2WithMain)
