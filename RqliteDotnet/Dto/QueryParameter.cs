namespace RqliteDotnet.Dto;

public class QueryParameter
{
    public QueryParamType ParamType { get; set; }
    
    public object Value { get; set; }

    public virtual string ToParamString()
    {
        return (ParamType == QueryParamType.Number ? Value : $"\"{Value}\"") + ",";
    }
}

public enum QueryParamType
{
    String,
    Number
}