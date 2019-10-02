using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ASCOM.EQ500X
{
    public class MechanicalPoint
    {
        // One degree, one arcminute, one arcsecond
        const double ONEDEGREE = 1.0;
        //const double ARCMINUTE = ONEDEGREE / 60.0;
        const double ARCSECOND = ONEDEGREE / 3600.0;

        // This is the minimum detectable movement in RA/DEC
        readonly double RA_GRANULARITY = Math.Round((15.0 * ARCSECOND) * 3600.0, MidpointRounding.AwayFromZero) / 3600.0;
        readonly double DEC_GRANULARITY = Math.Round((1.0 * ARCSECOND) * 3600.0, MidpointRounding.AwayFromZero) / 3600.0;

        const string MechanicalPoint_DEC_FormatR = "+DD:MM:SS";
        //const string MechanicalPoint_DEC_FormatW = "+DDD:MM:SS";
        const string MechanicalPoint_RA_Format = "HH:MM:SS";

        private readonly IFormatProvider formatProvider = new CultureInfo("en-GB");

        public MechanicalPoint(double ra, double dec)
        {
            RAm = ra;
            DECm = dec;
        }
        public MechanicalPoint()
        {
            _RAm = 0 * 3600;
            _DECm = 0 * 3600;
        }
        public bool atParkingPosition()
        {
            // Consider mount 0/0 is pole - no way to check if synced already
            return _RAm == 0 && _DECm == 0;
        }


        public double RAm
        {
            get
            {
                return ((double)_RAm) / 3600.0;
            }
            set
            {
                _RAm = (long)Math.Round(((value + 24.0) % 24.0) * 3600.0, MidpointRounding.AwayFromZero);
                Debug.Assert(0 <= _RAm && _RAm <= 24 * 3600);
            }
        }
        public double DECm
        {
            get
            {
                return ((double)_DECm) / 3600.0;
            }
            set
            {
                // Should be inside [-180,+180] but mount supports a larger (not useful) interval
                _DECm = (long)Math.Round((value % 256.0) * 3600.0, MidpointRounding.AwayFromZero);

                // Deduce pier side from mechanical DEC
                if ((-256 * 3600 < _DECm && _DECm < -180 * 3600) || (0 <= _DECm && _DECm <= +180 * 3600))
                    _pointingState = PointingStates.POINTING_NORMAL;
                else
                    _pointingState = PointingStates.POINTING_BEYOND_POLE;
            }
        }


        public double RAsky
        {
            get
            {
                switch (_pointingState)
                {
                    case PointingStates.POINTING_BEYOND_POLE:
                        return (double)((12 * 3600 + _RAm) % (24 * 3600)) / 3600.0;
                    case PointingStates.POINTING_NORMAL:
                    default:
                        return (double)((24 * 3600 + _RAm) % (24 * 3600)) / 3600.0;
                }
            }
            set
            {
                switch (_pointingState)
                {
                    case PointingStates.POINTING_BEYOND_POLE:
                        _RAm = (long)(((12.0 + value) % 24.0) * 3600.0);
                        break;
                    case PointingStates.POINTING_NORMAL:
                        _RAm = (long)(((24.0 + value) % 24.0) * 3600.0);
                        break;
                }
            }
        }
        public double DECsky
        {
            get
            {
                // Convert to sky DEC inside [-90,90], allowing +/-90 values
                long _DEC = 90 * 3600 - _DECm;
                if (PointingStates.POINTING_BEYOND_POLE == _pointingState)
                    _DEC = 180 * 3600 - _DEC;
                while (+90 * 3600 < _DEC) _DEC -= 180 * 3600;
                while (_DEC < -90 * 3600) _DEC += 180 * 3600;
                return (double)(_DEC) / 3600.0;
            }
            set
            {
                switch (_pointingState)
                {
                    case PointingStates.POINTING_BEYOND_POLE:
                        _DECm = 90 * 3600 - (long)Math.Round((180.0 - value) * 3600.0, MidpointRounding.AwayFromZero);
                        break;
                    case PointingStates.POINTING_NORMAL:
                        _DECm = 90 * 3600 - (long)Math.Round(value * 3600.0, MidpointRounding.AwayFromZero);
                        break;
                }
            }
        }

        public enum PointingStates { POINTING_NORMAL, POINTING_BEYOND_POLE }

        public PointingStates PointingState
        {
            get { return _pointingState; }
            set { _pointingState = value; }
        }

        public bool parseStringDEC(string s)
        {
            if (s is null || s.Length < MechanicalPoint_DEC_FormatR.Length - 1)
                return true;

            // Mount replies to "#GD:" with "sDD:MM:SS".
            // s is in {+,-} and provides a sign.
            // DD are degrees, unit D spans '0' to '9' in [0,9] but high D spans '0' to 'I' in [0,25].
            // MM are minutes, SS are seconds in [00:00,59:59].
            // The whole reply is in [-255:59:59,+255:59:59].

            // For flexibility, we allow parsing of +###:##:## instead of the spec +##:##:##

            Match dms = Regex.Match(s, @"([+\-]([\d:;<=>\?@A-I])\d{1,2})[^\d](\d{2})[^\d](\d{2})");
            if (!dms.Success)
                return true;

            int degrees = 0;
            if (4 == dms.Groups[1].Value.Length)
            {
                // Sign is processed when DMS is consolidated
                degrees += int.Parse(dms.Groups[1].Value.Substring(1, 3), formatProvider);
            }
            else
            {
                switch (dms.Groups[1].Value[1])
                {
                    case ':': degrees += 100; break;
                    case ';': degrees += 110; break;
                    case '<': degrees += 120; break;
                    case '=': degrees += 130; break;
                    case '>': degrees += 140; break;
                    case '?': degrees += 150; break;
                    case '@': degrees += 160; break;
                    case 'A': degrees += 170; break;
                    case 'B': degrees += 180; break;
                    case 'C': degrees += 190; break;
                    case 'D': degrees += 200; break;
                    case 'E': degrees += 210; break;
                    case 'F': degrees += 220; break;
                    case 'G': degrees += 230; break;
                    case 'H': degrees += 240; break;
                    case 'I': degrees += 250; break;
                    default: degrees += 10 * int.Parse(dms.Groups[1].Value[1].ToString(formatProvider), formatProvider); break;
                }
                degrees += int.Parse(dms.Groups[1].Value[2].ToString(formatProvider), formatProvider);
            }
            int minutes = int.Parse(dms.Groups[3].Value, formatProvider);
            int seconds = int.Parse(dms.Groups[4].Value, formatProvider);

            _DECm = (dms.Groups[1].Value[0] == '-' ? -1 : +1) * (degrees * 3600 + minutes * 60 + seconds);

            // Deduce pointing state from mechanical DEC
            if ((-256 * 3600 < _DECm && _DECm <= -180 * 3600) || (0 <= _DECm && _DECm <= +180 * 3600))
                _pointingState = PointingStates.POINTING_NORMAL;
            else
                _pointingState = PointingStates.POINTING_BEYOND_POLE;

            return false;
        }
        public bool parseStringRA(string s)
        {
            if (s is null || s.Length < MechanicalPoint_RA_Format.Length - 1)
                return true;

            // Mount replies to "#GR:" with "HH:MM:SS".
            // HH, MM and SS are respectively hours, minutes and seconds in [00:00:00,23:59:59].
            // FIXME: Sanitize.

            Match m = Regex.Match(s, @"(\d{2})[^\d](\d{2})[^\d](\d{2})");
            if (m.Success)
            {
                int hours = int.Parse(m.Groups[1].Value, formatProvider);
                int minutes = int.Parse(m.Groups[2].Value, formatProvider);
                int seconds = int.Parse(m.Groups[3].Value, formatProvider);

                //_RAm = (    (_pierSide == PIER_WEST ? -12*3600 : +0) + 24*3600 +
                _RAm = (24 * 3600 + (hours % 24) * 3600 + minutes * 60 + seconds) % (24 * 3600);
                return false;
            }

            return true;
        }
        public string toStringDEC(ref string s)
        {
            // See /test/test_eq500xdriver.cpp for description of DEC conversion

            char sign = _DECm < 0 ? '-' : '+';
            int degrees = (int)(Math.Abs(_DECm) / 3600) % 256;
            int minutes = (int)(Math.Abs(_DECm) / 60) % 60;
            int seconds = (int)(Math.Abs(_DECm)) % 60;

            if (degrees < -255 || +255 < degrees)
                return null;

            return s = $"{sign}{degrees:000}:{minutes:00}:{seconds:00}";
        }
        public string toStringRA(ref string s)
        {
            // See /test/test_eq500xdriver.cpp for description of RA conversion

            //int const hours = ((_pierSide == PIER_WEST ? 12 : 0) + 24 + static_cast <int> (_RAm/3600)) % 24;
            int hours = (24 + (int)(_RAm / 3600)) % 24;
            int minutes = (int)(_RAm / 60) % 60;
            int seconds = (int)(_RAm) % 60;

            return s = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        public string toStringDEC_Sim(ref string s)
        {
            // See /test/test_eq500xdriver.cpp for description of DEC conversion

            char sign = _DECm < 0 ? '-' : '+';
            int degrees = (int)(Math.Abs(_DECm) / 3600) % 256;
            int minutes = (int)(Math.Abs(_DECm) / 60) % 60;
            int seconds = (int)(Math.Abs(_DECm)) % 60;

            if (degrees < -255 || +255 < degrees)
                return null;

            char high_digit;
            if (-100 < degrees && degrees < 100)
            {
                high_digit = (char)((int)'0' + Math.Abs(degrees) / 10);
            }
            else switch (Math.Abs(degrees) / 10)
                {
                    case 10: high_digit = ':'; break;
                    case 11: high_digit = ';'; break;
                    case 12: high_digit = '<'; break;
                    case 13: high_digit = '='; break;
                    case 14: high_digit = '>'; break;
                    case 15: high_digit = '?'; break;
                    case 16: high_digit = '@'; break;
                    case 17: high_digit = 'A'; break;
                    case 18: high_digit = 'B'; break;
                    case 19: high_digit = 'C'; break;
                    case 20: high_digit = 'D'; break;
                    case 21: high_digit = 'E'; break;
                    case 22: high_digit = 'F'; break;
                    case 23: high_digit = 'G'; break;
                    case 24: high_digit = 'H'; break;
                    case 25: high_digit = 'I'; break;
                    default: return null;
                }

            char low_digit = (char)((int)'0' + (Math.Abs(degrees) % 10));

            return s = string.Format(formatProvider, "{0}{1}{2}:{3:D2}:{4:D2}", sign, high_digit, low_digit, minutes, seconds);
        }

        public double RA_degrees_to(MechanicalPoint b)
        {
            Contract.Requires(!(b is null));

            // RA is circular, DEC is not
            // We have hours and not degrees because that's what the mount is handling in terms of precision
            // We need to be cautious, as if we were to use real degrees, the RA movement would need to be 15 times more precise
            long delta = b._RAm - _RAm;
            if (delta > +12 * 3600) delta -= 24 * 3600;
            if (delta < -12 * 3600) delta += 24 * 3600;
            return (double)(delta * 15) / 3600.0;
        }
        public double DEC_degrees_to(MechanicalPoint b)
        {
            Contract.Requires(!(b is null));

            // RA is circular, DEC is not
            return (b._DECm - _DECm) / 3600.0;
        }

        public static double operator -(MechanicalPoint a, MechanicalPoint b) => Subtract(a, b);

        public static double Subtract(MechanicalPoint a, MechanicalPoint b)
        {
            Contract.Requires(!(a is null));
            Contract.Requires(!(b is null));

            double ra_distance = a.RA_degrees_to(b);
            double dec_distance = a.DEC_degrees_to(b);
            // FIXME: Incorrect distance calculation, but enough for our purpose
            return Math.Sqrt(ra_distance * ra_distance + dec_distance * dec_distance);
        }

        public static bool operator !=(MechanicalPoint a, MechanicalPoint b) => (a is null || b is null) ? false : !a.Equals(b);

        public static bool operator ==(MechanicalPoint a, MechanicalPoint b) => (a is null || b is null) ? false : a.Equals(b);

        public override bool Equals(object o)
        {
            return (o is MechanicalPoint b) && !((_pointingState != b._pointingState) || (RA_GRANULARITY <= Math.Abs(RA_degrees_to(b))) || (DEC_GRANULARITY <= Math.Abs(DEC_degrees_to(b))));
        }

        public override int GetHashCode()
        {
            return Tuple.Create(PointingState, _RAm, _DECm).GetHashCode();
        }

        private PointingStates _pointingState;
        private long _RAm, _DECm;
    }
}
