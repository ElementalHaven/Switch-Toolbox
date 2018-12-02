﻿using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System.Runtime.InteropServices;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.IO;

namespace FirstPlugin
{
    public class TextureImporterSettings 
    {
        public TextureImporterSettings()
        {
        }

        public string TexName;
        public uint arrayLength = 1;
        public uint AccessFlags = 0x20;
        public uint MipCount;
        public uint Depth = 1;
        public uint TexWidth;
        public uint TexHeight;
        public uint Flags;
        public uint Swizzle;
        public uint SampleCount = 1;
        public uint Pitch = 32;
        public uint[] Regs;
        public SurfaceFormat Format;
        public SurfaceDim SurfaceDim = SurfaceDim.Dim2D;
        public TileMode TileMode = TileMode.Default;
        public Dim Dim = Dim.Dim2D;
        public ChannelType RedComp = ChannelType.Red;
        public ChannelType GreenComp = ChannelType.Green;
        public ChannelType BlueComp = ChannelType.Blue;
        public ChannelType AlphaComp = ChannelType.Alpha;
        public TextureData textureData;
        public uint TextureLayout;
        public uint TextureLayout2 = 0x010007;
        public uint bpp;
        public List<byte[]> DataBlockOutput = new List<byte[]>();
        public List<byte[]> DecompressedData = new List<byte[]>();
        public BntxFile bntx;
        public int sparseResidency = 0; //false
        public int sparseBinding = 0; //false
        public bool IsSRGB = true;
        public bool GenerateMipmaps = false; //If bitmap and count more that 1 then geenrate

        private SurfaceFormat LoadDDSFormat(string fourCC, DDS dds = null, bool IsSRGB = false)
        {
            bool IsDX10 = false;

            switch (fourCC)
            {
                case "DXT1":
                    if (IsSRGB)
                        return SurfaceFormat.BC1_SRGB;
                    else
                        return SurfaceFormat.BC1_UNORM;
                case "DXT3":
                    if (IsSRGB)
                        return SurfaceFormat.BC2_SRGB;
                    else
                        return SurfaceFormat.BC2_UNORM;
                case "DXT5":
                    if (IsSRGB)
                        return SurfaceFormat.BC3_SRGB;
                    else
                        return SurfaceFormat.BC3_UNORM;
                case "BC4U":
                    return SurfaceFormat.BC4_UNORM;
                case "BC4S":
                    return SurfaceFormat.BC4_SNORM;
                case "ATI1":
                    return SurfaceFormat.BC4_UNORM;
                case "ATI2":
                    return SurfaceFormat.BC5_UNORM;
                case "BC5U":
                    return SurfaceFormat.BC5_UNORM;
                case "BC5S":
                    return SurfaceFormat.BC5_SNORM;
                case "DX10":
                    IsDX10 = true;
                    break;
                default:
                    return SurfaceFormat.R8_G8_B8_A8_SRGB;
            }
            if (IsDX10)
            {
                switch (dds.DX10header.DXGI_Format)
                {
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                        return SurfaceFormat.BC4_UNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                        return SurfaceFormat.BC4_SNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                        return SurfaceFormat.BC4_UNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                        return SurfaceFormat.BC5_UNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                        return SurfaceFormat.BC5_SNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                        return SurfaceFormat.BC5_UNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                        return SurfaceFormat.BC6_FLOAT;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                        return SurfaceFormat.BC6_UFLOAT;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                        return SurfaceFormat.BC7_UNORM;
                    case DDS.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                        return SurfaceFormat.BC7_SRGB;
                    default:
                        throw new Exception($"Format {dds.DX10header.DXGI_Format} not supported!");
                }
            }
            throw new Exception($"This shouldn't happen :(");
        }
        public void LoadDDS(string FileName, BntxFile bntxFile, byte[] FileData = null, TextureData tree = null)
        {
            TexName = Path.GetFileNameWithoutExtension(FileName);

            DDS dds = new DDS();

            if (FileData != null)
                dds.Load(new FileReader(new MemoryStream(FileData)));
            else
                dds.Load(new FileReader(FileName));
            MipCount = dds.header.mipmapCount;
            TexWidth = dds.header.width;
            TexHeight = dds.header.height;
            arrayLength = 1;
            if (dds.header.caps2 == (uint)DDS.DDSCAPS2.CUBEMAP_ALLFACES)
            {
                arrayLength = 6;
            }

            DataBlockOutput.Add(dds.bdata);


            Format = LoadDDSFormat(dds.header.ddspf.fourCC.ToString(), dds, IsSRGB);

            Texture tex = FromBitMap(DataBlockOutput[0], this);

            if (tree != null)
            {
                tree.LoadTexture(tex, 1);
            }
            else
            {
                textureData = new TextureData(tex, bntxFile);
            }
        }
        public void LoadBitMap(string FileName, BntxFile bntxFile)
        {
            DecompressedData.Clear();

            TexName = Path.GetFileNameWithoutExtension(FileName);
            bntx = bntxFile;
            Format = SurfaceFormat.BC1_SRGB;
            GenerateMipmaps = true;

            Bitmap Image = new Bitmap(FileName);
            Image = TextureData.SwapBlueRedChannels(Image);

            TexWidth = (uint)Image.Width;
            TexHeight = (uint)Image.Height;
            MipCount = (uint)GetTotalMipCount();

            DecompressedData.Add(BitmapExtension.ImageToByte(Image));

            Image.Dispose();
            if (DecompressedData.Count == 0)
            {
                throw new Exception("Failed to load " + Format);
            }
        }
        public int GetTotalMipCount()
        {
            int MipmapNum = 0;
            uint num = Math.Max(TexHeight, TexWidth);

            int width = (int)TexWidth;
            int height = (int)TexHeight;

            while (true)
            {
                num >>= 1;

                width = width / 2;
                height = height / 2;
                if (width <= 0 || height <= 0)
                    break;

                if (num > 0)
                    ++MipmapNum;
                else
                    break;
            }

            return MipmapNum;
        }
        public byte[] GenerateMips(int SurfaceLevel = 0)
        {
            Bitmap Image = BitmapExtension.GetBitmap(DecompressedData[SurfaceLevel], (int)TexWidth, (int)TexHeight);

            List<byte[]> mipmaps = new List<byte[]>();
            mipmaps.Add(TextureData.CompressBlock(DecompressedData[SurfaceLevel], (int)TexWidth, (int)TexHeight, Format));

            //while (Image.Width / 2 > 0 && Image.Height / 2 > 0)
            //      for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
            for (int mipLevel = 0; mipLevel < MipCount; mipLevel++)
            {
                Image = BitmapExtension.Resize(Image, Image.Width / 2, Image.Height / 2);
                mipmaps.Add(TextureData.CompressBlock(BitmapExtension.ImageToByte(Image), Image.Width, Image.Height, Format));
            }
            Image.Dispose();

            return Utils.CombineByteArray(mipmaps.ToArray());
        }
        public void Compress()
        {
            DataBlockOutput.Clear();
            foreach (var surface in DecompressedData)
            {
                DataBlockOutput.Add(TextureData.CompressBlock(surface, (int)TexWidth, (int)TexHeight, Format));
            }
        }
        public static uint DIV_ROUND_UP(uint value1, uint value2)
        {
            return TegraX1Swizzle.DIV_ROUND_UP(value1, value2);
        }
        public Texture FromBitMap(byte[] data, TextureImporterSettings settings)
        {
            Texture tex = new Texture();
            tex.Height = (uint)settings.TexHeight;
            tex.Width = (uint)settings.TexWidth;
            tex.Format = Format;
            tex.Name = settings.TexName;
            tex.Path = "";
            tex.TextureData = new List<List<byte[]>>();

            if (settings.MipCount == 0)
                settings.MipCount = 1;

            ChannelType[] channels = TextureData.SetChannelsByFormat(settings.Format);
            tex.ChannelRed = channels[0];
            tex.ChannelGreen = channels[1];
            tex.ChannelBlue = channels[2];
            tex.ChannelAlpha = channels[3];
            tex.sparseBinding = settings.sparseBinding;
            tex.sparseResidency = settings.sparseResidency;
            tex.AccessFlags = settings.AccessFlags;
            tex.ArrayLength = settings.arrayLength;
            tex.MipCount = settings.MipCount;
            tex.Depth = settings.Depth;
            tex.Dim = settings.Dim;
            tex.Flags = settings.Flags;
            tex.TileMode = settings.TileMode;
            tex.textureLayout = settings.TextureLayout;
            tex.textureLayout2 = settings.TextureLayout2;
            tex.Swizzle = settings.Swizzle;
            tex.SurfaceDim = settings.SurfaceDim;
            tex.SampleCount = settings.SampleCount;
            tex.Regs = settings.Regs;
            tex.Pitch = settings.Pitch;

            tex.MipOffsets = new long[tex.MipCount];

            List<byte[]> arrayFaces = new List<byte[]>();
            if (tex.ArrayLength > 1)
                arrayFaces = DDS.GetArrayFaces(data, tex.ArrayLength);
            else
                arrayFaces.Add(data);

            for (int i = 0; i < tex.ArrayLength; i++)
            {
                List<byte[]> mipmaps = SwizzleSurfaceMipMaps(tex, arrayFaces[i], tex.TileMode);
                tex.TextureData.Add(mipmaps);
                byte[] test = Combine(mipmaps);
                tex.TextureData[i][0] = test;
            }
            return tex;
        }
        public static List<byte[]> SwizzleSurfaceMipMaps(FTEX tex, byte[] data)
        {
            throw new Exception("Unimplemented");
        }
        public static List<byte[]> SwizzleSurfaceMipMaps(Texture tex,byte[] data, TileMode TileMode)
        {
            int blockHeightShift = 0;
            int target = 1;
            uint Pitch = 0;
            uint SurfaceSize = 0;
            uint blockHeight = 0;
            uint blk_dim = Formats.blk_dims((uint)((int)tex.Format >> 8));
            uint blkWidth = blk_dim >> 4;
            uint blkHeight = blk_dim & 0xF;
            uint linesPerBlockHeight = 0;

            uint bpp = Formats.bpps((uint)((int)tex.Format >> 8));

            if ((int)TileMode == 1)
            {
                blockHeight = 1;
                tex.BlockHeightLog2 = 0;
                tex.Alignment = 1;

                linesPerBlockHeight = 1;
                tex.ReadTextureLayout = 0;
            }
            else
            {
                blockHeight = TegraX1Swizzle.GetBlockHeight(DIV_ROUND_UP(tex.Height, blkHeight));
                tex.BlockHeightLog2 = (uint)Convert.ToString(blockHeight, 2).Length - 1;
                Console.WriteLine("BlockHeightLog2 " + tex.BlockHeightLog2);
                Console.WriteLine("blockHeight " + blockHeight);
                tex.Alignment = 512;
                tex.ReadTextureLayout = 1;

                linesPerBlockHeight = blockHeight * 8;

            }

            List<byte[]> mipmaps = new List<byte[]>();
            for (int mipLevel = 0; mipLevel < tex.MipCount; mipLevel++)
            {
                var result = TextureHelper.GetCurrentMipSize(tex.Width, tex.Height, blkWidth, blkHeight, bpp, mipLevel);
                uint offset = result.Item1;
                uint size = result.Item2;

                byte[] data_ = Utils.SubArray(data, offset, size);

                uint width_ = Math.Max(1, tex.Width >> mipLevel);
                uint height_ = Math.Max(1, tex.Height >> mipLevel);

                uint width__ = DIV_ROUND_UP(width_, blkWidth);
                uint height__ = DIV_ROUND_UP(height_, blkHeight);


                byte[] AlignedData = new byte[(TegraX1Swizzle.round_up(SurfaceSize, (uint)tex.Alignment) - SurfaceSize)];
                SurfaceSize += (uint)AlignedData.Length;

                Console.WriteLine("SurfaceSize Aligned " + AlignedData);

                Console.WriteLine("MipOffsets " + SurfaceSize);

                tex.MipOffsets[mipLevel] = SurfaceSize;
                if (tex.TileMode == TileMode.LinearAligned)
                {
                    Pitch = width__ * bpp;

                    Console.WriteLine("Pitch 1 " + Pitch);

                    if (target == 1)
                    {
                        Pitch = TegraX1Swizzle.round_up(width__ * bpp, 32);
                        Console.WriteLine("Pitch 2 " + Pitch);
                    }

                    SurfaceSize += Pitch * height__;
                }
                else
                {
                    if (TegraX1Swizzle.pow2_round_up(height__) < linesPerBlockHeight)
                        blockHeightShift += 1;

                    Pitch = TegraX1Swizzle.round_up(width__ * bpp, 64);

                    Console.WriteLine("Pitch 1 " + Pitch);
                    Console.WriteLine("blockHeightShift " + blockHeightShift);

                    SurfaceSize += Pitch * TegraX1Swizzle.round_up(height__, Math.Max(1, blockHeight >> blockHeightShift) * 8);

                    Console.WriteLine("SurfaceSize " + SurfaceSize);

                    byte[] SwizzledData = TegraX1Swizzle.swizzle(width_, height_, blkWidth, blkHeight, target, bpp, (uint)tex.TileMode, (int)Math.Max(0, tex.BlockHeightLog2 - blockHeightShift), data_);

                    mipmaps.Add(AlignedData.Concat(SwizzledData).ToArray());
                }
            }
            tex.ImageSize = SurfaceSize;

            return mipmaps;
        }
        private byte[] Combine(List<byte[]> arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (DataBlockOutput != null)
            {
                Texture tex = FromBitMap(DataBlockOutput[0], this);
                if (textureData != null)
                {
                    textureData.LoadTexture(tex, 1);

                }
                else
                {
                    textureData = new TextureData(tex, bntx);
                }
            }
            else
            {
                MessageBox.Show("Something went wrong???");
            }
        }
    }
}
