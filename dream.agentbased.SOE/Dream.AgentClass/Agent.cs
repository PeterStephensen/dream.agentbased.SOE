using System;
using System.IO;
using System.Collections;

namespace Dream.AgentClass
{

    #region IAgent
    public interface IAgent : IEnumerable
    {
        void EventProc(int idEvent);
        void AddAgent(IAgent a);
        void RemoveAgent(IAgent a);
        void RemoveThisAgent();
        void RandomizeAgents();
    }
    #endregion

    [Serializable]
    public class Agent : IAgent, ICollection
    {

        #region Public fields
        public bool Removed = false;
        /// <summary>
        /// If true, RemoveThisAgent() is executed at next EventProc() if no agents inside this agent
        /// </summary>
        public bool RemoveWhenEmpty = false;
        #endregion

        #region Static private fields
        static int _nAgent; // Total number of agents
        static Random _random;
        static int _seed = -1;
        #endregion

        #region Private fields
        Agent _next, _prev, _first, _last, _parent;
        int _count;
        Type _childType;
        int _id;
        #endregion

        #region Constructor
        public Agent()
        {
            _id = _nAgent;
            _nAgent++;
        }
        public Agent(Agent parent)
          : this()
        {
            parent.AddAgent(this);
        }

        #endregion

        #region Destructor
        //Just for counting
        //~Agent()
        //{
        //    _nAgent--;
        //}
        #endregion

        #region EventProc()
        /// <summary>
        /// Method that contains the behaviour of the agent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        virtual public void EventProc(int idEvent)
        {
            if (_first != null)
                for (Agent a = _first; a != null; a = a._next)
                    a.EventProc(idEvent);

            if (this.RemoveWhenEmpty)
                if (this.NumberOfAgents == 0)
                    RemoveThisAgent();

        }
        #endregion

        #region AddAgent()
        /// <summary>
        /// Adds a child-agent
        /// </summary>
        /// <param name="a"></param>
        public virtual void AddAgent(IAgent ia)
        {

            Agent a = (Agent)ia;

            // Test for child-type
            if (_childType != null)
            {
                if (a.GetType() != _childType)
                {
                    throw (new ArgumentException("The Agent-class " + this.GetType().ToString()
                      + " only accepts Agent-class " + _childType.ToString() + " as a child."));
                }
            }

            // release old relations
            a.RemoveThisAgent();

            // create new relations
            a._next = null;
            a._prev = _last;
            a._parent = this;

            if (_first == null)
                _first = a;
            else
                _last._next = a;

            _last = a;
            _count++;
        }
        #endregion

        #region RemoveAgent()
        /// <summary>
        /// Removes a child-agent
        /// </summary>
        /// <param name="a"></param>
        public virtual void RemoveAgent(IAgent ia)
        {
            Agent a = (Agent)ia;

            if (this != a._parent) throw new ArgumentException("a.RemoveAgent(b) requires b is a child of a");

            Removed = true;

            if (_count == 1)
            {
                _first = null;
                _last = null;
            }
            else
            {
                if (a._prev != null) a._prev._next = a._next;
                if (a._next != null) a._next._prev = a._prev;

                if (a == _first) _first = a._next;
                if (a == _last) _last = a._prev;

            }
            _count--;
        }
        #endregion

        #region RemoveThisAgent()
        public void RemoveThisAgent()
        {
            if (_parent == null) return;
            _parent.RemoveAgent(this);
        }
        #endregion

        #region RandomizeAgents()
        public void RandomizeAgents()
        {

            if (_count == 0) return;
            if (_count == 1) return;
            if (_random == null)
            {
                if (_seed > 0)
                    _random = new Random(_seed);
                else
                    _random = new Random();
            }


            Agent[] arr = new Agent[_count];
            int[] index = new int[_count];
            double[] rnd = new double[_count];

            arr[0] = _first;
            index[0] = 0;
            rnd[0] = _random.NextDouble();
            for (int i = 1; i < _count; i++)
            {
                arr[i] = arr[i - 1]._next;
                index[i] = i;
                rnd[i] = _random.NextDouble();
            }

            Array.Sort(rnd, index);

            _first = arr[index[0]];
            _first._next = arr[index[1]];
            _first._prev = null;

            for (int i = 1; i < _count - 1; i++)
            {
                arr[index[i]]._prev = arr[index[i - 1]];
                arr[index[i]]._next = arr[index[i + 1]];
            }

            _last = arr[index[_count - 1]];
            _last._prev = arr[index[_count - 2]];
            _last._next = null;

        }
        #endregion

        #region CopyTo()
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException("CopyTo not implemented in Agent-object");

        }
        #endregion

        #region Properties
        /// <summary>
        /// The number of child-agents
        /// </summary>
        public virtual int NumberOfAgents
        {
            get { return _count; }
        }
        /// <summary>
        /// The number of child-agents
        /// </summary>
        public int Count
        {
            get { return _count; }
        }
        public bool IsSynchronized
        {
            get { return false; }
        }
        public object SyncRoot
        {
            get { return null; }
        }

        /// <summary>
        /// The ID of the agent
        /// </summary>
        public int ID
        {
            get { return _id; }
        }

        /// <summary>
        /// The ID of parent agent
        /// </summary>
        public int ParentID
        {
            get
            {
                if (this._parent != null)
                    return _parent._id;
                else
                    return -1;
            }
        }


        public Type ChildType
        {
            get { return _childType; }
            set { _childType = value; }
        }

        /// <summary>
        /// Seed used to initialize the random generator used by RandomizeAgents. If RandomSeed=-1 (default) then the timer is used to initialize.
        /// </summary>
        public static int RandomSeed
        {
            get { return _seed; }
            set { _seed = value; }
        }


        /// <summary>
        /// The total number of agent-objects allocatet
        /// </summary>
        public int TotalNumberOfAgents
        {
            get { return _nAgent; }
        }

        /// <summary>
        /// The parent-agent. The agent that this agent was added to.
        /// </summary>
        public Agent ParentAgent
        {
            get { return _parent; }
        }

        virtual public Agent NextAgent
        {
            get { return _next; }
        }

        public Agent PreviousAgent
        {
            get { return _prev; }
        }

        virtual public Agent FirstAgent
        {
            get { return _first; }
        }

        public Agent LastAgent
        {
            get { return _last; }
        }
        #endregion

        #region Operator overloads
        public static Agent operator +(Agent ags, Agent a)
        {
            ags.AddAgent(a);
            return ags;
        }

        public static Agent operator -(Agent ags, Agent a)
        {
            ags.RemoveAgent(a);
            return ags;
        }
        #endregion

        #region IEnumerator-stuff

        #region GetEnumerator()
        public IEnumerator GetEnumerator()
        {

            Agent a = this._first;

            while (a != null)
            {
                yield return a;
                a = a._next;
            }
        }
        /// <summary>
        /// Enumerate over all agents in the tree
        /// </summary>
        public IEnumerable All()
        {
            Agent a = this._first;

            while (a != null)
            {
                yield return a;
                a = moveNext_All(a);
            }
        }

        /// <summary>
        /// Enumerate only over the deepest children in the tree
        /// </summary>
        public IEnumerable Deep()
        {
            Agent a = this._first;

            while (a != null)
            {
                if (a._first == null) yield return a;
                a = moveNext_All(a);
            }
        }


        #endregion

        Agent moveNext_All(Agent a)
        {

            if (a._first != null)
                return a._first;
            else if (a._next != null)
                return a._next;
            else if (a._parent != null)
            {
                Agent b = a;

                while (b._next == null)
                {
                    if (b._parent != null)
                        b = b._parent;
                    else
                        return null;
                }


                return b._next;

            }

            return null;

        }


        #region Old stuff




        //        #region AgentEnumerator-class
        //        class AgentEnumerator : IEnumerator
        //        {

        //            Agent _agent;
        //            Agent _current;
        //            bool _reset;

        //            public AgentEnumerator(IAgent ia)
        //            {
        //                _agent = (Agent)ia;
        //                this.Reset();
        //            }

        //            Agent moveNext_All(Agent a)
        //            {

        //                if (a._first != null)
        //                {
        //                    return a._first;
        //                }
        //                else if (a._next != null)
        //                {
        //                    return a._next;
        //                }
        //                else if (a._parent != null)
        //                {
        //                    Agent b = a;

        //                    while (b._next == null)
        //                    {
        //                        if (b._parent != null)
        //                            b = b._parent;
        //                        else
        //                            return null;
        //                    }


        //                    return b._next;

        //                }

        //                return null;

        //            }


        //            public bool MoveNext()
        //            {


        //                if (_agent.EnumerationType == IAgentEnumerationType.Simpel)
        //                {
        //                    if(_reset)
        //                        _current = _current._first;
        //                    else
        //                        _current = _current._next;

        //                }
        //                else if (_agent.EnumerationType == IAgentEnumerationType.All)
        //                {
        //                    if(!_reset)
        //                        _current = moveNext_All(_current);
        //                }
        //                else if (_agent.EnumerationType == IAgentEnumerationType.Deep)
        //                {

        //                    while (true)
        //                    {
        //                        _current = moveNext_All(_current);
        //                        if (_current == null) break;
        //                        if (_current._first == null) break;
        //                    }

        //                }

        //                _reset = false;
        //                return !(_current==null);

        //            }				

        //            public void Reset()
        //            {
        //                _reset=true;
        //                _current=_agent;
        //            }		

        //            public object Current
        //            {
        //                get{return _current;}
        //            }
        //        }
        //        #endregion			

        //        #region operator + - IKKE HELT OK!!!! - DISABLED
        ////		public static AgentSum operator +(Agent a1, Agent a2)
        ////		{
        ////			return new AgentSum(a1, a2);
        ////		}
        //        #endregion

        #endregion


        //
        #endregion

    }

}
