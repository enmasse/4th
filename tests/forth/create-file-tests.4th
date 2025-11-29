INCLUDE "../ttester.4th"

TESTING CREATE-FILE creates file and returns valid handle
T{ S" testfile.txt" 1 CREATE-FILE SWAP DROP 0= -> TRUE }T
T{ CLOSE-FILE -> }T