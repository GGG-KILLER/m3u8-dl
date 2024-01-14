{
  nixpkgs ? <nixpkgs>,
  pkgs ? import nixpkgs {},
}:
pkgs.callPackage ./default.nix {}
