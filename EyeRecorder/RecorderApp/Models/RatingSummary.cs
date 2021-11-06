using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class RatingSummary
    {
        public RatingSummary()
        {

        }

        public RatingSummary(int _sceneNumber, int _intervalStart, int _intervalEnd, int _top1Count=0, int _positiveCount=0, int _negativeCount=0, int _neutralCount=0)
        {
            sceneNumber = _sceneNumber;
            intervalStart = _intervalStart;
            intervalEnd = _intervalEnd;
            top1Count = _top1Count;
            positiveCount = _positiveCount;
            negativeCount = _negativeCount;
            neutralCount = _neutralCount;

        }


        [Index(0)]
        public int sceneNumber { get; set; }

        [Index(1)]
        public int intervalStart { get; set; }

        [Index(2)]
        public int intervalEnd { get; set; }

        [Index(3)]
        public int top1Count { get; set; }

        [Index(4)]
        public int positiveCount { get; set; }

        [Index(5)]
        public int negativeCount { get; set; }

        [Index(6)]
        public int neutralCount { get; set; }

    }
}
