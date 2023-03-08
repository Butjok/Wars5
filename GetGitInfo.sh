#!/usr/bin/env bash

# https://gist.github.com/textarcana/1306223
# Use this one-liner to produce a JSON literal from the Git log:

git log \
    --pretty=format:'{%n  "commit": "%h",%n  "author": "%aN <%aE>",%n  "date": "%aI",%n  "message": "%f"%n},' \
    $@ | \
    perl -pe 'BEGIN{print "["}; END{print "]\n"}' | \
    perl -pe 's/},]/}]/' > Assets/Resources/GitInfo.json