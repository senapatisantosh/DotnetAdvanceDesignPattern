using DesignPatterns.Behavioral.Interpreter.Expressions;

namespace DesignPatterns.Behavioral.Interpreter;

/// <summary>
/// Simple recursive-descent parser for search query DSL.
///
/// Grammar:
///   query      = or_expr
///   or_expr    = and_expr ("OR" and_expr)*
///   and_expr   = unary_expr ("AND" unary_expr)*
///   unary_expr = "NOT" unary_expr | primary
///   primary    = "(" query ")" | field_match
///   field_match = identifier ":" value
///
/// Examples:
///   "category:Electronics AND brand:Apple"
///   "category:Electronics OR category:Books"
///   "NOT color:Red AND brand:Nike"
///   "(category:Electronics OR category:Books) AND NOT brand:Generic"
/// </summary>
public sealed class SearchQueryParser
{
    private string[] _tokens = [];
    private int _position;

    public ISearchExpression Parse(string query)
    {
        _tokens = Tokenize(query);
        _position = 0;

        var expression = ParseOrExpression();

        if (_position < _tokens.Length)
            throw new FormatException($"Unexpected token '{_tokens[_position]}' at position {_position}.");

        return expression;
    }

    private ISearchExpression ParseOrExpression()
    {
        var left = ParseAndExpression();

        while (CurrentTokenIs("OR"))
        {
            _position++; // consume OR
            var right = ParseAndExpression();
            left = new OrExpression(left, right);
        }

        return left;
    }

    private ISearchExpression ParseAndExpression()
    {
        var left = ParseUnaryExpression();

        while (CurrentTokenIs("AND"))
        {
            _position++; // consume AND
            var right = ParseUnaryExpression();
            left = new AndExpression(left, right);
        }

        return left;
    }

    private ISearchExpression ParseUnaryExpression()
    {
        if (CurrentTokenIs("NOT"))
        {
            _position++; // consume NOT
            var expr = ParseUnaryExpression();
            return new NotExpression(expr);
        }

        return ParsePrimary();
    }

    private ISearchExpression ParsePrimary()
    {
        if (CurrentTokenIs("("))
        {
            _position++; // consume (
            var expr = ParseOrExpression();

            if (!CurrentTokenIs(")"))
                throw new FormatException("Expected closing parenthesis.");

            _position++; // consume )
            return expr;
        }

        return ParseFieldMatch();
    }

    private ISearchExpression ParseFieldMatch()
    {
        if (_position >= _tokens.Length)
            throw new FormatException("Unexpected end of query — expected a field:value expression.");

        var token = _tokens[_position++];
        var colonIndex = token.IndexOf(':');

        if (colonIndex <= 0 || colonIndex >= token.Length - 1)
            throw new FormatException($"Invalid field match expression: '{token}'. Expected format: field:value");

        var field = token[..colonIndex];
        var value = token[(colonIndex + 1)..];

        return new FieldMatchExpression(field, value);
    }

    private bool CurrentTokenIs(string expected) =>
        _position < _tokens.Length &&
        _tokens[_position].Equals(expected, StringComparison.OrdinalIgnoreCase);

    private static string[] Tokenize(string query)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var ch in query)
        {
            switch (ch)
            {
                case ' ' or '\t':
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    break;
                case '(' or ')':
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    tokens.Add(ch.ToString());
                    break;
                default:
                    current.Append(ch);
                    break;
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        return [.. tokens];
    }
}
