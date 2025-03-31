# publish commands

These are the commands for building/publishing butters. for building you must be in butters' root folder.

## osX

```bash
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/V2.0.0/osx-x64
```

## windows

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/V2.0.0/win-x64
```

## Linux

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/V2.0.0/linux-x64
```
