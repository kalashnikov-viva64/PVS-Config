# Utility for splitting sln-files of Visual Studio into a specified number of parts.
#
# Usage: SlnSpliter.py <slnFile> <numParts>
# <slnFile> - sln-file of Visual Studio
# <numParts> - number of parts

import sys
import os

def calcProjects(slnFile):
	cnt = 0
	f = open(slnFile)
	for line in f:
		if line.find("Project(") >= 0:
			cnt = cnt + 1
	f.close()
	return cnt
	
def makePart(slnFile, partFile, part, first, end):
	print(slnFile, partFile, first, end)
	cnt = 0
	body = False
	f1 = open(slnFile, 'r')
	f2 = open(partFile, 'w')

	for line in f1:
		if line.find("Project(") >= 0:
			cnt = cnt + 1
			body = True
		
		if not body or (first<=cnt and cnt<=end):
			f2.write(line)
			
		if line.find("EndProject") >= 0:
			body = False
		
	f1.close()
	f2.close()
	
def main():
	# sln-file of Visual Studio
	slnFile = sys.argv[1]
	# number of parts
	numParts = int(sys.argv[2])

	print("Utility for splitting sln-files of Visual Studio into a specified number of parts.")
	if not os.path.isfile(slnFile) or (numParts<=0 or numParts>9):
		print(	"(x) The program was started incorrectly!\n\n")
		print(	"Usage: SlnSpliter.py <slnFile> <numParts>\n"
				"<slnFile> - sln-file of Visual Studio\n"
				"<numParts> - number of parts")
		exit(1)
	
	slnDir = os.path.dirname(slnFile)
	baseName, baseExt = os.path.splitext(slnFile)
	numProj = calcProjects(slnFile)
	
	i = 1
	first = 1
	while i <= numParts:
		partFile = "%s_%d%s"%(baseName, i, baseExt)
		end = (numProj*i)/numParts
		makePart(slnFile, partFile, i, first, end)
		first = end+1
		i = i + 1

# Start point
main()