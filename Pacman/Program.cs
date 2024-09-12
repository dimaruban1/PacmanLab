using MyGame;
using System;
using System.Runtime.CompilerServices;
using Walls;

int coinsCount = 0;

for (int i = 0; i < 10; i++)
{
    var mazeBuilder = new MazeBuilder();
    mazeBuilder.Width = Math.Min(3 + i,8);
    mazeBuilder.Height = Math.Min(3 + i, 8);
    var maze = mazeBuilder.CreateMaze();

    var level = mazeBuilder.GetLevel(maze);

    using var game = new PacmanGame(level, 1, i / 2 + 1, i+1, coinsCount, GetEnemyCount(i+1));
    game.Run();
    while (game.IsActive)
    {
        
    }

    // level ended
    switch (game.Status)
    {
        case GameStatus.Quit:
            Environment.Exit(0);
            break;
        case GameStatus.Loss:
            var screen = new FinalScreen("GG YOU LOSE");
            screen.Run();
            return;
        case GameStatus.Win:
            break;
        default: break;

    }
    coinsCount = game.CoinsGained;
}

var screen1 = new FinalScreen("CONGRATS! YOU WON!");
screen1.Run();


int GetEnemyCount(int level)
{
    switch (level)
    {
        case 1: return 0;
        case 2: return 1;
        case 3: return 2;
        case 4: return 2;
        case 5: return 3;
        case 6: return 4;
        case 7: return 4;
        case 8: return 5;
        case 9: return 6;
        case 10: return 7;
        default: return 0;
    }
}