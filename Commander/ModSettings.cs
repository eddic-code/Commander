using UnityModManagerNet;

namespace Commander
{
    public class ModSettings : UnityModManager.ModSettings
    {
        public static UnityModManager.ModEntry ModEntry;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
