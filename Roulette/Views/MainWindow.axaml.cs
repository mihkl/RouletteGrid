using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using Roulette.HelperMethods;
using Roulette.Animation;
using static System.Net.Mime.MediaTypeNames;
using System.Data.Common;
using System.Xml.Linq;


namespace Roulette.Views;

public partial class MainWindow: Window { 
    private TcpListener? tcpListener;
    public MainWindow() {
        InitializeComponent();

        InitializeTcpListener();

        GenerateNumbers();

        GenerateRotated();

        GenerateSpecialBets(grid, 3, 1, 4, "1st 12", "1st 12");
        GenerateSpecialBets(grid, 3, 5, 4, "2nd 12", "2nd 12");
        GenerateSpecialBets(grid, 3, 9, 4, "3rd 12", "3rd 12");
        GenerateSpecialBets(grid, 4, 1, 2, "1 to 18", "1 to 18");
        GenerateSpecialBets(grid, 4, 3, 2, "Even", "Even");
        GenerateSpecialBets(grid, 4, 5, 2, "", "Black");
        GenerateSpecialBets(grid, 4, 7, 2, "", "Red", "Red");
        GenerateSpecialBets(grid, 4, 9, 2, "Odd", "Odd");
        GenerateSpecialBets(grid, 4, 11, 2, "19 to 36", "19 to 36");

        GenerateZero();
    }
    // TCP methods below
    private async void InitializeTcpListener() {
        try {
            tcpListener = new TcpListener(IPAddress.Any, 4948);tcpListener.Start();
            await Task.Run(ListenForTcpData);}
        catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); };
    }
    private async Task ListenForTcpData() {
        if (tcpListener == null) return;
        while (true) {
            TcpClient client = await tcpListener.AcceptTcpClientAsync();
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer);
            string jsonString = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            var winNr = ParseJson(jsonString);
            AnimateWinElement(winNr);

            client.Close();
        }
    }
    private static int ParseJson(string jsonString) {
        using JsonDocument document = JsonDocument.Parse(jsonString);
        var root = document.RootElement;
        var data = root.GetProperty("Data");
        var winNr = data.GetProperty("WinningNumber").GetInt32();
        return winNr;
    }
    // Methods to find which elements to animate below
    private static string FindWinColor(int winNr) {
        string winColor;
        if (winNr is >= 1 and <= 10 or >= 19 and <= 28) winColor = winNr % 2 == 0 ? "Black" : "Red";
        else winColor = winNr % 2 == 0 ? "Red" : "Black";
        return winColor;
    }
    private (string oddOrEven, string firstSecondOrThird, string highOrLow, string winColor, string winRow) FindWinConditions(int winNr) {
        if (winNr == 0) return ("", "", "", "", "");
        string oddOrEven = winNr % 2 == 0 ? "Even" : "Odd";
        string firstSecondOrThird = winNr == 0 ? "0" : winNr < 13 ? "1st 12" : winNr < 25 ? "2nd 12" : "3rd 12";
        string highOrLow = winNr < 19 ? "1 to 18" : "19 to 36";
        string winColor = FindWinColor(winNr);
        string winRow = winNr % 3 == 0 ? "0row" : winNr % 3 == 2 ? "1row" : "2row";
        return (oddOrEven, firstSecondOrThird, highOrLow, winColor, winRow);
    }
    private static bool IsBorderToAnimate(Border border, string winNrString, (string, string, string, string, string) winConditions) {
        var (oddOrEven, firstSecondOrThird, highOrLow, winColor, winRow) = winConditions;
        return BorderProperties.GetName(border) == winNrString ||
               BorderProperties.GetName(border) == oddOrEven ||
               BorderProperties.GetName(border) == highOrLow ||
               BorderProperties.GetName(border) == firstSecondOrThird ||
               BorderProperties.GetName(border) == winRow ||
               BorderProperties.GetName(border) == winColor;
    }
    private static bool IsTextBlockToAnimate(TextBlock textBlock) {
        return textBlock.Classes.Contains("Black") || textBlock.Classes.Contains("Rotated");
    }
    private void AnimateWinElement(int winNr) {
        var winConditions = FindWinConditions(winNr);
        var originalBorderBrushes = new Dictionary<Border, IBrush>();
        var originalTextBrushes = new Dictionary<TextBlock, IBrush>();
        string winNrString = winNr.ToString();

        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            IBrush winBorderBrush = Brushes.Cyan;
            IBrush winTextBrush = Brushes.Black;
            var animationTasks = new List<Task>();

            foreach (Control control in grid.Children) {
                if (control is Border border && IsBorderToAnimate(border, winNrString, winConditions)) {
                    var originalBorderBrush = border.Background ?? Brushes.Transparent;
                    animationTasks.Add(GridAnimation.AnimateBorderColorAsync(border, TimeSpan.FromSeconds(1), originalBorderBrush, winBorderBrush));
                    originalBorderBrushes.Add(border, originalBorderBrush);

                    if (border.Child is TextBlock textBlock && IsTextBlockToAnimate(textBlock)) {
                        var originalTextBrush = textBlock.Foreground ?? Brushes.White;
                        animationTasks.Add(GridAnimation.AnimateTextColorAsync(textBlock, TimeSpan.FromSeconds(1), originalTextBrush, winTextBrush));
                        originalTextBrushes.Add(textBlock, originalTextBrush);
                    }
                }
            }
            await Task.WhenAll(animationTasks);
            await Task.Delay(TimeSpan.FromSeconds(10));
            await GridAnimation.RevertAnimationsAsync(originalBorderBrushes, originalTextBrushes);
        });
    }
    // Only grid generation methods below
    private void GenerateNumbers() {
        for (int col = 1; col < 13; col++) {
            for (int row = 2; row > -1; row--) {
                var border = new Border();
                var textBlock = new TextBlock();

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                int number = ((col * 3) - row);

                BorderProperties.SetName(border, number.ToString());

                if (number is >= 1 and <= 10 or >= 19 and <= 28) {
                    border.Classes.Add((number) % 2 == 0 ? "Black" : "Red");
                    textBlock.Classes.Add((row + col) % 2 == 0 ? "Black" : "Red");}

                else {border.Classes.Add((number) % 2 == 0 ? "Red" : "Black");
                    textBlock.Classes.Add((row + col) % 2 == 0 ? "Red" : "Black");}

                border.Classes.Add("General");

                if (number < 10) textBlock.Padding = new Thickness(3);
                textBlock.Text = (number).ToString();

                border.Child = textBlock;
                grid.Children.Add(border);
            }
        }
    }
    private static void GenerateSpecialBets(Grid grid, int row, int column, int columnSpan, string text, string name, string classes = "") {
        var border = new Border();
        var textBlock = new TextBlock();

        Grid.SetRow(border, row);
        Grid.SetColumn(border, column);
        Grid.SetColumnSpan(border, columnSpan);

        BorderProperties.SetName(border, name);
        border.Classes.Add(classes);
        border.Classes.Add("General");

        textBlock.Text = text;
        textBlock.Classes.Add("Black");

        border.Child = textBlock;
        grid.Children.Add(border);
    }
    private void GenerateRotated() {
        for (int row = 0; row < 3; row++) {
            var border = new Border();
            var textBlock = new TextBlock();

            BorderProperties.SetName(border, $"{row}row");

            var rotateTransform = new RotateTransform(-90);
            border.RenderTransform = rotateTransform;

            border.Classes.Add("Rotated");
            textBlock.Classes.Add("Rotated");
            textBlock.Text = "2 to 1";

            border.SetValue(Grid.RowProperty, row);
            border.SetValue(Grid.ColumnProperty, 13);

            border.Margin = new Thickness(-5, 11, 0, 15);

            border.Child = textBlock;
            grid.Children.Add(border);
        }
    }
    private void GenerateZero() {
        var border = new Border();
        var textBlock = new TextBlock();

        Grid.SetRow(border, 0);
        Grid.SetColumn(border, 0);
        Grid.SetRowSpan(border, 3);

        BorderProperties.SetName(border, "0");
        border.Background = SolidColorBrush.Parse("#0bb01b");
        border.BorderBrush = SolidColorBrush.Parse("#0bb01b");
        border.CornerRadius = new CornerRadius(100, 1, 1, 100);
        border.Padding = new Thickness(20);
        border.Margin = new Thickness(1, 1, 3, 1);

        textBlock.Text = "0";
        textBlock.FontSize = 20;
        textBlock.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        textBlock.Foreground = SolidColorBrush.Parse("White");

        border.Child = textBlock;
        grid.Children.Add(border);
     
    }

}

