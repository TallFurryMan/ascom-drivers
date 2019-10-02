// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

// Names from the INDI driver
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.atParkingPosition~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.parseStringDEC(System.String)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.parseStringRA(System.String)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.toStringDEC(System.String@)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.toStringRA(System.String@)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.toStringDEC_Sim(System.String@)~System.String")]

// Names from the INDI driver
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707", Justification = "Names match the INDI driver implementation", Scope = "type", Target = "~T:ASCOM.EQ500X.MechanicalPoint.PointingStates")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.toStringDEC_Sim(System.String@)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.RA_degrees_to(ASCOM.EQ500X.MechanicalPoint)~System.Double")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707", Justification = "Names match the INDI driver implementation", Scope = "member", Target = "~M:ASCOM.EQ500X.MechanicalPoint.DEC_degrees_to(ASCOM.EQ500X.MechanicalPoint)~System.Double")]

// ASCOM-related restrictions
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065", Justification = "ASCOM-related", Scope = "member", Target = "~P:ASCOM.EQ500X.Telescope.Azimuth")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065", Justification = "ASCOM-related", Scope = "member", Target = "~P:ASCOM.EQ500X.Telescope.Altitude")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065", Justification = "ASCOM-related", Scope = "member", Target = "~P:ASCOM.EQ500X.Telescope.ApertureArea")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065", Justification = "ASCOM-related", Scope = "member", Target = "~P:ASCOM.EQ500X.Telescope.ApertureDiameter")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1065", Justification = "ASCOM-related", Scope = "member", Target = "~P:ASCOM.EQ500X.Telescope.SlewSettleTime")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010", Justification = "ASCOM-related", Scope = "type", Target = "~T:ASCOM.EQ500X.AxisRates")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710", Justification = "ASCOM-related", Scope = "type", Target = "~T:ASCOM.EQ500X.AxisRates")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010", Justification = "ASCOM-related", Scope = "type", Target = "~T:ASCOM.EQ500X.TrackingRates")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710", Justification = "ASCOM-related", Scope = "type", Target = "~T:ASCOM.EQ500X.TrackingRates")]
