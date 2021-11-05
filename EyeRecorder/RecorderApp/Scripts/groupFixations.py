'''
consolidation of python scripts for 

cleanIVT > groupFixations > findHighestN

'''

import csv
import sys

class Fixation:    
    def __init__(self, centroidX, centroidY, time_start, time_end, duration):
        self.centroidX = centroidX
        self.centroidY = centroidY
        self.time_start = time_start
        self.time_end = time_end
        self.duration = duration

class GazeData:
    def __init__(self, gazeX, gazeY, time, time_diff=0, distance=0, velocity=0, classification='', centroid_x=0, centroid_y=0):
        self.gazeX = gazeX
        self.gazeY = gazeY
        self.time = time
        self.time_diff = time_diff
        self.distance = distance
        self.velocity = velocity
        self.classification = classification
        self.centroid_x = centroid_x
        self.centroid_y = centroid_y 


# * read data from finalGazeData
def readFile(filename):
    readList = []
    with open(filename) as csv_file:
        reader = csv.reader(csv_file, delimiter=',')
        next(reader, None) #skip header

        for gazeX, gazeY, time, time_diff, distance, velocity, classification, centroid_x, centroid_y in reader:
            readList.append(GazeData(int(gazeX), int(gazeY), int(time), int(time_diff), distance, velocity, classification, centroid_x, centroid_y)) 
    
    return readList


# * remove erroneous data post-ivt
def cleanIVT (data):
    dataList = []
    #initialize values
    centroid_x = centroid_y = 0
         

    for idx, row in enumerate(data):
        nextX = 0
        if (idx+1 < len(data)):
            nextEl = data[idx+1]
            nextC = nextEl.classification

        if row.classification == "saccade":
            centroid_x = 0
            centroid_y = 0
        else: 
            centroid_x = row.centroid_x
            centroid_y = row.centroid_y

        dataList.append(GazeData(row.gazeX, row.gazeY, row.time, row.time_diff, row.distance, 
            row.velocity, row.classification, centroid_x, centroid_y))

    return dataList


# * group fixations and reshape? data structure 
def groupFixation(data):
    dataList = []
    inGroup = False
    timeStart = 0

    for idx, row in enumerate(data):
        nextX = 0
        if (idx+1 < len(data)):
            nextEl = data[idx+1]
            prevEl = data[idx-1]
            nextX = nextEl.centroid_x
            #print(row.centroidX, " ", nextEl.centroidX)
        # * check if fixation or saccade 
        if row.classification == "fixation":
            # * check if next row has the same centroid_x coordinate (whether theyre in the same fixation group)
            if row.centroid_x == nextX: #if ingroup
            
                if not(inGroup):
                    timeStart = row.time
                    inGroup = True
            else:
                inGroup = False
                # * checks if fixation is the only member of group. if so, duration is set according to timestamp of current row and timestamp of next row 
                # TODO: check validity of logic lol
                if timeStart == 0 or prevEl.classification == "saccade" and nextEl.classification == "saccade":
                    print(row.time, nextEl.time)
                    timeStart = row.time
                    timeEnd = nextEl.time
                else:
                    timeEnd = row.time
                duration = int(timeEnd) - int(timeStart)

                dataList.append(Fixation(row.centroid_x, row.centroid_y, timeStart, timeEnd, duration))

    return dataList


def writeFile(filename, data):
    print("Making file '", filename, "'...")

    with open(filename, mode='w', newline='') as validGazeData:
        header = ['Centroid X', 'Centroid Y', 'Time Start', 'Time End', 'Duration']
        writer = csv.DictWriter(validGazeData, fieldnames=header)

        writer.writeheader()
        for x in data: 
            writer.writerow({'Centroid X': str(x.centroidX), 'Centroid Y': str(x.centroidY), 'Time Start': str(x.time_start), 
            'Time End': str(x.time_end), 'Duration': str(x.duration)})        

    print("Created file '", filename, "'...")

def write2File(filename, data):
    with open(filename, mode='w', newline='') as validGazeData:
        
        header = ['Gaze X', 'Gaze Y', 'Time', 'Time Difference', 'Distance', 'Velocity', 'Classification', 'Centroid X', 'Centroid Y']
        writer = csv.DictWriter(validGazeData, fieldnames=header)

        writer.writeheader()
        for x in data: 
            writer.writerow({'Gaze X': str(x.gazeX), 'Gaze Y': str(x.gazeY), 'Time': str(x.time), 
            'Time Difference': str(x.time_diff), 'Distance': str(x.distance), 'Velocity': str(x.velocity),
            'Classification': str(x.classification), 'Centroid X': str(x.centroid_x), 'Centroid Y': str(x.centroid_y)})

if __name__ == "__main__":
    fileName = sys.argv[1]

    rawData = readFile(fileName)
    
    newData = cleanIVT(rawData)
    
    write2File('fixations.csv', newData)

    fixGroupData = groupFixation(newData)
    
    writeFile('fixationGrps.csv', fixGroupData)