using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using Compiler;
using Compiler.Parser;
using Compiler.Tokenizador;
using System;
using System.IO;
using Avalonia.Platform.Storage;

namespace Visual;

public partial class MainWindow : Window, ICanvasInfo
{
    private int dimensions;
    public int Dimensions
    {
        get => dimensions;
        set => dimensions = value >= 0 ? value : throw new Exception();
    }
    public Border Walle { get; set; } = new();
    public Dictionary<(int, int), Border> MyBorders { get; } = [];

    FunctionMethods functions;
    ActionMethods actions;

    public MainWindow()
    {
        InitializeComponent();
        functions = new FunctionMethods(this);
        actions = new ActionMethods(this);
    }
    public void OpenWorkWindow(object sender, RoutedEventArgs e)
    {
        StartWindow.IsVisible = false;
        WorkWindow.IsVisible = true;
    }
    public void GenerateBoard(object sender, RoutedEventArgs e)
    {
        MyBorders.Clear();
        MyCanvas.ColumnDefinitions.Clear();
        MyCanvas.RowDefinitions.Clear();
        MyCanvas.Children.Clear();
        {
            if (int.TryParse(MyDimension.Text, out int dimensions))
                Dimensions = dimensions;

            else { Dimensions = 10; }

            for (int i = 0; i < Dimensions; i++)
            {
                MyCanvas.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                WalleCanvas.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            for (int i = 0; i < Dimensions; i++)
            {
                MyCanvas.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                WalleCanvas.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }

            // Agregar elementos al Grid
            for (int row = 0; row < Dimensions; row++)
            {
                for (int col = 0; col < Dimensions; col++)
                {
                    var cell = new Border
                    {
                        Name = $"cell({row},{col})",
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(0.4),
                    };
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    MyCanvas.Children.Add(cell);
                    MyBorders.Add((row, col), cell);
                }
            }
        }

    }
    public void TextEditor_TextChanged(object sender, EventArgs e)
    {
        // TODO Probarlo bien con todas las funciones del pdf y mostrar errores parser y tokenizador
        var parser = new Parser();
        var context = new Context(functions, actions);
        var tokenizador = new Tokenizador();

        var code = textEditor.Text;
        var tokens = tokenizador.Tokenizar(code);
        var ast = parser.Parse(tokens);    //es ast pero es un bloque


        // Añadir lista de errores en el visual
        // StringBuilder sb = new();
        // foreach (var item in parser.ParserErrors)
        // {
        //     sb.AppendLine(item.Message);
        // }
        // TODO ErrorArea.Text = sb.ToString();
    }
    public void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        var parser = new Parser();
        var context = new Context(functions, actions);
        var tokenizador = new Tokenizador(); //TODO: Convertir a no estatico

        var code = textEditor.Text;
        var tokens = tokenizador.Tokenizar(code);
        var ast = parser.Parse(tokens);    //es ast pero es un bloque  
        ast.Excute(context);
    }
    private async void SaveDocument(object? sender, RoutedEventArgs e)
    {
        var savePicker = new FilePickerSaveOptions
        {
            Title = "Save file",
            SuggestedFileName = "nuevo_archivo.pw",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new("Archivo protegido")
                {
                    Patterns = new[] { "*.pw" }
                }
            }
        };

        var file = await StorageProvider.SaveFilePickerAsync(savePicker);

        if (file != null)
        {
            var stream = await file.OpenWriteAsync();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(textEditor.Text);
        }
    }
    private async void ChargeDocument(object? sender, RoutedEventArgs e)
    {
        var openPicker = new FilePickerOpenOptions
        {
            Title = "Open file",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Archivo protegido")
                {
                    Patterns = new[] { "*.pw" }
                }
            }
        };

        var files = await StorageProvider.OpenFilePickerAsync(openPicker);

        if (files.Count > 0)
        {
            var file = files[0];

            var stream = await file.OpenReadAsync();
            var reader = new StreamReader(stream);
            string contenido = await reader.ReadToEndAsync();

            textEditor.Text = contenido;
        }
    }

    public void CreateWalle()
    {
        RemoveWalle();
        var bitmap = new Bitmap(@"C:/Users/Kevin Emilio/Programación/Proyectos/Pixel-Wall-E/Visual/Assets/Wall_E.png");
        var wallPicture = new ImageBrush
        {
            Source = bitmap,
            Stretch = Stretch.UniformToFill
        };
        Walle = new Border
        {
            Background = wallPicture,
            BorderThickness = new Thickness(1),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            ZIndex = 3,
        };
        WalleCanvas.Children.Add(Walle);
    }
    public void RemoveWalle()
    {
        if (WalleCanvas.Children.Contains(Walle))
        {
            WalleCanvas.Children.Remove(Walle);
        }
    }
    public void SetWalle(int x, int y)
    {

        if (x >= Dimensions)
            Grid.SetColumn(Walle, Dimensions - 1);
        if (x < 0)
            Grid.SetColumn(Walle, 0);
        else { Grid.SetColumn(Walle, x); }

        if (y >= Dimensions)
            Grid.SetRow(Walle, Dimensions - 1);
        if (y < 0)
            Grid.SetRow(Walle, 0);
        else { Grid.SetRow(Walle, y); }

        // if (Walle.Parent is Panel parent)
        //     parent.Children.Remove(Walle);
        // WalleCanvas.Children.Add(Walle);

        // MyCanvas.Children.Add(Walle); iba aqui pero lo quite porque sino cuando seteara a walle 
        // con este metodo iba a crear otro border
    }
}

public interface ICanvasInfo
{
    Border Walle { get; set; }
    int Dimensions { get; }
    Dictionary<(int, int), Border> MyBorders { get; }
    void CreateWalle();
    void RemoveWalle();
    void SetWalle(int x, int y);
}
public class FunctionMethods(ICanvasInfo info) : IContextFunction
{
    private readonly ICanvasInfo _info = info;

    public int GetActualY() => Grid.GetRow(_info.Walle);
    public int GetActualX() => Grid.GetColumn(_info.Walle);
    public int GetCanvasSize() => _info.Dimensions;
    public int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        int count = 0;
        if (x1 < 0 || x1 > _info.Dimensions ||
            y1 < 0 || y1 > _info.Dimensions ||
            x2 < 0 || x2 > _info.Dimensions ||
            y2 < 0 || y2 > _info.Dimensions) return count;

        int majorX = x1 > x2 ? x1 : x2;
        int minorX = x1 + x2 - majorX;
        int majorY = y1 > y2 ? y1 : y2;
        int minorY = y1 + y2 - majorY;
        var MyColor = new SolidColorBrush(Color.Parse(color));

        for (int i = minorX; i <= majorX; i++)
        {
            for (int j = minorY; j <= majorY; j++)
            {
                if (_info.MyBorders[(i, j)].Background == MyColor)
                    count++;
            }
        }
        return count;
    }
    public int IsBrushColor(string color) => Brush.Color == color ? 1 : 0;
    public int IsBrushSize(int size) => Brush.Size == size ? 1 : 0;
    public int IsCanvasColor(string color, int vertical, int horizontal)
    {
        int fila = Grid.GetRow(_info.Walle) + horizontal;
        int columna = Grid.GetColumn(_info.Walle) + vertical;
        var MyColor = new SolidColorBrush(Color.Parse(color));
        if (fila < 0 || fila > _info.Dimensions ||
         columna < 0 || columna > _info.Dimensions) return 0;
        return _info.MyBorders[(fila, columna)].Background == MyColor ? 1 : 0;
    }
    public object CallFunction(string Name, object[] @params) => Name switch
    {
        //TODO hacer un switch por los metodos implementados en esta clase
        "GetActualY" => GetActualY(),
        "GetActualX" => GetActualX(),
        "GetCanvasSize" => GetCanvasSize(),
        "GetColorCount" => GetColorCount((string)@params[0], (int)@params[1], (int)@params[2], (int)@params[3], (int)@params[4]),
        "IsBrushColor" => IsBrushColor((string)@params[0]),
        "IsBrushSize" => IsBrushSize((int)@params[0]),
        "IsCanvasColor" => IsCanvasColor((string)@params[0], (int)@params[1], (int)@params[2]),
        _ => throw new NotImplementedException(),
    };

}
public class ActionMethods(ICanvasInfo info) : IContextAction
{
    private readonly ICanvasInfo _info = info;

    public void Spawn(int x, int y)
    {
        _info.CreateWalle();
        _info.SetWalle(x, y);

        // Walle = new Border
        // {
        //     Width = cellWidth * 0.2, // 20% del ancho de la celda
        //     Height = cellHeight * 0.2, // 20% del alto de la celda
        //     Background = Brushes.Blue,
        //     BorderThickness = new Thickness(1),
        //     HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        //     VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
        // };
    }
    public void BrushColor(string color)
    {
        switch (color)
        {
            case "Red":
                Brush.Color = color;
                break;
            case "Blue":
                Brush.Color = color;
                break;
            case "Green":
                Brush.Color = color;
                break;
            case "Yellow":
                Brush.Color = color;
                break;
            case "Orange":
                Brush.Color = color;
                break;
            case "Purple":
                Brush.Color = color;
                break;
            case "Black":
                Brush.Color = color;
                break;
            case "White" or "Transparent":
                Brush.Color = "White";
                break;
        }
    }
    public void Size(int k)
    {
        if (k % 2 == 0)
        {
            Brush.Size = k - 1;
            return;
        }
        if (k <= 0)
        {
            Brush.Size = 1;
            return;
        }
        else Brush.Size = k;

    }
    //    TODO hacer que pinte la linea usando los diferntes tamanos de brocha
    public void DrawLine(int dirX, int dirY, int distance) //en realidad dirX es col y dirY es row
    {
        if (dirX != -1 && dirX != 0 && dirX != 1) return;
        if (dirY != -1 && dirY != 0 && dirY != 1) return;
        int fila = Grid.GetRow(_info.Walle);
        int columna = Grid.GetColumn(_info.Walle);
        int i = 0;
        while (i <= distance)
        {
            int MyFilas = fila + dirY * i;
            int MyColumnas = columna + dirX * i;
            _info.SetWalle(MyColumnas, MyFilas);
            // Grid.SetRow(_info.Walle, MyFilas);
            // Grid.SetColumn(_info.Walle, MyColumnas);
            var myColorBrush = new SolidColorBrush(Color.Parse(Brush.Color));
            _info.MyBorders[(MyFilas, MyColumnas)].Background = myColorBrush;
            if (i == 0)
            {
                if (dirX == 1 && dirY != 0 || dirX == -1 && dirY != 0 ||
              dirX != 0 && dirY == 1 || dirX != 0 && dirY == -1)
                {
                    i++;
                    continue;
                }
            }
            Paint(dirX, dirY, Brush.Size, myColorBrush, i + 1, null);
            i++;
        }
        // por si no funciona el metodo sin especificar el grid
        // if (miGrid.Children.Contains(_info.Walle))
        // {
        // int fila = Grid.GetRow(_info.Walle);
        // int columna = Grid.GetColumn(_info.Walle);
        // Console.WriteLine($"Está dentro de miGrid en fila {fila}, columna {columna}");
        // }
    }
    public void Paint(int dirX, int dirY, int size, SolidColorBrush myColorBrush, int? myCount, string? form)
    {
        int col = Grid.GetColumn(_info.Walle);
        int row = Grid.GetRow(_info.Walle);
        int aumento = size / 2;

        //para circulos
        if (form is not null)
        {
            for (int i = col - aumento; i <= col + aumento; i++)
            {
                for (int j = row - aumento; j <= row + aumento; j++)
                {
                    _info.MyBorders[(i, j)].Background = myColorBrush;
                }
            }
        }

        //para lineas hacia los lados y arriba abajo
        if (dirX == 1 && dirY == 0 || dirX == -1 && dirY == 0 || dirX == 0 && dirY == 1 || dirX == 0 && dirY == -1)
        {

            int count = 1;
            while (count <= size / 2)
            {
                _info.MyBorders[(col + dirY * count, row + dirX * count)].Background = myColorBrush;
                _info.MyBorders[(col - dirY * count, row - dirX * count)].Background = myColorBrush;
                count++;
            }
            return;
        }
        //para lineas hacia las diagonales
        //para cuando distancia es menor que el size de la brocha
        if (myCount < Brush.Size)
        {
            for (int j = 1; j <= myCount - 1; j++)
            {
                _info.MyBorders[(col - dirX * j, row)].Background = myColorBrush;
                _info.MyBorders[(col, row - dirY * j)].Background = myColorBrush;

            }
            return;
        }
        //para cuando distancia es mayor o igual que el size de la brocha
        for (int j = 1; j < Brush.Size - 1; j++)
        {
            _info.MyBorders[(col - dirX * j, row)].Background = myColorBrush;
            _info.MyBorders[(col, row - dirY * j)].Background = myColorBrush;

        }
    }
    public void DrawCircle(int dirX, int dirY, int radius)
    {
        if (dirX != -1 && dirX != 0 && dirX != 1) return;
        if (dirY != -1 && dirY != 0 && dirY != 1) return;
        int WalleY = Grid.GetRow(_info.Walle);
        int WalleX = Grid.GetColumn(_info.Walle);
        var myColorBrush = new SolidColorBrush(Color.Parse(Brush.Color));
        // set walle psition
        int MyFilas = WalleY + dirY * radius;
        int MyColumnas = WalleX + dirX * radius;
        if (MyFilas < 0 || MyFilas > _info.Dimensions - 1 || MyColumnas < 0 ||
            MyColumnas > _info.Dimensions - 1)
            return;
        _info.SetWalle(MyColumnas, MyFilas);
        // Grid.SetRow(_info.Walle, MyFilas);
        // Grid.SetColumn(_info.Walle, MyColumnas);
        DrawCircle(MyColumnas, MyFilas, radius, myColorBrush);
        _info.SetWalle(MyColumnas, MyFilas);
        // Grid.SetRow(_info.Walle, MyFilas);
        // Grid.SetColumn(_info.Walle, MyColumnas);
    }
    public void DrawCircle(int centerX, int centerY, int radius, SolidColorBrush myColorBrush)
    {
        int x = radius + 1,
        y = 0;
        int error = 1 - x;

        while (x >= y)
        {
            PlotCircleCell(centerX, centerY, x, y, myColorBrush);
            y++;

            if (error < 0)
            {
                error += 2 * y + 1;
            }
            else
            {
                x--;
                error += 2 * (y - x) + 1;
            }
        }
    }
    public void PlotCircleCell(int cx, int cy, int x, int y, SolidColorBrush myColorBrush)
    {
        _info.SetWalle(cx + x, cy + y);
        // Grid.SetColumn(_info.Walle, cx + x);
        // Grid.SetRow(_info.Walle, cy + y);
        _info.MyBorders[(cx + x, cy + y)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx + x, cy + y, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx - x, cy + y);
        // Grid.SetColumn(_info.Walle, cx - x);
        // Grid.SetRow(_info.Walle, cy + y);
        _info.MyBorders[(cx - x, cy + y)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx - x, cy + y, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx + x, cy - y);
        // Grid.SetColumn(_info.Walle, cx + x);
        // Grid.SetRow(_info.Walle, cy - y);
        _info.MyBorders[(cx + x, cy - y)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx + x, cy - y, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx - x, cy - y);
        // Grid.SetColumn(_info.Walle, cx - x);
        // Grid.SetRow(_info.Walle, cy - y);
        _info.MyBorders[(cx - x, cy - y)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx - x, cy - y, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx + y, cy + x);
        // Grid.SetColumn(_info.Walle, cx + y);
        // Grid.SetRow(_info.Walle, cy + x);
        _info.MyBorders[(cx + y, cy + x)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx + y, cy + x, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx - y, cy + x);
        // Grid.SetColumn(_info.Walle, cx - y);
        // Grid.SetRow(_info.Walle, cy + x);
        _info.MyBorders[(cx - y, cy + x)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx - y, cy + x, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx + y, cy - x);
        // Grid.SetColumn(_info.Walle, cx + y);
        // Grid.SetRow(_info.Walle, cy - x);
        _info.MyBorders[(cx + y, cy - x)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx + y, cy - x, Brush.Size, myColorBrush, null, "circle");

        _info.SetWalle(cx - y, cy - x);
        // Grid.SetColumn(_info.Walle, cx - y);
        // Grid.SetRow(_info.Walle, cy - x);
        _info.MyBorders[(cx - y, cy - x)].Background = myColorBrush;
        if (Brush.Size > 1)
            Paint(cx - y, cy - x, Brush.Size, myColorBrush, null, "circle");

    }
    public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
    {
        if (dirX != -1 && dirX != 0 && dirX != 1) return;
        if (dirY != -1 && dirY != 0 && dirY != 1) return;
        int WalleY = Grid.GetRow(_info.Walle);
        int WalleX = Grid.GetColumn(_info.Walle);
        int MyFilas = WalleY + dirY * distance;
        int MyColumnas = WalleX + dirX * distance;
        if (MyFilas < 0 || MyFilas > _info.Dimensions - 1 || MyColumnas < 0 ||
            MyColumnas > _info.Dimensions - 1)
            return;
        Grid.SetRow(_info.Walle, MyFilas);
        Grid.SetColumn(_info.Walle, MyColumnas);

        int upParalelRow = 0;
        int leftParalelCol = 0;

        if (height % 2 == 0)
            upParalelRow = MyFilas - height / 2 + 1;
        if (height % 2 != 0)
            upParalelRow = MyFilas - height / 2;
        if (width % 2 == 0)
            leftParalelCol = MyColumnas - width / 2 + 1;
        if (width % 2 != 0)
            leftParalelCol = MyColumnas - width / 2;

        if (upParalelRow < 0 || upParalelRow > _info.Dimensions ||
            leftParalelCol < 0 || leftParalelCol > _info.Dimensions) return;

        Grid.SetRow(_info.Walle, upParalelRow);
        Grid.SetColumn(_info.Walle, leftParalelCol);

        DrawLine(1, 0, width);
        DrawLine(0, 1, height);
        DrawLine(-1, 0, width);
        DrawLine(0, -1, height);

        Grid.SetRow(_info.Walle, MyFilas);
        Grid.SetColumn(_info.Walle, MyColumnas);

    }
    public void Fill()
    {
        bool[,] mask = new bool[_info.Dimensions, _info.Dimensions];
        (int, int)[] direction = [(1, 0), (-1, 0), (0, 1), (-1, 0)];

        Fill(mask, direction);
    }
    public void Fill(bool[,] mask, (int, int)[] direction)
    {
        int WalleX = Grid.GetColumn(_info.Walle);
        int WalleY = Grid.GetRow(_info.Walle);
        mask[WalleY, WalleX] = true;
        var color = _info.MyBorders[(WalleX, WalleY)].Background;
        var myColorBrush = new SolidColorBrush(Color.Parse(Brush.Color));
        _info.MyBorders[(WalleX, WalleY)].Background = myColorBrush;

        for (int i = 0; i < direction.Length; i++)
        {
            var nextX = WalleX + direction[i].Item1;
            var nextY = WalleY + direction[i].Item2;
            if (nextX < 0 || nextX >= _info.Dimensions || nextY < 0 || nextY >= _info.Dimensions)
                return;
            if (_info.MyBorders[(nextX, nextY)].Background != color)
                return;
            if (mask[nextY, nextX]) //TODO ver si es Walle Y-X o Next Y-X
                return;
            _info.SetWalle(nextX, nextY);
            // Grid.SetColumn(_info.Walle, nextX);
            // Grid.SetRow(_info.Walle, nextY);
            Fill(mask, direction);
        }
    }
    public void CallAction(string Name, object[] @params)
    {
        switch (Name)
        {
            case "Spawn":
                Spawn((int)@params[0], (int)@params[1]);
                return;
            case "BrushColor":
                BrushColor((string)@params[0]);
                return;
            case "Size":
                Size((int)@params[0]);
                return;
            case "DrawLine":
                DrawLine((int)@params[0], (int)@params[1], (int)@params[2]);
                return;
            case "DrawCircle":
                DrawCircle((int)@params[0], (int)@params[1], (int)@params[2]);
                return;
            case "DrawRectangle":
                DrawRectangle((int)@params[0], (int)@params[1], (int)@params[2], (int)@params[3], (int)@params[4]);
                return;
            case "Fill":
                Fill();
                return;
            default: throw new NotImplementedException();
        }
    }

    //TODO hacer un switch por los metodos implementados en esta clase
}
