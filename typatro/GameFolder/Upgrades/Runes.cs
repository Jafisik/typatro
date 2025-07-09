using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace typatro.GameFolder.Runes{

    public enum Runes{
        [Display(Description = "\"Strength of will\"\n\nAdds +1 to\nall letters", Prompt = "First unlock")]
        Uruz,
        [Display(Description = "\"Harvest or Reward\"\n\nStart with\n80 coins", Prompt = "Reach\n200 coins")]
        Jera,
        [Display(Description = "\"Need or desire\"\n\nStart with\n10% bloom\nword chance", Prompt = "Defeat\nfirst boss")]
        Naudhiz,
        [Display(Description = "\"Destruction or chaos\"\n\nHalf are +10\nother half\nis -5", Prompt = "Get negative\nletter score")]
        Halagaz,
    }



}