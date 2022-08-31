using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
//using System.Threading;
using System.Text.Json;


using Dream.AgentClass;
using Dream.IO;

namespace Dream.Models.SOE_Basic
{
    public class Statistics : Agent
    {

        #region Private fields
        Simulation _simulation;
        Settings _settings;
        Time _time;
        double[] _marketPrice, _marketWage;
        double[] _employment, _sales, _production;
        double _marketWageTotal = 0;
        double _marketPriceTotal = 0;
        double _totalProfit, _profitPerHousehold, _expProfit;
        StreamWriter _fileFirmReport;
        StreamWriter _fileHouseholdReport;
        StreamWriter _fileDBHouseholds;
        StreamWriter _fileDBFirms;
        StreamWriter _fileDBStatistics;
        StreamWriter _fileMacro;
        StreamWriter _fileSectors;
        double _macroProductivity = 1.0;
        double[] _sectorProductivity;
        double _interestRate;
        double _meanValue = 0;
        double _discountedProfits = 0;
        int _nFirmCloseNatural = 0, _nFirmCloseNegativeProfit = 0, _nFirmCloseTooBig = 0, _nFirmCloseZeroEmployment=0;
        double _nFirmNew = 0;
        double _expDiscountedProfits = 0;
        double _sharpeRatioTotal = 0;
        double _sigmaRiskTotal = 0;
        double _expSharpeRatioTotal = 0;
        double[] _sharpeRatio;
        double[] _sigmaRisk;
        double[] _expSharpeRatio;
        double _yr_consumption = 0; 
        int _yr_employment = 0;
        double _totalEmployment = 0;
        double _totalSales = 0;
        double _totalProduction = 0;
        int _scenario_id = 0;
        string _runName = "Base";
        double _laborSupply = 0;  // Measured in productivity units
        int _n_laborSupply = 0;   // Measured in heads
        int _n_unemployed = 0;     // Measured in heads
        #endregion

        #region Constructor
        public Statistics()
        {
            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;

            _marketPrice = new double[_settings.NumberOfSectors];
            _marketWage = new double[_settings.NumberOfSectors];
            _employment = new double[_settings.NumberOfSectors];
            _sales = new double[_settings.NumberOfSectors];
            _production = new double[_settings.NumberOfSectors];
            _sectorProductivity = new double[_settings.NumberOfSectors];
            _sigmaRisk = new double[_settings.NumberOfSectors];
            _sharpeRatio = new double[_settings.NumberOfSectors];
            _expSharpeRatio = new double[_settings.NumberOfSectors];

            for (int i = 0; i < _settings.NumberOfSectors; i++)
            {
                _marketPrice[i] = _settings.StatisticsInitialMarketPrice;
                _marketWage[i] = _settings.StatisticsInitialMarketWage;
                _sectorProductivity[i] = 1.0;
            }
            _marketPriceTotal = _settings.StatisticsInitialMarketPrice;
            _marketWageTotal = _settings.StatisticsInitialMarketWage;
            _interestRate = _settings.StatisticsInitialInterestRate;

            string sJson = JsonSerializer.Serialize(_settings);
            File.WriteAllText(_settings.ROutputDir + "\\Settings.json", sJson);

            if (_settings.LoadDatabase)
            {
                TabFileReader file = new TabFileReader(_settings.ROutputDir + "\\db_statistics.txt");
                file.ReadLine();

                //_marketPrice = file.GetDouble("marketPrice");
                //_marketWage = file.GetDouble("marketWage");
                _expSharpeRatioTotal = file.GetDouble("expSharpeRatio");
                _sharpeRatioTotal = file.GetDouble("expSharpeRatio");

                file.Close();

            }

        }
        #endregion

        #region EventProc
        public override void EventProc(int idEvent)
        {
            
            switch (idEvent)
            {

                case Event.System.Start:
                    OpenFiles();                   
                    break;

                case Event.System.PeriodStart:
                    
                    if (_time.Now == _settings.StatisticsWritePeriode)
                    {

                        string path = _settings.ROutputDir + "\\db_households.txt";
                        if (File.Exists(path)) File.Delete(path);
                        _fileDBHouseholds = File.CreateText(path);
                        _fileDBHouseholds.WriteLine("ID\tAge\tFirmEmploymentID\tFirmShopID\tProductivity");

                        path = _settings.ROutputDir + "\\db_firms.txt";
                        if (File.Exists(path)) File.Delete(path);
                        _fileDBFirms = File.CreateText(path);
                        _fileDBFirms.WriteLine("ID\tAge\tphi0\texpPrice\texpWage\texpQuitters\texpApplications\texpPotentialSales\texpSales\tw\tp\tSales\tProfit");

                        path = _settings.ROutputDir + "\\db_statistics.txt";
                        if (File.Exists(path)) File.Delete(path);
                        _fileDBStatistics = File.CreateText(path);
                        _fileDBStatistics.WriteLine("expSharpeRatio\tmacroProductivity\tmarketPrice\tmarketWage");

                    }

                    if (_time.Now == _settings.StatisticsWritePeriode + 1)
                    {
                        if (_fileDBHouseholds != null)
                            _fileDBHouseholds.Close();

                        if (_fileDBFirms != null)
                            _fileDBFirms.Close();

                        if (_fileDBStatistics != null)
                            _fileDBStatistics.Close();
                    }

                    // Profit income to households
                    _totalProfit = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        foreach (Firm f in _simulation.Sector(i))
                            _totalProfit += f.Profit;

                    _profitPerHousehold = _totalProfit / _simulation.Households.Count; 
                    break;

                case Event.System.PeriodEnd:
                    if (_time.Now == _settings.StatisticsWritePeriode)
                        Write();

                    // Statistics
                    //List<double> wages = new();
                    //List<double> prices = new();

                    //foreach (Household h in _simulation.Households)
                    //{
                    //    if(h.FirmEmployment!=null)
                    //        wages.Add(h.FirmEmployment.Wage);
                    //    prices.Add(h.CES_Price);
                    //}

                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                    {
                        double meanWage = 0;
                        double meanPrice = 0;
                        double discountedProfits = 0;
                        _employment[i] = 0;
                        _sales[i] = 0;

                        foreach (Firm f in _simulation.Sector(i))
                        {
                            meanWage += f.Wage * f.Employment;
                            meanPrice += f.Price * f.Sales;
                            _employment[i] += f.Employment;
                            _sales[i] += f.Sales;
                            _production[i] += f.Production;
                            discountedProfits += f.Profit / Math.Pow(1 + _interestRate, f.Age);
                        }
                        
                        if (meanWage > 0)
                            _marketWage[i] = meanWage / _employment[i];

                        if (meanPrice > 0 & _sales[i] > 0)
                            _marketPrice[i] = meanPrice / _sales[i];

                        // Calculate Sharpe Ratio
                        double m_pi0 = discountedProfits / _simulation.Sector(i).Count;
                        _sigmaRisk[i] = 0;

                        foreach (Firm f in _simulation.Sector(i))
                            _sigmaRisk[i] += Math.Pow(f.Profit / Math.Pow(1 + _interestRate, f.Age) - m_pi0, 2);
                        _sigmaRisk[i] = Math.Sqrt(_sigmaRisk[i] / _simulation.Sector(i).Count);
                        _sharpeRatio[i] = _sigmaRisk[i] > 0 ? m_pi0 / _sigmaRisk[i] : 0;

                        _expSharpeRatio[i] = _settings.StatisticsExpectedSharpeRatioSmooth * _expSharpeRatio[i] + (1 - _settings.StatisticsExpectedSharpeRatioSmooth) * _sharpeRatio[i];


                    }

                    _meanValue = 0;
                    _discountedProfits = 0;
                    _totalSales = 0;
                    _totalEmployment = 0;
                    _totalProduction = 0;
                    //double totProfit = 0;
                    double mean_age = 0;
                    double tot_vacancies = 0;
                    double meanWageTot = 0;
                    double meanPriceTot = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        foreach (Firm f in _simulation.Sector(i))
                        {
                            meanWageTot += f.Wage * f.Employment;
                            meanPriceTot += f.Price * f.Sales;
                            _totalEmployment += f.Employment;
                            _totalSales += f.Sales;
                            _totalProduction += f.Production;
                            _meanValue += f.Value;
                            mean_age += f.Age;
                            tot_vacancies += f.Vacancies;
                            _discountedProfits += f.Profit / Math.Pow(1+_interestRate, f.Age);
                        }

                    int n_firms = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        n_firms += _simulation.Sector(i).Count;

                    double m_pi = _discountedProfits /n_firms;
                    _sigmaRiskTotal = 0;

                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        foreach (Firm f in _simulation.Sector(i))
                            _sigmaRiskTotal += Math.Pow(f.Profit / Math.Pow(1 + _interestRate, f.Age) - m_pi, 2);
                    _sigmaRiskTotal = Math.Sqrt(_sigmaRiskTotal / n_firms);
                    _sharpeRatioTotal = _sigmaRiskTotal > 0 ? m_pi / _sigmaRiskTotal : 0;

                    _expDiscountedProfits = 0.99 * _expDiscountedProfits + (1 - 0.99) * _discountedProfits; // Bruges ikke
                    _expSharpeRatioTotal = _settings.StatisticsExpectedSharpeRatioSmooth * _expSharpeRatioTotal + (1 - _settings.StatisticsExpectedSharpeRatioSmooth) * _sharpeRatioTotal;
                    mean_age /= n_firms;
                    _meanValue /= n_firms;
                    //_expProfit = totProfit / n_firms;

                    if (meanWageTot > 0)
                        _marketWageTotal = meanWageTot / _totalEmployment;

                    // Calculate median wage
                    //if (wages.Count > 0)
                    //    _marketWage = wages.Median();

                    if (_time.Now > _settings.FirmPriceMechanismStart)
                    {
                        // Calculate median price
                        //if (prices.Count > 0)
                        //    _marketPrice = prices.Median();

                        if (meanPriceTot > 0 & _totalSales > 0)
                            _marketPriceTotal = meanPriceTot / _totalSales;

                        //_discountedProfits /= _marketPrice;
                    }

                    if((_time.Now + 1) % _settings.PeriodsPerYear == 0)
                    {
                        _yr_consumption = 0;
                        _yr_employment = 0;
                        foreach (Household h in _simulation.Households)
                        {
                            _yr_consumption += h.YearConsumption;
                            _yr_employment += h.YearEmployment;
                        }
                    }

                    #region Graphics
                    // Graphics
                    if (_settings.StatisticsGraphicsPlotInterval > 0 & (_time.Now > _settings.StatisticsGraphicsStartPeriod))
                        if (_time.Now % _settings.StatisticsGraphicsPlotInterval == 0) // Once a year
                        {
                            double tot_opt_l = 0;// Calculate total optimal employment  
                            //double tot_l = 0;// Calculate total employment  
                            double prod_avr = 0; // Calculate average productivity
                            //double tot_vacancies = 0;
                            //double tot_sales = 0;
                            using (StreamWriter sw = File.CreateText(_settings.ROutputDir + "\\data_firms.txt"))
                            {

                                sw.WriteLine("Productivity\tOptimalEmployment\tOptimalProduction\tEmployment\tProfit\tSales\tAge\tDiscountedProfits");
                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                    foreach (Firm f in _simulation.Sector(i))
                                    {
                                        double disc = 1.0 / (1 + _interestRate);
                                        double discProfit = f.Profit * Math.Pow(disc, f.Age) / _marketPrice[f.Sector];
                                    
                                        sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", f.Productivity, f.OptimalEmployment, f.OptimalProduction, f.Employment, f.Profit, f.Sales, f.Age, discProfit);
                                        //prod_avr += f.Productivity;
                                        prod_avr += Math.Pow(f.Productivity, 1/(1-_settings.FirmAlpha));
                                        tot_opt_l += f.OptimalEmployment;
                                        //tot_l += f.Employment;
                                        //tot_vacancies += f.Vacancies;
                                        //tot_sales += f.Sales;
                                    }
                            }

                            //for (int i = 0; i < _settings.NumberOfSectors; i++)
                            //    n_firms += _simulation.Sector(i).Count;

                            prod_avr /= n_firms;
                            prod_avr = Math.Pow(prod_avr, 1 - _settings.FirmAlpha);
                            //double P_star = _marketWage * Math.Pow(_settings.NumberOfHouseholdsPerFirm, 1 - _settings.FirmAlpha) / (_settings.FirmAlpha * prod_avr);
                            //tot_opt_l /= _settings.NumberOfFirms * _settings.NumberOfHouseholdsPerFirm;
                            double P_star = 0;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                            //int nUnemp = 0;
                            //int laborSupply = 0;
                            //foreach (Household h in _simulation.Households)
                            //{
                            //    if(h.Age<_settings.HouseholdPensionAge)
                            //    {
                            //        nUnemp += h.Unemployed ? 1 : 0;
                            //        laborSupply++;
                            //    }
                            //}

                            //----------
                            int nUnemp = 0;
                            int laborSupply = 0;
                            _n_laborSupply = 0;
                            _laborSupply = 0;
                            _n_unemployed = 0;
                            foreach (Household h in _simulation.Households)
                            {
                                if (h.Age < _settings.HouseholdPensionAge)
                                {
                                    nUnemp += h.Unemployed ? 1 : 0;
                                    laborSupply++;
                                    _n_laborSupply++;
                                    _laborSupply += h.Productivity;
                                    _n_unemployed += h.Unemployed ? 1 : 0;
                                }
                            }


                            using (StreamWriter sw = File.AppendText(_settings.ROutputDir + "\\data_year.txt"))
                            {
                                sw.WriteLine("{0:#.##}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t" +
                                    "{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}\t{20}\t{21}\t{22}\t{23}\t{24}\t{25}\t{26}", 
                                    1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear,
                                    _simulation.Households.Count, prod_avr, nUnemp, tot_opt_l, P_star, _totalEmployment, 
                                    tot_vacancies, _marketWageTotal, _marketPriceTotal, _totalSales, _profitPerHousehold,
                                    n_firms, _expProfit, mean_age, _meanValue, _nFirmCloseNatural, 
                                    _nFirmCloseNegativeProfit, _nFirmCloseTooBig, _nFirmNew, _discountedProfits, 
                                    _expDiscountedProfits, _sharpeRatioTotal, _expSharpeRatioTotal, laborSupply, _yr_consumption, _yr_employment);
                                sw.Flush();

                            }

                            //using (StreamWriter sw = File.AppendText(_settings.ROutputDir + "\\sectors_year.txt"))
                            //{
                            //    for (int i = 0; i < _settings.NumberOfSectors; i++)
                            //    {
                            //        sw.WriteLineTab(1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear, i,
                            //        _marketPrice[i], _marketWage[i], _marketPriceTotal, _marketWageTotal, _employment[i], _production[i],
                            //        _sales[i], _expSharpeRatio[i], _simulation.Sector(i).Count);
                            //    }
                            //    sw.Flush();
                            //}


                            using (StreamWriter sw = File.CreateText(_settings.ROutputDir + "\\data_households.txt"))
                            {
                                sw.WriteLine("UnemplDuration\tProductivity\tAge\tGood\tShopGood\tPrice\tWage");
                                foreach (Household h in _simulation.Households)
                                {
                                    double w = h.FirmEmployment!=null ? h.FirmEmployment.Wage : 0;
                                    sw.WriteLineTab(h.UnemploymentDuration, h.Productivity, h.Age, 0, 0, h.CES_Price, w);
                                    //sw.WriteLineTab(h.UnemploymentDuration, h.Productivity, h.Age, h.Good, h.FirmShop.Good, h.FirmShop.Price, w);
                                }
                            }

                            RunRScript("..\\..\\..\\R\\graphs.R");

                        }
                    #endregion

                    // Shock: Productivity shock
                    if (_time.Now == _settings.ShockPeriod)
                    {
                        if(_settings.Shock==EShock.Productivity)
                            _macroProductivity = 1.1;

                        if (_settings.Shock == EShock.ProductivitySector0)
                            _sectorProductivity[0] = 1.1;

                    }

                    int nFirmClosed = _nFirmCloseNatural + _nFirmCloseNegativeProfit + _nFirmCloseTooBig + _nFirmCloseZeroEmployment;
                    _fileMacro.WriteLineTab(_scenario_id, _runName, _time.Now, _expSharpeRatioTotal, _macroProductivity, _marketPriceTotal, _marketWageTotal,
                                                n_firms, _totalEmployment, _totalSales, _laborSupply, _n_laborSupply, _n_unemployed,
                                                _totalProduction, _simulation.Households.Count, _nFirmNew, nFirmClosed, _sigmaRiskTotal, _sharpeRatioTotal, 
                                                mean_age, tot_vacancies, _marketPrice[0], _marketWage[0], _employment[0], _sales[0], 
                                                _simulation.Sector(0).Count, _expSharpeRatio[0]);
                    _fileMacro.Flush();

                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                    {
                        _fileSectors.WriteLineTab(_scenario_id, _runName, _time.Now, i,
                        _marketPrice[i], _marketWage[i], _marketPriceTotal, _marketWageTotal, _employment[i], _production[i],
                        _sales[i], _expSharpeRatio[i], _simulation.Sector(i).Count);
                    }                    
                    _fileSectors.Flush();
                




                    if (_time.Now==_settings.StatisticsOutputPeriode)
                    {
                        using (StreamWriter sw = File.AppendText(_settings.ROutputDir + "\\output.txt"))
                        {
                            sw.WriteLineTab(n_firms, _marketPrice, _marketWage, _discountedProfits);
                        }
                    }

                    _nFirmCloseNatural = 0;
                    _nFirmCloseTooBig = 0;
                    _nFirmCloseNegativeProfit = 0;
                    _nFirmCloseZeroEmployment = 0;
                    _nFirmNew = 0;
                    break;

                case Event.System.Stop:
                    CloseFiles();
                    if(!_settings.SaveScenario)
                    {
                        Console.WriteLine("\nRunning R-scripts:");

                        Console.WriteLine("-- macro.R..");
                        RunRScript("..\\..\\..\\R\\macro.R");

                        Console.WriteLine("-- macro_q.R..");
                        RunRScript("..\\..\\..\\R\\macro_q.R");

                        Console.WriteLine("-- firm_reports.R..");
                        RunRScript("..\\..\\..\\R\\firm_reports.R");
                    }
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }
        #endregion
            
        #region Communicate
        public void Communicate(EStatistics comID, object o)
        {
            switch (comID)
            {
                case EStatistics.FirmCloseNatural:
                    _nFirmCloseNatural++;
                    return;

                case EStatistics.FirmCloseTooBig:
                    _nFirmCloseTooBig++;
                    return;

                case EStatistics.FirmCloseNegativeProfit:
                    _nFirmCloseNegativeProfit++;
                    return;

                case EStatistics.FirmCloseZeroEmployment:
                    _nFirmCloseZeroEmployment++;
                    return;
                case EStatistics.FirmNew:
                    _nFirmNew += (double)o;
                    return;

                default:
                    return;
            }
        }
        #endregion

        #region Internal methods
        #region Write()
        void Write()
        {
            _fileDBStatistics.WriteLineTab(_expSharpeRatioTotal, _macroProductivity, _marketPrice, _marketWage);

        }
        #endregion

        #region RunRScript()
        void RunRScript(string fileName)
        {

            Process r = new();

            r.StartInfo.FileName = _settings.RExe;
            r.StartInfo.Arguments = "CMD BATCH " + fileName;
            r.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            r.Start();
            r.WaitForExit();

            //            Thread.Sleep(100);

        }
        #endregion

        #region OpenFiles()
        void OpenFiles()
        {
            if (!_settings.SaveScenario)
            {
                string path = _settings.ROutputDir + "\\data_year.txt";
                if (File.Exists(path)) File.Delete(path);
                using (StreamWriter sw = File.CreateText(path))
                    sw.WriteLine("Year\tn_Households\tavr_productivity\tnUnemployed\tnOptimalEmplotment\tP_star\tnEmployment\tnVacancies\tWage\tPrice\t" +
                        "Sales\tProfitPerHousehold\tnFirms\tProfitPerFirm\tMeanAge\tMeanValue\tnFirmCloseNatural\tnFirmCloseNegativeProfit\tnFirmCloseTooBig\t" +
                        "nFirmNew\tDiscountedProfits\tExpDiscountedProfits\tSharpeRatio\tExpSharpRatio\tLaborSupply\tYearConsumption\tYearEmployment");

                //path = _settings.ROutputDir + "\\sectors_year.txt";
                //if (File.Exists(path)) File.Delete(path);
                //using (StreamWriter sw = File.CreateText(path))
                //    sw.WriteLine("Year\tSector\tPrice\tWage\tPriceTotal\tWageTotal\tEmployment\tProduction\tSales\tExpShapeRatio\tnFirm"); 

                path = _settings.ROutputDir + "\\file_reports.txt"; // Ret til firms !!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (File.Exists(path)) File.Delete(path);
                _fileFirmReport = File.CreateText(path);
                _fileFirmReport.WriteLine("Time\tID\tProductivity\tEmployment\tProduction\tSales\tVacancies\tExpectedPrice\tExpectedWage\tPrice\tWage\tApplications" +
                    "\tQuitters\tProfit\tValue\tPotensialSales\tOptimalEmployment\tOptimalProduction\tExpectedSales");

                path = _settings.ROutputDir + "\\household_reports.txt";
                if (File.Exists(path)) File.Delete(path);
                _fileHouseholdReport = File.CreateText(path);
                _fileHouseholdReport.WriteLine("Time\tID\tProductivity\tAge\tConsumption\tValConsumption\tIncome");

                path = _settings.ROutputDir + "\\output.txt";
                if (!File.Exists(path))
                    using (StreamWriter sw = File.CreateText(path))
                        sw.WriteLine("n_firms\tPrice\tWage\tDiscountedProfits");

            }

            string macroPath = _settings.ROutputDir + "\\macro.txt";
            string sectorsPath = _settings.ROutputDir + "\\sectors.txt";

            #region Scenario-stuff
            if (_settings.SaveScenario)
            {
                Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios");
                Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios\\Macro");
                Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios\\Sectors");

                string scnPath = _settings.ROutputDir + "\\scenario_info.txt";
                if (_settings.Shock == EShock.Nothing) // Base run
                {
                    if (!File.Exists(scnPath))
                        _scenario_id = 1;
                    else
                    {
                        using (StreamReader sr = File.OpenText(scnPath))
                            _scenario_id = Int32.Parse(sr.ReadLine());
                        _scenario_id++;
                        Console.WriteLine("Base: {0}, {1}", _scenario_id, _simulation.Seed); // Save seed so it can be used in shocks

                    }

                    if (File.Exists(scnPath)) File.Delete(scnPath);
                    using (StreamWriter sw = File.CreateText(scnPath))
                    {
                        sw.WriteLine("{0}", _scenario_id);
                        sw.WriteLine("{0}", _simulation.Seed);
                    }

                    macroPath = _settings.ROutputDir + "\\Scenarios\\Macro\\base_" + _scenario_id.ToString() + ".txt";
                    sectorsPath = _settings.ROutputDir + "\\Scenarios\\Sectors\\base_" + _scenario_id.ToString() + ".txt";

                }
                else //Counterfactual
                {
                    
                    using (StreamReader sr = File.OpenText(scnPath))
                        _scenario_id = Int32.Parse(sr.ReadLine());

                    _runName = _settings.Shock.ToString();                                    
                    macroPath = _settings.ROutputDir + "\\Scenarios\\Macro\\count_"  + _runName + "_" + _scenario_id.ToString() + ".txt";
                    sectorsPath = _settings.ROutputDir + "\\Scenarios\\Sectors\\count_" + _runName + "_" + _scenario_id.ToString() + ".txt";
                    Console.WriteLine("{0}: {1}, {2}", _runName, _scenario_id, _simulation.Seed);

                }
            }
            #endregion

            if (File.Exists(macroPath)) File.Delete(macroPath);
            _fileMacro = File.CreateText(macroPath);
            _fileMacro.WriteLine("Scenario\tRun\tTime\texpSharpeRatio\tmacroProductivity\tmarketPrice\t" +
                   "marketWage\tnFirms\tEmployment\tSales\tLaborSupply\tnLaborSupply\tnUnemployed\t" +
                   "Production\tnHouseholds\tnFirmNew\tnFirmClosed\tSigmaRisk\tSharpeRatio\tMeanAge\t" +
                   "Vacancies\tmarketPrice0\tmarketWage0\temployment0\tsales0\tnFirm0\texpShapeRatio0");

            if (File.Exists(sectorsPath)) File.Delete(sectorsPath);
            _fileSectors = File.CreateText(sectorsPath);
            _fileSectors.WriteLine("Scenario\tRun\tTime\tSector\tPrice\tWage\tPriceTotal\tWageTotal\tEmployment\tProduction\tSales\tExpShapeRatio\tnFirm");



        }
        #endregion

        #region CloseFiles()
        void CloseFiles()
        {
            if (!_settings.SaveScenario)
            {
                _fileFirmReport.Close();
                _fileHouseholdReport.Close();
                _fileMacro.Close();
                _fileSectors.Close();
            }
        }
        #endregion
        #endregion

        #region Public proporties
        public double[] PublicMarketWage
        {
            get { return _marketWage; }
        }
        public double PublicMarketWageTotal
        {
            get { return _marketWageTotal; }
        }
        public double PublicMarketPriceTotal
        {
            get { return _marketPriceTotal; }
        }

        public double[] PublicMarketPrice
        {
            get { return _marketPrice; }
        }

        public double PublicProductivity
        {
            get { return _macroProductivity; }
        }
        public double[] PublicSectorProductivity
        {
            get { return _sectorProductivity; }
        }
        public double PublicProfitPerHousehold
        {
            get { return _profitPerHousehold; }
        }
        public double PublicMeanValue
        {
            get { return _meanValue; }
        }

        public double PublicExpectedProfitPerFirm
        {
            get { return _expProfit; }
        }
        public double PublicDiscountedProfits
        {
            get { return _discountedProfits; }
        }
        public double PublicExpectedDiscountedProfits
        {
            get { return _expDiscountedProfits; }
        }
        public double PublicExpectedSharpRatioTotal
        {
            get { return _expSharpeRatioTotal; }
        }
        public double[] PublicExpectedSharpRatio
        {
            get { return _expSharpeRatio; }
        }
        public double PublicSharpRatioTotal
        {
            get { return _sharpeRatioTotal; }
        }
        public double PublicInterestRate
        {
            get { return _interestRate; }
        }

        public StreamWriter StreamWriterFirmReport
        {
            get { return _fileFirmReport; }
        }
        public StreamWriter StreamWriterHouseholdReport
        {
            get { return _fileHouseholdReport; }
        }
        public StreamWriter StreamWriterDBHouseholds
        {
            get { return _fileDBHouseholds; }
        }
        public StreamWriter StreamWriterDBFirms
        {
            get { return _fileDBFirms; }
        }

        #endregion

    }
}



