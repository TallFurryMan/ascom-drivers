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
using System.Runtime.InteropServices;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

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

        internal static string siteLongitudeName = "Site Longitude";
        internal static string siteLatitudeName = "Site Latitude";
        internal static string siteElevationName = "Site Elevation";

        internal static string comPort; // Variables to hold the currrent device configuration
        private double m_RightAscension;
        private double m_Declination;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;
        private System.Timers.Timer m_ReadScopeTimer;

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
        private bool m_TargetRightAscension_set = false;
        private bool m_TargetDeclination_set = false;
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
            internal long last_sim = 0;
        };

        private SimEQ500X simEQ500X = new SimEQ500X();

        public class LocationProfile
        {
            internal bool elevation_set = false;
            internal bool latitude_set = false;
            internal bool longitude_set = false;
            internal double m_Elevation = 0;
            internal double m_Latitude = 0;
            internal double m_Longitude = 0;
            public double Elevation
            {
                get { return m_Elevation; }
                set
                {
                    if (-300 <= value && value <= 10000)
                    {
                        LogMessage("SiteElevation Set", String.Format("Set Elevation {0}", value));
                        m_Elevation = value;
                        elevation_set = true;
                    }
                    else throw new ASCOM.InvalidValueException("SiteElevation", value.ToString(), "-300", "+100000");
                }
            }
            public double Latitude
            {
                get { return m_Latitude; }
                set
                {
                    if (-90 <= value && value <= +90)
                    {
                        LogMessage("SiteLatitude Set", String.Format("Latitude {0}", m_Latitude));
                        m_Latitude = value;
                        latitude_set = true;
                    }
                    else throw new ASCOM.InvalidValueException("SiteLatitude", value.ToString(), "-90", "+90");
                }
            }
            public double Longitude
            {
                get { return m_Longitude;  }
                set
                {
                    if (-180 <= value && value <= +180)
                    {

                        LogMessage("SiteLongitude Set", String.Format("Longitude {0}", value));
                        m_Longitude = value;
                        longitude_set = true;
                    }
                    else throw new ASCOM.InvalidValueException("SiteLongitude", value.ToString(), "-180", "+180");
                }
            }
        };

        public static LocationProfile m_LocationProfile = new LocationProfile();

        /// <summary>
        /// Initializes a new instance of the <see cref="EQ500X"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Telescope()
        {
            tl = new TraceLogger("", "EQ500X");

            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.Enabled = true;
            LogMessage("Telescope", "Starting initialisation");
            Debug.WriteLine($"Logging to {tl.LogFileName}");

            connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object

            LogMessage("Telescope", "Completed initialisation");
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
                LogMessage("SupportedActions Get", "Returning empty arraylist");
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
            lock (internalLock) switch (command)
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

            if (isSimulated) lock (internalLock)
                {
                    if ("getCurrentMechanicalPosition" == command)
                    {
                        String ra = "", dec = "";
                        currentMechPosition.toStringRA(ref ra);
                        currentMechPosition.toStringDEC(ref dec);
                        return string.Format("{0} {1}", ra, dec);
                    }
                    else if ("getReadScopeStatusInterval" == command)
                    {
                        return string.Format("{0}", PollMs);
                    }
                    else if ("getCurrentSlewRate" == command)
                    {
                        return m_SlewRate.ToString();
                    }
                }

            throw new ASCOM.MethodNotImplementedException(String.Format("CommandString - {0}", command));
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            if (null != tl)
            {
                tl.Enabled = false;
                tl.Dispose();
                tl = null;
            }
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
                LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value) lock (internalLock)
                    {
                        if (isSimulated)
                        {
                            LogMessage("Connected Set", "Connecting in simulation mode");

                            if (!ReadScopeStatus())
                                throw new FormatException();

                            connectedState = true;

                            restartReadScopeStatusTimer();

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
                                    throw new FormatException("Connected Set - Failed handshake");

                                connectedState = true;

                                try
                                {
                                    SiteLongitude = m_LocationProfile.Longitude;
                                    SiteLatitude = m_LocationProfile.Latitude;
                                    SiteElevation = m_LocationProfile.Elevation;
                                }
                                catch(Exception)
                                {
                                    connectedState = false;
                                    throw new FormatException(("Connected Set - Invalid profile information"));
                                }

                                restartReadScopeStatusTimer();
                            }
                            catch (FormatException e)
                            {
                                LogMessage("Connected Set", $"Port {comPort} open, but device failed handshake ({e.Message})");
                                m_Port.Connected = false;
                                m_Port.Dispose();
                            }
                        }
                    }
                else
                {
                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);

                    m_ReadScopeTimer.Stop();
                    m_ReadScopeTimer.Dispose();
                    m_ReadScopeTimer = null;

                    connectedState = false;

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
                LogMessage("Simulation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Simulated", true);
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                LogMessage("Description Get", driverDescription);
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
                LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                LogMessage("DriverVersion Get", driverVersion);
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
                LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ITelescope Implementation
        public void AbortSlew()
        {
            lock (internalLock)
            {
                LogMessage("AbortSlew", $"Aborting slew called while {m_TrackState.ToString()}");
                if (TrackState.TRACKING != m_TrackState)
                {
                    PollMs = 1000;
                    m_TrackState = TrackState.TRACKING;
                    // Abort movement
                    sendCmd(":Q#");
                    // Abort NS Guide
                    // Abort WE Guide
                    updateSlewRate(savedSlewRateIndex);
                }
            }
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                LogMessage("AlignmentMode Get", "German Equatorial Mount");
                return AlignmentModes.algGermanPolar;
            }
        }

        public double Altitude
        {
            get
            {
                LogMessage("Altitude", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Altitude", false);
            }
        }

        public double ApertureArea
        {
            get
            {
                LogMessage("ApertureArea Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                LogMessage("ApertureDiameter Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                LogMessage("AtHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool AtPark
        {
            get
            {
                LogMessage("AtPark", "Get - " + false.ToString());
                return false;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            LogMessage("AxisRates", "Get - " + Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                LogMessage("Azimuth Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
            }
        }

        public bool CanFindHome
        {
            get
            {
                LogMessage("CanFindHome", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {
            LogMessage("CanMoveAxis", "Get - " + Axis.ToString());
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary: return true;
                case TelescopeAxes.axisSecondary: return true;
                case TelescopeAxes.axisTertiary: return false;
                default: throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }
        }

        public bool CanPark
        {
            get
            {
                LogMessage("CanPark", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                LogMessage("CanPulseGuide", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                LogMessage("CanSetDeclinationRate", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                LogMessage("CanSetGuideRates", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPark
        {
            get
            {
                LogMessage("CanSetPark", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                LogMessage("CanSetPierSide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                LogMessage("CanSetRightAscensionRate", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                LogMessage("CanSetTracking", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlew
        {
            get
            {
                LogMessage("CanSlew", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                LogMessage("CanSlewAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                LogMessage("CanSlewAltAzAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                LogMessage("CanSlewAsync", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                LogMessage("CanSync", "Get - True");
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                LogMessage("CanSyncAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                LogMessage("CanUnpark", "Get - " + false.ToString());
                return false;
            }
        }

        public double Declination
        {
            get
            {
                lock (internalLock)
                {
                    if (!connectedState)
                        return 0;
                    if (getCurrentMechanicalPosition(ref currentMechPosition))
                        throw new ASCOM.ValueNotSetException("Declination");
                    double declination = currentMechPosition.DECsky;
                    LogMessage("Declination", "Get - " + utilities.DegreesToDMS(declination, ":", ":"));
                    return declination;
                }
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = 0.0;
                LogMessage("DeclinationRate", "Get - " + declination.ToString());
                return declination;
            }
            set
            {
                LogMessage("DeclinationRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            LogMessage("DestinationSideOfPier Get", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("DestinationSideOfPier");
        }

        public bool DoesRefraction
        {
            get
            {
                LogMessage("DoesRefraction Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
            }
            set
            {
                LogMessage("DoesRefraction Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equTopocentric;
                LogMessage("DeclinationRate", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            LogMessage("FindHome", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("FindHome");
        }

        public double FocalLength
        {
            get
            {
                LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public double GuideRateDeclination
        {
            get
            {
                LogMessage("GuideRateDeclination Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
            }
            set
            {
                LogMessage("GuideRateDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                LogMessage("GuideRateRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
            }
            set
            {
                LogMessage("GuideRateRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                LogMessage("IsPulseGuiding Get", $"Tracking state is {m_TrackState.ToString()}");
                return TrackState.GUIDING == m_TrackState;
            }
        }

        private SlewRate findSlewRate(TelescopeAxes axis, double rate)
        {
            // Requires strict match between SlewRate and IAxisRates in terms of rate
            IAxisRates rates = AxisRates(axis);
            for (int i = 1; i < rates.Count + 1; i++)
                if (rates[i].Minimum <= rate && rate <= rates[i].Maximum)
                    return (SlewRate)(i - 1);
            throw new ASCOM.InvalidValueException($"Invalid axis rate {rate} on {axis.ToString()}");
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            LogMessage("MoveAxis", $"Moving {Axis.ToString()} at {Rate}°/s");
            lock (internalLock)
            {
                if (TrackState.TRACKING == m_TrackState || TrackState.MOVING == m_TrackState)
                {
                    if (0.0 == Rate)
                    {
                        switch (Axis)
                        {
                            case TelescopeAxes.axisPrimary:
                                if (0 != m_RASlewRate)
                                {
                                    sendCmd(":Q" + (0 < m_RASlewRate ? 'w' : 'e') + '#');
                                    m_RASlewRate = 0;
                                }
                                break;
                            case TelescopeAxes.axisSecondary:
                                if (0 != m_DECSlewRate)
                                {
                                    sendCmd(":Q" + (0 < m_DECSlewRate ? 'n' : 's') + '#');
                                    m_DECSlewRate = 0;
                                }
                                break;
                            default:
                                throw new ASCOM.InvalidValueException($"MoveAxis - Invalid axis {Axis.ToString()}");
                        }
                        if (0 == m_RASlewRate && 0 == m_DECSlewRate)
                        {
                            updateSlewRate(savedSlewRateIndex);
                            m_TrackState = TrackState.TRACKING;
                        }
                        return;
                    }

                    SlewRate matchingRate = findSlewRate(Axis, Math.Abs(Rate));

                    if (TrackState.MOVING == m_TrackState && matchingRate != m_SlewRate)
                        throw new ASCOM.InvalidOperationException($"MoveAxis - Mount is already moving at {m_SlewRate.ToString()}");

                    CheckConnected("MoveAxis requires hardware connection");

                    switch (Axis)
                    {
                        case TelescopeAxes.axisPrimary:
                            sendCmd(":M" + (0 < Rate ? 'w' : 'e') + '#');
                            m_RASlewRate = Rate;
                            break;
                        case TelescopeAxes.axisSecondary:
                            sendCmd(":M" + (0 < Rate ? 'n' : 's') + '#');
                            m_DECSlewRate = Rate;
                            break;
                        default:
                            throw new ASCOM.InvalidValueException($"MoveAxis - Invalid axis {Axis.ToString()}");
                    }

                    if (TrackState.TRACKING == m_TrackState)
                        savedSlewRateIndex = m_SlewRate;

                    updateSlewRate(matchingRate);

                    m_TrackState = TrackState.MOVING;
                }
                else throw new ASCOM.InvalidOperationException($"MoveAxis - Not tracking");
            }
        }

        public void Park()
        {
            LogMessage("Park", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Park");
        }

        private void completeGuideCommand(GuideDirections direction)
        {
            LogMessage("completeGuideCommand", $"Cleaning up {direction}");

            lock (internalLock)
            {
                if (TrackState.GUIDING == m_TrackState)
                {
                    bool ra_complete = null == m_RAGuideTask || m_RAGuideTask.IsCompleted;
                    bool dec_complete = null == m_DECGuideTask || m_DECGuideTask.IsCompleted;

                    switch (direction)
                    {
                        case GuideDirections.guideWest:
                            sendCmd(":Qw#");
                            ra_complete = true;
                            break;
                        case GuideDirections.guideEast:
                            sendCmd(":Qe#");
                            ra_complete = true;
                            break;
                        case GuideDirections.guideSouth:
                            sendCmd(":Qs#");
                            dec_complete = true;
                            break;
                        case GuideDirections.guideNorth:
                            sendCmd(":Qn#");
                            dec_complete = true;
                            break;
                    }

                    if (ra_complete)
                        m_RAGuideTask = null;

                    if (dec_complete)
                        m_DECGuideTask = null;

                    if (ra_complete && dec_complete)
                    {
                        updateSlewRate(savedSlewRateIndex);
                        m_TrackState = TrackState.TRACKING;
                    }
                }
            }
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            LogMessage("PulseGuide", $"Guiding in direction {Direction.ToString()} for {Duration}ms");

            switch (Direction)
            {
                case GuideDirections.guideWest:
                case GuideDirections.guideEast:
                    if (null != m_RAGuideTaskCancellation)
                        m_RAGuideTaskCancellation.Cancel();
                    if (null != m_RAGuideTask)
                        m_RAGuideTask.Wait();
                    break;

                case GuideDirections.guideSouth:
                case GuideDirections.guideNorth:
                    if (null != m_DECGuideTaskCancellation)
                        m_DECGuideTaskCancellation.Cancel();
                    if (null != m_DECGuideTask)
                        m_DECGuideTask.Wait();
                    break;
            }

            completeGuideCommand(Direction);

            lock (internalLock)
            {
                if (TrackState.TRACKING == m_TrackState || TrackState.GUIDING == m_TrackState)
                {
                    if (0 == Duration)
                        return;

                    CheckConnected("PulseGuide requires hardware connection");

                    if (TrackState.TRACKING == m_TrackState)
                        savedSlewRateIndex = m_SlewRate;
                    updateSlewRate(SlewRate.SLEW_GUIDE);

                    switch (Direction)
                    {
                        case GuideDirections.guideWest:
                            sendCmd(":Mw#");
                            m_RAGuideTaskCancellation = new CancellationTokenSource();
                            m_RAGuideTask = Task.Factory.StartNew(() =>
                            {
                                m_RAGuideTaskCancellation.Token.WaitHandle.WaitOne(Duration);
                                completeGuideCommand(Direction);
                            });
                            break;

                        case GuideDirections.guideEast:
                            sendCmd(":Me#");
                            m_RAGuideTaskCancellation = new CancellationTokenSource();
                            m_RAGuideTask = Task.Factory.StartNew(() =>
                            {
                                m_RAGuideTaskCancellation.Token.WaitHandle.WaitOne(Duration);
                                completeGuideCommand(Direction);
                            });
                            break;

                        case GuideDirections.guideNorth:
                            sendCmd(":Mn#");
                            m_DECGuideTaskCancellation = new CancellationTokenSource();
                            m_DECGuideTask = Task.Factory.StartNew(() =>
                            {
                                m_DECGuideTaskCancellation.Token.WaitHandle.WaitOne(Duration);
                                completeGuideCommand(Direction);
                            });
                            break;

                        case GuideDirections.guideSouth:
                            sendCmd(":Ms#");
                            m_DECGuideTaskCancellation = new CancellationTokenSource();
                            m_DECGuideTask = Task.Factory.StartNew(() =>
                            {
                                m_DECGuideTaskCancellation.Token.WaitHandle.WaitOne(Duration);
                                completeGuideCommand(Direction);
                            });
                            break;

                        default:
                            updateSlewRate(savedSlewRateIndex);
                            throw new ASCOM.InvalidValueException($"PulseGuide - Invalid direction {Direction.ToString()}");
                    }

                    // Track state can be changed as final instruction because we locked the internals, thus won't get disturbed
                    m_TrackState = TrackState.GUIDING;
                }
                else throw new ASCOM.InvalidOperationException($"PulseGuide - Not tracking");
            }
        }

        public double RightAscension
        {
            get
            {
                lock (internalLock)
                {
                    if (!connectedState)
                        return 0;
                    if (getCurrentMechanicalPosition(ref currentMechPosition))
                        throw new ASCOM.ValueNotSetException("Declination");
                    double rightAscension = currentMechPosition.RAsky;
                    LogMessage("RightAscension", "Get - " + utilities.HoursToHMS(rightAscension));
                    return rightAscension;
                }
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = 0.0;
                LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                LogMessage("RightAscensionRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            LogMessage("SetPark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SetPark");
        }

        public PierSide SideOfPier
        {
            get
            {
                lock (internalLock)
                {
                    if (!connectedState)
                        return PierSide.pierUnknown;
                    LogMessage("SideOfPier Get", $"Pointing {m_SideOfPier.ToString()}");
                    return m_SideOfPier;
                }
            }
            set
            {
                LogMessage("SideOfPier Set", "Not implemented");
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
                        GstType.GreenwichApparentSiderealTime,
                        Method.EquinoxBased,
                        Accuracy.Reduced, ref siderealTime);
                }

                // Allow for the longitude
                siderealTime += SiteLongitude / 360.0 * 24.0;

                // Reduce to the range 0 to 24 hours
                siderealTime = astroUtilities.ConditionRA(siderealTime);

                LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                if (!connectedState)
                    return 0;

                if (m_LocationProfile.elevation_set)
                {
                    LogMessage("SiteElevation Get", String.Format("Elevation {0}", m_LocationProfile.Elevation));
                    return m_LocationProfile.Elevation;
                }
                else throw new ASCOM.InvalidOperationException("SiteElevation - Not initialized");
            }
            set
            {
                lock (internalLock) m_LocationProfile.Elevation = value;
            }
        }

        public double SiteLatitude
        {
            get
            {
                if (!Connected)
                    return 0;

                if (m_LocationProfile.latitude_set)
                {
                    LogMessage("SiteElevation Get", String.Format("Elevation {0}", m_LocationProfile.Latitude));
                    return m_LocationProfile.Latitude;
                }
                else throw new ASCOM.InvalidOperationException("SiteLatitude - Not initialized");
            }
            set
            {
                lock (internalLock) m_LocationProfile.Latitude = value;
            }
        }

        public double SiteLongitude
        {
            get
            {
                if (!Connected)
                    return 0;

                if (m_LocationProfile.longitude_set)
                {
                    LogMessage("SiteLongitude Get", String.Format("Elevation {0}", m_LocationProfile.Longitude));
                    return m_LocationProfile.Longitude;
                }
                else throw new ASCOM.InvalidOperationException("SiteLongitude - Not initialized");
            }
            set
            {
                lock (internalLock) m_LocationProfile.Longitude = value;

                //CheckConnected("Setting SiteLongitude requires hardware connection");
                if (connectedState)
                {
                    if (isSimulated)
                        simEQ500X.LST = 0.0 + value / 15.0;

                    if (!getCurrentMechanicalPosition(ref currentMechPosition) && currentMechPosition.atParkingPosition())
                    {
                        double LST = isSimulated ? simEQ500X.LST : SiderealTime;
                        Sync(LST - 6, currentMechPosition.DECsky);
                        LogMessage("SiteLongitude Set", String.Format("Location updated: mount considered parked, synced to LST {0}", utilities.HoursToHMS(LST)));
                    }
                }
            }
        }

        public short SlewSettleTime
        {
            get
            {
                LogMessage("SlewSettleTime Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
            }
            set
            {
                LogMessage("SlewSettleTime Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinatesAsync(double ra, double dec)
        {
            if (ra < 0 || +24 < ra)
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync - RightAscension", ra.ToString(), "-24", "+24");
            }
            else if (dec < -90 || +90 < dec)
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync - Declination", dec.ToString(), "-90", "+90");
            }
            else
            {
                lock (internalLock)
                {
                    LogMessage("SlewToCoordinatesAsync", $"Slewing to {ra} {dec}");

                    CheckConnected("SlewToCoordinates/Async require hardware connection");

                    // Check whether a meridian flip is required
                    double LST = isSimulated ? simEQ500X.LST : SiderealTime;
                    double HA = LST - ra;
                    while (+12 <= HA) HA -= 24;
                    while (HA <= -12) HA += 24;

                    // Deduce required orientation of mount in HA quadrants - set orientation BEFORE coordinates!
                    targetMechPosition.PointingState = (0 <= HA && HA < 12) ? MechanicalPoint.PointingStates.POINTING_NORMAL : MechanicalPoint.PointingStates.POINTING_BEYOND_POLE;
                    targetMechPosition.RAsky = ra;
                    targetMechPosition.DECsky = dec;
                    m_TargetRightAscension_set = m_TargetDeclination_set = true;

                    // If moving, let's stop it first.
                    if (Slewing)
                    {
                        AbortSlew();

                        // sleep for 100 mseconds
                        Thread.Sleep(100);
                    }

                    /* The goto feature is quite imprecise because it will always use full speed.
                     * By the time the mount stops, the position is off by 0-5 degrees, depending on the speed attained during the move.
                     * Additionally, a firmware limitation prevents the goto feature from slewing to close coordinates, and will cause uneeded axis rotation.
                     * Therefore, don't use the goto feature for a goto, and let ReadScope adjust the position by itself.
                     */

                    // Set target position and adjust
                    if (setTargetMechanicalPosition(targetMechPosition))
                    {
                        //EqNP.s = IPS_ALERT;
                        //IDSetNumber(&EqNP, "Error setting RA/DEC.");
                        return;// false;
                    }
                    else
                    {
                        //targetMechPosition.RAsky = /* targetRA = */ ra;
                        //targetMechPosition.DECsky = /* targetDEC = */ dec;

                        LogMessage("SlewToCoordinatesAsync", string.Format("Goto target ({0}h,{1:F2}°) HA {2}, LST {3}, quadrant {4}", ra, dec, HA, LST, targetMechPosition.PointingState == MechanicalPoint.PointingStates.POINTING_NORMAL ? "normal" : "beyond pole"));
                    }

                    // Limit the number of loops
                    countdown = MAX_CONVERGENCE_LOOPS;

                    // Reset original adjustment
                    // Reset movement markers

                    m_TrackState = TrackState.SLEWING;

                    // Remember current slew rate
                    savedSlewRateIndex = m_SlewRate;// static_cast <enum TelescopeSlewRate> (IUFindOnSwitchIndex(&SlewRateSP));

                    // Format RA/DEC for logs
                    //    char RAStr[16] = { 0 }, DecStr[16] = { 0 };
                    //fs_sexa(RAStr, targetRA, 2, 3600);
                    //fs_sexa(DecStr, targetDEC, 2, 3600);

                    //LogMessage("SlewToCoordinates", $"Slewing to JNow RA: {RAStr} - DEC: {DecStr}");
                }
            }
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            LogMessage("SlewToCoordinates", $"Slewing to {RightAscension},{Declination}");
            SlewToCoordinatesAsync(RightAscension, Declination);
            while (!Tracking)
                Thread.Sleep(250);
        }

        public void SlewToTarget()
        {
            LogMessage("SlewToTarget", $"Slewing to {TargetRightAscension},{TargetDeclination}");
            SlewToCoordinatesAsync(TargetRightAscension, TargetDeclination);
            while (!Tracking)
                Thread.Sleep(250);
        }

        public void SlewToTargetAsync()
        {
            LogMessage("SlewToTargetAsync", $"Slewing to {TargetRightAscension},{TargetDeclination}");
            SlewToCoordinatesAsync(TargetRightAscension, TargetDeclination);
        }

        public bool Slewing
        {
            get
            {
                lock (internalLock)
                {
                    LogMessage("Slewing", $"Get - {m_TrackState.ToString()}");
                    return m_TrackState == TrackState.SLEWING || m_TrackState == TrackState.MOVING;
                }
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            if (RightAscension < 0 || +24 < RightAscension)
            {
                throw new ASCOM.InvalidValueException("SyncToCoordinates - RightAscension", RightAscension.ToString(), "-24", "+24");
            }
            else if (Declination < -90 || +90 < Declination)
            {
                throw new ASCOM.InvalidValueException("SyncToCoordinates - Declination", Declination.ToString(), "-90", "+90");
            }
            else
            {
                LogMessage("SyncToCoordinates", String.Format("Set RA:{0:F2} DEC:{1:F2}", RightAscension, Declination));
                lock (internalLock)
                {
                    if (TrackState.TRACKING != m_TrackState)
                        throw new ASCOM.InvalidOperationException("SyncToCoordinates - mount is not tracking");

                    if (!Sync(RightAscension, Declination))
                        throw new ASCOM.DriverException("SyncToCoordinates");
                }
            }
        }

        public void SyncToTarget()
        {
            if (!m_TargetRightAscension_set)
            {
                throw new ASCOM.ValueNotSetException("SyncToTarget - RightAscension not set");
            }
            else if (!m_TargetDeclination_set)
            {
                throw new ASCOM.ValueNotSetException("SyncToTarget - Declination not set");
            }
            else
            {
                LogMessage("SyncToTarget", String.Format("Set RA:{0:F2} DEC:{1:F2}", targetMechPosition.RAsky, targetMechPosition.DECsky));
                lock (internalLock)
                {
                    if (TrackState.TRACKING != m_TrackState)
                        throw new ASCOM.InvalidOperationException("SyncToCoordinates - mount is not tracking");

                    if (!Sync(targetMechPosition.RAsky, targetMechPosition.DECsky))
                        throw new ASCOM.DriverException("SyncToCoordinates");
                }
            }
        }

        public double TargetDeclination
        {
            get
            {
                if (m_TargetDeclination_set)
                {
                    LogMessage("TargetDeclination Get", $"Target DEC {targetMechPosition.DECsky}");
                    return targetMechPosition.DECsky;
                }
                else throw new ASCOM.InvalidOperationException("TargetDeclination - not set");
            }
            set
            {
                if (value < -90 || +90 < value)
                    throw new ASCOM.InvalidValueException("TargetDeclination", Declination.ToString(), "-90", "+90");

                LogMessage("TargetDeclination Set", $"Target DEC {value}");
                targetMechPosition.DECsky = value;
                m_TargetDeclination_set = true;
            }
        }

        public double TargetRightAscension
        {
            get
            {
                if (m_TargetRightAscension_set)
                {
                    LogMessage("TargetRightAscension Get", $"Target RA {targetMechPosition.RAsky}");
                    return targetMechPosition.RAsky;
                }
                else throw new ASCOM.InvalidOperationException("TargetRightAscension - not set");
            }
            set
            {
                if (value < 0 || +24 < value)
                    throw new ASCOM.InvalidValueException("TargetRightAscension", RightAscension.ToString(), "-24", "+24");

                LogMessage("TargetRightAscension Set", $"Target RA {value}");
                targetMechPosition.RAsky = value;
                m_TargetRightAscension_set = true;
            }
        }

        public bool Tracking
        {
            get
            {
                lock (internalLock)
                {
                    LogMessage("Tracking", "Get - " + m_TrackState.ToString());
                    return m_TrackState == TrackState.TRACKING;
                }
            }
            set
            {
                throw new ASCOM.PropertyNotImplementedException("Tracking - cannot change tracking state");
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                LogMessage("TrackingRate Get", $"Get - {DriveRates.driveSidereal.ToString()}");
                return DriveRates.driveSidereal;
            }
            set
            {
                LogMessage("TrackingRate Set", $"Set - {value}");
                if (DriveRates.driveSidereal != value)
                    throw new ASCOM.InvalidValueException("TrackingRate", value.ToString(), DriveRates.driveSidereal.ToString(), DriveRates.driveSidereal.ToString());
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                ITrackingRates trackingRates = new TrackingRates();
                LogMessage("TrackingRates", "Get - ");
                foreach (DriveRates driveRate in trackingRates)
                {
                    LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                DateTime utcDate = DateTime.UtcNow;
                LogMessage("TrackingRates", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                LogMessage("UTCDate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
            }
        }

        public void Unpark()
        {
            LogMessage("Unpark", "Not implemented");
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
                try { m_LocationProfile.Longitude = Convert.ToDouble(driverProfile.GetValue(driverID, siteLongitudeName, string.Empty)); }
                catch (Exception) { LogMessage("ReadProfile", "No suitable value for longitude in profile."); }
                try { m_LocationProfile.Latitude = Convert.ToDouble(driverProfile.GetValue(driverID, siteLatitudeName, string.Empty)); }
                catch (Exception) { LogMessage("ReadProfile", "No suitable value for latitude in profile."); }
                try { m_LocationProfile.Elevation = Convert.ToDouble(driverProfile.GetValue(driverID, siteElevationName, string.Empty)); }
                catch (Exception) { LogMessage("ReadProfile", "No suitable value for elevation in profile."); }
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
                if (driverProfile.IsRegistered(driverID))
                {
                    driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
                    driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
                    driverProfile.WriteValue(driverID, siteLongitudeName, m_LocationProfile.Longitude.ToString());
                    driverProfile.WriteValue(driverID, siteLatitudeName, m_LocationProfile.Latitude.ToString());
                    driverProfile.WriteValue(driverID, siteElevationName, m_LocationProfile.Elevation.ToString());
                }
                else LogMessage("WriteProfile", $"ASCOM Driver {driverID} is not registered");
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
            Debug.WriteLine(identifier + " | " + msg);
            tl.LogMessage(identifier, msg);
        }
        #endregion

        #region Mount functionalities

        // One degree, one arcminute, one arcsecond
        const double ONEDEGREE = 1.0;
        const double ARCMINUTE = ONEDEGREE / 60.0;
        const double ARCSECOND = ONEDEGREE / 3600.0;

        // This is the minimum detectable movement in RA/DEC
        readonly double RA_GRANULARITY = Math.Round((15.0 * ARCSECOND) * 3600.0, MidpointRounding.AwayFromZero) / 3600.0;
        readonly double DEC_GRANULARITY = Math.Round((1.0 * ARCSECOND) * 3600.0, MidpointRounding.AwayFromZero) / 3600.0;

        // This is the number of loops expected to achieve convergence on each slew rate
        // A full rotation at 5deg/s would take 360/5=72s to complete at RS speed, checking position twice per second
        const int MAX_CONVERGENCE_LOOPS = 144;


        // Hardcoded adjustment intervals
        // RA/DEC deltas are adjusted at specific 'slew_rate' down to 'epsilon' degrees when smaller than 'distance' degrees
        // The greater adjustment requirement drives the slew rate (one single command for both axis)
        // From https://stackoverflow.com/a/309528/528052
        struct _adjustment
        {
            public string slew_rate;
            public int switch_index;
            public double epsilon;
            public double distance;
            public int polling_interval;
            public _adjustment(string sr, int si, double e, double d, int pi)
            {
                slew_rate = sr;
                switch_index = si;
                epsilon = e;
                distance = d;
                polling_interval = pi;
            }
        };

        static readonly IList<_adjustment> adjustments = new ReadOnlyCollection<_adjustment>(new[] {
            new _adjustment(":RG#", 0, 1*ARCSECOND, 0.7*ARCMINUTE,  100 ),   // Guiding speed
            new _adjustment(":RC#", 1, 0.7*ARCMINUTE, 10*ARCMINUTE, 200 ),   // Centering speed
            new _adjustment(":RM#", 2, 10*ARCMINUTE, 5*ONEDEGREE, 500 ),     // Finding speed
            new _adjustment(":RS#", 3, 5*ONEDEGREE,  360*ONEDEGREE, 1000 ),  // Slew speed
        });

        private int adjustment = -1;
        private bool RAmDecrease = false, RAmIncrease = false, DECmDecrease = false, DECmIncrease = false;
        private long PollMs = 1000;
        //private TelescopeSlewRate savedSlewRateIndex;
        private int countdown;

        private enum TrackState { TRACKING, SLEWING, MOVING, GUIDING };
        private TrackState m_TrackState = TrackState.TRACKING;

        private enum SlewRate { SLEW_GUIDE, SLEW_CENTER, SLEW_FIND, SLEW_MAX }; // Strict match with IAxisRates
        private SlewRate m_SlewRate = SlewRate.SLEW_MAX;
        private SlewRate savedSlewRateIndex = SlewRate.SLEW_MAX;
        private bool _gotoEngaged = false;

        // Previous alignment marker to spot when to change slew rate
        private static int previous_adjustment = -1;
        private PierSide m_SideOfPier = PierSide.pierWest;
        private object internalLock = new object();

        private Task m_RAGuideTask = null;
        private Task m_DECGuideTask = null;

        private CancellationTokenSource m_RAGuideTaskCancellation = new CancellationTokenSource();
        private CancellationTokenSource m_DECGuideTaskCancellation = new CancellationTokenSource();
        private double m_RASlewRate;
        private double m_DECSlewRate;

        private bool ReadScopeStatus()
        {
            lock (internalLock)
            {
                if (isSimulated)
                {
                    // These are the simulated rates
                    double[] rates = {
                        /*RG*/  5 * ARCSECOND,
                        /*RC*/  5 * ARCMINUTE,
                        /*RM*/ 20 * ARCMINUTE,
                        /*RS*/  5 * ONEDEGREE,
                    };

                    // Calculate elapsed time since last status read
                    long now = Stopwatch.GetTimestamp();
                    long delta = 0;
                    if (0 != simEQ500X.last_sim)
                        if (simEQ500X.last_sim < now)
                            delta = now - simEQ500X.last_sim;
                        else
                            delta = (long.MaxValue - simEQ500X.last_sim) + now;
                    simEQ500X.last_sim = now;
                    double delta_s = (double)delta / Stopwatch.Frequency;

                    // Simulate movement if needed
                    if (0 <= adjustment)
                    {
                        // Use currentRA/currentDEC to store smaller-than-one-arcsecond values
                        if (RAmDecrease) simEQ500X.MechanicalRA = (simEQ500X.MechanicalRA - rates[adjustment] * delta_s / 15.0 + 24.0) % 24.0;
                        if (RAmIncrease) simEQ500X.MechanicalRA = (simEQ500X.MechanicalRA + rates[adjustment] * delta_s / 15.0 + 24.0) % 24.0;
                        if (DECmDecrease) simEQ500X.MechanicalDEC -= rates[adjustment] * delta_s;
                        if (DECmIncrease) simEQ500X.MechanicalDEC += rates[adjustment] * delta_s;

                        // Update current position and rewrite simulated mechanical positions
                        MechanicalPoint p = new MechanicalPoint(simEQ500X.MechanicalRA, simEQ500X.MechanicalDEC);
                        p.toStringRA(ref simEQ500X.MechanicalRAStr);
                        p.toStringDEC_Sim(ref simEQ500X.MechanicalDECStr);

                        LogMessage("ReadScopeStatus", "New mechanical RA/DEC simulated as {0:F2}°/{1:F2}° ({2:F3}°,{3:F3}°) after {8:F3}s, stored as {4}h/{5:F2}° = {6}/{7}", simEQ500X.MechanicalRA * 15.0, simEQ500X.MechanicalDEC, (RAmDecrease || RAmIncrease) ? rates[adjustment] * delta : 0, (DECmDecrease || DECmIncrease) ? rates[adjustment] * delta : 0, p.RAm, p.DECm, simEQ500X.MechanicalRAStr, simEQ500X.MechanicalDECStr, delta_s);
                    }
                }

                if (getCurrentMechanicalPosition(ref currentMechPosition))
                {
                    LogMessage("ReadScopeStatus", "Reading scope status failed");
                    restartReadScopeStatusTimer();
                    return false;
                }

                bool ra_changed = m_RightAscension != currentMechPosition.RAsky;
                bool dec_changed = m_Declination != currentMechPosition.DECsky;

                if (dec_changed)
                    m_Declination = currentMechPosition.DECsky;

                if (ra_changed)
                {
                    m_RightAscension = currentMechPosition.RAsky;

                    // Update the side of pier
                    double LST = isSimulated ? simEQ500X.LST : SiderealTime;
                    double HA = LST - m_RightAscension;
                    while (+12 <= HA) HA -= 24;
                    while (HA <= -12) HA += 24;
                    switch (currentMechPosition.PointingState)
                    {
                        case MechanicalPoint.PointingStates.POINTING_NORMAL:
                            m_SideOfPier = HA < 6 ? PierSide.pierEast : PierSide.pierWest;
                            break;
                        case MechanicalPoint.PointingStates.POINTING_BEYOND_POLE:
                            m_SideOfPier = 6 < HA ? PierSide.pierEast : PierSide.pierWest;
                            break;
                    }
                    LogMessage("ReadScopeStatus", "Mount HA={0:F3}h pointing {1} on {2} side", HA, currentMechPosition.PointingState == MechanicalPoint.PointingStates.POINTING_NORMAL ? "normal" : "beyond pole", SideOfPier == PierSide.pierEast ? "east" : "west");
                }

                /*
                // If we are using the goto feature, check state
                if (TrackState.SLEWING == m_TrackState && _gotoEngaged)
                {
                    if (EqN[AXIS_RA].value == currentRA && EqN[AXIS_DE].value == currentDEC)
                    {
                        _gotoEngaged = false;

                        // Preliminary goto is complete, continue
                        if (!Goto(targetMechPosition.RAsky, targetMechPosition.DECsky))
                            goto slew_failure;
                    }
                }
                */

                // If we are adjusting, adjust movement and timer time to achieve arcsecond goto precision
                if (TrackState.SLEWING == m_TrackState && !_gotoEngaged)
                {
                    Debug.Assert(m_TargetDeclination_set && m_TargetRightAscension_set);

                    // Compute RA/DEC deltas - keep in mind RA is in hours on the mount, with a granularity of 15 degrees
                    double ra_delta = currentMechPosition.RA_degrees_to(targetMechPosition);
                    double dec_delta = currentMechPosition.DEC_degrees_to(targetMechPosition);
                    double abs_ra_delta = Math.Abs(ra_delta);
                    double abs_dec_delta = Math.Abs(dec_delta);

                    // If mount is not at target, adjust
                    if (RA_GRANULARITY <= abs_ra_delta || DEC_GRANULARITY <= abs_dec_delta)
                    {
                        // This will hold required adjustments in RA and DEC axes
                        int ra_adjust = -1, dec_adjust = -1;

                        // Choose slew rate for RA based on distance to target
                        for (int i = 0; i < adjustments.Count && -1 == ra_adjust; i++)
                            if (abs_ra_delta <= adjustments[i].distance)
                                ra_adjust = i;
                        Debug.Assert(-1 != ra_adjust);
                        LogMessage("ReadScopeStatus", "RA  {0:F2}-{1:F2} = {2:F2}° under {3:F2}° would require adjustment at {4} until less than {5:F2}°", targetMechPosition.RAm * 15.0, currentMechPosition.RAm * 15.0, ra_delta, adjustments[ra_adjust].distance, adjustments[ra_adjust].slew_rate, Math.Max(adjustments[ra_adjust].epsilon, 15.0 / 3600.0));

                        // Choose slew rate for DEC based on distance to target
                        for (int i = 0; i < adjustments.Count && -1 == dec_adjust; i++)
                            if (abs_dec_delta <= adjustments[i].distance)
                                dec_adjust = i;
                        Debug.Assert(-1 != dec_adjust);
                        LogMessage("ReadScopeStatus", "DEC {0:F2}-{1:F2} = {2:F2}° under {3:F2}° would require adjustment at {4} until less than {5:F2}°", targetMechPosition.DECm, currentMechPosition.DECm, dec_delta, adjustments[dec_adjust].distance, adjustments[dec_adjust].slew_rate, adjustments[dec_adjust].epsilon);

                        // This will hold the command string to send to the mount, with move commands
                        String CmdString = "";

                        // We adjust the axis which has the faster slew rate first, eventually both axis at the same time if they have same speed
                        // Because we have only one rate for both axes, we need to choose the fastest rate and control the axis (eventually both) which requires that rate
                        adjustment = ra_adjust < dec_adjust ? dec_adjust : ra_adjust;

                        // If RA was moving but now would be moving at the wrong rate, stop it
                        if (ra_adjust != adjustment)
                        {
                            if (RAmIncrease) { CmdString += ":Qe#"; RAmIncrease = false; }
                            if (RAmDecrease) { CmdString += ":Qw#"; RAmDecrease = false; }
                        }

                        // If DEC was moving but now would be moving at the wrong rate, stop it
                        if (dec_adjust != adjustment)
                        {
                            if (DECmDecrease) { CmdString += ":Qn#"; DECmDecrease = false; }
                            if (DECmIncrease) { CmdString += ":Qs#"; DECmIncrease = false; }
                        }

                        // Prepare for the new rate
                        if (previous_adjustment != adjustment)
                        {
                            // Add the new slew rate
                            CmdString += adjustments[adjustment].slew_rate;

                            // If adjustment goes expectedly down, reset countdown
                            if (adjustment < previous_adjustment)
                                countdown = MAX_CONVERGENCE_LOOPS;

                            // FIXME: wait for the mount to slow down to improve convergence?

                            // Remember previous adjustment
                            previous_adjustment = adjustment;
                        }
                        LogMessage("ReadScopeStatus", $"Current adjustment speed is {adjustments[adjustment].slew_rate}");

                        // If RA is being adjusted, check delta against adjustment epsilon to enable or disable movement
                        // The smallest change detectable in RA is 1/3600 hours, or 15/3600 degrees
                        if (ra_adjust == adjustment)
                        {
                            // This is the lowest limit of this adjustment
                            double ra_epsilon = Math.Max(adjustments[adjustment].epsilon, RA_GRANULARITY);

                            // Find requirement
                            bool doDecrease = ra_epsilon <= ra_delta;
                            bool doIncrease = ra_delta <= -ra_epsilon;
                            Debug.Assert(!(doDecrease && doIncrease));

                            // Stop movement if required - just stopping or going opposite
                            if (RAmIncrease && (!doDecrease || doIncrease)) { CmdString += ":Qe#"; RAmIncrease = false; }
                            if (RAmDecrease && (!doIncrease || doDecrease)) { CmdString += ":Qw#"; RAmDecrease = false; }

                            // Initiate movement if required
                            if (doDecrease && !RAmIncrease) { CmdString += ":Me#"; RAmIncrease = true; }
                            if (doIncrease && !RAmDecrease) { CmdString += ":Mw#"; RAmDecrease = true; }
                        }

                        // If DEC is being adjusted, check delta against adjustment epsilon to enable or disable movement
                        // The smallest change detectable in DEC is 1/3600 degrees
                        if (dec_adjust == adjustment)
                        {
                            // This is the lowest limit of this adjustment
                            double dec_epsilon = Math.Max(adjustments[adjustment].epsilon, DEC_GRANULARITY);

                            // Find requirement
                            bool doDecrease = dec_epsilon <= dec_delta;
                            bool doIncrease = dec_delta <= -dec_epsilon;
                            Debug.Assert(!(doDecrease && doIncrease));

                            // Stop movement if required - just stopping or going opposite
                            if (DECmIncrease && (!doDecrease || doIncrease)) { CmdString += ":Qn#"; DECmIncrease = false; }
                            if (DECmDecrease && (!doIncrease || doDecrease)) { CmdString += ":Qs#"; DECmDecrease = false; }

                            // Initiate movement if required
                            if (doDecrease && !DECmIncrease) { CmdString += ":Mn#"; DECmIncrease = true; }
                            if (doIncrease && !DECmDecrease) { CmdString += ":Ms#"; DECmDecrease = true; }
                        }

                        // Basic algorithm sanitization on movement orientation: move one way or the other, or not at all
                        Debug.Assert(!(RAmIncrease && RAmDecrease) && !(DECmDecrease && DECmIncrease));

                        // This log shows target in Degrees/Degrees and delta in Degrees/Degrees
                        LogMessage("ReadScopeStatus", string.Format("Centering ({0:F2}°,{1:F2}°) delta ({2:F2}°,{3:F2}°) moving {4}{5}{6}{7} at {8} until less than ({9:F2}°,{10:F2}°)", targetMechPosition.RAm * 15.0, targetMechPosition.DECm, ra_delta, dec_delta, RAmDecrease ? 'W' : '.', RAmIncrease ? 'E' : '.', DECmDecrease ? 'N' : '.', DECmIncrease ? 'S' : '.', adjustments[adjustment].slew_rate, Math.Max(adjustments[adjustment].epsilon, RA_GRANULARITY), adjustments[adjustment].epsilon));

                        // If we have a command to run, issue it
                        if (0 < CmdString.Length)
                        {
                            // Send command to mount
                            if (0 < sendCmd(CmdString))
                            {
                                LogMessage("ReadScopeStatus", $"Error centering ({targetMechPosition.RAm * 15.0:F2}°,{targetMechPosition.DECm:F2}°)");
                                //slewError(-1);
                                restartReadScopeStatusTimer();
                                return false;
                            }

                            // Update slew rate
                            // IUResetSwitch(&SlewRateSP);
                            // SlewRateS[adjustment->switch_index].s = ISS_ON;
                            // IDSetSwitch(&SlewRateSP, nullptr);
                        }

                        // If all movement flags are cleared, we are done adjusting
                        if (!RAmIncrease && !RAmDecrease && !DECmDecrease && !DECmIncrease)
                        {
                            LogMessage("ReadScopeStatus", $"Centering delta ({ra_delta:F2},{dec_delta:F2}) intermediate adjustment complete ({MAX_CONVERGENCE_LOOPS - countdown} loops)");
                            adjustment = -1;
                        }
                        // Else, if it has been too long since we started, maybe we have a convergence problem.
                        // The mount slows down when requested to stop under minimum distance, so we may miss the target.
                        // The behavior is improved by changing the slew rate while converging, but is still tricky to tune.
                        else if (--countdown <= 0)
                        {
                            LogMessage("ReadScopeStatus", $"Failed centering to ({targetMechPosition.RAm},{targetMechPosition.DECm}) under loop limit, aborting...");
                            goto slew_failure;
                        }
                        // Else adjust poll timeout to adjustment speed and continue
                        else PollMs = adjustments[adjustment].polling_interval;
                    }
                    // If we attained target position at one arcsecond precision, finish procedure and track target
                    else
                    {
                        LogMessage("ReadScopeStatus", "Slew is complete. Tracking...");
                        sendCmd(":Q#");
                        updateSlewRate(savedSlewRateIndex);
                        adjustment = -1;
                        PollMs = 1000;
                        m_TrackState = TrackState.TRACKING;
                    }
                }
                else
                {
                    // Force-reset markers in case we got aborted
                    if (DECmDecrease) DECmDecrease = false;
                    if (DECmIncrease) DECmIncrease = false;
                    if (RAmIncrease) RAmIncrease = false;
                    if (RAmDecrease) RAmDecrease = false;
                    adjustment = -1;
                }
                // Update RA/DEC properties
                //if (ra_changed || dec_changed)
                //    NewRaDec(currentRA, currentDEC);

                restartReadScopeStatusTimer();
                return true;

            slew_failure:
                // If we failed at some point, attempt to stop moving and update properties with error
                sendCmd(":Q#");
                updateSlewRate(savedSlewRateIndex);
                adjustment = -1;
                PollMs = 1000;
                m_TrackState = TrackState.TRACKING;
                m_RightAscension = currentMechPosition.RAsky;
                m_Declination = currentMechPosition.DECsky;
                //NewRaDec(currentRA, currentDEC);
                //slewError(-1);
                restartReadScopeStatusTimer();
                return false;
            }
        }

        private void restartReadScopeStatusTimer()
        {
            if (null == m_ReadScopeTimer)
            {
                m_ReadScopeTimer = new System.Timers.Timer(PollMs);
                m_ReadScopeTimer.Elapsed += new ElapsedEventHandler((object obj, ElapsedEventArgs e) => ReadScopeStatus());
                m_ReadScopeTimer.AutoReset = false;
                m_ReadScopeTimer.Enabled = true;
            }
            else
            {
                if (PollMs != m_ReadScopeTimer.Interval)
                {
                    LogMessage("restartReadScopeStatusTimer", $"Polling mount status every {PollMs}ms");
                    m_ReadScopeTimer.Interval = PollMs;
                }
                m_ReadScopeTimer.Start();
            }
        }

        private bool Sync(double ra, double dec)
        {
            if (ra < -24 || +24 < ra)
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync - RightAscension", ra.ToString(), "24-", "+24");
            }
            else if (dec < -90 || +90 < dec)
            {
                throw new ASCOM.InvalidValueException("SlewToCoordinatesAsync - Declination", dec.ToString(), "-90", "+90");
            }
            else
            {
                lock (internalLock)
                {
                    targetMechPosition.RAsky = ra;
                    targetMechPosition.DECsky = dec;
                    m_TargetRightAscension_set = m_TargetDeclination_set = true;

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

                        LogMessage("Sync", $"Mount synced to target RA '{currentMechPosition.RAsky:F2}' DEC '{currentMechPosition.DECsky:F2}'");
                        return true;
                    }

                sync_error:
                    LogMessage("Sync", $"Mount sync to target RA '{ra:F2}' DEC '{dec:F2}' failed");
                    return false;
                }
            }
        }

        private void updateSlewRate(SlewRate rate)
        {
            if (rate != m_SlewRate)
            {
                lock (internalLock)
                {
                    switch (rate)
                    {
                        case SlewRate.SLEW_MAX:
                            sendCmd(":RS#");
                            break;
                        case SlewRate.SLEW_FIND:
                            sendCmd(":RF#");
                            break;
                        case SlewRate.SLEW_CENTER:
                            sendCmd(":RC#");
                            break;
                        case SlewRate.SLEW_GUIDE:
                            sendCmd(":RG#");
                            break;
                        default: return;
                    }
                    m_SlewRate = rate;
                }
            }
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

            LogMessage("getCurrentMechanicalPosition", "Mount mechanical DEC reads '{0}' as {1:F4}.", b, result.DECm);

            if (isSimulation())
                b = simEQ500X.MechanicalRAStr;
            else if (getCommandString(ref b, ":GR#") < 0)
                goto radec_error;

            if (result.parseStringRA(b))
                goto radec_error;

            LogMessage("getCurrentMechanicalPosition", "Mount mechanical RA reads '{0}' as {1:F4}.", b, result.RAm);

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
                String CmdString = String.Format(":Sr{0}#:Sd{1}#", p.toStringRA(ref bufRA), p.toStringDEC(ref bufDEC));
                LogMessage("setTargetMechanicalPosition", "Target RA '{0}' DEC '{0}' converted to '{1}'", p.RAm, p.DECm, CmdString);

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
            LogMessage("SendCmd", "<{0}>", data);
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
                LogMessage("getReply", "<{0}>", data);
            }
            return 0;
        }

        private int getCommandString(ref String data, String cmd)
        {
            LogMessage("getCommandString", "CMD <{0}>", cmd);

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

            LogMessage("getCommandString", "RES <{0}>", data);

            return 0;
        }

        private bool isSimulation()
        {
            return isSimulated;
        }
        #endregion
    }
}
