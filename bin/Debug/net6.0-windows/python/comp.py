import zlib

def comp(path):
    with open(path[:-4] + ".recom", 'rb') as file:
        data = file.read()
    compressed = zlib.compress(data, level=zlib.Z_BEST_COMPRESSION)
    with open(path, 'ab') as f_out:
        f_out.write(compressed)