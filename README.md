# Large File Splitter

A simple program to break large text files into smaller pieces. 

It handles any sizes and is usually used for files > GB. 

## Usage

Place .exe into the folder in which your larges files rely or pass this folder path directly in the command line (see Example). Then, follow instructions.

## Arguments
**-min** specifies the minimum size (GB) large files must have to be displayed on the suggestion list (default is 2GB)

**-size** is the final size (GB) each cut pieces should have (default is 1GB)

**-start** is an offset (in GB) to be applied before starting processing (default is 0GB), useful when you want to cut the end of the file only

**--help** displays instructions in the console

## Example
    filesplitter ./logs/ --min 0.01 --size 0.001
will split any selected file (files larger than 10 MB will be reported) into 1 MB files.

## Notes
Only support text files.

The splitter preserves lines, which means that pieces might be a bit longer than set with 'size' parameter, since the last line will be entirely included for each portion. This tool has thus no effect on single line text file.

Single self-contained executables can be produced with .NET Core 3 using the following command:

    dotnet publish -r win-x64 -c Release -f netcoreapp3.0 /p:PublishSingleFile=true

For other operating systems, change -r with any of the [supported identifiers](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

## License
This work is licensed under the [MIT License](LICENSE.md).
