import os
import sys

def getfilesize(filename):
   with open(filename,"rb") as fr:
       fr.seek(0,2) # move to end of the file
       size=fr.tell()
       print("getfilesize: size: %s" % size)
       return fr.tell()

def splitfile(filename, splitsize):
   # Open original file in read only mode
   if not os.path.isfile(filename):
       print("No such file as: \"%s\"" % filename)
       return

   filesize=getfilesize(filename)
   with open(filename,"rb") as fr:
    counter=1
    orginalfilename = filename.split(".")
    readlimit = 5000 #read 5kb at a time
    n_splits = filesize//splitsize
    print("splitfile: No of splits required: %s" % str(n_splits))
    for i in range(n_splits+1):
        chunks_count = int(splitsize)//int(readlimit)
        data_5kb = fr.read(readlimit) # read
        # Create split files
        print("chunks_count: %d" % chunks_count)
        with open(orginalfilename[0]+"_{id}.".format(id=str(counter))+orginalfilename[1],"ab") as fw:
            fw.seek(0) 
            fw.truncate()# truncate original if present
            while data_5kb:                
                fw.write(data_5kb)
                if chunks_count:
                    chunks_count-=1
                    data_5kb = fr.read(readlimit)
                else: break            
        counter+=1 

if __name__ == "__main__":
   if len(sys.argv) < 3: print("Filename or splitsize not provided: Usage:     filesplit.py filename splitsizeinkb ")
   else:
       filesize = int(sys.argv[2]) * 1000 #make into kb
       filename = sys.argv[1]
       splitfile(filename, filesize)