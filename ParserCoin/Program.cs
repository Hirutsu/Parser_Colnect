//переменные для запуска
using ParserCoin;

var uri = new Uri("https://colnect.com/ru/coins/countries");
var countries = @"E:\ParserCoin\Countries.txt";
var catalogCountries = @"E:\ParserCoin\CatalogCountries.txt";
var connectionString = @"Data Source=HIRUTSU\SQLEXPRESS;Initial Catalog=ParserColnect; Integrated Security=true;";
var imagesForlder = @"E:\ParserCoin\Images";

Parser parser = new(countries, catalogCountries, uri,connectionString, imagesForlder);

if (parser.RunAsync().Result)
{
    Console.WriteLine("Парсер успешно отработал");
}
else
{
    Console.WriteLine("Парсер отработал с ошибками, посмотрите ошибки выше");
}
Console.ReadLine();