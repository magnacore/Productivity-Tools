import os

def update_counter(file_path):
    if not os.path.exists(file_path):
        with open(file_path, 'w') as f:
            f.write('0')

    with open(file_path, 'r') as f:
        counter = f.readline()

    counter = int(counter)    
    counter += 1

    with open(file_path, 'w') as f:
        f.write(str(counter))

    if counter%2 == 0:
        print('even')
    else:
        print('odd')

update_counter('counter.txt')

