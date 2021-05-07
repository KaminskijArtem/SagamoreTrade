using System.Collections.Generic;

namespace QuartzScheduler.Base
{
    public class GlobalValues
    {
        public static List<string> symbols = new List<string>
        {
            "US30", "US500",  "US100", "DE30", "EU50", "UK100", "FR40", "NL25",
            "AAL", "AAPL", "AMD", "AIRfr", "AMZN", "BA", "BYND", "CCL", "DAL", "EPAM",
            "FB", "LHA", "MSFT", "NCLH", "RCL", "TLRY", "WLL"
        };
    }

    public class NoLeverageGlobalValues
    {
        public static List<string> symbols = new List<string>
        {
            "ONEM", "VNET", "AES", "AIEQ", "AIG", "AIV", "AMC", "T", "AVRO", "ABCL",
            "ANF", "ADPT", "ADNT", "YOLO", "AA", "ALSN", "ATUS", "MO", "AAL", "AEO",
            "AMH", "AMRS", "AMX", "AMOV", "AR", "APA", "ARMK", "MTus", "ARNC", "ARCT",
            "ASAN", "AVT", "AXTA", "BP", "BKR", "BLDP", "BBD", "BAC", "BZUN", "BBBY",
            "BRBR", "BB", "HRB", "BE", "BLUE", "BSX", "BEDU", "BTI", "BRX", "CNX",
            "CRON", "COG", "CPE", "CPB", "CNQ", "GOEV", "CCL", "CNP", "CC", "CNK",
            "CFG", "CLF", "CLVS", "TPR", "COMM", "DXC", "CAG", "WISH", "CLR", "CLB",
            "GLW", "CPNG", "COUR", "DISH", "PLAY", "DK", "DAL", "DVN", "JDST", "DISCA",
            "DBX", "DRE", "EQT", "MJ", "EOS", "ENB", "WATT", "ET", "EPD", "EXEL", "EXC",
            "FTI", "FOX", "FANH", "FTCH", "FSLY", "FNF", "FITB", "XLF", "FHN", "AG",
            "FAN", "FTXR", "FE", "FLS", "FLR", "F", "FOXA", "BEN", "FCX", "FCEL", "GFL",
            "GPS", "GE", "GNTX", "GSK", "DRIV", "BOTZ", "GOCO", "GPRO", "GDRX", "GT",
            "HPQ", "HSBC", "HAL", "HOG", "PEAK", "HTBX", "HP", "HLF", "HPE", "HFC",
            "HOLI", "HRL", "HST", "HBAN", "HUN", "HYLN", "IR", "INO", "ICPT", "IPG",
            "IVZ", "PBD", "INVH", "IRM", "ITUB", "JBLU", "JKS", "JNPR", "KBH", "KEY",
            "KIM", "KMI", "KNX", "KOPN", "KHC", "LKQ", "LYFT", "LX", "LIus", "LBTYA",
            "RIDE", "LU", "LUMN", "LAZR", "MGM", "MMP", "MPLX", "MAC", "MIC", "M", "MANU",
            "MFC", "MRO", "MARA", "MRVI", "MRVL", "MAT", "MPW", "MD", "MLCO", "MOMO",
            "MOS", "MUR", "NIO", "NRG", "NYCB", "NYT", "NOV", "NKTR", "EDU", "NWL", 
            "NLSN", "NKLA", "NI", "NOAH", "JWN", "NLOK", "NCLH", "NTNX"
        };

        public static List<string> inPositionSymbols = new List<string>
        {
            "ICPT", "FSLY"
        };
    }
}
