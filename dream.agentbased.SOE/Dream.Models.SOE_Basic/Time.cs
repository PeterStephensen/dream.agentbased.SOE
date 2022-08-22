using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dream.Models.SOE_Basic
{
public class Time
  {

    #region Private fields
    int _start, _end;
    int _t, _n;
    #endregion

    #region Constructors
    public Time(int start, int end)
    {
      _start = start;
      _end = end;
      _t = start;

    }
    #endregion

    #region NextPeriod()
    public bool NextPeriod()
    {
      _t++;
      if (_t > _end)
        return false;
      else
        return true;
    }
    #endregion

    #region Public properties

    /// <summary>
    /// First period in simulation
    /// </summary>
    public int StartPeriod
    {
      get { return _start; }
    }

    /// <summary>
    /// Last year in simulation
    /// </summary>
    public int EndPeriod
    {
      get { return _end; }
    }

    /// <summary>
    /// Current year in simulation
    /// </summary>
    public int Now
    {
      get { return _t; }
    }
    #endregion

  }

}
