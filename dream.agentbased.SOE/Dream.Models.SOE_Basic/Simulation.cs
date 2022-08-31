using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Dream.AgentClass;
using Dream.IO;

namespace Dream.Models.SOE_Basic
{
    public class Simulation : Agent
    {
        #region Static private fields
        static Simulation _instance;
        #endregion

        #region Private fields
        Time _time;
        Statistics _statistics;
        Agents<Household> _households;
        //Agents<Firm> _firms;
        Agents<Agent> _tools;
        Agents<Agent> _sectors;
        Settings _settings;
        Random _random;
        int _seed = 0;
        PublicSector _publicSector;
        Forecaster _forecaster;
        double _nFirmNewTotal = 0;
        double[] _nFirmNew;
        Dictionary<int, Firm> _firmDict;
        Agents<Firm>[] _sectorList = null;
        Firm[] _randomFirmList = null;
        #endregion

        #region Constructors
        public Simulation(Settings settings, Time time)
        {
            _settings = settings;
            _time = time;

            if (_settings.RandomSeed > 0)
            {
                _random = new(_settings.RandomSeed);     // The one and only Random object
                Agent.RandomSeed = _settings.RandomSeed;
                _seed = _settings.RandomSeed;
            }
            else
            {
                _random = new();                      // We need to know the seed, even when we havent defined it
                _seed = _random.Next();
                _random = new(_seed);                // Overwrite _random with seeded 
                Agent.RandomSeed = _seed;
            }

            if (_settings.SaveScenario & _settings.Shock!=EShock.Nothing)
            {
                string scnPath = _settings.ROutputDir + "\\scenario_info.txt";
                using (StreamReader sr = File.OpenText(scnPath))
                {
                    sr.ReadLine();                          // Read first line
                    _seed = Int32.Parse(sr.ReadLine());  // Seed on second line

                    _random = new(_seed);                    // Overwrite _random with seeded 
                    Agent.RandomSeed = _seed;

                }
            }

            if (_instance != null)
                throw new Exception("Simulation object is singleton");
            
            _instance = this;

            _statistics = new Statistics();
            _publicSector = new PublicSector(); // Not used
            _forecaster = new Forecaster();     // Not used
            _households = new Agents<Household>();
            _tools = new Agents<Agent>();

            _sectors = new Agents<Agent>();

            this.AddAgent(_tools);
            this.AddAgent(_households);
            this.AddAgent(_sectors);
            this.AddAgent(_publicSector);

            _sectorList = new Agents<Firm>[_settings.NumberOfSectors];
            _randomFirmList = new Firm[_settings.NumberOfSectors];
            for (int i = 0; i < _settings.NumberOfSectors; i++)
            {
                Agents<Firm> sector = new Agents<Firm>();
                // 'sector' is placed in 2 lists: the Agent-linked-list _sectors and the C#-list _sectorList
                _sectors += sector;         // This is for the agent tree to work
                _sectorList[i] = sector;    // This is for looking up firms

            }

            _tools += _statistics;
            _tools += _forecaster;
            
            if(settings.LoadDatabase)
            {
                throw new NotImplementedException();
                
                #region Loating database
                Console.WriteLine("LoadDatabase..");


                TabFileReader file = new TabFileReader(_settings.ROutputDir + "\\db_firms.txt");

                _firmDict = new();
                
                while(file.ReadLine())
                {
                    int id = file.GetInt32("ID");
                    Firm f = new(file);
                    _firmDict.Add(id, f);

                }
                file.Close();

                file = new TabFileReader(_settings.ROutputDir + "\\db_households.txt");

                while (file.ReadLine())
                    _households += new Household(file);

                file.Close();
                #endregion
            }
            else
            {
                int n_perSector = (int)(settings.NumberOfFirms / settings.NumberOfSectors);

                for (int s = 0; s < settings.NumberOfSectors; s++)
                {
                    for (int i = 0; i < n_perSector; i++)
                    {
                        List<Household> hs = new();
                        for (int j = 0; j < settings.NumberOfHouseholdsPerFirm; j++)
                        {
                            Household h = new();
                            _households += h;
                            hs.Add(h);
                        }

                        Firm f = new(hs, s); //Allocate firm
                        _sectorList[s] += f;
                        foreach (Household h in hs)
                            h.Communicate(ECommunicate.Initialize, f); // Tell households where they are employed
                    }
                }
            }

            _nFirmNew = new double[_settings.NumberOfSectors];           
            
            EventProc(Event.System.Start);

        }
        #endregion

        #region EventProc()
        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {
                case Event.System.Start:
                    base.EventProc(idEvent);
                    // Event pump
                    do
                    {
                        this.EventProc(Event.System.PeriodStart);
                        this.EventProc(Event.Economics.Update);
                        this.EventProc(Event.System.PeriodEnd);
                        foreach(Agent firms in _sectors)
                           firms.RandomizeAgents();
                    } while (_time.NextPeriod());

                    this.EventProc(Event.System.Stop);
                    break;

                case Event.System.PeriodStart:
                    _statistics.Communicate(EStatistics.FirmNew, _nFirmNewTotal);
                    int n_firms = 0;
                    foreach (Agent fs in _sectors)
                        n_firms += fs.Count;
                    //Console.Write("\r                                                                           "); // Erase line
                    //Console.Write("\r{0:#.##}\t{1}\t{2}", 1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear, n_firms, _households.Count);
                    //Console.WriteLine("{0:#.##}\t{1}\t{2}", 1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear, n_firms, _households.Count);
                    Console.WriteLine("{0:#.##}\t{1}\t{2}\t{3:#.######}\t{4:#.######}\t{5:#.######}", 1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear,
                        n_firms, _households.Count, _statistics.PublicMarketWageTotal, _statistics.PublicMarketPriceTotal, 
                        _statistics.PublicMarketWageTotal/ _statistics.PublicMarketPriceTotal); 

                    base.EventProc(idEvent);
                    break;

               case Event.System.PeriodEnd:
                    base.EventProc(idEvent);
                    // New households
                    for (int i = 0; i < _settings.HouseholdNewBorn; i++)
                        _households += new Household();

                    // Shock: 10% stigning i arbejdsudbud 
                    if (_time.Now == _settings.ShockPeriod)
                    {
                        if(_settings.Shock==EShock.LaborSupply)
                            for (int i = 0; i < 0.1 * _households.Count; i++)
                                _households += new Household();

                    }

                    // After burn-in-stuff    
                    if (_time.Now == _settings.BurnInPeriod1)
                    {
                        //_settings.FirmDefaultProbability = 1.0 / (40 * 12);  // Expectet default age: 40 years
                        _settings.FirmDefaultProbability = 0; //!!!!!!!!!!!!!!!!!!!!
                        _settings.FirmStartNewFirms = true;
                        _settings.FirmStartupPeriod = 6;
                        _settings.FirmStartupEmployment = 10;  //15
                    }
                    
                    // Investor
                    if (_settings.FirmStartNewFirms)
                    {

                        if (_time.Now < _settings.BurnInPeriod2)
                            _nFirmNewTotal = _settings.InvestorInitialInflow;
                        else
                        {
                            _nFirmNewTotal += _settings.InvestorProfitSensitivity * _statistics.PublicExpectedSharpRatioTotal * _nFirmNewTotal;
                            if (_nFirmNewTotal<0)
                                _nFirmNewTotal = 0;

                        }

                        if (_nFirmNewTotal > 0)
                        {
                            if (_time.Now < _settings.BurnInPeriod2)
                            {
                                int n = _random.NextInteger(_nFirmNewTotal);
                                for (int i = 0; i < n; i++)
                                {
                                    // Random sector
                                    int sector = _random.Next(_settings.NumberOfSectors);
                                    _sectorList[sector] += new Firm(sector);

                                }
                            }
                            else
                            {
                                double kappa = 0.1;

                                double sum = 0;
                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                    sum += _nFirmNew[i] * Math.Exp(kappa * _statistics.PublicExpectedSharpRatio[i]);

                                for (int i = 0  ; i < _settings.NumberOfSectors; i++)
                                {
                                    double d = _nFirmNewTotal * _nFirmNew[i] * Math.Exp(kappa * _statistics.PublicExpectedSharpRatio[i]) / sum;
                                    _nFirmNew[i] = _random.NextInteger(d);
                                    for (int j = 0; j < _nFirmNew[i]; j++)
                                        _sectorList[i] += new Firm(i);
                                }
                            }
                        }
                    }                   
                    break;

                case Event.System.Stop:
                    base.EventProc(idEvent);
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }
        double Func(double x)
        {
            return x < 1 ? x : 1;
        }

        #endregion

        #region GetRandomFirm
        public Firm GetRandomFirm_old()
        {

            //if (_randomFirm != null)
            //{
            //    if (_firms.Count == 1)
            //        return _randomFirm;
            //    _randomFirm = (Firm)_randomFirm.NextAgent;
            //}

            //if (_randomFirm == null)
            //{
            //    _firms.RandomizeAgents();
            //    _randomFirm = (Firm)_firms.FirstAgent;
            //}
            //return _randomFirm;
            return null;

        }
        public Firm GetRandomFirm(int sector)
        {

            if (_randomFirmList[sector] != null)
            {
                if (_sectorList[sector].Count == 1)
                    return _randomFirmList[sector];
                _randomFirmList[sector] = (Firm)_randomFirmList[sector].NextAgent;
            }

            if (_randomFirmList[sector] == null)
            {
                _sectorList[sector].RandomizeAgents();
                _randomFirmList[sector] = (Firm)_sectorList[sector].FirstAgent;
            }
            return _randomFirmList[sector];

        }
        #endregion

        #region GetRandomFirms
        public List<Firm> GetRandomFirms_old(int n)
        {
            if (n < 1) return null;
            
            List<Firm> lst = new();
            //for (int i = 0; i < n; i++)
            //    lst.Add(GetRandomFirm());

            return lst;
        }
        public List<Firm> GetRandomFirms(int n, int sector)
        {
            if (n < 1) return null;

            List<Firm> lst = new();
            for (int i = 0; i < n; i++)
                lst.Add(GetRandomFirm(sector));

            return lst;
        }
        public List<Firm> GetRandomFirmsAllSectors(int n)  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            if (n < 1) return null;

            List<Firm> lst = new();
            for (int i = 0; i < n; i++)
            {
                int sector = _random.Next(_settings.NumberOfSectors);
                lst.Add(GetRandomFirm(sector));
            }

            return lst;
        }
        public List<Firm> GetRandomFirmsAllSectors_old(int n)
        {
            return GetRandomFirms(n,0);
        }
        #endregion

        public Agents<Firm> Sector(int sector)
        {
            return _sectorList[sector];
        }



        #region GetFirmFromID()
        public Firm GetFirmFromID(int ID)
        {
            if(_firmDict!=null)
            {
                try 
                {
                    return _firmDict[ID];
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
        #endregion

        #region Public properties
        public Settings Settings
        {
            get { return _settings; }
        }

        public Random Random
        {
            get { return _random; }
        }
        /// <summary>
        /// Seed used to initialize the Random object
        /// </summary>
        public int Seed
        {
            get { return _seed; }
        }

        public Time Time
        {
            get { return _time; }
        }


        public static Simulation Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Groups
        /// </summary>
        public Agents<Household> Households
        {
            get { return _households; }
        }

        //public Agents<Firm> Firms
        //{
        //    get { return _firms; }
        //}


        public Statistics Statistics
        {
            get { return _statistics; }
        }

        public PublicSector PublicSector
        {
            get { return _publicSector; }
        }

        public Forecaster Forecaster
        {
            get { return _forecaster; }
        }

        public Agents<Agent> Tools
        {
            get { return _tools; }
            //set { _tools = value; }
        }

        #endregion

    }
}
