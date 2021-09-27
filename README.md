# Productivity-Tools
A collection of scripts I use for my productivity system. The commands follow the following naming convention:

> who-what-how

* Who: On what entity do we want to perform the operation
* What : What do we want to do to the entity
* How : command arguments 

## Add Markdown To Right Click Menu
* Platform : Windows
* Language : Registry Script
* Description : This is a windows registry script that adds to Windows OS the ability to create markdown files by right clicking and selecting new.
* Usage : Double click the file.

## Copy Clipboard to File
* Platform : Windows
* Language : Python
* Description : This is a python executable that copies the contents of the clipboard to a file in a particular folder, and tags it with subject and priority. This is not a generic script but highly customised for my own productivity system.
* Usage : Add this to the taskbar or shortcut. It will copy text from clipboard when executed.

## Move to Individual Directories
* Platform : Windows
* Language : C#
* Description : When executed, it will copy all the files to an individual directory and renames the directories in sequence.
* Usage : Copy this script to a directory where there are lots of individual files.

## Rename MP3 to MKA
* Platform : Windows
* Language : Batch
* Description : This script renames MP3 files to MKA files. This is written because after splitting MP3 files, they dont work unless renamed to MKA for some reason.
* Usage : Execute the script in the same folder where MP3s are present. This script is only useful for MP3 files which are splitted using Split Video and Audio script, not for regular MP3 files.

## Split Text
* Platform : Windows
* Language : C#
* Description : This is a C# script that splits a text file into 5KB or 10KB files. Useful for consuming large articles or text in chuncks and then use a text to speech reader. It does not destory the original file.
* Usage : Execute this program in the same directory as the large text file that needs to be split.

## Split Video and Audio
* Platform : Windows
* Language : Batch
* Description : This is a windows tool that need [MKVToolNix](https://www.videohelp.com/software/MKVToolNix) to be installed on the system beforehand. I use this tool to split long videos, such as Keynotes into 5 minute chunks and load them into smartphone to be reviewed while travelling.
* Usage : Execute the script in the same folder as the video or audio file and it will split it into 5 or 10 minutes chuncks. Note: after splitting audio files, they need to be renamed using the Rename MP3 to MKA script else they wont play properly. Videos dont need any futher processing.


## Split PDF
* Platform : Anywhere Python can run
* Language : Python
* Description : This is a Python script to split a PDF into batches of 10 pages each. The idea is to split a PDF converted using Calibre from ePUBs and split them for reading on phone in sprints of 10 pages each. It can split multiple files in the same directory.
* Usage : Execute this script in the same directory as the PDF file that needs to be split.

## Image To PDF
* Platform : Anywhere Python can run
* Language : Python
* Description : This is a Python script to convert jpg files to a PDF document.
* Usage : Execute this script in the same directory as the jpg file that needs to be combined into a PDF document.

## Merge PDF
* Platform : Anywhere Python can run. Requires PyPDF2
* Language : Python
* Description : This is a Python script to combine multiple pdf documents into a single PDF document.
* Usage : Execute this script in the same directory as the PDF file that needs to be combined into a single PDF document.

## Rename Files In Directories
* Platform : Anywhere Python can run
* Language : Python
* Description : This is a Python script to append integer counts in files and folders.
* Usage : python rename_directory_files.py <name of directory>

## Autokey Linux
* Platform : Linux
* Description : These are AutoKey shortcuts used for increasing productivity.
* 7z Encrypt File : Compress a file using 7zip and add a password
* 7z Unrar : Unrar a *.rar file
* Hibernate : If supported, hibernate PopOS

And many many more...! :D
