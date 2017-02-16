using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DataCalculationsLib;

namespace FormatControl
{
    public class MillPlusStringData
    {
        public string _fullString;
        public string currentN;
        public string currentPath;
        public double currentSpin;
        public string currentTool;
        public string settedTool;
        public double currentAngleB;
        public double currentAngleC;
    }
    public class MillPlusDataObjectModel
    {
        private int _maxSpin;
        private decimal _maxB;
        private decimal _minB;
        private string _report;
        private static List<MillPlusStringData> _dom;

        public string Report
        {
            get { return _report; }
            private set { _report = value; }
        }

        public bool ReadFile(string pathToFile,int machineId,out string errMsg)
        {
            //_dom=new List<string>();
            errMsg = "";
            var fi = new FileInfo(pathToFile);
            if (!fi.Exists)
            {
                errMsg = "Файл не найден";
                return false;
            }
            if (fi.Extension != ".pm")
            {
                errMsg = "Файл не является файлом системы ЧПУ MillPlus";
                return false;
            }
            var db = new MillPlusFormatControlDb();
            var machineDb = new MachineDB();

            machineDb.GetMachineParams(machineId, out _minB, out _maxB, out _maxSpin, out errMsg);
            List<int> gFuncAvailable;
            List<int> mFuncAvailable;
            if (!db.GetListOfAvailableGFunc(out gFuncAvailable, out errMsg))
            {
                return false;
            }
            if (!db.GetListOfAvailableMFunc(out mFuncAvailable, out errMsg))
            {
                return false;
            }
            _report = ReportCreator.StartReport(fi.Name);
            foreach (var line in File.ReadLines(pathToFile))
            {
                var v = new MillPlusStringData
                {
                    _fullString = line
                };
                _dom.Add(v);
            }
            if (!Build())
            {
                _report += ReportCreator.EndReport();
                return false;
            }
            else
            {
                if (!Check(gFuncAvailable, mFuncAvailable))
                {
                    _report += ReportCreator.EndReport();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Поиск подстроки в строке с обходом комментариев для системы MillPlus
        /// </summary>
        /// <returns></returns>
        public static int FindSubString(string where,string what)
        {
            var rez = -1;
            var comCnt = 0;
            var whereLen = where.Length;
            var whatLen = what.Length;
            if (whatLen>whereLen)
            {
                return rez;
            }
            for (var i = 0; i < whereLen; i++)
            {
                if (where[i] == '(')
                {
                    comCnt++;
                    while (comCnt!=0 && i<whereLen)
                    {
                        if (where[i]==')')
                        {
                            comCnt--;
                        }
                        if (where[i] == '(')
                        {
                            comCnt++;
                        }
                        i++;
                    }
                }
                else
                {
                    if (where[i]==what[0])
                    {
                        var j = 0;
                        var cnt=i;
                        while (where[cnt]==what[j])
                        {
                            j++;
                            cnt++;
                            if (j>=whatLen)
                            {
                                return i;
                            }
                            if (cnt>=whereLen)
                            {
                                return rez;
                            }
                        }
                    }
                }
            }
            return rez;
        }
        private bool Build()
        {
            var upperBound = _dom.Count;
            int nLen;
            uint n;
            double curSpin = 0;
            double curAngleB = 0;
            double curAngleC = 0;
            string curPath = "Заголовок программы";
            string curN;
            string curTool="";
            string setTool="";
            string localErr;

            for (var i = 0; i < upperBound; i++)
            {
                localErr = ReportCreator.Event("Ошибка формата файла.", _dom[i]._fullString, curPath);
                #region Определение текущего номера кадра

                if (!_dom[i]._fullString.StartsWith("N")) //Все строки должны начинаться с номера кадра
                {
                    _report += localErr;
                    return false;
                }
                nLen = _dom[i]._fullString.IndexOf(" ");

                curN = nLen == -1
                    ? _dom[i]._fullString.Substring(1)
                    : _dom[i]._fullString.Substring(1, nLen);

                try
                {
                    n = Convert.ToUInt32(curN);
                }
                catch (Exception e)
                {
                    _report += ReportCreator.Event("Ошибка формата числа.", i.ToString(), curPath);
                    return false;
                }
                _dom[i].currentN = string.Format("N{0}", n);

                #endregion

                #region Проверка целостнсти комментариев

                var cnt = 0;
                foreach (var str in _dom[i]._fullString)
                {
                    if (str == '(')
                    {
                        cnt++;
                    }
                    if (str == ')')
                    {
                        cnt--;
                    }
                }
                if (cnt != 0)
                {
                    _report += localErr;
                    return false;
                }

                #endregion

                #region Определение текущего PATH

                int pathStart = _dom[i]._fullString.IndexOf("PATH/");
                if (pathStart != -1 && (pathStart + 1) < _dom[i]._fullString.Length)
                {
                    pathStart = _dom[i]._fullString.IndexOf("/", pathStart)+1;
                
                    var pathEnd = _dom[i]._fullString.IndexOf(",", pathStart);
                    if (pathEnd==-1)
                    {
                        _report += localErr;
                        return false;
                    }
                    curPath = _dom[i]._fullString.Substring(pathStart, pathEnd - pathStart);

                }
                #region Поиск намёка на смену инструмента

                if (_dom.Count > i + 1)
                {
                    int toolStart = _dom[i + 1]._fullString.IndexOf("Load");
                    if (toolStart != -1)
                    {
                        toolStart = _dom[i + 1]._fullString.IndexOf("T");
                        if (toolStart != -1)
                        {
                            int toolEnd = _dom[i + 1]._fullString.IndexOf(" ", toolStart);
                            if (toolEnd != -1)
                            {
                                curTool = _dom[i + 1]._fullString.Substring(toolStart, toolEnd - toolStart);
                            }
                        }
                        else
                        {
                            _report += localErr;
                            return false;
                        }
                    }
                }

                #endregion
                _dom[i].currentPath = curPath;
                _dom[i].currentTool = curTool;
                


                #endregion

                #region Определение текущих углов поворота
                
                var nfi = new CultureInfo("ru-RU", true).NumberFormat;
                nfi.NumberDecimalSeparator = ".";
                nfi.CurrencyDecimalSeparator = ".";
                if (-1 != FindSubString(_dom[i]._fullString, "M56") ||
                    -1 != FindSubString(_dom[i]._fullString, "M59") ||
                    -1 != FindSubString(_dom[i]._fullString, "G7 L1=1"))
                {
                    curAngleB = 0;
                    curAngleC = 0;
                }
                int startB = FindSubString(_dom[i]._fullString, " B");
                if (startB!=-1)
                {
                    startB = _dom[i]._fullString.IndexOf("B",startB);
                    int endB = _dom[i]._fullString.IndexOf(" ", startB);
                    string tcurAngleB = endB==-1 ? 
                        _dom[i]._fullString.Substring(startB + 1, _dom[i]._fullString.Length - (startB + 1)) ://true
                        _dom[i]._fullString.Substring(startB + 1, endB - (startB + 1));//false
                    double b;
                    if (double.TryParse(tcurAngleB,NumberStyles.AllowDecimalPoint|NumberStyles.Float,nfi, out b))
                    {
                        curAngleB = b;
                    }
                }
                int startC = FindSubString(_dom[i]._fullString, " C");
                if (startC != -1)
                {
                    startC = _dom[i]._fullString.IndexOf("C",startC);
                    int endC = _dom[i]._fullString.IndexOf(" ", startC);
                    string tcurAngleC = endC==-1 ? 
                        _dom[i]._fullString.Substring(startC + 1, _dom[i]._fullString.Length - (startC + 1)) ://true
                        _dom[i]._fullString.Substring(startC + 1, endC - (startC + 1));//false
                    double c;
                    if (double.TryParse(tcurAngleC, NumberStyles.AllowDecimalPoint | NumberStyles.Float, nfi, out c))
                    {
                        curAngleC = c;
                    }
                }
                _dom[i].currentAngleB = curAngleB;
                _dom[i].currentAngleC = curAngleC;

                #endregion

                #region Определение установленного инструмента


                if (-1 != FindSubString(_dom[i]._fullString, "M6") ||
                    -1 != FindSubString(_dom[i]._fullString, "M06"))
                {
                    int t = FindSubString(_dom[i]._fullString, " T");
                    if (t != -1)
                    {
                        int tEnd = _dom[i]._fullString.IndexOf(" ", t + 2);
                        if (tEnd == -1)
                        {
                            tEnd = _dom[i]._fullString.Length;
                        }
                        var curT = _dom[i]._fullString.Substring(t + 1, tEnd - (t + 1));
                        if (curT=="T0")
                        {
                            curT = "";
                        }
                        setTool = curT;
                    }
                }
                _dom[i].settedTool = setTool;

                #endregion

                #region Определение скорости вращения шпинделя

                var spin = "";
                int m3 = FindSubString(_dom[i]._fullString, "M3");
                int m03 = FindSubString(_dom[i]._fullString, "M03");
                int m30 = FindSubString(_dom[i]._fullString, "M30");
                int m13 = FindSubString(_dom[i]._fullString, "M13");
                int m4 = FindSubString(_dom[i]._fullString, "M4");
                int m14 = FindSubString(_dom[i]._fullString, "M14");

                if ((m3!=-1||
                    m03!=-1||
                    m13!=-1||
                    m4!=-1||
                    m14!=-1)&& 
                    m30==-1)//Поиск всех комманд включения шпинделя
                {
                    int s = FindSubString(_dom[i]._fullString, "S");
                    if (s!=-1)
                    {
                        int sEnd = _dom[i]._fullString.IndexOf(" ", s + 1);
                        if (sEnd==-1)
                        {
                            sEnd = _dom[i]._fullString.Length;
                        }
                         spin = _dom[i]._fullString.Substring(s + 1, sEnd - (s + 1));
                    }
                    double b;
                    if (double.TryParse(spin, NumberStyles.AllowDecimalPoint | NumberStyles.Float, nfi, out b))
                    {
                        curSpin = b;
                    }
                }
                _dom[i].currentSpin = curSpin;
                #endregion
            }
            return true;
        }
        private bool Check(List<int> gFuncAvailable, List<int> mFuncAvailable)
        {
            var mpc = new MillPlusChecker();
            int rez=0;
            
                rez += mpc.CheckGFunctions(_dom, gFuncAvailable, ref _report);

                rez += mpc.CheckMFunctions(_dom, mFuncAvailable, ref _report);

                rez += mpc.CheckBAngle(_dom, _minB, _maxB, ref _report);

                rez += mpc.CheckMaxSpin(_dom,_maxSpin, ref _report);

                rez += mpc.CheckFormatKadr(_dom, ref _report);

                rez += mpc.CheckUnVerticalToolChange(_dom, ref _report);

                rez += mpc.CheckLooseToolChangeCommand(_dom, ref _report);

                rez += mpc.CheckMovigWithoutTool(_dom, ref _report);

                rez += mpc.CheckFlatCircle(_dom, ref _report);

                rez += mpc.CheckNullPoint(_dom, ref _report);

                rez += mpc.CheckMovingWithDeactivatedSpindel(_dom, ref _report);

                rez += mpc.CheckSpinSettedWithoutTool(_dom, ref _report);

                rez += mpc.CheckBlockedSpindelActivated(_dom, ref _report);

                rez += mpc.CheckMeasuringProbeActivated(_dom, ref _report);

                rez += mpc.CheckFeedSetted(_dom, ref _report);

                rez += mpc.CheckMutuallyExclusiveCommands(_dom, ref _report);

                rez += mpc.CheckWrongG7G141Order(_dom, ref _report);

                rez += mpc.CheckCorrection(_dom, ref _report);

            return rez == 0;
        }
      
        public MillPlusDataObjectModel()
        {
            _dom = new List<MillPlusStringData>();
        }

    }
}
