@echo off

dotnet tool install --no-cache --global nuget-license

(
  echo # Licenses of used Packages
  echo.
  echo ## Source
  echo.
  echo Generated with https://github.com/sensslen/nuget-license
  echo.
  echo ```cmd
  echo dotnet tool install --no-cache --global nuget-license
  echo nuget-license --include-transitive --output markdown --input "Mbemc.DataExchange\Mbemc.DataExchange.csproj"
  echo ```
  echo.
  echo ## Table
  echo.
  nuget-license --include-transitive --output markdown --input "Mbemc.DataExchange\Mbemc.DataExchange.csproj"
) > PACKAGES.md
