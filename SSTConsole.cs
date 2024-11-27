using System;
using System.IO;

using sstNET.Galaxy;
using sstNET.Galaxy.SectorObjects;

namespace sstNET
{
    public class SSTConsole
    {
        private string[] _lines;
        private int _linePtr = 0;
        private StreamWriter _sw = null;
        private StreamReader _sr = null;

        public SSTConsole(Arguments parsedArgs)
        {
            if (System.Console.WindowWidth < 100)
                System.Console.WindowWidth = 100;

            if (System.Console.WindowHeight < 40)
                System.Console.WindowHeight = 40;

            System.Console.Title = "Super Star Trek - 2006";
            if (parsedArgs.keys != null && File.Exists(parsedArgs.keys))
            {
                _sr = new StreamReader(parsedArgs.keys);
            }//if
            else if (parsedArgs.cmds != null)
                this.setLine(parsedArgs.cmds);

            if (parsedArgs.output != null)
            {
                _sw = new StreamWriter(parsedArgs.output);
            }//if
        }//Console ctor

        public static bool isit(object citem, string str)
        {
            string item = citem as string;
            //compares str to scanned citem and returns true if it matches to the length of str
            if (item != null && item.Length >= 1)
            {
                if (item.Length > str.Length)
                    return false;

                int len = Math.Min(item.Length, str.Length);
                return (string.Compare(str, 0, item, 0, len, true) == 0);
            }
            return false;
        }

        public void stars()
        {
            WriteLine("******************************************************\n");
        }//stars

        private void setLine(string str)
        {
            _lines = str.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            _linePtr = 0;
        }

        public bool EOL { get { return _lines == null || _linePtr >= _lines.Length; } }

        public object scan()
        {
            if (_lines == null)
            {
                string line = null;
                if (_sr != null)
                {
                    line = _sr.ReadLine();
                    if (line == null)
                    {
                        _sr.Close();
                        _sr = null;
                    }
                    else
                    {
                        WriteLine(line);
                    }
                }//if

                if (line == null)
                    line = System.Console.ReadLine();

                if (line.StartsWith(";"))
                    line = " ";

                this.setLine(line.Trim());
            }//while

            if (_linePtr >= _lines.Length)
            {
                _lines = null;
                return null;
            }

            string tok = _lines[_linePtr++];
            char thisChar = tok[0];
            if (char.IsDigit(thisChar) || thisChar == '+' || thisChar == '-' || thisChar == '.')
            {
                // treat as a number
                try
                {
                    return double.Parse(tok);
                }
                catch (Exception)
                {
                    this.chew();
                    return null;
                }
            }
            return tok;
        }

        public void chew()
        {
            _lines = null;
        }//chew
        //todo - check where this is used in original?
        public void chew2()
        {
            _lines = null;
        }//chew

        public bool ja()
        {
            chew();
            while (true)
            {
                object citem = scan();
                chew();

                if (citem is string && (citem as string).Length > 0)
                {
                    char thisChar = char.ToLower((citem as string)[0]);
                    if (thisChar == 'y') return true;
                    if (thisChar == 'n') return false;
                }
                Write("Please answer with \"Y\" or \"N\":");
            }//while
        }//ja

        public void huh()
        {
            chew();
            WriteLine("\nBeg your pardon, Captain?");
        }//huh

        public void Close()
        {
            if (_sr != null)
                _sr.Close();

            if (_sw != null)
                _sw.Close();

        }//Close

        public void crmena(bool stars, SectorObject so, bool key, SectorCoordinate sc)
        {
            if (stars)
                Write("***");

            Write("{0} at{1}", so.Name, sc.ToString(key));

        }//crmena

        public void Skip(int lines)
        {
            while (lines-- > 0)
            {
                if (_sw != null)
                    _sw.WriteLine();
                System.Console.WriteLine();
            }
        }

        public void Write(char ch)
        {
            if (_sw != null)
                _sw.Write(ch);
            System.Console.Write(ch);
        }

        public void Write(string message)
        {
            if (_sw != null)
                _sw.Write(message);
            System.Console.Write(message);
        }
        public void Write(string message, params object[] parms)
        {
            Write(string.Format(message, parms));
            //Skip(1);
        }

        public void WriteLine(string message)
        {
            Write(message);
            Skip(1);
        }
        public void WriteLine(string message, params object[] parms)
        {
            Write(string.Format(message, parms));
            Skip(1);
        }


    }//class Console
}