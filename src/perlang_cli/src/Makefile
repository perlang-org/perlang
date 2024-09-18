.PHONY: all clean

CPP_SOURCES = perlang_cli.cc

all: $(CPP_SOURCES)

clean:
	rm $(CPP_SOURCES)

# Note: this deliberately uses the Perlang installed using install-latest-snapshot, to avoid depending on a Perlang
# toolchain built in this repo (which depends on perlang_cli.so, built using these rules). The latest snapshot is
# expected to work; any other versions are not guaranteed to work.
#
# Note that the order of the files is important here, since we currently don't perform multiple passes in the Perlang
# compilation.
perlang_cli.cc: version.per native_main.per
	([ -x ~/.perlang/nightly/bin/perlang ] && ~/.perlang/nightly/bin/perlang -c $^ -o $(patsubst %.cc,%.o,$(@)) --idempotent) || :
	([ ! -x ~/.perlang/nightly/bin/perlang ] && echo "NOTE: Perlang snapshot not found in ~/.perlang/nightly/bin/perlang, using prebuilt C++ sources instead (which may be out-of-date).") || :
