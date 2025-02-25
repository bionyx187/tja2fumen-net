using NAudio.Wave;
using NVorbis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tja2fumen
{

    struct FumenSection
    {
        public string hexStringNotes;
        public int noteCount;
    }

    public class FumenCSV
    {

        static readonly string[] BRANCH_NAMES_JP = new string[] { "普通", "玄人", "達人" }; //Normal, Professional, Master

        static readonly Dictionary<string, string> DIFF_NAMES_JP = new Dictionary<string, string> { 
            { "Easy", "かんたん" },
            { "Normal", "ふつう" },
            { "Hard", "難しい" },
            { "Oni", "鬼" },
            { "Ura", "裏鬼" },
        };

        const string CSV_HEADER = "ID,名前,分岐,難易度,分割数,チャプター1,チャプター2,チャプター3,チャプター4,チャプター5,チャプター6,チャプター7,チャプター8,チャプター9,チャプター10,BPM変化1,BPM変化2,BPM変化3,BPM変化4,BPM変化5,音符データ1,音符データ2,音符データ3,音符データ4,音符データ5,音符データ6,音符データ7,音符データ8,音符データ9,音符データ10\n";

        static int GetModeBPM(FumenCourse course)
        {
            return course.measures.GroupBy(measure => (int)measure.bpm).OrderByDescending(bpm => bpm.Count()).First().Key;
        }

        static List<FumenNote> GetNotes(FumenCourse course, string branch) 
        {
            List<FumenNote> notes = new List<FumenNote>();
            foreach(var measure in course.measures)
            {
                foreach(var note in measure.branches[branch].notes)
                {
                    // posAbs is only calculated for do/ka notes, so we calculate it again just in case
                    note.posAbs = (measure.offsetStart + note.pos + (4 * 60_000 / measure.bpm));
                    notes.Add(note);
                }
            }
            
            return notes;
        }


        static int GetSongDuration(string wavePath)
        {
            if (!File.Exists(wavePath))
            {
                return 0;
            }
            if (wavePath.EndsWith(".ogg"))
            {
                var vorbisReader = new VorbisReader(wavePath);
                return (int)vorbisReader.TotalTime.TotalMilliseconds;
            }
            
            var audioReader = new AudioFileReader(wavePath);
            return (int)audioReader.TotalTime.TotalMilliseconds;
        }

        static string GenerateHexString(FumenNote note, string diff)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write(Constants.FUMEN_TYPE_NOTES[note.noteType]);
            binaryWriter.Write(Constants.FUMEN_TYPE_NOTES[note.noteType]);
            binaryWriter.Write(note.posAbs);
            binaryWriter.Write(note.duration);
            binaryWriter.Write((float)Constants.TIMING_WINDOWS[diff].Good);
            binaryWriter.Write((float)Constants.TIMING_WINDOWS[diff].Ok);
            binaryWriter.Write((float)Constants.TIMING_WINDOWS[diff].Bad);
            binaryWriter.Write(note.hits);
            
            binaryWriter.Close();
            return Convert.ToHexString(memoryStream.ToArray()) + "/";
        }

        static string GenerateCsvLine(List<FumenNote> notes, string songId, int branch, int sectionNumber, string diff, float songLength, int bpm)
        {
            float divisonLength = songLength / 5;

            FumenSection[] sections = new FumenSection[5];
            FumenSection currentSection = new FumenSection();

            int currentDivision = 0;
            float currentDivisionEnd = divisonLength;

            foreach (var note in notes) 
            { 
                if(note.posAbs > currentDivisionEnd && currentDivision < 4)
                {
                    sections[currentDivision] = currentSection;

                    currentDivision++;
                    currentDivisionEnd = divisonLength * (currentDivision + 1);

                    currentSection = new FumenSection();
                }

                currentSection.hexStringNotes += GenerateHexString(note, diff);
                currentSection.noteCount++;
            }

            // Save last section
            sections[currentDivision] = currentSection;

            string csvLine = string.Join(',', new string[]
            {
                sectionNumber.ToString(), songId, BRANCH_NAMES_JP[branch], DIFF_NAMES_JP[diff], "5",
                $"時間: {(divisonLength * 1 / 1000).ToString("F5", CultureInfo.InvariantCulture)} 音符数: {sections[0].noteCount}",
                $"時間: {(divisonLength * 2 / 1000).ToString("F5", CultureInfo.InvariantCulture)} 音符数: {sections[1].noteCount}",
                $"時間: {(divisonLength * 3 / 1000).ToString("F5", CultureInfo.InvariantCulture)} 音符数: {sections[2].noteCount}",
                $"時間: {(divisonLength * 4 / 1000).ToString("F5", CultureInfo.InvariantCulture)} 音符数: {sections[3].noteCount}",
                $"時間: {(divisonLength * 5 / 1000).ToString("F5", CultureInfo.InvariantCulture)} 音符数: {sections[4].noteCount}",
                "", "", "", "", "",
                $"変化値: {bpm} 時間: 0", "", "", "", "",
                sections[0].hexStringNotes, sections[1].hexStringNotes, sections[2].hexStringNotes, sections[3].hexStringNotes,
                sections[4].hexStringNotes, "", "", "", "", ""
            }
            ) + Environment.NewLine;
            
            

            return csvLine;
        }

        public static string GenerateCsv(List<FumenCourse> courses, string songId, string wavePath)
        {
            string csv = "";
            
            SortedDictionary<int, string> csvLines = new SortedDictionary<int, string>();

            int songLenght = GetSongDuration(wavePath);

            int bpm = GetModeBPM(courses[0]);
            csv += CSV_HEADER;

            int step = 1;
            int diffCount = courses.Count;

            foreach(var course in courses)
            {
                string diff = course.diff;
                if (course.hasBranches)
                {
                    var normalNotes = GetNotes(course, Constants.BRANCH_NAMES[0]);
                    var proNotes = GetNotes(course, Constants.BRANCH_NAMES[1]);
                    var masterNotes = GetNotes(course, Constants.BRANCH_NAMES[2]);

                    csvLines.Add(step, GenerateCsvLine(normalNotes, songId, 0, step, diff, songLenght, bpm));
                    csvLines.Add(step + diffCount, GenerateCsvLine(proNotes, songId, 1, step + diffCount, diff, songLenght, bpm));
                    csvLines.Add(step + diffCount * 2, GenerateCsvLine(masterNotes, songId, 2, step + diffCount * 2, diff, songLenght, bpm));
                }
                else
                {
                    var normalNotes = GetNotes(course, Constants.BRANCH_NAMES[0]);

                    csvLines.Add(step, GenerateCsvLine(normalNotes, songId, 0, step, diff, songLenght, bpm));
                    csvLines.Add(step + diffCount, GenerateCsvLine(normalNotes, songId, 1, step + diffCount, diff, songLenght, bpm));
                    csvLines.Add(step + diffCount * 2, GenerateCsvLine(normalNotes, songId, 2, step + diffCount * 2, diff, songLenght, bpm));
                }
                step++;
            }
            foreach (KeyValuePair<int, string> pair in csvLines)
            {
                csv += pair.Value;
            }

            return csv;
        }

    }
}
