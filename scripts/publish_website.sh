#!/bin/bash

#
# Builds static website using DocFx and publish it to the production
# server.
#

set -e

[ -z "$PERLANG_PUBLISH_TARGET" ] && (echo \$PERLANG_PUBLISH_TARGET must be set; exit 1)

GIT_LAST_COMMIT=$(git rev-parse --short HEAD)

make docs-clean docs
echo "Built from $GIT_LAST_COMMIT" > _site/.gitinfo

rsync -av --delete _site/ $PERLANG_PUBLISH_TARGET
