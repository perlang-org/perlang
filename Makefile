# src/Perlang.Common/CommonConstants.Generated.cs is not phony, but we always want
# to force it to be regenerated.
.PHONY: \
	all auto-generated clean darkerfx-push docs docs-serve docs-test-examples \
	install release run src/Perlang.Common/CommonConstants.Generated.cs

# Enable fail-fast in case of errors
SHELL=/bin/bash -e -o pipefail

all: auto-generated
	dotnet build

release:
	dotnet build -c Release

auto-generated: src/Perlang.Common/CommonConstants.Generated.cs

src/Perlang.Common/CommonConstants.Generated.cs: ./scripts/update_common_constants.rb
	./scripts/update_common_constants.rb `pwd`

clean:
	dotnet clean
	rm -f src/Perlang.Common/CommonConstants.Generated.cs

docs-clean:
	rm -rf _site

docs: docfx/docfx.exe docs/templates/darkerfx/styles/main.css
	./docfx/docfx.exe docs/docfx.json

docs/templates/darkerfx/styles/main.css: docs/templates/darkerfx/styles/main.scss
	sass $< $(@)

docs-autobuild:
	while true; do find docs Makefile src -type f | entr -d bash -c 'scripts/time_it make docs' ; done

docs-test-examples:
	for e in docs/examples/*.per ; do src/Perlang.ConsoleApp/bin/Release/net5.0/linux-x64/publish/perlang $$e ; done

docs-serve:
	live-server _site

docfx/docfx.exe:
	wget -qO- https://github.com/dotnet/docfx/releases/download/v2.57.2/docfx.zip | busybox unzip - -d docfx
	chmod +x docfx/docfx.exe

# Pushes local changes to the darkerfx theme to the darkerfx repo. Presumes that
# it is checked out next to the perlang repository.
darkerfx-push:
	rsync -av docs/templates/darkerfx/ ../darkerfx/darkerfx/

install: auto-generated
	./scripts/local_install_linux.sh

run: auto-generated
	# Cannot use 'dotnet run' at the moment, since it's impossible to pass
	# /p:SolutionDir=$(pwd)/ to it.
	dotnet build
	src/Perlang.ConsoleApp/bin/Debug/net5.0/perlang
