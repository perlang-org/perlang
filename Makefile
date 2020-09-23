.PHONY: all auto-generated clean docs docs-serve install run

all: auto-generated
	dotnet build

auto-generated:
	./scripts/update_common_constants.rb `pwd`

clean:
	dotnet clean
	rm -f Perlang.Common/CommonConstants.Generated.cs

docs-clean:
	rm -rf _site

docs:
	./docfx/docfx.exe docs/docfx.json

docs-serve:
	live-server _site

install: all
	./scripts/local_install_linux.sh

run:
	cd Perlang.ConsoleApp && dotnet run
