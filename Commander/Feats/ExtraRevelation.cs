using Kingmaker.Blueprints.Classes.Selection;

namespace Commander.Feats
{
    internal static class ExtraRevelation 
    {
        public static void Create() 
        {
            var oracleRevelationSelection = Resources.GetBlueprint<BlueprintFeatureSelection>("60008a10ad7ad6543b1f63016741a5d2");

            var extraRevelation = FeatTools.CreateExtraSelectionFeat("ExtraRevelation", "1bd5150f5b254b6db6eda8bb8a13397c", oracleRevelationSelection, bp => 
            {
                bp.SetName("Extra Revelation");
                bp.SetDescription("You gain one additional revelation. You must meet all of the prerequisites for this revelation." +
                                  "\nYou can gain Extra Revelation multiple times.");
            });

            FeatTools.AddAsFeat(extraRevelation);
        }
    }
}
