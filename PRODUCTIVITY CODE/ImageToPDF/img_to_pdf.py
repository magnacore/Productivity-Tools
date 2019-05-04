from PIL import Image
import glob
import os

allPdfFiles = glob.glob("*.jpg")

for filename in allPdfFiles:
    image = Image.open(filename)
    new_name = filename[:-4] + ".pdf"
    image.save(new_name, "PDF", resolution = 100.0)
    os.remove(filename)