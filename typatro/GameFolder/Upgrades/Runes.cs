using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace typatro.GameFolder.Runes{

    public enum Runes{
        [Display(Description = "Strength\nof will\n\nAdds +1 to\nall letters", Prompt = "First unlock")]
        Uruz,
        [Display(Description = "Destruction\nor chaos\n\nHalf is +5\nother half -3", Prompt = "Get negative\nletter score")]
        Halagaz,
        [Display(Description = "Need\nor desire\n\nStart with\n50 score", Prompt = "Reach\n100 coins")]
        Naudhiz,
        [Display(Description = "Harvest\nor Reward\n\nStart with\n60 coins", Prompt = "Reach\n100 coins")]
        Jera,
    }



}