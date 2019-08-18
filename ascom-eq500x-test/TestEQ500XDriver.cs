using System;
using ASCOM.EQ500X;
using ASCOM.DeviceInterface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ascom_eq500x_test
{
    [TestClass]
    public class TestEQ500XDriver
    {
        public ASCOM.DriverAccess.Telescope device;

        public bool getCurrentMechanicalPosition(ref MechanicalPoint p)
        {
            Assert.IsTrue(device.Connected);
            String result = device.CommandString("getCurrentMechanicalPosition", false);
            String[] radec = result.Split(new char[] { ' ' });
            Assert.IsFalse(p.parseStringRA(radec[0]));
            Assert.IsFalse(p.parseStringDEC(radec[1]));
            return false;
        }

        [TestInitialize]
        public void setUp()
        {
            device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope");
            device.CommandBool("Simulated", true);
        }

        [TestCleanup]
        public void tearDown()
        {
            device.Dispose();
            device = null;
        }

        [TestMethod]
        public void TestDriverProperties()
        {
            using (var device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope"))
            {
                Assert.AreEqual("Omegon EQ500X", device.Name);
                Assert.AreEqual("ASCOM Telescope Driver for EQ500X.", device.Description);
                Assert.AreEqual("6.4", device.DriverVersion);
            }
        }

        [TestMethod]
        public void TestDriverSupportedFeatures()
        {
            using (var device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope"))
            {
                Assert.AreEqual(0, device.SupportedActions.Count);

                /* Moving */
                Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.MoveAxis(TelescopeAxes.axisPrimary, 0));
                Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.MoveAxis(TelescopeAxes.axisSecondary, 0));
                Assert.IsTrue(device.CanMoveAxis(TelescopeAxes.axisPrimary));
                Assert.IsTrue(device.CanMoveAxis(TelescopeAxes.axisSecondary));
                Assert.IsFalse(device.CanMoveAxis(TelescopeAxes.axisTertiary));

                /* Tracking */
                Assert.IsFalse(device.CanSetDeclinationRate);
                Assert.IsFalse(device.CanSetRightAscensionRate);
                Assert.IsFalse(device.CanSetTracking);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DeclinationRate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DeclinationRate = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.RightAscensionRate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.RightAscensionRate = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Tracking);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Tracking = false);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TrackingRate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TrackingRate = 0);

                /* Slewing */
                Assert.IsTrue(device.CanSlew);
                Assert.IsFalse(device.CanSlewAltAz);
                Assert.IsFalse(device.CanSlewAltAzAsync);
                Assert.IsTrue(device.CanSlewAsync);
                //Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.AbortSlew);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToAltAz(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToAltAzAsync(0, 0));
                //Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToCoordinates(0, 0));
                //Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToCoordinatesAsync(0, 0));
                //Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.SlewToTarget);
                //Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.SlewToTargetAsync);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Slewing);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DestinationSideOfPier(0, 0));
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SlewSettleTime);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SlewSettleTime = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetDeclination);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetDeclination = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetRightAscension);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetRightAscension = 0);

                /* Parking */
                Assert.IsFalse(device.CanFindHome);
                Assert.IsFalse(device.CanPark);
                Assert.IsFalse(device.CanSetPark);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.FindHome);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.FindHome);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.SetPark);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.Unpark);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.AtHome);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.AtPark);

                /* Guiding */
                Assert.IsTrue(device.CanPulseGuide);
                Assert.IsFalse(device.CanSetGuideRates);
                //Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.PulseGuide(GuideDirections.guideEast, 0));
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateDeclination);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateDeclination = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateRightAscension);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateRightAscension = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.IsPulseGuiding);

                /* Optical */
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.ApertureArea);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.ApertureDiameter);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.ApertureArea);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DoesRefraction);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DoesRefraction = false);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.FocalLength);
            }
        }

        [TestMethod]
        public void TestLocation()
        {
            /* Location */

            Assert.IsFalse(device.Connected);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SiteElevation);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SiteElevation = 0);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SiteLatitude);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SiteLatitude = 0);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SiteLongitude);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SiteLongitude = 0);
            Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.UTCDate = DateTime.UtcNow);

            device.Connected = true;
            //Assert.AreEqual(0, device.SiteElevation);
            //Assert.AreEqual(0, device.SiteLatitude);
            //Assert.AreEqual(0, device.SiteLongitude);
            Assert.AreEqual(DateTime.UtcNow.ToLongTimeString(), device.UTCDate.ToLongTimeString());

            device.SiteLongitude = 0;
            for (double h = -300; h < 10000; h += 100)
            {
                device.SiteElevation = h;
                for (double lat = -90; lat <= 90; lat += 1)
                {
                    device.SiteLatitude = lat;
                    Assert.AreEqual(h, device.SiteElevation);
                    Assert.AreEqual(lat, device.SiteLatitude);
                    Assert.AreEqual(0, device.SiteLongitude);
                }
            }

            Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.UTCDate = DateTime.UtcNow);
        }

        [TestMethod]
        public void TestLSTSync()
        {
            Assert.IsFalse(device.Connected);
            device.Connected = true;

            // Assign a longitude that makes the RA of the scope point east - default position is 90° east
            device.SiteLongitude = 6 * 15;
            Assert.AreEqual(+0.0, device.RightAscension);
            Assert.AreEqual(+90.0, device.Declination);

            // Assign a new longitude
            device.SiteLongitude = 5 * 15;
            Assert.AreEqual(23.0, device.RightAscension);
            Assert.AreEqual(+90.0, device.Declination);

            // Assign a new longitude - but this time the mount is not considered "parked" east/pole and does not sync
            device.SiteLongitude = 7 * 15;
            Assert.AreEqual(23.0, device.RightAscension); // Expected 1h - not possible to assign longitude without restarting the mount
            Assert.AreEqual(+90.0, device.Declination);
        }

        [TestMethod]
        public void TestCoordinates()
        {
            /* Coordinates */
            Assert.AreEqual(AlignmentModes.algGermanPolar, device.AlignmentMode);

            Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Altitude);
            Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Azimuth);

            Assert.IsFalse(device.Connected);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.Declination);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.RightAscension);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SideOfPier);
            Assert.ThrowsException<ASCOM.NotConnectedException>(() => device.SideOfPier = 0);
            Assert.IsFalse(device.CanSetPierSide);

            device.Connected = true;
            Assert.AreEqual(90, device.Declination);
            Assert.AreEqual(0, device.RightAscension);
        }

        [TestMethod]
        public void TestSyncing()
        {
            Assert.IsFalse(device.Connected);
            device.Connected = true;
            Assert.IsTrue(device.CanSync);
            Assert.IsFalse(device.CanSyncAltAz);
            Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SyncToAltAz(0, 0));
            /*
            for (int ra = 0; ra < 24 * 60 * 60; ra++)
            {
                for (int dec = -89 * 60 * 60; dec < 89 * 60 * 60; dec++)
                {
                    double RA = ra / 3600.0;
                    double DEC = dec / 3600.0;
                    device.SyncToCoordinates(RA, DEC);
                    Assert.AreEqual(RA, device.RightAscension);
                    Assert.AreEqual(DEC, device.Declination);
                }
            }
            */

            MechanicalPoint p = new MechanicalPoint();
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(0.0, p.RAm);
            Assert.AreEqual(0.0, p.DECm);
            Assert.AreEqual(0.0, p.RAsky);
            Assert.AreEqual(90.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SyncToCoordinates(-24.1, 0));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SyncToCoordinates(+24.1, 0));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SyncToCoordinates(0, -91));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SyncToCoordinates(0, +91));

            device.SyncToCoordinates(0, 0);
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(0.0, p.RAm);
            Assert.AreEqual(90.0, p.DECm);
            Assert.AreEqual(0.0, p.RAsky);
            Assert.AreEqual(0.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            device.SyncToCoordinates(10, 0);
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(10.0, p.RAm);
            Assert.AreEqual(90.0, p.DECm);
            Assert.AreEqual(10.0, p.RAsky);
            Assert.AreEqual(0.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            device.SyncToCoordinates(14, 0);
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(14.0, p.RAm);
            Assert.AreEqual(90.0, p.DECm);
            Assert.AreEqual(14.0, p.RAsky);
            Assert.AreEqual(0.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            device.SyncToCoordinates(0, 10);
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(0.0, p.RAm);
            Assert.AreEqual(80.0, p.DECm);
            Assert.AreEqual(0.0, p.RAsky);
            Assert.AreEqual(10.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            device.SyncToCoordinates(0, -10);
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(0.0, p.RAm);
            Assert.AreEqual(100.0, p.DECm);
            Assert.AreEqual(0.0, p.RAsky);
            Assert.AreEqual(-10.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            device.SyncToCoordinates(14, -10);
            Assert.IsFalse(getCurrentMechanicalPosition(ref p));
            Assert.AreEqual(14.0, p.RAm);
            Assert.AreEqual(100.0, p.DECm);
            Assert.AreEqual(14.0, p.RAsky);
            Assert.AreEqual(-10.0, p.DECsky);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
        }

        [TestMethod]
        public void TestConnection()
        {
            using (var device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope"))
            {
                bool dummy = false;
                Assert.IsFalse(device.Connected);
                Assert.IsFalse(device.CommandBool("isSimulated", dummy));
                try
                {
                    device.Connected = true;
                    //Assert.IsTrue(device.Connected);
                    //Assert.IsTrue(device.CommandBool("isSimulated", dummy));
                    device.Connected = false;
                }
                catch (Exception) { }
                Assert.IsFalse(device.Connected);
                Assert.IsFalse(device.CommandBool("isSimulated", dummy));
            }
        }

        [TestMethod]
        public void TestSimulation()
        {
            using (var device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope"))
            {
                bool dummy = false;
                Assert.IsFalse(device.Connected);
                Assert.IsFalse(device.CommandBool("isSimulated", dummy));
                Assert.IsTrue(device.CommandBool("Simulated", true));
                Assert.IsTrue(device.CommandBool("isSimulated", dummy));
                device.Connected = true;
                Assert.IsTrue(device.Connected);
                Assert.IsTrue(device.CommandBool("isSimulated", dummy));
                device.Connected = false;
                Assert.IsFalse(device.Connected);
                Assert.IsTrue(device.CommandBool("isSimulated", dummy));
                Assert.IsFalse(device.CommandBool("Simulated", false));
                Assert.IsFalse(device.CommandBool("isSimulated", dummy));
            }
        }

        [TestMethod]
        public void TestReadScopeStatus()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(0, device.RightAscension);
            Assert.AreEqual(90, device.Declination);
            Assert.AreEqual(1000, int.Parse(device.CommandString("getReadScopeStatusInterval", true)));
        }

        [TestMethod]
        public void Test_InterfaceValidations()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);

            Assert.ThrowsException<ASCOM.ValueNotSetException>(() => device.SiteLongitude);
            Assert.ThrowsException<ASCOM.ValueNotSetException>(() => device.SiteLatitude);
            Assert.ThrowsException<ASCOM.ValueNotSetException>(() => device.SiteElevation);

            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SiteLongitude = -181);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SiteLongitude = +181);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SiteLatitude = -91);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SiteLatitude = +91);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SiteElevation = -301);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SiteElevation = +10001);

            Assert.ThrowsException<ASCOM.ValueNotSetException>(() => device.TargetDeclination);
            Assert.ThrowsException<ASCOM.ValueNotSetException>(() => device.TargetRightAscension);

            Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.Tracking = true);

            Assert.AreEqual(DriveRates.driveSidereal, device.TrackingRate);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.TrackingRate = DriveRates.driveKing);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.TrackingRate = DriveRates.driveLunar);
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.TrackingRate = DriveRates.driveSolar);

            Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.DestinationSideOfPier(0, 0));
        }

        [TestMethod]
        public void Test_RatesAndMoveAxis()
        {
            // Unsimulated movement causing no M commands, but testing IRates
            device.Connected = true;
            Assert.IsTrue(device.Connected);

            IAxisRates ra_rates = device.AxisRates(TelescopeAxes.axisPrimary);
            IAxisRates dec_rates = device.AxisRates(TelescopeAxes.axisSecondary);

            Assert.AreEqual(4, ra_rates.Count);
            Assert.AreEqual(5.0 / 3600.0, ra_rates[1].Minimum); // 1-based
            Assert.AreEqual(5.0 / 60.0, ra_rates[2].Minimum);
            Assert.AreEqual(20.0 / 60.0, ra_rates[3].Minimum);
            Assert.AreEqual(5.0, ra_rates[4].Minimum);

            Assert.AreEqual(4, dec_rates.Count);
            Assert.AreEqual(5.0 / 3600.0, dec_rates[1].Minimum);
            Assert.AreEqual(5.0 / 60.0, dec_rates[2].Minimum);
            Assert.AreEqual(20.0 / 60.0, dec_rates[3].Minimum);
            Assert.AreEqual(5.0, dec_rates[4].Minimum);

            device.MoveAxis(TelescopeAxes.axisPrimary, 0.0);
            device.MoveAxis(TelescopeAxes.axisSecondary, 0.0);
            Assert.IsTrue(device.Tracking);

            Assert.IsTrue(ra_rates.Count == dec_rates.Count);
            string[] rates = new string[] { "SLEW_GUIDE", "SLEW_CENTER", "SLEW_FIND", "SLEW_MAX" };

            for (int i = 1; i < ra_rates.Count + 1; i++)
            {
                IRate ra_rate = ra_rates[i];
                Assert.IsTrue(ra_rate.Minimum == ra_rate.Maximum);
                Assert.IsTrue(0 < ra_rate.Minimum);

                IRate dec_rate = dec_rates[i];
                Assert.IsTrue(dec_rate.Minimum == dec_rate.Maximum);
                Assert.IsTrue(0 < dec_rate.Minimum);

                Assert.IsTrue(device.Tracking);
                Assert.AreEqual(rates[3], device.CommandString("getCurrentSlewRate", true));
                Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.MoveAxis(TelescopeAxes.axisPrimary, ra_rate.Minimum + 1 / 3600.0));
                Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.MoveAxis(TelescopeAxes.axisSecondary, dec_rate.Minimum + 1 / 3600.0));

                device.MoveAxis(TelescopeAxes.axisPrimary, ra_rate.Minimum);
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
                device.MoveAxis(TelescopeAxes.axisSecondary, dec_rate.Minimum);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
                Assert.IsTrue(device.Slewing);
                Assert.AreEqual(rates[i - 1], device.CommandString("getCurrentSlewRate", true));

                device.AbortSlew();
                Assert.IsTrue(device.Tracking);

                device.MoveAxis(TelescopeAxes.axisPrimary, -ra_rate.Minimum);
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
                device.MoveAxis(TelescopeAxes.axisSecondary, -dec_rate.Minimum);
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
                Assert.AreEqual(rates[i - 1], device.CommandString("getCurrentSlewRate", true));

                device.AbortSlew();
                Assert.IsTrue(device.Tracking);
                Assert.AreEqual(rates[3], device.CommandString("getCurrentSlewRate", true));
            }

            // One single rate for both axes
            device.MoveAxis(TelescopeAxes.axisPrimary, ra_rates[1].Minimum);
            Assert.IsTrue(device.Slewing);
            Assert.ThrowsException<ASCOM.InvalidOperationException> (() => device.MoveAxis(TelescopeAxes.axisSecondary, dec_rates[2].Minimum));
            Assert.IsTrue(device.Slewing);
            device.AbortSlew();
            Assert.IsTrue(device.Tracking);
        }

        [TestMethod]
        public void Test_PulseGuiding()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);

            Assert.AreEqual("SLEW_MAX", device.CommandString("getCurrentSlewRate", true));

            // Zero-time pulse guiding: no-op
            foreach (GuideDirections direction in Enum.GetValues(typeof(GuideDirections)))
            {
                device.PulseGuide(direction, 0);
                Assert.IsTrue(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsFalse(device.IsPulseGuiding);
            }

            // Single-axis pulse guiding
            foreach (GuideDirections direction in Enum.GetValues(typeof(GuideDirections)))
            {
                device.PulseGuide(direction, 500);
                Assert.IsFalse(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsTrue(device.IsPulseGuiding);
                System.Threading.Thread.Sleep(200);
                Assert.IsFalse(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsTrue(device.IsPulseGuiding);
                System.Threading.Thread.Sleep(350);
                Assert.IsTrue(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsFalse(device.IsPulseGuiding);
            }

            Assert.AreEqual("SLEW_MAX", device.CommandString("getCurrentSlewRate", true));

            // Dual-axis pulse guiding, multiple times
            for (int i = 0; i < 5; i++)
            {
                device.PulseGuide(GuideDirections.guideWest, 500);
                Assert.AreEqual("SLEW_GUIDE", device.CommandString("getCurrentSlewRate", true));
                System.Threading.Thread.Sleep(200);
                device.PulseGuide(GuideDirections.guideNorth, 500);
                Assert.IsFalse(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsTrue(device.IsPulseGuiding);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
                Assert.AreEqual("SLEW_GUIDE", device.CommandString("getCurrentSlewRate", true));
                System.Threading.Thread.Sleep(350);
                Assert.IsFalse(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsTrue(device.IsPulseGuiding);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
                Assert.AreEqual("SLEW_GUIDE", device.CommandString("getCurrentSlewRate", true));
                System.Threading.Thread.Sleep(200);
                Assert.IsTrue(device.Tracking);
                Assert.IsFalse(device.Slewing);
                Assert.IsFalse(device.IsPulseGuiding);
                Assert.AreEqual("SLEW_MAX", device.CommandString("getCurrentSlewRate", true));
            }

            // Duplicate guide command
            device.PulseGuide(GuideDirections.guideWest, 500);
            device.PulseGuide(GuideDirections.guideWest, 100);
            System.Threading.Thread.Sleep(150);
            Assert.IsTrue(device.Tracking);
            Assert.IsFalse(device.Slewing);
            Assert.IsFalse(device.IsPulseGuiding);
        }

        [TestMethod]
        public void Test_Goto_NoMovement()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
            double target_ra = device.RightAscension, target_dec = device.Declination;
            device.SlewToCoordinatesAsync(target_ra, target_dec);
            Assert.AreEqual(target_ra, device.TargetRightAscension);
            Assert.AreEqual(target_dec, device.TargetDeclination);
            Assert.IsTrue(device.Slewing);
            for (int i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(200);
                Assert.AreEqual(target_ra, device.TargetRightAscension);
                Assert.AreEqual(target_dec, device.TargetDeclination);
                if (device.Tracking) break;
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            }
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);

            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinates(-24.1, 0));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinates(+24.1, 0));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinates(0, -91));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinates(0, +91));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinatesAsync(-24.1, 0));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinatesAsync(+24.1, 0));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinatesAsync(0, -91));
            Assert.ThrowsException<ASCOM.InvalidValueException>(() => device.SlewToCoordinatesAsync(0, +91));
        }

        [TestMethod]
        public void Test_Goto_AbortMovement()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.AreEqual(0, device.RightAscension);
            Assert.AreEqual(90, device.Declination);
            double target_ra = -1, target_dec = 80;
            device.SlewToCoordinatesAsync(target_ra, target_dec);
            target_ra = 23; // -1 on other side
            Assert.AreEqual(target_ra, device.TargetRightAscension);
            Assert.AreEqual(target_dec, device.TargetDeclination);
            Assert.IsTrue(device.Slewing);
            Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            for (int i = 0; i < 4; i++)
            {
                int statusInterval = int.Parse(device.CommandString("getReadScopeStatusInterval", true));
                int seconds = statusInterval / 1000;
                System.Threading.Thread.Sleep(seconds * 1000 + (statusInterval - seconds * 1000));
                Assert.AreEqual(target_ra, device.TargetRightAscension);
                Assert.AreEqual(target_dec, device.TargetDeclination);
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            }
            Assert.IsTrue(device.Slewing);
            device.AbortSlew();
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(1000, int.Parse(device.CommandString("getReadScopeStatusInterval", true)));
        }

        [TestMethod]
        public void Test_Goto_SouthMovement()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
            Assert.AreEqual(0, device.RightAscension);
            Assert.AreEqual(90, device.Declination);
            double target_ra = 0, target_dec = 80;
            device.SlewToCoordinatesAsync(target_ra, target_dec);
            Assert.AreEqual(target_ra, device.TargetRightAscension);
            Assert.AreEqual(target_dec, device.TargetDeclination);
            Assert.IsTrue(device.Slewing);
            Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            for (int i = 0; i < 150; i++)
            {
                int statusInterval = int.Parse(device.CommandString("getReadScopeStatusInterval", true));
                int seconds = statusInterval / 1000;
                System.Threading.Thread.Sleep(seconds * 1000 + (statusInterval - seconds * 1000));
                Assert.AreEqual(target_ra, device.TargetRightAscension);
                Assert.AreEqual(target_dec, device.TargetDeclination);
                if (device.Tracking) break;
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            }
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
        }

        [TestMethod]
        public void Test_Goto_NorthMovement()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
            Assert.AreEqual(0, device.RightAscension);
            Assert.AreEqual(90, device.Declination);
            double target_ra = 0, target_dec = 100;
            device.SlewToCoordinatesAsync(target_ra, target_dec);
            target_dec = -80; // 100 on other side
            Assert.AreEqual(target_ra, device.TargetRightAscension);
            Assert.AreEqual(target_dec, device.TargetDeclination);
            Assert.IsTrue(device.Slewing);
            Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            for (int i = 0; i < 150; i++)
            {
                int statusInterval = int.Parse(device.CommandString("getReadScopeStatusInterval", true));
                int seconds = statusInterval / 1000;
                System.Threading.Thread.Sleep(seconds * 1000 + (statusInterval - seconds * 1000));
                Assert.AreEqual(target_ra, device.TargetRightAscension);
                Assert.AreEqual(target_dec, device.TargetDeclination);
                if (device.Tracking) break;
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            }
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
        }

        [TestMethod]
        public void Test_Goto_EastMovement()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
            Assert.AreEqual(0, device.RightAscension);
            Assert.AreEqual(90, device.Declination);
            double target_ra = +1, target_dec = 90;
            device.SlewToCoordinatesAsync(target_ra, target_dec);
            Assert.AreEqual(target_ra, device.TargetRightAscension);
            Assert.AreEqual(target_dec, device.TargetDeclination);
            Assert.IsTrue(device.Slewing);
            Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            for (int i = 0; i < 150; i++)
            {
                int statusInterval = int.Parse(device.CommandString("getReadScopeStatusInterval", true));
                int seconds = statusInterval / 1000;
                System.Threading.Thread.Sleep(seconds * 1000 + (statusInterval - seconds * 1000));
                Assert.AreEqual(target_ra, device.TargetRightAscension);
                Assert.AreEqual(target_dec, device.TargetDeclination);
                if (device.Tracking) break;
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            }
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(PierSide.pierEast, device.SideOfPier);
        }

        [TestMethod]
        public void Test_Goto_WestMovement()
        {
            device.Connected = true;
            Assert.IsTrue(device.Connected);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
            Assert.AreEqual(0, device.RightAscension);
            Assert.AreEqual(90, device.Declination);
            double target_ra = -1, target_dec = 90;
            device.SlewToCoordinatesAsync(target_ra, target_dec);
            target_ra = 23; // -1 on other side
            Assert.AreEqual(target_ra, device.TargetRightAscension);
            Assert.AreEqual(target_dec, device.TargetDeclination);
            Assert.IsTrue(device.Slewing);
            Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            for (int i = 0; i < 150; i++)
            {
                int statusInterval = int.Parse(device.CommandString("getReadScopeStatusInterval", true));
                int seconds = statusInterval / 1000;
                System.Threading.Thread.Sleep(seconds * 1000 + (statusInterval - seconds * 1000));
                Assert.AreEqual(target_ra, device.TargetRightAscension);
                Assert.AreEqual(target_dec, device.TargetDeclination);
                if (device.Tracking) break;
                Assert.IsTrue(device.Slewing);
                Assert.ThrowsException<ASCOM.InvalidOperationException>(() => device.SyncToCoordinates(0, 0));
            }
            Assert.IsTrue(device.Tracking);
            Assert.AreEqual(PierSide.pierWest, device.SideOfPier);
        }
    }
}