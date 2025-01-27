using System.Collections.Generic;
using System.IO;
using Godot;

public class AssetManager {
    private static Dictionary<string, object> s_Assets = new Dictionary<string, object>();

    public static void Register(string id, object value) {
        GD.Print("[Assets] Registered " + id);

        s_Assets.Add(id, value);
    }

    public static T Get<T>(string id) {
        return (T)s_Assets[id];
    }

    public static PackedScene GetScene(string id) {
        return (PackedScene)s_Assets[id];
    }

    public static void Load(string path) {
        using DirAccess content = DirAccess.Open(path);

        if (content == null) return;

        content.ListDirBegin();
        string entryName = content.GetNext();
        while (entryName != "") {
            string entryPath = Path.Join(path, entryName);

            if (content.CurrentIsDir()) {
                Load(entryPath);
            } else {
                Register(Path.GetFileNameWithoutExtension(entryName), ResourceLoader.Load(entryPath));
            }

            entryName = content.GetNext();
        }
    }

    public static void DebugAssets() {
        GD.Print("Registered assets:");

        foreach (string id in s_Assets.Keys) {
            GD.Print(id);
        }
    }
}