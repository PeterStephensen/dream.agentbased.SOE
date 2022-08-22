using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Globalization;
using System.IO;
using System.Data.Odbc;
using System.Data;


namespace Dream.IO
{

    #region enum TabFileOption
    public enum TabFileOption
    {
        /// <summary>Calculate the number of obsevations in the file. </summary>		
        CalcNumberOfObs = 1,
        /// <summary>Transpose file before reading</summary>
        Transpose = 2
        //Option3=4,
        //Option4=8,
        //Option5=16,	
        //Option6=32,
    }
    #endregion
    /// <summary>
    /// Implements a DREAM.IO.TabFileReader
    /// </summary>
    public class TabFileReader
    {

        #region Constants
        const string TMP_FILE = "~TabFileReader.tmp";
        #endregion

        #region Public fields
        public string Missing = "";
        public CultureInfo CultureInfo;
        public int MaxScanRows = 1000;
        #endregion

        #region Private fields
        StreamReader _file;
        string[] _keyword;
        string[] _element;
        string _line, _firstline, _tmpStr;
        int _nLines = 0;
        string _fileName;

        int _maxGrpSize = 100;
        int _groupSize = 0, _next_group = 0, _nGroup = 0;
        string _group;
        string[] _last_element;
        string _last_line;
        string[][] _group_element;
        string[] _group_line;
        Hashtable _hashTable;

        #region Missing Values
        Int64 _missingInt64 = Int64.MinValue;
        Int32 _missingInt32 = Int32.MinValue;
        Int16 _missingInt16 = Int16.MinValue;
        Byte _missingByte = Byte.MinValue;
        SByte _missingSByte = SByte.MinValue;
        double _missingDouble = Double.MinValue;
        char _missingChar = (char)0;
        string _missingString = "";
        #endregion

        #endregion

        #region Constructors
        public TabFileReader(string FileName) : this(FileName, 0) { }

        public TabFileReader(string FileName, TabFileOption flags)
        {

            this.CultureInfo = CultureInfo.CurrentCulture;

            _hashTable = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());
            _fileName = FileName;

            if (isFlag(flags, TabFileOption.Transpose))
            {
                _fileName = System.Environment.GetEnvironmentVariable("TEMP") + TMP_FILE;
                Transpose(FileName, _fileName);
            }

            try
            {
                _file = new StreamReader(_fileName);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
            // Calc number of lines
            _nLines = 0;
            if (isFlag(flags, TabFileOption.CalcNumberOfObs))
            {
                while (_file.ReadLine() != null) _nLines++;
                _file.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            // Read 1. line and parse it for keywords
            _firstline = _file.ReadLine();
            while (_firstline[0] == '*') _firstline = _file.ReadLine();   // Allow for comments (starting with *)
            _keyword = _firstline.Split('\t');
            for (int i = 0; i < _keyword.Length; i++) _hashTable[_keyword[i]] = i;

        }


        #region Old-style

        //public TabFileReader(string FileName) : this(FileName, true, false){}

        // Old-style
        public TabFileReader(string FileName, bool bCalcNumOfObs) : this(FileName, bCalcNumOfObs, false) { }

        // Old-style
        public TabFileReader(string FileName, bool bCalcNumOfObs, bool bTransposed)
        {
            this.CultureInfo = CultureInfo.CurrentCulture;

            _hashTable = new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer());
            _fileName = FileName;

            if (bTransposed)
            {
                _fileName = System.Environment.GetEnvironmentVariable("TEMP") + TMP_FILE;
                Transpose(FileName, _fileName);
            }

            try
            {
                _file = new StreamReader(_fileName);
            }
            catch { throw; }
            // Calc number of lines
            _nLines = 0;
            if (bCalcNumOfObs)
            {
                while (_file.ReadLine() != null) _nLines++;
                _file.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            // Read 1. line and parse it for keywords
            _firstline = _file.ReadLine();
            while (_firstline[0] == '*') _firstline = _file.ReadLine();   // Allow for comments (starting with *)
            _keyword = _firstline.Split('\t');
            for (int i = 0; i < _keyword.Length; i++) _hashTable[_keyword[i]] = i;
        }
        #endregion
        #endregion

        #region Public Methods
        #region Group-related methods

        /// <summary>
        /// Set the name of the keyword that ReadGroup() use
        /// </summary>
        /// <param name="keyword">A valid keyword</param>
        public void SetGroup(string keyword)
        {
            _group_element = new String[_maxGrpSize][];
            _group_line = new String[_maxGrpSize];
            _group = keyword;
            //_useGroup = true;
        }

        /// <summary>
        ///	Read lines as long as keyword defined by SetGroup is unchanged	 
        /// </summary>
        /// <returns>False if End Of File</returns>
        public bool ReadGroup()
        {
            if (_next_group == -1)
            {
                _next_group = 0;
                return false;
            }

            int g = 0, g0 = _next_group;

            if (_nGroup == 0)  // First time
            {
                this.ReadLine();
                this.Get(_group, out g0);
                _group_element[0] = _element;
                _group_line[0] = _line;
            }
            else
            {
                _group_element[0] = _last_element;
                _group_line[0] = _last_line;
            }


            _groupSize = 1;
            _nGroup++;
            while (this.ReadLine())
            {
                this.Get(_group, out g);

                if (g == g0)
                {
                    _group_element[_groupSize] = _element;
                    _group_line[_groupSize] = _line;
                }
                else
                {
                    _last_element = _element;
                    _last_line = _line;
                    _element = _group_element[0];  // Dette sikrer at scalar-Get kan bruges
                    _line = _group_line[0];  // Dette sikrer at scalar-Get kan bruges
                    _next_group = g;
                    return true;
                }

                _groupSize++;
                if (_groupSize == _maxGrpSize) throw new ArgumentOutOfRangeException("Group too big. _maxGroupSize=" + _maxGrpSize.ToString());
                g0 = g;
            }

            // EOF
            _element = _group_element[0];  // Dette sikrer at scalar-Get kan bruges
            _line = _group_line[0];  // Dette sikrer at scalar-Get kan bruges
            _next_group = -1;
            return true;
            //return false;
        }
        #endregion

        #region ReadLine(), Rewind(), Close()
        /// <summary>
        /// Reads and parses the next line
        /// </summary>
        public bool ReadLine()
        {

            _line = _file.ReadLine();
            if (_line == null) return false;

            _element = _line.Split('\t');

            return true;
        }

        /// <summary>
        /// Rewind the file to the beginning
        /// </summary>
        public void Rewind()
        {
            _file.BaseStream.Seek(0, SeekOrigin.Begin);
            _file.ReadLine();
        }

        /// <summary>
        /// Close the file
        /// </summary>
        public void Close()
        {
            FileInfo fi = new FileInfo(_fileName);
            FileInfo fi1 = new FileInfo(fi.DirectoryName + "\\schema.ini");
            if (fi1.Exists) fi1.Delete();

            if (_file != null) _file.Close();
        }



        #endregion
        #region Get...
        #region GetHandle
        public int GetHandle(string keyword)
        {
            object o = _hashTable[keyword];
            if (o == null) throw new ArgumentException(keyword + " is unknown keyword.");

            return (int)o;

        }
        #endregion

        #region GetString
        public string GetString(string keyword)
        {
            return _element[GetHandle(keyword)];
        }

        public string GetString(int handle)
        {
            return _element[handle];
        }
        #endregion

        #region Get()
        #region Scalar
        public void Get(string keyword, out string ret)
        {
            ret = _element[GetHandle(keyword)];
        }

        public void Get(string keyword, out char ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? _element[j].ToCharArray()[0] : MissingChar;
        }

        public void Get(string keyword, out byte ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? Convert.ToByte(_element[j], this.CultureInfo) : MissingByte;
        }

        public void Get(string keyword, out sbyte ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? Convert.ToSByte(_element[j], this.CultureInfo) : MissingSByte;
        }

        public void Get(string keyword, out int ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? Convert.ToInt32(_element[j], this.CultureInfo) : MissingInt32;
        }
        public void Get(string keyword, out Int64 ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? Convert.ToInt64(_element[j], this.CultureInfo) : MissingInt64;
        }

        public void Get(string keyword, out Int16 ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? Convert.ToInt16(_element[j], this.CultureInfo) : MissingInt16;
        }

        public void Get(string keyword, out double ret)
        {
            int j = GetHandle(keyword);
            ret = (_element[j] != "") ? Convert.ToDouble(_element[j], this.CultureInfo) : MissingDouble;
        }
        #endregion

        #region Array
        public void Get(string keyword, string[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = _group_element[i][j];
        }

        public void Get(string keyword, char[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = _group_element[i][j].ToCharArray()[0];
        }

        public void Get(string keyword, byte[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = (_group_element[i][j] != "") ? Convert.ToByte(_group_element[i][j], this.CultureInfo) : MissingByte;
        }

        public void Get(string keyword, sbyte[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = (_group_element[i][j] != "") ? Convert.ToSByte(_group_element[i][j], this.CultureInfo) : MissingSByte;
        }

        public void Get(string keyword, int[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = (_group_element[i][j] != "") ? Convert.ToInt32(_group_element[i][j], this.CultureInfo) : MissingInt32;


        }
        public void Get(string keyword, Int64[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
            {
                ret[i] = (_group_element[i][j] != "") ? Convert.ToInt64(_group_element[i][j], this.CultureInfo) : MissingInt64;
            }

        }

        public void Get(string keyword, Int16[] ret)
        {
            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = (_group_element[i][j] != "") ? Convert.ToInt16(_group_element[i][j], this.CultureInfo) : MissingInt16;
        }

        public void Get(string keyword, double[] ret)
        {

            int j = GetHandle(keyword);
            for (int i = 0; i < _groupSize; i++)
                ret[i] = (_group_element[i][j] != "") ? Convert.ToDouble(_group_element[i][j], this.CultureInfo) : MissingDouble;
        }

        #endregion
        #endregion

        #region GetChar
        public char GetChar(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim().Length == 0) return ' ';

            return s.Trim().ToCharArray()[0];
        }

        public char GetChar(int handle)
        {
            string s = GetString(handle);
            if (s.Trim().Length == 0) return ' ';

            return s.Trim().ToCharArray()[0];
        }
        #endregion

        #region GetByte
        public byte GetByte(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim() == "") return _missingByte;

            return Convert.ToByte(s, this.CultureInfo);
        }

        public byte GetByte(int handle)
        {
            string s = GetString(handle);
            if (s.Trim() == "") return _missingByte;

            return Convert.ToByte(s, this.CultureInfo);
        }
        #endregion

        #region GetSByte
        public sbyte GetSByte(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim() == "") return _missingSByte;

            return Convert.ToSByte(s, this.CultureInfo);
        }

        public sbyte GetSByte(int handle)
        {
            string s = GetString(handle);
            if (s.Trim() == "") return _missingSByte;

            return Convert.ToSByte(s, this.CultureInfo);
        }
        #endregion

        #region GetInd64
        public Int64 GetInt64(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim() == "") return _missingInt64;

            return Convert.ToInt64(s, this.CultureInfo);
        }

        public Int64 GetInt64(int handle)
        {
            string s = GetString(handle);
            if (s.Trim() == "") return _missingInt64;

            return Convert.ToInt64(s, this.CultureInfo);
        }
        #endregion

        #region GetInd32
        public int GetInt32(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim() == "") return _missingInt32;

            return Convert.ToInt32(s, this.CultureInfo);
        }

        public int GetInt32(int handle)
        {
            string s = GetString(handle);
            if (s.Trim() == "") return _missingInt32;

            return Convert.ToInt32(s, this.CultureInfo);
        }
        #endregion

        #region GetInt16
        public Int16 GetInt16(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim() == "") return _missingInt16;

            return Convert.ToInt16(s, this.CultureInfo);
        }

        public Int16 GetInt16(int handle)
        {
            string s = GetString(handle);
            if (s.Trim() == "") return _missingInt16;

            return Convert.ToInt16(s, this.CultureInfo);
        }
        #endregion

        #region GetDouble
        public double GetDouble(string keyword)
        {
            string s = GetString(keyword);
            if (s.Trim() == "") return _missingDouble;

            return Convert.ToDouble(s, this.CultureInfo);
        }

        public double GetDouble(int handle)
        {
            string s = GetString(handle);
            if (s.Trim() == "") return _missingDouble;

            return Convert.ToDouble(s, this.CultureInfo);
        }
        #endregion

        #endregion

        #region GetLine()
        public string GetLine(int index)
        {
            if (index < this.GroupSize)
            {
                return _group_line[index];
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Sort()
        public static void Sort(string Tabfile, string SortedTabFile, string SortVar)
        {
            Sort(Tabfile, SortedTabFile, SortVar, typeof(Int64));
        }

        public static void Sort(string Tabfile, string SortedTabFile, string SortVar, Type type)
        {
            TabFileReader tab;
            StreamWriter file;
            StreamReader test;
            string sortVar = SortVar.ToLower();

            test = new StreamReader(Tabfile);

            int n = 0;
            while (test.ReadLine() != null) n++; n--;
            test.Close();

            tab = new TabFileReader(Tabfile);
            file = new StreamWriter(SortedTabFile);

            string[] line = new string[n];
            Array index = Array.CreateInstance(type, n);

            int j = 0;
            while (tab.ReadLine())
            {
                line[j] = tab.Line;
                if (type == typeof(Int64))
                    index.SetValue(tab.GetInt64(sortVar), j);
                else if (type == typeof(Int32))
                    index.SetValue(tab.GetInt32(sortVar), j);
                else if (type == typeof(Int16))
                    index.SetValue(tab.GetInt16(sortVar), j);
                else if (type == typeof(double))
                    index.SetValue(tab.GetDouble(sortVar), j);
                else if (type == typeof(byte))
                    index.SetValue(tab.GetByte(sortVar), j);
                else if (type == typeof(sbyte))
                    index.SetValue(tab.GetSByte(sortVar), j);
                else if (type == typeof(string))
                    index.SetValue(tab.GetString(sortVar), j);
                else
                    throw new Exception("The type " + type.ToString() + " is unknown for TabFileReader.Sort");

                j++;
            }

            Array.Sort(index, line);

            file.WriteLine(tab.FirstLine);

            for (int i = 0; i < n; i++) file.WriteLine(line[i]);

            tab.Close();
            file.Close();

        }
        #endregion

        #region Transpose()
        public static void Transpose(string Tabfile1)
        {
            Transpose(Tabfile1, Tabfile1);
        }
        public static void Transpose(string Tabfile1, string TransposedTabfile)
        {
            TabFileReader tab1;
            try
            {
                tab1 = new TabFileReader(Tabfile1);
            }
            catch { throw; }
            string[] Columns = new string[tab1.Keywords.Length];
            tab1.Keywords.CopyTo(Columns, 0);
            bool loop = tab1.ReadLine();
            while (loop)
            {
                for (int i = 0; i < Columns.Length; i++)
                {
                    Columns[i] = Columns[i] + "\t" + tab1.Elements[i].ToString();
                }
                loop = tab1.ReadLine();
            }
            tab1.Close();

            StreamWriter file;
            try
            {
                file = new StreamWriter(TransposedTabfile);
            }
            catch { throw; }
            for (int i = 0; i < Columns.Length; i++)
            {
                file.WriteLine(Columns[i]);
            }
            file.Close();
        }
        #endregion

        #region Merge()
        public static void Merge(string Tabfile1, string Tabfile2, string MergedTabFile, string MergeVar)
        {
            TabFileReader tab1, tab2;
            StreamWriter file;
            string mergeVar = MergeVar.ToUpper();

            try
            {
                tab1 = new TabFileReader(Tabfile1);
                tab2 = new TabFileReader(Tabfile2);
                file = new StreamWriter(MergedTabFile);
            }
            catch { throw; }


            string varline1 = tab1.FirstLine;
            string varline2 = tab2.FirstLine;
            int i1 = -1, i2 = -1;

            for (int i = 0; i < tab1._keyword.Length; i++)
            {
                if (mergeVar == tab1._keyword[i])
                {
                    i1 = i;
                    break;
                }
            }

            for (int i = 0; i < tab2._keyword.Length; i++)
            {
                if (mergeVar == tab2._keyword[i])
                {
                    i2 = i;
                    break;
                }
            }

            if ((i1 == -1) || (i2 == -1))
            {
                throw new Exception("Both tab-files should include the variable " + MergeVar);
            }

            // Skriv 1. linie
            for (int i = 0; i < tab1._keyword.Length; i++) file.Write("{0}\t", tab1._keyword[i]);
            for (int i = 0; i < i2; i++) file.Write("{0}\t", tab2._keyword[i]);
            for (int i = i2 + 1; i < tab2._keyword.Length - 1; i++) file.Write("{0}\t", tab2._keyword[i]);
            if (i2 != tab2._keyword.Length - 1) file.Write(tab2._keyword[tab2._keyword.Length - 1]);
            file.Write("\n");

            int index1, index2, z;
            bool loop = true;

            tab1.ReadLine();
            index1 = tab1.GetInt32(mergeVar);

            tab2.ReadLine();
            index2 = tab2.GetInt32(mergeVar);

            while (loop)
            {
                if (index1 <= index2)
                {
                    if (index1 == index2)
                    {
                        for (int i = 0; i < tab1._element.Length; i++) file.Write("{0}\t", tab1._element[i]);
                        for (int i = 0; i < i2; i++) file.Write("{0}\t", tab2._element[i]);
                        for (int i = i2 + 1; i < tab2._element.Length - 1; i++) file.Write("{0}\t", tab2._element[i]);
                        if (i2 != tab2._element.Length - 1) file.Write(tab2._element[tab2._element.Length - 1]);
                        file.Write("\n");

                    }

                    loop = tab1.ReadLine();
                    if (loop)
                    {
                        z = tab1.GetInt32(mergeVar);
                        if (z <= index1) throw new Exception("The tab-file '" + Tabfile1 + "' is not sorted with respect to merge-variable " + MergeVar);
                        index1 = z;
                    }
                }
                else
                {
                    loop = tab2.ReadLine();
                    if (loop)
                    {
                        z = tab2.GetInt32(mergeVar);
                        Console.WriteLine("{0},{1}", index2, z);
                        if (z <= index2) throw new Exception("The tab-file '" + Tabfile2 + "' is not sorted with respect to merge-variable " + MergeVar);
                        index2 = z;
                    }
                }
            }

            tab1.Close();
            tab2.Close();
            file.Close();

        }
        #endregion

        #region Tab() og String()
        public string Tab(params object[] o)
        {

            string[] s = new string[o.Length];

            for (int i = 0; i < o.Length; i++)
            {
                if (o[i].GetType() == typeof(int)) s[i] = ((int)o[i] == _missingInt32) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(double)) s[i] = ((double)o[i] == _missingDouble) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(string)) s[i] = ((string)o[i] == _missingString) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(char)) s[i] = ((char)o[i] == _missingChar) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(byte)) s[i] = ((byte)o[i] == _missingByte) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(sbyte)) s[i] = ((sbyte)o[i] == _missingSByte) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(Int16)) s[i] = ((Int16)o[i] == _missingInt16) ? "" : o[i].ToString();
                else if (o[i].GetType() == typeof(Int64)) s[i] = ((Int64)o[i] == _missingInt64) ? "" : o[i].ToString();
            }

            string ret = s[0];
            for (int i = 1; i < s.Length; i++) ret = ret + "\t" + s[i];

            return ret;

        }

        public string String(params int[] x)
        {
            string s = (x[0] == _missingInt32) ? Missing : x[0].ToString();
            for (int i = 1; i < x.Length; i++) s = s + "\t" + ((x[i] == _missingInt32) ? Missing : x[i].ToString());
            return s;
        }

        public string String(params double[] x)
        {
            string s = (x[0] == _missingDouble) ? this.Missing : x[0].ToString();
            for (int i = 1; i < x.Length; i++) s = s + "\t" + ((x[i] == _missingDouble) ? Missing : x[i].ToString());
            return s;
        }



        #endregion

        #region GetDataTable

        public DataTable GetDataTable()
        {
            Type[] type = new Type[_keyword.Length];
            for (int i = 0; i < type.Length; i++) type[i] = typeof(System.String);

            return GetDataTable(type, "");

        }

        public DataTable GetDataTable(string sqlSelect)
        {
            Type[] type = new Type[_keyword.Length];
            for (int i = 0; i < type.Length; i++) type[i] = typeof(System.String);

            return GetDataTable(type, sqlSelect);

        }

        public DataTable GetDataTable(Type t, string sqlSelect)
        {
            Type[] type = new Type[_keyword.Length];
            for (int i = 0; i < type.Length; i++) type[i] = t;

            return GetDataTable(type, sqlSelect);

        }

        //Problem med System.Data.Odbc
        public DataTable GetDataTable(Type[] type, string sqlSelect)
        {

            return null;

            //    _file.Close();

            //    FileInfo fi = new FileInfo(_fileName);
            //    StreamWriter schema = new StreamWriter(fi.DirectoryName + "\\schema.ini");

            //    schema.WriteLine("[" + fi.Name + "]");
            //    schema.WriteLine("Format=TabDelimited");
            //    schema.WriteLine("ColNameHeader=True");
            //    schema.WriteLine("MaxScanRows=" + MaxScanRows.ToString());
            //    schema.WriteLine();

            //    schema.Close();

            //    string connString = @"Driver={Microsoft Text Driver (*.txt; *.csv)};DBQ=" + @fi.DirectoryName;

            //    OdbcConnection conn = new OdbcConnection(connString);

            //    conn.Open();

            //    OdbcDataAdapter da;

            //    if (sqlSelect == "")
            //        da = new OdbcDataAdapter("SELECT * FROM " + fi.Name, conn);
            //    else
            //        da = new OdbcDataAdapter(sqlSelect, conn);

            //    DataSet ds;
            //    if (da != null)
            //    {
            //        ds = new DataSet();
            //        da.Fill(ds, "TabFileReaderTable");

            //    }
            //    else
            //        throw new Exception("Error creating OdbcDataAdapter in TabFileReader");

            //    conn.Close();

            //    return ds.Tables[0];

        }

        #endregion
        #endregion

        #region Private Methods
        // Helper-functions
        #region getIndex
        int getindex(string keyword)
        {
            string kw = keyword.ToUpper();
            for (int i = 0; i < _keyword.Length; i++)
            {
                if (kw == _keyword[i]) return i;
            }

            throw new InvalidOperationException(keyword + " is unknown keyword.");
        }
        #endregion

        #region isFlag
        bool isFlag(TabFileOption all, TabFileOption one)
        {
            return ((int)all & (int)one) == (int)one;
        }
        #endregion

        #endregion

        #region Properties
        #region Missing...
        /// <summary>
        /// Returned if missing value. Default=Int64.MinValue
        /// </summary>
        public Int64 MissingInt64
        {
            get { return _missingInt64; }
            set { _missingInt64 = value; }
        }

        /// <summary>
        /// Returned if missing value. Default=Int32.MinValue
        /// </summary>
        public Int32 MissingInt32
        {
            get { return _missingInt32; }
            set { _missingInt32 = value; }
        }

        /// <summary>
        /// Returned if missing value. Default=-1
        /// </summary>
        public Int16 MissingInt16
        {
            get { return _missingInt16; }
            set { _missingInt16 = value; }
        }

        /// <summary>
        /// Returned if missing value. Default=-1.0
        /// </summary>
        public Double MissingDouble
        {
            get { return _missingDouble; }
            set { _missingDouble = value; }
        }

        /// <summary>
        /// Returned if missing value. Default=0
        /// </summary>
        public Byte MissingByte
        {
            get { return _missingByte; }
            set { _missingByte = value; }
        }

        /// <summary>
        /// Returned if missing value. Default=-1
        /// </summary>
        public SByte MissingSByte
        {
            get { return _missingSByte; }
            set { _missingSByte = value; }
        }
        public char MissingChar
        {
            get { return _missingChar; }
            set { _missingChar = value; }
        }
        public string MissingString
        {
            get { return _missingString; }
            set { _missingString = value; }
        }


        #endregion

        /// <summary>
        /// First line in the file, containing keywords
        /// </summary>
        public string FirstLine
        {
            get { return _firstline; }
        }

        /// <summary>
        /// Current line
        /// </summary>
        public string Line
        {
            get { return _line; }
        }

        /// <summary>
        /// Returns the underlying StreamReader-object
        /// </summary>
        public StreamReader StreamReader
        {
            get { return _file; }
        }

        /// <summary>
        /// Number of lines in the file
        /// </summary>
        public int NumberOfLines
        {
            get { return _nLines; }
        }

        /// <summary>
        /// Number of observations in the file.
        /// </summary>
        public int NumberOfObservations
        {
            get { return _nLines - 1; }
        }


        /// <summary>
        /// The size of the current group
        /// </summary>
        public int GroupSize
        {
            get { return _groupSize; }
        }

        /// <summary>
        /// Returns an array of all keywords defined in the file
        /// </summary>
        public string[] Keywords
        {
            get { return _keyword; }
        }

        /// <summary>
        /// Returns an array of all elements parsed from the current line
        /// </summary>
        public string[] Elements
        {
            get { return _element; }
        }

        #endregion

    }


}
