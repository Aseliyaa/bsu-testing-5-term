﻿namespace Execution.Compiled;

public enum ExpressionType
{
    Undefined,
    Int,
    Double,
    Str,
    Bool
}

public struct TypedValue
{
    public ExpressionType type = ExpressionType.Undefined;
    public int intValue = 0;
    public double doubleValue = 0;
    public string? stringValue;
    public bool boolValue = false;

    public TypedValue()
    {
        type = ExpressionType.Undefined;
    }
    public void SetFrom(TokenConstantType token)
    {
        if (token is TokenConstant<int> ti)
        {
            intValue = ti.value;
            type = ExpressionType.Int;
        }
        else if (token is TokenConstant<double> td)
        {
            doubleValue = td.value;
            type = ExpressionType.Double;
        }
        else if (token is TokenConstant<string> ts)
        {
            stringValue = ts.value;
            type = ExpressionType.Str;
        }
        else if (token is TokenConstant<bool> tb)
        {
            boolValue = tb.value;
            type = ExpressionType.Bool;
        }
    }
}

public static class TypeResolver
{
    public static ExpressionType ResultingOperationType(string operation, ExpressionType type1, ExpressionType type2)
    {
        if (type1 == ExpressionType.Int || type1 == ExpressionType.Double)
        { 
            if (operation == "Unary-"
                || operation == "Unary+"
               )
            return type1;
        }

        if (type1 == type2)
        {
            if (operation == "=="
             || operation == "!="
             || operation == "<="
             || operation == ">="
             || operation == "<"
             || operation == ">"
                )
            {
                return ExpressionType.Bool;
            }

            if (operation == "+") // plus or string concat   
            {
                return type1;
            }
        }

        if (type1 == ExpressionType.Int || type1 == ExpressionType.Double) 
        {
            //if (operation == "++" || operation == "--")
            //{
            //    return type1;
            //}

            if (type2 == ExpressionType.Int || type2 == ExpressionType.Double)
            {
                if (operation == "+"
                    || operation == "-"
                    || operation == "*"
                    || operation == "/"
                    )
                {
                    if (type1 == ExpressionType.Double || type2 == ExpressionType.Double)
                        return ExpressionType.Double;
                    else
                        return ExpressionType.Int;
                }

                if (operation == "%")
                {
                    if (type1 == ExpressionType.Int || type2 == ExpressionType.Int)
                        return type1;
                }
            }
        }

        if (type1 == ExpressionType.Bool)
        {
            if (operation == "!")
            {
                return type1;
            }
            if (type2 == ExpressionType.Bool)
            {
                if (operation == "&&" || operation == "||")
                {
                    return type1;
                }
            }
        }

        //StopOnError("qqqError");
        return ExpressionType.Undefined;
    }

}