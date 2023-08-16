using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WhiteBoard.Core;

namespace WhiteBoard.Hubs
{
    enum Command
    {
        Start,
        Skip,
        Unknown,
        Ready,
        Reset
    }
    public class ChatHub : Hub
    {
        public const string Game = "Game";
        public async Task Send(string name, string message)
        {
            System.Console.WriteLine(message);
            var command = CheckMessageForCommands(message);

            if(command == Command.Unknown)
            {
                if(Context.ConnectionId == GameEngine.Instance.CurrentDrawer.Id)
                    await Clients.Caller.SendAsync("broadcastMessage", Game, "Bruh, rly");
                    
                await Clients.All.SendAsync("broadcastMessage", name, message);
                var result = GameEngine.Instance.Validate(name, message);

                if(!result.Value)
                {
                    if(!string.IsNullOrEmpty(result.Reason))
                        await Clients.Caller.SendAsync("broadcastMessage", Game, result.Reason);
                    return;
                }

                if(result.Value)
                {
                    await Clients.All.SendAsync("broadcastMessage", Game, $"{name} guessed the word {GameEngine.Instance.CurrentWord}");
                    await Clear();
                    if(!string.IsNullOrEmpty(result.Reason))
                    {
                        await Clients.All.SendAsync("broadcastMessage", Game, result.Reason);
                        var sb = new StringBuilder();
                        sb.AppendLine("\t STATISTICS");
                        foreach (var player in GameEngine.Instance.Players)
                        {
                            sb.Append(player.Name);
                            sb.Append(" : ");
                            sb.AppendLine(player.Score.ToString());
                        }
                        GameEngine.Instance.Reset();
                        await Clients.All.SendAsync("broadcastMessage", Game, sb.ToString());
                        return;
                    }
                    GameEngine.Instance.NextRound();
                }

                await Clients.Client(GameEngine.Instance.CurrentDrawer.Id)
                    .SendAsync("broadcastMessage", Game, $"Your turn, the word is: {GameEngine.Instance.CurrentWord}");
            }
            if(command == Command.Ready)
            {
                GameEngine.Instance.PlayerIsReady(name);
            }
            if(command == Command.Start)
            {
                var result = GameEngine.Instance.Start();
                if(result.Value)
                {
                    await Clients.All.SendAsync("broadcastMessage", Game, $"Game started with total of {GameEngine.Instance.WordsCount() + 1} words");
                    
                    await Clients.Client(GameEngine.Instance.CurrentDrawer.Id)
                        .SendAsync("broadcastMessage", Game, $"Your turn, the word is: {GameEngine.Instance.CurrentWord}");
                    
                    return;
                }

                await Clients.Caller.SendAsync("broadcastMessage", Game, result.Reason);
            }
            if(command == Command.Skip)
            {
                GameEngine.Instance.NextRound();
                await Clients.Client(GameEngine.Instance.CurrentDrawer.Id)
                    .SendAsync("broadcastMessage", Game, $"Your turn, the word is: {GameEngine.Instance.CurrentWord}");
            }
            if(command == Command.Reset)
            {
                GameEngine.Instance.Reset();
            }
        }

        public void AddPlayer(string name)
        {
            var player = new Player(name, Context.ConnectionId);
            System.Console.WriteLine(JsonSerializer.Serialize(player));
            GameEngine.Instance.AddPlayer(player);
        }

        public void AddNewWord(string word)
        {
            System.Console.WriteLine(word);
            GameEngine.Instance.AddWord(word);
        }

        private Task Clear()
        {
            return Clients.All.SendAsync("clearCanvas");
        }

        private Command CheckMessageForCommands(string message)
        {
            switch(message)
            {
                case "/start":
                    return Command.Start;
                case "/skip":
                    return Command.Skip;
                case "/ready":
                    return Command.Ready;
                case "/reset":
                    return Command.Reset;                    
                default:
                    return Command.Unknown;
            }
        }
    }
}