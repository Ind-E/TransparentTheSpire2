using BaseLib.Config;

namespace TransparentTheSpire.TransparentTheSpireCode;

internal class Config : SimpleModConfig
{
    public static bool SuperTransparentMode
    {
        get;
        set
        {
            field = value;
            MainFile.ApplyConfigToAllNodes();
        }
    }
}
