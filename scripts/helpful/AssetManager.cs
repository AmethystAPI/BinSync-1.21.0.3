using System.Collections.Generic;
using System.IO;
using Godot;

public class AssetManager {
    private static Dictionary<string, object> s_Assets = new Dictionary<string, object>();

    public static void Register(string id, object value) {
        GD.Print("[Assets] Registered " + id);
    }

    public static T Get<T>(string id) {
        return (T)s_Assets[id];
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
}