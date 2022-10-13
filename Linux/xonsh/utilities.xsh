def generateKey(string, key):
	key = list(key)
	if len(string) == len(key):
		return(key)
	else:
		for i in range(len(string) - len(key)):
			key.append(key[i % len(key)])
	return("" . join(key))

def cipherText(string, key, ascii_min=32, ascii_max=126):
	cipher_text = []
	string = str(string)
	for i in range(len(string)):
		x = ((ord(string[i])-ascii_min) + (ord(key[i])-ascii_min)) % ((ascii_max+1)-ascii_min)
		cipher_text.append(chr(x+ascii_min))
	return("" . join(cipher_text))

def originalText(cipher_text, key, ascii_min=32, ascii_max=126):
	orig_text = []
	for i in range(len(cipher_text)):
		x = ((ord(cipher_text[i])-ascii_min) - (ord(key[i])-ascii_min) + ((ascii_max+1)-ascii_min)) % ((ascii_max+1)-ascii_min)
		orig_text.append(chr(x+ascii_min))
	return("" . join(orig_text))

# get_mime cannot detect mime for mka files - this is a linux problem?
def get_mime(file, part=0):
	import mimetypes
	mimetypes.init()

	mimestart = mimetypes.guess_type(file)[0]

	if mimestart != None:
		mimestart = mimestart.split('/')[part]
		return mimestart.strip().lower()
	else:
		return None

def get_duration(file):
	from rich import print
	duration = $(ffprobe -v quiet -of csv=p=0 -show_entries format=duration @(file))

	if duration == '':
		print(f"[bold red]{file} is not returning a valid duration.")
		duration = '0'

	return float(duration)

def get_filename_extension(file):
	import os
	filename = os.path.splitext(file)[0]
	extension = os.path.splitext(file)[1]

	return filename, extension

def get_timestamp():
	from datetime import datetime

	date_time = datetime.now().strftime('%Y-%m-%d_%H-%M-%S')

	return date_time

def get_valid_filename(value, allow_unicode=False):
	"""
	Taken from https://github.com/django/django/blob/master/django/utils/text.py
	Convert to ASCII if 'allow_unicode' is False. Convert spaces or repeated
	dashes to single dashes. Remove characters that aren't alphanumerics,
	underscores, or hyphens. Convert to lowercase. Also strip leading and
	trailing whitespace, dashes, and underscores.
	"""
	import unicodedata
	import re

	value = str(value)

	if allow_unicode:
		value = unicodedata.normalize("NFKC", value)
	else:
		value = (
			unicodedata.normalize("NFKD", value)
			.encode("ascii", "ignore")
			.decode("ascii")
		)

	# use value.lower() below to convert all characters to lowercase
	# because Windoze is case insensitive. This is to prevent file overwrite.
	value = re.sub(r"[^\w\s-]", "", value.lower())
	return re.sub(r"[-\s]+", "-", value).strip("-_")

def set_valid_file_names(filenames):
	import ast

	clean_file_names = $(file-rename-valid @(filenames))
	return ast.literal_eval(clean_file_names) # Convert string representation of a list to a real list

def get_user_selection(options):
	from simple_term_menu import TerminalMenu

	terminal_menu = TerminalMenu(options)
	menu_entry_index = terminal_menu.show()
	return options[menu_entry_index]