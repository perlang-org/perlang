.PHONY: all clean

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

CPP_SOURCES = perlang_cli_preprocessed.cc

GIT_COMMIT_ID := $(shell git rev-parse --short HEAD)

# The input to these sed operations is something like `dev/0.1.0-224-g09f4704`. The output is expected to produce a
# SemVer-compliant version number. For snapshots, it will be something like 0.1.0-dev.224+9ccaf15
GIT_DESCRIBE_VERSION := $(shell git describe --tags | sed s%^dev/%% | sed s/-g.*$$// | sed -E "s/-([0-9]*)$$/-dev.\\1/" | sed s/^v//)

all: $(CPP_SOURCES)

clean:
# perlang_cli.cc is also removed so we can force it to be rebuilt (requiring a suitable Perlang snapshot version to be
# available)
	rm -f $(CPP_SOURCES) perlang_cli.cc

perlang_cli_preprocessed.cc: perlang_cli.cc
	sed \
		-e 's/##GIT_COMMIT_ID##/$(GIT_COMMIT_ID)/g' \
		-e 's/##GIT_DESCRIBE_VERSION##/$(GIT_DESCRIBE_VERSION)/g' \
		-e 's/##BUILD_TIMESTAMP##/$(shell date -u +'%Y-%m-%dT%H:%M:%SZ')/g' \
		-e 's/##BUILD_USER##/$(shell whoami)/g' \
		-e 's/##BUILD_HOST##/$(shell hostname)/g' \
		perlang_cli.cc > $@

# Note: this deliberately uses the Perlang installed using install-latest-snapshot, to avoid depending on a Perlang
# toolchain built in this repo (which depends on perlang_cli.so, built using these rules). Without this, it gets very
# hard to be able to build perlang_cli unless you already have a working perlang_cli.so in place (chicken-and-egg), and
# also if you make a mistake that breaks the compiler, you would also be unable to fix it.
#
# The latest snapshot is expected to work; any other versions are not guaranteed to work.
perlang_cli.cc: native_main.per perlang_scanner.per token.per token_type.per
	~/.perlang/nightly/bin/perlang -c $^ -o $(patsubst %.cc,%.o,$(@)) --idempotent
