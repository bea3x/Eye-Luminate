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

                if (gaze_x >= 0 && gaze_y >= 0 && time > 0)
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

        


    }
}
