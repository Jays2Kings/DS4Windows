## Bundle Translations

### Edit Project File Post-Build

Open the DS4WindowsWPF.csproj file. Navigate to the Post-Build section.
Add two `:` characters before the `GOTO END` statement to comment the line out.
Build the program in Visual Studio or directly with MSBuild. The dedicated Lang folder
will be created with the compiled translation assemblies

#### Add new Translations in project file

Edit the `langs` variable in the Post-Build step with the .NET culture code for a
locale.

### Add Lang folder loading

.NET normally expects translation assemblies to be bundled in folders
for each locale directly next to the running executable. Since DS4Windows
uses a Lang subfolder, an extra step must be performed to make sure .NET will
find the compiled assemblies.

A Python 3 (3.10) script is included in the solution `utils` folder (`inject_deps_path.py`).
It accepts the path to the relevant `DS4Windows.deps.json` file as a cmd argument. The script
will add necessary path variable for the project entry to include an extra include path
for the .NET Runtime.
