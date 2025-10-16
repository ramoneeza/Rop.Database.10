namespace Rop.Database10.TypeHandlers;

public static class NewTypeHandlers
{
    private static bool _registered = false;

    public static void Ensure()
    {
        if (_registered) return;
        _registered = true;
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler2());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler2());
    }
}