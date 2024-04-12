.PHONE: all clean

.PRECIOUS: %.cc

CPP_SOURCES = $(patsubst %.per,%.cc,$(wildcard *.per))

all: $(CPP_SOURCES)

clean:
	rm $(CPP_SOURCES)

# Note: this deliberately uses the Perlang installed using install-latest-snapshot, to avoid depending on a Perlang
# toolchain built in this repo (which depends on perlang_cli.so, built using these rules). The latest snapshot is
# expected to work; any other versions are not guaranteed to work.
%.cc: %.per
	~/.perlang/nightly/bin/perlang -c  $< --idempotent
