using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gimmickalizer
{
    public class Barlines
    {
        public static void MakeBarlines(in StreamReader sr, in string fileFullName, in Logger logger)
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
            StreamWriter sw = new(outFileFullName);

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
                TimingPoints temp = new()
                {
                    Offset = Convert.ToInt32(subs[0]),
                    BPM = subs[1],
                    TimeSignature = Convert.ToInt32(subs[2]),
                    Sample = Convert.ToInt32(subs[3]),
                    SampleNumber = Convert.ToInt32(subs[4]),
                    Volume = Convert.ToInt16(subs[5]),
                    RedOrGreen = Convert.ToInt16(subs[6]),
                    KiaiMode = Convert.ToInt16(subs[7])
                };
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
                HitObjects temp = new()
                {
                    XCoordinate = Convert.ToInt32(subs[0]),
                    YCoordinate = Convert.ToInt32(subs[1]),
                    Offset = Convert.ToInt32(subs[2]),
                    Type = Convert.ToInt16(subs[3]),
                    SoundEffect = Convert.ToInt16(subs[4])
                };
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
                    int j = General.GetLastRedLineIndex(in RedLines, in hitObjects[i].Offset);
                    int k = General.GetLastGreenLineIndex(in GreenLines, in hitObjects[i].Offset);

                    switch (hitObjects[i].SoundEffect)
                    {
                        case 0:
                        case 4:
                            {
                                // 如果为咚 插3条红线
                                if (k != -1)
                                {
                                    TimingPoints RedLine1 = new()
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
                                    TimingPoints RedLine2 = new()
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
                                    TimingPoints RedLine3 = new()
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
                                    TimingPoints RedLine1 = new()
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
                                    TimingPoints RedLine2 = new()
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
                                    TimingPoints RedLine3 = new()
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

                                break;
                            }
                        case 2:
                        case 8:
                        case 6:
                        case 12:
                            {
                                // 如果为咔 插7条红线
                                if (k != -1)
                                {
                                    TimingPoints RedLine1 = new()
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
                                    TimingPoints RedLine2 = new()
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
                                    TimingPoints RedLine3 = new()
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
                                    TimingPoints RedLine4 = new()
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
                                    TimingPoints RedLine5 = new()
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
                                    TimingPoints RedLine6 = new()
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
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    if ((hitObjects[i].SoundEffect == 0) || (hitObjects[i].SoundEffect == 4))
                    {
                        // 如果为咚 插3条红线
                        if (k != -1)
                        {
                            TimingPoints RedLine1 = new()
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
                            TimingPoints RedLine2 = new()
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
                            TimingPoints RedLine3 = new()
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
                            TimingPoints RedLine1 = new()
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
                            TimingPoints RedLine2 = new()
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
                            TimingPoints RedLine3 = new()
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
                            TimingPoints RedLine1 = new()
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
                            TimingPoints RedLine2 = new()
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
                            TimingPoints RedLine3 = new()
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
                            TimingPoints RedLine4 = new()
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
                            TimingPoints RedLine5 = new()
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
                            TimingPoints RedLine6 = new()
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
                switch (objects.Type)
                {
                    case 1:
                    case 5:
                        {
                            sw.WriteLine($"{objects.XCoordinate},{objects.YCoordinate},{objects.Offset},{objects.Type},{objects.SoundEffect},{objects.UselessEnding}");
                            break;
                        }
                    case 2:
                    case 6:
                        {
                            sw.WriteLine($"{objects.XCoordinate},{objects.YCoordinate},{objects.Offset},{objects.Type},{objects.SoundEffect},{objects.SliderSomething},{objects.SliderRepeatTimes},{objects.OffsetLength}");
                            break;
                        }
                    case 12:
                        {
                            sw.WriteLine($"{objects.XCoordinate},{objects.YCoordinate},{objects.Offset},{objects.Type},{objects.SoundEffect},{objects.SpinnerEndOffset},{objects.UselessEnding}");
                            break;
                        }
                }
            }

            sw.Close();

            logger.Info("Done!");
        }
    }
}
