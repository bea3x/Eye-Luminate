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
            //accuracyPct = _accuracyPct;

        }

        public RatingSummary(int _sceneNumber, string _timestamp, int _top1Count = 0, int _positiveCount = 0, int _negativeCount = 0, int _neutralCount = 0, double _accuracyPct=0)
        {
            sceneNumber = _sceneNumber;
            timestamp = _timestamp;
            top1Count = _top1Count;
            positiveCount = _positiveCount;
            negativeCount = _negativeCount;
            neutralCount = _neutralCount;
            accuracyPct = _accuracyPct;

        }


        [Name("Scene #")]
        public int sceneNumber { get; set; }

        [Name("Timestamp")]
        public string timestamp { get; set; }

        //[Name("Interval Start")]
        [Ignore]
        public int intervalStart { get; set; }

        //[Name("Interval End")]
        [Ignore]
        public int intervalEnd { get; set; }
        
        [Name("Rank 1 Count")]
        public int top1Count { get; set; }

        [Name("Positive Ratings")]
        public int positiveCount { get; set; }

        [Name("Negative Ratings")]
        public int negativeCount { get; set; }

        [Name("Neutral Ratings")]
        public int neutralCount { get; set; }

        [Name("Accuracy (%)")]
        public double accuracyPct { get; set; }

    }
}
