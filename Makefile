# src/Perlang.Common/CommonConstants.Generated.cs is not phony, but we always want
# to force it to be regenerated.
.PHONY: \
	all auto-generated clean darkerfx-push docs docs-serve docs-test-examples \
	install install-latest-snapshot release run src/Perlang.Common/CommonConstants.Generated.cs

RELEASE_PERLANG=src/Perlang.ConsoleApp/bin/Release/net6.0/linux-x64/publish/perlang

# Enable fail-fast in case of errors
SHELL=/bin/bash -e -o pipefail

all: auto-generated
	dotnet build

release:
	dotnet publish src/Perlang.ConsoleApp/Perlang.ConsoleApp.csproj -c Release -r linux-x64 --self-contained true /p:PublishReadyToRun=true /p:SolutionDir=$$(pwd)/

auto-generated: src/Perlang.Common/CommonConstants.Generated.cs

src/Perlang.Common/CommonConstants.Generated.cs: scripts/update_common_constants.rb
	scripts/update_common_constants.rb

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
	for e in docs/examples/quickstart/*.per ; do echo -e \\n\\e[1m[$$e]\\e[0m ; $(RELEASE_PERLANG) $$e ; done
	for e in docs/examples/the-language/*.per ; do echo -e \\n\\e[1m[$$e]\\e[0m ; $(RELEASE_PERLANG) $$e ; done

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

# Downloads and installs the latest snapshot from https://builds.perlang.org
install-latest-snapshot:
	curl -sSL https://perlang.org/install.sh | sh -s -- --force

run: auto-generated
	# Cannot use 'dotnet run' at the moment, since it's impossible to pass
	# /p:SolutionDir=$(pwd)/ to it.
	dotnet build
	src/Perlang.ConsoleApp/bin/Debug/net6.0/perlang

test:
	dotnet test --configuration Release
