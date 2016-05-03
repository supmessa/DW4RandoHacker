﻿using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;

namespace DW4RandoHacker
{
    public partial class Form1 : Form
    {
        byte[] romData;
        byte[] romData2;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = openFileDialog1.FileName;
                runChecksum();
            }
        }

        private void runChecksum()
        {
            try
            {
                using (var md5 = SHA1.Create())
                {
                    using (var stream = File.OpenRead(txtFileName.Text))
                    {
                        lblSHAChecksum.Text = BitConverter.ToString(md5.ComputeHash(stream)).ToLower().Replace("-", "");
                    }
                }
            }
            catch
            {
                lblSHAChecksum.Text = "????????????????????????????????????????";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboXPAdjustment.SelectedIndex = 1;
            cboGoldAdjustment.SelectedIndex = 1;

            txtSeed.Text = (DateTime.Now.Ticks % 2147483647).ToString();

            try
            {
                using (TextReader reader = File.OpenText("lastFile4.txt"))
                {
                    txtFileName.Text = reader.ReadLine();
                    txtCompare.Text = reader.ReadLine();
                    txtC1Name1.Text = reader.ReadLine();
                    txtC2Name1.Text = reader.ReadLine();
                    txtC2Name2.Text = reader.ReadLine();
                    txtC2Name3.Text = reader.ReadLine();
                    txtC3Name1.Text = reader.ReadLine();
                    txtC4Name1.Text = reader.ReadLine();
                    txtC4Name2.Text = reader.ReadLine();

                    chkSoloHero.Checked = (reader.ReadLine() == "T");
                    cboSoloHero.SelectedItem = reader.ReadLine();
                    chkSoloCanEquipAll.Checked = (reader.ReadLine() == "T");
                    chkC14Random.Checked = (reader.ReadLine() == "T");
                    chkC5Random.Checked = (reader.ReadLine() == "T");

                    c1Hero.SelectedItem = reader.ReadLine();
                    c2Hero1.SelectedItem = reader.ReadLine();
                    c2Hero2.SelectedItem = reader.ReadLine();
                    c2Hero3.SelectedItem = reader.ReadLine();
                    chkCh2AwardXPTournament.Checked = (reader.ReadLine() == "T");
                    c3Hero.SelectedItem = reader.ReadLine();
                    chkShop1.Checked = (reader.ReadLine() == "T");
                    chkShop25K.Checked = (reader.ReadLine() == "T");
                    chkTunnel1.Checked = (reader.ReadLine() == "T");
                    c4Hero1.SelectedItem = reader.ReadLine();
                    c4Hero2.SelectedItem = reader.ReadLine();
                    c5Hero1.SelectedItem = reader.ReadLine();
                    c5Hero2.SelectedItem = reader.ReadLine();
                    c5Hero3.SelectedItem = reader.ReadLine();
                    c5Hero4.SelectedItem = reader.ReadLine();
                    c5Hero5.SelectedItem = reader.ReadLine();
                    c5Hero6.SelectedItem = reader.ReadLine();
                    c5Hero7.SelectedItem = reader.ReadLine();
                    c5Hero8.SelectedItem = reader.ReadLine();

                    cboXPAdjustment.SelectedItem = reader.ReadLine();
                    chkXPRandom.Checked = (reader.ReadLine() == "T");
                    cboGoldAdjustment.SelectedItem = reader.ReadLine();
                    chkGoldRandom.Checked = (reader.ReadLine() == "T");
                    cboEncounterRate.SelectedItem = reader.ReadLine();
                    chkRandomMonsters.Checked = (reader.ReadLine() == "T");
                    txtSeed.Text = reader.ReadLine();
                    chkSpeedUpBattles.Checked = (reader.ReadLine() == "T");

                    runChecksum();
                }
            }
            catch
            {
                // ignore error
                txtC1Name1.Text = "Ragnar";
                txtC2Name1.Text = "Alena";
                txtC2Name2.Text = "Cristo";
                txtC2Name3.Text = "Brey";
                txtC3Name1.Text = "Taloon";
                txtC4Name1.Text = "Mara";
                txtC4Name2.Text = "Nara";
            }
        }

        private void btnNewSeed_Click(object sender, EventArgs e)
        {
            txtSeed.Text = (DateTime.Now.Ticks % 2147483647).ToString();
        }

        private void btnRandomize_Click(object sender, EventArgs e)
        {
            if (lblSHAChecksum.Text != lblReqChecksum.Text)
            {
                if (MessageBox.Show("The checksum of the ROM does not match the required checksum.  Patch anyway?", "Checksum mismatch", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            if (!loadRom())
                return;

            hackRom();

            //// All ROM hacks will revive ALL characters on a ColdAsACod.
            //// There will be a temporary graphical error if you use less than four characters, but I'm going to leave it be.
            //byte[] codData1 = { 0xa0, 0x00, // Make sure Y is 0 first.
            //    0xb9, 0x3c, 0x07,
            //    0xc9, 0x80,
            //    0x90, 0x03, // If less than 0x80, skip.
            //    0x20, 0xb2, 0xbf, // JSR to a bunch of unused code, which will have the "revive one character code" that I'm replacing.
            //    0xc8, 0xc8, // Increment Y twice (Y is used to revive the characters)
            //    0xc0, 0x08, // Compare Y with 08
            //    0xd0, 0xf0, // If not equal, go back to the JSR mentioned above
            //    0xa0, 0x00, // Set Y back to 0 to make sure the game doesn't think something is up
            //    0xea, 0xea, 0xea, 0xea, 0xea,
            //    0xea, 0xea, 0xea, 0xea, 0xea,
            //    0xea, 0xea }; // 12 NOPs, since I have nothing else to do.
            //byte[] codData2 = { 0xa9, 0x80, // Load 80, the status for alive
            //    0x99, 0x3c, 0x07, // store to two status bytes
            //    0x99, 0x3d, 0x07,
            //    0xb9, 0x24, 0x07, // Load max HP
            //    0x99, 0x1c, 0x07, // save max HP
            //    0xb9, 0x25, 0x07, // second byte
            //    0x99, 0x1d, 0x07,
            //    0xb9, 0x34, 0x07, // Load max MP
            //    0x99, 0x2c, 0x07, // save max MP
            //    0xb9, 0x35, 0x07, // second byte
            //    0x99, 0x2d, 0x07,
            //    0x60 }; // end JSR

            //for (int lnI = 0; lnI < codData1.Length; lnI++)
            //    romData[0x22b3 + lnI] = codData1[lnI];
            //for (int lnI = 0; lnI < codData2.Length; lnI++)
            //    romData[0x3fc2 + lnI] = codData2[lnI];

            //// Rename the starting characters.
            //for (int lnI = 0; lnI < 12; lnI++)
            //{
            //    string name = (lnI == 0 ? txtDefault1.Text :
            //        lnI == 1 ? txtDefault2.Text :
            //        lnI == 2 ? txtDefault3.Text :
            //        lnI == 3 ? txtDefault4.Text :
            //        lnI == 4 ? txtDefault5.Text :
            //        lnI == 5 ? txtDefault6.Text :
            //        lnI == 6 ? txtDefault7.Text :
            //        lnI == 7 ? txtDefault8.Text :
            //        lnI == 8 ? txtDefault9.Text :
            //        lnI == 9 ? txtDefault10.Text :
            //        lnI == 10 ? txtDefault11.Text :
            //        txtDefault12.Text);
            //    for (int lnJ = 0; lnJ < 8; lnJ++)
            //    {
            //        romData[0x1ed52 + (8 * lnI) + lnJ] = 0;
            //        try
            //        {
            //            char character = Convert.ToChar(name.Substring(lnJ, 1));
            //            if (character >= 0x30 && character <= 0x39)
            //                romData[0x1ed52 + (8 * lnI) + lnJ] = (byte)(character - 47);
            //            if (character >= 0x41 && character <= 0x5a)
            //                romData[0x1ed52 + (8 * lnI) + lnJ] = (byte)(character - 28);
            //            if (character >= 0x61 && character <= 0x7a)
            //                romData[0x1ed52 + (8 * lnI) + lnJ] = (byte)(character - 86);
            //        }
            //        catch
            //        {
            //            romData[0x1ed52 + (8 * lnI) + lnJ] = 0; // no more characters to process - make the rest of the characters blank
            //        }
            //    }
            //}

            saveRom();
        }

        private void hackRom()
        {
            for (int lnI = 0; lnI < 8; lnI++)
                romData[0x49145 + lnI] = 0; // Make sure all characters are loaded right away; otherwise, Chapter 1 will most likely start with a ghost.

            if (chkSoloHero.Checked)
            {
                byte power = 0;
                if ((string)cboSoloHero.SelectedItem == "Hero") power = 0;
                if ((string)cboSoloHero.SelectedItem == "Cristo") power = 1;
                if ((string)cboSoloHero.SelectedItem == "Nara") power = 2;
                if ((string)cboSoloHero.SelectedItem == "Mara") power = 3;
                if ((string)cboSoloHero.SelectedItem == "Brey") power = 4;
                if ((string)cboSoloHero.SelectedItem == "Taloon") power = 5;
                if ((string)cboSoloHero.SelectedItem == "Ragnar") power = 6;
                if ((string)cboSoloHero.SelectedItem == "Alena") power = 7;
                if (chkSoloCanEquipAll.Checked)
                {
                    for (int lnI = 0; lnI < 80; lnI++)
                        romData[0x40c75 + lnI] = (byte)Math.Pow(2, power); // Going to make sure the character gets to equip everything!
                }
                else
                { // still have to allow equipping of the Zenethian equipment so you can get through the tower and castle...
                    romData[0x40c75 + 0x14] = (byte)Math.Pow(2, power);
                    romData[0x40c75 + 0x21] = (byte)Math.Pow(2, power);
                    romData[0x40c75 + 0x37] = (byte)Math.Pow(2, power);
                    romData[0x40c75 + 0x44] = (byte)Math.Pow(2, power);
                    romData[0x40c75 + 0x4b] = (byte)Math.Pow(2, power);
                }
                for (int lnI = 0; lnI < 5; lnI++)
                    romData[0x4914d + lnI] = (byte)(128 + power); // This will ensure the same character starts each chapter.

                // This makes sure all item choices point to the right person's inventory. (you get tripped up in that in Chapter 3's Armor charity in Bonmalmo at a minimum)
                romData[0x41f93] = (byte)(1 + (30 * power));
                romData[0x41f95] = (byte)(1 + (30 * power));
                romData[0x41f97] = (byte)(1 + (30 * power));
                romData[0x41f99] = (byte)(1 + (30 * power));
                romData[0x41f9b] = (byte)(1 + (30 * power));
                romData[0x41f9d] = (byte)(1 + (30 * power));
                romData[0x41f9f] = (byte)(1 + (30 * power));
                romData[0x41fa1] = (byte)(1 + (30 * power));

                // Double the HP gain
                romData[0x49e22] = 0x20; // JSR to an unused portion of the rom
                romData[0x49e23] = 0x68;
                romData[0x49e24] = 0xbf;
                romData[0x4bf78] = 0x0a; // Arithmetic shift left... multipling the vitality gain by 4 instead of 2
                romData[0x4bf79] = 0x8d; // Store accumulator absolute -> 6e09
                romData[0x4bf7a] = 0x09;
                romData[0x4bf7b] = 0x6e;
                romData[0x4bf7c] = 0x60; // end subroutine

                // Remove baseline - otherwise, you can't double HP(until I figure out how to double the HP baseline...)
                romData[0x49df6] = 0xb0;
                romData[0x49df7] = 0x06;

                // Need to dodge a check to make sure you can have multiple of the same person in the party.
                romData[0x41338] = 0xea;
                romData[0x41339] = 0xea;

                romData[0x4af70] = 0xbf; // Make the medical herb super powerful out of battle...
                romData[0x4f7f9] = 0xff; // ... and even more powerful in battle! (next 2 lines)
                romData[0x4f7fa] = 0xff;

                // Ensure that ? allies become the solo hero instead.
                for (int lnI = 0; lnI < 6; lnI++)
                    romData[0x413ea + lnI] = 0xea;
                romData[0x413f1] = (byte)(128 + power);
                romData[0x413fc] = 0x4c;
                romData[0x413fd] = 0x9c;
                romData[0x413fe] = 0x93;

                // Make the Cristo and Brey join change to the solo hero joining twice instead in Chapter 2.
                romData[0x79d44] = power;
                romData[0x79d49] = power;

                // Force the solo hero to fight in Chapter 2's tournament.  This isn't completely neccessary, but it will smooth out processing.
                romData[0x79074] = power;

                // Turn Nara into the solo hero in Chapter 4!
                romData[0x76b3c] = power;

                // You have to gain the magic key because having Orin in the party destroys the solo hero concept.  
                // We'll replace the first treasure chest in the Aktemto Mine with the Magic Key.
                romData[0x7bef1] = 0x72;

                // Give full control over all players in Chapter 5.  You lose the wagon control though.  I would LOVE to figure out how to get both though!  Maybe some nops?
                romData[0x46e1e] = 0x7f; // You can make it any number higher than 04, chapter 5 I think... 

                // Force Nara to solo hero in Chapter 5
                romData[0x77903] = power;
                // Force Mara to solo hero in Chapter 5
                romData[0x54ad7] = power; // Otherwise, Mara will think that Nara isn't a part of the party...
                romData[0x77909] = power;

                // Dodge an issue with Chapter 5's Cave Of Betrayal
                romData[0x41423] = 0xa0;
                romData[0x41424] = 0x00; // LDY $#00 - This will force whoever is in the lead to survive "the drop"
                romData[0x41425] = 0xea;

                // Force Taloon to solo hero in Chapter 5
                romData[0x732ad] = power;

                // Force Alena, Brey, and Cristo to solo hero in Chapter 5
                romData[0x739d9] = power;
                romData[0x739de] = power;
                romData[0x739e9] = power;

                romData[0x41ad4] = power; // better yet, just fill the wagon full of solo heroes... you only need four... but sometimes it winds up less than four, and we don't want that.
                romData[0x41ad6] = 0x02; // LDY #$02 - it was #$09 - that causes a graphics crash with the line above.

                // A bunch of weird stuff preventing the Keeleon battle from occuring... (Ragnar demanding "the hero"...)
                romData[0x54e08] = power;
                romData[0x7724e] = power;
                // ... and Ragnar joining...
                romData[0x73634] = power;

                // This gets us past the Zenethian Helm block...
                romData[0x56c1d] = power; // Panon -> solo hero

                // This passes the Zenithian checks...
                romData[0x774cb] = power; // To enter the tower...
                romData[0x2377a] = power; // And the castle...

                // Only one unit of experience points - otherwise Taloon could get 10 times the experience points earned.
                //romData[0x41ff1] = 0x00;
            }

            Random r1;
            try
            {
                if (txtSeed.Text == "whoa") r1 = new Random((int)DateTime.Now.Ticks % 2147483647);
                    else r1 = new Random(int.Parse(txtSeed.Text));
            }
            catch
            {
                MessageBox.Show("Invalid seed.  It must be a number from 0 to 2147483648.");
                return;
            }

            int finalHero = 0;
            if (chkC14Random.Checked)
            {
                // Randomize the starting character for each chapter...
                // Come up with eight distinct numbers...
                int[] heroes = { 0, 1, 2, 3, 4, 5, 6, 7 };
                for (int lnI = 0; lnI < 100; lnI++)
                {
                    int numberToSwap1 = (r1.Next() % 8);
                    int numberToSwap2 = (r1.Next() % 8);
                    int swappy = heroes[numberToSwap1];
                    heroes[numberToSwap1] = heroes[numberToSwap2];
                    heroes[numberToSwap2] = swappy;
                }

                for (int lnI = 0; lnI < 5; lnI++)
                    romData[0x4914d + lnI] = (byte)(128 + heroes[lnI]); // This will ensure the same character starts each chapter.

                // Make the Cristo and Brey join change to the solo hero joining twice instead in Chapter 2.
                romData[0x79d44] = (byte)heroes[5];
                romData[0x79d49] = (byte)heroes[6];

                // Force the hero acting as the chapter 2 "princess" to fight in Chapter 2's tournament.  
                // This IS neccessary with random heroes... especially if Alena is acting as Cristo or Brey!
                romData[0x79074] = (byte)heroes[1];

                // Force the heroes acting as the chapter 4 "Nara and Mara" get out of Chapter 4 successfully.
                romData[0x7907a] = (byte)heroes[3];
                romData[0x7907b] = (byte)heroes[7];

                // Turn Nara into the solo hero in Chapter 4!
                romData[0x76b3c] = (byte)heroes[7];

                finalHero = heroes[4];
            }

            if (chkC5Random.Checked)
            {
                // Randomize the starting character for each chapter...
                // Come up with eight distinct numbers...
                int[] heroes = { 0, 1, 2, 3, 4, 5, 6, 7 };

                for (int lnI = 0; lnI < 100; lnI++)
                {
                    int numberToSwap1 = (r1.Next() % 8);
                    int numberToSwap2 = (r1.Next() % 8);
                    int swappy = heroes[numberToSwap1];
                    heroes[numberToSwap1] = heroes[numberToSwap2];
                    heroes[numberToSwap2] = swappy;
                }

                if (chkC14Random.Checked)
                {
                    for (int lnI = 0; lnI < 8; lnI++)
                    {
                        if (heroes[lnI] == finalHero)
                        {
                            int swappy = heroes[lnI];
                            heroes[lnI] = heroes[0];
                            heroes[0] = swappy;
                        }
                    }
                }

                // Force Nara to solo hero in Chapter 5
                romData[0x77903] = (byte)heroes[1];
                // Force Mara to solo hero in Chapter 5
                romData[0x54ad7] = (byte)heroes[0]; // Otherwise, Mara will think that Nara isn't a part of the party...
                romData[0x77909] = (byte)heroes[2];

                // Dodge an issue with Chapter 5's Cave Of Betrayal
                romData[0x41423] = 0xa0;
                romData[0x41424] = 0x00; // LDY $#00 - This will force whoever is in the lead to survive "the drop"
                romData[0x41425] = 0xea;

                // Force Taloon to solo hero in Chapter 5
                romData[0x732ad] = (byte)heroes[3];

                // Force Alena, Brey, and Cristo to solo hero in Chapter 5
                romData[0x739d9] = (byte)heroes[4];
                romData[0x739de] = (byte)heroes[5];
                romData[0x739e9] = (byte)heroes[6];

                // A bunch of weird stuff preventing the Keeleon battle from occuring... (Ragnar demanding "the hero"...)
                romData[0x54e08] = (byte)heroes[0];
                romData[0x7724e] = (byte)heroes[0];
                // ... and Ragnar joining...
                romData[0x73634] = (byte)heroes[7];

                // Give full control over all players in Chapter 5.  You lose the wagon control though.  I would LOVE to figure out how to get both though!  Maybe some nops?
                romData[0x46e1e] = 0x7f; // You can make it any number higher than 04, chapter 5 I think... 
            }

            // Make Chapter 2 adjustments if requested.  This is 
            if (chkCh2AwardXPTournament.Checked)
            {
                romData[0x60054 + (0xaf * 22) + 2] = 60;
                romData[0x60054 + (0xb0 * 22) + 2] = 80;
                romData[0x60054 + (0xb1 * 22) + 2] = 80;
                romData[0x60054 + (0xb2 * 22) + 2] = 100;
                romData[0x60054 + (0xba * 22) + 2] = 100;
            }

            // Make Chapter 3 adjustments if requested.
            // Make the shop one piece of gold... I still think that Chapter 3 sucks.  :)
            if (chkShop1.Checked)
            {
                romData[0x5603c] = 0x00;
                romData[0x56044] = 0x01;
            }
            if (chkShop25K.Checked)
            {
                romData[0x5603c] = 0x61;
                romData[0x56044] = 0xa8;
            }
            // This can make the tunnel one piece of gold...
            if (chkTunnel1.Checked)
            {
                romData[0x56641] = 0x00;
                romData[0x56645] = 0x01;
            }

            // Now adjust XP for all monsters...
            for (int lnI = 0; lnI <= 0xc2; lnI++)
            {
                double xp = (romData[0x60054 + (lnI * 22) + 3] * 255) + romData[0x60054 + (lnI * 22) + 2];
                if ((string)cboXPAdjustment.SelectedItem == "50%") xp = xp / 2;
                if ((string)cboXPAdjustment.SelectedItem == "150%") xp = xp * 3 / 2;
                if ((string)cboXPAdjustment.SelectedItem == "200%") xp = xp * 2;
                if ((string)cboXPAdjustment.SelectedItem == "250%") xp = xp * 5 / 2;
                if ((string)cboXPAdjustment.SelectedItem == "300%") xp = xp * 3;
                if ((string)cboXPAdjustment.SelectedItem == "400%") xp = xp * 4;
                if ((string)cboXPAdjustment.SelectedItem == "500%") xp = xp * 5;
                if (txtSeed.Text == "whoa") xp = 65000;

                if (chkXPRandom.Checked && txtSeed.Text != "whoa")
                    xp = (r1.Next() % (xp * 2));

                int xpTrue = (int)Math.Round(xp);
                if (xpTrue < 1 && !chkXPRandom.Checked) xpTrue = 1;
                if (xpTrue > 65000) xp = 65000;
                romData[0x60054 + (lnI * 22) + 3] = (byte)(xpTrue / 256);
                romData[0x60054 + (lnI * 22) + 2] = (byte)(xpTrue % 256);
            }

            // Then the gold for all monsters...
            for (int lnI = 0; lnI <= 0xc2; lnI++)
            {
                double xp = ((romData[0x60054 + (lnI * 22) + 20] % 4) * 256) + (romData[0x60054 + (lnI * 22) + 9]);
                if ((string)cboGoldAdjustment.SelectedItem == "50%") xp = xp / 2;
                if ((string)cboGoldAdjustment.SelectedItem == "150%") xp = xp * 3 / 2;
                if ((string)cboGoldAdjustment.SelectedItem == "200%") xp = xp * 2;
                if (txtSeed.Text == "whoa") xp = 1000;

                if (chkGoldRandom.Checked && txtSeed.Text != "whoa")
                    xp = (r1.Next() % (xp * 2));

                int xpTrue = (int)Math.Round(xp);
                if (xpTrue < 1 && !chkGoldRandom.Checked) xpTrue = 1;
                if (xpTrue > 1000) xp = 1000;
                romData[0x60054 + (lnI * 22) + 20] -= (byte)(romData[0x60054 + (lnI * 22) + 20] % 4);
                romData[0x60054 + (lnI * 22) + 20] += (byte)(xpTrue / 256);
                romData[0x60054 + (lnI * 22) + 9] = (byte)(xpTrue % 256);
            }

            // Finally, the encounter rate.  I've noticed that the encounter rate by default is VARIABLE!
            // 25% of normal = Branca Castle, north to the Heroes' Hometown, the approach to Necrosaro itself, and the Gardenbur Cave.  (the later two is guaranteed 1/64)
            // Part of the Zenithian Tower, Necrosaro's Castle, and Santeem Castle is 1/32 guaranteed.  (50% of normal)
            // The other part of Zenithian Tower is 1/24 guaranteed.  (75% of normal)
            // Frenor, the zones just outside of Colossus (not Dire Castle), Loch Tower (Chapter 1), Konenber to the Great Lighthouse, and Stancia all have a +25% encounter rate.
            // This makes Loch Tower a guaranteed 1/12.8 encounter rate.
            // Santeem and Endor in Chapter 2, and Lakanaba and the approach to the Cave Of Betrayal in Chapter 5 all have a +50% encounter rate.
            // The area outside of Tempe in Chapter 2 has a +75% encounter rate.
            // Finally, the big desert you must go through after acquiring the wagon has a +100% encounter rate.  This also may be why Fairy Water doesn't work through there.
            // I believe that makes the encounter rate 1/6.4 through there!
            for (int lnI = 0; lnI < 16; lnI++)
            {
                double encounterRate = (romData[0x6228b + lnI]);
                if ((string)cboEncounterRate.SelectedItem == "1/4") encounterRate = Math.Round(encounterRate / 4);
                if ((string)cboEncounterRate.SelectedItem == "1/3") encounterRate = Math.Round(encounterRate / 3);
                if ((string)cboEncounterRate.SelectedItem == "1/2") encounterRate = Math.Round(encounterRate / 2);
                if ((string)cboEncounterRate.SelectedItem == "2/3") encounterRate = Math.Round(encounterRate * 2 / 3);
                if ((string)cboEncounterRate.SelectedItem == "x1.5") encounterRate = Math.Round(encounterRate * 3 / 2);
                if ((string)cboEncounterRate.SelectedItem == "x2") encounterRate = Math.Round(encounterRate * 2);
                if ((string)cboEncounterRate.SelectedItem == "x2.5") encounterRate = Math.Round(encounterRate * 5 / 2);
                if ((string)cboEncounterRate.SelectedItem == "x3") encounterRate = Math.Round(encounterRate * 3);
                if ((string)cboEncounterRate.SelectedItem == "x4") encounterRate = Math.Round(encounterRate * 4);
                romData[0x6228b + lnI] = (byte)encounterRate;
            }

            if (chkRandomMonsters.Checked)
            {
                int[] monsterRank = // after 0x55, 0x??????, - bisonhawk unknown
                {
                    0x5c, 0x01, 0x00, 0x03, 0x02, 0x05, 0x04, 0x08, 0x07, 0x09, 0x06, 0x0b, 0x0e, 0x0a, 0x11, 0x0d, // 6
                    0x0f, 0x14, 0x0c, 0x1c, 0x1a, 0x18, 0x13, 0x10, 0x1f, 0x26, 0x16, 0x1e, 0x19, 0x17, 0x24, 0x1b, // 15
                    0x22, 0x23, 0x15, 0x1d, 0x2a, 0x20, 0x27, 0x25, 0x21, 0x43, 0x28, 0x2f, 0x31, 0x2c, 0x3c, 0x29, // 27
                    0x3d, 0x2d, 0x36, 0x45, 0x2e, 0x38, 0x39, 0x33, 0x42, 0x3e, 0x58, 0x4d, 0x40, 0x4a, 0x32, 0x47, // 45
                    0x2b, 0x35, 0x52, 0x48, 0x46, 0x37, 0x4c, 0xaf, 0x34, 0x5e, 0x3a, 0x4f, 0x66, 0xb3, 0x3b, 0x49, // 77
                    0xb0, 0xb1, 0x56, 0x41, 0x51, 0x50, 0x55, 0x57, 0x44, 0x5a, 0x3f, 0xb2, 0xba, 0x30, 0x53, 0x60, // 104
                    0x5b, 0x75, 0x5f, 0x4b, 0x68, 0x63, 0x5d, 0x54, 0x6b, 0x61, 0x67, 0x6e, 0x6a, 0x76, 0x12, 0x69,
                    0x70, 0x59, 0x78, 0x72, 0xab, 0x7c, 0x7d, 0x73, 0x71, 0x6c, 0x6f, 0x7f, 0x77, 0x74, 0x64, 0x86,
                    0x7a, 0x6d, 0x79, 0x7b, 0x80, 0x81, 0x7e, 0x87, 0x83, 0x88, 0x85, 0x82, 0x84, 0x62, 0x8c, 0x97,
                    0x89, 0x8a, 0x8d, 0x8b, 0x93, 0x90, 0xc0, 0x9c, 0x8e, 0x98, 0x95, 0xb4, 0x96, 0x9f, 0x9a, 0x99,
                    0x92, 0x9d, 0xa2, 0x94, 0x9e, 0x91, 0x8f, 0xa1, 0xa9, 0x9b, 0xa0, 0xa6, 0xaa, 0xac, 0x65, 0xa8,
                    0xb8, 0xa3, 0xa4, 0xa5, 0xa7, 0xbf, 0xb9, 0xbe, 0xb7, 0xb6, 0xb5, 0xc1, 0xc2, 0xbc // 15000 - bosses start at 0xb5
                };

                int[] maxMonster = { 0xb4, 0xb4, 0xb4, 0xb4, // 0x00-0x03 - Sea(0x00), wasted (0x01-0x03)
                    11, 21, 22, // 0x04-0x06 - Chapter 1
                    13, 22, 27, 41, 41, 71, // 0x07-0x0c - Chapter 2
                    11, 15, 24, 41, 45, 51, // 0x0d-0x12 - Chapter 4
                    0xbc, // 0x13 - Zenithian Tower
                    10, 15, 24, 31, // 0x14-0x17 - Chapter 3
                    11, 21, 26, 39, 39, 45, // 0x18-0x1d - Chapter 5 -> Symbol Of Faith
                    51, 55, 63, // 0x1e-0x20 - Chapter 5 -> Ship
                    85, 85, 94, 94, // 0x21-0x24 - Chapter 5 -> Padequila Root
                    0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, 0xb4, // 0x25-0x33 - Chapter 5 worldwide
                    0xbc, 0xbc, 0xbd, // 0x34-0x36 - Evil Island/Dark side - need to change this to include nasty bosses
                    0xbc, 0xbc, 0xbc, // 0x37-0x39 - Zenithian Tower - boss needs
                    0xbd, 0xbd, 0xbd, // 0x3a-0x3c - Final Cave - boss needs
                    0xbd, 0xbd, 0xbd, // 0x3d-0x3f - Necrosaro's Palace - boss needs
                    11, 24, 50, 50, // 0x40-0x43 - Chapter 1 - Cave To Izmit, Old Well, Loch Tower(2 zones)
                    33, 69, 69, 33, // 0x44-0x47 - Chapter 4 - Cave west of Kievs (1/4), Aktemto Mine (2/3)
                    12, 12, 39, 39, // 0x48-0x4b - Chapter 3 - Cave north of Lakanaba(2 zones), Cave of Silver Statuette(2 zones)
                    33, 33, 59, 59, // 0x4c-0x4f - Chapter 2 - Cave south of Frenor, Birdsong Tower
                    0xbc, // 0x50 - Chapter 5 - World Tree - boss needs
                    77, 77, // 0x51-0x52 - Chapter 5 - Great Lighthouse
                    104, // 0x53 - Chapter 5 - Cave of the Padequila
                    0xb2, 0xb2, // 0x54-0x55 - Chapter 5 - Cave West Of Kievs
                    0xb2, // Santeem
                    0xb2, 0xb2, // Cascade Cave
                    0xb2, // Shrine of Breaking Waves
                    0xb2, 0xb2, // Cave SE of Gardenbur
                    0xb2, 0xb2, // Royal Crypt
                    0xb2, 0xb2, 0xb2, // Colossus
                    0xbc, 0xbc, // Aktemto Mine
                    0xbc, 0xbc }; // World Tree

                int[] minMonster =
                {
                    0, 0, 0, 0, // 0x00-0x03 - Sea(0x00), wasted (0x01-0x03)
                    0, 0, 0, // 0x04-0x06 - Chapter 1
                    0, 0, 0, 0, 0, 0, // 0x07-0x0c - Chapter 2
                    0, 0, 0, 0, 0, 0, // 0x0d-0x12 - Chapter 4
                    127, // 0x13 - Zenithian Tower
                    0, 0, 0, 0, // 0x14-0x17 - Chapter 3
                    0, 0, 0, 0, 0, 0, // 0x18-0x1d - Chapter 5 -> Symbol Of Faith
                    0, 0, 0, // 0x1e-0x20 - Chapter 5 -> Ship
                    0, 0, 0, 0, // 0x21-0x24 - Chapter 5 -> Padequila Root
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0x25-0x33 - Chapter 5 worldwide
                    114, 114, 141, // 0x34-0x36 - Evil Island/Dark side - need to change this to include nasty bosses
                    127, 127, 127, // 0x37-0x39 - Zenithian Tower
                    135, 135, 135, // 0x3a-0x3c - Final Cave - boss needs
                    148, 148, 148, // 0x3d-0x3f - Necrosaro's Palace - boss needs
                    0, 0, 0, 0, // 0x40-0x43 - Chapter 1 - Cave To Izmit, Old Well, Loch Tower(2 zones)
                    0, 0, 0, 0, // 0x44-0x47 - Chapter 4 - Cave west of Kievs (1/4), Aktemto Mine (2/3)
                    0, 0, 0, 0, // 0x48-0x4b - Chapter 3 - Cave north of Lakanaba(2 zones), Cave of Silver Statuette(2 zones)
                    0, 0, 0, 0, // 0x4c-0x4f - Chapter 2 - Cave south of Frenor, Birdsong Tower
                    106, // 0x50 - Chapter 5 - World Tree - boss needs
                    0, 0, // 0x51-0x52 - Chapter 5 - Great Lighthouse
                    0, // 0x53 - Chapter 5 - Cave of the Padequila
                    0, 0, // 0x54-0x55 - Chapter 5 - Cave West Of Kievs
                    0, // Santeem
                    64, 64, // Cascade Cave
                    64, // Shrine of Breaking Waves
                    0, 0, // Cave SE of Gardenbur
                    80, 80, // Royal Crypt
                    94, 94, 94, // Colossus
                    106, 106, // Aktemto Mine
                    106, 106 }; // World Tree

                for (int lnI = 0; lnI <= 0x64; lnI++)
                {
                    int byteToUse = 0x612ba + (lnI * 16);
                    // Byte 0 - Day/night monsters
                    // Byte 1 - Minimum level to guarantee runaway (check this out someday)
                    // Byte 2-13 - Actual monsters
                    // Byte 14-15 - Special fights (make FF for now)
                    for (int lnJ = 0; lnJ < 16; lnJ++)
                    {
                        if (lnJ == 0)
                        {
                            romData[byteToUse + lnJ] -= (byte)(romData[byteToUse + lnJ] % 32);
                            romData[byteToUse + lnJ] += 0x1c; // This should ensure that we get to see nearly all of the monsters programmed.
                            continue;
                        }
                        if (lnJ == 1) continue;
                        romData[byteToUse + lnJ] = (byte)(monsterRank[(r1.Next() % (maxMonster[lnI] - minMonster[lnI])) + minMonster[lnI]]);
                        if (lnJ >= 14) romData[byteToUse + lnJ] = 0xff;
                    }
                }
            }

            if (chkSpeedUpBattles.Checked)
            {
                romData[0x502ff] = 1; // instead of 4.  6 frames saved each enemy hit when there is one monster left.
                romData[0x5033f] = 2; // instead of 8.  6 frames saved each enemy hit where there are two or more monsters left.
                romData[0x50420] = 2; // instead of 12.  10 frames saved each time YOU are hit.
                romData[0x63237] = 2; // instead of 12.  10 frames saved each time an encounter begins.  (flash)
                romData[0x62eb8] = 1; // instead of 29.  28 frames saved each time an encounter begins.  (spiral)
                // Speed up the message speed
                romData[0x48624] = 0x01;
                romData[0x48625] = 0x04;
                romData[0x48626] = 0x08;
                romData[0x48627] = 0x0f;
                romData[0x48628] = 0x1f;
                romData[0x48629] = 0x2f;
                romData[0x4862a] = 0x3f;

            }

            // Weapon stores start at 0x6341f
            // Armor stores start at 0x634a1
            // Item stores start at 0x63537
        }

        private void textGet()
        {
            List<string> txtStrings = new List<string>();
            string tempWord = "";
            for (int lnI = 0; lnI < 1913; lnI++)
            {
                int starter = 0x1b2da;
                if (romData[starter + lnI] == 255)
                {
                    txtStrings.Add(tempWord);
                    tempWord = "";
                }
                else if (romData[starter + lnI] >= 0 && romData[starter + lnI] <= 9)
                {
                    tempWord += (char)(romData[starter + lnI] + 39);
                }
                else if (romData[starter + lnI] >= 10 && romData[starter + lnI] <= 35)
                {
                    tempWord += (char)(romData[starter + lnI] + 87);
                }
                else if (romData[starter + lnI] >= 36 && romData[starter + lnI] <= 61)
                {
                    tempWord += (char)(romData[starter + lnI] + 29);
                }
            }
            using (StreamWriter writer = File.CreateText(Path.Combine(Path.GetDirectoryName(txtFileName.Text), "DW3Strings.txt")))
            {
                int lnJ = 1;
                foreach (string word in txtStrings)
                {
                    writer.WriteLine(lnJ.ToString("X3") + "-" + word);
                    lnJ++;
                }
            }
        }

        private bool loadRom(bool extra = false)
        {
            try
            {
                romData = File.ReadAllBytes(txtFileName.Text);
                if (extra)
                    romData2 = File.ReadAllBytes(txtCompare.Text);
            }
            catch
            {
                MessageBox.Show("Empty file name(s) or unable to open files.  Please verify the files exist.");
                return false;
            }
            return true;
        }

        private void saveRom()
        {
            string options = "";
            //string options = (chkHalfExpGoldReq.Checked ? "h" : "");
            //options += (chkDoubleXP.Checked ? "d" : "");
            //options += (chkRandStores.Checked ? "1" : "");
            //options += (chkRandStores.Checked ? "2" : "");
            //options += (chkRandStores.Checked ? "3" : "");
            //options += (chkRandStores.Checked ? "4" : "");
            //options += (chkRandStores.Checked ? "5" : "");
            //options += (chkRandStores.Checked ? "6" : "");
            //options += (chkRandStores.Checked ? "7" : "");
            //options += (chkRandStores.Checked ? "8" : "");
            //options += (optNoIntensity.Checked ? "_none" : radSlightIntensity.Checked ? "_slight" : radModerateIntensity.Checked ? "_moderate" : radHeavyIntensity.Checked ? "_heavy" : "_insane");
            string finalFile = Path.Combine(Path.GetDirectoryName(txtFileName.Text), "DW4RH_" + txtSeed.Text + options + ".nes");
            File.WriteAllBytes(finalFile, romData);
            lblIntensityDesc.Text = "ROM hacking complete!  (" + finalFile + ")";
            txtCompare.Text = finalFile;
        }

        private void doubleExp()
        {
            //// Divide encounter rates by three, rounding as needed.
            romData[0x944] = 2; // was 4
            romData[0x945] = 5; // was 15
            romData[0x946] = 4; // was 10
            romData[0x947] = 5; // was 15
            romData[0x948] = 6; // was 18
            romData[0x949] = 8; // was 25
            romData[0x94a] = 28; // was 84
            romData[0x94b] = 6; // was 18
            romData[0x94c] = 4; // was 10
            romData[0x94d] = 2; // was 5
            romData[0x94e] = 6; // was 19
            romData[0x94f] = 4; // was 13
            romData[0x950] = 6; // was 19
            romData[0x951] = 7; // was 22
            romData[0x952] = 10; // was 31 
            romData[0x953] = 28; // was 84
            romData[0x954] = 7; // was 22
            romData[0x955] = 3; // was 10

            // Replace monster data
            for (int lnI = 0; lnI < 125; lnI++)
            {
                int byteValStart = 0x32e3 + (23 * lnI);

                int xp = romData[byteValStart + 1] + (romData[byteValStart + 2] * 256);
                if (lnI != 0x31 && lnI != 0x6c)
                    xp = xp * 3 / 2;

                xp = (xp > 64000 ? 64000 : xp);

                romData[byteValStart + 1] = (byte)(xp % 256);
                romData[byteValStart + 2] = (byte)(xp / 256);
            }
        }

        private void forceItemSell()
        {
            int[] forcedItemSell = { 0x16, 0x1c, 0x28, 0x32, 0x34, 0x36, 0x3b, 0x3f, 0x42, 0x48, 0x4b, 0x4c, 0x50, 0x52, 0x53, 0x58, 0x59, 0x69, 0x6f, 0x70, 0x71 };
            for (int lnI = 0; lnI < forcedItemSell.Length; lnI++)
                if (romData[0x11be + forcedItemSell[lnI]] % 32 >= 16) // Not allowed to be sold
                    romData[0x11be + forcedItemSell[lnI]] -= 16; // Now allowed to be sold!

            int[] itemstoAdjust = { 0x16, 0x1c, 0x28, 0x32, 0x34, 0x36, 0x3b, 0x3f, 0x42, 0x48, 0x4b, 0x4c, 0x50, 0x52, 0x53, 0x58, 0x59, 0x5a, 0x69, 0x6f, 0x70, 0x71, // forced items to sell AND...
               0x5f, 0x60, 0x62, 0x64, 0x57, 0x75, 0x55, 0x4e, 0x4f, 0x49 }; // Some other items I want sold (see above)

            int[] itemPriceAdjust = { 5000, 35000, 15000, 10000, 8000, 12000, 10000, 800, 10, 5000, 5000, 8000, 20000, 1000, 1000, 500, 2000, 5000, 5000, 500, 2000, 500,
                5000, 3000, 2500, 5000, 800, 10000, 3000, 2000, 10000, 5000, 1000 };

            for (int lnI = 0; lnI < itemstoAdjust.Length; lnI++)
            {
                // Remove any price adjustment first.
                romData[0x11be + itemstoAdjust[lnI]] -= (byte)(romData[0x11be + itemstoAdjust[lnI]] % 4);
                int priceToUse = (romData[0x123b + itemstoAdjust[lnI]] >= 128 ? romData[0x123b + itemstoAdjust[lnI]] - 128 : romData[0x123b + itemstoAdjust[lnI]]);
                if (itemPriceAdjust[lnI] >= 10000)
                {
                    romData[0x11be + itemstoAdjust[lnI]] += 3; // Now multiply by 1000
                    romData[0x123b + itemstoAdjust[lnI]] = (byte)(romData[0x123b + itemstoAdjust[lnI]] >= 128 ? (itemPriceAdjust[lnI] / 1000) + 128 : itemPriceAdjust[lnI] / 1000);
                }
                else if (itemPriceAdjust[lnI] >= 1000)
                {
                    romData[0x11be + itemstoAdjust[lnI]] += 2; // Now multiply by 100
                    romData[0x123b + itemstoAdjust[lnI]] = (byte)(romData[0x123b + itemstoAdjust[lnI]] >= 128 ? (itemPriceAdjust[lnI] / 100) + 128 : itemPriceAdjust[lnI] / 100);
                }
                else if (itemPriceAdjust[lnI] >= 100)
                {
                    romData[0x11be + itemstoAdjust[lnI]] += 1; // Now multiply by 10
                    romData[0x123b + itemstoAdjust[lnI]] = (byte)(romData[0x123b + itemstoAdjust[lnI]] >= 128 ? (itemPriceAdjust[lnI] / 10) + 128 : itemPriceAdjust[lnI] / 10);
                }
                else
                {
                    romData[0x123b + itemstoAdjust[lnI]] = (byte)(romData[0x123b + itemstoAdjust[lnI]] >= 128 ? itemPriceAdjust[lnI] + 128 : itemPriceAdjust[lnI]);
                }
            }
        }

        private void halfExpAndGoldReq(bool special = false)
        {
            romData[0x282fd] = 25; // instead of 24 -> This will raise the exp earned by 133%

            // 0x11be for the multiplier.  Maintain two bits except for various key items that I want sold.
            // Sword Of Illusion - 5000G
            // Sword Of Kings - 35000G
            // Armor Of Radiance - 15000G
            // Magic Bikini - 10000G
            // Armor of Terrafirma - 8000G
            // Swordedge Armor - 12000G
            // Shield Of Heroes - 10000G
            // Golden Crown - 800G
            // Unlucky Helmet - 10G
            // Ring of Life - 5000G
            // Meteorite Armband - 5000G
            // Book of Satori - 8000G
            // Sage's Stone - 20000G
            // Vase Of Draught - 1000G
            // Lamp of Darkness - 1000G
            // Thief's Key - 500G
            // Magic Key - 2000G
            // Final Key - 5000G (not sold)
            // Echoing Flute - 500G
            // Fairy Flute - 2000G
            // Silver Harp - 500G

            // Change costs of various items because they might be sold from now on:
            // Strength seed:  5000G
            // Agility seed:  3000G
            // Luck seed:  2500G
            // Acorns Of Life:  5000G
            // Magic Ball:  800G
            // Stone of Sunlight:  10000G
            // Staff of change:  3000G
            // Stone of Life:  2000G
            // Wizard's Ring:  10000G
            // Black Pepper:  5000G
            // Shoes of Happiness:  5000G

            for (int lnI = 0; lnI < 125; lnI++)
            {
                // Need to determine original price...
                int priceMultiplier = (romData[0x11be + lnI] % 4 == 0 ? 1 : romData[0x11be + lnI] % 4 == 1 ? 10 : romData[0x11be + lnI] % 4 == 2 ? 100 : 1000);
                int priceMultiplier2 = (romData[0x123b + lnI] >= 128 ? romData[0x123b + lnI] - 128 : romData[0x123b + lnI]);
                // Don't bother reducing the price if it's 0 or 1 piece of gold.
                int price = priceMultiplier * priceMultiplier2;
                if (price == 0 || price == 1)
                    continue;

                price /= 2;

                // Remove any price adjustment first.
                romData[0x11be + lnI] -= (byte)(romData[0x11be + lnI] % 4);
                if (price >= 10000)
                {
                    romData[0x11be + lnI] += 3; // Now multiply by 1000
                    price /= 1000;
                }
                else if (price >= 1000)
                {
                    romData[0x11be + lnI] += 2; // Now multiply by 100
                    price /= 100;
                }
                else if (price >= 100)
                {
                    romData[0x11be + lnI] += 1; // Now multiply by 10
                    price /= 10;
                }
                else
                {
                    romData[0x11be + lnI] += 0;
                }

                // Must keep special effects if romData is >= 128
                if (romData[0x123b + lnI] > 128)
                    romData[0x123b + lnI] = (byte)(128 + price);
                else
                    romData[0x123b + lnI] = (byte)(price);
            }

            //// House of healing cost halved
            //romData[0x18659] = (20 / 2);

            // Inn prices halved
            for (int lnI = 0; lnI < 26; lnI++)
            {
                int innPrice = romData[0x367c1 + lnI] / 2;
                romData[0x367c1 + lnI] -= (byte)(romData[0x367c1 + lnI] % 32);
                romData[0x367c1 + lnI] += (byte)innPrice;
            }
        }

        private void superRandomize()
        {
            //Random r1;
            //try
            //{
            //    r1 = new Random(int.Parse(txtSeed.Text));
            //}
            //catch
            //{
            //    MessageBox.Show("Invalid seed.  It must be a number from 0 to 2147483648.");
            //    return;
            //}

            //if (chkRandEnemyPatterns.Checked)
            //{
            //    byte[] monsterSize = { 8, 4, 4, 4, 4, 4, 7, 4, 4, 8, 4, 4, 4, 2, 4, 4,
            //    4, 4, 5, 5, 2, 4, 4, 5, 4, 4, 4, 4, 4, 4, 3, 2,
            //    4, 4, 4, 2, 4, 5, 4, 4, 4, 4, 4, 8, 4, 4, 4, 3,
            //    2, 8, 4, 3, 4, 4, 2, 3, 4, 7, 3, 4, 2, 4, 4, 7,
            //    8, 3, 3, 4, 3, 2, 3, 4, 4, 4, 4, 4, 4, 3, 3, 4,
            //    2, 4, 3, 4, 3, 2, 2, 4, 3, 2, 2, 3, 2, 5, 1, 4,
            //    3, 3, 2, 3, 4, 1, 3, 3, 8, 7, 4, 2, 7, 4, 3, 2,
            //    3, 3, 3, 3, 3, 3, 3, 4, 4, 2, 1, 2, 4, 2, 3, 3,
            //    3, 1, 1, 3, 1, 1, 1, 2, 3, 3, 4 };

            //    // Totally randomize monsters (13805-13cd2)
            //    for (int lnI = 0; lnI < 0x8a; lnI++)
            //    {
            //        if (lnI == 0x85 || lnI == 0x86)
            //            continue; // Do not adjust either Zoma.

            //        //0 - Monster Level (probably used for running away)
            //        //1 - EXP
            //        //2 - EXP * 256
            //        //3 - Agility
            //        //4 - GP
            //        //5 - Attack
            //        //6 - Defense
            //        //7 - HP
            //        //8 - MP
            //        //9 - Item dropped
            //        //10 = Action 1
            //        //11 = Action 2(first half related to "AI-Lv)
            //        //12 = Action 3
            //        //13 = Action 4(first half related to "Pattern")
            //        //14 = Action 5(related to # atks, first bit)
            //        //15 = Action 6(also related to # atks, first bit)
            //        //16 = Action 7[1] = related to regen
            //        //17 = Action 8[1] = also related to regen 
            //        //18 - Bits 0-1 - GPx256, Bits 2-3 - Infernos resist, Bits 4-5 - Ice resist, Bits 6-7 - Fire resist
            //        //19 - Bits 0-1 - Attackx256, 2-3 - Sacrifice resist, 4-5 - Beat resist, 6-7 - Lightning resist
            //        //20 - Bits 0-1 - Defx256, 2-3 - Defense resist, 4-5 - Stopspell resist, 6-7 - Sleep resist
            //        //21 - Bits 0-1 - HPx256, 2-3 - Chaos resist, 4-5 - RobMagic resist, 6-7 - Surround resist
            //        //22 - Bits 0-3 - Drop chance (1/1, 8, 16, 32, 64, 128, 256, and 2048), 4-5 - Expel resist, 6-7 - Limbo/Slow resist
            //        byte[] enemyStats = { romData[0x32e3 + (lnI * 23) + 0], romData[0x32e3 + (lnI * 23) + 1], romData[0x32e3 + (lnI * 23) + 2], romData[0x32e3 + (lnI * 23) + 3], romData[0x32e3 + (lnI * 23) + 4],
            //        romData[0x32e3 + (lnI * 23) + 5], romData[0x32e3 + (lnI * 23) + 6], romData[0x32e3 + (lnI * 23) + 7], romData[0x32e3 + (lnI * 23) + 8], romData[0x32e3 + (lnI * 23) + 9],
            //        romData[0x32e3 + (lnI * 23) + 10], romData[0x32e3 + (lnI * 23) + 11], romData[0x32e3 + (lnI * 23) + 12], romData[0x32e3 + (lnI * 23) + 13], romData[0x32e3 + (lnI * 23) + 14],
            //        romData[0x32e3 + (lnI * 23) + 15], romData[0x32e3 + (lnI * 23) + 16], romData[0x32e3 + (lnI * 23) + 17], romData[0x32e3 + (lnI * 23) + 18], romData[0x32e3 + (lnI * 23) + 19],
            //        romData[0x32e3 + (lnI * 23) + 20], romData[0x32e3 + (lnI * 23) + 21], romData[0x32e3 + (lnI * 23) + 22] };

            //        int byteValStart = 0x32e3 + (23 * lnI);

            //        for (int lnJ = 3; lnJ <= 7; lnJ++)
            //        {
            //            int totalAtk = enemyStats[lnJ] + ((enemyStats[lnJ + 14] % 4) * 256);
            //            if (lnJ == 3) totalAtk = enemyStats[lnJ];
            //            if (lnJ == 7 && lnI == 0x87) totalAtk = 5; // We want Ortega to die quickly by giving him 5 HP.
            //            if (lnJ == 5 && lnI == 0x87) totalAtk = 2000; // ... or win the battle quickly by giving him hoards of strength!  (he still winds up "dead" I think)

            //            // Potentially add quadruple the possible gold for each monster.  Average 2 1/2 times...
            //            if (lnJ == 4 && totalAtk > 0)
            //                totalAtk += (r1.Next() % (totalAtk * 3));
            //            else
            //            {
            //                int atkRandom = (r1.Next() % 3);
            //                int atkDiv2 = (totalAtk / 2) + 1;
            //                if (atkRandom == 1)
            //                    totalAtk += (r1.Next() % atkDiv2);
            //                else if (atkRandom == 2)
            //                    totalAtk -= (r1.Next() % atkDiv2);
            //            }

            //            totalAtk = (totalAtk < 1 ? 1 : totalAtk);
            //            totalAtk = (totalAtk > 1020 ? 1020 : totalAtk);
            //            if (lnJ == 3)
            //                totalAtk = (totalAtk > 255 ? 255 : totalAtk);
            //            enemyStats[lnJ] = (byte)(totalAtk % 256);
            //            if (lnJ > 3)
            //                enemyStats[lnJ + 14] = (byte)(enemyStats[lnJ + 14] - (enemyStats[lnJ + 14] % 4) + (totalAtk / 256));
            //        }
            //        enemyStats[8] = 255; // Always make sure the monster has MP

            //        // Needs to be a "legal treasure..."
            //        byte[] legalMonsterTreasures = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            //                        0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
            //                        0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
            //                        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
            //                        0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x48, 0x49, 0x4b, 0x4c, 0x4e,
            //                        0x50, 0x55, 0x56, 0x5f,
            //                        0x60, 0x62, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6c, 0x6d,
            //                        0x70, 0x71, 0x73, 0x74,
            //                        0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74, 0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74,
            //                        0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74, 0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74 };
            //        enemyStats[9] = (legalMonsterTreasures[r1.Next() % legalMonsterTreasures.Length]);

            //        byte[] res1 = { 0, 0, 0, 0, 0, 1, 2, 3 };
            //        byte[] res2 = { 0, 0, 0, 0, 1, 1, 2, 3 };
            //        byte[] res3 = { 0, 0, 0, 1, 1, 2, 2, 3 };
            //        byte[] res4 = { 0, 0, 1, 1, 2, 2, 3, 3 };
            //        byte[] res5 = { 0, 1, 1, 2, 2, 3, 3, 3 };
            //        byte[] res6 = { 0, 1, 2, 2, 3, 3, 3, 3 };
            //        byte[] res7 = { 0, 1, 2, 3, 3, 3, 3, 3 };
            //        byte[] finalRes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //        for (int lnJ = 0; lnJ < finalRes.Length; lnJ++)
            //        {
            //            if (lnI < 25)
            //                finalRes[lnJ] = (res1[r1.Next() % 8]);
            //            else if (lnI < 50)
            //                finalRes[lnJ] = (res2[r1.Next() % 8]);
            //            else if (lnI < 70)
            //                finalRes[lnJ] = (res3[r1.Next() % 8]);
            //            else if (lnI < 90)
            //                finalRes[lnJ] = (res4[r1.Next() % 8]);
            //            else if (lnI < 105)
            //                finalRes[lnJ] = (res5[r1.Next() % 8]);
            //            else if (lnI < 115)
            //                finalRes[lnJ] = (res6[r1.Next() % 8]);
            //            else
            //                finalRes[lnJ] = (res7[r1.Next() % 8]);
            //        }

            //        enemyStats[18] = (byte)(enemyStats[18] % 4 + (finalRes[0] * 4) + (finalRes[1] * 16) + (finalRes[2] * 64));
            //        enemyStats[19] = (byte)(enemyStats[19] % 4 + (finalRes[3] * 4) + (finalRes[4] * 16) + (finalRes[5] * 64));
            //        enemyStats[20] = (byte)(enemyStats[20] % 4 + (finalRes[6] * 4) + (finalRes[7] * 16) + (finalRes[8] * 64));
            //        enemyStats[21] = (byte)(enemyStats[21] % 4 + (finalRes[9] * 4) + (finalRes[10] * 16) + (finalRes[11] * 64));
            //        // First part:  item drop chance.  Make sure it's at best 1/8.
            //        if (lnI == 0x36 || lnI == 0x62) // EXCEPT Man-eater Chests and Mimics
            //            enemyStats[22] = (byte)(0 + (finalRes[12] * 16) + (finalRes[13] * 64));
            //        else
            //            enemyStats[22] = (byte)(((r1.Next() % 7) + 1) + (finalRes[12] * 16) + (finalRes[13] * 64));

            //        byte[] enemyPatterns = { 2, 2, 2, 2, 2, 2, 2, 2 };

            //        // Types of patterns... 0:  Attack only, 1:  "Goofy attack", 2:  Totally random, 3:  Annoying, 4:  Quite annyoing, 5:  Hell monster
            //        byte[] pattern1 = { 45, 65, 100, 100, 100 };
            //        byte[] pattern2 = { 35, 60, 90, 100, 100 };
            //        byte[] pattern3 = { 25, 50, 80, 90, 100 };
            //        byte[] pattern4 = { 15, 45, 75, 85, 100 };
            //        byte[] pattern5 = { 10, 40, 70, 85, 100 };
            //        byte[] pattern6 = { 5, 30, 70, 80, 100 };
            //        byte[] pattern7 = { 0, 20, 60, 80, 100 };
            //        byte[] pattern8 = { 0, 10, 50, 60, 100 };
            //        byte[] pattern9 = { 0, 0, 30, 30, 100 };

            //        int enemyPattern = r1.Next() % 100;

            //        if (lnI < 15 || lnI == 0x87 || lnI == 0x68) // Ortega, so he dies quickly, and red slime, because that monster is WAY out of order
            //            enemyPattern = (enemyPattern < pattern1[0] ? 0 : enemyPattern < pattern1[1] ? 1 : enemyPattern < pattern1[2] ? 2 : enemyPattern < pattern1[3] ? 3 : 4);
            //        else if (lnI < 30)
            //            enemyPattern = (enemyPattern < pattern2[0] ? 0 : enemyPattern < pattern2[1] ? 1 : enemyPattern < pattern2[2] ? 2 : enemyPattern < pattern2[3] ? 3 : 4);
            //        else if (lnI < 45 || lnI == 0x88 || lnI == 0x8a) // Kandar 1 and Kandar Henchman
            //            enemyPattern = (enemyPattern < pattern3[0] ? 0 : enemyPattern < pattern3[1] ? 1 : enemyPattern < pattern3[2] ? 2 : enemyPattern < pattern3[3] ? 3 : 4);
            //        else if (lnI < 60)
            //            enemyPattern = (enemyPattern < pattern4[0] ? 0 : enemyPattern < pattern4[1] ? 1 : enemyPattern < pattern4[2] ? 2 : enemyPattern < pattern4[3] ? 3 : 4);
            //        else if (lnI < 75 || lnI == 0x89) // Kandar 2
            //            enemyPattern = (enemyPattern < pattern5[0] ? 0 : enemyPattern < pattern5[1] ? 1 : enemyPattern < pattern5[2] ? 2 : enemyPattern < pattern5[3] ? 3 : 4);
            //        else if (lnI < 90)
            //            enemyPattern = (enemyPattern < pattern6[0] ? 0 : enemyPattern < pattern6[1] ? 1 : enemyPattern < pattern6[2] ? 2 : enemyPattern < pattern6[3] ? 3 : 4);
            //        else if (lnI < 105)
            //            enemyPattern = (enemyPattern < pattern7[0] ? 0 : enemyPattern < pattern7[1] ? 1 : enemyPattern < pattern7[2] ? 2 : enemyPattern < pattern7[3] ? 3 : 4);
            //        else if (lnI < 120)
            //            enemyPattern = (enemyPattern < pattern8[0] ? 0 : enemyPattern < pattern8[1] ? 1 : enemyPattern < pattern8[2] ? 2 : enemyPattern < pattern8[3] ? 3 : 4);
            //        else
            //            enemyPattern = (enemyPattern < pattern9[0] ? 0 : enemyPattern < pattern9[1] ? 1 : enemyPattern < pattern9[2] ? 2 : enemyPattern < pattern9[3] ? 3 : 4);

            //        switch (enemyPattern)
            //        {
            //            case 0: // leave everything alone; it's a basic attack monster.
            //                break;
            //            case 1: // Give the monster a little goofyness to their attack...
            //                for (int lnJ = 0; lnJ < 8; lnJ++)
            //                {
            //                    // 50% chance of setting a different attack.
            //                    byte[] attackPattern = { 2, 2, 2, 2, 2, 0, 1, 3, 4, 5, 6, 8 };
            //                    byte random = (attackPattern[r1.Next() % attackPattern.Length]);
            //                    if (random != 2)
            //                        enemyPatterns[lnJ] = random;
            //                }
            //                break;
            //            case 2:
            //                for (int lnJ = 0; lnJ < 8; lnJ++)
            //                {
            //                    // 75% chance of setting a different attack.
            //                    byte random = (byte)(r1.Next() % 80);
            //                    if (random != 2 && random < 64 && random != 0x2b)
            //                        enemyPatterns[lnJ] = random;
            //                }
            //                break;
            //            case 3:
            //                for (int lnJ = 0; lnJ < 8; lnJ++)
            //                {
            //                    // Normal, heroic, poison, faint, heal, healmore (both self and others), sleep, stopspell, weak flames, 
            //                    // poison and sweet breaths, call for help, double attacks, and strange jigs.
            //                    byte[] attackPattern = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 3, 4, 5, 6, 8, 9, 10, 13, 16, 17, 19, 22, 23, 28, 34, 35, 36, 38, 39, 41, 45, 49, 54, 59 };
            //                    byte random = (attackPattern[r1.Next() % attackPattern.Length]);
            //                    if (random != 2 && random < 64)
            //                        enemyPatterns[lnJ] = random;
            //                }
            //                break;
            //            case 4:
            //                for (int lnJ = 0; lnJ < 8; lnJ++)
            //                {
            //                    byte[] attackPattern = { 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 6, 6, 8, 11, 12, 14, 15, 18, 20, 21, 24, 25, 26, 27, 29, 30, 31, 32, 33, 34, 40, 40, 42, 44, 47, 48, 51, 53, 56, 58, 60 };
            //                    byte random = (attackPattern[r1.Next() % attackPattern.Length]);
            //                    if (random >= 1 && random <= 31) // 0 would be fine, but it's already set.
            //                        enemyPatterns[lnJ] = random;
            //                }
            //                break;
            //        }

            //        if (lnI == 0x31 || lnI == 0x6c) // Metal slime, Metal Babble
            //        {
            //            enemyPatterns[0] = 7; // run away
            //            enemyPatterns[1] = 7; // run away
            //            enemyPatterns[2] = 7; // run away
            //            enemyPatterns[3] = 7; // run away
            //            if (lnI == 0x41)
            //            {
            //                enemyPatterns[4] = 7; // run away
            //                enemyPatterns[5] = 7; // run away
            //            }
            //        }
            //        if (lnI == 0x05 || lnI == 0x28)
            //        { // Healer, Curer
            //            byte[] attackPattern = { 49, 50, 51, 52, 53, 54, 55, 56, 57, 58 };
            //            enemyPatterns[0] = (attackPattern[r1.Next() % attackPattern.Length]);
            //            enemyPatterns[1] = (attackPattern[r1.Next() % attackPattern.Length]);
            //            enemyPatterns[2] = (attackPattern[r1.Next() % attackPattern.Length]);
            //            enemyPatterns[3] = (attackPattern[r1.Next() % attackPattern.Length]);
            //        }
            //        if (lnI == 0x0c || lnI == 0x14)
            //        { // Poison Toad, Poison Silkworm
            //            byte[] attackPattern = { 5, 17 };
            //            enemyPatterns[0] = (attackPattern[r1.Next() % attackPattern.Length]);
            //            enemyPatterns[1] = (attackPattern[r1.Next() % attackPattern.Length]);
            //            enemyPatterns[2] = (attackPattern[r1.Next() % attackPattern.Length]);
            //            enemyPatterns[3] = (attackPattern[r1.Next() % attackPattern.Length]);
            //        }
            //        if (lnI == 0x07 || lnI == 0x22 || lnI == 0x25 || lnI == 0x28 || lnI == 0x2e || lnI == 0x34 || lnI == 0x35 || // Magician, Lumpus, Mage Toadstool, Nev, Evil Mage, Demonite, Deranger
            //            lnI == 0x3c || lnI == 0x4f || lnI == 0x50 || lnI == 0x59 || lnI == 0x5f || lnI == 0x6b || lnI == 0x77 || lnI == 0x78) // Witch, Witch Doctor, Old Hag, Voodoo Shaman, Minidemon, Voodoo Warlock, Archmage, Magiwyvern
            //        {
            //            enemyPatterns[0] = (byte)((r1.Next() % 38) + 19); // Any magic spell
            //            enemyPatterns[1] = (byte)((r1.Next() % 38) + 19);
            //            enemyPatterns[2] = (byte)((r1.Next() % 38) + 19);
            //            enemyPatterns[3] = (byte)((r1.Next() % 38) + 19);
            //        }
            //        if (lnI == 0x12) // Gas clouds
            //        {
            //            enemyPatterns[0] = (byte)((r1.Next() % 3) + 16); // breathe something
            //            enemyPatterns[1] = (byte)((r1.Next() % 3) + 16); // breathe something
            //        }
            //        // Flamapede, Heat Cloud, Sky Dragon, Lava Basher, Orochi, Salamander, Hydra, Green Dragon, King Hydra
            //        if (lnI == 0x23 || lnI == 0x29 || lnI == 0x3a || lnI == 0x4e || lnI == 0x65 || lnI == 0x67 || lnI == 0x7a || lnI == 0x7c || lnI == 0x81)
            //        {
            //            enemyPatterns[0] = (byte)((r1.Next() % 3) + 10); // breathe fire
            //            enemyPatterns[1] = (byte)((r1.Next() % 3) + 10); // breathe fire
            //        }
            //        if (lnI == 0x52 || lnI == 0x5b || lnI == 0x5d) // Glacier Basher, Snow Dragon, Frost Cloud
            //        {
            //            enemyPatterns[0] = (byte)((r1.Next() % 3) + 13); // breathe ice
            //            enemyPatterns[1] = (byte)((r1.Next() % 3) + 13); // breathe ice
            //        }
            //        if (lnI == 0x57) // Bomb Crag
            //            enemyPatterns[0] = 21; // Sacrifice!  :)

            //        // Both bits set = 2 attacks guaranteed.  2nd bit set = up to 3 attacks.  1st bit set = up to 2 attacks.
            //        int badChance = (3 * lnI > 300 ? 300 : 3 * lnI);
            //        if (r1.Next() % 1000 < badChance / 4)
            //            enemyPatterns[5] += 128;
            //        else if (r1.Next() % 1000 < badChance / 3)
            //        {
            //            enemyPatterns[4] += 128;
            //            enemyPatterns[5] += 128;
            //        }
            //        else if (r1.Next() % 1000 < badChance)
            //            enemyPatterns[4] += 128;

            //        if (r1.Next() % 1000 < badChance / 3)
            //        {
            //            enemyPatterns[6] += 128;
            //            enemyPatterns[7] += 128;
            //        }
            //        else if (r1.Next() % 1000 < badChance / 2)
            //            enemyPatterns[7] += 128;
            //        else if (r1.Next() % 1000 < badChance)
            //            enemyPatterns[6] += 128;

            //        for (int lnJ = 0; lnJ < 8; lnJ++)
            //            enemyStats[10 + lnJ] = (enemyPatterns[lnJ]);

            //        for (int lnJ = 0; lnJ < 23; lnJ++)
            //            romData[byteValStart + lnJ] = enemyStats[lnJ];
            //    }
            //}

            //if (chkRandMonsterZones.Checked)
            //{
            //    // Aliahan 1, 2, 3, Promontory Cave, Tower of Najimi B, 1, 2, Aliahan 4, Enticement Cave 1, 2, Romaly, Kanave, Champange Tower, Noaniels, Dream Cave, Assaram, Isis 1, 2, Pyramid 1, 2, 3
            //    List<int> gentleZones = new List<int>() { 4, 5, 6, 65, 66, 67, 68, 7, 69, 70, 8, 9, 71, 72, 10, 74, 75, 12, 13, 14, 76, 77, 80 };
            //    List<int> violentZone1 = new List<int>() { 78, 48, 79, 81 }; // Cave of Necrogund
            //    List<int> violentZone2 = new List<int>() { 82, 39, 11 }; // Baramos Castle
            //    List<int> violentZone3 = new List<int>() { 64, 50, 51, 52, 54, 55, 57, 58, 60, 61, 63, 59, 62, 40, 53, 56 };  // Tantegel overworld, caves, and towers
            //    List<int> violentZone4 = new List<int>() { 25, 34, 38, 63 }; // Zoma's Castle
            //                                                                 // Totally randomize monster zones
            //    for (int lnI = 0; lnI < 95; lnI++)
            //    {
            //        int byteToUse = 0xaeb + (lnI * 15);
            //        bool nonViolent = false;
            //        for (int lnJ = 1; lnJ < 13; lnJ++)
            //        {
            //            if (gentleZones.IndexOf(lnI) != -1)
            //                romData[byteToUse + lnJ] = monsterOrder[r1.Next() % ((gentleZones.IndexOf(lnI) * 2) + 8)];
            //            else if (violentZone1.Contains(lnI))
            //                romData[byteToUse + lnJ] = monsterOrder[(r1.Next() % 92) + 40];
            //            else if (violentZone2.Contains(lnI))
            //                romData[byteToUse + lnJ] = monsterOrder[(r1.Next() % 72) + 60];
            //            else if (violentZone3.Contains(lnI))
            //                romData[byteToUse + lnJ] = monsterOrder[(r1.Next() % 56) + 80];
            //            else if (violentZone4.Contains(lnI))
            //                romData[byteToUse + lnJ] = monsterOrder[(r1.Next() % 37) + 99];
            //            else
            //            {
            //                romData[byteToUse + lnJ] = monsterOrder[r1.Next() % 131];
            //                nonViolent = true;
            //            }
            //        }
            //        if (nonViolent && r1.Next() % 3 == 1)
            //        {
            //            romData[byteToUse + 13] = (byte)(r1.Next() % 20);
            //            romData[byteToUse + 14] = (byte)(r1.Next() % 20);
            //        }
            //    }

            //    // Randomize the 19 special battles
            //    for (int lnI = 0; lnI < 20; lnI++)
            //    {
            //        int byteToUse = 0x107a + (6 * lnI);
            //        for (int lnJ = 0; lnJ < 4; lnJ++)
            //        {
            //            if (r1.Next() % 2 == 1 || lnJ == 3)
            //                romData[byteToUse + lnJ] = monsterOrder[r1.Next() % 129];
            //        }
            //    }

            //    // Not sure we can really randomize boss fights... (ff separates boss fights - 0x8ee-0x918 AND 0x919-0x944)
            //    // But I can change the Mummy Men treasure fights to Shadow fights!
            //    romData[0x909] = 0x18; // was 0x20 - Mummy Men
            //                           // We could randomize the Granite Titan and Boss Troll fights too...
            //                           // Maybe remove two of the Kandar Henchmen in the first fight and place two "bonus monsters" in other fights...

            //    //// Randomize the first 12 boss fights, but make sure the last four of those involve Atlas, Bazuzu, Zarlox, and Hargon.
            //    //// The 13th and final fight cannot be manipulated:  Malroth, and Malroth alone.
            //    //for (int lnI = 0; lnI < 18; lnI++)
            //    //{
            //    //    int byteToUse = 0x8ee + (lnI * 2);
            //    //    int byteToUse2 = 0x919 + (lnI * 2);
            //    //    int boss1 = (lnI >= 8 ? 78 + (lnI - 8) : ((r1.Next() % 77) + 1));
            //    //    boss1 = (lnI == 0 ? (r1.Next() % 40) + 1 : boss1);
            //    //    int quantity1 = (boss1 >= 80 ? 1 : (r1.Next() % monsterSize[boss1]) + 1);
            //    //    int boss2 = (lnI == 0 ? (r1.Next() % 40) + 1 : (r1.Next() % 78) + 1);
            //    //    romData[byteToUse + 0] = (byte)boss1;
            //    //    romData[byteToUse + 1] = (byte)quantity1;
            //    //    romData[byteToUse + 2] = (byte)boss2;
            //    //    romData[byteToUse + 3] = 8; // It's too many monsters, but the width of the screen will trim the rest of the monsters off.
            //    //}
            //}

            //if (chkRandEquip.Checked)
            //{
            //    string[] weaponText = { "Cypress stick", "Club", "Copper sword", "Magic Knife", "Iron Spear", "Battle Axe", "Broad Sword", "Wizard's Wand",
            //    "Poison Needle", "Iron Claw", "Thorn Whip", "Giant Shears", "Chain Sickle", "Thor's Sword", "Snowblast Sword", "Demon Axe",
            //    "Staff of Rain", "Sword of Gaia", "Staff of Reflection", "Sword of Destruction", "Multi - Edge Sword", "Staff of Force", "Sword of Illusion", "Zombie Slasher",
            //    "Falcon Sword", "Sledge Hammer", "Thunder Sword", "Staff of Thunder", "Sword of Kings", "Orochi Sword", "Dragon Killer", "Staff of Judgement",
            //    "Clothes", "Training Suit", "Leather Armor", "Flashy Clothes", "Half Plate Armor", "Full Plate Armor", "Magic Armor", "Cloak of Evasion",
            //    "Armor of Radiance", "Iron Apron", "Animal Suit", "Fightting Suit", "Sacred Robe", "Armor of Hades", "Water Flying Cloth", "Chain Mail",
            //    "Wayfarers Clothes", "Revealing Swimsuit", "Magic Bikini", "Shell Armor", "Armor of Terrafirma", "Dragon Mail", "Swordedge Armor", "Angel's Robe",
            //    "Leather Shield", "Iron Shield", "Shield of Strength", "Shield of Heroes", "Shield of Sorrow", "Bronze Shield", "Silver Shield", "Golden Crown",
            //    "Iron Helmet", "Mysterious Hat", "Unlucky Helmet", "Turban", "Noh Mask", "Leather Helmet", "Iron Mask", "Golden Claw" };

            //    // Randomize which items equate to which effects
            //    // Select 21 items randomly from a set defined as follows:
            //    int[] legalEffectItems = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            //                          0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
            //                          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
            //                          0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
            //                          0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46 };

            //    List<int> keyEffectItems = new List<int> { 0x10, 0x11 };

            //    // Wipe out the use byte by totally resetting the price.
            //    for (int lnI = 0; lnI < legalEffectItems.Length; lnI++)
            //    {
            //        int oldVal = romData[0x11be + legalEffectItems[lnI]];
            //        romData[0x11be + legalEffectItems[lnI]] = (byte)(oldVal % 32);
            //        //romData[0x11be + legalEffectItems[lnI]] = (byte)(oldVal % 32 >= 16 ? 0x10 : 0x00);
            //        //romData[0x11be + legalEffectItems[lnI]] += (byte)(oldVal % 16 >= 8 ? 0x08 : 0x00);
            //    }
            //    int oldVal1 = romData[0x11be + 0x4a];
            //    romData[0x11be + 0x4a] = (byte)(oldVal1 % 32);
            //    int oldVal2 = romData[0x11be + 0x5b];
            //    romData[0x11be + 0x5b] = (byte)(oldVal2 % 32);

            //    int[] legalItemSpells = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            //                          0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
            //                          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e,
            //                          0x30, 0x31, 0x32, 0x34,
            //                          0x38, 0x39, 0x3a }; // restore MP, everyone sneezes, self numb - 54 spells total

            //    List<int> enemyGroupSpells = new List<int> { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x09, 0x0a, 0x0b, 0x0d, 0x0e, 0x0f,
            //                           0x10, 0x12, 0x13, 0x15, 0x16, 0x17, 0x18, 0x22, 0x24, 0x25, 0x27, 0x2b, 0x2c }; // 25
            //    List<int> enemyAllSpells = new List<int> { 0x06, 0x07, 0x08, 0x0c, 0x11, 0x14, 0x39 }; // 7
            //    List<int> allySelfSpells = new List<int> { 0x1a, 0x1b, 0x1c, 0x23, 0x29, 0x2d, 0x30, 0x32, 0x34, 0x38, 0x3a }; // 11
            //    List<int> allySelectSpells = new List<int> { 0x20, 0x21, 0x28 }; // 3
            //    List<int> allyAllSpells = new List<int> { 0x19, 0x1d, 0x1e, 0x1f, 0x26, 0x2a, 0x2e, 0x31 }; // 8

            //    for (int lnI = 0; lnI < 21; lnI++)
            //    {
            //        int effectItem = legalEffectItems[r1.Next() % legalEffectItems.Length];
            //        if (romData[0x11be + effectItem] >= 0x80) // If it's already been selected...
            //        {
            //            lnI--;
            //        }
            //        else
            //        {
            //            romData[0x11be + effectItem] += 0x80;
            //        }
            //    }

            //    int iSpell = -1;
            //    for (int lnI = 0; lnI < legalEffectItems.Length; lnI++)
            //    {
            //        // Otherwise, randomize the spell it will be using.
            //        if (romData[0x11be + legalEffectItems[lnI]] < 0x80)
            //            continue;

            //        int effectSpell = legalItemSpells[r1.Next() % legalItemSpells.Length];
            //        if (effectSpell == 0x38 && keyEffectItems.Contains(effectSpell)) // Can't let a key item potentially crumble!  Redo that randomization.
            //        {
            //            lnI--;
            //            continue;
            //        }

            //        iSpell++;
            //        // Now determine what spell it is... that will determine whether to "attack" yourself, a group of monsters, a selected ally, or all monsters/allies.
            //        if (enemyGroupSpells.Contains(effectSpell))
            //            romData[0x11be + legalEffectItems[lnI]] += 0x60;
            //        else if (enemyAllSpells.Contains(effectSpell))
            //            romData[0x11be + legalEffectItems[lnI]] += 0x20;
            //        else if (allySelfSpells.Contains(effectSpell)) // 50/50 chance of targetting for self or an ally.
            //            romData[0x11be + legalEffectItems[lnI]] += (byte)(r1.Next() % 2 == 1 ? 0x00 : 0x40);
            //        else if (allySelectSpells.Contains(effectSpell))
            //            romData[0x11be + legalEffectItems[lnI]] += 0x40;
            //        else if (allyAllSpells.Contains(effectSpell))
            //            romData[0x11be + legalEffectItems[lnI]] += 0x00;

            //        romData[0x13280 + iSpell] = (byte)effectSpell;
            //    }

            //    // Totally randomize weapons, armor, shields, helmets (13efb-13f1d, 1a00e-1a08b for pricing)
            //    for (int lnI = 0; lnI <= 70; lnI++)
            //    {
            //        byte power = 0;

            //        if (lnI == 0 || lnI == 1 || lnI == 2 || lnI == 32 || lnI == 34 || lnI == 48)
            //            power = (byte)(r1.Next() % 12);
            //        else if (lnI < 31)
            //            power = (byte)(Math.Pow(r1.Next() % 1000, 2.5) / 243252); // max 130
            //        else if (lnI < 55)
            //            power = (byte)(Math.Pow(r1.Next() % 1000, 2.5) / 395284); // max 80
            //        else if (lnI < 62)
            //            power = (byte)(Math.Pow(r1.Next() % 1000, 2.5) / 574959); // max 55
            //        else
            //            power = (byte)(Math.Pow(r1.Next() % 1000, 2.5) / 903507); // max 35
            //        power += 2; // To avoid 0 power...
            //        romData[0x279a0 + lnI] = power;

            //        // You want a max price of about 20000, shields 18300, helmets 15000
            //        double price = Math.Round((lnI < 31 ? Math.Pow(power, 2.04) : lnI < 55 ? Math.Pow(power, 2.26) : lnI < 62 ? Math.Pow(power, 2.45) : Math.Pow(power, 2.7)), 0);
            //        // TO DO:  Round to the nearest 10 (after 100GP), 50(after 1000 GP), or 100 (after 2500 GP)
            //        price = (float)Math.Round(price, 0);

            //        //// Remove any price adjustment first.
            //        romData[0x11be + lnI] -= (byte)(romData[0x11be + lnI] % 4);
            //        if (price >= 10000)
            //        {
            //            romData[0x11be + lnI] += 3; // Now multiply by 1000
            //            price /= 1000;
            //        }
            //        else if (price >= 1000)
            //        {
            //            romData[0x11be + lnI] += 2; // Now multiply by 100
            //            price /= 100;
            //        }
            //        else if (price >= 100)
            //        {
            //            romData[0x11be + lnI] += 1; // Now multiply by 10
            //            price /= 10;
            //        }
            //        else
            //        {
            //            romData[0x11be + lnI] += 0;
            //        }

            //        // Must keep special effects if romData is >= 128
            //        if (lnI < 80)
            //        {
            //            if (romData[0x123b + lnI] >= 128)
            //                romData[0x123b + lnI] = (byte)(128 + price);
            //            else
            //                romData[0x123b + lnI] = (byte)(price);

            //            if (lnI <= 2)
            //            {
            //                if ((romData[0x123b + lnI] % 16) >= 8)
            //                    romData[0x123b + lnI] -= (byte)((romData[0x123b + lnI] % 8) + 1);
            //            }
            //        }
            //    }

            //    string options = (chkHalfExpGoldReq.Checked ? "h" : "");
            //    options += (chkDoubleXP.Checked ? "d" : "");
            //    //options += (optNoIntensity.Checked ? "_none" : radSlightIntensity.Checked ? "_slight" : radModerateIntensity.Checked ? "_moderate" : radHeavyIntensity.Checked ? "_heavy" : "_insane");
            //    string finalFile = Path.Combine(Path.GetDirectoryName(txtFileName.Text), "DW3Random_" + txtSeed.Text + options + "_guide.txt");

            //    // Totally randomize who can equip (1a3ce-1a3f0).  At least one person can equip something...
            //    using (StreamWriter writer = File.CreateText(finalFile))
            //    {
            //        for (int lnI = 0; lnI <= 70; lnI++)
            //        {
            //            // Maintain equipment requirements for the starting equipment
            //            if (!(lnI == 0x00 || lnI == 0x01 || lnI == 0x02 || lnI == 0x20 || lnI == 0x22 || lnI == 0x30))
            //                romData[0x1147 + lnI] = (byte)(r1.Next() % 255 + 1);

            //            // EXCEPT those that are "FF", update the "who can use the item" to the people who are allowed to equip the item
            //            if (romData[0x1196 + lnI] != 255 && romData[0x1196 + lnI] != 0 && lnI < 32)
            //                romData[0x1196 + lnI] = romData[0x1147 + lnI];

            //            string equipOut = "";
            //            equipOut += (romData[0x1147 + lnI] % 2 >= 1 ? "Hr  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] % 32 >= 16 ? "Sr  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] % 8 >= 4 ? "Pr  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] % 4 >= 2 ? "Wi  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] % 16 >= 8 ? "Sg  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] % 128 >= 64 ? "Fi  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] % 64 >= 32 ? "Mr  " : "--  ");
            //            equipOut += (romData[0x1147 + lnI] >= 128 ? "Gf  " : "--  ");
            //            equipOut += (romData[0x11be + lnI] >= 128 ? "**  " : "    ");
            //            equipOut += (romData[0x279a0 + lnI]);
            //            writer.WriteLine(weaponText[lnI].PadRight(24) + equipOut);
            //        }
            //    }

            //    // Remove the lines that penalize a fighter for not equipping claws.
            //    romData[0x1507] = romData[0x1508] = romData[0x1509] = romData[0x150a] = 0xea;
            //}

            //if (chkRandSpellLearning.Checked)
            //{
            //    // Totally randomize spell learning
            //    // First, clear out all of the magic bytes...
            //    for (int lnI = 0; lnI < 252; lnI++)
            //        romData[0x29d6 + lnI] = 0x3f;
            //    int[] fightSpells = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53 };
            //    int[] commandSpells = { 26, 27, 28, 30, 31, 32, 33, 38, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61 };
            //    // Randomize 8 command spells for the hero, pilgrim, and wizard.
            //    int[] heroCommand = { 26, 27, 28, 30, 31, 32, 33, 52, 53, 54, 55, 56, 57, 58, 60, 61 };
            //    int[] pilgrimCommand = { 27, 28, 30, 31, 32, 33, 38, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61 };
            //    int[] wizardCommand = { 26, 27, 28, 30, 31, 32, 33, 38, 53, 54, 55, 56, 57, 58, 59, 60, 61 };

            //    for (int lnI = 0; lnI < commandSpells.Length * 20; lnI++)
            //    {
            //        swapArray(heroCommand, (r1.Next() % heroCommand.Length), (r1.Next() % heroCommand.Length));
            //        swapArray(pilgrimCommand, (r1.Next() % pilgrimCommand.Length), (r1.Next() % pilgrimCommand.Length));
            //        swapArray(wizardCommand, (r1.Next() % wizardCommand.Length), (r1.Next() % wizardCommand.Length));
            //    }

            //    // Randomize 16 fight spells for the hero, and 24 spells for the pilgrim, and wizard.
            //    int[] heroFight = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 39, 40, 41, 42, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53 };
            //    int[] pilgrimFight = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53 };
            //    int[] wizardFight = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 48, 49, 50, 51, 53 };

            //    bool legal = false;
            //    while (!legal)
            //    {
            //        for (int lnI = 0; lnI < fightSpells.Length * 20; lnI++)
            //        {
            //            swapArray(heroFight, (r1.Next() % heroFight.Length), (r1.Next() % heroFight.Length));
            //            swapArray(pilgrimFight, (r1.Next() % pilgrimFight.Length), (r1.Next() % pilgrimFight.Length));
            //            swapArray(wizardFight, (r1.Next() % wizardFight.Length), (r1.Next() % wizardFight.Length));
            //        }

            //        bool healAll = false;
            //        bool healUs = false;
            //        bool healUsAll = false;
            //        // Must have HealAll, HealUs, and HealUsAll somewhere or the Zoma fight isn't going to work well...
            //        for (int lnJ = 0; lnJ < 24; lnJ++)
            //        {
            //            if (lnJ < 16)
            //            {
            //                if (heroFight[lnJ] == 28 || pilgrimFight[lnJ] == 28 || wizardFight[lnJ] == 28)
            //                    healAll = true;
            //                if (heroFight[lnJ] == 30 || pilgrimFight[lnJ] == 30 || wizardFight[lnJ] == 30)
            //                    healUs = true;
            //                if (heroFight[lnJ] == 31 || pilgrimFight[lnJ] == 31 || wizardFight[lnJ] == 31)
            //                    healUsAll = true;
            //            }
            //            else
            //            {
            //                if (pilgrimFight[lnJ] == 28 || wizardFight[lnJ] == 28)
            //                    healAll = true;
            //                if (pilgrimFight[lnJ] == 30 || wizardFight[lnJ] == 30)
            //                    healUs = true;
            //                if (pilgrimFight[lnJ] == 31 || wizardFight[lnJ] == 31)
            //                    healUsAll = true;
            //            }
            //        }
            //        if (healAll && healUs && healUsAll)
            //            legal = true;
            //    }

            //    for (int lnI = 0; lnI < 8; lnI++)
            //    {
            //        romData[0x29d6 + heroCommand[lnI]] = (byte)(r1.Next() % 35 + 1);
            //        romData[0x2a15 + pilgrimCommand[lnI]] = (byte)(r1.Next() % 35 + 1);
            //        romData[0x2a54 + wizardCommand[lnI]] = (byte)(r1.Next() % 35 + 1);
            //        romData[0x2a93 + pilgrimCommand[lnI]] = romData[0x2a15 + pilgrimCommand[lnI]];
            //        romData[0x2a93 + wizardCommand[lnI]] = romData[0x2a54 + wizardCommand[lnI]];
            //        romData[0x22e7 + 24 + lnI] = (byte)heroCommand[lnI];
            //        romData[0x22e7 + 32 + 24 + lnI] = (byte)pilgrimCommand[lnI];
            //        romData[0x22e7 + 64 + 24 + lnI] = (byte)wizardCommand[lnI];
            //    }
            //    romData[0x22e7 + 24] = 38; // Hero learns Return first.
            //    romData[0x29d6 + romData[0x22e7 + 24]] = 2;
            //    romData[0x22e7 + 24 + 1] = 59; // ... and Outside
            //    romData[0x29d6 + romData[0x22e7 + 24 + 1]] = 2;
            //    romData[0x22e7 + 32 + 24] = 26; // Wizard learns Heal first.
            //    romData[0x29d6 + romData[0x22e7 + 32 + 24]] = 2;
            //    romData[0x22e7 + 64 + 24] = 52; // Pilgrim learns Antidote first.
            //    romData[0x29d6 + romData[0x22e7 + 64 + 24]] = 2;

            //    romData[0x29d6 + 63 + romData[0x22e7 + 32 + 24]] = 1;
            //    romData[0x29d6 + 126 + romData[0x22e7 + 64 + 24]] = 1;

            //    for (int lnI = 0; lnI < 24; lnI++)
            //    {
            //        if (lnI < 16)
            //            romData[0x29d6 + heroFight[lnI]] = (byte)(r1.Next() % 35 + 1);
            //        romData[0x2a15 + pilgrimFight[lnI]] = (byte)(r1.Next() % 35 + 1);
            //        romData[0x2a54 + wizardFight[lnI]] = (byte)(r1.Next() % 35 + 1);
            //        romData[0x2a93 + pilgrimFight[lnI]] = romData[0x2a15 + pilgrimFight[lnI]];
            //        romData[0x2a93 + wizardFight[lnI]] = romData[0x2a54 + wizardFight[lnI]];
            //        if (lnI < 16)
            //            romData[0x22e7 + lnI] = (byte)heroFight[lnI];
            //        romData[0x22e7 + 32 + lnI] = (byte)pilgrimFight[lnI];
            //        romData[0x22e7 + 64 + lnI] = (byte)wizardFight[lnI];
            //    }
            //    romData[0x29d6 + romData[0x22e7]] = 2;

            //    // Must "complete the sentence" or really bad things happen...
            //    romData[0x29d6 + 62] = 0xff;
            //    romData[0x29d6 + 125] = 0xff;
            //    romData[0x29d6 + 188] = 0xff;
            //    romData[0x29d6 + 251] = 0xff;
            //}

            //if (chkRandSpellStrength.Checked)
            //{
            //    // Totally randomize spell strengths - first, attack spells
            //    for (int lnI = 0; lnI < 17; lnI++)
            //    {
            //        int byteToUse = 0x134b1 + (lnI * 2);
            //        romData[byteToUse] = (byte)((r1.Next() % 200) + 2);
            //        if (lnI == 0x0d || lnI == 0x0e || lnI == 0x0f)
            //            romData[byteToUse + 1] = (byte)(r1.Next() % romData[byteToUse]);
            //        else
            //            romData[byteToUse + 1] = (byte)(r1.Next() % (romData[byteToUse] / 2));
            //    }

            //    // And then healing spells
            //    for (int lnI = 0; lnI < 6; lnI++)
            //    {
            //        if (lnI == 2 || lnI == 5) continue; // Healall/Healusall
            //        int byteToUse = 0x134f9 + (lnI * 2);
            //        romData[byteToUse] = (byte)((r1.Next() % 200) + 2);
            //        romData[byteToUse + 1] = (byte)(r1.Next() % (romData[byteToUse] / 2));
            //    }
            //}

            //if (chkRandTreasures.Checked)
            //{
            //    bool legal = false;

            //    // Totally randomize treasures... but make sure key items exist before they are needed!
            //    // Keep the Rainbow drop where it is
            //    int[] treasureAddrZ0 = { 0x29237, 0x29238, 0x29239, // Promontory Cave
            //    0x2927b, 0x292C4, 0x292C5, 0x292c6 }; // Najimi Tower - Thief's Key, Magic Ball - 7
            //    int[] treasureAddrZ1 = { 0x2927c, 0x2927d }; // Najimi Tower behind Thief's Key - Magic Ball - 2
            //    int[] treasureAddrZ2 = { 0x2927e, 0x2927f, // Enticement cave
            //    0x29234, 0x29235, // Kanave
            //    0x2923a, 0x2923b, 0x29280, 0x29281, 0x29282, 0x29283, 0x29284, 0x29285, 0x29286, 0x29287, // Dream cave/Wake Up Powder
            //    0x29252, 0x292d2, 0x292e6, // champange tower
            //    0x2925c, // isis meteorite band
            //    0x29249, 0x2924a, 0x2924b, 0x2924c, 0x2924d, 0x2924e, 0x2924f, 0x292b4, 0x292b5, 0x292b6 }; // Pyramid -> Magic key - 28
            //    int[] treasureAddrZ3 = { 0x292c3, 0x317f4, // Pyramid continued
            //    0x29255, 0x29256, 0x29257, 0x29258, 0x29249, 0x2924a, // Aliahan continued
            //    0x31b9c, 0x2925d, 0x2925e, 0x2925f, 0x29260, 0x29261, 0x29262, 0x29263, 0x29264, // Isis continued
            //    0x29269, 0x2926a, 0x2926b }; // Portuga -> Royal Scroll - 20
            //    int[] treasureAddrZ4 = { 0x2923c, 0x2923d, // Dwarf's Cave
            //    0x29251, 0x292c7, 0x292c8, 0x292c9, 0x292ca, 0x292b7, // Garuna Tower
            //    0x29242, 0x29240, 0x2923f, 0x2923e, 0x29241, 0x29243, 0x2928b, 0x2928c, 0x2928e, 0x2928d }; // Kidnapper's Cave -> Black Pepper - 18
            //    int[] treasureAddrZ5 = { 0x31b94, 0x29270, // Tedan (except Green Orb)
            //    0x292e4, 0x292e7, // Jipang
            //    0x29272, 0x29271, 0x29273, // Pirate Cove
            //    0x292d1, 0x292d0, 0x292cf, 0x292cd, 0x292ce, 0x292cc, 0x292cb }; // Arp Tower - Final Key - 14
            //    int[] treasureAddrZ6 = { 0x29299, 0x2929c, 0x2929b, 0x2929d, 0x2929a, 0x29298, 0x29293, 0x29294, 0x29295, 0x29291, 0x29292, // Samanao Cave
            //    0x29296, 0x29297, 0x292a3, 0x292a4, 0x292a2, 0x2929f, 0x2929e, 0x292a0, 0x292a5, 0x292a6, 0x292a1, 0x292a7, 0x29296, // Samanao Cave
            //    0x29246, 0x29248, 0x29247, 0x29245, 0x29244, 0x29290, 0x2928f }; // Lancel Cave - Mirror Of Ra - 31
            //    int[] treasureAddrZ7 = { 0x292e5 }; // Staff Of Change - Samanao Castle - 1
            //    int[] treasureAddrZ8 = { 0x29277, 0x29276, 0x29275, 0x29278, 0x29279, 0x2927a }; // Sword Of Gaia - Ghost ship - 6
            //    int[] treasureAddrZ10 = { 0x29288, 0x29289, 0x2928a }; // All orbs - Cave Of Necrogund - 3
            //    int[] treasureAddrZ11 = { 0x29267, 0x29266, 0x29265, 0x29268, // Tantegel Castle
            //    0x292a8, 0x292ab, 0x292ac, 0x292aa, 0x292a9, // Erdrick's Cave
            //    0x29274, // Garin's home
            //    0x292df, 0x292e3, 0x292e1, 0x292e2, 0x292e0, // Rocky Mountain Cave
            //    0x31b90, // Hauksness
            //    0x31b88, // Kol
            //    0x29253, 0x29254, 0x292d7, 0x292d5, 0x292d6, 0x292d8, 0x292da, 0x292d9, 0x292db, 0x292dc, 0x292de, 0x292dd, // Kol Tower
            //    0x29233 }; // Rimuldar - Staff Of Rain, Stones Of Sunlight, Sacred Amulet - 30
            //    int[] treasureAddrZ12 = { 0x292ad, 0x292ae, 0x292af, 0x292b0, 0x292b1, 0x292b2, 0x292b3 }; // Zoma's Castle - Sphere of Light - 7
            //    int[] treasureAddrZ13 = { 0x2922a, 0x29229, 0x29228, // Baramos's Castle
            //    0x292b7, 0x292b8, 0x292b9, 0x292ba, 0x292bb, 0x292bc, 0x292bd, 0x292be, 0x292bf, 0x292c0, 0x292c1, 0x292c2, // Pyramid Mummy Men Chests
            //    0x2925b, // Eginbear
            //    0x31b9f, // World Tree
            //    0x31b97, // Luzami
            //    0x31b8c, // Soo
            //    0x2922b, // Final Key Shrine
            //    0x2926c, 0x2926d, 0x31b80, // New Town  0x378A9
            //    0x37DF1, 0x375aa, 0x37786, 0x37cb9, 0x377D5, 0x37828, 0x377fe, 0x37907, 0x37929, 0x37d5a, 0x37a25, 0x37d9d }; // NPCs - Dead zone - 33

            //    // NOTICE:  Using 0x3b785, supposedly the wake-up powder NPC, warps you to weird places after jumping off the rope in the tower of Garuna...

            //    List<int> allTreasureList = new List<int>();

            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ0);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ1);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ2);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ3);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ4);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ5);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ6);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ7);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ8);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ10);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ11);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ12);
            //    allTreasureList = addTreasure(allTreasureList, treasureAddrZ13);

            //    int[] allTreasure = allTreasureList.ToArray();

            //    // randomize starting gold
            //    romData[0x2914f] = (byte)(r1.Next() % 256);

            //    List<byte> treasureList = new List<byte>();
            //    byte[] legalTreasures = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            //                          0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
            //                          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
            //                          0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
            //                          0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4b, 0x4c, 0x4e, 0x4f,
            //                          0x50, 0x51, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5c, 0x5d, 0x5f,
            //                          0x60, 0x62, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6c, 0x6d,
            //                          0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x78, 0x79, 0x7a, 0x7b,
            //                          0x88, 0x90, 0x98, 0xa0, 0xa8, 0xb0, 0xb8, 0xc0, 0xc8, 0xd0, 0xd8, 0xe0, 0xe8, 0xf0, 0xf8,
            //                          0xfd, 0xfe, 0xff, 0xfd, 0xfe, 0xff, 0xfd, 0xfe, 0xff, 0xfd, 0xfe, 0xff, 0xfd, 0xfe, 0xff};
            //    for (int lnI = 0; lnI < allTreasureList.Count; lnI++)
            //    {
            //        legal = false;
            //        while (!legal)
            //        {
            //            byte treasure = (byte)((r1.Next() % legalTreasures.Length)); // the last two items we can't get...
            //            treasure = legalTreasures[treasure];
            //            // Disallow earning gold for searchable items... this is because 0x80 = 0x00 in this scenario, so anything over 0x80 is useless.  
            //            // (in fact, 0xfd = 0x7d, the Stick Slime, a null item.)
            //            if (allTreasure[lnI] > 0x29400 && treasure >= 0x80)
            //                continue;

            //            //byte[] keyItems = { 0x59, 0x5a, 0x54, 0x11, 0x78, 0x79, 0x7a, 0x7b, 0x10, 0x75 };
            //            //byte[] minKeyTreasure = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 134, 134 };
            //            //byte[] keyTreasure = { 37, 116, 124, 130, 133, 133, 133, 133, 165, 165 };

            //            // We need to make sure key items doesn't exceed a certain point in the story.
            //            if ((treasure == 0x58 && lnI >= 6) ||
            //                (treasure == 0x57 && lnI >= 8) ||
            //                (treasure == 0x59 && lnI >= 36) ||
            //                (treasure == 0x5d && lnI >= 56) ||
            //                (treasure == 0x4f && lnI >= 74) ||
            //                (treasure == 0x5a && lnI >= 88) ||
            //                (treasure == 0x51 && lnI >= 119) ||
            //                (treasure == 0x54 && lnI >= 120) ||
            //                (treasure == 0x11 && lnI >= 126) ||
            //                (treasure == 0x77 && lnI >= 129) ||
            //                (treasure == 0x78 && lnI >= 129) ||
            //                (treasure == 0x79 && lnI >= 129) ||
            //                (treasure == 0x7a && lnI >= 129) ||
            //                (treasure == 0x7b && lnI >= 129) ||
            //                (treasure == 0x7c && lnI >= 129) ||
            //                (treasure == 0x72 && !(lnI >= 130 && lnI <= 166)) ||
            //                (treasure == 0x10 && !(lnI >= 130 && lnI <= 159)) ||
            //                (treasure == 0x75 && !(lnI >= 130 && lnI <= 159)))
            //                continue;

            //            // Verify that only one location exists for key items
            //            if (!(treasureList.Contains(treasure) && (treasure == 0x58 || treasure == 0x57 || treasure == 0x59 || treasure == 0x5d || treasure == 0x4f
            //                || treasure == 0x5a || treasure == 0x51 || treasure == 0x54 || treasure == 0x11 || treasure == 0x4f || treasure == 0x53
            //                || treasure == 0x77 || treasure == 0x78 || treasure == 0x79 || treasure == 0x7a || treasure == 0x7b || treasure == 0x7c
            //                || treasure == 0x5c || treasure == 0x70 || treasure == 0x71 || treasure == 0x72 || treasure == 0x10 || treasure == 0x75)))
            //            {
            //                legal = true;
            //                treasureList.Add(treasure);
            //                romData[allTreasure[lnI]] = treasure;
            //            }
            //        }
            //    }

            //    // Verify that key items are available in either a store or a treasure chest in the right zone.
            //    byte[] keyItems = { 0x58, 0x57, 0x59, 0x5d, 0x4f, 0x5a, 0x51, 0x54, 0x11, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x10, 0x75, 0x72 };
            //    byte[] minKeyTreasure = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 130, 130, 130 };
            //    byte[] keyTreasure = { 6, 8, 36, 56, 74, 88, 119, 120, 126, 129, 129, 129, 129, 129, 129, 159, 159, 166 };
            //    for (int lnI = 0; lnI < keyItems.Length; lnI++)
            //    {
            //        legal = false;
            //        for (int lnJ = minKeyTreasure[lnI]; lnJ < keyTreasure[lnI]; lnJ++)
            //        {
            //            if (romData[allTreasure[lnJ]] == keyItems[lnI])
            //                legal = true;
            //        }

            //        // If legal = false, then the item was not found, so we'll have to place it in a treasure somewhere...
            //        while (!legal)
            //        {
            //            byte tRand = (byte)((r1.Next() % (keyTreasure[lnI] - minKeyTreasure[lnI] + 1)) + minKeyTreasure[lnI]);

            //            bool dupCheck = false;
            //            // Make sure we're not replacing a item that also happens to be key!
            //            for (int lnJ = 0; lnJ < keyItems.Length; lnJ++)
            //            {
            //                if (romData[allTreasure[tRand]] == keyItems[lnJ])
            //                    dupCheck = true;
            //            }
            //            if (dupCheck == false)
            //            {
            //                romData[allTreasure[tRand]] = keyItems[lnI];
            //                legal = true;
            //            }
            //        }
            //    }
            //}

            //if (chkRandStores.Checked)
            //{
            //    // Totally randomize stores (19 weapon stores, 24 item stores, 248 items total)  No store can have more than 12 items.
            //    // I would just create random values for 248 items, then determine weapon and item stores out of that!
            //    int[] legalStoreItems = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            //                          0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
            //                          0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
            //                          0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f,
            //                          0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
            //                          0x48, 0x49, 0x4b, 0x4c, 0x4e,
            //                          0x50, 0x53, 0x55, 0x56, 0x5f,
            //                          0x60, 0x62, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6c, 0x6d,
            //                          0x70, 0x71, 0x73, 0x74,
            //                          0x56, 0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74,
            //                          0x56, 0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74,
            //                          0x56, 0x65, 0x66, 0x67, 0x68, 0x6c, 0x73, 0x74
            //};

            //    int[] storeItems = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //    List<int> itemStore = new List<int>();
            //    List<int> weaponStore = new List<int>();
            //    for (int lnI = 0; lnI < 248; lnI++)
            //        romData[0x36838 + lnI] = (byte)(legalStoreItems[(r1.Next() % legalStoreItems.Length)]);

            //    int[] weaponStoreLocations = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 247 };

            //    int lnMarker = -1;
            //    // Need to make sure this doesn't exceed 11
            //    int average = 248 / weaponStoreLocations.Length;
            //    for (int lnI = 0; lnI < weaponStoreLocations.Length - 1; lnI++)
            //    {
            //        int storeSize = average;
            //        storeSize += (-3 + (r1.Next() % 7));
            //        if (storeSize > 12 || average >= 10)
            //            storeSize = 12;
            //        lnMarker += storeSize;
            //        weaponStoreLocations[lnI] = lnMarker;
            //        int avgPart1 = 248 - lnMarker + 1;
            //        int avgPart2 = weaponStoreLocations.Length - lnI - 1;
            //        average = ((248 - lnMarker + 1) / (weaponStoreLocations.Length - lnI - 1));
            //    }

            //    // Now we can plug in the numbers...
            //    for (int lnI = 0; lnI < weaponStoreLocations.Length; lnI++)
            //        romData[0x36838 + weaponStoreLocations[lnI]] += 128;

            //    // Go through each of the stores and check for duplicates.
            //    int startStore = 0;
            //    List<int> storeContents = new List<int> { };
            //    for (int lnI = 0; lnI < 248; lnI++)
            //    {
            //        bool lastItem = (romData[0x36838 + lnI] >= 128);
            //        int itemToCompare = (romData[0x36838 + lnI] >= 128 ? romData[0x36838 + lnI] - 128 : romData[0x36838 + lnI]);
            //        if (storeContents.Contains(itemToCompare))
            //        {
            //            romData[0x36838 + lnI] = (byte)((lastItem ? 128 : 0) + legalStoreItems[r1.Next() % legalStoreItems.Length]);
            //            lnI = startStore - 1;
            //            storeContents.Clear();
            //            continue;
            //        }
            //        storeContents.Add(itemToCompare);
            //        if (lastItem)
            //        {
            //            storeContents.Clear();
            //            startStore = lnI + 1;
            //        }
            //    }

            //    // Inn prices randomized
            //    for (int lnI = 0; lnI < 26; lnI++)
            //    {
            //        int innPrice = (r1.Next() % 20) + 1;
            //        romData[0x367c1 + lnI] -= (byte)(romData[0x367c1 + lnI] % 32);
            //        romData[0x367c1 + lnI] += (byte)innPrice;
            //    }
            //}

            //if (chkRandStatGains.Checked)
            //{
            //    //// Randomize starting stats.
            //    // Give each hero from 22HP (min for Wizard) to about 36 HP.  (Hero)  Just so everybody has a chance!
            //    romData[0x1eed7] = (byte)((r1.Next() % 13) + 5 + 9);
            //    // Remove the baseline for HP...
            //    romData[0x24f4] = 0xea;
            //    romData[0x24f5] = 0x4c;
            //    romData[0x24f6] = 0xfa;
            //    romData[0x24f7] = 0xa4;
            //    // ... and MP...
            //    romData[0x2555] = 0xea;
            //    romData[0x2556] = 0x4c;
            //    romData[0x2557] = 0x5b;
            //    romData[0x2558] = 0xa5;
            //    // ... and the rest!  But we also need to prevent someone gaining 200 points in a stat...
            //    romData[0x247c] = 0xa9;
            //    romData[0x247d] = 0x00;
            //    romData[0x247e] = 0x8d;
            //    romData[0x247f] = 0x05;
            //    romData[0x2480] = 0x00;
            //    romData[0x2481] = 0x4c;
            //    romData[0x2482] = 0x7d;
            //    romData[0x2483] = 0xa4;

            //    //romData[0x2480] = 0xea;

            //    // Randomize stat gains.
            //    // First, we'll randomize the multipliers.  They will range from 4 to 16, in multiples of 4.

            //    for (int lnI = 0; lnI < 10; lnI++)
            //    {
            //        if (lnI == 4 || lnI == 9)
            //            romData[0x281b + lnI] = 16;
            //        else
            //            romData[0x281b + lnI] = (byte)(((r1.Next() % 4) + 1) * 4);
            //    }

            //    //int[] baseStat = { 3, 2, 4, 3, 2, // Hero
            //    //                   1, 3, 2, 3, 2, // Wizard
            //    //                   2, 2, 2, 3, 2, // Pilgrim
            //    //                   2, 2, 2, 2, 2, // Sage
            //    //                   3, 1, 4, 1, 1, // Soldier
            //    //                   3, 2, 3, 2, 1, // Merchant
            //    //                   4, 4, 3, 3, 1, // Fighter
            //    //                   3, 4, 3, 4, 1}; // Goof-off, s/b 1, 2, 2, 10, 3, but rewarding for trying something crazy
            //    //int[] baseStat = { 15, 14, 14, 15, 15, // Hero (7 vit, 3 int originally)
            //    //                   11, 15, 14, 14, 14, // Wizard (5 vit, 5 int originally)
            //    //                   12, 13, 14, 15, 14, // Pilgrim (4 vit, 5 int originally)
            //    //                   14, 14, 14, 14, 14, // Sage (6 vit, 4 int originally)
            //    //                   12, 12, 14, 11, 11, // Soldier (7 vit originally)
            //    //                   14, 14, 14, 14, 13, // Merchant (4 vit originally)
            //    //                   12, 14, 14, 14, 12, // Fighter (6 vit originally)
            //    //                   12, 12, 13, 10, 12}; // Goof-off, s/b 2, 2, 3, 10, 2, but rewarding for trying something crazy

            //    //int[] baseStat = { 5, 4, 7, 5, 4, // Hero (7 vit, 3 int originally)
            //    //                   1, 6, 5, 6, 6, // Wizard (5 vit, 5 int originally)
            //    //                   2, 3, 4, 5, 6, // Pilgrim (4 vit, 5 int originally)
            //    //                   4, 4, 6, 4, 5, // Sage (6 vit, 4 int originally)
            //    //                   6, 2, 7, 1, 1, // Soldier (7 vit originally)
            //    //                   4, 4, 4, 3, 3, // Merchant (4 vit originally)
            //    //                   8, 8, 6, 6, 2, // Fighter (6 vit originally)
            //    //                   2, 2, 3, 10, 2}; // Goof-off, s/b 2, 2, 3, 10, 2, but rewarding for trying something crazy

            //    // ORDER:  Strength, agility, vitality, luck, intelligence
            //    int[] statAdjust = { 1, 1, 1, 1, 1, // Hero
            //                   -3, 0, -1, 1, 3, // Wizard
            //                   -1, -2, 0, 1, 2, // Pilgrim
            //                   0, 0, 1, -1, 1, // Sage
            //                   3, -3, 3, -3, 0, // Soldier
            //                   0, 0, 0, 0, 0, // Merchant
            //                   1, 2, -1, -2, 0, // Fighter
            //                   2, 2, 2, 2, 0}; // Goof-off, big rewards for trying something crazy

            //    for (int lnI = 0; lnI < 40; lnI++)
            //    {
            //        bool multA = (r1.Next() % 2 == 1);
            //        // Determine maximum base stat for the stats in mind.  Remember... average gain = base * mult / 13.75
            //        // We want to reach 250 by level 48.
            //        double attribute = 0;
            //        for (int lnJ = 0; lnJ < 4; lnJ++)
            //            attribute += (romData[0x281b + (multA ? 0 : 5) + lnJ] / 13.75 * (lnJ == 4 ? 24 : 6));

            //        int maxBase = (int)Math.Floor(attribute);
            //        // UP NEXT:  Randomize up to max base, then adjust for each character!
            //        int newBase = r1.Next() % maxBase;

            //        //int newBase = baseStat[lnI];
            //        //int upDown = (r1.Next() % 3);
            //        //if (upDown == 0)
            //        //    newBase--;
            //        //else if (upDown == 2)
            //        //    newBase++;

            //        if (newBase < 1)
            //            newBase = 1;
            //        if (newBase > 15)
            //            newBase = 15;

            //        if (lnI >= 16 && lnI < 24 && newBase < (int)Math.Ceiling((double)maxBase / 3))
            //            newBase = (int)Math.Ceiling((double)maxBase / 3); // Vitality base REALLY needs to be 1/3 max or greater or you'll never survive.
            //        if (lnI >= 32 && lnI < 36 && newBase < (int)Math.Ceiling((double)maxBase / 3))
            //            newBase = (int)Math.Ceiling((double)maxBase / 3); // Intelligence base REALLY needs to be 1/3 max or greater or you'll never get MP.
            //        if (lnI >= 36 && lnI < 40)
            //            newBase = 0; // Give out no intelligence to non-MP users.
            //                         //int charLevel = 0;
            //        for (int lnJ = 0; lnJ < 5; lnJ++)
            //        {
            //            if (lnJ == 0) // Determine Multiplier path A or B with byte 0.
            //                romData[0x290e + (lnI * 5) + lnJ] = (byte)(!multA ? 128 : 0); // (byte)(lnI >= 16 && lnI < 24 ? 128 : 0);
            //            if (lnJ == 1)
            //                romData[0x290e + (lnI * 5) + lnJ] = (byte)(newBase >= 8 ? 128 : 0);
            //            if (lnJ == 2)
            //                romData[0x290e + (lnI * 5) + lnJ] = (byte)(newBase % 8 >= 4 ? 128 : 0);
            //            if (lnJ == 3)
            //                romData[0x290e + (lnI * 5) + lnJ] = (byte)(newBase % 4 >= 2 ? 128 : 0);
            //            if (lnJ == 4)
            //                romData[0x290e + (lnI * 5) + lnJ] = (byte)(newBase % 2 == 1 ? 255 : 127);

            //            if (lnJ <= 3)
            //            {
            //                int lvlsToNext = 5 + (r1.Next() % 3); // (r1.Next() % (50 - charLevel));
            //                                                      //if (lvlsToNext < 2 && charLevel == 0)
            //                                                      //    lvlsToNext = 2;

            //                romData[0x290e + (lnI * 5) + lnJ] += (byte)(lvlsToNext);
            //                //charLevel += lvlsToNext;
            //            }
            //        }
            //    }
            //}
        }

        private List<int> addTreasure(List<int> currentList, int[] treasureData)
        {
            for (int lnI = 0; lnI < treasureData.Length; lnI++)
                currentList.Add(treasureData[lnI]);
            return currentList;
        }

        private void shuffle(int[] treasureData, Random r1, bool keyItemAvoidance = false)
        {
            // Do not exceed these zones defined for the key items, or you're going to be stuck!
            int[] keyZoneMax = { 13, 13, 23, 40, 45, 53 }; // Cloak of wind, Mirror Of Ra, Golden Key, Jailor's Key, Moon Fragment, Eye Of Malroth
            List<byte> keyItems = new List<byte> { 0x2b, 0x2e, 0x37, 0x39, 0x26, 0x28 }; // When we reach insane randomness, we'll want to know what the key items are so we place them in the appropriate zones...

            // Shuffle each zone 15 times the length of the array for randomness.
            for (int lnI = 0; lnI < 15 * treasureData.Length; lnI++)
            {
                int swap1 = r1.Next() % treasureData.Length;
                int swap2 = r1.Next() % treasureData.Length;

                // Don't shuffle if key items would be swapped into inaccessible areas.
                if (keyItemAvoidance)
                {
                    int position1 = keyItems.IndexOf(romData[treasureData[swap1]]);
                    int position2 = keyItems.IndexOf(romData[treasureData[swap2]]);
                    if (position1 > -1 && swap2 > keyZoneMax[position1])
                        continue;
                    if (position2 > -1 && swap1 > keyZoneMax[position2])
                        continue;
                }

                swap(treasureData[swap1], treasureData[swap2]);
            }
        }

        private void swap(int firstAddress, int secondAddress)
        {
            byte holdAddress = romData[secondAddress];
            romData[secondAddress] = romData[firstAddress];
            romData[firstAddress] = holdAddress;
        }

        private int[] swapArray(int[] array, int first, int second)
        {
            int holdAddress = array[second];
            array[second] = array[first];
            array[first] = holdAddress;
            return array;
        }

        // Reserve for another time...
        private void button1_Click(object sender, EventArgs e)
        {
            if (!loadRom()) return;
            halfExpAndGoldReq(true);
            for (int lnI = 0; lnI < 68; lnI++)
            {
                int byteToUse = 0x10519 + (lnI * 6);
                byte valToUpdate = (byte)(129 + lnI);
                romData[byteToUse + 0] = valToUpdate;
                romData[byteToUse + 1] = valToUpdate;
                romData[byteToUse + 2] = valToUpdate;
                romData[byteToUse + 3] = valToUpdate;
                romData[byteToUse + 4] = valToUpdate;
                romData[byteToUse + 5] = valToUpdate;
            }
            saveRom();
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (!loadRom(true)) return;
            using (StreamWriter writer = File.CreateText(Path.Combine(Path.GetDirectoryName(txtFileName.Text), "DW3Compare.txt")))
            {
                for (int lnI = 0; lnI < 0x8a; lnI++)
                    compareComposeString("monsters" + lnI.ToString("X2"), writer, (0x32e3 + (23 * lnI)), 23);

                compareComposeString("itemPrice1", writer, 0x11be, 128);
                compareComposeString("itemPrice2", writer, 0x123b, 128);
                compareComposeString("weaponEffects", writer, 0x13280, 40);

                compareComposeString("treasures-Promontory", writer, 0x29237, 3);
                compareComposeString("treasures-NajimiBasement", writer, 0x2927B, 3);
                compareComposeString("treasures-Najimi", writer, 0x292C4, 3);
                compareComposeString("treasures-Thief'sKey", writer, 0x37DF1, 1);
                compareComposeString("treasures-MagicBall", writer, 0x375AA, 1);
                compareComposeString("treasures-Invitation", writer, 0x2927E, 2);
                compareComposeString("treasures-Kanave", writer, 0x29234, 2);
                compareComposeString("treasures-Champange1", writer, 0x29252, 1);
                compareComposeString("treasures-Champange2", writer, 0x292D2, 1);
                compareComposeString("treasures-Champange3", writer, 0x292E6, 1);
                compareComposeString("treasures-Isis", writer, 0x2925C, 9);
                compareComposeString("treasures-IsisWizards", writer, 0x31B9C, 1);
                compareComposeString("treasures-GoldenClaw", writer, 0x317F4, 1);
                compareComposeString("treasures-Pyramid1st", writer, 0x29249, 7);
                compareComposeString("treasures-Pyramid3rd4th5th", writer, 0x292B4, 15);
                compareComposeString("treasures-DreamCave1", writer, 0x2923A, 2);
                compareComposeString("treasures-DreamCave2", writer, 0x29280, 8);
                compareComposeString("treasures-WakeUpNPC", writer, 0x37786, 1);
                compareComposeString("treasures-Aliahan", writer, 0x29255, 5);
                compareComposeString("treasures-Portuga", writer, 0x29269, 3);
                compareComposeString("treasures-RoyalScroll", writer, 0x37CB9, 1);
                compareComposeString("treasures-Dwarf", writer, 0x2923C, 2);
                compareComposeString("treasures-Kidnappers1", writer, 0x2923E, 6);
                compareComposeString("treasures-Kidnappers2", writer, 0x2928B, 4);
                compareComposeString("treasures-BlackPepperNPC", writer, 0x377D5, 1);
                compareComposeString("treasures-Tedan1", writer, 0x31B94, 1);
                compareComposeString("treasures-Tedan2", writer, 0x29270, 1);
                compareComposeString("treasures-TedanGreenOrb", writer, 0x37828, 1);
                compareComposeString("treasures-Garuna1", writer, 0x29251, 1);
                compareComposeString("treasures-Garuna2", writer, 0x292C7, 4);
                compareComposeString("treasures-NohMask", writer, 0x292E4, 1);
                compareComposeString("treasures-PurpleOrb", writer, 0x292E7, 1);
                compareComposeString("treasures-WaterBlaster", writer, 0x377FE, 1);
                compareComposeString("treasures-PirateCove", writer, 0x29271, 3);
                compareComposeString("treasures-Eginbear", writer, 0x2925B, 1);
                compareComposeString("treasures-FinalKey", writer, 0x2922B, 1);
                compareComposeString("treasures-ArpTower", writer, 0x292CB, 7);
                compareComposeString("treasures-Soo", writer, 0x31B8C, 1);
                compareComposeString("treasures-SamanaoCave", writer, 0x29291, 23);
                compareComposeString("treasures-SamanaoCastle", writer, 0x292E5, 1);
                compareComposeString("treasures-LancelCave1", writer, 0x29244, 5);
                compareComposeString("treasures-LancelCave2", writer, 0x2928F, 2);
                compareComposeString("treasures-Luzami", writer, 0x31B97, 1);
                compareComposeString("treasures-NewTown1", writer, 0x2926C, 2);
                compareComposeString("treasures-NewTownYellowOrb", writer, 0x31B80, 1);
                compareComposeString("treasures-Sailor'sThighNPC", writer, 0x378A9, 1);
                compareComposeString("treasures-GhostShip", writer, 0x29275, 6);
                compareComposeString("treasures-SwordOfGaia", writer, 0x31B84, 1);
                compareComposeString("treasures-Negrogund", writer, 0x29288, 3);
                compareComposeString("treasures-SilverOrb", writer, 0x37907, 1);
                compareComposeString("treasures-LeafOfWorld", writer, 0x31B9F, 1);
                compareComposeString("treasures-SphereOfLight", writer, 0x37929, 1);
                compareComposeString("treasures-Baramos", writer, 0x29228, 3);
                compareComposeString("treasures-SwordOfIllusion", writer, 0x37a25, 1);
                compareComposeString("treasures-Tantegel", writer, 0x29265, 4);
                compareComposeString("treasures-Erdrick's", writer, 0x292A8, 5);
                compareComposeString("treasures-SilverHarp", writer, 0x29274, 1);
                compareComposeString("treasures-MountainCave", writer, 0x292DF, 5);
                compareComposeString("treasures-Oricon", writer, 0x31B90, 1);
                compareComposeString("treasures-FairyFlute", writer, 0x31B88, 1);
                compareComposeString("treasures-KolTower1", writer, 0x29253, 2);
                compareComposeString("treasures-KolTower2", writer, 0x292D5, 10);
                compareComposeString("treasures-SacredAmulet", writer, 0x37D5A, 1);
                compareComposeString("treasures-StaffOfRain", writer, 0x37D9D, 1);
                compareComposeString("treasures-RainbowDrop", writer, 0x37D80, 1);
                compareComposeString("treasures-Rimuldar", writer, 0x29233, 1);
                compareComposeString("treasures-ZomaCastle", writer, 0x292AD, 7);

                compareComposeString("stores", writer, 0x36838, 248, 1, "g128");

                for (int lnI = 0; lnI < 100; lnI++)
                    compareComposeString("monsterZones" + lnI.ToString("X2"), writer, (0x61000 + (16 * lnI)), 16);
                //for (int lnI = 0; lnI < 20; lnI++)
                //    compareComposeString("monsterSpecial" + lnI.ToString("X2"), writer, (0x107a + (6 * lnI)), 6);
                //for (int lnI = 0; lnI < 13; lnI++)
                //    compareComposeString("monsterBoss" + lnI.ToString("X2"), writer, (0x10356 + (4 * lnI)), 4);
                //compareComposeString("statStart", writer, 0x13dd1, 12);
                //compareComposeString("statMult", writer, 0x281b, 10);
                //compareComposeString("statUpsStrength", writer, 0x290e + 0, 40);
                //compareComposeString("statUpsAgility", writer, 0x290e + 40, 40);
                //compareComposeString("statUpsVitality", writer, 0x290e + 80, 40);
                //compareComposeString("statUpsLuck", writer, 0x290e + 120, 40);
                //compareComposeString("statUpsIntelligence", writer, 0x290e + 160, 40);

                //compareComposeString("spellLearningHero", writer, 0x29d6, 63);
                //compareComposeString("spellsLearnedHero", writer, 0x22E7, 32);
                //compareComposeString("spellLearningPilgrim", writer, 0x2A15, 63);
                //compareComposeString("spellsLearnedPilgrim", writer, 0x2307, 32);
                //compareComposeString("spellLearningWizard", writer, 0x2A54, 63);
                //compareComposeString("spellsLearnedWizard", writer, 0x2327, 32);
                //compareComposeString("spellLearningSage", writer, 0x2A93, 63);
                //for (int lnI = 0; lnI < 28; lnI++)
                //    compareComposeString("spellStats" + (lnI).ToString(), writer, 0x127d5 + (5 * lnI), 5);
                //compareComposeString("spellCmd", writer, 0x13528, 28);
                //compareComposeString("spellFieldHeal", writer, 0x18be0, 16, 8);
                //compareComposeString("spellFieldMedical", writer, 0x19602, 1);

                //compareComposeString("start1", writer, 0x3c79f, 8);
                //compareComposeString("start2", writer, 0x3c79f + 8, 8);
                //compareComposeString("start3", writer, 0x3c79f + 16, 8);
                //compareComposeString("weapons", writer, 0x13efb, 16);
                //compareComposeString("weaponcost (2.3)", writer, 0x1a00e, 32);
                //compareComposeString("armor", writer, 0x13efb + 16, 11);
                //compareComposeString("armorcost (2.4)", writer, 0x1a00e + 32, 22);
                //compareComposeString("shields", writer, 0x13efb + 27, 5);
                //compareComposeString("shieldcost (2.8)", writer, 0x1a00e + 54, 10);
                //compareComposeString("helmets", writer, 0x13efb + 32, 3);
                //compareComposeString("helmetcost (3.0)", writer, 0x1a00e + 64, 6);

            }
            lblIntensityDesc.Text = "Comparison complete!  (DW3Compare.txt)";
        }

        private StreamWriter compareComposeString(string intro, StreamWriter writer, int startAddress, int length, int skip = 1, string delimiter = "")
        {
            if (delimiter == "")
            {
                writer.WriteLine(intro);
                string final = "";
                string final2 = "";
                for (int lnI = 0; lnI < length; lnI += skip)
                {
                    final += romData[startAddress + lnI].ToString("X2") + " ";
                    if (lnI % 16 == 15)
                    {
                        writer.WriteLine(final);
                        final = "";
                    }
                }
                writer.WriteLine(final);
                if (length >= 16) writer.WriteLine();
                for (int lnI = 0; lnI < length; lnI += skip)
                {
                    final2 += romData2[startAddress + lnI].ToString("X2") + " ";
                    if (lnI % 16 == 15)
                    {
                        writer.WriteLine(final2);
                        final2 = "";
                    }
                }
                writer.WriteLine(final2);
                writer.WriteLine();
            }
            else
            {
                writer.WriteLine(intro);

                string final = "";
                for (int lnI = 0; lnI < length; lnI += skip)
                {
                    final += romData[startAddress + lnI].ToString("X2") + " ";
                    if (delimiter == "g128" && romData[startAddress + lnI] >= 128)
                    {
                        writer.WriteLine(final);
                        final = "";
                    }
                }
                writer.WriteLine(final);
                writer.WriteLine("---------------- VS -------------");
                final = "";
                for (int lnI = 0; lnI < length; lnI += skip)
                {
                    final += romData2[startAddress + lnI].ToString("X2") + " ";
                    if (delimiter == "g128" && romData2[startAddress + lnI] >= 128)
                    {
                        writer.WriteLine(final);
                        final = "";
                    }
                }
                writer.WriteLine(final);
                writer.WriteLine("");
            }
            return writer;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (txtFileName.Text != "")
                using (StreamWriter writer = File.CreateText("lastFile4.txt"))
                {
                    writer.WriteLine(txtFileName.Text);
                    writer.WriteLine(txtCompare.Text);
                    writer.WriteLine(txtC1Name1.Text);
                    writer.WriteLine(txtC2Name1.Text);
                    writer.WriteLine(txtC2Name2.Text);
                    writer.WriteLine(txtC2Name3.Text);
                    writer.WriteLine(txtC3Name1.Text);
                    writer.WriteLine(txtC4Name1.Text);
                    writer.WriteLine(txtC4Name2.Text);
                    writer.WriteLine(chkSoloHero.Checked ? "T" : "F");
                    writer.WriteLine(cboSoloHero.SelectedItem);
                    writer.WriteLine(chkSoloCanEquipAll.Checked ? "T" : "F");
                    writer.WriteLine(chkC14Random.Checked ? "T" : "F");
                    writer.WriteLine(chkC5Random.Checked ? "T" : "F");
                    writer.WriteLine(c1Hero.SelectedItem);
                    writer.WriteLine(c2Hero1.SelectedItem);
                    writer.WriteLine(c2Hero2.SelectedItem);
                    writer.WriteLine(c2Hero3.SelectedItem);
                    writer.WriteLine(chkCh2AwardXPTournament.Checked ? "T" : "F");
                    writer.WriteLine(c3Hero.SelectedItem);
                    writer.WriteLine(chkShop1.Checked ? "T" : "F");
                    writer.WriteLine(chkShop25K.Checked ? "T" : "F");
                    writer.WriteLine(chkTunnel1.Checked ? "T" : "F");
                    writer.WriteLine(c4Hero1.SelectedItem);
                    writer.WriteLine(c4Hero2.SelectedItem);
                    writer.WriteLine(c5Hero1.SelectedItem);
                    writer.WriteLine(c5Hero2.SelectedItem);
                    writer.WriteLine(c5Hero3.SelectedItem);
                    writer.WriteLine(c5Hero4.SelectedItem);
                    writer.WriteLine(c5Hero5.SelectedItem);
                    writer.WriteLine(c5Hero6.SelectedItem);
                    writer.WriteLine(c5Hero7.SelectedItem);
                    writer.WriteLine(c5Hero8.SelectedItem);
                    writer.WriteLine(cboXPAdjustment.SelectedItem);
                    writer.WriteLine(chkXPRandom.Checked ? "T" : "F");
                    writer.WriteLine(cboGoldAdjustment.SelectedItem);
                    writer.WriteLine(chkGoldRandom.Checked ? "T" : "F");
                    writer.WriteLine(cboEncounterRate.SelectedItem);
                    writer.WriteLine(chkRandomMonsters.Checked ? "T" : "F");
                    writer.WriteLine(txtSeed.Text);
                    writer.WriteLine(chkSpeedUpBattles.Checked ? "T" : "F");
                }
        }

        private void txtFileName_Leave(object sender, EventArgs e)
        {
            runChecksum();
        }

        private void btnCompareBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtCompare.Text = openFileDialog1.FileName;
            }
        }

        private void textOutput()
        {
            loadRom(false);
            using (StreamWriter writer = File.CreateText(Path.Combine(Path.GetDirectoryName(txtFileName.Text), "DW3TextOutput.txt")))
            {
                for (int lnI = 0; lnI < 96; lnI++)
                    outputComposeString("monstersZones" + (lnI).ToString("X2"), writer, (0xaeb + (15 * lnI)), 15);

                for (int lnI = 0; lnI < 19; lnI++)
                    outputComposeString("monstersZoneSpecial" + (lnI + 1).ToString("X2"), writer, (0x108b + (6 * lnI)), 6);

                for (int lnI = 0; lnI < 140; lnI++)
                    outputComposeString("monsters" + (lnI).ToString("X2"), writer, (0x32e3 + (23 * lnI)), 23);

                for (int lnI = 0; lnI < 21; lnI++)
                    outputComposeString("bosses" + (lnI).ToString("X2"), writer, (0x8ee + (2 * lnI)), 2, 1, 43);
            }
            lblIntensityDesc.Text = "Text output complete!  (DW3TextOutput.txt)";
        }

        private StreamWriter outputComposeString(string intro, StreamWriter writer, int startAddress, int length, int skip = 1, int duplicate = 0)
        {
            string final = "";
            for (int lnI = 0; lnI < length; lnI += skip)
            {
                final += romData[startAddress + lnI].ToString("X2") + " ";
            }
            if (duplicate != 0)
            {
                for (int lnI = duplicate; lnI < length + duplicate; lnI += skip)
                {
                    final += romData[startAddress + lnI].ToString("X2") + " ";
                }
            }
            writer.WriteLine(intro);
            writer.WriteLine(final);
            writer.WriteLine();
            return writer;
        }

        private void btnMonsterOutput_Click(object sender, EventArgs e)
        {
            loadRom(false);
            using (StreamWriter writer = File.CreateText(Path.Combine(Path.GetDirectoryName(txtFileName.Text), "DW4MonsterOutput.txt")))
            {
                for (int lnI = 0; lnI < 214; lnI++)
                    outputComposeString("monsters" + (lnI).ToString("X2"), writer, (0x60054 + (22 * lnI)), 22);

                for (int lnI = 0; lnI < 101; lnI++)
                    outputComposeString("monstersZones" + (lnI).ToString("X2"), writer, (0x612ba + (16 * lnI)), 16);

                for (int lnI = 0; lnI < 34; lnI++)
                    outputComposeString("bossFights" + (lnI).ToString("X2"), writer, (0x6235c + (8 * lnI)), 8);
            }
            lblIntensityDesc.Text = "Text output complete!  (DW4MonsterOutput.txt)";
        }

        private void chkSoloHero_CheckedChanged(object sender, EventArgs e)
        {
            cboSoloHero.Enabled = chkSoloHero.Checked;
            chkSoloCanEquipAll.Enabled = chkSoloHero.Checked;
            c1Hero.Enabled = !chkSoloHero.Checked;
            c2Hero1.Enabled = !chkSoloHero.Checked;
            c2Hero2.Enabled = !chkSoloHero.Checked;
            c2Hero3.Enabled = !chkSoloHero.Checked;
            c3Hero.Enabled = !chkSoloHero.Checked;
            c4Hero1.Enabled = !chkSoloHero.Checked;
            c4Hero2.Enabled = !chkSoloHero.Checked;
            c5Hero1.Enabled = !chkSoloHero.Checked;
            c5Hero2.Enabled = !chkSoloHero.Checked;
            c5Hero3.Enabled = !chkSoloHero.Checked;
            c5Hero4.Enabled = !chkSoloHero.Checked;
            c5Hero5.Enabled = !chkSoloHero.Checked;
            c5Hero6.Enabled = !chkSoloHero.Checked;
            c5Hero7.Enabled = !chkSoloHero.Checked;
            c5Hero8.Enabled = !chkSoloHero.Checked;
            chkC14Random.Enabled = !chkSoloHero.Checked;
            chkC5Random.Enabled = !chkSoloHero.Checked;
        }

        private void chkShop1_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShop1.Checked) chkShop25K.Checked = false;
        }

        private void chkShop25K_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShop25K.Checked) chkShop1.Checked = false;
        }
    }
}
