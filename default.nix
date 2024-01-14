{
  lib,
  buildDotnetModule,
  dotnetCorePackages,
  makeWrapper,
  ffmpeg,
}:
buildDotnetModule rec {
  pname = "m3u8-dl";
  version = "0.1";

  src = ./.;

  projectFile = "src/m3u8-dl.csproj";
  testProjectFile = "test/m3u8-dl.Test.csproj";
  # To update nuget deps, run `$(nix-build -A fetch-deps ./build.nix --no-out-link)`
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_8_0;
  dotnet-runtime = dotnetCorePackages.runtime_8_0;

  postFixup = ''
    wrapProgram $out/bin/m3u8-dl --prefix LD_LIBRARY_PATH ":" "${lib.makeLibraryPath [ffmpeg]}"
  '';

  executables = ["m3u8-dl"];
}
