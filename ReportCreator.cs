namespace FormatControl
{
    public static class ReportCreator
    {
        public static string StartReport(string fileName)
        {
            var rez = string.Format("<html>\r\n" +
                                    "<HEAD>" +
                                        "<title>Отчёт</title>" +
                                        "<META http-equiv=Content-Type content=\"text/html; charset=windows-1251\">\r\n" +
                                        "<body BGCOLOR=\"slategray\">" +
                                    "</HEAD>\r\n" +
                                    "<center>\r\n" +
                                    "<h1>{0}</h1>\r\n", fileName);
            rez +="<h1>Предупреждения</h1>\r\n" +
                  "<Table cellpadding=\"1\" cellspacing=\"0\" BORDER=1 align=center BGCOLOR=\"#9fa9b3\" width=\"100%\">\r\n";
            rez += "<TR BGCOLOR=\"silver\"><TH>Событие</TH><TH>Kадр</TH><TH>PATH</TH></TR>";
            return rez;
        }
        public static string Event(string eventText,string currentN,string currentPath,bool isError=true)
        {
            var color = isError ? "FFFF99" : "FF9999";
            var rez = string.Format("<TR bgcolor=\"{0}\"><TD>{1}</TD><TD>{2}</TD><TD>{3}</TD></TR>\r\n",color,eventText,currentN,currentPath);
            return rez;
        }
        public static string EndReport()
        {
            return "</TABLE>\r\n</center>\r\n </body>\r\n</html>";
        }
    }
}