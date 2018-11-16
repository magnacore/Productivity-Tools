# Productivity-Tools
A collection of scripts I use for my productivity system

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