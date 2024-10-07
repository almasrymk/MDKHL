using System;

public class SmartParkingBaseComponent
{
    public string Message { get; set; } = "Hello from Base Class!";

    public void ShowMessage()
    {
        Console.WriteLine(Message);
    }
}
