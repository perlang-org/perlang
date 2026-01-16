#!/bin/bash

#
# Builds and installs a local version of the Perlang toolchain, in
# ~/.perlang/nightly/bin
#

set -e

mkdir -p $HOME/.perlang/nightly/bin

dotnet publish src/Perlang.ConsoleApp/Perlang.ConsoleApp.csproj -c Release -r linux-x64 --self-contained true /p:PublishReadyToRun=true /p:SolutionDir=$(pwd)/
cp -r src/Perlang.ConsoleApp/bin/Release/net10.0/linux-x64/publish/* $HOME/.perlang/nightly/bin

# Copy the precompiled stdlib binaries as well, so that experimental compiled mode can find them
cp -r lib/ $HOME/.perlang/nightly/bin

# perlang_cli.so is also required by the Perlang C# binary nowadays
cp lib/perlang_cli/lib/perlang_cli.so $HOME/.perlang/nightly/bin
