#!/bin/bash

# Originally inspired by
# https://gitlab.com/gitlab-org/fleeting/plugins/aws/-/blob/7328e430d0ce9896770417d8cd7317f66342cbc0/ci/release.sh
# (MIT licensed, Copyright (c) 2015-present GitLab B.V.)

# This script assumes it is running within a
# registry.gitlab.com/gitlab-org/release-cli:latest image, or at least that
# `release-cli` is installed and in $PATH. Also note that this is very much a
# bash script and does not run under plain sh.

set -eo pipefail

args=( create --name "Release ${CI_COMMIT_TAG}" --tag-name "${CI_COMMIT_TAG}" --description "release-notes/${CI_COMMIT_TAG}.md" )
while read -r FILE
do
    # TODO: change "filepath" to "direct_asset_path" when https://gitlab.com/gitlab-org/release-cli/-/issues/165 is fixed.
    args+=( --assets-link "{\"name\":\"${FILE}\",\"url\":\"${PACKAGE_REGISTRY_URL}/${CI_COMMIT_TAG}/${FILE}\", \"filepath\":\"/${FILE}\"}" )
done < manifest.txt

release-cli "${args[@]}"
