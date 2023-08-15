namespace WhiteBoard.Core;

public class GameResponse<T>
{
    public T Value { get; set; }
    public string Reason { get; set; }    
}