global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
global using MonoGame.Extended;
global using Quest.Decals;
global using Quest.Entities;
global using Quest.Items;
global using Quest.Managers;
global using Quest.Tiles;
global using Quest.Utilities;
global using System;
global using System.Collections.Generic;
global using static Quest.Managers.TextureManager;
using Quest.Editor;
using Quest.Editor.Generator;
using System.Linq;


/*
     __    __                   _____               
     |  \/  |                  / ____|              
     | \  / | ___  _ __   ___ | |     ___  _ __ ___ 
     | |\/| |/ _ \| '_ \ / _ \| |    / _ \| '__/ _ \
     | |  | | (_) | | | | (_) | |___| (_) | | |  __/
     |_|  |_|\___/|_| |_|\___/ \_____\___/|_|  \___|
    __            _          ___               
    \ \        / (_)         |  |                  
     \ \  /\  / / _ _ __   __|  | _____      _____ 
      \ \/  \/ / | | '_ \ / _`  |/ _ \ \ /\ / / __|
       \  /\  /  | | | | | (_|  | (_) \ V  V /\__ \
        \/  \/   |_|_| |_|\__, _|\___/ \_/\_/ |___/
      __      __  ___         ___         ___  
      \ \    / / |__ \       / _ \       / _ \ 
       \ \  / /     ) |     | | | |     | | | |
        \ \/ /     / /      | | | |     | | | |
         \  /     / /_      | |_| |     | |_| |
          \/     |____| (_)  \___/  (_)  \___/                                          
*/
namespace Quest
{
    public enum ProgramMode
    {
        Game,
        LevelEditor,
        CodeGenerator,
    }
}

static class Program
{
    public static Quest.ProgramMode Mode = Quest.ProgramMode.Game;
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Contains("--level-editor"))
        {
            Mode = Quest.ProgramMode.LevelEditor;
            using var levelEditorApp = new LevelEditor();
            levelEditorApp.Run();
        }
        else if (args.Contains("--code-generator"))
        {
            Mode = Quest.ProgramMode.CodeGenerator;
            CodeGenerator.Run("C:/Users/nekta/source/repos/Quest");
        }
        else
        {
            Mode = Quest.ProgramMode.Game;
            using var gameApp = new Quest.Window();
            gameApp.Run();
        }
    }
}