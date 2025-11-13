namespace MovieHall.Helpers
{
    public class OptionHelper
    {
        // ---------------------- Anime ----------------------

        public static List<string> ViewDubOptions => new()
        {
            { "ALL" },
            { "DUB" },
            { "SUB" }
        };

        public static List<string> ViewLangOptions => new()
        {
            { "ALL" },
            { "JAPAN" },
            { "CHINA" },
            { "KOREA" }
        };

        public static Dictionary<int, string> TopOptions => new()
        {
            { 1, "Top 1" },
            { 2, "Top 2" },
            { 3, "Top 3" },
            { 4, "Top 4" },
            { 5, "Top 5" },
            { 6, "Top 6" },
            { 7, "Top 7" },
            { 8, "Top 8" },
            { 9, "Top 9" },
            { 10, "Top 10" }
        };

        public static Dictionary<string, string> CountryOptions => new()
        {
            { "", "Land wählen..." },
            { "Japan", "Japan" },
            { "China", "China" },
            { "Korea", "Korea" }
        };

        public static List<string> LanguageOptions => new()
        {
            { "Englisch" },
            { "Japanisch" },
            { "Chinesisch" },
            { "Deutsch" }
        };

        public static Dictionary<int, string> MonthsOptions => new()
        {
            { 1, "Januar" },
            { 2, "Februar" },
            { 3, "März" },
            { 4, "April" },
            { 5, "Mai" },
            { 6, "Juni" },
            { 7, "Juli" },
            { 8, "August" },
            { 9, "September" },
            { 10, "Oktober" },
            { 11, "November" },
            { 12, "Dezember" }
        };


        // ---------------------- Movie ----------------------

        public static List<string> ViewFSKOptions => new()
        {
            { "All" },
            { "0" },
            { "6" },
            { "12" },
            { "16" },
            { "18" },
            { "Favorit" }
        };

        public static Dictionary<int, string> FSKOptions => new()
        {
            { 0, "0+" },
            { 6, "6+" },
            { 12, "12+" },
            { 16, "16+" },
            { 18, "18+" }
        };

        public static List<string> MovielangOptions => new()
        {
            { "Deutsch"},
            { "Englisch"}
        };

        // ---------------------- Both ----------------------

        public static Dictionary<int, string> BuyOptions => new()
        {
            { 0, "Noch nicht" },
            { 1, "DVD" },
            { 2, "Blu-ray" },
            { 3, "3D" }
        };

        public static int CurrentYear => DateTime.Now.Year;
        public static int StartYear => 1980;

    }
}
