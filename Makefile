# src/Perlang.Common/CommonConstants.Generated.cs is not phony, but we always want
# to force it to be regenerated.
.PHONY: \
	all clean darkerfx-push docs docs-serve docs-test-examples valgrind-docs-test-examples \
	docs-validate-api-docs install install-latest-snapshot perlang_cli_clean perlang_cli_install_debug \
	perlang_cli_install_release perlang_cli_install_integration_test_debug perlang_cli_install_integration_test_release \
	publish-release release run run-hello-world-example valgrind-perlang-run-hello-world-example valgrind-hello-world-example \
	test valgrind-test-stdlib src/Perlang.Common/CommonConstants.Generated.cs

.PRECIOUS: %.cc

# This gets overridden in CI when building for another architecture
ARCH=linux-x64

DEBUG_PERLANG_DIRECTORY=src/Perlang.ConsoleApp/bin/Debug/net8.0
DEBUG_PERLANG=$(DEBUG_PERLANG_DIRECTORY)/perlang

RELEASE_PERLANG_DIRECTORY=src/Perlang.ConsoleApp/bin/Release/net8.0/$(ARCH)/publish
RELEASE_PERLANG=$(RELEASE_PERLANG_DIRECTORY)/perlang

# --undef-value-errors are caused by the .NET runtime, so we ignore them for now to avoid noise. Must also use
# --suppressions because .NET runtime has some existing Valgrind issues: https://github.com/dotnet/runtime/issues/52872
VALGRIND=DOTNET_GCHeapHardLimit=C800000 valgrind --undef-value-errors=no --error-exitcode=1 --show-leak-kinds=all --leak-check=full --suppressions=scripts/valgrind-suppressions.txt

UNAME := $(shell uname -s)

# bash is located in different directories on different systems. Also, enable fail-fast in case of errors.
ifeq ($(UNAME), Linux)
    SHELL := /bin/bash -e -o pipefail
else ifeq ($(UNAME), FreeBSD)
    SHELL := /usr/local/bin/bash -e -o pipefail

    # Workaround for Clang 18 warnings triggered by deprecated declarations in the libc++ LLVM C++ library.
    export CXXFLAGS=-Wno-deprecated-declarations
else ifeq ($(UNAME), NetBSD)
    SHELL := /usr/pkg/bin/bash -e -o pipefail
else ifeq ($(UNAME), OpenBSD)
    SHELL := /usr/local/bin/bash -e -o pipefail
else
    $(error Unsupported operating system $(UNAME) encountered)
endif

CLANGPP=clang++-14

all: auto-generated auto-generated-bindings perlang_cli
	dotnet build

release:
	dotnet publish src/Perlang.ConsoleApp/Perlang.ConsoleApp.csproj -c Release -r $(ARCH) --self-contained true /p:PublishReadyToRun=true /p:SolutionDir=$$(pwd)/

.PHONY: auto-generated
auto-generated: src/Perlang.Common/CommonConstants.Generated.cs

.PHONY: auto-generated-bindings
auto-generated-bindings: perlang_cli
	dotnet build src/Perlang.GenerateCppSharpBindings/Perlang.GenerateCppSharpBindings.csproj
	dotnet run --project src/Perlang.GenerateCppSharpBindings/Perlang.GenerateCppSharpBindings.csproj

src/Perlang.Common/CommonConstants.Generated.cs: scripts/update_common_constants.rb
	scripts/update_common_constants.rb

clean:
	dotnet clean
	rm -f src/Perlang.Common/CommonConstants.Generated.cs
	rm -rf src/Perlang.ConsoleApp/bin/Debug src/Perlang.ConsoleApp/bin/Release
	rm -rf lib/
	rm -rf src/stdlib/out src/stdlib/cmake-build-debug
	rm -rf src/perlang_cli/out src/perlang_cli/cmake-build-debug

docs-clean:
	rm -rf _site

docs: docfx/docfx.exe docs/templates/darkerfx/styles/main.css
	./docfx/docfx.exe docs/docfx.json

docs/templates/darkerfx/styles/main.css: docs/templates/darkerfx/styles/main.scss
	sass $< $(@)

docs-autobuild:
	while true; do find docs Makefile src -type f | entr -d bash -c 'scripts/time_it make docs' ; done

docs-test-examples: perlang_cli_install_release
	for e in docs/examples/quickstart/*.per ; do echo -e \\n\\e[1m[$$e]\\e[0m ; $(RELEASE_PERLANG) $$e ; done
	for e in docs/examples/the-language/*.per ; do echo -e \\n\\e[1m[$$e]\\e[0m ; $(RELEASE_PERLANG) $$e ; done

valgrind-docs-test-examples: perlang_cli_install_release
	for e in docs/examples/quickstart/*.per ; do echo -e \\n\\e[1m[$$e]\\e[0m ; $(VALGRIND) $(DEBUG_PERLANG) $$e ; done
	for e in docs/examples/the-language/*.per ; do echo -e \\n\\e[1m[$$e]\\e[0m ; $(VALGRIND) $(DEBUG_PERLANG) $$e ; done

docs-serve:
	live-server _site

docfx/docfx.exe:
	wget -qO- https://github.com/dotnet/docfx/releases/download/v2.59.4/docfx.zip | busybox unzip - -d docfx
	chmod +x docfx/docfx.exe

# Pushes local changes to the darkerfx theme to the darkerfx repo. Presumes that
# it is checked out next to the perlang repository.
darkerfx-push:
	rsync -av docs/templates/darkerfx/ ../darkerfx/darkerfx/

install: auto-generated stdlib perlang_cli
	./scripts/local_install_linux.sh

# Downloads and installs the latest snapshot from https://builds.perlang.org
install-latest-snapshot:
	curl -sSL https://perlang.org/install.sh | sh -s -- --force

# Performing an unconditional "perlang_cli_clean" here is sometimes redundant, but without it the .cc files will not
# always be rebuilt which can be confusing during development. The Perlang compilation is idempotent, so this should
# hopefully not cause any unnecessary noise in the generated C++ files.
run: auto-generated perlang_cli_clean perlang_cli perlang_cli_install_debug
	# Cannot use 'dotnet run' at the moment, since it's impossible to pass
	# /p:SolutionDir=$(pwd)/ to it.
	dotnet build
	$(DEBUG_PERLANG) -v

valgrind-perlang-run-hello-world-example: auto-generated perlang_cli perlang_cli_install_debug
	dotnet build
	$(VALGRIND) $(DEBUG_PERLANG) docs/examples/quickstart/hello_world.per

valgrind-hello-world-example: auto-generated perlang_cli perlang_cli_install_debug
	dotnet build
# The PERLANG_RUN_WITH_VALGRIND environment variable is not honoured by the Perlang CLI, only EvalHelper at the moment
	$(DEBUG_PERLANG) docs/examples/quickstart/hello_world.per
	$(VALGRIND) docs/examples/quickstart/hello_world

run-hello-world-example: auto-generated perlang_cli_clean perlang_cli perlang_cli_install_debug
	dotnet build
	$(DEBUG_PERLANG) docs/examples/quickstart/hello_world.per

# Creating a subdirectory is important in these targets, since cmake will otherwise clutter the stdlib directory with
# its auto-generated build scripts
stdlib:
	cd src/stdlib && mkdir -p out && cd out && cmake -DCMAKE_INSTALL_PREFIX:PATH=../../../lib/stdlib -G "Unix Makefiles" .. && make stdlib install

# perlang_cli depends on stdlib, so it must be built before this can be successfully built
.PHONY: perlang_cli
perlang_cli: stdlib
# Precompile the Perlang files to C++ if needed, so that they can be picked up by the CMake build
	cd src/perlang_cli/src && $(MAKE)

	cd src/perlang_cli && mkdir -p out && cd out && cmake -DCMAKE_INSTALL_PREFIX:PATH=../../../lib/perlang_cli -G "Unix Makefiles" .. && make perlang_cli install

# Note that this removes all auto-generated files, including the C++ files which
# are normally committed to git. This makes it easy to regenerate them from the
# Perlang sources.
perlang_cli_clean:
	cd src/perlang_cli/src && $(MAKE) clean
	cd src/perlang_cli && rm -rf out

perlang_cli_install_debug: perlang_cli
	mkdir -p $(DEBUG_PERLANG_DIRECTORY)
	cp lib/perlang_cli/lib/perlang_cli.so $(DEBUG_PERLANG_DIRECTORY)

perlang_cli_install_release: perlang_cli
	mkdir -p $(RELEASE_PERLANG_DIRECTORY)
	cp lib/perlang_cli/lib/perlang_cli.so $(RELEASE_PERLANG_DIRECTORY)

perlang_cli_install_integration_test_debug: perlang_cli
	mkdir -p src/Perlang.Tests/bin/Debug/net8.0
	mkdir -p src/Perlang.Tests.Integration/bin/Debug/net8.0
	cp lib/perlang_cli/lib/perlang_cli.so src/Perlang.Tests/bin/Debug/net8.0
	cp lib/perlang_cli/lib/perlang_cli.so src/Perlang.Tests.Integration/bin/Debug/net8.0

perlang_cli_install_integration_test_release: perlang_cli
	mkdir -p src/Perlang.Tests/bin/Release/net8.0
	mkdir -p src/Perlang.Tests.Integration/bin/Release/net8.0
	cp lib/perlang_cli/lib/perlang_cli.so src/Perlang.Tests/bin/Release/net8.0
	cp lib/perlang_cli/lib/perlang_cli.so src/Perlang.Tests.Integration/bin/Release/net8.0

test:
	dotnet test --configuration Release

.PHONY: test-stdlib
test-stdlib: stdlib
# We need --colour-mode since colour support is not auto-detected in CI.
	src/stdlib/out/tests --reporter console::out=-::colour-mode=ansi $(EXTRA_CATCH_REPORTER)

valgrind-test-stdlib: stdlib
	$(VALGRIND) src/stdlib/out/tests

.PHONY: test-perlang-cli
test-perlang-cli: perlang_cli
# We need --colour-mode since colour support is not auto-detected in CI.
	src/perlang_cli/out/tests --reporter console::out=-::colour-mode=ansi $(EXTRA_CATCH_REPORTER)

#
# Steps for publishing a new release:
#
# 1. Ensure release-notes/v($NEXT_RELEASE_VERSION).md is polished and contains
#   today's date. Remember that this file will be automatically used in the
#   GitLab Release created in the next step.
#
# 2. `make publish-release` - this creates a release in GitLab automatically.
#
# 3. Bump NEXT_RELEASE_VERSION below.
#
# 4. `make prepare-new-dev-version`
#

# Note: run `make prepare-new-dev-version` when updating this, to ensure that
# snapshot releases have the correct -V info.
NEXT_RELEASE_VERSION=0.7.0
NEXT_RELEASE_TAG=v$(NEXT_RELEASE_VERSION)

prepare-new-dev-version:
	cp release-notes/template.md release-notes/$(NEXT_RELEASE_TAG).md
	git add Makefile release-notes/$(NEXT_RELEASE_TAG).md
	git commit -m '(Makefile) Start working on $(NEXT_RELEASE_TAG)'
	git push
	git tag dev/$(NEXT_RELEASE_VERSION) && git push origin dev/$(NEXT_RELEASE_VERSION)

publish-release:
	echo $(NEXT_RELEASE_TAG) > .metadata/latest-release.txt
	git release $(NEXT_RELEASE_TAG)

# Targets used to support CI
.PHONY: upload-release
upload-release:
	scripts/ci/upload-release.sh

.PHONY: create-gitlab-release
create-gitlab-release:
	scripts/ci/create-gitlab-release.sh
