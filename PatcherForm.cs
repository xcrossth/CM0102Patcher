﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CM0102Patcher
{
    public partial class PatcherForm : Form
    {
        public PatcherForm()
        {
            InitializeComponent();

            try
            {
                var exeLocation = (string)Registry.GetValue(RegString.GetRegString(), "Location", "");
                if (!string.IsNullOrEmpty(exeLocation))
                {
                    labelFilename.Text = Path.Combine(exeLocation, "cm0102.exe");
                }
                comboBoxGameSpeed.Items.AddRange(new ComboboxItem[]
                {
                    new ComboboxItem("don't modify", 0),
                    new ComboboxItem("x0.5", 20000),
                    new ComboboxItem("default", 10000),
                    new ComboboxItem("x2", 5000),
                    new ComboboxItem("x4", 2500),
                    new ComboboxItem("x8", 1250),
                    new ComboboxItem("x20", 500),
                    new ComboboxItem("x200", 50),
                    new ComboboxItem("Max", 1)
                });
                comboBoxGameSpeed.SelectedIndex = 4;

                // Set selectable leagues
                comboBoxReplacementLeagues.Items.Add("English National League North");
                comboBoxReplacementLeagues.Items.Add("English National League South");
                comboBoxReplacementLeagues.Items.Add("English Southern Premier Central Division");

                // Screen Resolution
                comboBoxResolution.Items.AddRange(new ComboboxItem[]
                {
                    new ComboboxItem("720 x 480", new Point(720, 480)),
                    new ComboboxItem("800 x 600 (default)", new Point(800, 600)),
                    new ComboboxItem("1024 x 600", new Point(1024, 600)),
                    new ComboboxItem("1024 x 768", new Point(1024, 768)),
                    new ComboboxItem("1280 x 720", new Point(1280, 720)),
                    new ComboboxItem("1280 x 800 (recommended)", new Point(1280, 800)),
                    new ComboboxItem("1280 x 960", new Point(1280, 960)),
                    new ComboboxItem("1280 x 1024", new Point(1280, 1024)),
                    new ComboboxItem("1366 x 768", new Point(1366, 768)),
                    new ComboboxItem("1400 x 900", new Point(1400, 900)),
                    new ComboboxItem("1680 x 1050", new Point(1680, 1050)),
                    new ComboboxItem("1920 x 1080", new Point(1920, 1080)),
                });

                // Set Default Start Year to this year if we're past July (else use last year) 
                var currentYear = DateTime.Now.Year;
                if (DateTime.Now.Month < 7)
                    currentYear--;
                numericGameStartYear.Value = currentYear;

                TapaniDetection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TapaniDetection()
        {
            try
            {
                var windowText = " - (TAPANI EXE DETECTED)";
                var exeFile = labelFilename.Text;
                // Use the pattern finder in the NoCD patcher
                bool isTapani = false;
                NoCDPatch.FindPattern(exeFile, Encoding.ASCII.GetBytes("Tapani v"), (file, br, bw, offset) => { isTapani = true; });
                if (!isTapani)
                    NoCDPatch.FindPattern(exeFile, Encoding.ASCII.GetBytes("TapaniPatch"), (file, br, bw, offset) => { isTapani = true; });
                if (!isTapani)
                    NoCDPatch.FindPattern(exeFile, Encoding.ASCII.GetBytes("Tapani 2"), (file, br, bw, offset) => { isTapani = true; });
                this.Text = this.Text.Replace(windowText, "");
                if (isTapani)
                {
                    ResetControls(this);
                    this.Text += windowText;
                }
                checkBoxChangeStartYear.Enabled = !isTapani;
                checkBoxIdleSensitivity.Enabled = !isTapani;
                checkBoxCDRemoval.Enabled = !isTapani;
                checkBoxDisableSplashScreen.Enabled = !isTapani;
                checkBox7Subs.Enabled = !isTapani;
                checkBoxAllowCloseWindow.Enabled = !isTapani;
                checkBoxShowStarPlayers.Enabled = !isTapani;
                checkBoxRegenFixes.Enabled = !isTapani;
                checkBoxJobsAbroadBoost.Enabled = !isTapani;
                checkBoxRemove3NonEULimit.Enabled = !isTapani;
                checkBoxReplaceWelshPremier.Enabled = !isTapani;
                checkBoxNewRegenCode.Enabled = !isTapani;
                checkBoxManageAnyTeam.Enabled = !isTapani;
                checkBoxUpdateNames.Enabled = !isTapani;
                checkBoxSwapSKoreaForChina.Enabled = !isTapani;
                checkBoxChangeStartYear_CheckedChanged(null, null);

                // Do some defaults - as it seems to confuse people without
                checkBoxEnableColouredAtts.Checked = true;
                checkBoxDisableUnprotectedContracts.Checked = true;
                checkBoxHideNonPublicBids.Checked = true;
                numericCurrencyInflation.Value = 2.5m;
                comboBoxGameSpeed.SelectedIndex = 4;
            }
            catch { }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "CM0102.exe Files|*.exe|All files (*.*)|*.*";
            ofd.Title = "Select a CM0102.exe file";
            try
            {
                var path = (string)Registry.GetValue(RegString.GetRegString(), "Location", "");
                if (!string.IsNullOrEmpty(path))
                    ofd.InitialDirectory = path;
            }
            catch { }
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                labelFilename.Text = ofd.FileName;
                TapaniDetection();
            }
        }

        private int yearExeSyncDecrement = 0;
        private void buttonApply_Click(object sender, EventArgs e)
        {
            try
            {
                // Let's go! - check the file exists and is writeable
                if (string.IsNullOrEmpty(labelFilename.Text))
                {
                    MessageBox.Show("Please select a cm0102.exe file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(labelFilename.Text))
                {
                    MessageBox.Show("Cannot find cm0102.exe file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    using (var file = File.Open(labelFilename.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to open and/or write to cm0102.exe file", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var fileLock = File.Open(labelFilename.Text, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    var dir = Path.GetDirectoryName(labelFilename.Text);
                    var dataDir = Path.Combine(dir, "Data");

                    // Start the patcher
                    Patcher patcher = new Patcher();
                    if (!patcher.CheckForV3968(labelFilename.Text))
                    {
                        var YesNo = MessageBox.Show("This does not look to be a 3.9.68 exe. Are you sure you wish to continue?", "3.9.68 Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (YesNo == DialogResult.No)
                            return;
                    }

                    // Initialise the name patcher
                    var namePatcher = new NamePatcher(labelFilename.Text, dataDir);

                    // Game speed hack
                    var speed = (short)(int)(comboBoxGameSpeed.SelectedItem as ComboboxItem).Value;
                    if (speed != 0)
                        patcher.SpeedHack(labelFilename.Text, speed);

                    // Currency Inflation
                    if (numericCurrencyInflation.Value != 0)
                        patcher.CurrencyInflationChanger(labelFilename.Text, (double)numericCurrencyInflation.Value);

                    // Year Change
                    if (checkBoxChangeStartYear.Checked)
                    {
                        // Assume Staff.data is in Data
                        var staffFile = Path.Combine(dataDir, "staff.dat");
                        var indexFile = Path.Combine(dataDir, "index.dat");
                        var playerConfigFile = Path.Combine(dataDir, "player_setup.cfg");
                        var staffCompHistoryFile = Path.Combine(dataDir, "staff_comp_history.dat");
                        var clubCompHistoryFile = Path.Combine(dataDir, "club_comp_history.dat");
                        var staffHistoryFile = Path.Combine(dataDir, "staff_history.dat");
                        var nationCompHistoryFile = Path.Combine(dataDir, "nation_comp_history.dat");

                        // Old Version
                        try
                        {
                            YearChanger yearChanger = new YearChanger();
                            var currentYear = yearChanger.GetCurrentExeYear(labelFilename.Text);
                            if (currentYear != (int)numericGameStartYear.Value)
                            {
                                if (!File.Exists(staffFile) || !File.Exists(indexFile))
                                {
                                    MessageBox.Show("staff.dat or index.dat not found in Data directory. Aborting year change.", "Files Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                var yesNo = MessageBox.Show("The Start Year Changer updates staff.dat and other files in the Data directory with the correct years as well as the cm0102.exe. Are you happy to proceed?", "CM0102Patcher - Year Changer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (yesNo == DialogResult.No)
                                    return;

                                int yearIncrement = (((int)numericGameStartYear.Value) - currentYear);

                                // e.g. When using 2018 data, to make it 2019 birth dates, etc
                                yearIncrement -= yearExeSyncDecrement;

                                yearChanger.ApplyYearChangeToExe(labelFilename.Text, (int)numericGameStartYear.Value);
                                yearChanger.UpdateStaff(indexFile, staffFile, yearIncrement);
                                yearChanger.UpdatePlayerConfig(playerConfigFile, yearIncrement);

                                yearChanger.UpdateHistoryFile(staffCompHistoryFile, 0x3a, yearIncrement, 0x8, 0x30);
                                yearChanger.UpdateHistoryFile(staffHistoryFile, 0x11, yearIncrement, 0x8);

                                yearChanger.UpdateHistoryFile(nationCompHistoryFile, 0x1a, yearIncrement + 1, 0x8);
                                yearChanger.UpdateHistoryFile(clubCompHistoryFile, 0x1a, yearIncrement, 0x8);
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionMsgBox.Show(ex);
                            return;
                        }
                        /*
                        // New EXE Version
                        YearChanger yearChanger = new YearChanger();
                        var currentYear = yearChanger.GetCurrentExeYear(labelFilename.Text);
                        if (currentYear != (int)numericGameStartYear.Value)
                        {
                            int yearIncrement = (((int)numericGameStartYear.Value) - currentYear);
                            yearChanger.ApplyYearChangeToExe(labelFilename.Text, (int)numericGameStartYear.Value);
                            patcher.ApplyPatch(labelFilename.Text, patcher.patches["datecalcpatch"]);
                            patcher.ApplyPatch(labelFilename.Text, patcher.patches["datecalcpatchjumps"]);
                            patcher.ApplyPatch(labelFilename.Text, patcher.patches["comphistory_datecalcpatch"]);
                        }*/
                    }

                    // Patches
                    if (checkBoxEnableColouredAtts.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["colouredattributes"]);
                    if (checkBoxIdleSensitivity.Checked)
                    {
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["idlesensitivity"]);
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["idlesensitivitytransferscreen"]);
                    }
                    if (checkBoxHideNonPublicBids.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["hideprivatebids"]);
                    if (checkBox7Subs.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["sevensubs"]);
                    if (checkBoxShowStarPlayers.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["showstarplayers"]);
                    if (checkBoxDisableUnprotectedContracts.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["disableunprotectedcontracts"]);
                    if (checkBoxCDRemoval.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["disablecdremove"]);
                    if (checkBoxDisableSplashScreen.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["disablesplashscreen"]);
                    if (checkBoxAllowCloseWindow.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["allowclosewindow"]);
                    if (checkBoxForceLoadAllPlayers.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["forceloadallplayers"]);
                    if (checkBoxRegenFixes.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["regenfixes"]);
                    if (checkBoxChangeResolution.Checked)
                    {
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["to1280x800"]);
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["tapanispacemaker"]);

                        int newWidth = ((Point)((comboBoxResolution.SelectedItem as ComboboxItem).Value)).X;
                        int newHeight = ((Point)((comboBoxResolution.SelectedItem as ComboboxItem).Value)).Y;
                        ResolutionChanger.SetResolution(labelFilename.Text, newWidth, newHeight);

                        // Convert the core gfx
                        int menuWidth = newWidth > 800 ? 126 : 90;
                        RGNConverter.RGN2RGN(Path.Combine(dataDir, "DEFAULT_PIC.RGN"), Path.Combine(dataDir, "bkg1280_800.rgn"), newWidth, newHeight);
                        RGNConverter.RGN2RGN(Path.Combine(dataDir, "match.mbr"), Path.Combine(dataDir, "m800.mbr"), menuWidth, newHeight); // 800 => 90 - 1280 => 126
                        RGNConverter.RGN2RGN(Path.Combine(dataDir, "game.mbr"), Path.Combine(dataDir, "g800.mbr"), menuWidth, newHeight);

                        var picturesDir = Path.Combine(dir, "Pictures");

                        if (Directory.Exists(picturesDir))
                        {
                            var yesNo = MessageBox.Show(string.Format("Do you wish to convert your CM0102 Pictures directory to {0}x{1} too?\r\n\r\nIf no, please turn off Background Changes in CM0102's Options else pictures will not appear correctly.\r\n\r\nIf yes, this takes a few moments.", newWidth, newHeight), "CM0102Patcher - Resolution Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (yesNo == DialogResult.Yes)
                            {
                                var pf = new PictureConvertProgressForm();

                                pf.OnLoadAction = () =>
                                {
                                    new Thread(() =>
                                    {
                                        var lastPic = "";
                                        try
                                        {
                                            int converting = 1;
                                            Thread.CurrentThread.IsBackground = true;

                                            var picFiles = Directory.GetFiles(picturesDir, "*.rgn");
                                            foreach (var picFile in picFiles)
                                            {
                                                lastPic = picFile;
                                                pf.SetProgressText(string.Format("Converting {0}/{1} ({2})", converting++, picFiles.Length, Path.GetFileName(picFile)));
                                                pf.SetProgressPercent((int)(((double)(converting - 1) / ((double)picFiles.Length)) * 100.0));
                                                int Width, Height;
                                                if (RGNConverter.GetImageSize(picFile, out Width, out Height))
                                                {
                                                    if (Width == 800 && Height == 600)
                                                        RGNConverter.RGN2RGN(picFile, picFile + ".tmp", newWidth, newHeight, 0, 35, 0, 100 - 35);
                                                    else
                                                        RGNConverter.RGN2RGN(picFile, picFile + ".tmp", newWidth, newHeight);
                                                    File.SetAttributes(picFile, FileAttributes.Normal);
                                                    File.Delete(picFile);
                                                    File.Move(picFile + ".tmp", picFile);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(string.Format("Failed when converting images!\r\nLast Pic: {0}", lastPic), "Image Convert", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            ExceptionMsgBox.Show(ex);
                                        }

                                        pf.CloseForm();
                                    }).Start();
                                };

                                pf.ShowDialog();
                            }
                        }
                    }
                    if (checkBoxJobsAbroadBoost.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["jobsabroadboost"]);
                    if (checkBoxNewRegenCode.Checked)
                    {
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["tapaninewregencode"]);
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["tapanispacemaker"]);
                    }
                    if (checkBoxUpdateNames.Checked)
                    {
                        namePatcher.RunPatch();
                    }
                    if (checkBoxManageAnyTeam.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["manageanyteam"]);
                    if (checkBoxRemove3NonEULimit.Checked)
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["remove3playerlimit"]);
                    if (checkBoxReplaceWelshPremier.Checked)
                    {
                        switch (comboBoxReplacementLeagues.SelectedIndex)
                        {
                            case 0:
                                namePatcher.PatchWelshWithNorthernLeague();
                                break;
                            case 1:
                                namePatcher.PatchWelshWithSouthernLeague();
                                break;
                            case 2:
                                namePatcher.PatchWelshWithSouthernPremierCentral();
                                break;
                        }
                    }
                    if (checkBoxRestrictTactics.Checked)
                    {
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["restricttactics"]);
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["changegeneraldat"]);
                    }
                    if (checkBoxMakeExecutablePortable.Checked)
                    {
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["changeregistrylocation"]);
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["memorycheckfix"]);
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["removemutexcheck"]);
                    }
                    if (checkBoxSwapSKoreaForChina.Checked)
                    {
                        patcher.ApplyPatch(labelFilename.Text, patcher.patches["chinapatch"]);
                        namePatcher.PatchStaffAward("South Korean Best 11 Of The Year", "Chinese Super League Best XI", true, true);
                        namePatcher.PatchStaffAward("South Korean Most Assisted Player Of The Year", "Chinese Super League Top Assistor", true, true);
                        namePatcher.PatchStaffAward("South Korean Top Goal Scorer Of The Year", "Chinese Super League Top Scorer", true, true);
                        namePatcher.PatchStaffAward("South Korean Young Player Of The Year", "Super League Young Player Of the Year", true, true);
                        namePatcher.PatchStaffAward("South Korean Manager Of The Year", "Super League Manager Of the Year", true, true);
                        namePatcher.PatchStaffAward("South Korean Player Of The Month", "Super League Player Of the Month", true, true);
                        namePatcher.PatchStaffAward("South Korean Player Of The Year", "Super League Player Of The Year", true, true);
                        namePatcher.PatchComp("Chinese First Division A", "Chinese Super League", "First Division A", "Super League", "CSL");
                    }

                    // NOCD Crack
                    if (checkBoxRemoveCDChecks.Checked)
                    {
                        var patched = NoCDPatch.PatchEXEFile(labelFilename.Text);
                    }

                    MessageBox.Show("Patched Successfully!", "CM0102 Patcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExceptionMsgBox.Show(ex);
            }
        }

        private void checkBoxChangeStartYear_CheckedChanged(object sender, EventArgs e)
        {
            labelGameStartYear.Enabled = numericGameStartYear.Enabled = checkBoxChangeStartYear.Checked;
        }

        private void buttonTools_Click(object sender, EventArgs e)
        {
            Tools tools = new Tools(labelFilename.Text);
            tools.ShowDialog();
        }

        private void ResetControls(Control controlContainer)
        {
            foreach (var control in controlContainer.Controls)
            {
                if (control is CheckBox)
                {
                    (control as CheckBox).Checked = false;
                }

                if (control is GroupBox || control is TabPage || control is TabControl)
                    ResetControls(control as Control);
            }
            numericCurrencyInflation.Value = 0;
            comboBoxGameSpeed.SelectedIndex = 0;
        }

        private void PatcherForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control &&
                (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (e.KeyChar == (char)19) // (S)ecret Mode
                {
                    checkBoxRemoveCDChecks.Checked = checkBoxRemoveCDChecks.Visible = true;
                    e.Handled = true;
                }
                if (e.KeyChar == (char)14) // N - Blank out all fields
                {
                    ResetControls(this);
                    e.Handled = true;
                }
                if (e.KeyChar == (char)1 && checkBoxRemoveCDChecks.Visible) // A
                {
                    string doubleWarning = labelFilename.Text.Contains("cm0001.exe") ? "" : "\r\n\r\nTHIS DOES NOT LOOK LIKE A CM0001.EXE!!!!!!!!!!\r\nDOUBLE CHECK BEFORE HITTING YES!!\r\n";
                    if (MessageBox.Show(string.Format("This will change exe:\r\n{0}\r\nTo Year: {1}\r\n\r\nARE YOU SURE YOU WANT TO DO THIS?!{2}", labelFilename.Text, (int)numericGameStartYear.Value, doubleWarning), "CM 00/01 Year Changer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        var yearChanger = new YearChanger();
                        var currentYear = yearChanger.GetCurrentExeYear(labelFilename.Text, 0x0001009F);
                        yearChanger.ApplyYearChangeTo0001Exe(labelFilename.Text, (int)numericGameStartYear.Value);

                        var dir = Path.GetDirectoryName(labelFilename.Text);
                        var dataDir = Path.Combine(dir, "Data");
                        var staffFile = Path.Combine(dataDir, "staff.dat");
                        var indexFile = Path.Combine(dataDir, "index.dat");
                        var playerConfigFile = Path.Combine(dataDir, "player_setup.cfg");
                        var staffCompHistoryFile = Path.Combine(dataDir, "staff_comp_history.dat");
                        var clubCompHistoryFile = Path.Combine(dataDir, "club_comp_history.dat");
                        var staffHistoryFile = Path.Combine(dataDir, "staff_history.dat");
                        var nationCompHistoryFile = Path.Combine(dataDir, "nation_comp_history.dat");

                        /*
                        // Update Data Too
                        int yearIncrement = (((int)numericGameStartYear.Value) - currentYear);
                        yearChanger.UpdateStaff(indexFile, staffFile, yearIncrement);
                        yearChanger.UpdatePlayerConfig(playerConfigFile, yearIncrement);

                        // Update History
                        yearChanger.UpdateHistoryFile(staffCompHistoryFile, 0x3a, yearIncrement, 0x8, 0x30);
                        yearChanger.UpdateHistoryFile(clubCompHistoryFile, 0x1a, yearIncrement, 0x8);
                        yearChanger.UpdateHistoryFile(staffHistoryFile, 0x11, yearIncrement, 0x8);
                        yearChanger.UpdateHistoryFile(nationCompHistoryFile, 0x1a, yearIncrement + 1, 0x8);
                        */

                        MessageBox.Show("CM0001 Year Changer Patch Applied!", "CM 00/01 Year Changer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    e.Handled = true;
                }
                if (e.KeyChar == (char)2 && checkBoxRemoveCDChecks.Visible) // B
                {
                    var yearChanger = new YearChanger();
                    var dir = Path.GetDirectoryName(labelFilename.Text);
                    var dataDir = Path.Combine(dir, "Data");
                    var nationCompHistoryFile = Path.Combine(dataDir, "nation_comp_history.dat");
                    var clubCompHistoryFile = Path.Combine(dataDir, "club_comp_history.dat");
                    //yearChanger.UpdateHistoryFile(nationCompHistoryFile, 0x1a, +2, 0x8);
                    yearChanger.UpdateHistoryFile(clubCompHistoryFile, 0x1a, 3, 0x8);
                }
                if (e.KeyChar == (char)3 && checkBoxRemoveCDChecks.Visible) // C
                {
                    NoCDPatch.PatchEXEFile0001FixV2(labelFilename.Text);
                }
                if (e.KeyChar == (char)4 && checkBoxRemoveCDChecks.Visible) // D
                {
                    var yearChanger = new YearChanger();
                    var dir = Path.GetDirectoryName(labelFilename.Text);
                    var dataDir = Path.Combine(dir, "Data");
                    var staffFile = Path.Combine(dataDir, "staff.dat");
                    var indexFile = Path.Combine(dataDir, "index.dat");
                    yearChanger.UpdateStaff(indexFile, staffFile, 17);
                    MessageBox.Show("staff.dat updated");
                }
                if (e.KeyChar == (char)5 && checkBoxRemoveCDChecks.Visible) // E
                {
                    var patcher = new Patcher();
                    patcher.CurrencyInflationChanger0001(labelFilename.Text, (double)numericCurrencyInflation.Value);
                    MessageBox.Show("CM0001 EXE Updated with new Inflation Multiplier!");
                }
                if (e.KeyChar == (char)6 && checkBoxRemoveCDChecks.Visible) // F
                {
                    var yearChanger = new YearChanger();
                    var patcher = new Patcher();
                    using (var pp = new ProcessPatch())
                    {
                        var currentYear = yearChanger.GetCurrentExeYear(labelFilename.Text);
                        if (pp.LoadProcess(labelFilename.Text))
                        {
                            var ms = pp.ReadIn();
                            patcher.ApplyPatch(ms, patcher.patches["changeregistrylocation"]);
                            patcher.ApplyPatch(ms, patcher.patches["memorycheckfix"]);
                            patcher.ApplyPatch(ms, patcher.patches["removemutexcheck"]);
                            patcher.ApplyPatch(ms, patcher.patches["colouredattributes"]);
                            patcher.ApplyPatch(ms, patcher.patches["allowclosewindow"]);

                            yearChanger.ApplyYearChangeToExe(ms, (int)numericGameStartYear.Value);
                            patcher.ApplyPatch(ms, patcher.patches["datecalcpatch"]);
                            patcher.ApplyPatch(ms, patcher.patches["datecalcpatchjumps"]);
                            patcher.ApplyPatch(ms, patcher.patches["comphistory_datecalcpatch"]);

                            NoCDPatch.PatchMemoryStream(ms);
                            pp.Write();
                            pp.Start();
                        }
                    }
                }
                if (e.KeyChar == (char)31 && checkBoxRemoveCDChecks.Visible) // -
                {
                    yearExeSyncDecrement = 1;
                    MessageBox.Show("Year Exe vs Data Sync Decrement Set to 1");
                }
            }
        }

        public class ComboboxItem
        {
            public ComboboxItem(string Text, object Value)
            {
                this.Text = Text;
                this.Value = Value;
            }
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("CM0102Patcher by Nick\r\n\r\nAll credit should go to the geniuses that found and shared their code and great patching work:\r\nTapani\r\nJohnLocke\r\nSaturn\r\nxeno\r\nMadScientist\r\nAnd so many others!\r\n\r\nThanks to everyone at www.champman0102.co.uk for keeping the game alive :)", "CM0102Patcher", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            RestorePoint.Save(labelFilename.Text);
        }

        private void buttonRestore_Click(object sender, EventArgs e)
        {
            RestorePoint.Restore(labelFilename.Text);
        }

        private void checkBoxAddNorthernLeague_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxReplacementLeagues.Enabled = checkBoxReplaceWelshPremier.Checked;
            if (checkBoxReplaceWelshPremier.Checked && comboBoxReplacementLeagues.SelectedIndex == -1)
                comboBoxReplacementLeagues.SelectedIndex = 0;
        }

        private void checkBoxChangeResolution_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxResolution.Enabled = checkBoxChangeResolution.Checked;
            if (checkBoxChangeResolution.Checked && comboBoxResolution.SelectedIndex == -1)
                comboBoxResolution.SelectedIndex = 5;
        }
    }
}
