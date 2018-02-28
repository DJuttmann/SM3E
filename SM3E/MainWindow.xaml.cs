using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace SM3E
{

  enum ProjectLoadStatus
  {
    NotLoaded,
    Loading,
    Loaded,
  }

  
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    Project MainProject;

    ProjectLoadStatus Status = ProjectLoadStatus.NotLoaded;


    // Constructor
    public MainWindow ()
    {
      InitializeComponent ();

      if (!Logging.Open ())
      {
        System.Windows.MessageBox.Show ("log failed");
        return;
      }
      MainProject = new Project ();
      MainProject.ProjectFinishedLoading += ProjectFinishedLoading;
      MainProject.ProjectFailedLoading += ProjectFailedLoading;
      MainProject.ProjectClosed += ProjectClosed;
      MainProject.ProjectChanged += ProjectChanged;
      MainProject.ProjectSaved += ProjectSaved;

      EditorView.SetProject (MainProject);
    }


//========================================================================================
// Project Loading and Saving

    
    // Wait till the project is loaded, then prepare the editor if loaded successfully.
    private void WaitProjectLoaded (string projectPath, string projectFileName,
                                    string romFileName)
    {
      LoadIndicator.Visibility = Visibility.Visible;
      int n = 0;
      while (Status == ProjectLoadStatus.Loading)
      {
        LoadText.Content = "Loading" + "...".Substring (0, n);
        LoadIndicator.Dispatcher.Invoke (() => System.Threading.Thread.Sleep(100), 
          System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        n = (n + 1) % 4;
      }
      LoadIndicator.Visibility = Visibility.Hidden;

      if (Status == ProjectLoadStatus.Loaded)
      {
        MainProject.Start ();
        if (projectPath != null && projectFileName != null && romFileName != null)
        {
          MainProject.ProjectPath = projectPath;
          MainProject.ProjectFileName = projectFileName;
          MainProject.RomFileName = romFileName;
        }
        EditorView.IsEnabled = true;
        MenuNew.IsEnabled = false;
        MenuOpen.IsEnabled = false;
        MenuSave.IsEnabled = true;
        MenuClose.IsEnabled = true;
        MenuSaveStations.IsEnabled = true;
        StatusMessage.Content = "Loaded project " + MainProject.ProjectFileName;
      }
      else
        StatusMessage.Content = "Failed to load project " + MainProject.ProjectFileName;
    }


    // Set the project status marker to loaded.
    private void ProjectFinishedLoading (object sender, EventArgs e)
    {
      Status = ProjectLoadStatus.Loaded;
    }


    // Set the project status marker to not loaded, show error message.
    private void ProjectFailedLoading (object sender, LoadFailEventArgs e)
    {
      Status = ProjectLoadStatus.NotLoaded;
      string message;
      switch (e.LoadFailType)
      {
      case ProjectLoadException.Type.ProjectFileNotAccessible:
        message = "The project could not be opened. It may be in use by another program.";
        break;
      case ProjectLoadException.Type.RomFileNotSpecified:
        message = "The project file does not specify a ROM file.";
        break;
      case ProjectLoadException.Type.RomFileNotFound:
        message = "The ROM associated with the project could not be found." +
          "Make sure that the following file exists:" + Environment.NewLine +
          MainProject.ProjectPath + MainProject.RomFileName;
        break;
      case ProjectLoadException.Type.RomFileNotAccessible:
        message = "Unknown load error";
        break;
      default:
        message = "Unknown load error";
        break;
      }
      MessageBox.Show (message);
    }


    private void ProjectChanged (object sender, EventArgs e)
    {
      StatusMessage.Content = String.Empty;
    }


    private void ProjectSaved (object sender, EventArgs e)
    {
      StatusMessage.Content = "Project saved to " + MainProject.ProjectFileName;
    }


    private void ProjectClosed (object sender, EventArgs e)
    {
      EditorView.IsEnabled = false;
      MenuNew.IsEnabled = true;
      MenuOpen.IsEnabled = true;
      MenuSave.IsEnabled = false;
      MenuClose.IsEnabled = false;
      MenuSaveStations.IsEnabled = false;
    }


//========================================================================================
// Event handlers

    
    private void NewProject_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.NewProjectWindow (MainProject);
      window.Owner = Window.GetWindow (this);
      if (window.ShowDialog () == true)
      {
        Status = ProjectLoadStatus.Loading;
        Task.Run (() => MainProject.Load (window.TemplateFileName));
        WaitProjectLoaded (window.ProjectPath, window.ProjectFileName, window.RomFileName);
      }
    }


    private void OpenProject_Click (object sender, RoutedEventArgs e)
    {
      var Open = new Microsoft.Win32.OpenFileDialog ()
      {
        Filter = "Project files (*.sm3p)|*.sm3p"
        // InitialDirectory = Directory.GetCurrentDirectory ()
      };
      if (Open.ShowDialog (Window.GetWindow (this)) ?? false)
      {
        Status = ProjectLoadStatus.Loading;
        Task.Run (() => MainProject.Load (Open.FileName));
        WaitProjectLoaded (null, null, null);
      }
    }


    private void SaveProject_Click (object sender, RoutedEventArgs e)
    {
      MainProject.Save ();
    }


    private void CloseProject_Click (object sender, RoutedEventArgs e)
    {
      if (MainProject.ChangesMade)
      {
        var m = MessageBox.Show ("Save changes to " + MainProject.ProjectFileName,
                                 "Unsaved changed", MessageBoxButton.YesNoCancel,
                                 MessageBoxImage.Warning);
        if (m == MessageBoxResult.Yes)
          MainProject.Save ();
        if (m == MessageBoxResult.Cancel)
          return;
      }
      MainProject.Close ();
    }


    private void Quit_Click (object sender, RoutedEventArgs e)
    {
      Application.Current.MainWindow.Close ();
    }


    private void MainWindow_Close (object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (MainProject.ChangesMade)
      {
        var m = MessageBox.Show ("Save changes to " + MainProject.ProjectFileName,
                                 "Unsaved changed", MessageBoxButton.YesNoCancel,
                                 MessageBoxImage.Warning);
        if (m == MessageBoxResult.Yes)
          MainProject.Save ();
        if (m == MessageBoxResult.Cancel)
          e.Cancel = true;
      }
      Logging.Close ();
    }


    private void SaveStations_Click (object sender, RoutedEventArgs e)
    {
      var window = new UI.SaveStationEditor (MainProject);
      window.Owner = Window.GetWindow (this);
      window.ShowDialog ();
    }

  } // partial class MainWindow

}
