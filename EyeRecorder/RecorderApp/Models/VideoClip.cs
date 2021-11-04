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

        public VideoClip(string _fileName, string _filePath, int _duration, int _rank, string _imgPath, int _rating, string _rateValue)
        {
            fileName = _fileName;
            filePath = _filePath;
            duration = _duration;
            rank = _rank;
            imgPath = _imgPath;
            rating = _rating;
            rateValue = _rateValue;
        }


        [Index(0)]
        public string fileName { get; set; }

        [Index(1)]
        public string filePath { get; set; }

        [Index(2)]
        public int duration { get; set; }

        [Index(3)]
        public int rank { get; set; }

        [Index(4)]
        public string imgPath { get; set; }

        [Index(5)]
        public int rating { get; set; }

        [Index(6)]
        public string rateValue { get; set; }
    }
}
