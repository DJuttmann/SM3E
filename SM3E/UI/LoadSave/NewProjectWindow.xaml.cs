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
using System.Windows.Shapes;
using System.IO;

namespace SM3E.UI
{
  /// <summary>
  /// Interaction logic for NewProjectWindow.xaml
  /// </summary>
  public partial class NewProjectWindow: Window
  {
    const string TemplatePath = "Data\\Project Templates";
    Project MainProject;
    FileInfo [] TemplateFiles;


    public NewProjectWindow (Project p)
    {
      InitializeComponent ();

      MainProject = p;

      DirectoryInfo directory = new DirectoryInfo (TemplatePath);
      TemplateFiles = directory.GetFiles ("*.sm3p", SearchOption.AllDirectories);
      string [] names = new string [TemplateFiles.Length];
      for (int i = 0; i < names.Length; i++)
      {
        string s = TemplateFiles [i].Name;
        Tools.TrimFileExtension (ref s, out string ext);
        names [i] = s;
      }

      TemplateSelect.ItemsSource = names;
      if (TemplateFiles.Length > 0)
        TemplateSelect.SelectedIndex = 0;
    }


    private void SelectPath_Click (object sender, RoutedEventArgs e)
    {
      var Save = new Microsoft.Win32.SaveFileDialog ()
      {
        Filter = "Project files (*.sm3p)|*.sm3p"
        // InitialDirectory = Directory.GetCurrentDirectory ()
      };
      if (Save.ShowDialog (Window.GetWindow (this)) ?? false)
      {
        string savePath = Save.FileName;
        PathInput.Text = savePath;
        PathInput.ScrollToEnd ();
        if (RomFileInput.Text.Length == 0)
        {
          string filename = Tools.FilenameFromPath (savePath);
          Tools.TrimFileExtension (ref filename, out string extension);
          RomFileInput.Text = filename + ".sfc";
        }
      }
    }


    private void Cancel_Click (object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close ();
    }


    private void Create_Click (object sender, RoutedEventArgs e)
    {
      if (TemplateSelect.SelectedIndex == -1)
        return;
      FileInfo loadTemplate = TemplateFiles [TemplateSelect.SelectedIndex];
      MainProject.Load (loadTemplate.FullName);


      MainProject.ProjectPath = Tools.FolderFromPath (PathInput.Text);
      MainProject.ProjectFileName = Tools.FilenameFromPath (PathInput.Text);
      MainProject.RomFileName = RomFileInput.Text;

      DialogResult = true;
      Close ();
    }

  }

}