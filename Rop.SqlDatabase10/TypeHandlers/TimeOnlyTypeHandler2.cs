using System.Data;

namespace Rop.Database10.TypeHandlers
{
    public class TimeOnlyTypeHandler2 : SqlMapper.TypeHandler<TimeOnly?>
    {
        public override TimeOnly? Parse(object? value)
        {
            return value is null ? null : TimeOnly.FromTimeSpan((TimeSpan)value);
        }

        public override void SetValue(IDbDataParameter parameter, TimeOnly? value)
        {
            parameter.Value = value?.ToTimeSpan();
        }
    }
}