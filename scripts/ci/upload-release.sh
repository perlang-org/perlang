#!/bin/bash

# Originally inspired by
# https://gitlab.com/gitlab-org/fleeting/plugins/aws/-/blob/7328e430d0ce9896770417d8cd7317f66342cbc0/ci/upload-release.sh
# (MIT licensed, Copyright (c) 2015-present GitLab B.V.)

set -e

OUT_PATH="${OUT_PATH:-out}"

for FILE in "${OUT_PATH}"/*.tar.gz
do
    URL="${PACKAGE_REGISTRY_URL}/${CI_COMMIT_TAG}/$(basename "${FILE}")"
    echo "Uploading ${FILE} to ${URL}"
    curl --header "JOB-TOKEN: ${CI_JOB_TOKEN}" --upload-file "${FILE}" "${URL}"
done

# List the filenames uploaded so we can use them in the release job
ls "${OUT_PATH}/" > manifest.txt
