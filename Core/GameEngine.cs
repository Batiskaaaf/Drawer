using System;
using System.Collections.Generic;
using System.Linq;

namespace WhiteBoard.Core;


public class GameEngine
{
    GameEngine() {}
    private static readonly object @lock = new object();
    private static GameEngine instance = null;
    public static GameEngine Instance {
        get {
            lock(@lock) {
                if (instance == null) {
                    instance = new GameEngine();
                }
                return instance;
            }
        }
    }

    public List<Player> Players { get; private set; } = new();
    private List<string> words = new();
    private List<string> usedWords = new();

    public Player CurrentDrawer { get; private set; }

    private bool isStarted = false;

    public string CurrentWord;

    public void AddWord(string word)
    {
        if(isStarted)
            return;
        words.Add(word.ToLower());
    }

    public int WordsCount()
        => words.Count();

    public void Reset()
    {
        words.Clear();
        usedWords.Clear();
        Players.Clear();
        isStarted = false;
    }

    public GameResponse<bool> Start()
    {   
        if(isStarted)
            return new GameResponse<bool>{Value = false, Reason = "Already started"};

        if (Players.Count() < 1)
            return new GameResponse<bool>{Value = false, Reason = "Cant start, not enough players"};

        if (Players.Any(p => !p.IsReady))
            return new GameResponse<bool>{Value = false, Reason = "Cant start, one or more players are not ready"};
        
        if (words.Count() == 0)
            return new GameResponse<bool>{Value = false, Reason = "Cant start, no words were added"};
        
        words = words.Distinct().ToList();
        isStarted = true;
        NextRound();

        return new GameResponse<bool>{Value = true};
    }

    public GameResponse<bool> Validate(string playerName, string word)
    {
        if(!Players.Any(p => p.Name == playerName))
            return new GameResponse<bool>{Value = false, Reason = "Who are u ?"};


        if(CurrentWord != word.ToLower())
            return new GameResponse<bool>{Value = false};
        
        var player = Players.FirstOrDefault(p => p.Name == playerName);
        player.IncrementScore();
        if(words.Count() == 0)
        {
            return new GameResponse<bool>{Value = true, Reason = "Game over"};    
        }

        return new GameResponse<bool>{Value = true};

    }
    
    public void NextRound()
    { 
        ChangeDrawer();
        ChangeWord();
    }

    public GameResponse<bool> AddPlayer(Player player)
    {
        if(Players.Any(p => p.Name == player.Name))
            return new GameResponse<bool>{Value = false, Reason = $"Player with nickname {player.Name} already joined"};

        Players.Add(player);
        return new GameResponse<bool>{Value = true};
    }

    public void PlayerIsReady(string name)
    {
        var player = Players.FirstOrDefault(p => p.Name == name);
        if(player == null)
            return;
        player.SetIsReady();
    }

    private void ChangeDrawer()
    {
        CurrentDrawer = Players.OrderBy(p => p.DrawCount).First();
        CurrentDrawer.IncrementDrawCount();
    }

    private void ChangeWord()
    {
        CurrentWord = words[new Random().Next(0, words.Count)];
        words.Remove(CurrentWord);
        usedWords.Add(CurrentWord);
    }
}