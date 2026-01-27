#!/bin/bash

OUTPUT=PACKAGES.md
PATH=$PATH:~/.dotnet/tools
dotnet tool install --no-cache --global nuget-license

cat <<EOF >$OUTPUT
# Licenses of used Packages

## Source

Generated with https://github.com/sensslen/nuget-license

\`\`\`cmd
dotnet tool install --no-cache --global nuget-license
nuget-license --include-transitive --output markdown --input "Mbemc.DataExchange\Mbemc.DataExchange.csproj"
\`\`\`

## Table

EOF

nuget-license --include-transitive --input "Mbemc.DataExchange\Mbemc.DataExchange.csproj" >>$OUTPUT
