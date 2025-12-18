using Robust.Shared.Serialization;

namespace Content.Shared.Eye
{
    [Flags]
    [FlagsFor(typeof(VisibilityMaskLayer))]
    public enum VisibilityFlags : int
    {
        None = 0,
        Normal = 1 << 0,
        Ghost  = 1 << 1,
        Subfloor = 1 << 2,
        Admin = 1 << 3, // Reserved for admins in stealth mode and admin tools.
        PsionicInvisibility = 1 << 4, //Nyano - Summary: adds Psionic Invisibility as a visibility layer. Currently does nothing. 
        NullSpace = 1 << 5, // Starlight
    }
}
    

