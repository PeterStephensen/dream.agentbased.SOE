using System;
using System.Collections.Generic;
using System.Text;

namespace Dream.AgentClass
{

    public class Agents<T> : Agent
    {

        #region Constructers
        public Agents()
        {
            if (typeof(T) != typeof(Agent))
                this.ChildType = typeof(T);

        }
        public Agents(bool removeWhenEmpty) : this()
        {
            this.RemoveWhenEmpty = removeWhenEmpty;
        }

        #endregion

        #region Operator overloads
        public static Agents<T> operator +(Agents<T> ags, Agent a)
        {
            ags.AddAgent(a);
            return ags;
        }

        public static Agents<T> operator -(Agents<T> ags, Agent a)
        {
            ags.RemoveAgent(a);
            return ags;
        }
        #endregion

    }
}
