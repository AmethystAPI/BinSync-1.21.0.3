#!/bin/sh
echo -ne '\033c\033]0;Project Squad\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/Project Squad.x86_64" "$@"
