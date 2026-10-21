using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyFileManger;

public partial class MainWindow : Window
{
    private readonly FileManager fileManager;

    private SpaceBackground? spaceBackground;

    // Loaded font
    private readonly FontFamily AndyFont =
        new FontFamily(
            new Uri(
                "pack://application:,,,/"),
            "./Fonts/ANDYB.ttf#ANDYB");


    public MainWindow()
    {
        InitializeComponent();

        // Apply font to the entire window.
        FontFamily = AndyFont;

        fileManager = new FileManager();

        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        Refresh();
    }


    private void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        spaceBackground =
            new SpaceBackground(
                SpaceCanvas);
    }


    private void MainWindow_Closed(
        object? sender,
        EventArgs e)
    {
        spaceBackground?.Stop();
    }


    private void Refresh()
    {
        FileList.ItemsSource = null;

        FileList.ItemsSource =
            fileManager.GetItems();

        PathBox.Text =
            fileManager.CurrentPath;

        BackButton.IsEnabled =
            fileManager.CanGoBack();

        ForwardButton.IsEnabled =
            fileManager.CanGoForward();

        UpButton.IsEnabled =
            fileManager.CanGoUp();
    }


    private void BackButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (fileManager.GoBack())
            Refresh();
    }


    private void ForwardButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (fileManager.GoForward())
            Refresh();
    }


    private void UpButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (fileManager.GoUp())
            Refresh();
    }


    private void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Refresh();
    }


    private void HomeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        fileManager.GoHome();

        Refresh();
    }


    private void DesktopButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenSpecialFolder(
            Environment.SpecialFolder.Desktop);
    }


    private void DocumentsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenSpecialFolder(
            Environment.SpecialFolder.MyDocuments);
    }


    private void DownloadsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string home =
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

        string downloads =
            Path.Combine(
                home,
                "Downloads");

        OpenFolder(downloads);
    }


    private void PicturesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenSpecialFolder(
            Environment.SpecialFolder.MyPictures);
    }


    private void VideosButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        OpenSpecialFolder(
            Environment.SpecialFolder.MyVideos);
    }


    private void ThisPCButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        List<FileItem> drives = new();


        foreach (string drive
                 in Directory.GetLogicalDrives())
        {
            try
            {
                drives.Add(
                    new FileItem(drive));
            }
            catch
            {
                // Ignore inaccessible drives.
            }
        }


        FileList.ItemsSource = null;

        FileList.ItemsSource =
            drives;

        PathBox.Text =
            "This PC";

        BackButton.IsEnabled =
            false;

        ForwardButton.IsEnabled =
            false;

        UpButton.IsEnabled =
            false;
    }


    private void OpenSpecialFolder(
        Environment.SpecialFolder folder)
    {
        string path =
            Environment.GetFolderPath(folder);

        OpenFolder(path);
    }


    private void OpenFolder(
        string path)
    {
        if (!Directory.Exists(path))
            return;


        if (fileManager.OpenFolder(path))
            Refresh();
    }


    private void PathBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;


        string path =
            PathBox.Text.Trim();


        if (Directory.Exists(path))
        {
            if (fileManager.OpenFolder(path))
                Refresh();
        }
        else
        {
            PathBox.Text =
                fileManager.CurrentPath;
        }
    }


    private void FileList_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (FileList.SelectedItem
            is not FileItem item)
        {
            return;
        }


        if (!item.IsDirectory)
            return;


        if (fileManager.OpenFolder(
                item.FullPath))
        {
            Refresh();
        }
    }
}