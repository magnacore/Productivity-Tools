import mimetypes
mimetypes.init()

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
def get_mime(file):
	mimestart = mimetypes.guess_type(file)[0]

	if mimestart != None:
		mimestart = mimestart.split('/')[0]
		return mimestart.strip().lower()
	else:
		return None

def get_duration(file):
	duration = $(ffprobe -v quiet -of csv=p=0 -show_entries format=duration @(file))

	if duration == '':
		print(f"{file} is not returning a valid duration.")
		duration = '0'

	return float(duration)
