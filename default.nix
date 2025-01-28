{
  buildDotnetModule,
  dotnetCorePackages,
  ffmpeg_7,
}:
buildDotnetModule {
  pname = "m3u8-dl";
  version = "0.1";

  src = ./.;

  projectFile = "src/m3u8-dl.csproj";
  # To update nuget deps, run `$(nix-build -A fetch-deps ./build.nix --no-out-link)`
  nugetDeps = ./deps.json;

  runtimeDeps = [ ffmpeg_7 ];

  dotnet-sdk = dotnetCorePackages.sdk_9_0;
  dotnet-runtime = dotnetCorePackages.runtime_9_0;

  executables = [ "m3u8-dl" ];
  meta.mainProgram = "m3u8-dl";
}
