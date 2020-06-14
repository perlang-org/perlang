#!/bin/bash

#
# Builds and installs a local version of the Perlang toolchain, in
# ~/.perlang/nightly/bin
#

set -e

mkdir -p $HOME/.perlang/nightly/bin

pushd Perlang.ConsoleApp
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishReadyToRun=true
cp -r bin/Release/netcoreapp3.1/linux-x64/publish/* $HOME/.perlang/nightly/bin
popd
