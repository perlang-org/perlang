.PHONY: all auto_generated clean install run

all: auto_generated
	dotnet build

auto_generated:
	./scripts/update_common_constants.rb `pwd`

clean:
	dotnet clean
	rm -f Perlang.Common/CommonConstants.Generated.cs

install: all
	./scripts/local_install_linux.sh

run:
	cd Perlang.ConsoleApp && dotnet run
