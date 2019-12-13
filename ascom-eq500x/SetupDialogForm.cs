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
// 31-Oct-2019  ED  1.2     Release 1.2
// --------------------------------------------------------------------------------
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ASCOM.EQ500X.Properties;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;

namespace ASCOM.EQ500X
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        private ASCOM.Utilities.Util m_Util = new ASCOM.Utilities.Util();
        private const string m_DMSRegex = @"\d+[° ]+(\d+[' ]+(\d+["" ]*)*)*";
        private static readonly char[] m_ElevationSymbols = { ' ', 'm' };

        private Telescope.LocationProfile m_LocationProfile = null;
        private string m_PortName;

        private readonly IFormatProvider formatProvider = typeof(Telescope).Assembly.GetName().CultureInfo;

        public SetupDialogForm()
        {
            InitializeComponent();
            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void InitUI()
        {
            // Set localized strings
            Text = Resources.SetupDialog_DialogBox_Text;
            cmdOK.Text = Resources.SetupDialog_DialogOK_Text;
            cmdCancel.Text = Resources.SetupDialog_DialogCancel_Text;
            GeographicalSiteGroupBox.Text = Resources.SetupDialog_GeographicalSite_Text;
            ConnectionGroupBox.Text = Resources.SetupDialog_HardwareConnection_Text;
            chkTrace.Text = Resources.SetupDialog_TraceOn_Text;
            CommPortLabel.Text = Resources.SetupDialog_CommPort_Text;
            LongitudeLabel.Text = Resources.SetupDialog_Longitude_Text;
            LatitudeLabel.Text = Resources.SetupDialog_Latitude_Text;
            ElevationLabel.Text = Resources.SetupDialog_Elevation_Text;

            // Set version link
            ReleaseslinkLabel.Text = typeof(Telescope).Assembly.GetName().Version.ToString();

            // Set the list of com ports to those that are currently available
            PortLookupTimer_Tick(null, null);

            // Set location from profile
            m_LocationProfile = Telescope.m_LocationProfile;
            LongitudeBox.Text = m_Util.DegreesToDMS(m_LocationProfile.Longitude);
            LatitudeBox.Text = m_Util.DegreesToDMS(m_LocationProfile.Latitude);
            ElevationBox.Text = m_LocationProfile.Elevation.ToString(formatProvider) + " m";

            // Set logging from profile
            chkTrace.Checked = Telescope.tl.Enabled;

            // Set communication port from profile
            m_PortName = Telescope.comPort;
            comboBoxComPort.SelectedItem = m_PortName;
            cmdOK.Enabled = (0 < comboBoxComPort.Text.Length);

            // Run port updater
            portLookupTimer.Start();
        }

        private void SetupDialogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_Util?.Dispose();
            m_Util = null;
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            portLookupTimer.Stop();

            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            Telescope.comPort = (string)comboBoxComPort.SelectedItem;
            Telescope.tl.Enabled = chkTrace.Checked;
            Telescope.m_LocationProfile = m_LocationProfile;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            portLookupTimer.Stop();
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void LongitudeBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Match m = Regex.Match(LongitudeBox.Text, m_DMSRegex);
                double value = m.Success ? m_Util.DMSToDegrees(LongitudeBox.Text) : double.Parse(LongitudeBox.Text, NumberStyles.Number, formatProvider);
                m_LocationProfile.Longitude = value;
                LongitudeBox.Text = m_Util.DegreesToDMS(value);
            }
            catch (Exception)
            {
                e.Cancel = true;
                LongitudeBox.Select(0, LongitudeBox.Text.Length);
            }
        }

        private void LatitudeBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Match m = Regex.Match(LongitudeBox.Text, m_DMSRegex);
                double value = m.Success ? m_Util.DMSToDegrees(LatitudeBox.Text) : double.Parse(LatitudeBox.Text, NumberStyles.Number, formatProvider);
                m_LocationProfile.Latitude = value;
                LatitudeBox.Text = m_Util.DegreesToDMS(value);
            }
            catch (Exception)
            {
                e.Cancel = true;
                LatitudeBox.Select(0, LatitudeBox.Text.Length);
            }
        }

        private void ElevationBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                double value = double.Parse(ElevationBox.Text.TrimEnd(m_ElevationSymbols), NumberStyles.Number, formatProvider);
                m_LocationProfile.Elevation = value;
                ElevationBox.Text = Telescope.m_LocationProfile.Elevation.ToString(formatProvider) + " m";
            }
            catch (Exception)
            {
                e.Cancel = true;
                ElevationBox.Select(0, ElevationBox.Text.Length);
            }
        }

        private void ReleaseslinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/TallFurryMan/ascom-drivers/releases");
        }

        private void PortLookupTimer_Tick(object sender, EventArgs e)
        {
            List<string> ports = new List<string>(System.IO.Ports.SerialPort.GetPortNames());

            if (ports.Count + 1 != comboBoxComPort.Items.Count)
            {
                ComboBoxComPort_FillDropList(ports);
            }
            else
            {
                for (int i = 0; i < comboBoxComPort.Items.Count - 1; i++)
                {
                    if (comboBoxComPort.Items[i].ToString() != ports[i])
                    {
                        ComboBoxComPort_FillDropList(ports);
                        break;
                    }
                }
            }

            portLookupTimer.Start();
        }

        private void ComboBoxComPort_FillDropList(List<string> ports)
        {
            comboBoxComPort.BeginUpdate();
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(ports.ToArray());
            comboBoxComPort.Items.Add(Resources.SetupDialog_AutoSearch_Text);
            comboBoxComPort.EndUpdate();

            // Attempt to select the port that was validated earlier
            comboBoxComPort.SelectedItem = m_PortName;

            // Fall back to automatic if port name vanished, but keep the original name in mind
            if (null == comboBoxComPort.SelectedItem)
                comboBoxComPort.SelectedItem = Resources.SetupDialog_AutoSearch_Text;
        }

        private void ComboBoxComPort_Validating(object sender, CancelEventArgs e)
        {
            // Choose automatic if selected port is unknown
            if (null == comboBoxComPort.SelectedItem)
                comboBoxComPort.SelectedItem = Resources.SetupDialog_AutoSearch_Text;

            // And remember that port as what the user selected for the next port refresh
            m_PortName = comboBoxComPort.SelectedItem.ToString();
        }
    }
}
