using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class Variability
    {
        public Variability()
        {

        }

        public Variability(int _sceneNumber, int _intervalStart, int _intervalEnd, double _centroidX=0, double _centroidY=0, double _durationLen=0, double _fixationCount=0, double _standardDev=0)
        {
            sceneNumber = _sceneNumber;
            intervalStart = _intervalStart;
            intervalEnd = _intervalEnd;
            centroidX = _centroidX;
            centroidY = _centroidY;
            durationLen = _durationLen;
            fixationCount = _fixationCount;
            //mean = _mean;
            standardDev = _standardDev;


            centroidXList = new List<double>();
            centroidYList = new List<double>();
            durationList = new List<double>();
        }

        public Variability(int _sceneNumber, string _timestamp, double _centroidX =0, double _centroidY=0, double _durationLen=0, double _fixationCount=0, double _standardDev=0)
        {
            sceneNumber = _sceneNumber;
            timestamp = _timestamp;
            centroidX = _centroidX;
            centroidY = _centroidY;
            durationLen = _durationLen;
            fixationCount = _fixationCount;
            //mean = _mean;
            standardDev = _standardDev;

            centroidXList = new List<double>();
            centroidYList = new List<double>();
            durationList = new List<double>();
        }

        public void addToXList(double _centroidX)
        {
            centroidXList.Append(_centroidX);
        }

        public void addToYList(double _centroidY)
        {
            centroidYList.Append(_centroidY);
        }

        public void addToDurationList(double _duration)
        {
            centroidXList.Append(_duration);
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

        [Ignore]
        public double centroidX { get; set; }

        [Ignore]
        public double centroidY { get; set; }

        [Ignore]
        public List<double> centroidXList { get; set; }

        [Ignore]
        public List<double> centroidYList { get; set; }

        [Ignore]
        public List<double> durationList { get; set; }

        [Name("Duration Length")]
        public double durationLen { get; set; }

        [Name("Fixation Count")]
        public double fixationCount { get; set; }

        [Name("Standard Deviation")]
        public double standardDev { get; set; }
    }
}
