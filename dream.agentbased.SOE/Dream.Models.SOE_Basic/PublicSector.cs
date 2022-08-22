using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dream.AgentClass;

namespace Dream.Models.SOE_Basic
{
    public class PublicSector : Agent
    {

        #region Private fields
        Simulation _simulation;
        Settings _settings;
        Time _time;
        Statistics _statistics;
        #endregion

        #region Constructor
        public PublicSector()
        {
            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;
        }
        #endregion

        #region EventProc
        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {

                case Event.System.Start:
                    break;

                case Event.System.PeriodStart:
                    break;

                case Event.System.Stop:
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }
        #endregion

        #region Communicate
        public ECommunicate Communicate(ECommunicate comID, object o)
        {
            switch (comID)
            {
                case ECommunicate.Yes:
                    return ECommunicate.Ok;
                default:
                    return ECommunicate.Ok;
            }
        }
        #endregion

    }
}
