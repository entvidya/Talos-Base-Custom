using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Talos.Base;
using Talos.Capricorn.Drawing;
using Talos.Capricorn.IO;
using Talos.Forms.UI;
using Talos.Helper;
using Talos.Properties;

namespace Talos.Forms
{
    internal partial class EnemyPage : UserControl
    {
        internal Client Client { get; set; }
        internal Enemy Enemy { get; set; }

        internal EnemyPage(Enemy enemy, Client client)
        {

            Enemy = enemy;
            Client = client;
            UIHelper.Initialize(Client);
            InitializeComponent();

            LoadEnemySprite(enemy);
            ConfigureTargetComboBox(enemy);
            OnlyDisplaySpellsWeHave();
        }

        private void OnlyDisplaySpellsWeHave()
        {

            Dictionary<string, string> singleTarget = new Dictionary<string, string>
            {
                { "Hail of Feathers", "Hail of Feathers" },
                { "Frost Arrow", "Frost Arrow" },
				{ "Shock Arrow", "Shock Arrow" },
				{ "Chadul's Shot", "Chadul's Shot" },
                { "Supernova Shot", "Supernova Shot" },
				{ "Hypernova Shot", "Hypernova Shot" },
                { "Keeter", "Keeter" },
                { "Groo", "Groo" },
                { "Torch", "Torch" },
                { "Mermaid", "Mermaid" },
                { "ard pian na dion", "A/M/PND" },
                { "mor pian na dion", "A/M/PND" },
                { "pian na dion", "A/M/PND" },
				{ "Unholy Explosion", "Unholy Explosion" },
                { "deo searg", "A/DS" },
                { "ard deo searg", "A/DS" },
                { "Deception of Life", "Deception of Life" },
                { "Dragon Blast", "Dragon Blast" },
                { "lamh", "lamh" }
            };
            Dictionary<string, string> multiTarget = new Dictionary<string, string> 
            { 
                { "mor strioch pian gar", "MSPG" },
                { "Cursed Tune", "Cursed Tune" },
                { "Volley", "Volley" },
                { "Barrage", "Barrage" },
                { "Star Arrow", "Star Arrow" },
                { "deo searg gar", "M/DSG" },
                { "mor deo searg gar", "M/DSG" }
            };

            UIHelper.SetupComboBox(spellsCurseCombox, new[] { "Demon Seal", "Demise", "Darker Seal", "Dark Seal", "ard cradh", "mor cradh", "cradh", "beag cradh" }, spellsCurseCbox);
            UIHelper.SetupComboBox(spellsFasCombox, new[] { "ard fas nadur", "mor fas nadur", "fas nadur", "beag fas nadur" }, spellsFasCbox);
            UIHelper.SetupComboBox(spellsControlCombox, new[] { "Mesmerize", "pramh", "beag pramh", "suain" }, spellsControlCbox);
            UIHelper.SetupComboBox(attackComboxOne, new[] { "Hail of Feathers", "Frost Arrow", "Shock Arrow", "Chadul's Shot", "Supernova Shot", "Hypernova Shot", "Keeter", "Groo", "Torch", "Mermaid", "ard pian na dion", "mor pian na dion", "pian na dion", "Unholy Explosion", "ard deo searg", "deo searg", "Deception of Life", "Dragon Blast", "lamh" }, attackCboxOne, null, null, null, partialMatch: true, abbreviations: singleTarget);
            UIHelper.SetupComboBox(attackComboxTwo, new[] { "mor strioch pian gar", "Cursed Tune", "Volley", "Barrage", "Star Arrow", "mor deo searg gar", "deo searg gar"}, attackCboxTwo, null, null, null, partialMatch: true, abbreviations: multiTarget);
            UIHelper.SetupCheckbox(mpndSilenced, "ard pian na dion", "mor pian na dion", "pian na dion");
            UIHelper.SetupCheckbox(mpndDioned, "ard pian na dion", "mor pian na dion", "pian na dion");
            UIHelper.SetupCheckbox(mspgSilenced, "mor strioch pian gar");
            UIHelper.SetupCheckbox(mspgPct, "mor strioch pian gar");

        }



        private void LoadEnemySprite(Enemy enemy)
        {
            Name = enemy.SpriteID.ToString();
            string spriteFileName = $"MNS{enemy.SpriteID:D3}.MPF";
            string archivePath = Settings.Default.DarkAgesPath.Replace("Darkages.exe", "hades.dat");

            DATArchive archive = null;
            try
            {
                archive = DATArchive.FromFile(archivePath);
                if (archive.Contains(spriteFileName))
                {
                    var spriteImage = LoadSpriteFromArchive(spriteFileName, archive);
                    if (spriteImage != null)
                    {
                        ConfigureEnemyPicture(spriteImage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in InitializeEnemySprite: {ex.Message}");
            }
            finally
            {
                // DATArchive doesn't have a dispose?
            }
        }

        private Bitmap LoadSpriteFromArchive(string spriteFileName, DATArchive archive)
        {
            MPFImage sprite = MPFImage.FromArchive(spriteFileName, archive);
            int frameIndex = CalculateFrameIndex(sprite);

            Palette256 palette = Palette256.FromArchive(sprite.palette, archive);
            Bitmap renderedImage = DAGraphics.RenderImage(sprite[frameIndex], palette);  // Ensure RenderImage returns a Bitmap
            
            return renderedImage;
        }

        private int CalculateFrameIndex(MPFImage sprite)
        {
            int frameIndex = (sprite.idleLength == 0 || sprite.walkStart == sprite.idleStart) ? sprite.walkStart + sprite.walkLength : sprite.walkStart - sprite.idleLength;
            frameIndex = Math.Max(frameIndex, 0); // Ensure frameIndex is not negative
            frameIndex = Math.Min(frameIndex, sprite.expectedFrames - 1); // Ensure frameIndex does not exceed bounds
            return frameIndex;
        }

        private void ConfigureEnemyPicture(Bitmap spriteImage)
        {
            enemyPicture.SizeMode = (spriteImage.Width > 100 || spriteImage.Height > 100) ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
            enemyPicture.Image = spriteImage;  // Assign the Bitmap directly to the PictureBox

            //flip the image
            spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
            enemyPicture.Image = spriteImage;
        }

        private void ConfigureTargetComboBox(Enemy enemy)
        {
            if (!enemy.IsAllMonsters)
            {
                targetCombox.Items.Remove("Cluster 29");
                targetCombox.Items.Remove("Cluster 13");
                targetCombox.Items.Remove("Cluster 5");
            }
        }

        internal void enemyRemoveBtn_Click(object sender, EventArgs e)
        {
            foreach (Enemy enemy in new List<Enemy>(Client.Bot.ReturnEnemyList()))
            {
                if (enemy.EnemyPage == this)
                {
                    Client.Bot.ClearEnemyLists(enemy.SpriteID.ToString());
                }
            }
            if (Enemy.ToString() == "all monsters")
            {
                Client.Bot.AllMonsters = null;

                // Ensure thread access for monsterTabControl
                if (Client.ClientTab.monsterTabControl.InvokeRequired)
                {
                    Client.ClientTab.monsterTabControl.Invoke(new Action(() =>
                    {
                        Client.ClientTab.monsterTabControl.TabPages.Add(Client.ClientTab.nearbyEnemyTab);
                    }));
                }
                else
                {
                    Client.ClientTab.monsterTabControl.TabPages.Add(Client.ClientTab.nearbyEnemyTab);
                }
            }

            // Dispose the parent safely
            if (Parent != null)
            {
                if (Parent.InvokeRequired)
                {
                    Parent.Invoke(new Action(() => Parent.Dispose()));
                }
                else
                {
                    Parent.Dispose();
                }
            }

            Client.RefreshRequest(false);
        }

        private void priorityAddBtn_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(priorityTbox.Text, out ushort result) && result >= 1 && result <= 1000)
            {
                if (priorityLbox.Items.Contains(result.ToString()))
                {
                    MessageDialog.Show(Client.Server.MainForm, "\tEnemy already in list\t");
                    priorityTbox.Clear();
                }
                else
                {
                    priorityLbox.Items.Add(result.ToString());
                    priorityTbox.Clear();
                }
            }
            else
            {
                MessageDialog.Show(Client.Server.MainForm, "Your sprite must be a number between 1-1000");
                priorityTbox.Clear();
            }
        }

        private void priorityRemoveBtn_Click(object sender, EventArgs e)
        {
            while (priorityLbox.SelectedItems.Count > 0)
            {
                priorityLbox.Items.RemoveAt(priorityLbox.SelectedIndex);
            }
        }

        private void ignoreAddBtn_Click(object sender, EventArgs e)
        {
            if (ushort.TryParse(ignoreTbox.Text, out ushort result) && result >= 1 && result <= 1000)
            {
                if (priorityLbox.Items.Contains(result.ToString()))
                {
                    MessageDialog.Show(Client.Server.MainForm, "\tEnemy already in list\t");
                    ignoreTbox.Clear();
                }
                else
                {
                    ignoreLbox.Items.Add(result.ToString());
                    ignoreTbox.Clear();
                }
            }
            else
            {
                MessageDialog.Show(Client.Server.MainForm, "Your sprite must be a number between 1-1000");
                ignoreTbox.Clear();
            }
        }

        private void ignoreRemoveBtn_Click(object sender, EventArgs e)
        {
            		while (ignoreLbox.SelectedItems.Count > 0)
		{
			ignoreLbox.Items.RemoveAt(ignoreLbox.SelectedIndex);
		}
        }

        private bool isHandlingCheck = false;
        private void NearestFirstCbx_CheckedChanged(object sender, EventArgs e)
        {
            if (isHandlingCheck)
                return;

            if (NearestFirstCbx.Checked)
            {
                try
                {
                    isHandlingCheck = true;
                    FarthestFirstCbx.Checked = false;
                }
                finally
                {
                    isHandlingCheck = false;
                }
            }
        }

        private void FarthestFirstCbx_CheckedChanged(object sender, EventArgs e)
        {
            if (isHandlingCheck)
                return;

            if (FarthestFirstCbx.Checked)
            {
                try
                {
                    isHandlingCheck = true;
                    NearestFirstCbx.Checked = false;
                }
                finally
                {
                    isHandlingCheck = false;
                }
            }
        }
    }
}
