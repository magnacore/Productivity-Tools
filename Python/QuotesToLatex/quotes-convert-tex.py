#!/home/manuj/anaconda3/envs/xonsh/bin/xonsh
cyan = '\x1b[1;36m'
no_color = '\x1b[0m'

import os
import sys
from tqdm import tqdm

# sys.argv[0] is the name of the script which will always be there
if len(sys.argv) == 1: # No arguments passed
	selection = False
elif len(sys.argv) > 1:
	selection = True

def markup_quote(quote_line):
	split_quote = quote_line.split("~")
	quote = split_quote[0].strip()
	author = ""
	info = ""
	
	if len(split_quote) > 1:
		author = split_quote[1].strip()

	if len(split_quote) == 3:
		info = split_quote[2].strip()
	
	return f"""\\testimonial\n{{{quote}}}\n{{{author}}}\n{{{info}}}\n\n"""

if selection:
	
	input_file = sys.argv[1]

	root_ext = os.path.splitext(input_file)
	chapter_name = root_ext[0]

	tex_chapter_name = f"\\chapter{{{chapter_name}}}"

	file_contents = tex_chapter_name + "\n\n"

	with open(input_file) as quote_file:
		for quote in quote_file:
			if quote != "\n":				
				file_contents = file_contents + markup_quote(quote)

	with open(f'{chapter_name}.tex', 'w') as f:
		f.write(file_contents)

print(u'\u2500' * os.get_terminal_size().columns)
