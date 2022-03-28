using CsvHelper;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class CleanDataViewModel : BindableBase
    {
        public CleanDataViewModel()
        {

        }

        
        private void check(List<RawGaze> rdata)
        {
            foreach(RawGaze data in rdata)
            {
                Console.WriteLine(data.gazeX + " " + data.gazeY + " " + data.time);
            }
        }

        int gaze_x, gaze_y = 0;
        int time = 0;

        /// <summary>
        /// remove entries with zero values
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<RawGaze> removeBlanks(List<RawGaze> data)
        {
            List<RawGaze> GazeList = new List<RawGaze>();
            foreach(RawGaze obj in data)
            {
                gaze_x = Convert.ToInt32(obj.gazeX);
                gaze_y = Convert.ToInt32(obj.gazeY);
                time = Convert.ToInt32(obj.time);

                if (gaze_x >= 0 && gaze_x <= 1920 && gaze_y >= 0 && gaze_y <= 1080 && time > 0)
                {
                    GazeList.Add(new RawGaze(gaze_x, gaze_y, time));
                }
            }
            return GazeList;
        }

        /// <summary>
        /// remove one of duplicate timestamps
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<RawGaze> removeDuplicates(List<RawGaze> data)
        {
            List<RawGaze> GazeList = new List<RawGaze>();
            int prev_time = 0;
            foreach(RawGaze obj in data)
            {
                if (obj.time > prev_time)
                {
                    GazeList.Add(new RawGaze(obj.gazeX, obj.gazeY, obj.time));
                }

                prev_time = obj.time;
            }

            return GazeList;
        }

        /// <summary>
        /// if gaze coordinate has not changed after 100ms, consider duplicates as invalid
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<RawGaze> removeSuspicious(List<RawGaze> data)
        {
            List<RawGaze> GazeList = new List<RawGaze>();
            int start_time = 0;
            int prev_gaze_x = -1;
            int prev_gaze_y = -1;
            
            bool counter_set = false; //check if start counter set
            int max_time = 100; //max time for same coordinates (ms)
            foreach (RawGaze obj in data)
            {
                gaze_x = Convert.ToInt32(obj.gazeX);
                gaze_y = Convert.ToInt32(obj.gazeY);
                time = Convert.ToInt32(obj.time);

                if (gaze_x == prev_gaze_x && gaze_y == prev_gaze_y)
                {
                    if (!counter_set)
                    {
                        start_time = time;
                        counter_set = true;
                    } 

                    if (time-start_time > max_time)
                    {

                        GazeList.Add(new RawGaze(-1, -1, obj.time));
                    }
                    else
                    {
                        GazeList.Add(new RawGaze(obj.gazeX, obj.gazeY, obj.time));
                    }

                }
                else
                {

                    GazeList.Add(new RawGaze(obj.gazeX, obj.gazeY, obj.time));
                    counter_set = false;
                }

                prev_gaze_x = gaze_x;
                prev_gaze_y = gaze_y;
                // 1. if prev is the same as the current gaze x and y
                // 2. if start_time is not set yet, set it and turn boolean value to true; else, kepp going
                // 3. if time-start_time > 100 and gaze x and gazey is still the same, then turn gazex and gazey to -1
            }

            return GazeList;
        }


    }
}
