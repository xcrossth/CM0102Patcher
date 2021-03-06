﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CM0102Patcher
{
    public class NamePatcher
    {
        string exeFile;
        string dataDir;
        Patcher patcher;
        int freePos = 0x5861d0;

        public NamePatcher(string exeFile, string dataDir)
        {
            this.exeFile = exeFile;
            this.dataDir = dataDir;
            this.patcher = new Patcher();
        }

        public void RunPatch()
        {
            // Change game name :)
            YearChanger yearChanger = new YearChanger();
            var currentYear = yearChanger.GetCurrentExeYear(exeFile);
            var newGameName = currentYear.ToString() + "/" + (currentYear+1).ToString().Substring(2);
            ByteWriter.WriteToFile(exeFile, 0x68029d, newGameName);

            // Add Transfer Window Patch (from Saturn's v3 Patches)
            patcher.ApplyPatch(exeFile, patcher.patches["transferwindowpatch"]);

            // Patch Holland
            PatchHolland();

            // Patch Comps
            PatchComp("European Champions Cup", "UEFA Champions League", "Champions Cup", "Champions League");
            PatchComp("UEFA Cup", "UEFA Europa League");

            // English
            PatchComp("English Premier Division", "English Premier League", "Premier Division", "Premier League", "EPL");
            PatchComp("English First Division", "English Football League Championship", "First Division", "Championship", "FLC");
            PatchComp("English Second Division", "English Football League One", "Second Division", "League One", "FL1");
            PatchComp("English Third Division", "English Football League Two", "Third Division", "League Two", "FL2");
        }

        public void PatchWelshWithNorthernLeague()
        {
            PatchComp("English Northern Premier League Premier Division", "English National League North", "Northern Premier", "National League North", "NLN");
            patcher.ApplyPatch(exeFile, patcher.patches["englishleaguenorthpatch"]);
            ByteWriter.WriteToFile(exeFile, 0x6d56b8, "English National League North" + "\0");

            PatchStaffAward("Welsh Team of the Week",           "English National League North Team of the Week");
            PatchStaffAward("Welsh Player of the Year",         "English National League North Player of the Year");
            PatchStaffAward("Welsh Young Player of the Year",   "English National League North Youth of the Year");
            PatchStaffAward("Welsh Top Goalscorer",             "English National League North Top Goalscorer");
            PatchStaffAward("Welsh Manager of the Year",        "English National League North Manager of the Year");
            PatchStaffAward("Welsh Manager of the Month",       "English National League North Manager of the Month");
            patcher.ApplyPatch(exeFile, patcher.patches["englishleaguenorthawards"]);
        }

        public void PatchWelshWithSouthernLeague()
        {
            // Apply the standard north patch first
            patcher.ApplyPatch(exeFile, patcher.patches["englishleaguenorthpatch"]);

            var cm = new ClubMover();
            cm.LoadClubAndComp(Path.Combine(dataDir, "club_comp.dat"), Path.Combine(dataDir, "club.dat"));
            var southernTeams = cm.CountSouthernTeams();

            // Patch the number of teams
            ByteWriter.WriteToFile(exeFile, 0x525B3C, BitConverter.GetBytes(southernTeams * 59));
            ByteWriter.WriteToFile(exeFile, 0x525B46, new byte[] { ((byte)southernTeams) });

            PatchComp("English Southern League Premier Division", "English National League South", "Southern Premier", "National League South", "NLS");
            ByteWriter.WriteToFile(exeFile, 0x6d56b8, "English National League South" + "\0");

            PatchStaffAward("Welsh Team of the Week", "English National League South Team of the Week");
            PatchStaffAward("Welsh Player of the Year", "English National League South Player of the Year");
            PatchStaffAward("Welsh Young Player of the Year", "English National League South Youth of the Year");
            PatchStaffAward("Welsh Top Goalscorer", "English National League South Top Goalscorer");
            PatchStaffAward("Welsh Manager of the Year", "National League South Manager of the Year");
            PatchStaffAward("Welsh Manager of the Month", "National League South Manager of the Month");
            patcher.ApplyPatch(exeFile, patcher.patches["englishleaguesouthawards"]);

            patcher.ApplyPatch(exeFile, 0x1751ff, "9c");
        }

        public void PatchWelshWithSouthernPremierCentral()
        {
            // Apply the standard north patch first
            patcher.ApplyPatch(exeFile, patcher.patches["englishleaguenorthpatch"]);
            
            var cm = new ClubMover();
            cm.LoadClubAndComp(Path.Combine(dataDir, "club_comp.dat"), Path.Combine(dataDir, "club.dat"));
            var southernTeams = cm.SetupEnglishSouthernLeague();
            
            // Patch the number of teams
            ByteWriter.WriteToFile(exeFile, 0x525B3C, BitConverter.GetBytes(southernTeams*59));
            ByteWriter.WriteToFile(exeFile, 0x525B46, new byte[] { ((byte)southernTeams) });

            //ByteWriter.WriteToFile(exeFile, 0x6d56b8, "English Southern League Premier Division" + "\0");
                        
            PatchComp("English Southern League Premier Division", "English Southern Premier Central", "Southern Premier", "Southern Premier", "SPC");
            ByteWriter.WriteToFile(exeFile, 0x6d56b8, "English Southern Premier Central" + "\0");
            
            PatchStaffAward("Welsh Team of the Week",           "English Southern Premier Team of the Week");
            PatchStaffAward("Welsh Player of the Year",         "English Southern Premier Player of the Year");
            PatchStaffAward("Welsh Young Player of the Year",   "English Southern Premier Youth of the Year");
            PatchStaffAward("Welsh Top Goalscorer",             "English Southern Central Premier Top Goalscorer");
            PatchStaffAward("Welsh Manager of the Year",        "English Southern Premier Manager of the Year");
            PatchStaffAward("Welsh Manager of the Month",       "English Southern Premier Manager of the Month");
            patcher.ApplyPatch(exeFile, patcher.patches["englishleaguesouthawards"]);

            // Let's allow more loans seeing as we don't get many players
            patcher.ApplyPatch(exeFile, 0x179e5B, "07");
            patcher.ApplyPatch(exeFile, 0x179f17, "06");

            /*
            005751F8  |> \A1 FCADAD00   MOV EAX,DWORD PTR DS:[0ADADFC]
            005751FD  |.  8BB8 A0050000 MOV EDI,DWORD PTR DS:[EAX+5A0]
            00575203  |.  8BB0 74010000 MOV ESI,DWORD PTR DS:[EAX+174]

            0x5A0 = 0x168 * 4 = Team 0x168 which is the Northern Premier League
            0x167 is the southern, so we need 0x167 * 4 = 0x59c
            */
            patcher.ApplyPatch(exeFile, 0x1751ff, "9c");
        }

        // https://champman0102.co.uk/showthread.php?t=8267&highlight=Netherlands
        // Going to stick things at 009861d0 (005861d0 = in binary)
        // 0060E100  |> 68 34159B00 PUSH OFFSET 009B1534                     ; /Arg2 = ASCII "Holland"
        // 0060E100     68 D0619800 PUSH OFFSET 009861D0
        void PatchHolland()
        {
            freePos += ByteWriter.WriteToFile(exeFile, freePos, "Netherlands\0");
            patcher.ApplyPatch(exeFile, 0x20e100, "68D0619800");
            // nation.dat
            var nationDatFilename = Path.Combine(dataDir, "nation.dat");
            var nationDat = ByteWriter.LoadFile(nationDatFilename);
            var pos = ByteWriter.SearchBytes(nationDat, "Holland");
            if (pos != -1)
            {
                ByteWriter.WriteToFile(nationDatFilename, pos, "Netherlands", 52);
                ByteWriter.WriteToFile(nationDatFilename, pos + 52, "Netherlands", 27);
                ByteWriter.WriteToFile(nationDatFilename, pos + 52 + 27, "NED");
            }
            // nat_club.dat
            ByteWriter.BinFileReplace(Path.Combine(dataDir, "nat_club.dat"), "Holland", "Netherlands");
            // euro.cfg
            ByteWriter.TextFileReplace(Path.Combine(dataDir, "euro.cfg"), "Holland", "Netherlands");
        }

        public static void PatchCompAcronym(string fileName, int startPos, string acronym)
        {
            ByteWriter.WriteToFile(fileName, startPos + 79, acronym, 3);
        }

        public void PatchComp(string oldName, string newName, string oldShortName, string newShortName, string newAcronym = null)
        {
            int compChangePos = PatchComp(oldName, newName);
            if (compChangePos != -1)
            {
                PatchComp(oldShortName, newShortName, compChangePos, -1);
                if (newAcronym != null)
                    PatchCompAcronym(Path.Combine(dataDir, "club_comp.dat"), compChangePos, newAcronym);
            }
        }

        public void PatchStaffAward(string oldName, string newName, bool patchExe = false, bool ignoreCase = false)
        {
            var staff_comp = Path.Combine(dataDir, "staff_comp.dat");
            ByteWriter.BinFileReplace(staff_comp, oldName, newName + "\0", 0, 0, ignoreCase);
            if (patchExe)
                ByteWriter.BinFileReplace(exeFile, oldName, newName + "\0", 0, 0, ignoreCase);
        }

        int PatchComp(string fromComp, string toComp, int clubCompStartPos = 0, int exeStartPos = 0x5d9590)
        {
            var club_comp = Path.Combine(dataDir, "club_comp.dat");
            var exeBytes = ByteWriter.LoadFile(exeFile);
            int compChangePos = ByteWriter.BinFileReplace(club_comp, fromComp, toComp, clubCompStartPos, clubCompStartPos != 0 ? 1 : 0);

            if (exeStartPos != -1)
            {
                // Find where the string is held
                var pos = ByteWriter.SearchBytes(exeBytes, fromComp, exeStartPos);
                // Convert the position of the current string, to a PUSH statement in the exe
                var searchBytes = new byte[5] { 0x68, 0x00, 0x00, 0x00, 0x00 };
                BitConverter.GetBytes(pos + 0x400000).ToArray().CopyTo(searchBytes, 1);
                // Find the PUSH Statement in the EXE to this string
                var posExePush = ByteWriter.SearchBytes(exeBytes, searchBytes);
                if (posExePush != -1)
                {
                    // Get the next free position of text and convert to a PUSH
                    BitConverter.GetBytes(freePos + 0x400000).ToArray().CopyTo(searchBytes, 1);
                    // Write the new PUSH statement to the free pos
                    ByteWriter.WriteToFile(exeFile, posExePush, searchBytes);
                    // Write the new string to the free pos and increment the free pos
                    ByteWriter.WriteToFile(exeFile, freePos, toComp + "\0");
                }
                // Just because it wasn't found this time doesn't mean it wasn't already written to, so push the ptr forward anyway
                freePos += toComp.Length + 1;
            }

            return compChangePos;
        }
    }
}
