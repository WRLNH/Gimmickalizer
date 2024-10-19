using System.Collections.Generic;

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

    public class General
    {
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
    }
}
