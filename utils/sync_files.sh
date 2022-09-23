#!/bin/sh

SOURCE_DIR=${1:-""}
if [ ! -d "$SOURCE_DIR" ]; then
  echo "Must specify an source directory"
  exit 1
fi

DEST_DIR=${2:-""}
if [ ! -d "$DEST_DIR" ]; then
  echo "Must specify an destination directory"
  exit 1
fi

rsync -aP --exclude="ar" --exclude="cs" --exclude="de" --exclude="es" --exclude="fr" --exclude="he" \
  --exclude="hu-HU" --exclude="it" --exclude="ja" --exclude="ko" --exclude="nl" --exclude="pl" \
  --exclude="pt" --exclude="pt-BR" --exclude="ru" --exclude="se" --exclude="tr" --exclude="uk-UA" \
  --exclude="zh-CN" --exclude="zh-Hans" --exclude="zh-Hant" --exclude="zh-TW" --exclude="Logs" \
  --exclude="Profiles" --exclude="*.xml" --exclude="custom_exe_name.txt" \
  --exclude="task.bat" --exclude="*.un~" "${SOURCE_DIR}" "${DEST_DIR}"
