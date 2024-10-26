#if UMM
using UnityEngine;
using UnityModManagerNet;

namespace PowerMeter;

public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Header("General")]
    [Draw("Show usage in Nav Display")]
    public bool showInGUI = true;

    [Draw("Show usage in Power Overlay")]
    public bool showInOverlay = true;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
        Patch.showInGUI = showInGUI;
        Patch.showInOverlay = showInOverlay;
    }
}
#endif
