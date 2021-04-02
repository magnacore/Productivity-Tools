import PyPDF2 as PDF
import glob

allPdfFiles = glob.glob("*.pdf")
merger = PDF.PdfFileMerger(strict=False)

for filename in allPdfFiles:
    merger.append(PDF.PdfFileReader(filename, "rb"))

merger.write("pdf_merged.pdf")