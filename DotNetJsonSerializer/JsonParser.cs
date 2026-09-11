using System.Text;

namespace DotnetJsonSerializer;

internal class JsonParser(string json)
{
    private readonly string _json = json;
    private int _index = 0;

    internal object? Parse()
    {
        var value = ParseValue();
        SkipWhiteSpace();

        if(_index != _json.Length)
            throw new FormatException("Unexpected Token!");

        return value;
    }

    private object? ParseValue()
    {
        SkipWhiteSpace();

        if (_index >= _json.Length) throw new FormatException("Unexpected End of JSON!");

        if (_json[_index] == '"') return ParseString();

        if (_json[_index..].StartsWith("null"))
        {
            _index += 4;
            return null;
        }

        if (_json[_index..].StartsWith("true"))
        {
            _index += 4;
            return true;
        }

        if (_json[_index..].StartsWith("false"))
        {
            _index += 5;
            return false;
        }

        if (_json[_index] == '-' || char.IsDigit(_json[_index])) return ParseNumber();

        throw new FormatException("Invalid JSON value.");
    }

    private static readonly Dictionary<char, char> EscapeSequences = new()
    {
        ['"'] = '"',
        ['\\'] = '\\',
        ['/'] = '/',
        ['n'] = '\n',
        ['r'] = '\r',
        ['t'] = '\t',
        ['b'] = '\b',
        ['f'] = '\f'
    };

    private string ParseString()
    {
        if (_json[_index] != '"') throw new FormatException("Expected '\"'!");

        _index++;
        var result = new StringBuilder();

        while (_index < _json.Length)
        {
            var ch = _json[_index++];
            if (ch == '"') return result.ToString();
            if (ch == '\\')
            {
                if (_index >= _json.Length) throw new FormatException("Invalid Escape Sequence!");

                var escaped = _json[_index++];
                if (EscapeSequences.TryGetValue(escaped, out var unescaped))
                {
                    result.Append(unescaped);
                    continue;
                }
                if (escaped == 'u')
                {
                    if (_index + 4 > _json.Length) throw new FormatException("Invalid Unicode Escape Eequence!");

                    var hex = _json.Substring(_index, 4);

                    if (!ushort.TryParse(
                            hex,
                            System.Globalization.NumberStyles.HexNumber,
                            null,
                            out var code))
                        throw new FormatException("Invalid Unicode Escape Eequence!");

                    result.Append((char)code);
                    _index += 4;
                    continue;
                }

                throw new FormatException("Invalid Escape Sequence!");
            }
        }
        throw new FormatException("Unterminated String!");
    }

    private object ParseNumber()
    {
        var start = _index;

        ParseSign();
        ParseIntegerPart();
        ParseFractionPart();
        ParseExponentPart();

        var text = _json[start.._index];

        if (text.Contains('.') || text.Contains('e') || text.Contains('E'))
        {
            if (double.TryParse(
                    text,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var doubleValue))
                return doubleValue;

            throw new FormatException("Invalid number.");
        }

        if (int.TryParse(text, out var intValue)) return intValue;
        if (long.TryParse(text, out var longValue)) return longValue;

        throw new FormatException("Invalid number.");
    }

    private void ParseSign()
    {
        if (_json[_index] == '-') _index++;
    }

    private void ParseIntegerPart()
    {
        if (_index >= _json.Length || !char.IsDigit(_json[_index]))
            throw new FormatException("Invalid number!");

        if (_json[_index] == '0')
        {
            _index++;

            if (_index < _json.Length && char.IsDigit(_json[_index]))
                throw new FormatException("Invalid number!");

            return;
        }

        while (_index < _json.Length && char.IsDigit(_json[_index])) _index++;
    }

    private void ParseFractionPart()
    {
        if (_index >= _json.Length || _json[_index] != '.') return;

        _index++;

        if (_index >= _json.Length || !char.IsDigit(_json[_index]))
            throw new FormatException("Invalid number!");

        while (_index < _json.Length && char.IsDigit(_json[_index])) _index++;
    }

    private void ParseExponentPart()
    {
        if (_index >= _json.Length || (_json[_index] != 'e' && _json[_index] != 'E')) return;

        _index++;

        if (_index < _json.Length && (_json[_index] == '+' || _json[_index] == '-')) _index++;

        if (_index >= _json.Length || !char.IsDigit(_json[_index]))
            throw new FormatException("Invalid number!");

        while (_index < _json.Length && char.IsDigit(_json[_index])) _index++;
    }

    private void SkipWhiteSpace()
    {
        while (_index < _json.Length && char.IsWhiteSpace(_json[_index])) _index++;
    }
}