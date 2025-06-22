global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;

global using Quest.Entities;
global using Quest.Tools;
global using static Quest.TextureManager;

#if EDITOR
    using var game = new Quest.Editor.EditorWindow();
    game.Run();
#else
    using var game = new Quest.Window();
    game.Run();
#endif