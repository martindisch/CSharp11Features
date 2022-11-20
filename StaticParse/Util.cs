public static class Util
{
    public static T GenericParse<T>(string input) where T : IParsable<T>
    {
        return T.Parse(input, null);
    }
}
