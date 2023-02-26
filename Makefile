# src/Perlang.Common/CommonConstants.Generated.cs is not phony, but we always want
# to force it to be regenerated.
.PHONY: \
	all auto-generated clean darkerfx-push docs docs-serve docs-test-examples \
	docs-validate-api-docs install install-latest-snapshot publish-release \
	release run test src/Perlang.Common/CommonConstants.Generated.cs

RELEASE_PERLANG=src/Perlang.ConsoleApp/bin/Release/net7.0/linux-x64/publish/perlang

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

# See #344 for the background of why this was added.
docs-validate-api-docs:
	@api_files=$$(find _site/api | wc -l); \
	if [ $$api_files -lt 5 ]; then \
		echo -e "\e[31;1mERROR:\e[0m _site/api contains an unxpectedly low number of files ($$api_files). Is DocFX API doc generation broken?"; \
		exit 1; \
	fi

docfx/docfx.exe:
	wget -qO- https://github.com/dotnet/docfx/releases/download/v2.59.4/docfx.zip | busybox unzip - -d docfx
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
	src/Perlang.ConsoleApp/bin/Debug/net7.0/perlang

test:
	dotnet test --configuration Release

#
# Steps for publishing a new release:
#
# 1. Ensure release-notes/v($NEXT_RELEASE_VERSION).md is polished and contains
#   today's date. Remember that this file will be automatically used in the
#   GitHub Release created in the next step.
#
# 2. `make publish-release` - this creates a release in GitHub automatically.
#
# 3. Bump NEXT_RELEASE_VERSION below.
#
# 4. `make prepare-new-dev-version`
#

# Note: run `make prepare-new-dev-version` when updating this, to ensure that
# snapshot releases have the correct -V info.
NEXT_RELEASE_VERSION=0.4.0
NEXT_RELEASE_TAG=v$(NEXT_RELEASE_VERSION)

prepare-new-dev-version:
	touch release-notes/$(NEXT_RELEASE_TAG).md
	git add Makefile release-notes/$(NEXT_RELEASE_TAG).md
	git commit -m '(Makefile) Start working on $(NEXT_RELEASE_TAG)'
	git push
	git tag dev/$(NEXT_RELEASE_VERSION) && git push origin dev/$(NEXT_RELEASE_VERSION)

publish-release:
	echo $(NEXT_RELEASE_TAG) > .metadata/latest-release.txt
	git release $(NEXT_RELEASE_TAG)
