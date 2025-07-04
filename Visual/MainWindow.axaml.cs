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
using Avalonia.Platform;
using Compiler.Language;
using Avalonia.Input;
using System.Text;
using Location = Compiler.Language.Location;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;


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
    public List<Exception> exceptions = [];
    FunctionMethods functions;
    ActionMethods actions;

    public MainWindow()
    {
        InitializeComponent();
        functions = new FunctionMethods(this);
        actions = new ActionMethods(this);
        this.KeyDown += OnKeyDown;
    }
    public void OpenWorkWindow(object sender, RoutedEventArgs e)
    {
        StartWindow.IsVisible = false;
        WorkWindow.IsVisible = true;
    }
    public void GenerateBoard(object sender, RoutedEventArgs e)
    {
        MyBorders.Clear();
        RemoveWalle();
        MyCanvas.ColumnDefinitions.Clear();
        MyCanvas.RowDefinitions.Clear();
        MyCanvas.Children.Clear();
        WalleCanvas.ColumnDefinitions.Clear();
        WalleCanvas.RowDefinitions.Clear();
        WalleCanvas.Children.Clear();
        errorBar.Text = "";
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
                    MyBorders.Add((col, row), cell);
                }
            }
        }

    }
    public void TextEditor_TextChanged(object sender, EventArgs e)
    {
        errorBar.Text = "";
        exceptions.Clear();

        var parser = new Parser();
        var tokenizador = new Tokenizador();

        var code = textEditor.Text;
        var tokens = tokenizador.Tokenizar(code);
        var ast = parser.Parse(tokens);    //es ast pero es un bloque

        var sb = new StringBuilder();
        if (Dimensions == 0)
            exceptions.Add(new Exception("Debe generar el tablero antes de ejecutar el programa"));

        if ((ast as Block)!.Instructions.Count == 0 || (ast as Block)!.Instructions[0] is not ActionMethod method || !method.Name.Equals("spawn", StringComparison.CurrentCultureIgnoreCase))
            exceptions.Add(new Exception("Debe comenzar el bloque de instrucciones con la instruccion 'Spawn' "));
        else if (method.Name != "Spawn")
            exceptions.Add(new Exception("El metodo 'Spawn' ha sido mal llamado"));

        foreach (var err in exceptions.Concat(parser.ParserErrors).Concat(tokenizador.tokenException))
        {
            sb.AppendLine($"{err.Message}");
        }

        errorBar.Document.Text = sb.ToString();
    }
    public void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        errorBar.Text = "";
        exceptions.Clear();

        var parser = new Parser();
        var context = new Context(functions, actions);
        var tokenizador = new Tokenizador();

        var code = textEditor.Text;
        var tokens = tokenizador.Tokenizar(code);
        var ast = parser.Parse(tokens);

        var sb = new StringBuilder();

        if (Dimensions == 0)
            exceptions.Add(new Exception("Debe generar el tablero antes de ejecutar el programa"));

        if ((ast as Block)!.Instructions.Count == 0 || (ast as Block)!.Instructions[0] is not ActionMethod method || !method.Name.Equals("spawn", StringComparison.CurrentCultureIgnoreCase))
            exceptions.Add(new Exception("Debe comenzar el bloque de instrucciones con la instruccion 'Spawn' "));
        else if (method.Name != "Spawn")
            exceptions.Add(new Exception("El metodo 'Spawn' ha sido mal llamado"));

        foreach (var err in exceptions.Concat(parser.ParserErrors).Concat(tokenizador.tokenException))
        {
            sb.AppendLine($"{err.Message}");
        }

        try
        {
            ast.Excute(context);
        }
        catch (Exception exc)
        {
            exc = exc is TargetInvocationException ? exc.InnerException! : exc;
            // Mensaje principal de la excepción atrapada
            sb.AppendLine($"{exc.Message}");
        }
        finally
        {
            errorBar.Document.Text = sb.ToString();
            ProblemWindow.IsVisible = true;
        }
    }

    private async void SaveDocument(object? sender, RoutedEventArgs e)
    {
        var savePicker = new FilePickerSaveOptions
        {
            Title = "Save file",
            SuggestedFileName = "nuevo_archivo.pw",
            FileTypeChoices =
            [
                new("Archivo protegido")
                {
                    Patterns = ["*.pw"]
                }
            ]
        };

        var file = await StorageProvider.SaveFilePickerAsync(savePicker);

        if (file != null)
        {
            using var stream = await file.OpenWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(textEditor.Text);
        }
    }
    private async void ChargeDocument(object? sender, RoutedEventArgs e)
    {
        var openPicker = new FilePickerOpenOptions
        {
            Title = "Open file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new("Archivo protegido")
                {
                    Patterns = ["*.pw"]
                }
            ]
        };

        var files = await StorageProvider.OpenFilePickerAsync(openPicker);

        if (files.Count > 0)
        {
            var file = files[0];

            using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            string contenido = await reader.ReadToEndAsync();
            textEditor.Text = contenido;
        }
    }
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (StartWindow.IsVisible && e.Key == Key.Enter)
        {
            StartWindow.IsVisible = false;
            WorkWindow.IsVisible = true;
            return;
        }

        // Solo activar Ctrl+J si la pantalla de WorkWindow esta en true
        if (!StartWindow.IsVisible && e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.J)
        {
            ProblemWindow.IsVisible = !ProblemWindow.IsVisible;
        }
    }

    public void CreateWalle()
    {
        Bitmap? bitmap = null;
        string assembyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "PIXEL-WALL-E";
        string uriString = $"avares://{assembyName}/Assets/Wall_E.png";
        Uri iconUri = new Uri(uriString);
        if (AssetLoader.Exists(iconUri))
        {
            using (var stream = AssetLoader.Open(iconUri))
            {
                bitmap = new Bitmap(stream);
            }
        }


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
        else if (x < 0)
            Grid.SetColumn(Walle, 0);
        else { Grid.SetColumn(Walle, x); }

        if (y >= Dimensions)
            Grid.SetRow(Walle, Dimensions - 1);
        else if (y < 0)
            Grid.SetRow(Walle, 0);
        else { Grid.SetRow(Walle, y); }

        // if (Walle.Parent is Panel parent)
        //     parent.Children.Remove(Walle);
        // WalleCanvas.Children.Add(Walle);

        // MyCanvas.Children.Add(Walle); iba aqui pero lo quite porque sino cuando seteara a walle 
        // con este metodo iba a crear otro border
    }
    public bool IsValidSetWalle() => WalleCanvas.Children.Contains(Walle);
    public Exception BuildExeptionMassege(string name, object?[] @params, string strParams, Location location)
    {
        var template = ExceptionTemplates.PARAMETERS1;
        string massege = string.Format(template, name, @params.Length, strParams, location);
        return new InvalidOperationException(massege);
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
    bool IsValidSetWalle();
    Exception BuildExeptionMassege(string name, object?[] @params, string strParams, Location location);
}
public class FunctionMethods(ICanvasInfo info) : IContextFunction
{
    private readonly ICanvasInfo _info = info;

    public int GetActualX() => Grid.GetColumn(_info.Walle);
    public int GetActualY() => Grid.GetRow(_info.Walle);
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
        string fixColor = "";

        for (int i = 1; i < color.Length - 1; i++)
            fixColor += color[i];

        var castColor = new SolidColorBrush(Color.Parse(fixColor));

        for (int i = minorX; i <= majorX; i++)
        {
            for (int j = minorY; j <= majorY; j++)
            {
                var cellColor = _info.MyBorders[(i, j)].Background as SolidColorBrush;
                if (cellColor?.Color == castColor?.Color)
                    count++;
            }
        }
        return count;
    }
    public int IsBrushColor(string color)
    {
        string fixColor = "";
        for (int i = 1; i < color.Length - 1; i++)
            fixColor += color[i];
        return Pincel.Color == fixColor ? 1 : 0;
    }
    public int IsBrushSize(int size) => Pincel.Size == size ? 1 : 0;
    public int IsCanvasColor(string color, int vertical, int horizontal)
    {
        int fila = Grid.GetRow(_info.Walle) + vertical;
        int columna = Grid.GetColumn(_info.Walle) + horizontal;
        if (fila < 0 || fila > _info.Dimensions - 1 || columna < 0 || columna > _info.Dimensions - 1)
            return 0;
        string fixColor = "";
        for (int i = 1; i < color.Length - 1; i++)
            fixColor += color[i];
        var MyColor = new SolidColorBrush(Color.Parse(fixColor));
        if (fila < 0 || fila > _info.Dimensions ||
         columna < 0 || columna > _info.Dimensions) return 0;
        var castColor = _info.MyBorders[(columna, fila)].Background as SolidColorBrush;
        return castColor?.Color == MyColor?.Color ? 1 : 0;
    }
    public object CallFunction(string Name, object?[] @params, Location location) => Name switch
    {
        "GetActualY" when @params.Length != 0 => throw _info.BuildExeptionMassege(Name, @params, "", location),
        "GetActualY" => GetActualY(),
        "GetActualX" when @params.Length != 0 => throw _info.BuildExeptionMassege(Name, @params, "", location),
        "GetActualX" => GetActualX(),
        "GetCanvasSize" when @params.Length != 0 => throw _info.BuildExeptionMassege(Name, @params, "", location),
        "GetCanvasSize" => GetCanvasSize(),
        "GetColorCount" when @params.Length != 5 => throw _info.BuildExeptionMassege(Name, @params, "string a, int b, int c, int d, int e", location),
        "GetColorCount" => GetColorCount((string)@params[0]!, (int)@params[1]!, (int)@params[2]!, (int)@params[3]!, (int)@params[4]!),
        "IsBrushColor" when @params.Length != 1 => throw _info.BuildExeptionMassege(Name, @params, "string a", location),
        "IsBrushColor" => IsBrushColor((string)@params[0]!),
        "IsBrushSize" when @params.Length != 1 => throw _info.BuildExeptionMassege(Name, @params, "int a", location),
        "IsBrushSize" => IsBrushSize((int)@params[0]!),
        "IsCanvasColor" when @params.Length != 3 => throw _info.BuildExeptionMassege(Name, @params, "string a, int b,int c", location),
        "IsCanvasColor" => IsCanvasColor((string)@params[0]!, (int)@params[1]!, (int)@params[2]!),
        _ => new Exception("El metodo no existe o ha sido mal llamado"),
    };

}
public class ActionMethods(ICanvasInfo info) : IContextAction
{
    private readonly ICanvasInfo _info = info;

    public void Spawn(int x, int y)
    {
        if (_info.IsValidSetWalle())
            throw new Exception("Solo puede invocar el metodo 'Spawn' una vez");
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
    public void Color(string color)
    {
        switch (color)
        {
            case "\"Crimson\"":
                Pincel.Color = "Crimson";
                return;
            case "\"Red\"":
                Pincel.Color = "Red";
                return;

            case "\"Dark Blue\"":
                Pincel.Color = "DarkBlue";
                return;
            case "\"Blue\"":
                Pincel.Color = "Blue";
                return;
            case "\"Dodger Blue\"":
                Pincel.Color = "DodgerBlue";
                return;
            case "\"Deep Sky Blue\"":
                Pincel.Color = "DeepSkyBlue";
                return;

            case "\"Dark Green\"":
                Pincel.Color = "DarkGreen";
                return;
            case "\"Green\"":
                Pincel.Color = "Green";
                return;
            case "\"Lime Green\"":
                Pincel.Color = "LimeGreen";
                return;

            case "\"Gold\"":
                Pincel.Color = "Gold";
                return;
            case "\"Yellow\"":
                Pincel.Color = "Yellow";
                return;
            case "\"Beige\"":
                Pincel.Color = "Beige";
                return;

            case "\"Dark Orange\"":
                Pincel.Color = "DarkOrange";
                return;
            case "\"Orange\"":
                Pincel.Color = "Orange";
                return;
            case "\"Light Salmon\"":
                Pincel.Color = "LightSalmon";
                return;

            case "\"Purple\"":
                Pincel.Color = "Purple";
                return;
            case "\"Blue Violet\"":
                Pincel.Color = "BlueViolet";
                return;
            case "\"Medium Purple\"":
                Pincel.Color = "MediumPurple";
                return;

            case "\"Deep Pink\"":
                Pincel.Color = "DeepPink";
                return;
            case "\"Hot Pink\"":
                Pincel.Color = "HotPink";
                return;
            case "\"Pink\"":
                Pincel.Color = "Pink";
                return;

            case "\"Dark Gray\"":
                Pincel.Color = "DarkGray";
                return;
            case "\"Slate Gray\"":
                Pincel.Color = "SlateGray";
                return;
            case "\"Black\"":
                Pincel.Color = "Black";
                return;

            case "\"White\"":
                Pincel.Color = "White";
                return;
            case "\"Transparent\"":
                Pincel.Color = "Transparent";
                return;
        }
    }
    public void Size(int k)
    {
        if (k % 2 == 0)
        {
            Pincel.Size = k - 1;
            return;
        }
        if (k <= 0)
        {
            Pincel.Size = 1;
            return;
        }
        else Pincel.Size = k;

    }
    public void DrawLine(int dirX, int dirY, int distance) //en realidad dirX es col y dirY es row
    {
        if (dirX != -1 && dirX != 0 && dirX != 1) return;
        if (dirY != -1 && dirY != 0 && dirY != 1) return;
        int fila = Grid.GetRow(_info.Walle);
        int columna = Grid.GetColumn(_info.Walle);
        int i = 0;
        //var endPos = _info.MyBorders[(columna + dirX * distance, fila + dirY * distance)].Background;
        while (i < distance)
        {
            int MyFilas = fila + dirY * i;
            int MyColumnas = columna + dirX * i;
            _info.SetWalle(MyColumnas, MyFilas);
            var myColorBrush = new SolidColorBrush(Avalonia.Media.Color.Parse(Pincel.Color));
            if (MyColumnas < 0 || MyColumnas > _info.Dimensions - 1 || MyFilas < 0 || MyFilas > _info.Dimensions - 1)
            {
                _info.SetWalle(columna + dirX * (i - 1), fila + dirY * (i - 1));
                return;
            }
            _info.MyBorders[(MyColumnas, MyFilas)].Background = myColorBrush;
            if (i == 0)
            {
                if (dirY != 0 && dirX != 0)
                {
                    i++;
                    continue;
                }
            }
            if (Pincel.Size > 1)
                Paint(dirX, dirY, Pincel.Size, myColorBrush, i + 1, null);
            i++;
        }
        _info.SetWalle(columna + dirX * distance, fila + dirY * distance);
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
                    if (i < 0 || i > _info.Dimensions - 1 || j < 0 || j > _info.Dimensions - 1)
                        continue;
                    _info.MyBorders[(i, j)].Background = myColorBrush;
                }
            }
            return;
        }

        //para lineas hacia los lados y arriba abajo
        if (dirX == 0 || dirY == 0)
        {
            int count = 1;
            while (count <= (size / 2))
            {
                if (0 <= col + (dirY * count) && col + (dirY * count) < _info.Dimensions
                    && 0 <= row + (dirX * count) && row + (dirX * count) < _info.Dimensions)
                    _info.MyBorders[(col + (dirY * count), row + (dirX * count))].Background = myColorBrush;

                if (0 <= col - (dirY * count) && col - (dirY * count) < _info.Dimensions
                   && 0 <= row - (dirX * count) && row - (dirX * count) < _info.Dimensions)
                    _info.MyBorders[(col - (dirY * count), row - (dirX * count))].Background = myColorBrush;

                count++;
            }
            return;
        }

        //para lineas hacia las diagonales
        //para cuando distancia es menor que el size de la brocha
        if (myCount < Pincel.Size)
        {
            for (int j = 1; j <= myCount - 1; j++)
            {
                _info.MyBorders[(col - dirX * j, row)].Background = myColorBrush;
                _info.MyBorders[(col, row - dirY * j)].Background = myColorBrush;
            }
            return;
        }
        //para cuando distancia es mayor o igual que el size de la brocha
        for (int j = 1; j <= Pincel.Size - 1; j++)
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
        var myColorBrush = new SolidColorBrush(Avalonia.Media.Color.Parse(Pincel.Color));

        //para calcular el centro
        int MyFilas = WalleY + dirY * radius;
        int MyColumnas = WalleX + dirX * radius;

        if (MyFilas < 0 || MyFilas > _info.Dimensions - 1 || MyColumnas < 0 ||
            MyColumnas > _info.Dimensions - 1)
            return;

        _info.SetWalle(MyColumnas, MyFilas);
        DrawCircle(MyColumnas, MyFilas, radius, myColorBrush);
        _info.SetWalle(MyColumnas, MyFilas);
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
        if (0 <= cx + x && cx + x < _info.Dimensions && 0 <= cy + y && cy + y < _info.Dimensions)
        {
            _info.SetWalle(cx + x, cy + y);
            _info.MyBorders[(cx + x, cy + y)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx + x, cy + y, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx - x && cx - x < _info.Dimensions && 0 <= cy + y && cy + y < _info.Dimensions)
        {
            _info.SetWalle(cx - x, cy + y);
            _info.MyBorders[(cx - x, cy + y)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx - x, cy + y, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx + x && cx + x < _info.Dimensions && 0 <= cy - y && cy - y < _info.Dimensions)
        {
            _info.SetWalle(cx + x, cy - y);
            _info.MyBorders[(cx + x, cy - y)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx + x, cy - y, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx - x && cx - x < _info.Dimensions && 0 <= cy - y && cy - y < _info.Dimensions)
        {
            _info.SetWalle(cx - x, cy - y);
            _info.MyBorders[(cx - x, cy - y)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx - x, cy - y, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx + y && cx + y < _info.Dimensions && 0 <= cy + x && cy + x < _info.Dimensions)
        {
            _info.SetWalle(cx + y, cy + x);
            _info.MyBorders[(cx + y, cy + x)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx + y, cy + x, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx - y && cx - y < _info.Dimensions && 0 <= cy + x && cy + x < _info.Dimensions)
        {
            _info.SetWalle(cx - y, cy + x);
            _info.MyBorders[(cx - y, cy + x)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx - y, cy + x, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx + y && cx + y < _info.Dimensions && 0 <= cy - x && cy - x < _info.Dimensions)
        {
            _info.SetWalle(cx + y, cy - x);
            _info.MyBorders[(cx + y, cy - x)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx + y, cy - x, Pincel.Size, myColorBrush, null, "circle");

        if (0 <= cx - y && cx - y < _info.Dimensions && 0 <= cy - x && cy - x < _info.Dimensions)
        {
            _info.SetWalle(cx - y, cy - x);
            _info.MyBorders[(cx - y, cy - x)].Background = myColorBrush;
        }
        if (Pincel.Size > 1)
            Paint(cx - y, cy - x, Pincel.Size, myColorBrush, null, "circle");


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
        _info.SetWalle(MyColumnas, MyFilas);
        int upParalelRow = MyFilas - 1 - height / 2 + 1;
        int leftParalelCol = MyColumnas - 1 - width / 2 + 1;
        if (height % 2 != 0)
            upParalelRow = MyFilas - 1 - height / 2;
        if (width % 2 != 0)
            leftParalelCol = MyColumnas - 1 - width / 2;
        if (upParalelRow < 0 || upParalelRow > _info.Dimensions ||
            leftParalelCol < 0 || leftParalelCol > _info.Dimensions) return;

        int incremento = (Pincel.Size - 1) / 2;
        Grid.SetRow(_info.Walle, upParalelRow);

        Grid.SetColumn(_info.Walle, leftParalelCol - incremento);
        DrawLine(1, 0, width + 1 + incremento * 2);
        Grid.SetColumn(_info.Walle, Grid.GetColumn(_info.Walle) - incremento);

        Grid.SetRow(_info.Walle, Grid.GetRow(_info.Walle) - incremento);
        DrawLine(0, 1, height + 1 + incremento * 2);
        Grid.SetRow(_info.Walle, Grid.GetRow(_info.Walle) - incremento);

        Grid.SetColumn(_info.Walle, Grid.GetColumn(_info.Walle) + incremento);
        DrawLine(-1, 0, width + 1 + incremento * 2);
        Grid.SetColumn(_info.Walle, Grid.GetColumn(_info.Walle) + incremento);

        Grid.SetRow(_info.Walle, Grid.GetRow(_info.Walle) + incremento);
        DrawLine(0, -1, height + 1 + incremento * 2);

        _info.SetWalle(MyColumnas, MyFilas);
    }
    public void Fill()
    {
        bool[,] mask = new bool[_info.Dimensions, _info.Dimensions];
        (int, int)[] direction = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        var myColorBrush = new SolidColorBrush(Avalonia.Media.Color.Parse(Pincel.Color));
        int WalleX = Grid.GetColumn(_info.Walle);
        int WalleY = Grid.GetRow(_info.Walle);
        var color = _info.MyBorders[(WalleX, WalleY)].Background;
        Fill(mask, direction, WalleX, WalleY, myColorBrush, color);
        _info.SetWalle(WalleX, WalleY);
    }
    public void Fill(bool[,] mask, (int, int)[] direction, int WalleX, int WalleY, SolidColorBrush myColorBrush, IBrush? color)
    {
        mask[WalleX, WalleY] = true;
        _info.MyBorders[(WalleX, WalleY)].Background = myColorBrush;

        for (int i = 0; i < direction.Length; i++)
        {
            var nextX = WalleX + direction[i].Item1;
            var nextY = WalleY + direction[i].Item2;
            if (nextX < 0 || nextX >= _info.Dimensions || nextY < 0 || nextY >= _info.Dimensions)
                continue;
            var cellColor = _info.MyBorders[(nextX, nextY)].Background as SolidColorBrush;
            var colorToRemplace = color as SolidColorBrush;

            if (cellColor?.Color != colorToRemplace?.Color)
                continue;
            if (mask[nextX, nextY])
                continue;
            Fill(mask, direction, nextX, nextY, myColorBrush, color);
        }
    }
    public void CallAction(string Name, object?[] @params, Location location)
    {
        var template = ExceptionTemplates.PARAMETERS1;
        string strParams;
        string message;
        switch (Name)
        {
            case "Spawn" when @params.Length != 2:
                strParams = "int a, int b";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "Spawn":
                Spawn((int)@params[0]!, (int)@params[1]!);
                return;
            case "Color" when @params.Length != 1:
                strParams = "string a";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "Color":
                Color((string)@params[0]!);
                return;
            case "Size" when @params.Length != 1:
                strParams = "int a";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "Size":
                Size((int)@params[0]!);
                return;
            case "DrawLine" when @params.Length != 3:
                strParams = "int a, int b, int c";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "DrawLine":
                DrawLine((int)@params[0]!, (int)@params[1]!, (int)@params[2]!);
                return;
            case "DrawCircle" when @params.Length != 3:
                strParams = "int a, int b, int c";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "DrawCircle":
                DrawCircle((int)@params[0]!, (int)@params[1]!, (int)@params[2]!);
                return;
            case "DrawRectangle" when @params.Length != 5:
                strParams = "int a, int b, int c, int d, int e";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "DrawRectangle":
                DrawRectangle((int)@params[0]!, (int)@params[1]!, (int)@params[2]!, (int)@params[3]!, (int)@params[4]!);
                return;
            case "Fill" when @params.Length != 0:
                strParams = "Este metodo no requiere parametros";
                message = string.Format(template, Name, @params.Length, strParams, location);
                throw new InvalidOperationException(message);
            case "Fill":
                Fill();
                return;
            default: throw new Exception("El metodo no existe o ha sido mal llamado");
        }
    }
}
