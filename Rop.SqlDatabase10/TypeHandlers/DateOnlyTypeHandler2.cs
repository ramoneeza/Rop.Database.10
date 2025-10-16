using System.Data;

namespace Rop.Database10.TypeHandlers
{
    public class DateOnlyTypeHandler2 : SqlMapper.TypeHandler<DateOnly?>
    {
        public override DateOnly? Parse(object? value)
        {
            return value is null ? null : DateOnly.FromDateTime((DateTime)value);
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.Value = value?.ToDateTime(TimeOnly.MinValue);
        }
    }
}