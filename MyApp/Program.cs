using Generated;
using MyApp;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var hello = new HelloWorld();
Console.WriteLine(hello.Message);

var attrs = typeof(MyService).GetCustomAttributes(false);
foreach (var attr in attrs)
{
    Console.WriteLine($"MyService has attribute: {attr.GetType().Name}");
}

var myService = new MyService();
myService.SayHello();
