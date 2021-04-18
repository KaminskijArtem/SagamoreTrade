using System;

namespace TradingDataLibrary.Models
{
    public class Candle
    {
        public DateTimeOffset OpenTime { get; set;}
        public decimal Open { get; set;}
        public decimal High { get; set;}
        public decimal Low { get; set;}
        public decimal Close { get; set;}
        public long Volume { get; set;}

        public bool IsWhite()
        {
            return Close > Open;
        }

        public bool IsBlack()
        {
            return !IsWhite();
        }

        public decimal GetLowerShadowRatio()
        {
            if(IsWhite())
            {
                return (Open - Low)/GetOpenCloseDiff();
            }
            else
            {
                return (Close - Low)/GetOpenCloseDiff();
            }
        }

        public decimal GetUpperShadowRatio()
        {
            if(IsWhite())
            {
                return (High - Close)/GetOpenCloseDiff();
            }
            else
            {
                return (High - Open)/GetOpenCloseDiff();
            }
        }

        public decimal GetOpenCloseDiff()
        {
            return Math.Abs(Open - Close);
        }

        public bool IsLowerShadowBigger()
        {
            if(IsWhite())
            {
                return Open - Low > High - Open;
            }
            else
            {
                return Close - Low > High - Close;
            }
        }

        public bool IsUpperShadowBigger()
        {
            if(IsWhite())
            {
                return High - Close > Close - Low;
            }
            else
            {
                return High - Open > Open - Low;
            }
        }
    }
}
