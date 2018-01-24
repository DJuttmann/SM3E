﻿using System;
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

namespace SM3E
{

//========================================================================================
// CLASS TILEVIEWER
//========================================================================================


  class UITileViewer
  {
    private static readonly GridLength gridSize =
      new GridLength (1.0, GridUnitType.Star);

    private ScrollViewer Parent;
    private Grid MainGrid;
//    private Image TileMap;
    private Border Selection;
    public double TileSize {get; private set;}
    public int RowCount {get; private set;}
    public int ColCount {get; private set;}
    public int ScreenCountH {get; private set;}
    public int ScreenCountV {get; private set;}
    public int ScreenWidth {get; private set;}
    public int ScreenHeight {get; private set;}
    public Image [,] Screens {get; private set;}

    private bool mouseOver = false;
    private bool mouseDown = false;
    public int TileClickX {get; private set;}
    public int TileClickY {get; private set;}
    public int TileOverX {get; private set;}
    public int TileOverY {get; private set;}

    public event ViewportEventHandler ViewportChanged;
    public event TileViewerMouseEventHandler MouseDown;
    public event TileViewerMouseEventHandler MouseDrag;
    public event TileViewerMouseEventHandler MouseUp;

    public double Width
    {
      get {return MainGrid.Width;}
    }

    public double Height
    {
      get {return MainGrid.Height;}
    }

    public UIElement Element
    {
      get {return MainGrid;}
    }

    public Color BackgroundColor
    {
      set
      {
        MainGrid.Background = new SolidColorBrush (value);
      }
    }


    // Constructor;
    public UITileViewer (double tileSize, int colCount, int rowCount,
                         int screenWidth, int screenHeight, UIElement parent)
    {
      Parent = parent is ScrollViewer s ? s : null;
      MainGrid = new Grid ();
      Selection = new Border ();
      Selection.BorderBrush = new SolidColorBrush (Color.FromRgb (0xFF, 0xFF, 0xFF));
      Selection.BorderThickness = new Thickness (1);

      ScreenWidth = screenWidth;
      ScreenHeight = screenHeight;
      SetSize (rowCount, colCount, tileSize);

      MainGrid.PreviewMouseMove += TileViewer_MouseMove;
      MainGrid.PreviewMouseDown += TileViewer_MouseDown;
      MainGrid.PreviewMouseUp += TileViewer_MouseUp;

      TileClickX = 0;
      TileClickY = 0;
      TileOverX = 0;
      TileOverY = 0;

      if (Parent != null)
      {
        Parent.ScrollChanged += ScrollHandler;
      }
    }


    // Update the size after row/col count or tile size.
    public void SetSize (int rowCount, int colCount, double tileSize)
    {
      RowCount = rowCount;
      ColCount = colCount;
      TileSize = tileSize;
      ScreenCountH = colCount / ScreenWidth;
      ScreenCountV = rowCount / ScreenHeight;
      MainGrid.Children.Clear ();

      // Setup screens.
      Screens = new Image [ScreenCountH, ScreenCountV];
      for (int y = 0; y < ScreenCountV; y++)
      {
        for (int x = 0; x < ScreenCountH; x++)
        {
          var newImage = new Image ();
          newImage.SetValue (Grid.ColumnProperty, ScreenWidth * x);
          newImage.SetValue (Grid.RowProperty, ScreenHeight * y);
          newImage.SetValue (Grid.ColumnSpanProperty, ScreenWidth);
          newImage.SetValue (Grid.RowSpanProperty, ScreenHeight);
          Screens [x, y] = newImage;
          MainGrid.Children.Add (newImage);
        }
      }

      // Setup main grid.
      MainGrid.Width = tileSize * colCount;
      MainGrid.Height = tileSize * RowCount;
      MainGrid.ColumnDefinitions.Clear ();
      MainGrid.RowDefinitions.Clear ();
      for (int col = 0; col < colCount; col++)
      {
        ColumnDefinition colDef = new ColumnDefinition ();
        colDef.Width = gridSize;
        MainGrid.ColumnDefinitions.Add (colDef);
      }
      for (int row = 0; row < rowCount; row++)
      {
        RowDefinition rowDef = new RowDefinition ();
        rowDef.Height = gridSize;
        MainGrid.RowDefinitions.Add (rowDef);
      }

      // Add selection.
      MainGrid.Children.Add (Selection);
    }


    // Reload the tiles currently visible (artificial scroll event.)
    public void ReloadVisibleTiles ()
    {
      ScrollHandler (null, null);
    }


//========================================================================================
// Event handlers


    private void TileViewer_MouseMove (object sender, MouseEventArgs e)
    {
      Point p = e.GetPosition (MainGrid);
      int newX = Convert.ToInt32 (Math.Floor (p.X / TileSize));
      int newY = Convert.ToInt32 (Math.Floor (p.Y / TileSize));
      if (newX < 0 || newX >= ColCount || newY < 0 || newY >= RowCount)
      {
        if (!mouseDown)
          Selection.Visibility = Visibility.Hidden;
        mouseOver = false;
        return;
      }
      Selection.Visibility = Visibility.Visible;
      mouseOver = true;
      if (System.Windows.Input.Mouse.LeftButton != MouseButtonState.Pressed)
        mouseDown = false;
      TileOverX = newX;
      TileOverY = newY;

      if (mouseDown)
      {
        Selection.SetValue (Grid.RowProperty, Math.Min (TileOverY, TileClickY));
        Selection.SetValue (Grid.ColumnProperty, Math.Min (TileOverX, TileClickX));
        Selection.SetValue (Grid.RowSpanProperty, Math.Abs (TileOverY - TileClickY) + 1);
        Selection.SetValue (Grid.ColumnSpanProperty, Math.Abs (TileOverX - TileClickX) + 1);
      }
      else
      {
        Selection.SetValue (Grid.RowProperty, TileOverY);
        Selection.SetValue (Grid.ColumnProperty, TileOverX);
        Selection.SetValue (Grid.RowSpanProperty, 1);
        Selection.SetValue (Grid.ColumnSpanProperty, 1);
      }
    }


    private void TileViewer_MouseDown (object sender, MouseButtonEventArgs e)
    {
      if (!mouseOver)
        return;
      mouseDown = true;
      Point p = e.GetPosition (MainGrid);
      TileClickX = Convert.ToInt32 (Math.Floor (p.X / TileSize));
      TileClickY = Convert.ToInt32 (Math.Floor (p.Y / TileSize));
      if (TileClickX < 0)
        TileClickX = 0;
      if (TileClickX >= ColCount)
        TileClickX = ColCount;
      if (TileClickY < 0)
        TileClickY = 0;
      if (TileClickY >= RowCount)
        TileClickY = RowCount;

      var a = new TileViewerMouseEventArgs ()
      {
        ClickX = p.X,
        ClickY = p.Y,
        ClickTileX = TileClickX,
        ClickTileY = TileClickY,
        Button = e.ChangedButton
      };
      MouseDown?.Invoke (this, a);
    }


    private void TileViewer_MouseUp (object sender, MouseButtonEventArgs e)
    {
      mouseDown = false;

      Point p = e.GetPosition (MainGrid);
      TileOverX = Convert.ToInt32 (Math.Floor (p.X / TileSize));
      TileOverY = Convert.ToInt32 (Math.Floor (p.Y / TileSize));
      if (TileOverX < 0)
        TileOverX = 0;
      if (TileOverX >= ColCount)
        TileOverX = ColCount;
      if (TileOverY < 0)
        TileOverY = 0;
      if (TileOverY >= RowCount)
        TileOverY = RowCount;

      var a = new TileViewerMouseEventArgs ()
      {
        ClickX = 0.0, // [wip]
        ClickY = 0.0, // [wip]
        ClickTileX = TileClickX,
        ClickTileY = TileClickY,
        PosX = p.X,
        PosY = p.Y,
        PosTileX = TileOverX,
        PosTileY = TileOverY,
        Button = e.ChangedButton
      };
      MouseUp?.Invoke (this, a);
    }


    private void ScrollHandler (object sender, ScrollChangedEventArgs e)
    {
      int startScreenX = Convert.ToInt32 (
        Math.Floor (Parent.HorizontalOffset / (TileSize * ScreenWidth)));
      int startScreenY = Convert.ToInt32 (
        Math.Floor (Parent.VerticalOffset / (TileSize * ScreenHeight)));
      int endScreenX = 1 + Convert.ToInt32 (
        Math.Floor ((Parent.HorizontalOffset + Parent.ViewportWidth) / (TileSize * ScreenWidth)));
      int endScreenY = 1 + Convert.ToInt32 (
        Math.Floor ((Parent.VerticalOffset + Parent.ViewportHeight) / (TileSize * ScreenHeight)));

      if (startScreenX < 0)
        startScreenX = 0;
      if (startScreenY < 0)
        startScreenY = 0;
      if (endScreenX >= ScreenCountH)
        endScreenX = ScreenCountH - 1;
      if (endScreenY >= ScreenCountV)
        endScreenY = ScreenCountV - 1;

      var a = new ViewportEventArgs ()
      {
        StartScreenX = startScreenX,
        StartScreenY = startScreenY,
        EndScreenX = endScreenX,
        EndScreenY = endScreenY
      };
      ViewportChanged?.Invoke (this, a);
    }

  } // class TileViewer


//========================================================================================
// TileViewer events


  public delegate void ViewportEventHandler (object sender, ViewportEventArgs e);


  public class ViewportEventArgs: EventArgs
  {
    public int StartScreenX;
    public int StartScreenY;
    public int EndScreenX;
    public int EndScreenY;

    public ViewportEventArgs () {}
  }


  public delegate void TileViewerMouseEventHandler (object sender, TileViewerMouseEventArgs e);


  public class TileViewerMouseEventArgs: EventArgs
  {
    public MouseButton Button;
    public double ClickX;
    public double ClickY;
    public double PosX;
    public double PosY;
    public int ClickTileX;
    public int ClickTileY;
    public int PosTileX;
    public int PosTileY;

    public TileViewerMouseEventArgs () {}
  }

}