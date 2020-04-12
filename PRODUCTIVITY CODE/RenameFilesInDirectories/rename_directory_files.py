import os
import sys

walk_dir = sys.argv[1]

#print('Walk Directory = ' + walk_dir)

# If your current working directory may change during script execution, it's recommended to
# immediately convert program arguments to an absolute path. Then the variable root below will
# be an absolute path as well. Example:
# walk_dir = os.path.abspath(walk_dir)
#print('Walk Directory (absolute) = ' + os.path.abspath(walk_dir))

for root, subdirs, files in os.walk(walk_dir):

	for subdir in subdirs:		
		if(subdir[0].isdigit() and not subdir[1].isdigit()):
			new_directory_name = "00" + subdir			
			source = os.path.join(root, subdir)
			destination = os.path.join(root, new_directory_name)
			os.rename(source, destination)

		
		if(subdir[0].isdigit() and subdir[1].isdigit()):
			new_directory_name = "0" + subdir
			source = os.path.join(root, subdir)
			destination = os.path.join(root, new_directory_name)
			os.rename(source, destination)

i = 0

for root, subdirs, files in os.walk(walk_dir):
    #print('--\nroot = ' + root)
	subdirs.sort()
	for filename in files:		
		source = os.path.join(root, filename)	

		new_file_name = '{0:03d}'.format(i) + "_" + filename		
		destination = os.path.join(root, new_file_name)
		
		os.rename(source, destination)
		
	i += 1
