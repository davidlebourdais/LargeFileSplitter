# Large File Splitter

A simple program to break large text files into smaller pieces. 

It handles any sizes and is usually used for files > GB. 

## Usage

Place .exe into the folder that holds your larges files then follow instructions or give output file folder path as parameter.

## Arguments
**-min** specifies the minimum size (GB) large files must have to be displayed in to menu (default is 2GB)

**-size** is the maximum final size (GB) that cut pieces should be (default is 1GB)

**-start** is a offset (in GB) to be applied before starting processing (default is 0GB), useful when you want to cut the end of the file only

**--help** displays instructions in the console

## Example
filesplitter ./logs/ --min 0.01 --size 0.001
will split any selected file (files larger than 10 MB will be reported) into 1 MB files.

## Note
Only support text files (v0.5)
Single executable can be produced with .NET Core 3.0