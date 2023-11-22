﻿using System.Text;

namespace SyntaxAnalyze;

public class Analyzer
{
    private static readonly char[] BlankSymbols = { ' ', '\n', '\t', '\r' };
    private static readonly char[] EscapedSymbols = { 'n', 't', 'r', '\\', '\'' };
    private const string SingleLineComment = "//";

    private readonly string expression;
    private int position;
    private readonly Dictionary<string, ParseResult> variables = new();

    public Analyzer(string expression)
    {
        this.expression = expression;
        this.position = 0;
    }

    public bool Parse()
    {
        position = 0;

        bool f;

        do
        {
            f = ParseAssigment();
        }
        while (f);

        f = EndCode();
        return f;
    }

    private bool EndCode()
    {
        SkipBlanks();

        if (position == expression.Length)
        {
            return true;
        }
        return false;
    }

    public bool IsValidExpression()
    {

        position = 0;

        ParseExpression();

        if (position == expression.Length)
        {
            return true;
        }

        throw new InvalidOperationException();
    }



    private bool ParseExpression(ParseResult parseResult)
    {
        if (!ParseOperand(parseResult))
        {
            throw new InvalidOperationException();
        }

        var expressionType = parseResult.Type;

        while (expression.Length >= position)
        {
            if (!ParseOperation())
            {
                return false;
            }

            if (!ParseOperand(parseResult))
            {
                throw new InvalidOperationException();
            }

            if (parseResult.Type != expressionType)
            {
                throw new InvalidOperationException();
            }
        }

        return true;
    }

    private bool ParseExpression()
    {
        var parseResult = new ParseResult();
        return ParseExpression(parseResult);
    }

    private void SkipBlanks()
    {
        while (position < expression.Length && BlankSymbols.Contains(expression[position]))
        {
            position++;
        }

        while (expression.Substring(position).StartsWith(SingleLineComment))
        {
            while (position < expression.Length && expression[position] != '\n')
            {
                position++;
            }

            if (position < expression.Length)
            {
                position++;
            }

            SkipBlanks();
        }
    }

    private char CurrentChar()
    {
        return expression[position];
    }

    private bool ParseString()
    {
        SkipBlanks();

        bool Escape = true;

        if (ParseChar('@'))
        {
            Escape = false;

            if (!ParseChar('\''))
            {
                throw new InvalidOperationException();
            }
        }
        else
        {
            if (!ParseChar('\''))
            {
                return false;
            }
        }

        int pos1 = position;

        while (!EndCode()) // don't use ParseChar() or SkipBlanks() here, they skip comments!
        {
            if (Escape && CurrentChar() == '\\')
            {
                position++;

                if (position >= expression.Length)
                    throw new InvalidOperationException();

                if (!EscapedSymbols.Contains(CurrentChar()))
                {
                    throw new InvalidOperationException();
                }
            }
            else if (CurrentChar() == '\'')
            {
                position++;
                return true;
            }

            position++;
        }

        throw new InvalidOperationException();
    }


    private bool ParseAssigment()
    {
        var parseResult = new ParseResult();

        string? name = ParseVariable();

        if (name == null)
        {
            return false;
        }

        parseResult.Name = name;

        if (!ParseChar('='))
        {
            return false;
        }

        ParseExpression(parseResult);

        if (!ParseChar(';'))
        {
            throw new InvalidOperationException();
        }

        AddVar(parseResult);

        return true;

    }

    private bool ParseOperation()
    {
        if (
              ParseChars("==")
           || ParseChars("<=")
           || ParseChars(">=")
           || ParseChars("<")
           || ParseChars(">")
           )
        {
            ;
        }
        else if (
              ParseChars("++")
           || ParseChars("--")
           )
        {
            ;
        }
        else if (
              ParseChars("+")
           || ParseChars("-")
           || ParseChars("*")
           || ParseChars("/")
           || ParseChars("%")
           )
        {
            ;
        }
        else
        {
            return false;
        }
        return true;

        //SkipBlanks();
        //if (position < expression.Length && Validators.IsOperation(expression[position]))
        //{
        //    position++;
        //    return true;
        //}
        //return false;
    }

    private bool ParseOperand(ParseResult parseResult)
    {
        if (ParseChar('('))
        {
            ParseExpression(parseResult);

            if (!ParseChar(')'))
            {
                throw new InvalidOperationException();
            }

            return true;
        }

        if (ParseString())
        {
            parseResult.Type = ExpressionType.Str;
            return true;
        }

        if (ParseNumber())
        {
            parseResult.Type = ExpressionType.Num;
            return true;
        }

        string? str = ParseVariable();

        if (str == null)
        {
            throw new InvalidOperationException();
        }
        var parseResultRhs = GetVar(str);
        parseResult.Type = parseResultRhs.Type;
        return true;
    }

    private bool ParseChar(char symbol)
    {
        SkipBlanks();

        if (position < expression.Length && expression[position] == symbol)
        {
            position++;
            return true;
        }

        return false;
    }

    private bool ParseChars(string str)
    {
        SkipBlanks();

        if (position < expression.Length && expression[position..].StartsWith(str))
        {
            position += str.Length;
            return true;
        }

        return false;
    }

    private string? ParseVariable()
    {
        SkipBlanks();
        if (position >= expression.Length)
        {
            return null;
        }

        if (!char.IsAscii(expression[position]) && expression[position] != '_')
        {
            return null;
        }

        int p1 = position;
        position++;

        while (position < expression.Length && IsValidVariableSymbol())
        {
            position++;
        }

        string v = expression[p1..position];
        return v;
    }

    private ParseResult GetVar(string name)
    {
        return variables[name];
    }


    private void AddVar(ParseResult parseResult)
    {
        variables.TryAdd(parseResult.Name, parseResult.Clone());
    }

    private bool IsValidVariableSymbol()
    {
        if (position >= expression.Length)
        {
            return false;
        }

        char suspect = expression[position];

        return char.IsDigit(suspect) || char.IsAsciiLetter(suspect) || suspect == '_';
    }

    private bool ParseNumber()
    {
        StringBuilder number = new();

        while (position < expression.Length && Validators.IsDigit(expression[position]))
        {
            number.Append(expression[position]);
            position++;
        }

        return number.Length > 0;
    }
}