#!/bin/sh

Help()
{
  echo "sync_files.sh [OPTION] SOURCE_DIR DEST_DIR";
  echo "";
  echo "Options:";
  echo "-h    Print help text";
}

# Handle passed options (only help)
while getopts ":h" option; do
  case $option in
    h) #Display help
      Help;
      exit;;
   esac
done

shift $((OPTIND-1))

if [ $# -eq 0 ]; then
  Help
  exit 0
fi

# Handle checking positional arguments
if [ $# -ne 2 ]; then
  echo "Must provide two positional arguments for directories";
  exit 1
fi

# Check for presence of exclude patterns file for rsync with
# EXCLUDE_PATTERNS_FILE envvar
EXCLUDE_FROM_ARG=""
if [ ! -z "${EXCLUDE_PATTERNS_FILE}" ]; then
  EXCLUDE_FROM_ARG="--exclude-from=${EXCLUDE_PATTERNS_FILE}"
fi

#echo "${EXCLUDE_FROM_ARG}"

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
