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