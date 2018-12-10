﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirstPlugin
{
    public partial class GTXTextureImporter : Form
    {
        public GTXTextureImporter()
        {
            InitializeComponent();


            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TCS_R8_G8_B8_A8_SRGB);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TCS_R10_G10_B10_A2_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TCS_R5_G6_B5_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TC_R5_G5_B5_A1_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TC_R4_G4_B4_A4_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TC_R8_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.TC_R8_G8_UNORM);

            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC1_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC1_SRGB);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC2_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC2_SRGB);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC3_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC3_SRGB);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC4_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC4_SNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC5_UNORM);
            formatComboBox.Items.Add(GTX.GX2SurfaceFormat.T_BC5_SNORM);

            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_DEFAULT);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_LINEAR_ALIGNED);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_1D_TILED_THIN1);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_1D_TILED_THICK);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2D_TILED_THIN1);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2D_TILED_THIN2);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2D_TILED_THIN4);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2D_TILED_THICK);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2B_TILED_THIN1);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2B_TILED_THIN2);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2B_TILED_THIN4);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_2B_TILED_THICK);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_3D_TILED_THIN1);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_3D_TILED_THICK);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_3B_TILED_THIN1);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_3B_TILED_THICK);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_LINEAR_SPECIAL);
            tileModeCB.Items.Add(GTX.GX2TileMode.MODE_DEFAULT_FIX2197);

            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_1D);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_1D_ARRAY);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_2D);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_2D_ARRAY);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_2D_MSAA);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_2D_MSAA_ARRAY);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_3D);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_CUBE);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_FIRST);
            ImgDimComb.Items.Add(GTX.GX2SurfaceDimension.DIM_LAST);

            ImgDimComb.SelectedItem = GTX.GX2SurfaceDimension.DIM_2D;
            tileModeCB.SelectedItem = GTX.GX2TileMode.MODE_2D_TILED_THIN1;
            formatComboBox.SelectedItem = GTX.GX2SurfaceFormat.T_BC1_SRGB;
        }
        GTXImporterSettings SelectedTexSettings;

        List<GTXImporterSettings> settings = new List<GTXImporterSettings>();
        public void LoadSettings(List<GTXImporterSettings> s)
        {
            settings = s;

            foreach (var setting in settings)
            {
                listViewCustom1.Items.Add(setting.TexName).SubItems.Add(setting.Format.ToString());
            }
            listViewCustom1.Items[0].Selected = true;
            listViewCustom1.Select();
        }
        public void LoadSetting(GTXImporterSettings setting)
        {
            settings = new List<GTXImporterSettings>();
            settings.Add(setting);

            listViewCustom1.Items.Add(setting.TexName).SubItems.Add(setting.Format.ToString());
            listViewCustom1.Items[0].Selected = true;
            listViewCustom1.Select();
        }

        private Thread Thread;
        public void SetupSettings()
        {
            if (SelectedTexSettings.Format == GTX.GX2SurfaceFormat.INVALID)
                return;


            if (Thread != null && Thread.IsAlive)
                Thread.Abort();

            if (formatComboBox.SelectedItem is GTX.GX2SurfaceFormat)
            {
                SelectedTexSettings.Format = (GTX.GX2SurfaceFormat)formatComboBox.SelectedItem;
                listViewCustom1.SelectedItems[0].SubItems[1].Text = SelectedTexSettings.Format.ToString();
            }
            HeightLabel.Text = $"Height: {SelectedTexSettings.TexHeight}";
            WidthLabel.Text = $"Width: {SelectedTexSettings.TexWidth}";

            Bitmap bitmap = Switch_Toolbox.Library.Imaging.GetLoadingImage();

            Thread = new Thread((ThreadStart)(() =>
            {
                pictureBox1.Image = bitmap;
                SelectedTexSettings.Compress();

                bitmap = FTEX.DecodeBlock(SelectedTexSettings.DataBlockOutput[0], SelectedTexSettings.
                TexWidth, SelectedTexSettings.TexHeight, (Syroot.NintenTools.Bfres.GX2.GX2SurfaceFormat)SelectedTexSettings.Format);

                pictureBox1.Image = bitmap;

            }));
            Thread.Start();
        }

        private void tileModeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tileModeCB.SelectedIndex > -1 && SelectedTexSettings != null)
                SelectedTexSettings.tileMode = (uint)tileModeCB.SelectedItem;
        }

        private void formatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (formatComboBox.SelectedIndex > -1 && SelectedTexSettings != null)
            {
                SetupSettings();
            }
        }

        private void listViewCustom1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCustom1.SelectedItems.Count > 0)
            {
                SelectedTexSettings = settings[listViewCustom1.SelectedIndices[0]];
                formatComboBox.SelectedItem = SelectedTexSettings.Format;

                SetupSettings();

                MipmapNum.Maximum = SelectedTexSettings.GetTotalMipCount();
                MipmapNum.Value = SelectedTexSettings.MipCount;
            }
        }
    }
}
