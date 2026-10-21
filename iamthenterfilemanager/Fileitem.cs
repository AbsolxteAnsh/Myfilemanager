using System;
using System.IO;

namespace MyFileManger;

public class FileItem
{
    public string Name { get; private set; } = "";

    public string FullPath { get; private set; } = "";

    public bool IsDirectory { get; private set; }

    public string Type { get; private set; } = "";

    public string Size { get; private set; } = "";

    public string Modified { get; private set; } = "";

    public string Icon { get; private set; } = "";

    public FileItem(string path)
    {
        FullPath = path;

        IsDirectory = Directory.Exists(path);

        if (IsDrive(path))
        {
            DriveInfo drive = new DriveInfo(path);

            Name = drive.Name;

            Type = "Drive";

            Icon = "💽";

            if (drive.IsReady)
            {
                Size =
                    $"{FormatSize(drive.AvailableFreeSpace)} free";

                Modified = "—";
            }
            else
            {
                Size = "Not ready";
                Modified = "—";
            }

            return;
        }

        if (IsDirectory)
        {
            DirectoryInfo directory =
                new DirectoryInfo(path);

            Name = directory.Name;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = directory.FullName;
            }

            Type = "Folder";

            Size = "—";

            Modified =
                directory.LastWriteTime.ToString(
                    "MMM dd, yyyy  h:mm tt");

            Icon = "📁";

            return;
        }

        try
        {
            FileInfo file =
                new FileInfo(path);

            Name = file.Name;

            Type =
                GetFileType(file.Extension);

            Size =
                FormatSize(file.Length);

            Modified =
                file.LastWriteTime.ToString(
                    "MMM dd, yyyy  h:mm tt");

            Icon =
                GetIcon(file.Extension);
        }
        catch
        {
            Name =
                Path.GetFileName(path);

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = path;
            }

            Type = "File";

            Size = "—";

            Modified = "—";

            Icon = "📄";
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = path;
        }
    }

    public override string ToString()
    {
        return Name;
    }

    private static bool IsDrive(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            string fullPath =
                Path.GetFullPath(path);

            if (fullPath.Length != 3)
                return false;

            if (fullPath[1] != ':')
                return false;

            if (fullPath[2] != '\\')
                return false;

            string root =
                Path.GetPathRoot(fullPath) ?? "";

            if (!string.Equals(
                    root,
                    fullPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            DriveInfo drive =
                new DriveInfo(fullPath);

            return string.Equals(
                       drive.RootDirectory.FullName,
                       fullPath,
                       StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string GetFileType(
        string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return "File";

        return
            extension
                .TrimStart('.')
                .ToUpperInvariant()
            + " File";
    }

    private static string GetIcon(
        string extension)
    {
        extension =
            extension.ToLowerInvariant();

        return extension switch
        {
            ".exe" => "⚙",
            ".msi" => "⚙",

            ".txt" => "📄",
            ".log" => "📄",

            ".pdf" => "📕",

            ".doc" => "📘",
            ".docx" => "📘",

            ".xls" => "📗",
            ".xlsx" => "📗",

            ".ppt" => "📙",
            ".pptx" => "📙",

            ".jpg" => "🖼",
            ".jpeg" => "🖼",
            ".png" => "🖼",
            ".gif" => "🖼",
            ".bmp" => "🖼",
            ".webp" => "🖼",
            ".ico" => "🖼",

            ".mp3" => "🎵",
            ".wav" => "🎵",
            ".flac" => "🎵",
            ".ogg" => "🎵",

            ".mp4" => "🎬",
            ".mkv" => "🎬",
            ".avi" => "🎬",
            ".mov" => "🎬",
            ".wmv" => "🎬",

            ".zip" => "📦",
            ".rar" => "📦",
            ".7z" => "📦",
            ".tar" => "📦",
            ".gz" => "📦",

            ".cs" => "💻",
            ".cpp" => "💻",
            ".c" => "💻",
            ".h" => "💻",
            ".hpp" => "💻",

            ".py" => "🐍",

            ".js" => "🟨",
            ".jsx" => "🟨",

            ".ts" => "🔷",
            ".tsx" => "🔷",

            ".html" => "🌐",
            ".htm" => "🌐",

            ".css" => "🎨",

            ".json" => "⚙",
            ".xml" => "⚙",
            ".yaml" => "⚙",
            ".yml" => "⚙",

            ".dll" => "🔧",

            _ => "📄"
        };
    }

    private static string FormatSize(
        long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes <
            1024L * 1024L)
        {
            return
                $"{bytes / 1024.0:0.##} KB";
        }

        if (bytes <
            1024L * 1024L * 1024L)
        {
            return
                $"{bytes / (1024.0 * 1024.0):0.##} MB";
        }

        return
            $"{bytes / (1024.0 * 1024.0 * 1024.0):0.##} GB";
    }
}