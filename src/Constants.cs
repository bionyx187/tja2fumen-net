namespace tja2fumen
{
    public static class Constants
    {
        public static readonly string[] BRANCH_NAMES = { "normal", "professional", "master" };

        public static readonly Dictionary<string, string> TJA_NOTE_TYPES = new Dictionary<string, string> {
            { "0", "Blank" },    { "1", "Don" },      { "2", "Ka" },       { "3", "DON" },      { "4", "KA" },
            { "5", "Drumroll" }, { "6", "DRUMROLL" }, { "7", "Balloon" },  { "8", "EndDRB" },   { "9", "Kusudama" },
            { "A", "DON2" },     { "B", "KA2" },      { "C", "Blank" },    { "D", "Drumroll" }, { "E", "DON2" },
            { "F", "Ka" },       { "G", "KA2" },      { "H", "DRUMROLL" }, { "I", "Drumroll" },
        };

        public static readonly Dictionary<int, string> SENOTECHANGE_TYPES = new Dictionary<int, string> {
            { 1, "Don" }, { 2, "Don2" }, { 3, "Don3" }, { 4, "Ka" }, { 5, "Ka2" },
        };

        public static readonly Dictionary<int, string> FUMEN_NOTE_TYPES = new Dictionary<int, string> {
            { 0x1, "Don" },        { 0x2, "Don2" },       { 0x3, "Don3" },       { 0x4, "Ka" },
            { 0x5, "Ka2" },        { 0x6, "Drumroll" },   { 0x7, "DON" },        { 0x8, "KA" },
            { 0x9, "DRUMROLL" },   { 0xa, "Balloon" },    { 0xb, "DON2" },       { 0xc, "Kusudama" },
            { 0xd, "KA2" },        { 0xe, "Unknown1" },   { 0xf, "Unknown2" },   { 0x10, "Unknown3" },
            { 0x11, "Unknown4" },  { 0x12, "Unknown5" },  { 0x13, "Unknown6" },  { 0x14, "Unknown7" },
            { 0x15, "Unknown8" },  { 0x16, "Unknown9" },  { 0x17, "Unknown10" }, { 0x18, "Unknown11" },
            { 0x19, "Unknown12" }, { 0x22, "Unknown13" }, { 0x62, "Drumroll2" }
        };

        public static readonly Dictionary<string, int> FUMEN_TYPE_NOTES =
            FUMEN_NOTE_TYPES.ToDictionary(x => x.Value, x => x.Key);

        public static readonly Dictionary<string, string> NORMALIZE_COURSE =
            new Dictionary<string, string> { { "0", "Easy" },        { "Easy", "Easy" }, { "1", "Normal" },
                                             { "Normal", "Normal" }, { "2", "Hard" },    { "Hard", "Hard" },
                                             { "3", "Oni" },         { "Oni", "Oni" },   { "4", "Ura" },
                                             { "Ura", "Ura" },       { "Edit", "Ura" } };

        public static readonly List<string> COURSE_NAMES = NORMALIZE_COURSE.Values.Distinct().ToList();

        public static readonly List<string> TJA_COURSE_NAMES =
            COURSE_NAMES.SelectMany(difficulty => new[] { "", "P1", "P2" }.Select(player => difficulty + player))
                .ToList();

        public static readonly Dictionary<string, string> COURSE_IDS = new Dictionary<string, string> {
            { "Easy", "e" }, { "Normal", "n" }, { "Hard", "h" }, { "Oni", "m" }, { "Ura", "x" }
        };


        public static readonly Dictionary<string, string> COURSE_IDS_REVERSE = new Dictionary<string, string> {
            { "e", "Easy" }, { "n", "Normal" }, { "h", "Hard" }, { "m", "Oni" }, { "x", "Ura" }
        };

        public static readonly Dictionary<string, (double Good, double Ok, double Bad)> TIMING_WINDOWS =
            new Dictionary<string, (double, double, double)> {
                { "Easy", (41.7083358764648, 108.441665649414, 125.125000000000) },
                { "Normal", (41.7083358764648, 108.441665649414, 125.125000000000) },
                { "Hard", (25.0250015258789, 075.075004577637, 108.441665649414) },
                { "Oni", (25.0250015258789, 075.075004577637, 108.441665649414) },
                { "Ura", (25.0250015258789, 075.075004577637, 108.441665649414) },
            };
    }

}
