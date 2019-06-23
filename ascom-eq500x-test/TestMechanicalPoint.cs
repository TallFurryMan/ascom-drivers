using ASCOM.EQ500X;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ascom_eq500x_test
{
    [TestClass]
    public class TestMechanicalPoint
    {
        // Right ascension is normal sexagesimal mapping.
        //
        // HA = LST - RA
        //
        // South is HA = +0,  RA = LST
        // East  is HA = -6,  RA = LST+6
        // North is HA = -12, RA = LST+12 on the east side
        // West  is HA = +6,  RA = LST-6
        // North is HA = +12, RA = LST-12 on the west side
        //
        // Telescope on western side of pier is 12 hours later than
        // telescope on eastern side of pier.
        //
        // PierEast             (LST = -6)           PierWest
        // E +12.0h = LST-18 <-> 12:00:00 <-> LST-18 = +00.0h W
        // N +18.0h = LST-12 <-> 18:00:00 <-> LST-12 = +06.0h N
        // W +00.0h = LST-6  <-> 00:00:00 <-> LST-6  = +12.0h E
        // S +06.0h = LST+0  <-> 06:00:00 <-> LST+0  = +18.0h S
        // E +12.0h = LST+6  <-> 12:00:00 <-> LST+6  = +00.0h W
        // N +18.0h = LST+12 <-> 18:00:00 <-> LST+12 = +06.0h N
        // W +00.0h = LST+18 <-> 00:00:00 <-> LST+18 = +12.0h E

        [TestMethod]
        public void Test_Equality()
        {
            MechanicalPoint p = new MechanicalPoint(), q = new MechanicalPoint();

            p.RAm = 1.23456789;
            p.DECm = 1.23456789;
            p.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL;
            q.RAm = 1.23456789;
            q.DECm = 1.23456789;
            q.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL;
            Assert.IsTrue(p == q);
            Assert.IsFalse(p != q);
            q.PointingState = MechanicalPoint.PointingStates.POINTING_BEYOND_POLE;
            Assert.IsFalse(p == q);
            Assert.IsTrue(p != q);
            q.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL;
            q.RAm = q.RAm + 15.0 / 3600.0;
            Assert.IsFalse(p == q);
            Assert.IsTrue(p != q);
            q.RAm = q.RAm - 15.0 / 3600.0;
            Assert.IsTrue(p == q);
            Assert.IsFalse(p != q);
            q.DECm = q.DECm + 1.0 / 3600.0;
            Assert.IsFalse(p == q);
            Assert.IsTrue(p != q);
            q.DECm = q.DECm - 1.0 / 3600.0;
            Assert.IsTrue(p == q);
            Assert.IsFalse(p != q);
        }

        [TestMethod]
        public void Test_RADistance()
        {
            MechanicalPoint p = new MechanicalPoint(), q = new MechanicalPoint();

            Assert.AreEqual(0.0, p.RAsky = 0.0);
            Assert.AreEqual(1.0, q.RAsky = 1.0);
            Assert.AreEqual(1.0 * 15, p.RA_degrees_to(q));
            Assert.AreEqual(-1.0 * 15, q.RA_degrees_to(p));

            Assert.AreEqual(2.0, q.RAsky = 2.0);
            Assert.AreEqual(2.0 * 15, p.RA_degrees_to(q));
            Assert.AreEqual(-2.0 * 15, q.RA_degrees_to(p));

            Assert.AreEqual(8.0, q.RAsky = 8.0);
            Assert.AreEqual(8.0 * 15, p.RA_degrees_to(q));
            Assert.AreEqual(-8.0 * 15, q.RA_degrees_to(p));

            Assert.AreEqual(12.0, q.RAsky = 12.0);
            Assert.AreEqual(12.0 * 15, p.RA_degrees_to(q));
            Assert.AreEqual(-12.0 * 15, q.RA_degrees_to(p));

            Assert.AreEqual(18.0, q.RAsky = 18.0);
            Assert.AreEqual(-6.0 * 15, p.RA_degrees_to(q));
            Assert.AreEqual(+6.0 * 15, q.RA_degrees_to(p));
        }

        [TestMethod]
        public void Test_PierFlip()
        {
            MechanicalPoint p = new MechanicalPoint();
            String b = "";

            // Mechanical point doesn't care about LST as it assumes the mount
            // is properly synced already. It only considers the pointing state.

            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState = MechanicalPoint.PointingStates.POINTING_BEYOND_POLE);
            Assert.AreEqual(0.0, p.RAsky = +0.0);
            Assert.AreEqual(+90.0, p.DECsky = +90.0);
            Assert.AreEqual("12:00:00", p.toStringRA(ref b));
            Assert.AreEqual("+000:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL);
            Assert.AreEqual(0.0, p.RAsky = +0.0);
            Assert.AreEqual(+90.0, p.DECsky = +90.0);
            Assert.AreEqual("00:00:00", p.toStringRA(ref b));
            Assert.AreEqual("+000:00:00", p.toStringDEC(ref b));

            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState = MechanicalPoint.PointingStates.POINTING_BEYOND_POLE);
            Assert.AreEqual(0.0, p.RAsky = +0.0);
            Assert.AreEqual(+80.0, p.DECsky = +80.0);
            Assert.AreEqual("12:00:00", p.toStringRA(ref b));
            Assert.AreEqual("-010:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL);
            Assert.AreEqual(0.0, p.RAsky = +0.0);
            Assert.AreEqual(+80.0, p.DECsky = +80.0);
            Assert.AreEqual("00:00:00", p.toStringRA(ref b));
            Assert.AreEqual("+010:00:00", p.toStringDEC(ref b));

            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState = MechanicalPoint.PointingStates.POINTING_BEYOND_POLE);
            Assert.AreEqual(0.0, p.RAsky = +0.0);
            Assert.AreEqual(+70.0, p.DECsky = +70.0);
            Assert.AreEqual("12:00:00", p.toStringRA(ref b));
            Assert.AreEqual("-020:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL);
            Assert.AreEqual(0.0, p.RAsky = +0.0);
            Assert.AreEqual(+70.0, p.DECsky = +70.0);
            Assert.AreEqual("00:00:00", p.toStringRA(ref b));
            Assert.AreEqual("+020:00:00", p.toStringDEC(ref b));
        }

        [TestMethod]
        public void Test_Stability_RA_Conversions()
        {
            MechanicalPoint.PointingStates[] sides = { MechanicalPoint.PointingStates.POINTING_NORMAL, MechanicalPoint.PointingStates.POINTING_BEYOND_POLE };
            for (int ps = 0; ps < sides.Length; ps++)
            {
                for (int s = 0; s < 60; s++)
                {
                    for (int m = 0; m < 60; m++)
                    {
                        for (int h = 0; h < 24; h++)
                        {
                            // Locals are on purpose - reset test material on each loop
                            MechanicalPoint p = new MechanicalPoint();
                            String b = "", c = "";

                            p.PointingState = sides[ps];

                            b = $"{h:D2}:{m:D2}:{s:D2}";
                            p.parseStringRA(b);
                            p.toStringRA(ref c);

                            // Debug test with this block
                            if (b != c[0] + c.Substring(2))
                            {
                                p.parseStringRA(b);
                                p.toStringRA(ref c);
                            }

                            Assert.AreEqual(b, c);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Test_Stability_DEC_Conversions()
        {
            // Doesn't test outside of -90,+90 but another test does roughly
            MechanicalPoint.PointingStates[] sides = { MechanicalPoint.PointingStates.POINTING_NORMAL, MechanicalPoint.PointingStates.POINTING_BEYOND_POLE };
            for (int ps = 0; ps < sides.Length; ps++)
            {
                for (int s = 0; s < 60; s++)
                {
                    for (int m = 0; m < 60; m++)
                    {
                        for (int d = -89; d <= +89; d++)
                        {
                            // Locals are on purpose - reset test material on each loop
                            MechanicalPoint p = new MechanicalPoint();
                            String b = "", c = "";

                            p.PointingState = sides[ps];

                            b = $"{d:+00;-00;+00}:{m:00}:{s:00}";
                            p.parseStringDEC(b);
                            p.toStringDEC(ref c);

                            // Debug test with this block
                            if (b != c[0] + c.Substring(2))
                            {
                                p.parseStringDEC(b);
                                p.toStringDEC(ref c);
                            }

                            Assert.AreEqual(b, c[0] + c.Substring(2));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Test_NormalPointing_RA_Conversions()
        {
            MechanicalPoint p = new MechanicalPoint();
            String b = "";


            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState = MechanicalPoint.PointingStates.POINTING_NORMAL);

            Assert.IsFalse(p.parseStringRA("00:00:00"));
            Assert.AreEqual(+0.0, p.RAsky);

            Assert.AreEqual("00:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("06:00:00"));
            Assert.AreEqual(+6.0, p.RAsky);
            Assert.AreEqual("06:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("12:00:00"));
            Assert.AreEqual(+12.0, p.RAsky);
            Assert.AreEqual("12:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("18:00:00"));
            Assert.AreEqual(+18.0, p.RAsky);
            Assert.AreEqual("18:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("24:00:00"));
            Assert.AreEqual(+0.0, p.RAsky);
            Assert.AreEqual("00:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("00:00:01"));
            Assert.IsTrue(Math.Abs(p.RAsky - (1 / 3600.0)) <= (1 / 3600.0));
            Assert.AreEqual("00:00:01", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("00:01:00"));
            Assert.IsTrue(Math.Abs(p.RAsky - (1 / 60.0)) <= (1 / 3600.0));
            Assert.AreEqual("00:01:00", p.toStringRA(ref b));
        }

        [TestMethod]
        public void Test_BeyondPolePointing_RA_Conversions()
        {
            MechanicalPoint p = new MechanicalPoint();
            String b = "";

            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState = MechanicalPoint.PointingStates.POINTING_BEYOND_POLE);

            Assert.IsFalse(p.parseStringRA("00:00:00"));
            Assert.AreEqual(+12.0, p.RAsky);
            Assert.AreEqual("00:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("06:00:00"));
            Assert.AreEqual(+18.0, p.RAsky);
            Assert.AreEqual("06:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("12:00:00"));
            Assert.AreEqual(+0.0, p.RAsky);
            Assert.AreEqual("12:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("18:00:00"));
            Assert.AreEqual(+6.0, p.RAsky);
            Assert.AreEqual("18:00:00", p.toStringRA(ref b));

            Assert.IsFalse(p.parseStringRA("24:00:00"));
            Assert.AreEqual(+12.0, p.RAsky);
            Assert.AreEqual("00:00:00", p.toStringRA(ref b));
        }

        // Declination goes from -255:59:59 to +255:59:59
        //
        // When reading, tenths and hundredths share the same character:
        // - 0-9 is mapped to {0,1,2,3,4,5,6,7,8,9}
        // - 10-16 is mapped to {:,;,<,=,>,?,@}
        // - 17-25 is mapped to {A,B,C,D,E,F,G,H,I}
        //
        // Side of pier is deduced by raw DEC value, which is offset by 90 degrees
        // - raw DEC in [0,+180] means "normal".
        // - raw DEC in [-180,0] means "beyond pole".
        // We support [+270,+256[ (beyond) and ]-256,-270] (normal) for convenience.
        //
        // Beyond        W  Mount DEC  R          Normal
        //(-165.0°)<-> -255:00:00 = -255.0 = -I5:00:00 <-> +345.0°
        //(-135.0°)<-> -225:00:00 = -F5:00:00 <-> +315.0°
        //  -90.0° <-> -180:00:00 = -B0:00:00 <-> +270.0°
        //  -45.0° <-> -135:00:00 = -=5:00:00 <->(+225.0°)
        //  +00.0° <->  -90:00:00 = -90:00:00 <->(+180.0°)
        //  +45.0° <->  -45:00:00 = -45:00:00 <->(+135.0°)
        //  +90.0° <->    0:00:00 = +00:00:00 <->  +90.0°
        //(+135.0°)<->   45:00:00 = +45:00:00 <->  +45.0°
        //(+180.0°)<->   90:00:00 = +90:00:00 <->  +00.0°
        //(+225.0°)<->  135:00:00 = +=5:00:00 <->  -45.0°
        // +270.0°)<->  180:00:00 = +B0:00:00 <->  -90.0°
        // +315.0° <->  225:00:00 = +F5:00:00 <->(-135.0°)
        // +345.0° <->  255:00:00 = +I5:00:00 <->(-165.0°)

        [TestMethod]
        public void Test_Sky_DEC_Conversion()
        {
            MechanicalPoint p = new MechanicalPoint();

            Assert.AreEqual(-255.0, p.DECm = -255.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(-15.0, p.DECsky);
            Assert.AreEqual(-15.0, p.DECsky = -15.0);
            Assert.AreEqual(+105.0, p.DECm);

            Assert.AreEqual(-225.0, p.DECm = -225.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(-45.0, p.DECsky);
            Assert.AreEqual(-45.0, p.DECsky = -45.0);
            Assert.AreEqual(+135.0, p.DECm);

            Assert.AreEqual(-180.0, p.DECm = -180.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.AreEqual(-90.0, p.DECsky);
            Assert.AreEqual(-90.0, p.DECsky = -90.0);
            Assert.AreEqual(-180.0, p.DECm);

            Assert.AreEqual(-135.0, p.DECm = -135.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.AreEqual(-45.0, p.DECsky);
            Assert.AreEqual(-45.0, p.DECsky = -45.0);
            Assert.AreEqual(-135.0, p.DECm);

            Assert.AreEqual(-90.0, p.DECm = -90.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.AreEqual(+0.0, p.DECsky);
            Assert.AreEqual(+0.0, p.DECsky = +0.0);
            Assert.AreEqual(-90.0, p.DECm);

            Assert.AreEqual(-45.0, p.DECm = -45.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.AreEqual(+45.0, p.DECsky);
            Assert.AreEqual(+45.0, p.DECsky = +45.0);
            Assert.AreEqual(-45.0, p.DECm);

            Assert.AreEqual(+0.0, p.DECm = +0.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(+90.0, p.DECsky);
            Assert.AreEqual(+90.0, p.DECsky = +90.0);
            Assert.AreEqual(+0.0, p.DECm);

            Assert.AreEqual(+45.0, p.DECm = +45.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(+45.0, p.DECsky);
            Assert.AreEqual(+45.0, p.DECsky = +45.0);
            Assert.AreEqual(+45.0, p.DECm);

            Assert.AreEqual(+90.0, p.DECm = +90.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(+0.0, p.DECsky);
            Assert.AreEqual(+0.0, p.DECsky = +0.0);
            Assert.AreEqual(+90.0, p.DECm);

            Assert.AreEqual(+135.0, p.DECm = +135.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(-45.0, p.DECsky);
            Assert.AreEqual(-45.0, p.DECsky = -45.0);
            Assert.AreEqual(+135.0, p.DECm);

            Assert.AreEqual(+180.0, p.DECm = +180.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.AreEqual(-90.0, p.DECsky);
            Assert.AreEqual(-90.0, p.DECsky = -90.0);
            Assert.AreEqual(+180.0, p.DECm);

            Assert.AreEqual(+225.0, p.DECm = +225.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.AreEqual(-45.0, p.DECsky);
            Assert.AreEqual(-45.0, p.DECsky = -45.0);
            Assert.AreEqual(-135.0, p.DECm);

            Assert.AreEqual(+255.0, p.DECm = +255.0);
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.AreEqual(-15.0, p.DECsky);
            Assert.AreEqual(-15.0, p.DECsky = -15.0);
            Assert.AreEqual(-105.0, p.DECm);
        }

        [TestMethod]
        public void Test_DEC_Conversions()
        {
            MechanicalPoint p = new MechanicalPoint();
            String b = "";

            Assert.IsFalse(p.parseStringDEC("-I5:00:00"));
            Assert.AreEqual(-255.0, p.DECm);
            Assert.AreEqual("-I5:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("-255:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("-F5:00:00"));
            Assert.AreEqual(-225.0, p.DECm);
            Assert.AreEqual("-F5:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("-225:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("-B0:00:00"));
            Assert.AreEqual(-180.0, p.DECm);
            Assert.AreEqual("-B0:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("-180:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("-=5:00:00"));
            Assert.AreEqual(-135.0, p.DECm);
            Assert.AreEqual("-=5:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("-135:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("-90:00:00"));
            Assert.AreEqual(-90.0, p.DECm);
            Assert.AreEqual("-90:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("-090:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("-45:00:00"));
            Assert.AreEqual(-45.0, p.DECm);
            Assert.AreEqual("-45:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("-045:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+00:00:00"));
            Assert.AreEqual(+0.0, p.DECm);
            Assert.AreEqual("+00:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+000:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+45:00:00"));
            Assert.AreEqual(+45.0, p.DECm);
            Assert.AreEqual("+45:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+045:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+90:00:00"));
            Assert.AreEqual(+90.0, p.DECm);
            Assert.AreEqual("+90:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+090:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+=5:00:00"));
            Assert.AreEqual(+135.0, p.DECm);
            Assert.AreEqual("+=5:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+135:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+B0:00:00"));
            Assert.AreEqual(+180.0, p.DECm);
            Assert.AreEqual("+B0:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+180:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+F5:00:00"));
            Assert.AreEqual(+225.0, p.DECm);
            Assert.AreEqual("+F5:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+225:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+I5:00:00"));
            Assert.AreEqual(+255.0, p.DECm);
            Assert.AreEqual("+I5:00:00", p.toStringDEC_Sim(ref b));
            Assert.AreEqual("+255:00:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("+00:00:01"));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.IsTrue(Math.Abs(p.DECm - (+1 / 3600.0)) <= (1 / 3600.0));
            Assert.AreEqual("+000:00:01", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.IsFalse(p.parseStringDEC("+00:01:00"));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);
            Assert.IsTrue(Math.Abs(p.DECm - (+1 / 60.0)) <= (1 / 3600.0));
            Assert.AreEqual("+000:01:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_NORMAL, p.PointingState);

            Assert.IsFalse(p.parseStringDEC("-00:00:01"));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.IsTrue(Math.Abs(p.DECm - (-1 / 3600.0)) <= (1 / 3600.0));
            Assert.AreEqual("+000:00:01", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.IsFalse(p.parseStringDEC("-00:01:00"));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);
            Assert.IsTrue(Math.Abs(p.DECm - (-1 / 60.0)) <= (1 / 3600.0));
            Assert.AreEqual("+000:01:00", p.toStringDEC(ref b));
            Assert.AreEqual(MechanicalPoint.PointingStates.POINTING_BEYOND_POLE, p.PointingState);

            // Negative tests
            Assert.IsTrue(p.parseStringDEC("+J0:00:00"));
            Assert.IsTrue(p.parseStringDEC("-J0:00:00"));
        }
    }
}
