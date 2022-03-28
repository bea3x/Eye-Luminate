import cv2
import os
import time
import csv
import sys
import re
#from moviepy.video.io.ffmpeg_tools import ffmpeg_extract_subclip
from moviepy.editor import *

class Fixation:
    
    def __init__(self, centroidX, centroidY, time_start, time_end, duration,rank=0):
        self.centroidX = centroidX
        self.centroidY = centroidY
        self.time_start = time_start
        self.time_end = time_end
        self.duration = duration
        self.rank = rank

class GazeData:
    def __init__(self, gazeX, gazeY, time, time_diff=0, distance=0, velocity=0, classification='', centroid_x=0, centroid_y=0):
        self.gazeX = float(gazeX)  
        self.gazeY = float(gazeY)
        self.time = int(time)
        self.time_diff = int(time_diff)
        self.distance = distance
        self.velocity = velocity
        self.classification = classification
        self.centroid_x = float(centroid_x)
        self.centroid_y = float(centroid_y)
        #self.width = 480
        #self.height = 852

class VideoClip:
        
    def __init__(self, name, fullpath, time_start, time_end, duration, rank, rating=0, rateValue=""):
        self.name = name
        self.fullpath = fullpath
        self.time_start = time_start
        self.time_end = time_end
        self.duration = duration
        self.rank = rank
        self.rating = rating
        self.rateValue = rateValue


def setResolution(width, height):
    ClipHeight = height
    ClipWidth = width


# * read data from finalGazeData
def readFile(filename):
    readList = []
    with open(filename) as csv_file:
        reader = csv.reader(csv_file, delimiter=',')
        next(reader, None) #skip header

        for gazeX, gazeY, time, time_diff, distance, velocity, classification, centroid_x, centroid_y in reader:
            readList.append(GazeData(gazeX, gazeY, time, time_diff, distance, velocity, classification, centroid_x, centroid_y)) 
    
    return readList


# * remove erroneous data post-ivt
def cleanIVT (data):
    dataList = []
    #initialize values
    centroid_x = centroid_y = 0

    for row in data:
        if row.classification == "saccade":
            centroid_x = -1
            centroid_y = -1
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



#Generate directory path
def makeDir(count=0,timestr=""):
    if(timestr == ""):
        timestr = time.strftime("%Y%m%d-%H%M%S")
    #folderpath = "./Results/" + str(timestr) + "-" + str(count)
    if (count == 0):
        folderpath = destPath + "/Results/" + str(timestr) + "-merged"
    else:
        folderpath = destPath + "/Results/" + str(timestr) + "-" + str(count)
    if not os.path.exists(folderpath):
        os.makedirs(folderpath)
    if not os.path.exists(folderpath):
        os.makedirs(folderpath)
    print ("Saving frames to " + folderpath)

    return folderpath

def makeAssetDir():
    timestr = time.strftime("%Y%m%d-%H%M%S")
    folderpath = destPath + "/Assets/" + str(timestr)
    if not os.path.exists(folderpath):
        os.makedirs(folderpath)
    if not os.path.exists(folderpath):
        os.makedirs(folderpath)
    print ("Creating folder " + folderpath)

    return folderpath



def writeFile(filename, data):
    print("Making file '", filename, "'...")

    with open(filename, mode='w', newline='') as validGazeData:
        header = ['Centroid X', 'Centroid Y', 'Time Start', 'Time End', 'Duration', 'Rank']
        writer = csv.DictWriter(validGazeData, fieldnames=header)

        writer.writeheader()
        for x in data: 
            writer.writerow({'Centroid X': str(x.centroidX), 'Centroid Y': str(x.centroidY), 'Time Start': str(x.time_start), 
            'Time End': str(x.time_end), 'Duration': str(x.duration), 'Rank': str(x.rank)})   
            
    print("Created file '", filename, "'...")


def writeClipDetails(filename, data):
    print("Making file '", filename, "'...")

    with open(filename, mode='w', newline='') as clipDetails:
        header = ['File Name', 'File Path', 'Time Start', 'Time End' , 'Duration', 'Rank', 'Rating', 'Rating Value']
        writer = csv.DictWriter(clipDetails, fieldnames=header)

        writer.writeheader()
        for x in data: 
            writer.writerow({'File Name': x.name, 'File Path': str(x.fullpath), 'Time Start': str(x.time_start), 'Time End': str(x.time_end) ,'Duration': str(x.duration), 
            'Rank': str(x.rank), 'Rating': str(x.rating), 'Rating Value': str(x.rateValue)})   
            
    print("Created file '", filename, "'...")


# * sorts data according to fixation duration, descending order
def sortByDuration(data):
    data.sort(key=lambda x: x.duration, reverse=True)
    return data

# * gets highest n from list of data where n is # set by user
def getHighestN(data, n):
    data = data[:n]

    count = 1
    for x in data:
        x.rank = count
        count += 1

    return data 

# * sorts selected scenes on chronological order before frames are extracted 
def sortByTimeStart(data):
    data.sort(key=lambda x: x.time_start, reverse=False)
    return data

#for testing purposes only
def printData(data):
    for x in data:
        print(x.centroidX, "start: ", x.time_start)


def getFrameSets(vidPath, time_start, time_end, folderpath):
    cap = cv2.VideoCapture(vidPath)
    fps = cap.get(cv2.CAP_PROP_FPS)
    
    timestamps = [cap.get(cv2.CAP_PROP_POS_MSEC)]
    calc_timestamps = [0.0]
    count = 0

    while(cap.isOpened()):
        frame_exists, curr_frame = cap.read()
        ts = cap.get(cv2.CAP_PROP_POS_MSEC)

        if frame_exists:
            timestamps.append(cap.get(cv2.CAP_PROP_POS_MSEC))
            calc_timestamps.append(calc_timestamps[-1] + 1000/fps)

            if ts >= time_start and ts <= time_end:
                cv2.imwrite(os.path.join(folderpath, "frame%d.jpg" % count), curr_frame)
                count+=1
        else:
            break

    cap.release()
    return os.path.join(folderpath, "frame0.jpg")

def selectScenes(data, vidPath):
    count = 1
    selectedClips = []
    for x in data:
        #create folder for frames
        folderpath = makeDir(count)
        #create folder for thumbnails
        #imgspath = makeAssetDir()
        imgPath = getFrameSets(vidPath, x.time_start, x.time_end, folderpath)

        clipName = createClip(vidPath, x.time_start, x.time_end, folderpath)
        #path = os.path.join(os.getcwd(),clipName)
        path = os.path.join(destPath, clipName)
        #path = destPath + "/" + clipName
        print("path: " + path)
        #imgPath =generateThumbnail(vidPath, )
        selectedClips.append(VideoClip(clipName,path,x.time_start,x.time_end,x.duration,x.rank))

        print("Done", count, "out of", len(data))
        count += 1
    
    #if len(selectedClips) > 1:
        #selectedClips = createMergedClip(vidPath, selectedClips)

    selectedClips = createMergedClip(vidPath, selectedClips)
    return selectedClips

#region sort filename with int/float properly
def atof(text):
    try:
        retval = float(text)
    except ValueError:
        retval = text
    return retval

def natural_keys(text):
    return [ atof(c) for c in re.split(r'[+-]?([0-9]+(?:[.][0-9]*)?|[.][0-9]+)', text) ]

#endregion

def createClip(vidPath, time_start, time_end, folderpath):
    x = folderpath.rfind('/')
    video_name = folderpath[x+1:] + ".mp4"
    #time_start = float(time_start)/1000.0 
    #time_end = float(time_end)/1000.0 
    #print(time_start/1000, " ", time_end/1000)
    #ffmpeg_extract_subclip(vidPath, time_start/1000, time_end/1000, targetname=video_name)
    clip = VideoFileClip(vidPath).subclip(time_start/1000, time_end/1000)
    clip = clip.resize(newsize=(resWidth,resHeight))
    clip.write_videofile(os.path.join(destPath,video_name))
    clip.close()

    return video_name

def createMergedClip(vidPath, clipsList):
    timestr = time.strftime("%Y%m%d-%H%M%S")
    video_name = timestr + "-merged.mp4"
    
    t_duration = 0
    clipLst = []
    #! TODO:use list comprehension instead
    for clip in clipsList:
        cl = VideoFileClip(clip.fullpath.replace('/','\\'))
        clipLst.append(cl)
        print("clip path",clip.fullpath.replace('/','\\'))
        t_duration += clip.duration
        print("clip duration",clip.duration)

    merged_clip = concatenate_videoclips(clipLst)
    folderpath = makeDir(0,timestr)
    imgPath = getFrameSets(vidPath, 0, merged_clip.duration, folderpath)
    path = os.path.join(destPath, video_name)
    
    #path = destPath + "/" + video_name
    #merged_clip = merged_clip.fx( vfx.speedx, 1.25)
    #resize clip
    merged_clip = merged_clip.resize(newsize=(resWidth,resHeight))
    merged_clip.write_videofile(os.path.join(destPath,video_name))

    merged_clip.close()
    clipsList.append(VideoClip(video_name,path,0,0,t_duration,0))

    return clipsList

def write2File(filename, data):
    with open(filename, mode='w', newline='') as validGazeData:
        
        header = ['Gaze X', 'Gaze Y', 'Time', 'Time Difference', 'Distance', 'Velocity', 'Classification', 'Centroid X', 'Centroid Y']
        writer = csv.DictWriter(validGazeData, fieldnames=header)

        writer.writeheader()
        for x in data: 
            writer.writerow({'Gaze X': str(x.gazeX), 'Gaze Y': str(x.gazeY), 'Time': str(x.time), 
            'Time Difference': str(x.time_diff), 'Distance': str(x.distance), 'Velocity': str(x.velocity),
            'Classification': str(x.classification), 'Centroid X': str(x.centroid_x), 'Centroid Y': str(x.centroid_y)})

def convert(data):

    convData = []
    for x in data:
        gx = round((x.gazeX * resWidth),4)
        gy = round((x.gazeY * resHeight),4)

        cx = round((x.centroid_x * resWidth),4)
        cy = round((x.centroid_y * resHeight),4)
        #print('new gx: ', gx, 'new gy: ', gy, 'new cx: ', cx, 'new cy: ', cy)

        convData.append(GazeData(gx,gy,x.time,x.time_diff,x.distance,x.velocity,x.classification,cx, cy))

    return convData

def normalize(data):
    convData = []
    for x in data:
        gx = x.gazeX/resWidth
        gy = x.gazeY/resHeight

        cx = x.centroid_x/resWidth
        cy = x.centroid_y/resHeight
        #print('new gx: ', gx, 'new gy: ', gy, 'new cx: ', cx, 'new cy: ', cy)

        convData.append(GazeData(gx,gy,x.time,x.time_diff,x.distance,x.velocity,x.classification,cx, cy))

    return convData
#480×360
resWidth = 640
resHeight = 360

if __name__ == "__main__":
    filename = sys.argv[1]
    n = int(sys.argv[2])
    vidPath = sys.argv[3]
    destPath = sys.argv[4]
    print('filename: ', filename)
    #destPath = os.path.abspath("r" + "'" + destPath + "'")
    #destPath = os.path.abspath(sys.argv[4])
    destPath = destPath.replace("\\","/")
    print("destination", destPath)
    #print("destination path", destPath.replace('\','/'))
    


    #strip filename of ext
    if filename.find('_finalGazeData.csv') != -1:
        newFn = filename.rstrip('_finalGazeData.csv')
    else:
        newFn = filename.rstrip('.csv')

    rawData = readFile(filename)
    
    convData = convert(rawData)

    newData = cleanIVT(convData)

    normData = normalize(newData)
    write2File(newFn+'_fixations.csv', normData)
        
    fixGroupData = groupFixation(normData)
    writeFile(newFn+'_fixGrouped.csv', fixGroupData)

    sortedData = sortByDuration(fixGroupData)
    writeFile(newFn+'_sorted.csv', sortedData)
    
    highestN = getHighestN(sortedData, n)  
    writeFile(newFn+'_highestN.csv', highestN)

    chronData = sortByTimeStart(highestN)

    clipInfo = selectScenes(chronData, vidPath)

    writeClipDetails(newFn+'_selectedClipInfo.csv', clipInfo)