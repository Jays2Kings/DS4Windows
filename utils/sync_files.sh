#!/bin/sh

# Check for presence of exclude patterns file for rsync with
# EXCLUDE_PATTERNS_FILE envvar
EXCLUDE_FROM_ARG=""
if [ ! -z "${EXCLUDE_PATTERNS_FILE}" ]; then
  EXCLUDE_FROM_ARG="--exclude-from=${EXCLUDE_PATTERNS_FILE}"
fi

echo "${EXCLUDE_FROM_ARG}"

SOURCE_DIR=${1:-""}
if [ ! -d "$SOURCE_DIR" ]; then
  echo "Must specify a source directory"
  exit 1
fi

DEST_DIR=${2:-""}
if [ ! -d "$DEST_DIR" ]; then
  echo "Must specify a destination directory"
  exit 1
fi

rsync -aP "${EXCLUDE_FROM_ARG}" "${SOURCE_DIR}" "${DEST_DIR}"
