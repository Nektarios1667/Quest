#if EDITOR
    using var game = new Quest.Editor.EditorWindow();
    game.Run();
#else
    using var game = new Quest.Window();
    game.Run();
#endif