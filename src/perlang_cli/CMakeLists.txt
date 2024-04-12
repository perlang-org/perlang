cmake_minimum_required(VERSION 3.5)
project(perlang_cli VERSION 0.1.0)

set(headers
        #src/ascii_ string.h
)

add_library(
        perlang_cli SHARED
        src/native_main.cc
)

# Ugly hack: libstdlib is not a "child project" of this project, so we add its output manually like this. In the future,
# stdlib could potentially be a static .so library instead that just exists in the same directory as the CLI.
#set(CMAKE_SHARED_LINKER_FLAGS ${CMAKE_SHARED_LINKER_FLAGS} "../../../lib/stdlib/lib/libstdlib.a")
#  -Wl,--whole-archive

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
        perlang_cli PRIVATE -Wall -Wextra -Werror
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
