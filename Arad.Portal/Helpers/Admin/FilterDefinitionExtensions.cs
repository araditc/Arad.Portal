namespace Arad.Portal.Helpers.Admin;

public static class FilterDefinitionExtensions
{
    //public static Expression<Func<T, bool>> ToExpression<T>(this FilterDefinition<T> filter)
    //{
    //    var parameter = Expression.Parameter(typeof(T), "x");
    //    var body = new MongoFilterVisitor(parameter).Visit(filter.Render(
    //        BsonSerializer.SerializerRegistry.GetSerializer<T>(),
    //        BsonSerializer.SerializerRegistry));

    //    return Expression.Lambda<Func<T, bool>>(body, parameter);
    //}

    //private class MongoFilterVisitor : System.Linq.Expressions.ExpressionVisitor
    //{
    //    private readonly ParameterExpression _parameter;

    //    public MongoFilterVisitor(ParameterExpression parameter)
    //    {
    //        _parameter = parameter;
    //    }

    //    public Expression Visit(FilterDefinition filter)
    //    {
    //        if (filter is FilterDefinition<T> typedFilter)
    //        {
    //            return Visit(typedFilter);
    //        }

    //        throw new NotSupportedException($"Unsupported filter type: {filter.GetType()}");
    //    }

    //    // Implement the specific visits for different filter types here.
    //    // This example assumes equality check. Extend this for other types like AND, OR, etc.
    //    protected override Expression VisitBinary(BinaryExpression node)
    //    {
    //        return Expression.MakeBinary(node.NodeType, Visit(node.Left), Visit(node.Right));
    //    }

    //    protected override Expression VisitConstant(ConstantExpression node)
    //    {
    //        return node;
    //    }

    //    protected override Expression VisitMember(MemberExpression node)
    //    {
    //        if (node.Expression == _parameter)
    //            return node;

    //        return Expression.MakeMemberAccess(_parameter, node.Member);
    //    }
    //}
}