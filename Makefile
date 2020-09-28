.PHONY: all auto-generated clean docs docs-serve install run

# Enable fail-fast in case of errors
SHELL=/bin/bash -e -o pipefail

all: auto-generated
	dotnet build

auto-generated:
	./scripts/update_common_constants.rb `pwd`

clean:
	dotnet clean
	rm -f Perlang.Common/CommonConstants.Generated.cs

docs-clean:
	rm -rf _site

docs: docfx/docfx.exe
	./docfx/docfx.exe docs/docfx.json

docs-serve:
	live-server _site

docfx/docfx.exe:
	wget -qO- https://github.com/dotnet/docfx/releases/download/v2.51/docfx.zip | busybox unzip - -d docfx
	chmod +x docfx/docfx.exe

install: all
	./scripts/local_install_linux.sh

run:
	cd Perlang.ConsoleApp && dotnet run
