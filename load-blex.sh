#!/bin/sh
set -e
lines=$(curl -s "https://scrabble.hrejsi.cz/pravidla/blex.htm" | iconv -f WINDOWS-1250 -t UTF8 | grep -Po '.+(?=<hr>)' | sed 's/, /\n/g' | tee blex.txt | wc -l)
echo Loaded $lines words
