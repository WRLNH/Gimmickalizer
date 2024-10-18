using NLog;
using NLog.Targets.Helper;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gimmickalizer.Views;
using System.Windows.Input;

namespace Gimmickalizer
{
    public class TimingPoints
    {
        /// <summary>
        /// Offset为offset
        /// BPM为BPM/SV速度参数
        /// TimeSignature为节拍
        /// Sample为音效种类  1为Normal 2为Soft 3为Drum
        /// SampleNumber为音效号码
        /// Volume为音量
        /// RedOrGreen为 红线 或 绿线  1为红线 0为绿线
        /// KiaiMode为Kiai Mode
        /// </summary>
        public int Offset;
        public string BPM;
        public int TimeSignature;
        public int Sample;
        public int SampleNumber;
        public short Volume;
        public short RedOrGreen;
        public short KiaiMode;
    }

    public class HitObjects
    {
        /// <summary>
        /// XCoordinate为X坐标
        /// YCoordinate为Y坐标
        /// Offset为时间点
        /// Type为类型  1为Circle 2为Slider 12为Spinner 5为Circle New Combo 6为Slider New Combo
        /// SoundEffect为Circle类型 若为Circl则0为o 2或8为x 4为O 6或12为X  若为Slider则0为小滑条 4为大滑条
        /// SliderSomthing为Slider的一些信息  Taiko里似乎没啥用 仅Slider该成员有值
        /// SliderRepeatTimes为Slider的一些信息  Taiko里似乎没啥用 仅Slider该成员有值
        /// OffsetLength为滑条长度  仅Slider该成员有值
        /// SpinnerSample不确定  一般为0 仅Spinner该成员有值
        /// SpinnerEndOffset为转盘结束时间点  仅Spinner该成员有值
        /// UselessEnding不知道是啥  一般为"0:0:0:0:" 仅Circle和Spinner该成员有值
        /// </summary>
        public int XCoordinate;
        public int YCoordinate;
        public int Offset;
        public short Type;
        public short SoundEffect;
        public string SliderSomething;
        public int SliderRepeatTimes;
        public string OffsetLength;
        public int SpinnerSample;
        public int SpinnerEndOffset;
        public string UselessEnding;
    }

    public class Gimmickalize
    {
        public static string GetDiffName(in string Name)
        {
            // 这个实现的鲁棒性不够好 也许以后有空再写个更好的实现

            string temp = "";
            string diffName = "";

            int i;
            for (i = Name.Length - 1; i >= 0; i--)
            {
                if (Name.ToString()[i] == ']')
                {
                    i--;
                    break;
                }
            }
            for (; Name.ToString()[i] != '['; i--)
            {
                temp += Name[i];
            }
            for (i = temp.Length - 1; i >= 0; i--)
            {
                diffName += temp[i];
            }

            return diffName;
        }

        // 返回所传入的offset的上一条红线的索引
        public static int GetLastRedLineIndex(in List<TimingPoints> RedLines, in int offset)
        {
            for (int i = 0; i < RedLines.Count; i++)
            {
                if (RedLines[i].Offset == offset)
                {
                    return i;
                }
                else if ((RedLines[i].Offset < offset) && (i == RedLines.Count - 1))
                {
                    return i;
                }
                else if ((RedLines[i].Offset < offset) && (RedLines[i + 1].Offset > offset))
                {
                    return i;
                }
            }

            return -1;
        }

        // 返回所传入的offset的上一条绿线的索引 若没有返回-1
        public static int GetLastGreenLineIndex(in List<TimingPoints> GreenLines, in int offset)
        {
            for (int i = 0; i < GreenLines.Count; i++)
            {
                if (GreenLines[i].Offset == offset)
                {
                    return i;
                }
                else if ((GreenLines[i].Offset < offset) && (i == GreenLines.Count - 1))
                {
                    return i;
                }
                else if ((GreenLines[i].Offset < offset) && (GreenLines[i + 1].Offset > offset))
                {
                    return i;
                }
            }

            return -1;
        }

        public static void Barlines(in StreamReader sr, in string fileFullName, in Logger logger)
        {
            string line;
            string flag = "";
            string outFileFullName = fileFullName.Substring(0, fileFullName.Length - 5);
            outFileFullName += " Barlines].osu";
            List<string> strLstTimingPoints = [];
            List<string> strLstHitObjects = [];
            List<TimingPoints> timingPoints = [];
            List<TimingPoints> RedLines = [];
            List<TimingPoints> GreenLines = [];
            List<HitObjects> hitObjects = [];
            StreamWriter sw = new StreamWriter(outFileFullName);

            // 遍历.osu文件
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "Mode: 0" || line == "Mode: 2" || line == "Mode: 3")
                {
                    logger.Error("模式错误, 请检查所选难度的模式...");
                    return;
                }

                if (line.Length >= 8)
                {
                    if (line.Substring(0, 8) == "Version:")
                    {
                        sw.WriteLine(line + " Barlines");
                        continue;
                    }
                }

                if (line == "[TimingPoints]")
                {
                    flag = "TimingPoints";
                    continue;
                }
                else if (line == "[HitObjects]")
                {
                    flag = "HitObjects";
                    continue;
                }

                if (flag == "TimingPoints" && line != "")
                {
                    strLstTimingPoints.Add(line);
                    continue;
                }
                else if (flag == "HitObjects" && line != "")
                {
                    strLstHitObjects.Add(line);
                    continue;
                }

                if (flag != "TimingPoints" || flag != "HitObjects")
                {
                    sw.WriteLine(line);
                }
            }

            // 获取所有TimingPoints信息
            foreach (string str in strLstTimingPoints)
            {
                string[] subs = str.Split(',');
                TimingPoints temp = new TimingPoints();

                temp.Offset = Convert.ToInt32(subs[0]);
                temp.BPM = subs[1];
                temp.TimeSignature = Convert.ToInt32(subs[2]);
                temp.Sample = Convert.ToInt32(subs[3]);
                temp.SampleNumber = Convert.ToInt32(subs[4]);
                temp.Volume = Convert.ToInt16(subs[5]);
                temp.RedOrGreen = Convert.ToInt16(subs[6]);
                temp.KiaiMode = Convert.ToInt16(subs[7]);
                timingPoints.Add(temp);
            }
            
            // 为了应对某些极端情况 按照Offset对timingPoints重排
            timingPoints.Sort((x, y) => x.Offset - y.Offset);
            foreach (TimingPoints timings in timingPoints)
            {
                if (timings.RedOrGreen == 1)
                {
                    RedLines.Add(timings);
                }
                else if (timings.RedOrGreen == 0)
                {
                    GreenLines.Add(timings);
                }
            }

            // 获取所有HitObjects的信息
            foreach (string str in strLstHitObjects)
            {
                string[] subs = str.Split(',');
                HitObjects temp = new HitObjects();

                temp.XCoordinate = Convert.ToInt32(subs[0]);
                temp.YCoordinate = Convert.ToInt32(subs[1]);
                temp.Offset = Convert.ToInt32(subs[2]);
                temp.Type = Convert.ToInt16(subs[3]);
                temp.SoundEffect = Convert.ToInt16(subs[4]);
                if (subs[3] == "1" || subs[3] == "5")
                {
                    // 这是个Circle
                    //Debug.WriteLine("Circle");
                    temp.UselessEnding = subs[5];
                }
                if (subs[3] == "2" || subs[3] == "6")
                {
                    // 这是个Slider
                    temp.SliderSomething = subs[5];
                    temp.SliderRepeatTimes = Convert.ToInt32(subs[6]);
                    temp.OffsetLength = subs[7];
                }
                if (subs[3] == "12")
                {
                    // 这是个Spinner
                    temp.SpinnerEndOffset = Convert.ToInt32(subs[5]);
                    temp.UselessEnding = subs[6];
                }
                hitObjects.Add(temp);
            }

            // 插红线以形成小节线
            for (int i = 1; i < hitObjects.Count; i++)
            {
                // 考虑到某些极端情况 第一个HitObject不Gimmick化
                if ((hitObjects[i].Type == 1) || (hitObjects[i].Type == 5))
                {
                    int j = GetLastRedLineIndex(in RedLines, in hitObjects[i].Offset);
                    int k = GetLastGreenLineIndex(in GreenLines, in hitObjects[i].Offset);
                    if ((hitObjects[i].SoundEffect == 0) || (hitObjects[i].SoundEffect == 4))
                    {
                        // 如果为咚 插3条红线
                        if (k != -1)
                        {
                            TimingPoints RedLine1 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine2 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset,
                                BPM = "1",
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine3 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            timingPoints.Add(RedLine1);
                            timingPoints.Add(RedLine2);
                            timingPoints.Add(RedLine3);
                        }
                        else
                        {
                            TimingPoints RedLine1 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine2 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset,
                                BPM = "1",
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine3 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            timingPoints.Add(RedLine1);
                            timingPoints.Add(RedLine2);
                            timingPoints.Add(RedLine3);
                        }
                    }
                    else if ((hitObjects[i].SoundEffect == 2) || (hitObjects[i].SoundEffect == 8) || (hitObjects[i].SoundEffect == 6) || (hitObjects[i].SoundEffect == 12))
                    {
                        // 如果为咔 插7条红线
                        if (k != -1)
                        {
                            TimingPoints RedLine1 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 7,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine2 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 4,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine3 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine4 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset,
                                BPM = "1",
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine5 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine6 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 4,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine7 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 7,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = GreenLines[k].Sample,
                                SampleNumber = GreenLines[k].SampleNumber,
                                Volume = GreenLines[k].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            timingPoints.Add(RedLine1);
                            timingPoints.Add(RedLine2);
                            timingPoints.Add(RedLine3);
                            timingPoints.Add(RedLine4);
                            timingPoints.Add(RedLine5);
                            timingPoints.Add(RedLine6);
                            timingPoints.Add(RedLine7);
                        }
                        else
                        {
                            TimingPoints RedLine1 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 7,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine2 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 4,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine3 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset - 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine4 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset,
                                BPM = "1",
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine5 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 1,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine6 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 4,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            TimingPoints RedLine7 = new TimingPoints
                            {
                                Offset = hitObjects[i].Offset + 7,
                                BPM = RedLines[j].BPM,
                                TimeSignature = RedLines[j].TimeSignature,
                                Sample = RedLines[j].Sample,
                                SampleNumber = RedLines[j].SampleNumber,
                                Volume = RedLines[j].Volume,
                                RedOrGreen = 1,
                                KiaiMode = 0
                            };
                            timingPoints.Add(RedLine1);
                            timingPoints.Add(RedLine2);
                            timingPoints.Add(RedLine3);
                            timingPoints.Add(RedLine4);
                            timingPoints.Add(RedLine5);
                            timingPoints.Add(RedLine6);
                            timingPoints.Add(RedLine7);
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            timingPoints.Sort((x, y) => x.Offset - y.Offset);

            // 写入TimingPoints
            sw.WriteLine("[TimingPoints]");
            foreach (TimingPoints timings in timingPoints)
            {
                sw.WriteLine($"{timings.Offset},{timings.BPM},{timings.TimeSignature},{timings.Sample},{timings.SampleNumber},{timings.Volume},{timings.RedOrGreen},{timings.KiaiMode}");
            }
            sw.Write("\n");

            // 写入HitObjects
            sw.WriteLine("[HitObjects]");
            foreach (HitObjects objects in hitObjects)
            {
                /// <summary>
                /// XCoordinate为X坐标
                /// YCoordinate为Y坐标
                /// Offset为时间点
                /// Type为类型  1为Circle 2为Slider 12为Spinner 5为Circle New Combo 6为Slider New Combo
                /// SoundEffect为Circle类型 若为Circl则0为o 2或8为x 4为O 6或12为X  若为Slider则0为小滑条 4为大滑条
                /// SliderSomthing为Slider的一些信息  Taiko里似乎没啥用 仅Slider该成员有值
                /// SliderRepeatTimes为Slider的一些信息  Taiko里似乎没啥用 仅Slider该成员有值
                /// OffsetLength为滑条长度  仅Slider该成员有值
                /// SpinnerSample不确定  一般为0 仅Spinner该成员有值
                /// SpinnerEndOffset为转盘结束时间点  仅Spinner该成员有值
                /// UselessEnding不知道是啥  一般为"0:0:0:0:" 仅Circle和Spinner该成员有值
                /// </summary>
                if (objects.Type == 1 || objects.Type == 5)
                {
                    sw.WriteLine($"{objects.XCoordinate},{objects.YCoordinate},{objects.Offset},{objects.Type},{objects.SoundEffect},{objects.UselessEnding}");
                }
                else if (objects.Type == 2 || objects.Type == 6)
                {
                    sw.WriteLine($"{objects.XCoordinate},{objects.YCoordinate},{objects.Offset},{objects.Type},{objects.SoundEffect},{objects.SliderSomething},{objects.SliderRepeatTimes},{objects.OffsetLength}");
                }
                else if (objects.Type == 12)
                {
                    sw.WriteLine($"{objects.XCoordinate},{objects.YCoordinate},{objects.Offset},{objects.Type},{objects.SoundEffect},{objects.SpinnerEndOffset},{objects.UselessEnding}");
                }    
            }

            sw.Close();

            logger.Info("Done!");
        }

        public static void YellowNotes(StreamWriter sr, string diffName)
        {

        }
    }
}
