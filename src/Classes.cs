using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tja2fumen
{
    public struct TJAData
    {
        public string name;
        public string value;
        public Int32 pos;
    }

    public class TJAMeasure
    {
        public List<string> notes = new List<string>();
        public List<TJAData> events = new List<TJAData>();
        public List<TJAData> combined = new List<TJAData>();
    }

    public class TJACourse
    {
        public float bpm;
        public float offset;
        public string course;
        public Int32 level;
        public List<Int32> balloon = new List<Int32>();
        public Int32 scoreInit;
        public Int32 scoreDiff;
        public List<string> data = new List<string>();
        public Dictionary<string, List<TJAMeasure>> branches;
    }

    public class TJASong
    {
        public float bpm;
        public float offset;
        public Dictionary<string, TJACourse> courses = new Dictionary<string, TJACourse>();
    }

    public class TJAMeasureProcessed
    {
        public float bpm;
        public float scroll;
        public bool gogo;
        public bool barline;
        public List<Int32> timeSignature = new List<int>();
        public Int32 subDivisions;
        public Int32 posStart;
        public Int32 posEnd;
        public float delay;
        public bool section;
        public bool levelHold;
        public string seNote;
        public string branchType;
        public (float, float) branchCond;
        public List<TJAData> notes = new List<TJAData>();
    }

    public class FumenNote
    {
        public string noteType;
        public float pos;
        public float posAbs;
        public int diff;
        public int scoreInit;
        public int scoreDiff;
        public float padding;
        public int item;
        public float duration;
        public bool multiMeasure;
        public int hits;
        public int hitsPadding;
        public byte[] drumrollBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
        public bool manuallySet;
    }

    public class FumenBranch
    {
        public int length;
        public float speed;
        public int padding;
        public List<FumenNote> notes = new List<FumenNote>();
    }

    public class FumenMeasure
    {
        public float bpm;
        public float offsetStart;
        public float offsetEnd;
        public float duration;
        public bool gogo = false;
        public bool barline = true;
        public List<int> branchInfo = new List<int>();
        public Dictionary<string, FumenBranch> branches = new Dictionary<string, FumenBranch>();
        public int padding1;
        public int padding2;

        public FumenMeasure()
        {
            foreach (string branchName in Constants.BRANCH_NAMES)
            {
                branches[branchName] = new FumenBranch();
            }

            for (int i = 0; i < 6; i++)
            {
                branchInfo.Add(-1);
            }
        }

        public void SetDuration(List<int> timeSig, int measureLength, int subDivisions)
        {
            float fullDuration = 4 * 60000 / this.bpm;

            float measureSize = timeSig[0] / timeSig[1];
            float measureRatio = subDivisions == 0.0f ? 1.0f : measureLength / subDivisions;
            this.duration = fullDuration * measureSize * measureRatio;
        }

        public void setFirstMsOffsets(float songOffset)
        {
            this.offsetStart = songOffset * -1 * 1000;
            this.offsetStart -= (4 * 60000 / this.bpm);
            this.offsetEnd = this.offsetStart + this.duration;
        }

        public void setMsOffsets(float delay, FumenMeasure prevMeasure)
        {
            this.offsetStart = prevMeasure.offsetEnd;

            this.offsetStart += delay;

            this.offsetStart += (4 * 60000 / prevMeasure.bpm);

            this.offsetStart -= (4 * 60000 / this.bpm);

            this.offsetEnd = this.offsetStart + this.duration;
        }

        public void setBranchInfo(string branchType, (float, float)branchCond, int branchPointsTotal,
                                  string currentBranch, bool hasLevehold)
        {
            if (hasLevehold)
            {
                switch (currentBranch)
                {
                case "normal":
                    this.branchInfo[0] = 999;
                    this.branchInfo[1] = 999;
                    break;
                case "professional":
                    this.branchInfo[2] = 0;
                    this.branchInfo[3] = 999;
                    break;
                case "master":
                    this.branchInfo[4] = 0;
                    this.branchInfo[5] = 0;
                    break;
                }
            }
            else if (branchType == "p")
            {
                List<int> vals = new List<int>();

                if (0 < branchCond.Item1 && branchCond.Item1 <= 1)
                {
                    vals.Add((int)(branchPointsTotal * branchCond.Item1));
                }
                else if (branchCond.Item1 > 1)
                {
                    vals.Add(999);
                }
                else
                {
                    vals.Add(0);
                }

                switch (currentBranch)
                {
                case "normal":
                    this.branchInfo[0] = vals[0];
                    this.branchInfo[1] = vals[1];
                    break;
                case "professional":
                    this.branchInfo[2] = vals[0];
                    this.branchInfo[3] = vals[1];
                    break;
                case "master":
                    this.branchInfo[4] = vals[0];
                    this.branchInfo[5] = vals[1];
                    break;
                }
            }
            else if (branchType == "r")
            {
                List<int> vals = new List<int>(new int[] { (int)branchCond.Item1, (int)branchCond.Item2 });
                switch (currentBranch)
                {
                case "normal":
                    this.branchInfo[0] = vals[0];
                    this.branchInfo[1] = vals[1];
                    break;
                case "professional":
                    this.branchInfo[2] = vals[0];
                    this.branchInfo[3] = vals[1];
                    break;
                case "master":
                    this.branchInfo[4] = vals[0];
                    this.branchInfo[5] = vals[1];
                    break;
                }
            }
        }
    }

    public class FumenHeader
    {
        public string order = "<";
        public float[] b000_b431_timing_windows = new float[108];
        public int b432_b435_has_branches = 0;
        public int b436_b439_hp_max = 10000;
        public int b440_b443_hp_clear = 8000;
        public int b444_b447_hp_gain_good = 10;
        public int b448_b451_hp_gain_ok = 5;
        public int b452_b455_hp_loss_bad = -20;
        public int b456_b459_normal_normal_ratio = 65536;
        public int b460_b463_normal_professional_ratio = 65536;
        public int b464_b467_normal_master_ratio = 65536;
        public int b468_b471_branch_pts_good = 20;
        public int b472_b475_branch_pts_ok = 10;
        public int b476_b479_branch_pts_bad = 0;
        public int b480_b483_branch_pts_drumroll = 1;
        public int b484_b487_branch_pts_good_big = 20;
        public int b488_b491_branch_pts_ok_big = 10;
        public int b492_b495_branch_pts_drumroll_big = 1;
        public int b496_b499_branch_pts_balloon = 30;
        public int b500_b503_branch_pts_kusudama = 30;
        public int b504_b507_branch_pts_unknown = 20;
        public int b508_b511_dummy_data = 12345678;
        public int b512_b515_number_of_measures = 0;
        public int b516_b519_unknown_data = 0;

        private void _parse_order(byte[] rawBytes)
        {
            order = "";
            byte[] measures = new byte[4];
            measures[0] = rawBytes[512];
            measures[1] = rawBytes[513];
            measures[2] = rawBytes[514];
            measures[3] = rawBytes[515];

            uint little = BinaryPrimitives.ReadUInt32LittleEndian(measures);
            uint big = BinaryPrimitives.ReadUInt32BigEndian(measures);
            if (big > little)
            {
                this.order = ">";
            }
            else
            {
                this.order = "<";
            }
        }

        public void ParseHeaderValues(byte[] rawBytes)
        {
            _parse_order(rawBytes);
            bool isLittleEndian = this.order == "<";
            var stream = new MemoryStream(rawBytes);

            var asd = new MyBinaryReader(stream);
            asd.isLittleEndian = isLittleEndian;

            for (int i = 0; i < 36; i++)
            {

                b000_b431_timing_windows[i] = asd.ReadSingle();
            }
            b432_b435_has_branches = asd.ReadInt32();
            b436_b439_hp_max = asd.ReadInt32();
            b440_b443_hp_clear = asd.ReadInt32();
            b444_b447_hp_gain_good = asd.ReadInt32();
            b448_b451_hp_gain_ok = asd.ReadInt32();
            b452_b455_hp_loss_bad = asd.ReadInt32();
            b456_b459_normal_normal_ratio = asd.ReadInt32();
            b460_b463_normal_professional_ratio = asd.ReadInt32();
            b464_b467_normal_master_ratio = asd.ReadInt32();
            b468_b471_branch_pts_good = asd.ReadInt32();
            b472_b475_branch_pts_ok = asd.ReadInt32();
            b476_b479_branch_pts_bad = asd.ReadInt32();
            b480_b483_branch_pts_drumroll = asd.ReadInt32();
            b484_b487_branch_pts_good_big = asd.ReadInt32();
            b488_b491_branch_pts_ok_big = asd.ReadInt32();
            b492_b495_branch_pts_drumroll_big = asd.ReadInt32();
            b496_b499_branch_pts_balloon = asd.ReadInt32();
            b500_b503_branch_pts_kusudama = asd.ReadInt32();
            b504_b507_branch_pts_unknown = asd.ReadInt32();
            b508_b511_dummy_data = asd.ReadInt32();
            b512_b515_number_of_measures = asd.ReadInt32();
            b516_b519_unknown_data = asd.ReadInt32();
        }

        public void setTimingWindows(string difficulty)
        {
            if (difficulty == "Ura" || difficulty == "Edit")
            {
                difficulty = "Oni";
            }
            for (int i = 0; i < 36; i++)
            {

                this.b000_b431_timing_windows[i * 3] = (float)Constants.TIMING_WINDOWS[difficulty].Good;
                this.b000_b431_timing_windows[i * 3 + 1] = (float)Constants.TIMING_WINDOWS[difficulty].Ok;
                this.b000_b431_timing_windows[i * 3 + 2] = (float)Constants.TIMING_WINDOWS[difficulty].Bad;
            }
        }

        public void SetHpBytes(int nNotes, string difficulty, int stars)
        {
            if (difficulty == "Ura" || difficulty == "Edit")
            {
                difficulty = "Oni";
            }

            this.GetHpFromLookupTables(nNotes, difficulty, stars);
            switch (difficulty)
            {
            case "Easy":
                this.b440_b443_hp_clear = 6000;
                break;
            case "Normal":
            case "Hard":
                this.b440_b443_hp_clear = 7000;
                break;
            case "Oni":
                this.b440_b443_hp_clear = 8000;
                break;
            }
        }

        private void GetHpFromLookupTables(int nNotes, string difficulty, int stars)
        {
            if (!(0 < nNotes) && !(nNotes <= 2500))
            {
                return;
            }

            var starToKey = new Dictionary<string, Dictionary<int, string>> {
                { "Oni", new Dictionary<int, string> { { 1, "17" },
                                                       { 2, "17" },
                                                       { 3, "17" },
                                                       { 4, "17" },
                                                       { 5, "17" },
                                                       { 6, "17" },
                                                       { 7, "17" },
                                                       { 8, "8" },
                                                       { 9, "910" },
                                                       { 10, "910" } } },
                { "Hard", new Dictionary<int, string> { { 1, "12" },
                                                        { 2, "12" },
                                                        { 3, "3" },
                                                        { 4, "4" },
                                                        { 5, "58" },
                                                        { 6, "58" },
                                                        { 7, "58" },
                                                        { 8, "58" },
                                                        { 9, "58" },
                                                        { 10, "58" } } },
                { "Normal", new Dictionary<int, string> { { 1, "12" },
                                                          { 2, "12" },
                                                          { 3, "3" },
                                                          { 4, "4" },
                                                          { 5, "57" },
                                                          { 6, "57" },
                                                          { 7, "57" },
                                                          { 8, "57" },
                                                          { 9, "57" },
                                                          { 10, "57" } } },
                { "Easy", new Dictionary<int, string> { { 1, "1" },
                                                        { 2, "23" },
                                                        { 3, "23" },
                                                        { 4, "45" },
                                                        { 5, "45" },
                                                        { 6, "45" },
                                                        { 7, "45" },
                                                        { 8, "45" },
                                                        { 9, "45" },
                                                        { 10, "45" } } }
            };

            var key = $"{difficulty}-{starToKey[difficulty][stars]}";
            for (int i = 0; i < HpValues.hpValues.Length; i++)
            {
                if (i + 1 == nNotes)
                {
                    this.b444_b447_hp_gain_good = HpValues.hpValues[i][HpValues.nameToHpValueIndex[$"good_{key}"]];
                    this.b448_b451_hp_gain_ok = HpValues.hpValues[i][HpValues.nameToHpValueIndex[$"ok_{key}"]];
                    this.b452_b455_hp_loss_bad = HpValues.hpValues[i][HpValues.nameToHpValueIndex[$"bad_{key}"]];
                    break;
                }
            }
        }

        public byte[] rawBytes
        {
            get {
                MemoryStream bytes = new MemoryStream();

                using (BinaryWriter bw = new BinaryWriter(bytes))
                {

                    foreach (float timingWindow in this.b000_b431_timing_windows)
                    {
                        bw.Write(timingWindow);
                    }
                    bw.Write(b432_b435_has_branches);
                    bw.Write(b436_b439_hp_max);
                    bw.Write(b440_b443_hp_clear);
                    bw.Write(b444_b447_hp_gain_good);
                    bw.Write(b448_b451_hp_gain_ok);
                    bw.Write(b452_b455_hp_loss_bad);
                    bw.Write(b456_b459_normal_normal_ratio);
                    bw.Write(b460_b463_normal_professional_ratio);
                    bw.Write(b464_b467_normal_master_ratio);
                    bw.Write(b468_b471_branch_pts_good);
                    bw.Write(b472_b475_branch_pts_ok);
                    bw.Write(b476_b479_branch_pts_bad);
                    bw.Write(b480_b483_branch_pts_drumroll);
                    bw.Write(b484_b487_branch_pts_good_big);
                    bw.Write(b488_b491_branch_pts_ok_big);
                    bw.Write(b492_b495_branch_pts_drumroll_big);
                    bw.Write(b496_b499_branch_pts_balloon);
                    bw.Write(b500_b503_branch_pts_kusudama);
                    bw.Write(b504_b507_branch_pts_unknown);
                    bw.Write(b508_b511_dummy_data);
                    bw.Write(b512_b515_number_of_measures);
                    bw.Write(b516_b519_unknown_data);
                }
                bytes.Flush();
                return bytes.ToArray();
            }
        }
    }

    public class FumenCourse
    {
        public FumenHeader header = new FumenHeader();
        public List<FumenMeasure> measures = new List<FumenMeasure>();
        public int scoreInit = 0;
        public int scoreDiff = 0;
    }

}
