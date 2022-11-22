record PersonResponse(string Name, int FriendCount) : IFrom<Person, PersonResponse>
{
    public static PersonResponse From(Person person) => new(person.Name, person.Friends.Count);
}
