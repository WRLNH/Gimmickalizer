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

namespace Gimmickalizer
{
    public class TimingPonits
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
        /// Type为类型  1为Circle 2为Slider 12为Spinner
        /// DonOrKat为Circle类型 0为o 2或8为x 4为O 6或12为X
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
        public short DonOrKat;
        public string SliderSomething;
        public int SliderRepeatTimes;
        public int OffsetLength;
        public int SpinnerSample;
        public int SpinnerEndOffset;
        public string UselessEnding;
    }

    public class Gimmickalize
    {
        public static string GetDiffName(string Name)
        {
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

        public static void Barlines(StreamReader sr, string diffName, Logger logger)
        {
            string line;
            string flag = "";
            List<string> readLines = [];
            List<string> outLines = [];
            List<string> strLstTimingPoints = [];
            List<string> strLstHitObjects = [];
            List<TimingPonits> timingPonits = [];
            List<HitObjects> hitObjects = [];

            while((line = sr.ReadLine()) != null)
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
                        readLines.Add(line);
                        outLines.Add(line + "Barlines");
                        continue;
                    }
                }

                if (line == "[TimingPoints]")
                {
                    readLines.Add(line);
                    outLines.Add(line);
                    flag = "TimingPoints";
                    continue;
                }
                else if (line == "[HitObjects]")
                {
                    readLines.Add(line);
                    outLines.Add(line);
                    flag = "HitObjects";
                    continue;
                }

                if (flag == "TimingPoints" && line != "")
                {
                    readLines.Add(line);
                    outLines.Add(line);
                    strLstTimingPoints.Add(line);
                    continue;
                }
                else if (flag == "HitObjects" && line != "")
                {
                    readLines.Add(line);
                    outLines.Add(line);
                    strLstHitObjects.Add(line);
                    continue;
                }

                readLines.Add(line);
                outLines.Add(line);
            }

            for (int i = 0; i < strLstTimingPoints.Count; i++)
            {
                string[] subs = strLstTimingPoints[i].Split(',');
                TimingPonits temp = new TimingPonits();
                temp.Offset = Convert.ToInt32(subs[0]);
                temp.BPM = subs[1];
                temp.TimeSignature = Convert.ToInt32(subs[2]);
                temp.Sample = Convert.ToInt32(subs[3]);
                temp.SampleNumber = Convert.ToInt32(subs[4]);
                temp.Volume = Convert.ToInt16(subs[5]);
                temp.RedOrGreen = Convert.ToInt16(subs[6]);
                temp.KiaiMode = Convert.ToInt16(subs[7]);
                timingPonits.Add(temp);
            }

            for (int i = 0; i < timingPonits.Count; i++)
            {
                Debug.WriteLine(timingPonits[i].Offset.ToString() + ',' + timingPonits[i].BPM + ',' + timingPonits[i].TimeSignature.ToString() + ',' + timingPonits[i].Sample.ToString() + ',' + timingPonits[i].SampleNumber.ToString() + ',' + timingPonits[i].Volume.ToString() + ',' + timingPonits[i].RedOrGreen.ToString() + ',' + timingPonits[i].KiaiMode.ToString());
            }
            logger.Info("Done!");
        }

        public static void YellowNotes(StreamWriter sr, string diffName)
        {

        }
    }
}
