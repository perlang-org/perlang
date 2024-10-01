.PHONY: all clean

SHELL := /bin/bash -e -o pipefail

CPP_SOURCES = perlang_cli_preprocessed.cc

GIT_COMMIT := $(shell git rev-parse HEAD)

# The input to these sed operations is something like `dev/0.1.0-224-g09f4704`. The output is expected to produce a
# SemVer-compliant version number. For snapshots, it will be something like 0.1.0-dev.224
GIT_DESCRIBE_VERSION := $(shell git describe --tags | sed s%^dev/%% | sed s/-g.*$$// | sed -E "s/-([0-9]*)$$/-dev.\\1/" | sed s/^v//)

all: $(CPP_SOURCES)

clean:
# perlang_cli.cc is also removed so we can force it to be rebuilt (requiring a suitable Perlang snapshot version to be
# available)
	rm -f $(CPP_SOURCES) perlang_cli.cc

perlang_cli_preprocessed.cc: perlang_cli.cc
	sed \
		-e 's/##GIT_COMMIT##/$(GIT_COMMIT)/g' \
		-e 's/##GIT_DESCRIBE_VERSION##/$(GIT_DESCRIBE_VERSION)/g' \
		$< > $@

# Note: this deliberately uses the Perlang installed using install-latest-snapshot, to avoid depending on a Perlang
# toolchain built in this repo (which depends on perlang_cli.so, built using these rules). The latest snapshot is
# expected to work; any other versions are not guaranteed to work.
perlang_cli.cc: native_main.per
	~/.perlang/nightly/bin/perlang -c $^ -o $(patsubst %.cc,%.o,$(@)) --idempotent
