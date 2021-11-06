using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RecorderApp.Utility
{
    public sealed class IOService
    {
        public string OpenFile(string defaultExtension, string filter, string initialDir)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = defaultExtension;
            fd.Filter = filter;
            // initial directory for open file dialog
            fd.InitialDirectory = initialDir;
            //fd.InitialDirectory = @"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\bin\x86\Debug\Output";

            // checks if a file is selected after dialog pops up
            bool? result = fd.ShowDialog();

            // if result is true, return filename (path)
            return result.Value ? fd.FileName : null;
        }

        public List<string> OpenFiles(string defaultExtension, string filter, string initialDir)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = defaultExtension;
            fd.Filter = filter;
            // initial directory for open file dialog
            fd.InitialDirectory = initialDir;
            fd.Multiselect = true;
            //fd.InitialDirectory = @"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\bin\x86\Debug\Output";

            // checks if a file is selected after dialog pops up
            List<string> files = new List<string>();
            if ((bool)fd.ShowDialog())
            {
                foreach (string selectedfile in fd.FileNames)
                {
                    files.Add(selectedfile);
                }
                return files;
            } 
            else
            {
                return null;
            }

        }

        public FileInfo SaveFile(string defaultExtension, string filter, string initialDir)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = defaultExtension;
            sfd.Filter = filter;
            sfd.InitialDirectory = initialDir;
            //sfd.FileName = defaultFileName;

            bool? result = sfd.ShowDialog();

            Console.WriteLine("result value: " + result);


            return result.Value ? new FileInfo(sfd.FileName) : null;
        }
    }
}
