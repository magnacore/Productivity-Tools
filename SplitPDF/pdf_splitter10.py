# pdf_splitter.py

import os
import glob
from PyPDF2 import PdfFileReader, PdfFileWriter

# Print iterations progress
def printProgressBar (iteration, total, prefix = '', suffix = '', decimals = 1, length = 100, fill = '█'):
    """
    Call in a loop to create terminal progress bar
    @params:
        iteration   - Required  : current iteration (Int)
        total       - Required  : total iterations (Int)
        prefix      - Optional  : prefix string (Str)
        suffix      - Optional  : suffix string (Str)
        decimals    - Optional  : positive number of decimals in percent complete (Int)
        length      - Optional  : character length of bar (Int)
        fill        - Optional  : bar fill character (Str)
    """
    percent = ("{0:." + str(decimals) + "f}").format(100 * (iteration / float(total)))
    filledLength = int(length * iteration // total)
    bar = fill * filledLength + '-' * (length - filledLength)
    print('\r%s |%s| %s%% %s' % (prefix, bar, percent, suffix), end = '\r')
    # Print New Line on Complete
    if iteration == total:
        print()

def pdf_splitter(paths):

	for path in paths:
		fname = path

		pdf = PdfFileReader(path)

		pdf_writer = PdfFileWriter()

		for page in range(pdf.getNumPages()):
			pdf_writer.addPage(pdf.getPage(page))

			if (page % 10 == 0) or (page+1 == pdf.getNumPages()):
                output_filename = f"{fname}_page_{str(page + 1).zfill(4)}.pdf"                

				with open(output_filename, 'wb') as out:
					pdf_writer.write(out)

				pdf_writer = PdfFileWriter()
			printProgressBar(page+1, pdf.getNumPages(), prefix = 'Progress:', suffix = 'Complete', length = 50)

if __name__ == '__main__':
    paths = glob.glob('*.pdf')
    paths.sort()
    pdf_splitter(paths)
