//tabs=4
// --------------------------------------------------------------------------------
// 
// ASCOM Telescope driver for Omegon EQ500X
//
// Description:	This is an ASCOM driver supporting control of the Omegon EQ-500-X 
//				Equatorial Mount.
//
// Implements:	ASCOM Telescope interface version: 6.4
// Author:		(ED) Eric Dejouhanet <eric.dejouhanet@gmail.com>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 10-Jun-2019	ED	0.0.1	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Telescope

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace ASCOM.EQ500X
{
    //
    // Your driver's DeviceID is ASCOM.EQ500X.Telescope
    //
    // The Guid attribute sets the CLSID for ASCOM.EQ500X.Telescope
    // The ClassInterface/None addribute prevents an empty interface called
    // _EQ500X from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Telescope Driver for EQ500X.
    /// </summary>
    [Guid("c46b23bc-44b8-4734-b6dd-caab44e07e2d")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Telescope : ITelescopeV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.EQ500X.Telescope";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM Telescope Driver for EQ500X.";

        internal static string comPortProfileName = "COM Port"; // Constants used for Profile persistence
        internal static string comPortDefault = "COM1";
        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "false";

        internal static string comPort; // Variables to hold the currrent device configuration
        private double m_RightAscension;
        private double m_Declination;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal static TraceLogger tl;
        private bool isSimulated;
        private Serial m_Port;
        //private double targetRA;
        //private double targetDEC;
        //private double currentRA;
        //private double currentDEC;
        private MechanicalPoint currentMechPosition = new MechanicalPoint();
        private MechanicalPoint targetMechPosition = new MechanicalPoint();
        private readonly string MechanicalPoint_DEC_FormatR = "+DD:MM:SS";
        private readonly string MechanicalPoint_DEC_FormatW = "+DDD:MM:SS";
        private readonly string MechanicalPoint_RA_Format = "HH:MM:SS";

        private class SimEQ500X
        {
            internal String MechanicalDECStr = "+00:00:00";
            internal String MechanicalRAStr = "00°00'00";
            internal double MechanicalRA = 0;
            internal double MechanicalDEC = 0;
            internal double LST = 6;
        };

        private SimEQ500X simEQ500X = new SimEQ500X();

        private struct Location
        {
            internal static double elevation;
            internal static double latitude;
            internal static double longitude;
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EQ500X"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Telescope()
        {
            tl = new TraceLogger("", "EQ500X");
            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("Telescope", "Starting initialisation");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object

            tl.LogMessage("Telescope", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ITelescopeV3 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // Call CommandString and return as soon as it finishes
            this.CommandString(command, raw);
            // or
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
            // DO NOT have both these sections!  One or the other
        }

        public bool CommandBool(string command, bool raw)
        {
            switch (command)
            {
                case "isSimulated":
                    return isSimulated;
                case "Simulated":
                    return isSimulated = raw;
                default:
                    break;
            }

            CheckConnected("CommandBool");

            string ret = CommandString(command, raw);
            // TODO decode the return string and return true or false
            // or
            throw new ASCOM.MethodNotImplementedException("CommandBool");
            // DO NOT have both these sections!  One or the other
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // it's a good idea to put all the low level communication with the device here,
            // then all communication calls this function
            // you need something to ensure that only one command is in progress at a time

            if (isSimulated)
            {
                if ("getCurrentMechanicalPosition" == command)
                {
                    String ra = "", dec = "";
                    currentMechPosition.toStringRA(ref ra);
                    currentMechPosition.toStringDEC(ref dec);
                    return string.Format("{0} {1}", ra, dec);
                }
            }

            throw new ASCOM.MethodNotImplementedException(String.Format("CommandString - $0", command));
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    if (isSimulated)
                    {
                        LogMessage("Connected Set", "Connecting in simulation mode");
                        connectedState = true;
                        return;
                    }

                    LogMessage("Connected Set", "Connecting to port {0}", comPort);

                    m_Port = new Serial();
                    m_Port.PortName = comPort;
                    m_Port.Speed = SerialSpeed.ps9600;
                    m_Port.DataBits = 8;
                    m_Port.Parity = SerialParity.None;
                    m_Port.StopBits = SerialStopBits.One;
                    m_Port.ReceiveTimeout = 5;
                    m_Port.DTREnable = true;
                    m_Port.Handshake = SerialHandshake.None;

                    try
                    {
                        m_Port.Connected = true;
                    }
                    catch (IOException) { }

                    if (m_Port.Connected)
                    {
                        try
                        {
                            if (!ReadScopeStatus())
                                throw new FormatException();

                            connectedState = true;
                        }
                        catch (FormatException)
                        {
                            LogMessage("Connected Set", "Port {0} open, but device failed handshake", comPort);
                            m_Port.Connected = false;
                            m_Port.Dispose();
                        }
                    }
                }
                else
                {
                    connectedState = false;
                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);

                    if (null != m_Port)
                    {
                        m_Port.Connected = false;
                        m_Port.Dispose();
                        m_Port = null;
                    }
                }
            }
        }

        public bool Simulated
        {
            get
            {
                LogMessage("Simulated", "Get {0}", isSimulated);
                return isSimulated;
            }
            set
            {
                tl.LogMessage("Simulation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Simulated", true);
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "Omegon EQ500X";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ITelescope Implementation
        public void AbortSlew()
        {
            tl.LogMessage("AbortSlew", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("AbortSlew");
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                tl.LogMessage("AlignmentMode Get", "German Equatorial Mount");
                return AlignmentModes.algGermanPolar;
            }
        }

        public double Altitude
        {
            get
            {
                tl.LogMessage("Altitude", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                tl.LogMessage("ApertureArea Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                tl.LogMessage("ApertureDiameter Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                tl.LogMessage("AtHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool AtPark
        {
            get
            {
                tl.LogMessage("AtPark", "Get - " + false.ToString());
                return false;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            tl.LogMessage("AxisRates", "Get - " + Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                tl.LogMessage("Azimuth Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                tl.LogMessage("CanFindHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {
            tl.LogMessage("CanMoveAxis", "Get - " + Axis.ToString());
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary: return false;
                case TelescopeAxes.axisSecondary: return false;
                case TelescopeAxes.axisTertiary: return false;
                default: throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }
        }

        public bool CanPark
        {
            get
            {
                tl.LogMessage("CanPark", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                tl.LogMessage("CanPulseGuide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                tl.LogMessage("CanSetDeclinationRate", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                tl.LogMessage("CanSetGuideRates", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPark
        {
            get
            {
                tl.LogMessage("CanSetPark", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                tl.LogMessage("CanSetPierSide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                tl.LogMessage("CanSetRightAscensionRate", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                tl.LogMessage("CanSetTracking", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlew
        {
            get
            {
                tl.LogMessage("CanSlew", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                tl.LogMessage("CanSlewAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                tl.LogMessage("CanSlewAltAzAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                tl.LogMessage("CanSlewAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSync
        {
            get
            {
                tl.LogMessage("CanSync", "Get - True");
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                tl.LogMessage("CanSyncAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                tl.LogMessage("CanUnpark", "Get - " + false.ToString());
                return false;
            }
        }

        public double Declination
        {
            get
            {
                if (!connectedState)
                    throw new ASCOM.NotConnectedException("Declination");
                if (getCurrentMechanicalPosition(ref currentMechPosition))
                    throw new ASCOM.ValueNotSetException("Declination");
                double declination = currentMechPosition.DECsky;
                tl.LogMessage("Declination", "Get - " + utilities.DegreesToDMS(declination, ":", ":"));
                return declination;
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = 0.0;
                tl.LogMessage("DeclinationRate", "Get - " + declination.ToString());
                return declination;
            }
            set
            {
                tl.LogMessage("DeclinationRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            tl.LogMessage("DestinationSideOfPier Get", "Not implemented");
            throw new ASCOM.PropertyNotImplementedException("DestinationSideOfPier", false);
        }

        public bool DoesRefraction
        {
            get
            {
                tl.LogMessage("DoesRefraction Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
            }
            set
            {
                tl.LogMessage("DoesRefraction Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equTopocentric;
                tl.LogMessage("DeclinationRate", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            tl.LogMessage("FindHome", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("FindHome");
        }

        public double FocalLength
        {
            get
            {
                tl.LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public double GuideRateDeclination
        {
            get
            {
                tl.LogMessage("GuideRateDeclination Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
            }
            set
            {
                tl.LogMessage("GuideRateDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                tl.LogMessage("GuideRateRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
            }
            set
            {
                tl.LogMessage("GuideRateRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                tl.LogMessage("IsPulseGuiding Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("IsPulseGuiding", false);
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            tl.LogMessage("MoveAxis", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("MoveAxis");
        }

        public void Park()
        {
            tl.LogMessage("Park", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Park");
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            tl.LogMessage("PulseGuide", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("PulseGuide");
        }

        public double RightAscension
        {
            get
            {
                if (!connectedState)
                    throw new ASCOM.NotConnectedException("RightAscension");
                if (getCurrentMechanicalPosition(ref currentMechPosition))
                    throw new ASCOM.ValueNotSetException("Declination");
                double rightAscension = currentMechPosition.RAsky;
                tl.LogMessage("RightAscension", "Get - " + utilities.HoursToHMS(rightAscension));
                return rightAscension;
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = 0.0;
                tl.LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                tl.LogMessage("RightAscensionRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            tl.LogMessage("SetPark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SetPark");
        }

        public PierSide SideOfPier
        {
            get
            {
                if (!connectedState)
                    throw new ASCOM.NotConnectedException("SideOfPier");
                switch (currentMechPosition.PointingState)
                {
                    case MechanicalPoint.PointingStates.POINTING_NORMAL:
                        tl.LogMessage("SideOfPier Get", "Pointing Normal - West");
                        return PierSide.pierWest;
                    case MechanicalPoint.PointingStates.POINTING_BEYOND_POLE:
                        tl.LogMessage("SideOfPier Get", "Pointing Beyond Pole - East");
                        return PierSide.pierWest;
                    default:
                        tl.LogMessage("SideOfPier Get", "Pointing Unknown");
                        return PierSide.pierUnknown;
                }
            }
            set
            {
                if (!connectedState)
                    throw new ASCOM.NotConnectedException("SideOfPier");
                tl.LogMessage("SideOfPier Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
            }
        }

        public double SiderealTime
        {
            get
            {
                // Now using NOVAS 3.1
                double siderealTime = 0.0;
                using (var novas = new ASCOM.Astrometry.NOVAS.NOVAS31())
                {
                    var jd = utilities.DateUTCToJulian(DateTime.UtcNow);
                    novas.SiderealTime(jd, 0, novas.DeltaT(jd),
                        ASCOM.Astrometry.GstType.GreenwichApparentSiderealTime,
                        ASCOM.Astrometry.Method.EquinoxBased,
                        ASCOM.Astrometry.Accuracy.Reduced, ref siderealTime);
                }

                // Allow for the longitude
                siderealTime += SiteLongitude / 360.0 * 24.0;

                // Reduce to the range 0 to 24 hours
                siderealTime = astroUtilities.ConditionRA(siderealTime);

                tl.LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                if (!Connected)
                    throw new ASCOM.NotConnectedException("SiteElevation");
                tl.LogMessage("SiteElevation Get", String.Format("Elevation $0", Location.elevation));
                return Location.elevation;
            }
            set
            {
                if (!Connected)
                    throw new ASCOM.NotConnectedException("SiteElevation");
                tl.LogMessage("SiteElevation Set", String.Format("Set Elevation $0", value));
                Location.elevation = value;
            }
        }

        public double SiteLatitude
        {
            get
            {
                if (!Connected)
                    throw new ASCOM.NotConnectedException("SiteLatitude");
                tl.LogMessage("SiteElevation Get", String.Format("Elevation $0", Location.latitude));
                return Location.latitude;
            }
            set
            {
                if (!Connected)
                    throw new ASCOM.NotConnectedException("SiteLatitude");
                tl.LogMessage("SiteLatitude Set", String.Format("Latitude $0", Location.latitude));
                Location.latitude = value;
            }
        }

        public double SiteLongitude
        {
            get
            {
                if (!Connected)
                    throw new ASCOM.NotConnectedException("SiteLongitude");
                tl.LogMessage("SiteLongitude Get", String.Format("Elevation $0", Location.longitude));
                return Location.longitude;
            }
            set
            {
                if (!Connected)
                    throw new ASCOM.NotConnectedException("SiteLongitude");

                tl.LogMessage("SiteLongitude Set", String.Format("Longitude $0", value));
                Location.longitude = value;

                if (isSimulated)
                    simEQ500X.LST = 0.0 + value / 15.0;

                if (!getCurrentMechanicalPosition(ref currentMechPosition) && currentMechPosition.atParkingPosition())
                {
                    double LST = isSimulated ? simEQ500X.LST : SiderealTime;
                    Sync(LST - 6, currentMechPosition.DECsky);
                    tl.LogMessage("SiteLongitude Set", String.Format("Location updated: mount considered parked, synced to LST $0", utilities.HoursToHMS(LST)));
                }
            }
        }

        public short SlewSettleTime
        {
            get
            {
                tl.LogMessage("SlewSettleTime Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
            }
            set
            {
                tl.LogMessage("SlewSettleTime Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            tl.LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            tl.LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            tl.LogMessage("SlewToCoordinates", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToCoordinates");
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            tl.LogMessage("SlewToCoordinatesAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToCoordinatesAsync");
        }

        public void SlewToTarget()
        {
            tl.LogMessage("SlewToTarget", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToTarget");
        }

        public void SlewToTargetAsync()
        {
            tl.LogMessage("SlewToTargetAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToTargetAsync");
        }

        public bool Slewing
        {
            get
            {
                tl.LogMessage("Slewing Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Slewing", false);
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            tl.LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            tl.LogMessage("SyncToCoordinates", String.Format("Set RA:$0 DEC:$1", RightAscension, Declination));
            if (!Sync(RightAscension, Declination))
                throw new ASCOM.DriverException("SyncToCoordinates");
        }

        public void SyncToTarget()
        {
            tl.LogMessage("SyncToTarget", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToTarget");
        }

        public double TargetDeclination
        {
            get
            {
                tl.LogMessage("TargetDeclination Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetDeclination", false);
            }
            set
            {
                tl.LogMessage("TargetDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetDeclination", true);
            }
        }

        public double TargetRightAscension
        {
            get
            {
                tl.LogMessage("TargetRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", false);
            }
            set
            {
                tl.LogMessage("TargetRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", true);
            }
        }

        public bool Tracking
        {
            get
            {
                bool tracking = true;
                tl.LogMessage("Tracking", "Get - " + tracking.ToString());
                return tracking;
            }
            set
            {
                tl.LogMessage("Tracking Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Tracking", true);
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                tl.LogMessage("TrackingRate Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TrackingRate", false);
            }
            set
            {
                tl.LogMessage("TrackingRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TrackingRate", true);
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                ITrackingRates trackingRates = new TrackingRates();
                tl.LogMessage("TrackingRates", "Get - ");
                foreach (DriveRates driveRate in trackingRates)
                {
                    tl.LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                DateTime utcDate = DateTime.UtcNow;
                tl.LogMessage("TrackingRates", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                tl.LogMessage("UTCDate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
            }
        }

        public void Unpark()
        {
            tl.LogMessage("Unpark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Unpark");
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Telescope";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        #endregion

        #region Mount functionalities

        private bool ReadScopeStatus()
        {
            if (getCurrentMechanicalPosition(ref currentMechPosition))
            {
                LogMessage("ReadScopeStatus", "Reading scope status failed");
                return false;
            }

            return true;
        }

        private bool Sync(double ra, double dec)
        {
            targetMechPosition.RAsky = ra;
            targetMechPosition.DECsky = dec;

            if (!setTargetMechanicalPosition(targetMechPosition))
            {
                if (!isSimulation())
                {
                    String b = "";

                    if (getCommandString(ref b, ":CM#") < 0)
                        goto sync_error;
                    if ("No name" == b)
                        goto sync_error;
                }
                else
                {
                    targetMechPosition.toStringRA(ref simEQ500X.MechanicalRAStr);
                    targetMechPosition.toStringDEC_Sim(ref simEQ500X.MechanicalDECStr);
                    simEQ500X.MechanicalRA = targetMechPosition.RAm;
                    simEQ500X.MechanicalDEC = targetMechPosition.DECm;
                }

                if (getCurrentMechanicalPosition(ref currentMechPosition))
                    goto sync_error;

                //currentRA = currentMechPosition.RAsky;
                //currentDEC = currentMechPosition.DECsky;
                //NewRaDec(currentRA, currentDEC);

                LogMessage("Sync", "Mount synced to target RA '$2' DEC '$1'", currentMechPosition.RAsky, currentMechPosition.DECsky);
                return true;
            }

        sync_error:
            LogMessage("Sync", "Mount sync to target RA '$0' DEC '$1' failed", ra, dec);
            return false;
        }
        #endregion

        #region Low-level commands
        internal bool gotoTargetPosition(MechanicalPoint p)
        {
            if (!isSimulation())
            {
                if (!setTargetMechanicalPosition(p))
                {
                    if (0 < sendCmd(":MS#"))
                    {
                        String buf = "";
                        if (0 < getReply(ref buf, 1))
                            return buf[0] != '0'; // 0 is valid for :MS
                    }
                    else return true;
                }
                else return true;
            }
            else return !Sync(p.RAsky, p.DECsky);

            return false;
        }

        internal bool getCurrentMechanicalPosition(ref MechanicalPoint p)
        {
            String b = "";
            MechanicalPoint result = p;

            // Always read DEC first as it gives the side of pier the scope is on, and has an impact on RA

            if (isSimulation())
                b = simEQ500X.MechanicalDECStr;
            else if (getCommandString(ref b, ":GD#") < 0)
                goto radec_error;

            if (result.parseStringDEC(b))
                goto radec_error;

            LogMessage("getCurrentMechanicalPosition", "Mount mechanical DEC reads '$0' as $1.", b, result.DECm);

            if (isSimulation())
                b = simEQ500X.MechanicalRAStr;
            else if (getCommandString(ref b, ":GR#") < 0)
                goto radec_error;

            if (result.parseStringRA(b))
                goto radec_error;

            LogMessage("getCurrentMechanicalPosition", "Mount mechanical RA reads '$0' as $1.", b, result.RAm);

            p = result;
            return false;

        radec_error:
            return true;
        }

        private bool setTargetMechanicalPosition(MechanicalPoint p)
        {
            if (!isSimulation())
            {
                String bufRA = "", bufDEC = "";

                // Write RA/DEC in placeholders
                String CmdString = String.Format(":Sr$0#:Sd$1#", p.toStringRA(ref bufRA), p.toStringDEC(ref bufDEC));
                LogMessage("setTargetMechanicalPosition", "Target RA '$0' DEC '$0' converted to '$1'", p.RAm, p.DECm, CmdString);

                String buf = "";

                if (0 < sendCmd(CmdString))
                    if (0 < getReply(ref buf, 2))
                        if (buf[0] == '1' && buf[1] == '1')
                            return false;
                        else LogMessage("setTargetMechanicalPosition", "Failed '%s', mount replied %c%c", CmdString, buf[0], buf[1]);
                    else LogMessage("setTargetMechanicalPosition", "Failed getting 2-byte reply to '%s'", CmdString);
                else LogMessage("setTargetMechanicalPosition", "Failed '%s'", CmdString);

                return true;
            }

            return false;
        }

        internal int sendCmd(String data)
        {
            LogMessage("SendCmd", "<$0>", data);
            if (!isSimulation())
            {
                try
                {
                    m_Port.Transmit(data);
                    return data.Length;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
            return 0;
        }

        internal int getReply(ref String data, int len)
        {
            if (!isSimulation())
            {
                data = m_Port.ReceiveCounted(len);
                LogMessage("getReply", "<$0>", data);
            }
            return 0;
        }

        private int getCommandString(ref String data, String cmd)
        {
            LogMessage("getCommandString", "CMD <$0>", cmd);

            /* Add mutex */
            //std::unique_lock<std::mutex> guard(lx200CommsLock);

            try
            {
                m_Port.Transmit(cmd);
                data = m_Port.ReceiveTerminated("#");
            }
            catch (Exception)
            {
                return -1;
            }

            Match m = Regex.Match(data, @"(.*)#.*");
            if (m.Success)
                data = m.Groups[1].Value;

            LogMessage("getCommandString", "RES <$0>", data);

            return 0;
        }

        private bool isSimulation()
        {
            return isSimulated;
        }
        #endregion
    }
}
