using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.EQ500X;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ASCOM.EQ500X
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        private ASCOM.Utilities.Util m_Util = new ASCOM.Utilities.Util();
        private const string m_DMSRegex = @"\d+[° ]+(\d+[' ]+(\d+["" ]*)*)*";
        private static readonly char[] m_ElevationSymbols = { ' ', 'm' };

        private readonly IFormatProvider formatProvider = new CultureInfo("en-GB", true);

        public SetupDialogForm()
        {
            InitializeComponent();
            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here
            // Update the state variables with results from the dialogue
            Telescope.comPort = (string)comboBoxComPort.SelectedItem;
            Telescope.tl.Enabled = chkTrace.Checked;
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
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

        private void InitUI()
        {
            chkTrace.Checked = Telescope.tl.Enabled;
            // set the list of com ports to those that are currently available
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());      // use System.IO because it's static
            // select the current port if possible
            if (comboBoxComPort.Items.Contains(Telescope.comPort))
            {
                comboBoxComPort.SelectedItem = Telescope.comPort;
            }
            this.LongitudeBox.Text = m_Util.DegreesToDMS(Telescope.m_LocationProfile.Longitude);
            this.LatitudeBox.Text = m_Util.DegreesToDMS(Telescope.m_LocationProfile.Latitude);
            this.ElevationBox.Text = Telescope.m_LocationProfile.Elevation.ToString(formatProvider) + " m";
        }

        private void LongitudeBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Match m = Regex.Match(LongitudeBox.Text, m_DMSRegex);
                double value = m.Success ? m_Util.DMSToDegrees(LongitudeBox.Text) : double.Parse(LongitudeBox.Text, NumberStyles.Number, formatProvider);
                Telescope.m_LocationProfile.Longitude = value;
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
                Telescope.m_LocationProfile.Latitude = value;
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
                Telescope.m_LocationProfile.Elevation = value;
                ElevationBox.Text = Telescope.m_LocationProfile.Elevation.ToString(formatProvider) + " m";
            }
            catch (Exception)
            {
                e.Cancel = true;
                ElevationBox.Select(0, ElevationBox.Text.Length);
            }
        }
    }
}