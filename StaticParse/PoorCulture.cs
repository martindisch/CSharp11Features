record PoorCulture(string Language, string Country) : IParsable<PoorCulture>
{
    public static PoorCulture Parse(string s, IFormatProvider? provider)
    {
        var parts = s.Split('-');
        if (parts.Length != 2)
        {
            throw new FormatException("Oh noes");
        }

        return new(parts[0], parts[1]);
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out PoorCulture result)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{Language}-{Country}";
}
