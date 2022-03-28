using CsvHelper.Configuration.Attributes;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderApp.Models
{
    public class GazeData : BindableBase
    {
        public GazeData()
        {

        }

        public GazeData(int gazeX, int gazeY, int time, int timeDiff, double distance, double velocity, string classification, double centroidX, double centroidY)
        {
            _gazeX = Convert.ToDouble(gazeX);
            _gazeY = Convert.ToDouble(gazeY);
            _time = time;
            _timeDiff = timeDiff;
            _distance = distance;
            _velocity = velocity;
            _classification = classification;
            _centroidX = centroidX;
            _centroidY = centroidY;
        }

        public GazeData(double gazeX, double gazeY, int time, int timeDiff, double distance, double velocity, string classification, double centroidX, double centroidY)
        {
            _gazeX = gazeX;
            _gazeY = gazeY;
            _time = time;
            _timeDiff = timeDiff;
            _distance = distance;
            _velocity = velocity;
            _classification = classification;
            _centroidX = centroidX;
            _centroidY = centroidY;
        }

        private double _gazeX;
        [Index(0)]
        public double GazeX
        {
            get
            {
                return _gazeX;
            }
            set
            {
                _gazeX = value;
                RaisePropertyChanged("GazeX");
            }
        }

        private double _gazeY;
        [Index(1)]
        public double GazeY
        {
            get
            {
                return _gazeY;
            }
            set
            {
                _gazeY = value;
                RaisePropertyChanged("GazeY");
            }
        }

        private int _time;
        [Index(2)]
        public int Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                RaisePropertyChanged("Time");
            }
        }

        private int _timeDiff;
        [Index(3)]
        public int TimeDiff
        {
            get { return _timeDiff; }
            set 
            { 
                _timeDiff = value;
                RaisePropertyChanged("TimeDiff");
            }
        }

        private double _distance;
        [Index(4)]
        public double Distance
        {
            get { return _distance; }
            set 
            { 
                _distance = value;
                RaisePropertyChanged("Distance");
            }
        }

        private double _velocity;
        [Index(5)]
        public double Velocity
        {
            get { return _velocity; }
            set 
            { 
                _velocity = value;
                RaisePropertyChanged("Velocity");
            }
        }

        private string _classification;
        [Index(6)]
        public string Classification
        {
            get { return _classification; }
            set 
            { 
                _classification = value;
                RaisePropertyChanged("Classification");
            }
        }


        private double _centroidX;
        [Index(7)]
        public double CentroidX
        {
            get { return _centroidX; }
            set 
            { 
                _centroidX = value;
                RaisePropertyChanged("CentroidX");
            }
        }

        private double _centroidY;
        [Index(8)]
        public double CentroidY
        {
            get { return _centroidY; }
            set 
            { 
                _centroidY = value;
                RaisePropertyChanged("CentroidY");
            }
        }


    }
}
