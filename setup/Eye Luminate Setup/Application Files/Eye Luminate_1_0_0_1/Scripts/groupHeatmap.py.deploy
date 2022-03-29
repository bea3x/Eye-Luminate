from collections import defaultdict
import os

from moviepy.editor import *
import numpy as np
from PIL import Image
import csv

from heatmappy import Heatmapper


class VideoHeatmapper:
    def __init__(self, img_heatmapper):
        self.img_heatmapper = img_heatmapper

    def heatmap_on_video(self, base_video, points,
                         heat_fps=20,
                         keep_heat=False,
                         heat_decay_s=None):
        width, height = base_video.size

        frame_points = self._frame_points(
            points,
            fps=heat_fps,
            keep_heat=keep_heat,
            heat_decay_s=heat_decay_s
        )
        heatmap_frames = self._heatmap_frames(width, height, frame_points)
        heatmap_clips = self._heatmap_clips(heatmap_frames, heat_fps)

        return CompositeVideoClip([base_video] + list(heatmap_clips))

    def heatmap_on_video_path(self, video_path, points, heat_fps=20, keep_heat=False, heat_decay_s=None):
        base = VideoFileClip(video_path)
        return self.heatmap_on_video(base, points, heat_fps, keep_heat, heat_decay_s)

    def heatmap_on_image(self, base_img, points,
                         heat_fps=20,
                         duration_s=None,
                         keep_heat=False,
                         heat_decay_s=None):
        base_img = np.array(base_img)
        points = list(points)
        if not duration_s:
            duration_s = max(t for x, y, t in points) / 1000
        base_video = ImageClip(base_img).set_duration(duration_s)

        return self.heatmap_on_video(
            base_video, points,
            heat_fps=heat_fps,
            keep_heat=keep_heat,
            heat_decay_s=heat_decay_s
        )

    def heatmap_on_image_path(self, base_img_path, points,
                              heat_fps=20,
                              duration_s=None,
                              keep_heat=False,
                              heat_decay_s=None):
        base_img = Image.open(base_img_path)
        return self.heatmap_on_image(
            base_img, points,
            heat_fps=heat_fps,
            duration_s=duration_s,
            keep_heat=keep_heat,
            heat_decay_s=heat_decay_s
        )

    @staticmethod
    def _frame_points(pts, fps, keep_heat=False, heat_decay_s=None):
        interval = 1000 // fps
        frames = defaultdict(list)

        if not keep_heat:
            for x, y, t in pts:
                start = (t // interval) * interval
                frames[start].append((x, y))

            return frames

        pts = list(pts)
        last_interval = max(t for x, y, t in pts)

        for x, y, t in pts:
            start = (t // interval) * interval
            pt_last_interval = int(start + heat_decay_s*1000) if heat_decay_s else last_interval
            for frame_time in range(start, pt_last_interval+1, interval):
                frames[frame_time].append((x, y))

        return frames

    def _heatmap_frames(self, width, height, frame_points):
        for frame_start, points in frame_points.items():
            heatmap = self.img_heatmapper.heatmap(width, height, points)
            yield frame_start, np.array(heatmap)

    @staticmethod
    def _heatmap_clips(heatmap_frames, fps):
        interval = 1000 // fps
        for frame_start, heat in heatmap_frames:
            yield (ImageClip(heat)
                   .set_start(frame_start/1000)
                   .set_duration(interval/1000))


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

class Fixation:
    
    def __init__(self, centroidX, centroidY, time_start, time_end, duration,rank=0):
        self.centroidX = float(centroidX)
        self.centroidY = float(centroidY)
        self.time_start = int(time_start)
        self.time_end = int(time_end)
        self.duration = int(duration)
        self.rank = rank

gazeData = []
def readFile(filename):
    with open(filename) as csv_file:
        reader = csv.reader(csv_file, delimiter=',')

        next(reader, None) #skip header lines

        for centroid_x, centroid_y, time_start, time_end, duration, rank in reader:
            gazeData.append(Fixation(centroid_x, centroid_y, time_start, time_end, duration, rank)) 

gPoints = []

def subList(data):
    lst = []
    for x in data:
        #lst.append([x.centroid_x, x.centroid_y, x.time])
        lst.append([x.centroidX, x.centroidY, x.time_start])
        print(x.time_start)

    tuples = [tuple(x) for x in lst]
    #print(tuples)
    return tuples
    #return( tuple([el.centroid_x, el.centroid_y, el.time]) for el in lst)

resWidth = 852
resHeight = 480
def convert(data):

    convData = []
    for x in data:
        

        cx = round((x.centroidX * resWidth),4)
        cy = round((x.centroidY * resHeight),4)
        #print('new gx: ', gx, 'new gy: ', gy, 'new cx: ', cx, 'new cy: ', cy)

        #convData.append(GazeData(gx,gy,x.time))
        convData.append(Fixation(cx,cy,x.time_start,x.time_end,x.duration))

    return convData

def drawHeatmap(data, vidPath, destPath, vidFileName):
    filename = vidFileName
    vid=vidPath

    clip = VideoFileClip(vidPath)
    clip = clip.resize(newsize=(resWidth,resHeight))
    tempFile = "temp.mp4"
    tempDir = os.path.join(destPath, tempFile)
    clip.write_videofile(tempDir)
    clip_fps = int(clip.fps)
    img_heatmapper = Heatmapper()
    video_heatmapper = VideoHeatmapper(img_heatmapper)

    heatmap_video = video_heatmapper.heatmap_on_video_path(
        video_path=tempDir,
        points=data,
        heat_fps=clip_fps,
        keep_heat=True,
        heat_decay_s=2
     )

    fp = os.path.join(destPath,filename)
    duration = clip.duration
    heatmap_video = heatmap_video.subclip(0, duration)
    heatmap_video.write_videofile(fp, bitrate="5000k", fps=24)
    
    os.remove(tempDir)


def readTxt(txtFile):
    f = open(txtFile, "r")
    for x in f:
        x = x.strip("\n")
        readFile(x)

        '''
    for x in txtFile:
        readFile(os.path.join(mypath, x))
        #print(os.path.join(mypath, x))
        #print(x)'''

def sortByTimeS(data):
    data.sort(key=lambda x: x.time_start, reverse=False)
    return data

if __name__ == "__main__":
    #readFile('Nike_20211021-141657_finalGazeData.csv')
    #gPoints = subList(gazeData)
    filename = sys.argv[1]
    vidPath = sys.argv[2]
    vidFileName = sys.argv[3]
    destPath = sys.argv[4]

    readTxt(filename)
    
    sortedData = sortByTimeS(gazeData)

    conv = convert(sortedData)
    fixPoints = subList(conv)
    #print(conv)
    drawHeatmap(fixPoints, vidPath, destPath, vidFileName)

    
    
