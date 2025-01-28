#! /usr/bin/nix-shell
#! nix-shell -i bash -p static-web-server
# shellcheck shell=bash

static-web-server \
    --port 8080 \
    --root . \
    --directory-listing true \
    --log-level info
