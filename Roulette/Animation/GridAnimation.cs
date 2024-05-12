using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roulette.Animation {
    public static class GridAnimation {
        public static async Task RevertAnimationsAsync(Dictionary<Border, IBrush> originalBorderBrushes, Dictionary<TextBlock, IBrush> originalTextBrushes) {
            var revertTasks = new List<Task>();

            foreach (var pair in originalBorderBrushes) {
                var (border, originalBrush) = pair;
                var winBorder = Brushes.Cyan;
                revertTasks.Add(AnimateBorderColorAsync(border, TimeSpan.FromSeconds(1), winBorder, originalBrush));
            }
            foreach (var pair in originalTextBrushes) {
                var (textBlock, originalBrush) = pair;
                var winText = Brushes.Black;
                revertTasks.Add(AnimateTextColorAsync(textBlock, TimeSpan.FromSeconds(1), winText, originalBrush));
            }
            await Task.WhenAll(revertTasks);
        }
        public static async Task AnimateColorAsync(Control control, TimeSpan duration, IBrush fromBrush, IBrush toBrush, Action<Control, IBrush> setColorProperty) {
            var steps = 100;
            var stepDuration = duration.TotalMilliseconds / steps;

            var fromColor = (fromBrush as ISolidColorBrush)?.Color ?? Colors.Transparent;
            var toColor = (toBrush as ISolidColorBrush)?.Color ?? Colors.Transparent;

            var rStep = (toColor.R - fromColor.R) / (float)steps;
            var gStep = (toColor.G - fromColor.G) / (float)steps;
            var bStep = (toColor.B - fromColor.B) / (float)steps;

            for (int i = 0; i <= steps; i++) {
                var newColor = Color.FromRgb(
                    (byte)(fromColor.R + rStep * i),
                    (byte)(fromColor.G + gStep * i),
                    (byte)(fromColor.B + bStep * i)
                );
                var newColorBrush = new SolidColorBrush(newColor);
                setColorProperty(control, newColorBrush);
                await Task.Delay(TimeSpan.FromMilliseconds(stepDuration));
            }
        }

        public static async Task AnimateBorderColorAsync(Border border, TimeSpan duration, IBrush fromBrush, IBrush toBrush) {
            await AnimateColorAsync(border, duration, fromBrush, toBrush, (control, brush) => { border.Background = brush; });
        }
        public static async Task AnimateTextColorAsync(TextBlock textBlock, TimeSpan duration, IBrush fromBrush, IBrush toBrush) {
            await AnimateColorAsync(textBlock, duration, fromBrush, toBrush, (control, brush) => { textBlock.Foreground = brush; });
        }
    }
}
