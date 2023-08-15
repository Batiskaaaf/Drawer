namespace WhiteBoard.Core;

public class Player
{
    public string Name { get; private set; }
    public int Score { get; private set;}
    public int DrawCount { get; private set;}
    public bool IsReady { get; private set; }

    public string Id { get; private set; }

    public Player(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public void IncrementScore()
        => Score++;

    public void IncrementDrawCount()
        => DrawCount++;

    public void SetIsReady()
        => IsReady = true;
}