import re

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

def find_urls(string):
    # findall() has been used
    # with valid conditions for urls in string
    regex = r"(?i)\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'\".,<>?«»“”‘’]))"
    url = re.findall(regex, string)
    return [x[0] for x in url]

def get_uuid():
    import uuid
    return str(uuid.uuid4())

def get_timestamp():
	from datetime import datetime

	date_time = datetime.now().strftime('%Y-%m-%d_%H-%M-%S')

	return date_time
