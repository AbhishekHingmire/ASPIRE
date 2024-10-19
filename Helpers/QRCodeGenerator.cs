#region Assembly QRCoder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Aspire\LocalCartAPI_AI_ERP\LocalCartAPI_AI_ERP\LocalCartAPI_AI\bin\QRCoder.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QRCoder;

public class QRCodeGenerator
{
    private static class ModulePlacer
    {
        private static class MaskPattern
        {
            public static bool Pattern1(int x, int y)
            {
                return (x + y) % 2 == 0;
            }

            public static bool Pattern2(int x, int y)
            {
                return y % 2 == 0;
            }

            public static bool Pattern3(int x, int y)
            {
                return x % 3 == 0;
            }

            public static bool Pattern4(int x, int y)
            {
                return (x + y) % 3 == 0;
            }

            public static bool Pattern5(int x, int y)
            {
                return (y / 2 + x / 3) % 2 == 0;
            }

            public static bool Pattern6(int x, int y)
            {
                return x * y % 2 + x * y % 3 == 0;
            }

            public static bool Pattern7(int x, int y)
            {
                return (x * y % 2 + x * y % 3) % 2 == 0;
            }

            public static bool Pattern8(int x, int y)
            {
                return ((x + y) % 2 + x * y % 3) % 2 == 0;
            }

            public static int Score(ref QRCode qrCode)
            {
                int num = 0;
                int count = qrCode.ModuleMatrix.Count;
                for (int i = 0; i < count; i++)
                {
                    int num2 = 0;
                    int num3 = 0;
                    bool flag = qrCode.ModuleMatrix[i][0];
                    bool flag2 = qrCode.ModuleMatrix[0][i];
                    for (int j = 0; j < count; j++)
                    {
                        num2 = ((qrCode.ModuleMatrix[i][j] != flag) ? 1 : (num2 + 1));
                        if (num2 == 5)
                        {
                            num += 3;
                        }
                        else if (num2 > 5)
                        {
                            num++;
                        }

                        flag = qrCode.ModuleMatrix[i][j];
                        num3 = ((qrCode.ModuleMatrix[j][i] != flag2) ? 1 : (num3 + 1));
                        if (num3 == 5)
                        {
                            num += 3;
                        }
                        else if (num3 > 5)
                        {
                            num++;
                        }

                        flag2 = qrCode.ModuleMatrix[j][i];
                    }
                }

                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = 0; j < count - 1; j++)
                    {
                        if (qrCode.ModuleMatrix[i][j] == qrCode.ModuleMatrix[i][j + 1] && qrCode.ModuleMatrix[i][j] == qrCode.ModuleMatrix[i + 1][j] && qrCode.ModuleMatrix[i][j] == qrCode.ModuleMatrix[i + 1][j + 1])
                        {
                            num += 3;
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < count - 10; j++)
                    {
                        if ((qrCode.ModuleMatrix[i][j] && !qrCode.ModuleMatrix[i][j + 1] && qrCode.ModuleMatrix[i][j + 2] && qrCode.ModuleMatrix[i][j + 3] && qrCode.ModuleMatrix[i][j + 4] && !qrCode.ModuleMatrix[i][j + 5] && qrCode.ModuleMatrix[i][j + 6] && !qrCode.ModuleMatrix[i][j + 7] && !qrCode.ModuleMatrix[i][j + 8] && !qrCode.ModuleMatrix[i][j + 9] && !qrCode.ModuleMatrix[i][j + 10]) || (!qrCode.ModuleMatrix[i][j] && !qrCode.ModuleMatrix[i][j + 1] && !qrCode.ModuleMatrix[i][j + 2] && !qrCode.ModuleMatrix[i][j + 3] && qrCode.ModuleMatrix[i][j + 4] && !qrCode.ModuleMatrix[i][j + 5] && qrCode.ModuleMatrix[i][j + 6] && qrCode.ModuleMatrix[i][j + 7] && qrCode.ModuleMatrix[i][j + 8] && !qrCode.ModuleMatrix[i][j + 9] && qrCode.ModuleMatrix[i][j + 10]))
                        {
                            num += 40;
                        }

                        if ((qrCode.ModuleMatrix[j][i] && !qrCode.ModuleMatrix[j + 1][i] && qrCode.ModuleMatrix[j + 2][i] && qrCode.ModuleMatrix[j + 3][i] && qrCode.ModuleMatrix[j + 4][i] && !qrCode.ModuleMatrix[j + 5][i] && qrCode.ModuleMatrix[j + 6][i] && !qrCode.ModuleMatrix[j + 7][i] && !qrCode.ModuleMatrix[j + 8][i] && !qrCode.ModuleMatrix[j + 9][i] && !qrCode.ModuleMatrix[j + 10][i]) || (!qrCode.ModuleMatrix[j][j] && !qrCode.ModuleMatrix[j + 1][i] && !qrCode.ModuleMatrix[j + 2][i] && !qrCode.ModuleMatrix[j + 3][i] && qrCode.ModuleMatrix[j + 4][i] && !qrCode.ModuleMatrix[j + 5][i] && qrCode.ModuleMatrix[j + 6][i] && qrCode.ModuleMatrix[j + 7][i] && qrCode.ModuleMatrix[j + 8][i] && !qrCode.ModuleMatrix[j + 9][i] && qrCode.ModuleMatrix[j + 10][i]))
                        {
                            num += 40;
                        }
                    }
                }

                int num4 = 0;
                foreach (BitArray item in qrCode.ModuleMatrix)
                {
                    foreach (bool item2 in item)
                    {
                        if (item2)
                        {
                            num4++;
                        }
                    }
                }

                int num5 = num4 / (qrCode.ModuleMatrix.Count * qrCode.ModuleMatrix.Count) * 100;
                if (num5 % 5 == 0)
                {
                    return num + Math.Min(Math.Abs(num5 - 55) / 5, Math.Abs(num5 - 45) / 5) * 10;
                }

                return num + Math.Min(Math.Abs((int)Math.Floor((decimal)num5 / 5m) - 50) / 5, Math.Abs((int)Math.Floor((decimal)num5 / 5m) + 5 - 50) / 5) * 10;
            }
        }

        public static void AddQuietZone(ref QRCode qrCode)
        {
            bool[] array = new bool[qrCode.ModuleMatrix.Count + 8];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = false;
            }

            for (int i = 0; i < 4; i++)
            {
                qrCode.ModuleMatrix.Insert(0, new BitArray(array));
            }

            for (int i = 0; i < 4; i++)
            {
                qrCode.ModuleMatrix.Add(new BitArray(array));
            }

            for (int i = 4; i < qrCode.ModuleMatrix.Count - 4; i++)
            {
                bool[] array2 = new bool[4];
                bool[] collection = array2;
                List<bool> list = new List<bool>(collection);
                foreach (bool item in qrCode.ModuleMatrix[i])
                {
                    list.Add(item);
                }

                list.AddRange(collection);
                qrCode.ModuleMatrix[i] = new BitArray(list.ToArray());
            }
        }

        public static void PlaceVersion(ref QRCode qrCode, string versionStr)
        {
            int count = qrCode.ModuleMatrix.Count;
            string text = new string(versionStr.Reverse().ToArray());
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    qrCode.ModuleMatrix[j + count - 11][i] = text[i * 3 + j] == '1';
                    qrCode.ModuleMatrix[i][j + count - 11] = text[i * 3 + j] == '1';
                }
            }
        }

        public static void PlaceFormat(ref QRCode qrCode, string formatStr)
        {
            int count = qrCode.ModuleMatrix.Count;
            string text = new string(formatStr.Reverse().ToArray());
            int[,] array = new int[15, 4]
            {
                { 8, 0, 0, 8 },
                { 8, 1, 0, 8 },
                { 8, 2, 0, 8 },
                { 8, 3, 0, 8 },
                { 8, 4, 0, 8 },
                { 8, 5, 0, 8 },
                { 8, 7, 0, 8 },
                { 8, 8, 0, 8 },
                { 7, 8, 8, 0 },
                { 5, 8, 8, 0 },
                { 4, 8, 8, 0 },
                { 3, 8, 8, 0 },
                { 2, 8, 8, 0 },
                { 1, 8, 8, 0 },
                { 0, 8, 8, 0 }
            };
            array[0, 2] = count - 1;
            array[1, 2] = count - 2;
            array[2, 2] = count - 3;
            array[3, 2] = count - 4;
            array[4, 2] = count - 5;
            array[5, 2] = count - 6;
            array[6, 2] = count - 7;
            array[7, 2] = count - 8;
            array[8, 3] = count - 7;
            array[9, 3] = count - 6;
            array[10, 3] = count - 5;
            array[11, 3] = count - 4;
            array[12, 3] = count - 3;
            array[13, 3] = count - 2;
            array[14, 3] = count - 1;
            int[,] array2 = array;
            for (int i = 0; i < 15; i++)
            {
                Point point = new Point(array2[i, 0], array2[i, 1]);
                Point point2 = new Point(array2[i, 2], array2[i, 3]);
                qrCode.ModuleMatrix[point.Y][point.X] = text[i] == '1';
                qrCode.ModuleMatrix[point2.Y][point2.X] = text[i] == '1';
            }
        }

        public static int MaskCode(ref QRCode qrCode, int version, ref List<Rectangle> blockedModules)
        {
            string patternName = string.Empty;
            int num = 0;
            int count = qrCode.ModuleMatrix.Count;
            MethodInfo[] methods = typeof(MaskPattern).GetMethods();
            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.Name.Length != 8 || !(methodInfo.Name.Substring(0, 7) == "Pattern"))
                {
                    continue;
                }

                QRCode qrCode2 = new QRCode(version);
                for (int j = 0; j < count; j++)
                {
                    for (int k = 0; k < count; k++)
                    {
                        qrCode2.ModuleMatrix[j][k] = qrCode.ModuleMatrix[j][k];
                    }
                }

                for (int k = 0; k < count; k++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (!IsBlocked(new Rectangle(k, j, 1, 1), blockedModules))
                        {
                            BitArray bitArray;
                            int index;
                            (bitArray = qrCode2.ModuleMatrix[j])[index = k] = bitArray[index] ^ (bool)methodInfo.Invoke(null, new object[2] { k, j });
                        }
                    }
                }

                int num2 = MaskPattern.Score(ref qrCode2);
                if (string.IsNullOrEmpty(patternName) || num > num2)
                {
                    patternName = methodInfo.Name;
                    num = num2;
                }
            }

            MethodInfo methodInfo2 = (from x in typeof(MaskPattern).GetMethods()
                                      where x.Name == patternName
                                      select x).First();
            for (int k = 0; k < count; k++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (!IsBlocked(new Rectangle(k, j, 1, 1), blockedModules))
                    {
                        BitArray bitArray;
                        int index;
                        (bitArray = qrCode.ModuleMatrix[j])[index = k] = bitArray[index] ^ (bool)methodInfo2.Invoke(null, new object[2] { k, j });
                    }
                }
            }

            return Convert.ToInt32(methodInfo2.Name.Substring(methodInfo2.Name.Length - 1, 1)) - 1;
        }

        public static void PlaceDataWords(ref QRCode qrCode, string data, ref List<Rectangle> blockedModules)
        {
            int count = qrCode.ModuleMatrix.Count;
            bool flag = true;
            Queue<bool> datawords = new Queue<bool>();
            data.ToList().ForEach(delegate (char x)
            {
                datawords.Enqueue(x != '0');
            });
            for (int num = count - 1; num >= 0; num -= 2)
            {
                if (num == 7 || num == 6)
                {
                    num = 5;
                }

                for (int i = 1; i <= count; i++)
                {
                    int num2 = 0;
                    if (flag)
                    {
                        num2 = count - i;
                        if (datawords.Count > 0 && !IsBlocked(new Rectangle(num, num2, 1, 1), blockedModules))
                        {
                            qrCode.ModuleMatrix[num2][num] = datawords.Dequeue();
                        }

                        if (datawords.Count > 0 && num > 0 && !IsBlocked(new Rectangle(num - 1, num2, 1, 1), blockedModules))
                        {
                            qrCode.ModuleMatrix[num2][num - 1] = datawords.Dequeue();
                        }
                    }
                    else
                    {
                        num2 = i - 1;
                        if (datawords.Count > 0 && !IsBlocked(new Rectangle(num, num2, 1, 1), blockedModules))
                        {
                            qrCode.ModuleMatrix[num2][num] = datawords.Dequeue();
                        }

                        if (datawords.Count > 0 && num > 0 && !IsBlocked(new Rectangle(num - 1, num2, 1, 1), blockedModules))
                        {
                            qrCode.ModuleMatrix[num2][num - 1] = datawords.Dequeue();
                        }
                    }
                }

                flag = !flag;
            }
        }

        public static void ReserveSeperatorAreas(int size, ref List<Rectangle> blockedModules)
        {
            blockedModules.AddRange(new Rectangle[6]
            {
                new Rectangle(7, 0, 1, 8),
                new Rectangle(0, 7, 7, 1),
                new Rectangle(0, size - 8, 8, 1),
                new Rectangle(7, size - 7, 1, 7),
                new Rectangle(size - 8, 0, 1, 8),
                new Rectangle(size - 7, 7, 7, 1)
            });
        }

        public static void ReserveVersionAreas(int size, int version, ref List<Rectangle> blockedModules)
        {
            blockedModules.AddRange(new Rectangle[6]
            {
                new Rectangle(8, 0, 1, 6),
                new Rectangle(8, 7, 1, 1),
                new Rectangle(0, 8, 6, 1),
                new Rectangle(7, 8, 2, 1),
                new Rectangle(size - 8, 8, 8, 1),
                new Rectangle(8, size - 7, 1, 7)
            });
            if (version >= 7)
            {
                blockedModules.AddRange(new Rectangle[2]
                {
                    new Rectangle(size - 11, 0, 3, 6),
                    new Rectangle(0, size - 11, 6, 3)
                });
            }
        }

        public static void PlaceDarkModule(ref QRCode qrCode, int version, ref List<Rectangle> blockedModules)
        {
            qrCode.ModuleMatrix[4 * version + 9][8] = true;
            blockedModules.Add(new Rectangle(8, 4 * version + 9, 1, 1));
        }

        public static void PlaceFinderPatterns(ref QRCode qrCode, ref List<Rectangle> blockedModules)
        {
            int count = qrCode.ModuleMatrix.Count;
            int[] array = new int[6]
            {
                0,
                0,
                count - 7,
                0,
                0,
                count - 7
            };
            for (int i = 0; i < 6; i += 2)
            {
                for (int j = 0; j < 7; j++)
                {
                    for (int k = 0; k < 7; k++)
                    {
                        if (((j != 1 && j != 5) || k <= 0 || k >= 6) && (j <= 0 || j >= 6 || (k != 1 && k != 5)))
                        {
                            qrCode.ModuleMatrix[k + array[i + 1]][j + array[i]] = true;
                        }
                    }
                }

                blockedModules.Add(new Rectangle(array[i], array[i + 1], 7, 7));
            }
        }

        public static void PlaceAlignmentPatterns(ref QRCode qrCode, List<Point> alignmentPatternLocations, ref List<Rectangle> blockedModules)
        {
            foreach (Point alignmentPatternLocation in alignmentPatternLocations)
            {
                Rectangle r = new Rectangle(alignmentPatternLocation.X, alignmentPatternLocation.Y, 5, 5);
                bool flag = false;
                foreach (Rectangle blockedModule in blockedModules)
                {
                    if (Intersects(r, blockedModule))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    continue;
                }

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (j == 0 || j == 4 || i == 0 || i == 4 || (i == 2 && j == 2))
                        {
                            qrCode.ModuleMatrix[alignmentPatternLocation.Y + j][alignmentPatternLocation.X + i] = true;
                        }
                    }
                }

                blockedModules.Add(new Rectangle(alignmentPatternLocation.X, alignmentPatternLocation.Y, 5, 5));
            }
        }

        public static void PlaceTimingPatterns(ref QRCode qrCode, ref List<Rectangle> blockedModules)
        {
            int count = qrCode.ModuleMatrix.Count;
            for (int i = 8; i < count - 8; i++)
            {
                if (i % 2 == 0)
                {
                    qrCode.ModuleMatrix[6][i] = true;
                    qrCode.ModuleMatrix[i][6] = true;
                }
            }

            blockedModules.AddRange(new Rectangle[2]
            {
                new Rectangle(6, 8, 1, count - 16),
                new Rectangle(8, 6, count - 16, 1)
            });
        }

        private static bool Intersects(Rectangle r1, Rectangle r2)
        {
            return r2.X < r1.X + r1.Width && r1.X < r2.X + r2.Width && r2.Y < r1.Y + r1.Height && r1.Y < r2.Y + r2.Height;
        }

        private static bool IsBlocked(Rectangle r1, List<Rectangle> blockedModules)
        {
            bool result = false;
            foreach (Rectangle blockedModule in blockedModules)
            {
                if (Intersects(blockedModule, r1))
                {
                    result = true;
                }
            }

            return result;
        }
    }

    public enum ECCLevel
    {
        L,
        M,
        Q,
        H
    }

    private enum EncodingMode
    {
        Numeric = 1,
        Alphanumeric = 2,
        Byte = 4,
        Kanji = 8,
        ECI = 7
    }

    private struct AlignmentPattern
    {
        public int Version;

        public List<Point> PatternPositions;
    }

    private struct CodewordBlock
    {
        public int GroupNumber;

        public int BlockNumber;

        public string BitString;

        public List<string> CodeWords;

        public List<string> ECCWords;
    }

    private struct ECCInfo
    {
        public int Version;

        public ECCLevel ErrorCorrectionLevel;

        public int TotalDataCodewords;

        public int ECCPerBlock;

        public int BlocksInGroup1;

        public int CodewordsInGroup1;

        public int BlocksInGroup2;

        public int CodewordsInGroup2;
    }

    private struct VersionInfo
    {
        public int Version;

        public List<VersionInfoDetails> Details;
    }

    private struct VersionInfoDetails
    {
        public ECCLevel ErrorCorrectionLevel;

        public Dictionary<EncodingMode, int> CapacityDict;
    }

    private struct Antilog
    {
        public int ExponentAlpha;

        public int IntegerValue;
    }

    private struct PolynomItem
    {
        public int Coefficient;

        public int Exponent;
    }

    private class Polynom
    {
        public List<PolynomItem> PolyItems { get; set; }

        public Polynom()
        {
            PolyItems = new List<PolynomItem>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            PolyItems.ForEach(delegate (PolynomItem x)
            {
                sb.Append("a^" + x.Coefficient + "*x^" + x.Exponent + " + ");
            });
            return sb.ToString().TrimEnd(' ', '+');
        }
    }

    public class QRCode
    {
        private int version;

        public List<BitArray> ModuleMatrix { get; set; }

        public QRCode(int version)
        {
            this.version = version;
            int num = ModulesPerSideFromVersion(version);
            ModuleMatrix = new List<BitArray>();
            for (int i = 0; i < num; i++)
            {
                ModuleMatrix.Add(new BitArray(num));
            }
        }

        public Bitmap GetGraphic(int pixelsPerModule)
        {
            int num = ModuleMatrix.Count * pixelsPerModule;
            Bitmap bitmap = new Bitmap(num, num);
            Graphics graphics = Graphics.FromImage(bitmap);
            for (int i = 0; i < num; i += pixelsPerModule)
            {
                for (int j = 0; j < num; j += pixelsPerModule)
                {
                    if (ModuleMatrix[(j + pixelsPerModule) / pixelsPerModule - 1][(i + pixelsPerModule) / pixelsPerModule - 1])
                    {
                        graphics.FillRectangle(Brushes.Black, new Rectangle(i, j, pixelsPerModule, pixelsPerModule));
                    }
                    else
                    {
                        graphics.FillRectangle(Brushes.White, new Rectangle(i, j, pixelsPerModule, pixelsPerModule));
                    }
                }
            }

            graphics.Save();
            return bitmap;
        }

        private int ModulesPerSideFromVersion(int version)
        {
            return 21 + (version - 1) * 4;
        }
    }

    private char[] alphanumEncTable = new char[45]
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
        'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z', ' ', '$', '%', '*',
        '+', '-', '.', '/', ':'
    };

    private int[] capacityBaseValues = new int[640]
    {
        41, 25, 17, 10, 34, 20, 14, 8, 27, 16,
        11, 7, 17, 10, 7, 4, 77, 47, 32, 20,
        63, 38, 26, 16, 48, 29, 20, 12, 34, 20,
        14, 8, 127, 77, 53, 32, 101, 61, 42, 26,
        77, 47, 32, 20, 58, 35, 24, 15, 187, 114,
        78, 48, 149, 90, 62, 38, 111, 67, 46, 28,
        82, 50, 34, 21, 255, 154, 106, 65, 202, 122,
        84, 52, 144, 87, 60, 37, 106, 64, 44, 27,
        322, 195, 134, 82, 255, 154, 106, 65, 178, 108,
        74, 45, 139, 84, 58, 36, 370, 224, 154, 95,
        293, 178, 122, 75, 207, 125, 86, 53, 154, 93,
        64, 39, 461, 279, 192, 118, 365, 221, 152, 93,
        259, 157, 108, 66, 202, 122, 84, 52, 552, 335,
        230, 141, 432, 262, 180, 111, 312, 189, 130, 80,
        235, 143, 98, 60, 652, 395, 271, 167, 513, 311,
        213, 131, 364, 221, 151, 93, 288, 174, 119, 74,
        772, 468, 321, 198, 604, 366, 251, 155, 427, 259,
        177, 109, 331, 200, 137, 85, 883, 535, 367, 226,
        691, 419, 287, 177, 489, 296, 203, 125, 374, 227,
        155, 96, 1022, 619, 425, 262, 796, 483, 331, 204,
        580, 352, 241, 149, 427, 259, 177, 109, 1101, 667,
        458, 282, 871, 528, 362, 223, 621, 376, 258, 159,
        468, 283, 194, 120, 1250, 758, 520, 320, 991, 600,
        412, 254, 703, 426, 292, 180, 530, 321, 220, 136,
        1408, 854, 586, 361, 1082, 656, 450, 277, 775, 470,
        322, 198, 602, 365, 250, 154, 1548, 938, 644, 397,
        1212, 734, 504, 310, 876, 531, 364, 224, 674, 408,
        280, 173, 1725, 1046, 718, 442, 1346, 816, 560, 345,
        948, 574, 394, 243, 746, 452, 310, 191, 1903, 1153,
        792, 488, 1500, 909, 624, 384, 1063, 644, 442, 272,
        813, 493, 338, 208, 2061, 1249, 858, 528, 1600, 970,
        666, 410, 1159, 702, 482, 297, 919, 557, 382, 235,
        2232, 1352, 929, 572, 1708, 1035, 711, 438, 1224, 742,
        509, 314, 969, 587, 403, 248, 2409, 1460, 1003, 618,
        1872, 1134, 779, 480, 1358, 823, 565, 348, 1056, 640,
        439, 270, 2620, 1588, 1091, 672, 2059, 1248, 857, 528,
        1468, 890, 611, 376, 1108, 672, 461, 284, 2812, 1704,
        1171, 721, 2188, 1326, 911, 561, 1588, 963, 661, 407,
        1228, 744, 511, 315, 3057, 1853, 1273, 784, 2395, 1451,
        997, 614, 1718, 1041, 715, 440, 1286, 779, 535, 330,
        3283, 1990, 1367, 842, 2544, 1542, 1059, 652, 1804, 1094,
        751, 462, 1425, 864, 593, 365, 3517, 2132, 1465, 902,
        2701, 1637, 1125, 692, 1933, 1172, 805, 496, 1501, 910,
        625, 385, 3669, 2223, 1528, 940, 2857, 1732, 1190, 732,
        2085, 1263, 868, 534, 1581, 958, 658, 405, 3909, 2369,
        1628, 1002, 3035, 1839, 1264, 778, 2181, 1322, 908, 559,
        1677, 1016, 698, 430, 4158, 2520, 1732, 1066, 3289, 1994,
        1370, 843, 2358, 1429, 982, 604, 1782, 1080, 742, 457,
        4417, 2677, 1840, 1132, 3486, 2113, 1452, 894, 2473, 1499,
        1030, 634, 1897, 1150, 790, 486, 4686, 2840, 1952, 1201,
        3693, 2238, 1538, 947, 2670, 1618, 1112, 684, 2022, 1226,
        842, 518, 4965, 3009, 2068, 1273, 3909, 2369, 1628, 1002,
        2805, 1700, 1168, 719, 2157, 1307, 898, 553, 5253, 3183,
        2188, 1347, 4134, 2506, 1722, 1060, 2949, 1787, 1228, 756,
        2301, 1394, 958, 590, 5529, 3351, 2303, 1417, 4343, 2632,
        1809, 1113, 3081, 1867, 1283, 790, 2361, 1431, 983, 605,
        5836, 3537, 2431, 1496, 4588, 2780, 1911, 1176, 3244, 1966,
        1351, 832, 2524, 1530, 1051, 647, 6153, 3729, 2563, 1577,
        4775, 2894, 1989, 1224, 3417, 2071, 1423, 876, 2625, 1591,
        1093, 673, 6479, 3927, 2699, 1661, 5039, 3054, 2099, 1292,
        3599, 2181, 1499, 923, 2735, 1658, 1139, 701, 6743, 4087,
        2809, 1729, 5313, 3220, 2213, 1362, 3791, 2298, 1579, 972,
        2927, 1774, 1219, 750, 7089, 4296, 2953, 1817, 5596, 3391,
        2331, 1435, 3993, 2420, 1663, 1024, 3057, 1852, 1273, 784
    };

    private int[] capacityECCBaseValues = new int[960]
    {
        19, 7, 1, 19, 0, 0, 16, 10, 1, 16,
        0, 0, 13, 13, 1, 13, 0, 0, 9, 17,
        1, 9, 0, 0, 34, 10, 1, 34, 0, 0,
        28, 16, 1, 28, 0, 0, 22, 22, 1, 22,
        0, 0, 16, 28, 1, 16, 0, 0, 55, 15,
        1, 55, 0, 0, 44, 26, 1, 44, 0, 0,
        34, 18, 2, 17, 0, 0, 26, 22, 2, 13,
        0, 0, 80, 20, 1, 80, 0, 0, 64, 18,
        2, 32, 0, 0, 48, 26, 2, 24, 0, 0,
        36, 16, 4, 9, 0, 0, 108, 26, 1, 108,
        0, 0, 86, 24, 2, 43, 0, 0, 62, 18,
        2, 15, 2, 16, 46, 22, 2, 11, 2, 12,
        136, 18, 2, 68, 0, 0, 108, 16, 4, 27,
        0, 0, 76, 24, 4, 19, 0, 0, 60, 28,
        4, 15, 0, 0, 156, 20, 2, 78, 0, 0,
        124, 18, 4, 31, 0, 0, 88, 18, 2, 14,
        4, 15, 66, 26, 4, 13, 1, 14, 194, 24,
        2, 97, 0, 0, 154, 22, 2, 38, 2, 39,
        110, 22, 4, 18, 2, 19, 86, 26, 4, 14,
        2, 15, 232, 30, 2, 116, 0, 0, 182, 22,
        3, 36, 2, 37, 132, 20, 4, 16, 4, 17,
        100, 24, 4, 12, 4, 13, 274, 18, 2, 68,
        2, 69, 216, 26, 4, 43, 1, 44, 154, 24,
        6, 19, 2, 20, 122, 28, 6, 15, 2, 16,
        324, 20, 4, 81, 0, 0, 254, 30, 1, 50,
        4, 51, 180, 28, 4, 22, 4, 23, 140, 24,
        3, 12, 8, 13, 370, 24, 2, 92, 2, 93,
        290, 22, 6, 36, 2, 37, 206, 26, 4, 20,
        6, 21, 158, 28, 7, 14, 4, 15, 428, 26,
        4, 107, 0, 0, 334, 22, 8, 37, 1, 38,
        244, 24, 8, 20, 4, 21, 180, 22, 12, 11,
        4, 12, 461, 30, 3, 115, 1, 116, 365, 24,
        4, 40, 5, 41, 261, 20, 11, 16, 5, 17,
        197, 24, 11, 12, 5, 13, 523, 22, 5, 87,
        1, 88, 415, 24, 5, 41, 5, 42, 295, 30,
        5, 24, 7, 25, 223, 24, 11, 12, 7, 13,
        589, 24, 5, 98, 1, 99, 453, 28, 7, 45,
        3, 46, 325, 24, 15, 19, 2, 20, 253, 30,
        3, 15, 13, 16, 647, 28, 1, 107, 5, 108,
        507, 28, 10, 46, 1, 47, 367, 28, 1, 22,
        15, 23, 283, 28, 2, 14, 17, 15, 721, 30,
        5, 120, 1, 121, 563, 26, 9, 43, 4, 44,
        397, 28, 17, 22, 1, 23, 313, 28, 2, 14,
        19, 15, 795, 28, 3, 113, 4, 114, 627, 26,
        3, 44, 11, 45, 445, 26, 17, 21, 4, 22,
        341, 26, 9, 13, 16, 14, 861, 28, 3, 107,
        5, 108, 669, 26, 3, 41, 13, 42, 485, 30,
        15, 24, 5, 25, 385, 28, 15, 15, 10, 16,
        932, 28, 4, 116, 4, 117, 714, 26, 17, 42,
        0, 0, 512, 28, 17, 22, 6, 23, 406, 30,
        19, 16, 6, 17, 1006, 28, 2, 111, 7, 112,
        782, 28, 17, 46, 0, 0, 568, 30, 7, 24,
        16, 25, 442, 24, 34, 13, 0, 0, 1094, 30,
        4, 121, 5, 122, 860, 28, 4, 47, 14, 48,
        614, 30, 11, 24, 14, 25, 464, 30, 16, 15,
        14, 16, 1174, 30, 6, 117, 4, 118, 914, 28,
        6, 45, 14, 46, 664, 30, 11, 24, 16, 25,
        514, 30, 30, 16, 2, 17, 1276, 26, 8, 106,
        4, 107, 1000, 28, 8, 47, 13, 48, 718, 30,
        7, 24, 22, 25, 538, 30, 22, 15, 13, 16,
        1370, 28, 10, 114, 2, 115, 1062, 28, 19, 46,
        4, 47, 754, 28, 28, 22, 6, 23, 596, 30,
        33, 16, 4, 17, 1468, 30, 8, 122, 4, 123,
        1128, 28, 22, 45, 3, 46, 808, 30, 8, 23,
        26, 24, 628, 30, 12, 15, 28, 16, 1531, 30,
        3, 117, 10, 118, 1193, 28, 3, 45, 23, 46,
        871, 30, 4, 24, 31, 25, 661, 30, 11, 15,
        31, 16, 1631, 30, 7, 116, 7, 117, 1267, 28,
        21, 45, 7, 46, 911, 30, 1, 23, 37, 24,
        701, 30, 19, 15, 26, 16, 1735, 30, 5, 115,
        10, 116, 1373, 28, 19, 47, 10, 48, 985, 30,
        15, 24, 25, 25, 745, 30, 23, 15, 25, 16,
        1843, 30, 13, 115, 3, 116, 1455, 28, 2, 46,
        29, 47, 1033, 30, 42, 24, 1, 25, 793, 30,
        23, 15, 28, 16, 1955, 30, 17, 115, 0, 0,
        1541, 28, 10, 46, 23, 47, 1115, 30, 10, 24,
        35, 25, 845, 30, 19, 15, 35, 16, 2071, 30,
        17, 115, 1, 116, 1631, 28, 14, 46, 21, 47,
        1171, 30, 29, 24, 19, 25, 901, 30, 11, 15,
        46, 16, 2191, 30, 13, 115, 6, 116, 1725, 28,
        14, 46, 23, 47, 1231, 30, 44, 24, 7, 25,
        961, 30, 59, 16, 1, 17, 2306, 30, 12, 121,
        7, 122, 1812, 28, 12, 47, 26, 48, 1286, 30,
        39, 24, 14, 25, 986, 30, 22, 15, 41, 16,
        2434, 30, 6, 121, 14, 122, 1914, 28, 6, 47,
        34, 48, 1354, 30, 46, 24, 10, 25, 1054, 30,
        2, 15, 64, 16, 2566, 30, 17, 122, 4, 123,
        1992, 28, 29, 46, 14, 47, 1426, 30, 49, 24,
        10, 25, 1096, 30, 24, 15, 46, 16, 2702, 30,
        4, 122, 18, 123, 2102, 28, 13, 46, 32, 47,
        1502, 30, 48, 24, 14, 25, 1142, 30, 42, 15,
        32, 16, 2812, 30, 20, 117, 4, 118, 2216, 28,
        40, 47, 7, 48, 1582, 30, 43, 24, 22, 25,
        1222, 30, 10, 15, 67, 16, 2956, 30, 19, 118,
        6, 119, 2334, 28, 18, 47, 31, 48, 1666, 30,
        34, 24, 34, 25, 1276, 30, 20, 15, 61, 16
    };

    private int[] alignmentPatternBaseValues = new int[280]
    {
        0, 0, 0, 0, 0, 0, 0, 6, 18, 0,
        0, 0, 0, 0, 6, 22, 0, 0, 0, 0,
        0, 6, 26, 0, 0, 0, 0, 0, 6, 30,
        0, 0, 0, 0, 0, 6, 34, 0, 0, 0,
        0, 0, 6, 22, 38, 0, 0, 0, 0, 6,
        24, 42, 0, 0, 0, 0, 6, 26, 46, 0,
        0, 0, 0, 6, 28, 50, 0, 0, 0, 0,
        6, 30, 54, 0, 0, 0, 0, 6, 32, 58,
        0, 0, 0, 0, 6, 34, 62, 0, 0, 0,
        0, 6, 26, 46, 66, 0, 0, 0, 6, 26,
        48, 70, 0, 0, 0, 6, 26, 50, 74, 0,
        0, 0, 6, 30, 54, 78, 0, 0, 0, 6,
        30, 56, 82, 0, 0, 0, 6, 30, 58, 86,
        0, 0, 0, 6, 34, 62, 90, 0, 0, 0,
        6, 28, 50, 72, 94, 0, 0, 6, 26, 50,
        74, 98, 0, 0, 6, 30, 54, 78, 102, 0,
        0, 6, 28, 54, 80, 106, 0, 0, 6, 32,
        58, 84, 110, 0, 0, 6, 30, 58, 86, 114,
        0, 0, 6, 34, 62, 90, 118, 0, 0, 6,
        26, 50, 74, 98, 122, 0, 6, 30, 54, 78,
        102, 126, 0, 6, 26, 52, 78, 104, 130, 0,
        6, 30, 56, 82, 108, 134, 0, 6, 34, 60,
        86, 112, 138, 0, 6, 30, 58, 86, 114, 142,
        0, 6, 34, 62, 90, 118, 146, 0, 6, 30,
        54, 78, 102, 126, 150, 6, 24, 50, 76, 102,
        128, 154, 6, 28, 54, 80, 106, 132, 158, 6,
        32, 58, 84, 110, 136, 162, 6, 26, 54, 82,
        110, 138, 166, 6, 30, 58, 86, 114, 142, 170
    };

    private int[] remainderBits = new int[40]
    {
        0, 7, 7, 7, 7, 7, 0, 0, 0, 0,
        0, 0, 0, 3, 3, 3, 3, 3, 3, 3,
        4, 4, 4, 4, 4, 4, 4, 3, 3, 3,
        3, 3, 3, 3, 0, 0, 0, 0, 0, 0
    };

    private List<AlignmentPattern> alignmentPatternTable;

    private List<ECCInfo> capacityECCTable;

    private List<VersionInfo> capacityTable;

    private List<Antilog> galoisField;

    private Dictionary<char, int> alphanumEncDict;

    public QRCodeGenerator()
    {
        CreateAntilogTable();
        CreateAlphanumEncDict();
        CreateCapacityTable();
        CreateCapacityECCTable();
        CreateAlignmentPatternTable();
    }

    public QRCode CreateQrCode(string plainText, ECCLevel eccLevel)
    {
        EncodingMode encodingFromPlaintext = GetEncodingFromPlaintext(plainText);
        int version = GetVersion(plainText, encodingFromPlaintext, eccLevel);
        string text = DecToBin((int)encodingFromPlaintext, 4);
        string text2 = DecToBin(plainText.Length, GetCountIndicatorLength(version, encodingFromPlaintext));
        string text3 = text + text2;
        string text4 = PlainTextToBinary(plainText, encodingFromPlaintext);
        text3 += text4;
        ECCInfo eccInfo = capacityECCTable.Where((ECCInfo x) => x.Version == version && x.ErrorCorrectionLevel.Equals(eccLevel)).Single();
        int num = eccInfo.TotalDataCodewords * 8;
        int num2 = num - text3.Length;
        if (num2 > 0)
        {
            text3 += new string('0', Math.Min(num2, 4));
        }

        if (text3.Length % 8 != 0)
        {
            text3 += new string('0', 8 - text3.Length % 8);
        }

        while (text3.Length < num)
        {
            text3 += "1110110000010001";
        }

        if (text3.Length > num)
        {
            text3 = text3.Substring(0, num);
        }

        List<CodewordBlock> list = new List<CodewordBlock>();
        for (int i = 0; i < eccInfo.BlocksInGroup1; i++)
        {
            string bitString = text3.Substring(i * eccInfo.CodewordsInGroup1 * 8, eccInfo.CodewordsInGroup1 * 8);
            list.Add(new CodewordBlock
            {
                BitString = bitString,
                BlockNumber = i + 1,
                GroupNumber = 1,
                CodeWords = BinaryStringToBitBlockList(bitString),
                ECCWords = CalculateECCWords(bitString, eccInfo)
            });
        }

        text3 = text3.Substring(eccInfo.BlocksInGroup1 * eccInfo.CodewordsInGroup1 * 8);
        for (int i = 0; i < eccInfo.BlocksInGroup2; i++)
        {
            string bitString = text3.Substring(i * eccInfo.CodewordsInGroup2 * 8, eccInfo.CodewordsInGroup2 * 8);
            list.Add(new CodewordBlock
            {
                BitString = bitString,
                BlockNumber = i + 1,
                GroupNumber = 2,
                CodeWords = BinaryStringToBitBlockList(bitString),
                ECCWords = CalculateECCWords(bitString, eccInfo)
            });
        }

        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < Math.Max(eccInfo.CodewordsInGroup1, eccInfo.CodewordsInGroup2); i++)
        {
            foreach (CodewordBlock item in list)
            {
                if (item.CodeWords.Count > i)
                {
                    stringBuilder.Append(item.CodeWords[i]);
                }
            }
        }

        for (int i = 0; i < eccInfo.ECCPerBlock; i++)
        {
            foreach (CodewordBlock item2 in list)
            {
                stringBuilder.Append(item2.ECCWords[i]);
            }
        }

        stringBuilder.Append(new string('0', remainderBits[version - 1]));
        string data = stringBuilder.ToString();
        QRCode qrCode = new QRCode(version);
        List<Rectangle> blockedModules = new List<Rectangle>();
        ModulePlacer.PlaceFinderPatterns(ref qrCode, ref blockedModules);
        ModulePlacer.ReserveSeperatorAreas(qrCode.ModuleMatrix.Count, ref blockedModules);
        ModulePlacer.PlaceAlignmentPatterns(ref qrCode, (from x in alignmentPatternTable
                                                         where x.Version == version
                                                         select x.PatternPositions).First(), ref blockedModules);
        ModulePlacer.PlaceTimingPatterns(ref qrCode, ref blockedModules);
        ModulePlacer.PlaceDarkModule(ref qrCode, version, ref blockedModules);
        ModulePlacer.ReserveVersionAreas(qrCode.ModuleMatrix.Count, version, ref blockedModules);
        ModulePlacer.PlaceDataWords(ref qrCode, data, ref blockedModules);
        int maskVersion = ModulePlacer.MaskCode(ref qrCode, version, ref blockedModules);
        string formatString = GetFormatString(eccLevel, maskVersion);
        ModulePlacer.PlaceFormat(ref qrCode, formatString);
        if (version >= 7)
        {
            string versionString = GetVersionString(version);
            ModulePlacer.PlaceVersion(ref qrCode, versionString);
        }

        ModulePlacer.AddQuietZone(ref qrCode);
        return qrCode;
    }

    private string GetFormatString(ECCLevel level, int maskVersion)
    {
        string text = "10100110111";
        string text2 = "101010000010010";
        string text3 = level switch
        {
            ECCLevel.Q => "11",
            ECCLevel.M => "00",
            ECCLevel.L => "01",
            _ => "10",
        } + DecToBin(maskVersion, 3);
        string text4 = text3.PadRight(15, '0').TrimStart('0');
        while (text4.Length > 10)
        {
            StringBuilder stringBuilder = new StringBuilder();
            text = text.PadRight(text4.Length, '0');
            for (int i = 0; i < text4.Length; i++)
            {
                stringBuilder.Append((Convert.ToInt32(text4[i]) ^ Convert.ToInt32(text[i])).ToString());
            }

            text4 = stringBuilder.ToString().TrimStart('0');
        }

        text4 = text4.PadLeft(10, '0');
        text3 += text4;
        StringBuilder stringBuilder2 = new StringBuilder();
        for (int i = 0; i < text3.Length; i++)
        {
            stringBuilder2.Append((Convert.ToInt32(text3[i]) ^ Convert.ToInt32(text2[i])).ToString());
        }

        return stringBuilder2.ToString();
    }

    private string GetVersionString(int version)
    {
        string text = "1111100100101";
        string text2 = DecToBin(version, 6);
        string text3 = text2.PadRight(18, '0').TrimStart('0');
        while (text3.Length > 12)
        {
            StringBuilder stringBuilder = new StringBuilder();
            text = text.PadRight(text3.Length, '0');
            for (int i = 0; i < text3.Length; i++)
            {
                stringBuilder.Append((Convert.ToInt32(text3[i]) ^ Convert.ToInt32(text[i])).ToString());
            }

            text3 = stringBuilder.ToString().TrimStart('0');
        }

        text3 = text3.PadLeft(12, '0');
        return text2 + text3;
    }

    private List<string> CalculateECCWords(string bitString, ECCInfo eccInfo)
    {
        int eCCPerBlock = eccInfo.ECCPerBlock;
        Polynom polynom = CalculateMessagePolynom(bitString);
        Polynom polynom2 = CalculateGeneratorPolynom(eCCPerBlock);
        for (int i = 0; i < polynom.PolyItems.Count; i++)
        {
            polynom.PolyItems[i] = new PolynomItem
            {
                Coefficient = polynom.PolyItems[i].Coefficient,
                Exponent = polynom.PolyItems[i].Exponent + eCCPerBlock
            };
        }

        int num = polynom.PolyItems[0].Exponent - polynom2.PolyItems[0].Exponent;
        for (int i = 0; i < polynom2.PolyItems.Count; i++)
        {
            polynom2.PolyItems[i] = new PolynomItem
            {
                Coefficient = polynom2.PolyItems[i].Coefficient,
                Exponent = polynom2.PolyItems[i].Exponent + num
            };
        }

        Polynom polynom3 = polynom;
        for (int i = 0; i < polynom.PolyItems.Count; i++)
        {
            Polynom poly = MultiplyGeneratorPolynomByLeadterm(polynom2, ConvertToAlphaNotation(polynom3).PolyItems[0], i);
            poly = ConvertToDecNotation(poly);
            poly = XORPolynoms(polynom3, poly);
            polynom3 = poly;
        }

        return polynom3.PolyItems.Select((PolynomItem x) => DecToBin(x.Coefficient, 8)).ToList();
    }

    private Polynom ConvertToAlphaNotation(Polynom poly)
    {
        Polynom polynom = new Polynom();
        for (int i = 0; i < poly.PolyItems.Count; i++)
        {
            polynom.PolyItems.Add(new PolynomItem
            {
                Coefficient = ((poly.PolyItems[i].Coefficient != 0) ? GetAlphaExpFromIntVal(poly.PolyItems[i].Coefficient) : 0),
                Exponent = poly.PolyItems[i].Exponent
            });
        }

        return polynom;
    }

    private Polynom ConvertToDecNotation(Polynom poly)
    {
        Polynom polynom = new Polynom();
        for (int i = 0; i < poly.PolyItems.Count; i++)
        {
            polynom.PolyItems.Add(new PolynomItem
            {
                Coefficient = GetIntValFromAlphaExp(poly.PolyItems[i].Coefficient),
                Exponent = poly.PolyItems[i].Exponent
            });
        }

        return polynom;
    }

    private int GetVersion(string plainText, EncodingMode encMode, ECCLevel eccLevel)
    {
        return (from x in capacityTable
                where x.Details.Where((VersionInfoDetails y) => y.ErrorCorrectionLevel == eccLevel && y.CapacityDict[encMode] >= Convert.ToInt32(plainText.Length)).Count() > 0
                select new
                {
                    version = x.Version,
                    capacity = x.Details.Where((VersionInfoDetails y) => y.ErrorCorrectionLevel == eccLevel).Single().CapacityDict[encMode]
                }).Min(x => x.version);
    }

    private EncodingMode GetEncodingFromPlaintext(string plainText)
    {
        if (plainText.All((char c) => "0123456789".Contains(c)))
        {
            return EncodingMode.Numeric;
        }

        if (plainText.All((char c) => alphanumEncTable.Contains(c)))
        {
            return EncodingMode.Alphanumeric;
        }

        return EncodingMode.Byte;
    }

    private Polynom CalculateMessagePolynom(string bitString)
    {
        Polynom polynom = new Polynom();
        for (int num = bitString.Length / 8 - 1; num >= 0; num--)
        {
            polynom.PolyItems.Add(new PolynomItem
            {
                Coefficient = BinToDec(bitString.Substring(0, 8)),
                Exponent = num
            });
            bitString = bitString.Remove(0, 8);
        }

        return polynom;
    }

    private Polynom CalculateGeneratorPolynom(int numEccWords)
    {
        Polynom polynom = new Polynom();
        polynom.PolyItems.AddRange(new PolynomItem[2]
        {
            new PolynomItem
            {
                Coefficient = 0,
                Exponent = 1
            },
            new PolynomItem
            {
                Coefficient = 0,
                Exponent = 0
            }
        });
        for (int i = 1; i <= numEccWords - 1; i++)
        {
            Polynom polynom2 = new Polynom();
            polynom2.PolyItems.AddRange(new PolynomItem[2]
            {
                new PolynomItem
                {
                    Coefficient = 0,
                    Exponent = 1
                },
                new PolynomItem
                {
                    Coefficient = i,
                    Exponent = 0
                }
            });
            polynom = MultiplyAlphaPolynoms(polynom, polynom2);
        }

        return polynom;
    }

    private List<string> BinaryStringToBitBlockList(string bitString)
    {
        return (from x in bitString.ToList().Select((char x, int i) => new
        {
            Index = i,
            Value = x
        })
                group x by x.Index / 8 into x
                select string.Join("", x.Select(v => v.Value.ToString()).ToArray())).ToList();
    }

    private int BinToDec(string binStr)
    {
        return Convert.ToInt32(binStr, 2);
    }

    private string DecToBin(int decNum)
    {
        return Convert.ToString(decNum, 2);
    }

    private string DecToBin(int decNum, int padLeftUpTo)
    {
        string text = DecToBin(decNum);
        return text.PadLeft(padLeftUpTo, '0');
    }

    private int GetCountIndicatorLength(int version, EncodingMode encMode)
    {
        if (version < 10)
        {
            if (encMode.Equals(EncodingMode.Numeric))
            {
                return 10;
            }

            if (encMode.Equals(EncodingMode.Alphanumeric))
            {
                return 9;
            }

            return 8;
        }

        if (version < 27)
        {
            if (encMode.Equals(EncodingMode.Numeric))
            {
                return 12;
            }

            if (encMode.Equals(EncodingMode.Alphanumeric))
            {
                return 11;
            }

            if (encMode.Equals(EncodingMode.Byte))
            {
                return 16;
            }

            return 10;
        }

        if (encMode.Equals(EncodingMode.Numeric))
        {
            return 14;
        }

        if (encMode.Equals(EncodingMode.Alphanumeric))
        {
            return 13;
        }

        if (encMode.Equals(EncodingMode.Byte))
        {
            return 16;
        }

        return 12;
    }

    private bool IsValidISO(string input)
    {
        byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(input);
        string @string = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
        return string.Equals(input, @string);
    }

    private string PlainTextToBinary(string plainText, EncodingMode encMode)
    {
        if (encMode.Equals(EncodingMode.Numeric))
        {
            return PlainTextToBinaryNumeric(plainText);
        }

        if (encMode.Equals(EncodingMode.Alphanumeric))
        {
            return PlainTextToBinaryAlphanumeric(plainText);
        }

        if (encMode.Equals(EncodingMode.Byte))
        {
            return PlainTextToBinaryByte(plainText);
        }

        return string.Empty;
    }

    private string PlainTextToBinaryNumeric(string plainText)
    {
        string text = string.Empty;
        while (plainText.Length >= 3)
        {
            int decNum = Convert.ToInt32(plainText.Substring(0, 3));
            text += DecToBin(decNum, 10);
            plainText = plainText.Substring(3);
        }

        if (plainText.Length == 2)
        {
            int decNum = Convert.ToInt32(plainText.Substring(0, plainText.Length));
            text += DecToBin(decNum, 7);
        }
        else if (plainText.Length == 1)
        {
            int decNum = Convert.ToInt32(plainText.Substring(0, plainText.Length));
            text += DecToBin(decNum, 4);
        }

        return text;
    }

    private string PlainTextToBinaryAlphanumeric(string plainText)
    {
        string text = string.Empty;
        while (plainText.Length >= 2)
        {
            string text2 = plainText.Substring(0, 2);
            int decNum = alphanumEncDict[text2[0]] * 45 + alphanumEncDict[text2[1]];
            text += DecToBin(decNum, 11);
            plainText = plainText.Substring(2);
        }

        if (plainText.Length > 0)
        {
            text += DecToBin(alphanumEncDict[plainText[0]], 6);
        }

        return text;
    }

    private string PlainTextToBinaryByte(string plainText)
    {
        byte[] array = new byte[1];
        string text = string.Empty;
        array = ((!IsValidISO(plainText)) ? Encoding.UTF8.GetBytes(plainText) : Encoding.GetEncoding("ISO-8859-1").GetBytes(plainText));
        byte[] array2 = array;
        foreach (byte decNum in array2)
        {
            text += DecToBin(decNum, 8);
        }

        return text;
    }

    private Polynom XORPolynoms(Polynom messagePolynom, Polynom resPolynom)
    {
        Polynom polynom = new Polynom();
        Polynom polynom2;
        Polynom polynom3;
        if (messagePolynom.PolyItems.Count >= resPolynom.PolyItems.Count)
        {
            polynom2 = messagePolynom;
            polynom3 = resPolynom;
        }
        else
        {
            polynom2 = resPolynom;
            polynom3 = messagePolynom;
        }

        for (int i = 0; i < polynom2.PolyItems.Count; i++)
        {
            PolynomItem item = default(PolynomItem);
            item.Coefficient = polynom2.PolyItems[i].Coefficient ^ ((polynom3.PolyItems.Count > i) ? polynom3.PolyItems[i].Coefficient : 0);
            item.Exponent = messagePolynom.PolyItems[0].Exponent - i;
            polynom.PolyItems.Add(item);
        }

        polynom.PolyItems.RemoveAt(0);
        return polynom;
    }

    private Polynom MultiplyGeneratorPolynomByLeadterm(Polynom genPolynom, PolynomItem leadTerm, int lowerExponentBy)
    {
        Polynom polynom = new Polynom();
        foreach (PolynomItem polyItem in genPolynom.PolyItems)
        {
            PolynomItem item = default(PolynomItem);
            item.Coefficient = (polyItem.Coefficient + leadTerm.Coefficient) % 255;
            item.Exponent = polyItem.Exponent - lowerExponentBy;
            polynom.PolyItems.Add(item);
        }

        return polynom;
    }

    private Polynom MultiplyAlphaPolynoms(Polynom polynomBase, Polynom polynomMultiplier)
    {
        Polynom polynom = new Polynom();
        foreach (PolynomItem polyItem in polynomMultiplier.PolyItems)
        {
            foreach (PolynomItem polyItem2 in polynomBase.PolyItems)
            {
                PolynomItem item = default(PolynomItem);
                item.Coefficient = ShrinkAlphaExp(polyItem.Coefficient + polyItem2.Coefficient);
                item.Exponent = polyItem.Exponent + polyItem2.Exponent;
                polynom.PolyItems.Add(item);
            }
        }

        IEnumerable<int> exponentsToGlue = from x in polynom.PolyItems
                                           group x by x.Exponent into x
                                           where x.Count() > 1
                                           select x.First().Exponent;
        List<PolynomItem> list = new List<PolynomItem>();
        int exponent;
        foreach (int item3 in exponentsToGlue)
        {
            exponent = item3;
            PolynomItem item2 = default(PolynomItem);
            item2.Exponent = exponent;
            int num = 0;
            foreach (PolynomItem item4 in polynom.PolyItems.Where((PolynomItem x) => x.Exponent == exponent))
            {
                num ^= GetIntValFromAlphaExp(item4.Coefficient);
            }

            item2.Coefficient = GetAlphaExpFromIntVal(num);
            list.Add(item2);
        }

        polynom.PolyItems.RemoveAll((PolynomItem x) => exponentsToGlue.Contains(x.Exponent));
        polynom.PolyItems.AddRange(list);
        polynom.PolyItems = polynom.PolyItems.OrderByDescending((PolynomItem x) => x.Exponent).ToList();
        return polynom;
    }

    private int GetIntValFromAlphaExp(int exp)
    {
        return (from alog in galoisField
                where alog.ExponentAlpha == exp
                select alog.IntegerValue).First();
    }

    private int GetAlphaExpFromIntVal(int intVal)
    {
        return (from alog in galoisField
                where alog.IntegerValue == intVal
                select alog.ExponentAlpha).First();
    }

    private int ShrinkAlphaExp(int alphaExp)
    {
        return (int)((double)(alphaExp % 256) + Math.Floor((double)(alphaExp / 256)));
    }

    private void CreateAlphanumEncDict()
    {
        alphanumEncDict = new Dictionary<char, int>();
        alphanumEncTable.ToList().Select((char x, int i) => new
        {
            Chr = x,
            Index = i
        }).ToList()
            .ForEach(x =>
            {
                alphanumEncDict.Add(x.Chr, x.Index);
            });
    }

    private void CreateAlignmentPatternTable()
    {
        alignmentPatternTable = new List<AlignmentPattern>();
        for (int i = 0; i < 280; i += 7)
        {
            List<Point> list = new List<Point>();
            for (int j = 0; j < 7; j++)
            {
                if (alignmentPatternBaseValues[i + j] == 0)
                {
                    continue;
                }

                for (int k = 0; k < 7; k++)
                {
                    if (alignmentPatternBaseValues[i + k] != 0)
                    {
                        Point item = new Point(alignmentPatternBaseValues[i + j] - 2, alignmentPatternBaseValues[i + k] - 2);
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                }
            }

            alignmentPatternTable.Add(new AlignmentPattern
            {
                Version = (i + 7) / 7,
                PatternPositions = list
            });
        }
    }

    private void CreateCapacityECCTable()
    {
        capacityECCTable = new List<ECCInfo>();
        for (int i = 0; i < 960; i += 24)
        {
            capacityECCTable.AddRange(new ECCInfo[4]
            {
                new ECCInfo
                {
                    Version = (i + 24) / 24,
                    ErrorCorrectionLevel = ECCLevel.L,
                    TotalDataCodewords = capacityECCBaseValues[i],
                    ECCPerBlock = capacityECCBaseValues[i + 1],
                    BlocksInGroup1 = capacityECCBaseValues[i + 2],
                    CodewordsInGroup1 = capacityECCBaseValues[i + 3],
                    BlocksInGroup2 = capacityECCBaseValues[i + 4],
                    CodewordsInGroup2 = capacityECCBaseValues[i + 5]
                },
                new ECCInfo
                {
                    Version = (i + 24) / 24,
                    ErrorCorrectionLevel = ECCLevel.M,
                    TotalDataCodewords = capacityECCBaseValues[i + 6],
                    ECCPerBlock = capacityECCBaseValues[i + 7],
                    BlocksInGroup1 = capacityECCBaseValues[i + 8],
                    CodewordsInGroup1 = capacityECCBaseValues[i + 9],
                    BlocksInGroup2 = capacityECCBaseValues[i + 10],
                    CodewordsInGroup2 = capacityECCBaseValues[i + 11]
                },
                new ECCInfo
                {
                    Version = (i + 24) / 24,
                    ErrorCorrectionLevel = ECCLevel.Q,
                    TotalDataCodewords = capacityECCBaseValues[i + 12],
                    ECCPerBlock = capacityECCBaseValues[i + 13],
                    BlocksInGroup1 = capacityECCBaseValues[i + 14],
                    CodewordsInGroup1 = capacityECCBaseValues[i + 15],
                    BlocksInGroup2 = capacityECCBaseValues[i + 16],
                    CodewordsInGroup2 = capacityECCBaseValues[i + 17]
                },
                new ECCInfo
                {
                    Version = (i + 24) / 24,
                    ErrorCorrectionLevel = ECCLevel.H,
                    TotalDataCodewords = capacityECCBaseValues[i + 18],
                    ECCPerBlock = capacityECCBaseValues[i + 19],
                    BlocksInGroup1 = capacityECCBaseValues[i + 20],
                    CodewordsInGroup1 = capacityECCBaseValues[i + 21],
                    BlocksInGroup2 = capacityECCBaseValues[i + 22],
                    CodewordsInGroup2 = capacityECCBaseValues[i + 23]
                }
            });
        }
    }

    private void CreateCapacityTable()
    {
        capacityTable = new List<VersionInfo>();
        for (int i = 0; i < 640; i += 16)
        {
            capacityTable.Add(new VersionInfo
            {
                Version = (i + 16) / 16,
                Details = new List<VersionInfoDetails>
                {
                    new VersionInfoDetails
                    {
                        ErrorCorrectionLevel = ECCLevel.L,
                        CapacityDict = new Dictionary<EncodingMode, int>
                        {
                            {
                                EncodingMode.Numeric,
                                capacityBaseValues[i]
                            },
                            {
                                EncodingMode.Alphanumeric,
                                capacityBaseValues[i + 1]
                            },
                            {
                                EncodingMode.Byte,
                                capacityBaseValues[i + 2]
                            },
                            {
                                EncodingMode.Kanji,
                                capacityBaseValues[i + 3]
                            }
                        }
                    },
                    new VersionInfoDetails
                    {
                        ErrorCorrectionLevel = ECCLevel.M,
                        CapacityDict = new Dictionary<EncodingMode, int>
                        {
                            {
                                EncodingMode.Numeric,
                                capacityBaseValues[i + 4]
                            },
                            {
                                EncodingMode.Alphanumeric,
                                capacityBaseValues[i + 5]
                            },
                            {
                                EncodingMode.Byte,
                                capacityBaseValues[i + 6]
                            },
                            {
                                EncodingMode.Kanji,
                                capacityBaseValues[i + 7]
                            }
                        }
                    },
                    new VersionInfoDetails
                    {
                        ErrorCorrectionLevel = ECCLevel.Q,
                        CapacityDict = new Dictionary<EncodingMode, int>
                        {
                            {
                                EncodingMode.Numeric,
                                capacityBaseValues[i + 8]
                            },
                            {
                                EncodingMode.Alphanumeric,
                                capacityBaseValues[i + 9]
                            },
                            {
                                EncodingMode.Byte,
                                capacityBaseValues[i + 10]
                            },
                            {
                                EncodingMode.Kanji,
                                capacityBaseValues[i + 11]
                            }
                        }
                    },
                    new VersionInfoDetails
                    {
                        ErrorCorrectionLevel = ECCLevel.H,
                        CapacityDict = new Dictionary<EncodingMode, int>
                        {
                            {
                                EncodingMode.Numeric,
                                capacityBaseValues[i + 12]
                            },
                            {
                                EncodingMode.Alphanumeric,
                                capacityBaseValues[i + 13]
                            },
                            {
                                EncodingMode.Byte,
                                capacityBaseValues[i + 14]
                            },
                            {
                                EncodingMode.Kanji,
                                capacityBaseValues[i + 15]
                            }
                        }
                    }
                }
            });
        }
    }

    private void CreateAntilogTable()
    {
        galoisField = new List<Antilog>();
        for (int i = 0; i < 256; i++)
        {
            int num = (int)Math.Pow(2.0, i);
            if (i > 7)
            {
                num = galoisField[i - 1].IntegerValue * 2;
            }

            if (num > 255)
            {
                num ^= 0x11D;
            }

            galoisField.Add(new Antilog
            {
                ExponentAlpha = i,
                IntegerValue = num
            });
        }
    }
}
#if false // Decompilation log
'63' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\mscorlib.dll'
------------------
Resolve: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Drawing.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Core.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.dll'
#endif
