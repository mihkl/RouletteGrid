using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Roulette.Views;

public partial class MainWindow: Window {
    public MainWindow() {
        InitializeComponent();

        GenerateNumbers();

        GenerateRotated();

        GenerateSpecialBets(grid, 3, 1, 4, "1st to 12");
        GenerateSpecialBets(grid, 3, 5, 4, "2nd to 12");
        GenerateSpecialBets(grid, 3, 9, 4, "3rd to 12");
        GenerateSpecialBets(grid, 4, 1, 2, "1 to 18");
        GenerateSpecialBets(grid, 4, 3, 2, "Even");
        GenerateSpecialBets(grid, 4, 5, 2, "");
        GenerateSpecialBets(grid, 4, 7, 2, "", "Red");
        GenerateSpecialBets(grid, 4, 9, 2, "Odd");
        GenerateSpecialBets(grid, 4, 11, 2, "19 to 36");
    }
    private void GenerateNumbers() {
        for (int col = 1; col < 13; col++) {
            for (int row = 2; row > -1; row--) {
                var border = new Border();
                var textBlock = new TextBlock();

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                int number = ((col * 3) - row);

                if (number is >= 1 and <= 10 or >= 19 and <= 28) {
                    border.Classes.Add((number) % 2 == 0 ? "Black" : "Red");
                    textBlock.Classes.Add((row + col) % 2 == 0 ? "Black" : "Red");
                } else {
                    border.Classes.Add((number) % 2 == 0 ? "Red" : "Black");
                    textBlock.Classes.Add((row + col) % 2 == 0 ? "Red" : "Black");
                }
                border.Classes.Add("General");

                if (number < 10) {
                    textBlock.Padding = new Thickness(5);
                }
                textBlock.Text = (number).ToString();

                border.Child = textBlock;

                grid.Children.Add(border);
            }
        }
    }
    private void GenerateSpecialBets(Grid grid, int row, int column, int columnSpan, string text, string classes = "" ) {
        var border = new Border();
        var textBlock = new TextBlock();

        Grid.SetRow(border, row);
        Grid.SetColumn(border, column);
        Grid.SetColumnSpan(border, columnSpan);

        border.Classes.Add(classes);
        border.Classes.Add("General");

        textBlock.Text = text;
        textBlock.Classes.Add("Black");

        border.Child = textBlock;

        grid.Children.Add(border);
    }
    private void GenerateRotated() {
        for (int row = 0; row < 3; row++) {
            var rotatedGrid = new Grid();
            rotatedGrid.SetValue(Grid.RowProperty, row);
            rotatedGrid.SetValue(Grid.ColumnProperty, 13);
            rotatedGrid.Margin = new Thickness(-20, 0, 0, 0);

            var rotateTransform = new RotateTransform(-90);
            rotatedGrid.RenderTransform = rotateTransform;

            var border = new Border();
            var textBlock = new TextBlock();

            border.Classes.Add("Rotated");
            textBlock.Classes.Add("Rotated");
            textBlock.Text = "2 to 1";
            border.Child = textBlock;

            rotatedGrid.Children.Add(border);
            grid.Children.Add(rotatedGrid);
        }
    }
}
