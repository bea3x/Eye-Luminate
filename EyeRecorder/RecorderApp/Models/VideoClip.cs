using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class VideoClip
    {
        public VideoClip()
        {

        }

        public VideoClip(string _fileName, string _filePath, int _timeStart, int _timeEnd, int _duration, int _rank, int _rating, string _rateValue)
        {
            fileName = _fileName;
            filePath = _filePath;
            timeStart = _timeStart;
            timeEnd = _timeEnd;
            duration = _duration;
            rank = _rank;
            rating = _rating;
            rateValue = _rateValue;
        }


        [Index(0)]
        public string fileName { get; set; }

        [Index(1)]
        public string filePath { get; set; }

        [Index(2)]
        public int timeStart { get; set; }

        [Index(3)]
        public int timeEnd { get; set; }

        [Index(4)]
        public int duration { get; set; }

        [Index(5)]
        public int rank { get; set; }

        [Index(6)]
        public int rating { get; set; }

        [Index(7)]
        public string rateValue { get; set; }
    }
}
