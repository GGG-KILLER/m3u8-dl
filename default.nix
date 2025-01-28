{
  buildDotnetModule,
  dotnetCorePackages,
  ffmpeg,
}:
buildDotnetModule {
  pname = "m3u8-dl";
  version = "0.1";

  src = ./.;

  projectFile = "src/m3u8-dl.csproj";
  testProjectFile = "test/m3u8-dl.Test.csproj";
  # To update nuget deps, run `$(nix-build -A fetch-deps ./build.nix --no-out-link)`
  nugetDeps = ./deps.json;

  runtimeDeps = [ ffmpeg ];

  dotnet-sdk = dotnetCorePackages.sdk_8_0;
  dotnet-runtime = dotnetCorePackages.runtime_8_0;

  executables = [ "m3u8-dl" ];
  meta.mainProgram = "m3u8-dl";
}
