using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management.Automation;
using System.Windows.Forms;
using Need_for_Speed_Underground_Launcher.Properties;

namespace Need_for_Speed_Underground_Launcher
{
    public partial class frmLauncher : Form
    {
        public frmLauncher()
        {
            InitializeComponent();
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            #region Play Sound Effect
            if(File.Exists(Settings.Default.PlayButton_Click_WAV))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Settings.Default.PlayButton_Click_WAV);
                player.Play();
            }
            #endregion Play Sound Effect

            lblPleaseWait.Visible = true;
            btnLaunch.Enabled = false;
            btnLaunch.BackColor = Color.LightGray;
            this.Refresh();
            LaunchNFSU();
        }

        private void LaunchNFSU()
        {
            try
            {
                string isoPath = Path.Combine(Settings.Default.NFSU_ISO_Path, Settings.Default.NFSU_ISO_Filename);
                FileInfo NFSU_Game_exe = new FileInfo(Path.Combine(Settings.Default.NFSU_GameEXE_Path, Settings.Default.NFSUL_GameEXE_Filename));

                #region Check if The ISO and Game Executable Exist
                if(!File.Exists(isoPath))
                    throw new FileNotFoundException($"Could not find the specified ISO file!  \r\nFile Specified: \"{isoPath}\"");
                if(!NFSU_Game_exe.Exists)
                    throw new FileNotFoundException($"Could not find the specified Game executable file!  \r\nFile Specified: \"{NFSU_Game_exe.FullName}\"");
                #endregion Check if the ISO and Game Executable Exist

                using(PowerShell ps = PowerShell.Create())
                using(Process NFSU_Game = new Process())
                {
                    #region Mount Need for Speed Underground Disk 2 ISO
                    if(!DetectISOMount())
                    {
                        ps.AddCommand("Mount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();
                        ps.Commands.Clear();
                    }
                    #endregion Mount Need for Speed Underground Disk 2 ISO

                    #region Make Sure Need for Speed Underground isn't Already Running
                    bool? continueAnyway = null;
                    if(Process.GetProcessesByName(NFSU_Game_exe.Name.Replace(NFSU_Game_exe.Extension, "")).Length > 0)
                        continueAnyway = MessageBox.Show("It appears that Need for Speed Underground is already running.  Do you want to launch the game again anyway?  This may cause unexpected behavior.  \r\n\r\nIf you select\"No\" the launcher will exit.", "Confirm Game Launch", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
                    
                    if(continueAnyway == false)
                    {
                        //Not dismounting the ISO, since Need for Speed was already running.
                        //ResetForm();
                        return;     //The finally { } block will still be executed
                    }
                    #endregion Make Sure Need for Speed Underground isn't Already Running

                    #region Start Need for Speed Underground Game
                    NFSU_Game.StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = Settings.Default.NFSU_GameEXE_Path,
                        FileName = Settings.Default.NFSUL_GameEXE_Filename
                    };
                    NFSU_Game.Start();          //Start the game
                    this.Hide();                //Hide the launcher so the user doesn't see it while the game is running
                    NFSU_Game.WaitForExit();    //Waits for the user to stop playing the game
                    //this.Show();              //Uncomment to unhide the launcher after the user exits the game
                    #endregion Start Need for Speed Underground Game

                    #region Dismount Need for Speed Underground Disk 2 ISO
                    if(!File.Exists(isoPath))
                        throw new FileNotFoundException($"The specified ISO file has gone missing!  \r\nFile Specified: \"{isoPath}\"");

                    if(DetectISOMount())
                        ps.AddCommand("Dismount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();

                    if(DetectISOMount())    //Whoops!  We didn't successfully dismount the ISO!
                        throw new TypeUnloadedException($"Could not dismount the ISO!  To manually dismount, open \"This PC\" right click the {Settings.Default.NFSU_ISO_VolumeLabel} Drive, and select \"Eject\"");
                    #endregion Dismount Need for Speed Underground Disk 2 ISO
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error was encountered!  Launcher will exit!  {Environment.NewLine}Exception Details (Try Googling this): {ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Close();   //Exit the launcher
            }
        }

        private bool DetectISOMount()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach(DriveInfo drive in drives)
                if(drive.IsReady && drive.VolumeLabel.Equals(Settings.Default.NFSU_ISO_VolumeLabel, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        private void frmLauncher_Load(object sender, EventArgs e)
        {
            //Check to make sure there's not another instance of this launcher running
            if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
            {
                MessageBox.Show("Another instance of the Need for Speed Underground Launcher is already running!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();   //Exit the launcher
            }    

            lblPleaseWait.Visible = false;

            string currentDir = Directory.GetCurrentDirectory();
            string startupOptionsFile = Path.Combine(currentDir, "StartupOptions.txt");

            if(File.Exists(startupOptionsFile))
            {
                using(StreamReader sr = new StreamReader(startupOptionsFile))
                {
                    string line = "";
                    while((line = sr.ReadLine()) != null)
                    {
                        if(string.IsNullOrWhiteSpace(line))
                            continue;

                        if(line.Contains("Skip Launcher", true) && line.Contains("True", true))
                        {
                            btnLaunch_Click(null, null);
                            break;
                        }
                    }
                }
            }
        }

        private void frmLauncher_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar.Equals(Keys.Enter))
                btnLaunch_Click(null, null);
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            #region Play Sound Effect
            if(File.Exists(Settings.Default.PlayButton_Click_WAV))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Settings.Default.PlayButton_Click_WAV);
                player.Play();
            }
            #endregion Play Sound Effect

            MessageBox.Show(
                "Created by Patrick Brophy (2021)  \r\n\r\n\r\n\r\n" +
                "Frequently Asked Questions:\r\n\r\n\r\n" +
                "Q: Where should I put these launcher files?  \r\n\r\n" +
                "A: The recommended location is to create a new \"Launcher\" folder inside Need for Speed Underground's installed location.  Then, put a shortcut to \"Need for Speed Underground Launcher.exe\" on your desktop.  \r\n\r\n\r\n" +

                "Q: The launcher keeps saying it can't find the ISO or EXE file.  How can I fix that?  \r\n\r\n" +
                $"A: Open \"{Path.Combine(Directory.GetCurrentDirectory(), "Need for Speed Underground Launcher.exe.config")}\" and update the paths to the installed locations of your ISO & game executable files, respectively.  \r\n\r\n\r\n" +

                "Q: How can I skip the launcher, and directly start the game?  \r\n\r\n" +
                $"A: Open \"{Directory.GetCurrentDirectory()}\" and create a new Text Document named \"StartupOptions.txt\".  Put a line in that file that says \"Skip Launcher:True\" and save the file.  " +
                "Next time you open the Need for Speed Underground lancher, the game will start immediately, skipping the splash screen.",

                "About Need for Speed Underground Launcher", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        /*
        private void ResetForm()
        {
            lblPleaseWait.Visible = false;
            btnLaunch.Enabled = true;
            btnLaunch.BackColor = Color.Black;
            this.Refresh();
        }
        */
    }

    public static class ExtensionMethods
    {
        public static bool Contains(this string text, string value, bool ignoreCase)
        {
            if(ignoreCase)
                return text.ToUpper().Contains(value.ToUpper());
            return text.Contains(value);
        }

        public static bool StartsWith(this string text, string value, bool ignoreCase)
        {
            if(ignoreCase)
                return text.ToUpper().StartsWith(value.ToUpper());
            return text.StartsWith(value);
        }

        /*
        public static string Replace(this string text, string oldString, string newString, bool ignoreCase)
        {
            if(ignoreCase)
            {
                //TODO: Write the logic to do a case-insensitive replace
            }
            return text.Replace(oldString, newString);
        }
        */
    }
}