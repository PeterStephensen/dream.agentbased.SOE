using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dream.Models.SOE_Basic
{
    public class Event
    {
        #region System
        // Use 1-9
        public struct System
        {
            public const int Start = 1;
            public const int Stop = 2;
            public const int PeriodStart = 3;
            public const int PeriodEnd = 4;


        }
        #endregion

        #region Economics
        // Use 10-29
        public struct Economics
        {
            public const int Delivery = 10;
            public const int Update = 11;

        }
        #endregion //Demographics

    }
}
