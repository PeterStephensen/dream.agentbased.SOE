using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dream.Models.SOE_Basic
{

    public class Settings
    {
        public int NumberOfHouseholdsPerFirm { get; set; } = 0;
        public bool FirmStartNewFirms { get; set; } = false; 

        public int NumberOfFirms { get; set; } = 0;
        /// <summary>
        /// Minimum productivity in pareto distribution
        /// </summary>
        public double FirmParetoMinPhi { get; set; } = 0.5;
        /// <summary>
        /// k-parameter in pareto productivity distribution
        /// </summary>
        public double FirmPareto_k { get; set; } = 2;
        /// <summary>
        /// Initial minimum productivity in pareto distribution
        /// </summary>
        public double FirmParetoMinPhiInitial { get; set; } = 0.5;

        /// <summary>
        /// Decreasing returns to scale parameter
        /// </summary>
        public double FirmAlpha { get; set; } = 0.8;
        /// <summary>
        /// Increasing returns to scale parameter
        /// </summary>
        public double FirmFi { get; set; } = 1.0;

        /// <summary>
        /// Wage markup if hard to find people (outside comfort zone)
        /// </summary>
        public double FirmWageMarkup { get; set; } = 0.001;
        public double FirmWageMarkupSensitivity { get; set; } = 1.0;

        /// <summary>
        /// Wage markup if hard to find people (in comfort zone)
        /// </summary>
        public double FirmWageMarkupInZone { get; set; } = 0.001;
        public double FirmWageMarkupSensitivityInZone { get; set; } = 1.0;


        /// <summary>
        /// Wage markdown if too many people (outside comfort zone)
        /// </summary>
        public double FirmWageMarkdown { get; set; } = 0.001;
        public double FirmWageMarkdownSensitivity { get; set; } = 1.0;

        /// <summary>
        /// Wage markdown if too many people (in comfort zone)
        /// </summary>
        public double FirmWageMarkdownInZone { get; set; } = 0.001;
        public double FirmWageMarkdownSensitivityInZone { get; set; } = 1.0;

        /// <summary>
        /// Price markup (outside comfort zone)
        /// </summary>
        public double FirmPriceMarkup { get; set; } = 0.001;
        public double FirmPriceMarkupSensitivity { get; set; } = 1.0;

        /// <summary>
        /// Price markup (in comfort zone)
        /// </summary>
        public double FirmPriceMarkupInZone { get; set; } = 0.001;
        public double FirmPriceMarkupSensitivityInZone { get; set; } = 1.0;


        /// <summary>
        /// Price markdown (outside comfort zone)
        /// </summary>
        public double FirmPriceMarkdown { get; set; } = 0.001;
        public double FirmPriceMarkdownSensitivity { get; set; } = 1.0;

        /// <summary>
        /// Price markdown (in comfort zone)
        /// </summary>
        public double FirmPriceMarkdownInZone { get; set; } = 0.001;
        public double FirmPriceMarkdownSensitivityInZone { get; set; } = 1.0;


        /// <summary>
        /// Periode when price mechanism starts
        /// </summary>
        public int FirmPriceMechanismStart { get; set; } = 0;

        /// <summary>
        /// Smoothing in expectations: xE(t) = b*xE(t-1) + (1-b)*x(t-1)
        /// </summary>
        public double FirmExpectationSmooth { get; set; } = 0.0;

        /// <summary>
        /// Maximum firm size
        /// </summary>
        public int FirmMaxEmployment { get; set; } = 10000000;

        /// <summary>
        /// If negative profit, the firm is closed with this probability
        /// </summary>
        public double FirmDefaultProbabilityNegativeProfit { get; set; } = 0.0;

        public int FirmNegativeProfitOkAge { get; set; } = 0;

        /// <summary>
        /// Exogeneous default risk
        /// </summary>
        public double FirmDefaultProbability { get; set; } = 0.0;

        /// <summary>
        /// Period where the posibility of default starts 
        /// </summary>
        public int FirmDefaultStart { get; set; } = 0;

        /// <summary>
        /// Period where the posibility of firings starts 
        /// </summary>
        public int FirmFiringsStart { get; set; } = 0;


        /// <summary>
        /// Probability of recalculating price outside comfort zone
        /// </summary>
        public double FirmProbabilityRecalculatePrice { get; set; } = 1.0;
        /// <summary>
        /// Probability of recalculating price inside comfort zone
        /// </summary>
        public double FirmProbabilityRecalculatePriceInZone { get; set; } = 1.0;

        /// <summary>
        /// Probability of recalculating wage outside comfort zone
        /// </summary>
        public double FirmProbabilityRecalculateWage { get; set; } = 1.0;
        /// <summary>
        /// Probability of recalculating wage inside comfort zone
        /// </summary>
        public double FirmProbabilityRecalculateWageInZone { get; set; } = 1.0;

        /// <summary>
        /// Productivity growth p.a. in new firms
        /// </summary>
        public double FirmProductivityGrowth { get; set; } = 0.0;

        public int FirmNumberOfNewFirms { get; set; } = 0;

        /// <summary>
        /// Duration of startup periode (= Entry Cost)
        /// </summary>
        public int FirmStartupPeriod { get; set; } = -1;

        /// <summary>
        /// Employment during startup periode
        /// </summary>
        public int FirmStartupEmployment { get; set; } = 0;

        /// <summary>
        /// The firm is in its comport zone if actual employment diviates less from optimal 
        /// </summary>
        public double FirmComfortZoneEmployment { get; set; } = 0.0;

        /// <summary>
        /// The firm is in its comport zone if actual sales diviates less from optimal production 
        /// </summary>
        public double FirmComfortZoneSales { get; set; } = 0.0;

        /// <summary>
        /// From this period, the profit limit os zero
        /// </summary>
        public double FirmProfitLimitZeroPeriod { get; set; } = 0.0;

        /// <summary>
        /// Proportion of optimal vacancies advertised 
        /// </summary>
        public double FirmVacanciesShare { get; set; } = 0.1;
        public int FirmMinRemainingVacancies { get; set; } = 0;
        

        //---------------------------------
        /// <summary>
        /// Number of firms contacted when searching for job
        /// </summary>
        public int HouseholdNumberFirmsSearchJob { get; set; } = 10;
        public int HouseholdNumberFirmsSearchShop { get; set; } = 5;
        public int HouseholdMaxNumberShops { get; set; } = 5;
        public double HouseholdProbabilityQuitJob { get; set; } = 0;
        public double HouseholdProbabilitySearchForJob { get; set; } = 0;
        public double HouseholdProbabilitySearchForShop { get; set; } = 0.01;
        public int HouseholdPensionAge { get; set; } = 0;
        public int HouseholdStartAge { get; set; } = 0;
        /// <summary>
        /// The number of new housholds each period
        /// </summary>
        public int HouseholdNewBorn { get; set; } = 0;

        /// <summary>
        /// Mean in log-normal productivity distribution (initial population)
        /// </summary>
        public double HouseholdProductivityLogMeanInitial { get; set; } = 0.0;
        /// <summary>
        /// Standard deviation in log-normal productivity distribution (initial population)
        /// </summary>
        public double HouseholdProductivityLogSigmaInitial { get; set; } = 0.3;
        /// <summary>
        /// Standard deviation in error term in dynamic productivity equation
        /// </summary>
        public double HouseholdProductivityErrorSigma { get; set; } = 0;

        /// <summary>
        /// Initial size of investor firm portefolio
        /// </summary>
        public int InvestorInitialInflow { get; set; } = 0;

        public double InvestorProfitSensitivity { get; set; } = 0;


        public double StatisticsInitialMarketPrice { get; set; } = 1.0;    
        public double StatisticsInitialMarketWage { get; set; } = 1.0;
        /// <summary>
        /// Interest rate usend to calulate discounted profits
        /// </summary>
        public double StatisticsInitialInterestRate { get; set; } = 0.0;

        /// <summary>
        /// The share of firms that is randomly picked to report monthly data 
        /// </summary>
        public double StatisticsFirmReportSampleSize { get; set; } = 0.0;

        /// <summary>
        /// The share of households that is randomly picked to report monthly data 
        /// </summary>
        public double StatisticsHouseholdReportSampleSize { get; set; } = 0.0;

        /// <summary>
        /// If this is x, graphics is plottet every x periods
        /// </summary>
        public int StatisticsGraphicsPlotInterval { get; set; } = 0;

        public int StatisticsGraphicsStartPeriod { get; set; } = 0;


        public int StatisticsOutputPeriode { get; set; } = -1;

        public double StatisticsExpectedSharpeRatioSmooth { get; set; } = 0.0;

        /// <summary>
        /// In this periode all agents are written to a data base
        /// </summary>
        public int StatisticsWritePeriode { get; set; } = -1;


        /// <summary>
        /// Seed for the random generator. Should be positive.
        /// </summary>
        public int RandomSeed { get; set; } = -1;

        public EShock Shock = EShock.Nothing;
        public int PeriodsPerYear { get; set; } = 1;
        /// <summary>
        /// Macro data is recorded and put in the scenario-folder with a scnario-id
        /// </summary>
        public bool SaveScenario { get; set; } = false;

        public int StartYear { get; set; } = 0;
        public int EndYear { get; set; } = 10;
        public int ShockPeriod { get; set; } = -1;
        public int BurnInPeriod1 { get; set; } = -1;
        public int BurnInPeriod2 { get; set; } = -1;
        public double Scale { get; set; } = 1;
        public bool LoadDatabase { get; set; } = false;

        public string RCodeDir { get; set; } = @"..\..\..\R";
        public string ROutputDir { get; set; } = "";
        public string RExe { get; set; } = "";
        public int IDScenario { get; set; } = 0;

    }
}
