from collections import defaultdict
import os
import random

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

    def heatmap_on_video_path(self, video_path, points, heat_fps=20):
        base = VideoFileClip(video_path)
        return self.heatmap_on_video(base, points, heat_fps)

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


def _example_random_points():
    def rand_point(max_x, max_y, max_t):
        return random.randint(0, max_x), random.randint(0, max_y), random.randint(0, max_t)

    return (rand_point(720, 480, 40000) for _ in range(500))


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


def readFile(filename):
    gazeData = []
    with open(filename) as csv_file:
        reader = csv.reader(csv_file, delimiter=',')

        next(reader, None) #skip header lines

        for gazeX, gazeY, time, time_diff, distance, velocity, classification, centroid_x, centroid_y in reader:
            gazeData.append(GazeData(int(gazeX), int(gazeY), int(time), int(time_diff), distance, velocity, classification, float(centroid_x), float(centroid_y))) 

    return gazeData



def subList(data):
    lst = []
    for x in data:
        lst.append([x.centroid_x, x.centroid_y, x.time])
        #lst.append([x.gazeX, x.gazeY, x.time])


    tuples = [tuple(x) for x in lst]
    return tuples


def drawHeatmap(data, vidPath, filename):
    filename = filename + ".mp4"
    vid=vidPath

    clip = VideoFileClip(vidPath)
    img_heatmapper = Heatmapper()
    video_heatmapper = VideoHeatmapper(img_heatmapper)

    heatmap_video = video_heatmapper.heatmap_on_video_path(
        video_path=vid,
        points=gPoints
     )
    fp = os.path.join(destPath,filename)
    heatmap_video.write_videofile(fp, bitrate="5000k", fps=24)


if __name__ == "__main__":
    
    filename = sys.argv[1]
    vidPath = sys.argv[2]
    destPath = sys.argv[3]
    
    fixData = readFile(filename)
    newFn = filename.rstrip('.csv')

    gPoints = subList(fixData)
    
    drawHeatmap(gPoints, vidPath, newFn+"_hm")

