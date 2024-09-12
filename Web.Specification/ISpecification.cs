using System.Linq.Expressions;

namespace Web.Specification
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ItemsCriteria { get; }
    }
}
