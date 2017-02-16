using System;
using System.Collections.Generic;
using System.Linq;

namespace FormatControl
{
    class MillPlusChecker
    {
        /// <summary>
        /// Функция поиска безусловного перехода
        /// </summary>
        /// <param name="source"></param>
        /// <param name="placeToGo"></param>
        /// <returns></returns>
        private bool UnconditionalTransferSearch(string source, ref string placeToGo)
        {
            bool rez = false;
            int g29 = MillPlusDataObjectModel.FindSubString(source, "G29 ");
            if (g29!=-1)
            {
                int n = source.IndexOf("N=", g29 + 3);
                if (n!=-1)
                {
                    int space = source.IndexOf(" ", n + 2);
                    placeToGo ="N"+source.Substring(n + 2, space - (n + 2));
                    rez = true;
                }
            }
            return rez;
        }
        public int CheckFormatKadr(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            var availableLetters = "ABCDEFGIJKLMNORSTXYZ0123456789-+/.=";
            foreach (var millPlusStringData in dom)
            {
                var upBound = millPlusStringData._fullString.Length;
                for (var i = 0; i < upBound; i++)
                {
                    var c = millPlusStringData._fullString[i];
                    #region SkipComments
                    if (c=='(')
                    {
                        var comCnt = 0;
                        while (comCnt!=0||i<upBound)
                        {
                            c = millPlusStringData._fullString[i];
                            if (c=='(')
                            {
                                comCnt++;
                            }
                            if (c==')')
                            {
                                comCnt--;
                            }
                            i++;
                        }
                    }
                    #endregion
                    if (c != 0x0020 && i < upBound)
                    {
                        if (!availableLetters.Contains(c))
                        {
                            errMsg += ReportCreator.Event("Нарушен формат кадра", millPlusStringData._fullString,
                                millPlusStringData.currentPath);
                            rez++;
                        }
                    }
                }
            }
            return rez;
        }
        public int CheckGFunctions(List<MillPlusStringData> dom,List<int> availableGfunctions,ref string errMsg)
        {
            int rez = 0;
            foreach (var d in dom)
            {
                int gStart = MillPlusDataObjectModel.FindSubString(d._fullString, " G");
                if (gStart != -1)
                {
                    int gEnd = d._fullString.IndexOf(" ", gStart+2);
                    int g;
                    if (gEnd==-1)
                    {
                        if (!int.TryParse(d._fullString.Substring(gStart+2, d._fullString.Length - (gStart+2)), out g))
                        {
                            errMsg += ReportCreator.Event("Задействуется неиспользуемая G функция", d._fullString, d.currentPath);
                            rez++;
                        }
                        if (!availableGfunctions.Contains(g))
                        {
                            errMsg += ReportCreator.Event("Задействуется неиспользуемая G функция", d._fullString, d.currentPath);
                            rez++;
                        }
                    }
                    else
                    {
                        var s = d._fullString.Substring(gStart+2, gEnd - (gStart + 2));
                        if (int.TryParse(s, out g))
                        {
                            if (!availableGfunctions.Contains(g))
                            {
                                errMsg += ReportCreator.Event("Задействуется неиспользуемая G функция", d._fullString, d.currentPath);
                                rez++;
                            }
                        }
                        else
                        {
                            errMsg += ReportCreator.Event("Задействуется неиспользуемая G функция", d._fullString, d.currentPath);
                            rez++;
                        }
                    }
                }
            }
            return rez;
        }
        public int CheckMFunctions(IEnumerable<MillPlusStringData> dom, List<int> availableMfunctions, ref string errMsg)
        {
            int rez = 0;
            foreach (var d in dom)
            {
                int mStart = MillPlusDataObjectModel.FindSubString(d._fullString, " M");
                if (mStart != -1)
                {
                    int mEnd = d._fullString.IndexOf(" ", mStart + 2);
                    int m;
                    if (mEnd == -1)
                    {
                        if (!int.TryParse(d._fullString.Substring(mStart + 2, d._fullString.Length - (mStart + 2)), out m))
                        {
                            errMsg += ReportCreator.Event("Задействуется неиспользуемая М функция", d._fullString, d.currentPath);
                            rez++;
                        }
                        if (!availableMfunctions.Contains(m))
                        {
                            errMsg += ReportCreator.Event("Задействуется неиспользуемая М функция", d._fullString, d.currentPath);
                            rez++;
                        }
                    }
                    else
                    {
                        var s = d._fullString.Substring(mStart + 2, mEnd - (mStart + 2));
                        if (int.TryParse(s, out m))
                        {
                            if (!availableMfunctions.Contains(m))
                            {
                                errMsg += ReportCreator.Event("Задействуется неиспользуемая М функция", d._fullString, d.currentPath);
                                rez++;
                            }
                        }
                        else
                        {
                            errMsg += ReportCreator.Event("Задействуется неиспользуемая М функция", d._fullString, d.currentPath);
                            rez++;
                        }
                    }
                }
            }
            return rez;
        }
        public int CheckUnVerticalToolChange(IEnumerable<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода
                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }
                #endregion
                var m = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString,"M6");
                var m1=MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");
                if (m != -1||m1!=-1)
                {
                        if (Math.Abs(millPlusStringData.currentAngleB) > 0)
                        {
                            rez++;
                            errMsg += ReportCreator.Event("Смена инструмента без вертикальной оси", millPlusStringData._fullString,
                                millPlusStringData.currentPath);
                        }
                }
            }
            return rez;
        }
        public int CheckLooseToolChangeCommand(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ct = dom[0].currentTool;
            int upperbound = dom.Count;
            for (int i = 0; i < upperbound; i++)
            {
                if (ct != dom[i].currentTool)
                {
                    for (int j = i; j < upperbound; j++)
                    {
                        int g79 = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, "G79 ");
                        int g1 = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, "G1 ");
                        int g2 = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, "G2 ");
                        int g3 = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, "G3 ");
                        int m6 = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, "M6");
                        int m06 = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, "M06");

                        if (m6!=-1||m06!=-1)
                        {
                            int t = MillPlusDataObjectModel.FindSubString(dom[j]._fullString, " T");
                            if (t!=-1)
                            {
                                int tEnd=dom[j]._fullString.IndexOf(" ",t+2);
                                if (tEnd==-1)
                                {
                                    tEnd = dom[j]._fullString.Length;
                                }
                                var curT = dom[j]._fullString.Substring(t + 1,tEnd-(t+1));
                                if (curT==dom[j].currentTool)
                                {
                                    ct = dom[j].currentTool;
                                    break;
                                }
                                else
                                {
                                    rez++;
                                    errMsg += ReportCreator.Event("Отсутствие необходимой команды смены инструмента",
                                        dom[j]._fullString, dom[j].currentPath);
                                }
                            }
                        }
                        if (g79!=-1||g1!=-1||g2!=-1||g3!=-1)
                        {
                            rez++;
                            errMsg += ReportCreator.Event("Отсутствие необходимой команды смены инструмента",
                                dom[j]._fullString, dom[j].currentPath);
                        }
                        if (rez > 0)
                        {
                            return rez;
                        }
                    }
                }
            }
            return rez;
        }
        public int CheckMovigWithoutTool(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода
                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }
                #endregion
                if (millPlusStringData.settedTool == "")
                {
                    int g1 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G1 ");
                    int g2 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G2 ");
                    int g3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G3 ");
                    if (g1 != -1 || g2 != -1 || g3 != -1)
                    {
                        rez++;
                        errMsg += ReportCreator.Event("Рабочие перемещения без установленного инструмента",
                            millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
                if (rez > 0)
                {
                    return rez;
                }
            }
            return rez;
        }
        public int CheckFlatCircle(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода
                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }
                #endregion
                    int g2 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G2 ");
                    int g3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G3 ");
                    if (g2 != -1 || g3 != -1)
                    {
                        if (Math.Abs(millPlusStringData.currentAngleB) > 0 || Math.Abs(millPlusStringData.currentAngleC) > 0)
                        {
                            rez++;
                            errMsg += ReportCreator.Event("G2 или G3 не в плоскости",
                                millPlusStringData._fullString, millPlusStringData.currentPath);
                        }
                        
                    }
            }
            return rez;
        }
        public int CheckSpinSettedWithoutTool(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool spindelActivatedFlag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int m3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M3");
                int m03 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M03");
                int m30 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M30");
                int m13 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M13");
                int m4 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M4");
                int m14 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M14");
                var m6 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M6");
                var m06 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");
                var m0 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M0");

                if ((m3 != -1 || m03 != -1 || m13 != -1 || m14 != -1 || m4 != -1) && m30 == -1)
                {
                    spindelActivatedFlag = true;
                }
                if (m06 != -1 || m6 != -1 || m0 != -1)
                {
                    spindelActivatedFlag = false;
                }
                if (spindelActivatedFlag &&
                    millPlusStringData.settedTool == "")
                {
                    rez++;
                    errMsg += ReportCreator.Event("Включение шпинделя без установленного инструмента",
                        millPlusStringData._fullString, millPlusStringData.currentPath);
                }
                if (rez > 0)
                {
                    return rez;
                }
            }

            return rez;
        }
        public int CheckNullPoint(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool g54Flag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int g54 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G54");
                int g53 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G53");
                int g1 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G1 ");
                int g2 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G2 ");
                int g3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G3 ");
                if (g54 != -1)
                {
                    g54Flag = true;
                }
                if (g53 != -1)
                {
                    g54Flag = false;
                }
                if ((g1 != -1 || g2 != -1 || g3 != -1)&&!g54Flag)
                {
                    rez++;
                    errMsg += ReportCreator.Event("Не задана нулевая точка",
                        millPlusStringData._fullString, millPlusStringData.currentPath);
                }
                if (rez > 0)
                {
                    return rez;
                }
            }

            return rez;
        }
        public int CheckMovingWithDeactivatedSpindel(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool m51Flag = false;
            bool spindelActivatedFlag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int m51 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M51");
                int m52 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M52");
                int g1 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G1 ");
                int g2 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G2 ");
                int g3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G3 ");
                int m3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M3");
                int m03 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M03");
                int m30 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M30");
                int m13 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M13");
                int m4 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M4");
                int m14 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M14");
                var m6 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M6");
                var m06 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");
                var m0 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M0");

                if ((m3!=-1||m03!=-1||m13!=-1||m14!=-1||m4!=-1)&& m30==-1)
                {
                    spindelActivatedFlag = true;
                }
                if (m06!=-1||m6!=-1||m51!=-1||m0!=-1)
                {
                    spindelActivatedFlag = false;
                }
                if (m51 != -1)
                {
                    m51Flag = true;
                }
                if (m52 != -1)
                {
                    m51Flag = false;
                }
                if (!spindelActivatedFlag)
                {
                    if (!m51Flag)
                    {
                        if ((g1 != -1 || g2 != -1 || g3 != -1))
                        {
                            rez++;
                            errMsg += ReportCreator.Event("Рабочее перемещение с выключеным шпинделем.",
                                millPlusStringData._fullString, millPlusStringData.currentPath);
                        }
                    }
                }
                if (rez > 0)
                {
                    return rez;
                }
            }

            return rez;
        }
        public int CheckBlockedSpindelActivated(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool m51Flag = false;
            bool spindelActivatedFlag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int m51 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M51");
                int m52 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M52");
                int m3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M3");
                int m03 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M03");
                int m30 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M30");
                int m13 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M13");
                int m4 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M4");
                int m14 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M14");
                var m6 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M6");
                var m06 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");
                var m0 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M0");

                if ((m3 != -1 || m03 != -1 || m13 != -1 || m14 != -1 || m4 != -1) && m30 == -1)
                {
                    spindelActivatedFlag = true;
                }
                if (m06 != -1 || m6 != -1 || m51 != -1 || m0 != -1)
                {
                    spindelActivatedFlag = false;
                }
                if (m51 != -1)
                {
                    m51Flag = true;
                }
                if (m52 != -1)
                {
                    m51Flag = false;
                }
                if (m51Flag)
                {
                    if (spindelActivatedFlag)
                    {
                        rez++;
                            errMsg += ReportCreator.Event("Попытка включения заблокированного шпинделя.",
                                millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
                if (rez > 0)
                {
                    return rez;
                }
            }

            return rez;
        }
        public int CheckMeasuringProbeActivated(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool m27Flag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int m27 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M27");
                int m28 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M28");
                int g621 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G621");
                if (m27!=-1)
                {
                    m27Flag = true;
                } 
                if (m28 != -1)
                {
                    m27Flag = false;
                }
                if (g621!=-1)
                {
                    if (!m27Flag)
                    {
                        rez++;
                        errMsg += ReportCreator.Event("Измерение без активации измерительного щупа.",
                            millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
            }

            return rez;
        }
        public int CheckFeedSetted(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool feedFlag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int g1 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G1 ");
                int g2 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G2 ");
                int g3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G3 ");
                int f = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "F");
                if (f!=-1)
                {
                    feedFlag = true;
                }
                if (!feedFlag)
                {
                    if (g1!=-1||g2!=-1||g3!=-1)
                    {
                        rez++;
                        errMsg += ReportCreator.Event("Не задана рабочая подача.",
                            millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
                if (rez > 0)
                {
                    return rez;
                }
            }
            return rez;
        }
        public int CheckMutuallyExclusiveCommands(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool g141Flag = false;
            bool g41Flag = false;
            bool g42Flag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int g141 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G141");
                int g41 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G41");
                int g42 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G42");
                int g40 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G40");
                int m6 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M6");
                int m06 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");
                if (g141!=-1)
                {
                    g141Flag = true;
                }
                if (g41 != -1)
                {
                    g41Flag = true;
                }
                if (g42 != -1)
                {
                    g42Flag = true;
                }
                if (g40!=-1||m6!=-1||m06!=-1)
                {
                    g41Flag = false;
                    g42Flag = false;
                    g141Flag = false;
                }
                if (g41 != -1 || g42 != -1 || g141 != -1)
                {
                    if ((g141Flag && (g41Flag || g42Flag)) || (g41Flag && g42Flag))
                    {
                        rez++;
                        errMsg += ReportCreator.Event("Использование взаимоисключающих функций (G41,42,141).",
                            millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
            }

            return rez;
        }
        public int CheckWrongG7G141Order(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool g141Flag = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int g141 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G141");
                int g40 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G40");
                int g7 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G7");
                int m6 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M6");
                int m06 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");

                if (g141 != -1)
                {
                    g141Flag = true;
                }
                if (g40 != -1 || m6 != -1 || m06 != -1)
                {
                    g141Flag = false;
                }
                if (g7!= -1)
                {
                    if (g141Flag)
                    {
                        rez++;
                        errMsg += ReportCreator.Event("Неправильный порядок функций G141, G7.",
                            millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
            }
            return rez;
        }
        public int CheckCorrection(List<MillPlusStringData> dom, ref string errMsg)
        {
            int rez = 0;
            string ptg = "";
            bool g41Flag = false;
            bool g42Flag = false;
            string curX="";
            string curY="";
            string lastX = "";
            string lastY = "";
            bool movedByG1 = false;
            foreach (var millPlusStringData in dom)
            {
                #region проверка безусловного перехода

                UnconditionalTransferSearch(millPlusStringData._fullString, ref ptg);
                if (ptg != "")
                {
                    if (millPlusStringData.currentN != ptg)
                    {
                        continue;
                    }
                    else
                    {
                        ptg = "";
                    }
                }

                #endregion
                int g41 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G41");
                int g42 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G42");
                int g40 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G40");
                int m6 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M6");
                int m06 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "M06");
                int x = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "X");
                int y = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "Y");
                int g1 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G1 ");
                int g2 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G2 ");
                int g3 = MillPlusDataObjectModel.FindSubString(millPlusStringData._fullString, "G3 ");
                if (g41 != -1)
                {
                    g41Flag = true;
                }
                if (g42 != -1)
                {
                    g42Flag = true;
                }
                if (g40 != -1 || m6 != -1 || m06 != -1)
                {
                    g41Flag = false;
                    g42Flag = false;
                    movedByG1 = false;
                }
                #region Текущий Х
                
                
                if (x!=-1)
                {
                    int endX=millPlusStringData._fullString.IndexOf(" ", x);
                    if (endX==-1)
                    {
                        endX = millPlusStringData._fullString.Length;
                    }
                    lastX = curX;
                    curX = millPlusStringData._fullString.Substring(x + 1, endX - (x + 1));

                }
                #endregion
                #region Текущий У
                if (y != -1)
                {
                    int endY = millPlusStringData._fullString.IndexOf(" ", y);
                    if (endY == -1)
                    {
                        endY = millPlusStringData._fullString.Length;
                    }
                    lastY = curY;
                    curY = millPlusStringData._fullString.Substring(y + 1, endY - (y + 1));
                }
                #endregion

                if (g41Flag || g42Flag)
                {
                    if (g1!=-1)
                    {
                        if (lastX!=curX||lastY!=curY)
                        {
                            movedByG1 = true;    
                        }
                    }
                }
                if ((g2!=-1 || g3!=-1) && (g41Flag || g42Flag))
                {
                    if (!movedByG1)
                    {
                        rez++;
                        errMsg += ReportCreator.Event("Включение корректора, без линейного перемещения.",
                            millPlusStringData._fullString, millPlusStringData.currentPath);
                    }
                }
                if (rez > 10)
                {
                    return rez;
                }
            }
            return rez;
        }
        public int CheckMaxSpin(List<MillPlusStringData> dom,int maxSpin, ref string errMsg)
        {
            int rez = 0;
            foreach (var millPlusStringData in dom)
            {
                if (millPlusStringData.currentSpin > maxSpin)
                {
                    rez++;
                    errMsg += ReportCreator.Event("Превышена максимальная скорость вращения шпинделя для станка.",
                        millPlusStringData._fullString, millPlusStringData.currentPath);
                }
                if (rez > 1)
                {
                    return rez;
                }
            }
            return rez;
        }
        public int CheckBAngle(List<MillPlusStringData> dom,decimal minB,decimal maxB, ref string errMsg)
        {
            int rez = 0;
            foreach (var millPlusStringData in dom)
            {
                if (millPlusStringData.currentAngleB < (double) minB || 
                    millPlusStringData.currentAngleB > (double) maxB)
                {
                    rez++;
                    errMsg += ReportCreator.Event("Превышен максимальный угол по оси \"B\" для станка.",
                        millPlusStringData._fullString, millPlusStringData.currentPath);
                }
                if (rez > 1)
                {
                    return rez;
                }
            }
            return rez;
        }
    }
}
