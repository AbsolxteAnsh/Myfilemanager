using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MyFileManger;

public class FileManager
{
    public string CurrentPath { get; private set; }

    private readonly Stack<string> backHistory = new();
    private readonly Stack<string> forwardHistory = new();

    public FileManager()
    {
        CurrentPath =
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile
            );
    }

    public List<FileItem> GetItems()
    {
        var items = new List<FileItem>();

        if (!Directory.Exists(CurrentPath))
            return items;

        try
        {
            foreach (string directory in
                     Directory.GetDirectories(CurrentPath))
            {
                try
                {
                    items.Add(new FileItem(directory));
                }
                catch
                {
                    // Ignore folders we cannot access
                }
            }

            foreach (string file in
                     Directory.GetFiles(CurrentPath))
            {
                try
                {
                    items.Add(new FileItem(file));
                }
                catch
                {
                    // Ignore files we cannot access
                }
            }
        }
        catch
        {
            // Directory could not be read
        }

        return items
            .OrderByDescending(x => x.IsDirectory)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public bool OpenFolder(string path)
    {
        if (!Directory.Exists(path))
            return false;

        if (!string.Equals(
                CurrentPath,
                path,
                StringComparison.OrdinalIgnoreCase))
        {
            backHistory.Push(CurrentPath);
            forwardHistory.Clear();

            CurrentPath = path;
        }

        return true;
    }

    public bool GoBack()
    {
        if (backHistory.Count == 0)
            return false;

        forwardHistory.Push(CurrentPath);

        CurrentPath = backHistory.Pop();

        return true;
    }

    public bool GoForward()
    {
        if (forwardHistory.Count == 0)
            return false;

        backHistory.Push(CurrentPath);

        CurrentPath = forwardHistory.Pop();

        return true;
    }

    public bool GoUp()
    {
        if (string.IsNullOrEmpty(CurrentPath))
            return false;

        DirectoryInfo? parent =
            Directory.GetParent(CurrentPath);

        if (parent == null)
            return false;

        return OpenFolder(parent.FullName);
    }

    public bool CanGoBack()
    {
        return backHistory.Count > 0;
    }

    public bool CanGoForward()
    {
        return forwardHistory.Count > 0;
    }

    public bool CanGoUp()
    {
        if (string.IsNullOrEmpty(CurrentPath))
            return false;

        return Directory.GetParent(CurrentPath) != null;
    }

    public void GoHome()
    {
        string home =
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile
            );

        if (Directory.Exists(home))
        {
            backHistory.Clear();
            forwardHistory.Clear();

            CurrentPath = home;
        }
    }
}