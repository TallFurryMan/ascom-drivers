﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ascom_eq500x_test
{
    [TestClass]
    public class TestEQ500XDriver
    {
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
            }
        }

        [TestMethod]
        public void TestDriverUnsupportedFeatures()
        {
            using (var device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope"))
            {
                Assert.AreEqual(0, device.SupportedActions.Count);

                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.AbortSlew);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.FindHome);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.MoveAxis(ASCOM.DeviceInterface.TelescopeAxes.axisPrimary, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.PulseGuide(ASCOM.DeviceInterface.GuideDirections.guideEast, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.FindHome);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.SetPark);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToAltAz(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToAltAzAsync(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToCoordinates(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SlewToCoordinatesAsync(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.SlewToTarget);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.SlewToTargetAsync);
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SyncToAltAz(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(() => device.SyncToCoordinates(0, 0));
                Assert.ThrowsException<ASCOM.MethodNotImplementedException>(device.Unpark);

                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.AlignmentMode);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Altitude);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.ApertureArea);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.ApertureDiameter);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.ApertureArea);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.AtHome);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.AtPark);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Azimuth);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Declination);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DeclinationRate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DeclinationRate = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DoesRefraction);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DoesRefraction = false);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.FocalLength);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateDeclination);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateDeclination = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateRightAscension);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.GuideRateRightAscension = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.IsPulseGuiding);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.RightAscension);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.RightAscensionRate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.RightAscensionRate = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SideOfPier);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SideOfPier = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SiteElevation);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SiteElevation = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SiteLatitude);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SiteLatitude = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SiteLongitude);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SiteLongitude = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SlewSettleTime);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.SlewSettleTime = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Slewing);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetDeclination);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetDeclination = 0);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetRightAscension);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TargetRightAscension = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Tracking);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.Tracking = false);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TrackingRate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.TrackingRate = 0);
                //Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.UTCDate);
                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.UTCDate = DateTime.UtcNow);

                Assert.ThrowsException<ASCOM.PropertyNotImplementedException>(() => device.DestinationSideOfPier(0, 0));

                Assert.IsFalse(device.CanFindHome);
                Assert.IsFalse(device.CanPark);
                Assert.IsFalse(device.CanPulseGuide);
                Assert.IsFalse(device.CanSetDeclinationRate);
                Assert.IsFalse(device.CanSetGuideRates);
                Assert.IsFalse(device.CanSetPark);
                Assert.IsFalse(device.CanSetPierSide);
                Assert.IsFalse(device.CanSetRightAscensionRate);
                Assert.IsFalse(device.CanSetTracking);
                Assert.IsFalse(device.CanSlew);
                Assert.IsFalse(device.CanSlewAltAz);
                Assert.IsFalse(device.CanSlewAltAzAsync);
                Assert.IsFalse(device.CanSlewAsync);
                Assert.IsFalse(device.CanSync);
                Assert.IsFalse(device.CanSyncAltAz);

            }
        }
    }
}
