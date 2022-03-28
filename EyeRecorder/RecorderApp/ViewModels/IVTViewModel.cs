using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecorderApp.ViewModels
{
    public class IVTViewModel : BindableBase
    {
        public IVTViewModel()
        {

        }

        #region ivt functions

        private double _threshold;

        public double Threshold
        {
            get { return _threshold; }
            set { _threshold = 1.15; }
        }


        private double getDistance(int x1, int y1, int x2, int y2)
        {
            double distance;

            distance = Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2);
            distance = Math.Sqrt(distance);

            return distance;
        }

        private double getVelocity(double distance, int time)
        {
            double velocity;

            velocity = distance / time;

            return velocity;
        }

        private string getClassification(double velocity, double threshold)
        {
            if (velocity > threshold)
            {
                return "saccade";
            }
            else
            {
                return "fixation";
            }
        }

        private double getCentroidX(double sumX, int n)
        {
            return Math.Round((sumX / n), 2);
        }

        private double getCentroidY(double sumY, int n)
        {
            return Math.Round((sumY / n), 2);
        }

        #endregion

        List<GazeData> gazeData = new List<GazeData>();

        /// <summary>
        /// get distance, velocity, classification and centroid coordinates
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<GazeData> runIVT(List<RawGaze> data)
        {
            List<GazeData> afterIVT = new List<GazeData>();
            int sum_x = 0, sum_y = 0, n = 0;
            int gaze_x = 0, gaze_y = 0, gaze_time = 0, time_diff = 0;
            double distance = 0, velocity = 0, centroid_x = 0, centroid_y = 0;
            string classification = "";
            
            foreach(RawGaze obj in data)
            {
                if (gaze_x > 0 && gaze_y > 0)
                {
                    time_diff = obj.time - gaze_time;
                    distance = getDistance(gaze_x, gaze_y, obj.gazeX, obj.gazeY);
                    velocity = getVelocity(distance, time_diff);
                    classification = getClassification(velocity, 1.15);
                }

                if (classification == "fixation")
                {
                    sum_x += obj.gazeX;
                    sum_y += obj.gazeY;
                    n++;
                    centroid_x = getCentroidX(sum_x, n);
                    centroid_y = getCentroidY(sum_y, n);
                }
                else if (classification == "saccade" && sum_x > 0 || sum_y > 0 || n > 0)
                {
                    centroid_x = centroid_y = 0;
                    sum_x = sum_y = n = 0;
                }

                gaze_x = obj.gazeX;
                gaze_y = obj.gazeY;
                gaze_time = obj.time;

                afterIVT.Add(new GazeData(obj.gazeX, obj.gazeY, obj.time, time_diff,
                    distance, velocity, classification, centroid_x, centroid_y));

            }



            return afterIVT;
        }

        /// <summary>
        /// fix centroid coordinates according to fixation group
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<GazeData> fixationGroup(List<GazeData> data)
        {
            List<GazeData> newData = new List<GazeData>();

            double centroid_x = 0;
            double centroid_y = 0;
            bool inGroup = false;

            // iterate through gazedata list in reverse
            for (int i = data.Count-1; i > 0; i--)
            {
                GazeData obj = data[i];
                                
                if (inGroup)
                {
                     
                    obj.CentroidX = centroid_x;
                    obj.CentroidY = centroid_y;
                }
                else
                {
                    // check if classification is fixation and get the centroid of the group 
                    if (obj.Classification.Equals("fixation"))
                    {
                        inGroup = true;
                        centroid_x = obj.CentroidX;
                        centroid_y = obj.CentroidY;
                    }
                }
                
                if (obj.Classification.Equals("saccade"))
                {
                    inGroup = false;
                    centroid_x = 0;
                    centroid_y = 0;
                }

            }

            return data;
        }

        public List<GazeData> normalizeCoords(List<GazeData> data, int width, int height)
        {
            if (width == 0 || height == 0)
            {
                width = 1920;
                height = 1080;
            }
                
            Console.WriteLine("W: " + width + " H: " + height);
            List<GazeData> newData = new List<GazeData>();
            foreach (GazeData row in data)
            {
             
                double gx = row.GazeX / width;
                double gy = row.GazeY / height;

                double cx = row.CentroidX / width;
                double cy = row.CentroidY / height;

                GazeData newObj = new GazeData(gx, gy, row.Time, row.TimeDiff, row.Distance, row.Velocity, row.Classification, cx, cy);

                newData.Add(newObj);
            }

            return newData;
        }


    }
}
