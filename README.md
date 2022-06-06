# GDLogger

**GDLogger** is a simple logger intended to be used in Godot applications.

Available in both GDScript and C#, it supports writing messages to a log file, displaying them in a neat and human-readable format.

Both versions of the logger provide important features such as periodically saving logs to disk and automatically closing the log file on application exit.  
Additionally, the C# version automatically logs unhandled `Exception`s.

# Installation

- ## C#
  **GDLogger** is available as a [NuGet package](https://www.nuget.org/packages/GDLogger/).  
  Simply include the following lines in a Godot project's `.csproj` file (either by editing the file manually or letting an IDE install the package):
  ```xml
  <ItemGroup>
    <PackageReference Include="GDLogger" Version="1.0.0"/>
  </ItemGroup>
  ```
  Due to [a bug](https://github.com/godotengine/godot/issues/42271) in Godot, the following lines will also need to be included in the `.csproj` file to properly compile along with NuGet packages:
  ```xml
  <PropertyGroup>
      <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
  </PropertyGroup>
  ```
- ## GDScript
  As there is no dedicated package management system for GDScript, simply download the `Log.gd` file and include it in a Godot project.  
  For best results, it is recommended to set it up as an autoload singleton, though it can still be instanced and used as a normal script.

# Interoperability

For a GDScript-only project, use the `Log.gd` file.

For a Mono (C#) project, use the `Log` static class.  
Note that this uses custom types that cannot be marshalled by the Godot engine. As such, if the logger's methods need to be called from both GDScript and C#, the GDScript version (`Log.gd`) should be used instead, as it inherits from `Node`.

Do not attempt to use both the C# and the GDScript versions of the logger at the same time, as it will lead to errors if trying to write to the same log file.