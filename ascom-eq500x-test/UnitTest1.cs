using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ascom_eq500x_test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var device = new ASCOM.DriverAccess.Telescope("ASCOM.EQ500X.Telescope"))
            {
                Assert.AreEqual("Omegon EQ500X", device.Name);
                Assert.AreEqual("ASCOM Telescope Driver for EQ500X.", device.Description);
                Assert.AreEqual("6.4", device.DriverVersion);
            }
        }
    }
}
