#Compile instructions
# pyinstaller.exe --onefile --windowed ClipboardToFile.py

#Imports
import os
import easygui
import pyperclip
import string
import sys

# Configurables
directoryToSave = os.path.join("C:\\", "DATA", "PRODUCTIVITY SYSTEM", "01 TASK CAPTURE BIN\\")
filenameLength = 60

#Helper Functions
def format_filename(s):
    """Take a string and return a valid filename constructed from the string.
Uses a whitelist approach: any characters not present in valid_chars are
removed. Also spaces are replaced with underscores.
 
Note: this method may produce invalid filenames such as ``, `.` or `..`
When I use this method I prepend a date string like '2009_01_15_19_46_32_'
and append a file extension like '.txt', so I avoid the potential of using
an invalid filename.
 
"""
    valid_chars = "-_.() %s%s" % (string.ascii_letters, string.digits)
    filename = ''.join(c for c in s if c in valid_chars)
    filename = filename.replace('<','_')
    filename = filename.replace('>','_')
    filename = filename.replace(':','_')
    filename = filename.replace('"','_')
    filename = filename.replace('/','_')
    filename = filename.replace('\\','_')
    filename = filename.replace('|','_')
    filename = filename.replace('?','_')
    filename = filename.replace('*','_')
    return filename

#Copy the contents of the clipboard
clipboardText = pyperclip.paste()

# Extract first 20 characters
rawFileName = clipboardText[:filenameLength]

# Show the filename for editing
fileName = easygui.enterbox("Edit file name:", default = rawFileName)

if fileName == None:
    sys.exit(fileName)

# Ask for the directory to store into
storageDirectoryOnHDD = easygui.choicebox("Which Directory to Store Into?",
                                          choices = ['relaxActive',
                                                     'relaxPassiveAssorted',
                                                     'relaxPassiveAudioBooks',
                                                     'relaxPassiveLearning',
                                                     'relaxPassiveGames',
                                                     'relaxPassiveMovies',
                                                     'StudyPassiveAssorted',
                                                     'StudyPassiveTrading',
                                                     'StudyPassiveDateScience',
                                                     'StudyPassiveGameDev',
                                                     'StudyPassiveMobile'] )

# Ask for the priority
priority = easygui.choicebox("Priority: ", choices = ['Important_Urgent',
                                                      'Important_Not Urgent',
                                                      'Not Important_Urgent',
                                                      'Not Important_Not Urgent'] )

# Check if filename is valid
validFilename = format_filename(fileName)

# Write to a file
fullFileName = validFilename + " #" + storageDirectoryOnHDD + " #" + priority + ".txt"
finalFullPath = os.path.join(directoryToSave, fullFileName)

with open(finalFullPath, 'a') as file:
    file.write(clipboardText)
