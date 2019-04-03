﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CM0102Patcher
{
    public class CM0102ClubMover
    {
        class Comp
        {
            public int Offset;
            public UInt32 ClubCompID;
            public string ClubCompName;
            public byte ClubCompGenderName;
            public string ClubCompNameShort;
            public byte ClubCompGenderNameShort;
            public string ClubCompNameThreeLetter;
        }

        class Club
        {
            public int Offset;
            public UInt32 ClubID;
            public string ClubName;
            public byte ClubGenderName;
            public string ClubNameShort;
            public byte ClubGenderNameShort;
            public UInt32 ClubNation;
            public UInt32 ClubDivision;
            public UInt32 ClubLastDivision;
        }

        string clubCompDatFile;
        string clubDatFile;
        List<Comp> compList = new List<Comp>();
        static List<Club> clubList = new List<Club>();

        public void LoadClubAndComp(string clubCompDatFile, string clubDatFile)
        {
            this.clubCompDatFile = clubCompDatFile;
            this.clubDatFile = clubDatFile;

            Encoding latin1 = Encoding.GetEncoding("ISO-8859-1");
            using (var file = File.Open(clubCompDatFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var fileLength = (int)file.Length;
                var bytes = new byte[fileLength];
                file.Read(bytes, 0, fileLength);

                for (int i = 0; i < fileLength; i += 107)
                {
                    if (i + 107 > fileLength)
                        break;
                    Comp comp = new Comp();
                    comp.Offset = i;
                    comp.ClubCompID = BitConverter.ToUInt32(bytes, i);
                    comp.ClubCompName = latin1.GetString(bytes, i + 4, 51).Replace("\0", "");
                    comp.ClubCompGenderName = bytes[i + 4 + 51];
                    comp.ClubCompNameShort = latin1.GetString(bytes, i + 4 + 51 + 1, 26).Replace("\0", "");
                    comp.ClubCompGenderNameShort = bytes[i + 4 + 51 + 1 + 26];
                    comp.ClubCompNameThreeLetter = latin1.GetString(bytes, i + 4 + 51 + 1 + 26 + 1, 3).Replace("\0", "");
                    compList.Add(comp);
                }
            }

            using (var file = File.Open(clubDatFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var fileLength = (int)file.Length;
                var bytes = new byte[fileLength];
                file.Read(bytes, 0, fileLength);

                for (int i = 0; i < fileLength; i += 581)
                {
                    if (i + 578 > fileLength)
                        break;
                    Club club = new Club();
                    club.Offset = i;
                    club.ClubID = BitConverter.ToUInt32(bytes, i);
                    club.ClubName = latin1.GetString(bytes, i + 4, 51).Replace("\0", "");
                    club.ClubGenderName = bytes[i + 4 + 51];
                    club.ClubNameShort = latin1.GetString(bytes, i + 4 + 51 + 1, 26).Replace("\0", "");
                    club.ClubGenderNameShort = bytes[i + 4 + 51 + 1 + 26];
                    club.ClubNation = BitConverter.ToUInt32(bytes, i + 4 + 51 + 1 + 26 + 1);
                    club.ClubDivision = BitConverter.ToUInt32(bytes, i + 4 + 51 + 1 + 26 + 1 + 4);
                    club.ClubLastDivision = BitConverter.ToUInt32(bytes, i + 4 + 51 + 1 + 26 + 1 + 4 + 4);
                    clubList.Add(club);
                }
            }
        }

        public int SetupEnglishSouthernLeague()
        {
            // Kick all clubs out of the southern league into a lower division
            var southernLeague = GetDivision("English Southern League Premier Division");
            var aLowerDivision = GetDivision("A Lower Division");
            var allSouthernClubs = clubList.FindAll(x => x.ClubDivision == southernLeague.ClubCompID);
            foreach (var southernClub in allSouthernClubs)
            {
                southernClub.ClubDivision = aLowerDivision.ClubCompID;
                SaveClubsDivision(clubDatFile, southernClub);
            }

            string[] southernTeams = new string[]
            {
                "Kettering Town",
                "Stourbridge",
                "King's Lynn Town",
                "Stratford Town",
                "Alvechurch FC",
                "AFC Rushden & Diamonds",
                "Biggleswade Town",
                "Coalville Town",
                "Royston Town",
                "Rushall Olympic",
                "Needham Market",
                "Tamworth",
                "St. Ives Town",
                "LeistonFC",
                "Hitchin Town",
                "Barwell",
                "Banbury United",
                "Redditch United",
                "Lowestoft Town",
                "St. Neots Town",
                "Halesowen Town",
                "Bedworth United"
            };

            foreach (var southernTeam in southernTeams)
                MoveClubToDivision(clubDatFile, southernTeam, "English Southern League Premier Division");

            return clubList.Count(x => x.ClubDivision == southernLeague.ClubCompID);
        }

        Club GetClub(string clubName)
        {
            return clubList.FirstOrDefault(x => x.ClubName == clubName);
        }

        Comp GetDivision(string compName)
        {
            return compList.FirstOrDefault(x => x.ClubCompName == compName);
        }

        Comp GetClubsDivision(Club club)
        {
            return compList.FirstOrDefault(x => x.ClubCompID == club.ClubDivision);
        }

        bool MoveClubToDivision(string clubDatFile, string clubName, string divisionName)
        {
            bool ret = false;
            var division = compList.FirstOrDefault(x => x.ClubCompName == divisionName);
            var club = clubList.FirstOrDefault(x => x.ClubName == clubName);
            if (division != null & club != null)
            {
                club.ClubDivision = division.ClubCompID;
                SaveClubsDivision(clubDatFile, club);
                ret = true;
            }
            return ret;
        }

        void SaveClubsDivision(string clubDat, Club club)
        {
            using (var file = File.Open(clubDat, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                file.Seek(club.Offset + 4 + 51 + 1 + 26 + 1 + 4, SeekOrigin.Begin);
                using (var bw = new BinaryWriter(file))
                {
                    bw.Write(club.ClubDivision);
                }
            }
        }
    }
}
