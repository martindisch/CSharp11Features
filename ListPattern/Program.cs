// Finally this is as elegant as its Rust counterpart
// https://adventures.michaelfbryan.com/posts/daily/slice-patterns/#checking-for-palindromes

bool IsPalindrome(char[] characters) => characters switch
{
    [var first, .. var middle, var last] => first == last && IsPalindrome(middle),
    [] or [_] => true,
};

var palindrome = "abba";
var notPalindrome = "abc";

Console.WriteLine($"{palindrome}: {IsPalindrome(palindrome.ToCharArray())}");
Console.WriteLine($"{notPalindrome}: {IsPalindrome(notPalindrome.ToCharArray())}");
