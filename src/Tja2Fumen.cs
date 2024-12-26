namespace tja2fumen
{
    public class Tja2Fumen
    {
        static void Main()
        {

            TJASong asd =
                Parsers.ParseTja("C:\\taikoReverseEngineering\\tja2fumen\\com.fluto.takotako\\Ready to\\Ready to.tja");
            FumenCourse course = Converters.ConvertTjaToFumen(asd.courses["Easy"]);
            var bytes = Writer.getFumenBytes(course);
            File.WriteAllBytes("E:\\Programacion\\Python\\tja2fumen\\readyto-cs.bin", bytes);
        }
    }
}
