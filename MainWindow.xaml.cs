using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.Json;
using System.Xml.Linq;
using System.Text;
using System.Linq;

namespace DiagramEditor
{
    // ==================== VALUE OBJECT ПАТТЕРН ====================
    public readonly struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point Offset(double dx, double dy) => new Point(X + dx, Y + dy);
        public double DistanceTo(Point other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;
            Point other = (Point)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
        public override string ToString() => $"({X:F1}, {Y:F1})";
    }

    public readonly struct DiagramSize
    {
        public double Width { get; }
        public double Height { get; }

        public DiagramSize(double width, double height)
        {
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
        }

        public double Area => Width * Height;
        public double Perimeter => 2 * (Width + Height);
        public bool IsSquare => Math.Abs(Width - Height) < 0.01;

        public override bool Equals(object obj)
        {
            if (!(obj is DiagramSize)) return false;
            DiagramSize other = (DiagramSize)obj;
            return Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode() => Width.GetHashCode() ^ Height.GetHashCode();
        public override string ToString() => $"{Width:F1}x{Height:F1}";
    }

    public readonly struct DiagramColor
    {
        public string Value { get; }

        public DiagramColor(string value)
        {
            Value = value ?? "Black";
        }

        public SolidColorBrush ToBrush() =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(Value));

        public bool IsLight()
        {
            var color = (Color)ColorConverter.ConvertFromString(Value);
            return (color.R + color.G + color.B) / 3.0 > 128;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DiagramColor)) return false;
            DiagramColor other = (DiagramColor)obj;
            return Value == other.Value;
        }

        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override string ToString() => Value;
    }

    // ==================== PROTOTYPE ПАТТЕРН ====================
    public interface IPrototype<T>
    {
        T Clone();
    }

    public abstract class DiagramShape : IPrototype<DiagramShape>
    {
        public Point Position { get; set; }
        public DiagramColor FillColor { get; set; }
        public DiagramColor BorderColor { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        protected DiagramShape(Point position, DiagramColor fillColor, DiagramColor borderColor, string name = "")
        {
            Position = position;
            FillColor = fillColor;
            BorderColor = borderColor;
            Name = name;
            CreatedAt = DateTime.Now;
        }

        public abstract DiagramShape Clone();
        public abstract void Accept(IShapeVisitor visitor);
        public abstract double GetArea();
        public abstract double GetPerimeter();
        public abstract bool ContainsPoint(Point point);
    }

    public class DiagramRectangle : DiagramShape
    {
        public DiagramSize Size { get; set; }

        public DiagramRectangle(Point position, DiagramSize size, DiagramColor fillColor, DiagramColor borderColor, string name = "Rectangle")
            : base(position, fillColor, borderColor, name)
        {
            Size = size;
        }

        public override DiagramShape Clone()
        {
            return new DiagramRectangle(
                Position.Offset(20, 20),
                Size,
                FillColor,
                BorderColor,
                Name + "_Copy"
            );
        }

        public override void Accept(IShapeVisitor visitor) => visitor.VisitRectangle(this);
        public override double GetArea() => Size.Area;
        public override double GetPerimeter() => Size.Perimeter;

        public override bool ContainsPoint(Point point)
        {
            return point.X >= Position.X && point.X <= Position.X + Size.Width &&
                   point.Y >= Position.Y && point.Y <= Position.Y + Size.Height;
        }
    }

    public class DiagramCircle : DiagramShape
    {
        public double Radius { get; set; }

        public DiagramCircle(Point position, double radius, DiagramColor fillColor, DiagramColor borderColor, string name = "Circle")
            : base(position, fillColor, borderColor, name)
        {
            Radius = Math.Max(0, radius);
        }

        public override DiagramShape Clone()
        {
            return new DiagramCircle(
                Position.Offset(20, 20),
                Radius,
                FillColor,
                BorderColor,
                Name + "_Copy"
            );
        }

        public override void Accept(IShapeVisitor visitor) => visitor.VisitCircle(this);
        public override double GetArea() => Math.PI * Radius * Radius;
        public override double GetPerimeter() => 2 * Math.PI * Radius;

        public override bool ContainsPoint(Point point)
        {
            return Position.DistanceTo(point) <= Radius;
        }
    }

    public class DiagramTriangle : DiagramShape
    {
        public double SideLength { get; set; }

        public DiagramTriangle(Point position, double sideLength, DiagramColor fillColor, DiagramColor borderColor, string name = "Triangle")
            : base(position, fillColor, borderColor, name)
        {
            SideLength = Math.Max(0, sideLength);
        }

        public override DiagramShape Clone()
        {
            return new DiagramTriangle(
                Position.Offset(20, 20),
                SideLength,
                FillColor,
                BorderColor,
                Name + "_Copy"
            );
        }

        public override void Accept(IShapeVisitor visitor) => visitor.VisitTriangle(this);
        public override double GetArea() => (Math.Sqrt(3) / 4) * SideLength * SideLength;
        public override double GetPerimeter() => 3 * SideLength;

        public override bool ContainsPoint(Point point)
        {
            return Position.DistanceTo(point) <= SideLength / 2;
        }
    }

    // ==================== VISITOR ПАТТЕРН ====================
    public interface IShapeVisitor
    {
        void VisitRectangle(DiagramRectangle rectangle);
        void VisitCircle(DiagramCircle circle);
        void VisitTriangle(DiagramTriangle triangle);
    }

    public class StatisticsVisitor : IShapeVisitor
    {
        public int RectangleCount { get; private set; }
        public int CircleCount { get; private set; }
        public int TriangleCount { get; private set; }
        public double TotalArea { get; private set; }
        public double TotalPerimeter { get; private set; }
        public Dictionary<string, int> ColorCount { get; private set; } = new Dictionary<string, int>();

        public void VisitRectangle(DiagramRectangle rectangle)
        {
            RectangleCount++;
            TotalArea += rectangle.GetArea();
            TotalPerimeter += rectangle.GetPerimeter();
            CountColor(rectangle.FillColor.Value);
        }

        public void VisitCircle(DiagramCircle circle)
        {
            CircleCount++;
            TotalArea += circle.GetArea();
            TotalPerimeter += circle.GetPerimeter();
            CountColor(circle.FillColor.Value);
        }

        public void VisitTriangle(DiagramTriangle triangle)
        {
            TriangleCount++;
            TotalArea += triangle.GetArea();
            TotalPerimeter += triangle.GetPerimeter();
            CountColor(triangle.FillColor.Value);
        }

        private void CountColor(string color)
        {
            if (ColorCount.ContainsKey(color))
                ColorCount[color]++;
            else
                ColorCount[color] = 1;
        }

        public void Reset()
        {
            RectangleCount = CircleCount = TriangleCount = 0;
            TotalArea = TotalPerimeter = 0;
            ColorCount.Clear();
        }

        public string GetReport()
        {
            var total = RectangleCount + CircleCount + TriangleCount;
            var report = new StringBuilder();
            report.AppendLine("=== СТАТИСТИКА ДИАГРАММЫ ===");
            report.AppendLine($"Всего фигур: {total}");
            report.AppendLine($"Прямоугольники: {RectangleCount}");
            report.AppendLine($"Круги: {CircleCount}");
            report.AppendLine($"Треугольники: {TriangleCount}");
            report.AppendLine($"Общая площадь: {TotalArea:F2}");
            report.AppendLine($"Общий периметр: {TotalPerimeter:F2}");

            if (ColorCount.Any())
            {
                report.AppendLine("\nЦвета:");
                foreach (var color in ColorCount)
                    report.AppendLine($"  {color.Key}: {color.Value}");
            }

            return report.ToString();
        }
    }

    public class JsonExportVisitor : IShapeVisitor
    {
        private List<object> jsonObjects = new List<object>();

        public void VisitRectangle(DiagramRectangle rectangle)
        {
            jsonObjects.Add(new
            {
                Type = "Rectangle",
                Name = rectangle.Name,
                X = rectangle.Position.X,
                Y = rectangle.Position.Y,
                Width = rectangle.Size.Width,
                Height = rectangle.Size.Height,
                Area = rectangle.GetArea(),
                FillColor = rectangle.FillColor.Value,
                BorderColor = rectangle.BorderColor.Value,
                CreatedAt = rectangle.CreatedAt
            });
        }

        public void VisitCircle(DiagramCircle circle)
        {
            jsonObjects.Add(new
            {
                Type = "Circle",
                Name = circle.Name,
                X = circle.Position.X,
                Y = circle.Position.Y,
                Radius = circle.Radius,
                Area = circle.GetArea(),
                FillColor = circle.FillColor.Value,
                BorderColor = circle.BorderColor.Value,
                CreatedAt = circle.CreatedAt
            });
        }

        public void VisitTriangle(DiagramTriangle triangle)
        {
            jsonObjects.Add(new
            {
                Type = "Triangle",
                Name = triangle.Name,
                X = triangle.Position.X,
                Y = triangle.Position.Y,
                SideLength = triangle.SideLength,
                Area = triangle.GetArea(),
                FillColor = triangle.FillColor.Value,
                BorderColor = triangle.BorderColor.Value,
                CreatedAt = triangle.CreatedAt
            });
        }

        public string GetJson()
        {
            var json = JsonSerializer.Serialize(jsonObjects, new JsonSerializerOptions { WriteIndented = true });
            jsonObjects.Clear();
            return json;
        }
    }

    public class XmlExportVisitor : IShapeVisitor
    {
        private XElement root = new XElement("Diagram");

        public void VisitRectangle(DiagramRectangle rectangle)
        {
            root.Add(new XElement("Rectangle",
                new XAttribute("Name", rectangle.Name),
                new XAttribute("X", rectangle.Position.X),
                new XAttribute("Y", rectangle.Position.Y),
                new XAttribute("Width", rectangle.Size.Width),
                new XAttribute("Height", rectangle.Size.Height),
                new XAttribute("Area", rectangle.GetArea()),
                new XAttribute("FillColor", rectangle.FillColor.Value),
                new XAttribute("BorderColor", rectangle.BorderColor.Value),
                new XAttribute("CreatedAt", rectangle.CreatedAt)
            ));
        }

        public void VisitCircle(DiagramCircle circle)
        {
            root.Add(new XElement("Circle",
                new XAttribute("Name", circle.Name),
                new XAttribute("X", circle.Position.X),
                new XAttribute("Y", circle.Position.Y),
                new XAttribute("Radius", circle.Radius),
                new XAttribute("Area", circle.GetArea()),
                new XAttribute("FillColor", circle.FillColor.Value),
                new XAttribute("BorderColor", circle.BorderColor.Value),
                new XAttribute("CreatedAt", circle.CreatedAt)
            ));
        }

        public void VisitTriangle(DiagramTriangle triangle)
        {
            root.Add(new XElement("Triangle",
                new XAttribute("Name", triangle.Name),
                new XAttribute("X", triangle.Position.X),
                new XAttribute("Y", triangle.Position.Y),
                new XAttribute("SideLength", triangle.SideLength),
                new XAttribute("Area", triangle.GetArea()),
                new XAttribute("FillColor", triangle.FillColor.Value),
                new XAttribute("BorderColor", triangle.BorderColor.Value),
                new XAttribute("CreatedAt", triangle.CreatedAt)
            ));
        }

        public XElement GetXml()
        {
            var result = root;
            root = new XElement("Diagram");
            return result;
        }
    }

    public class ValidationVisitor : IShapeVisitor
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public void VisitRectangle(DiagramRectangle rectangle)
        {
            if (rectangle.Size.Width <= 0 || rectangle.Size.Height <= 0)
                Errors.Add($"Прямоугольник '{rectangle.Name}' имеет некорректные размеры");

            if (rectangle.Position.X < 0 || rectangle.Position.Y < 0)
                Warnings.Add($"Прямоугольник '{rectangle.Name}' частично за пределами области");

            if (rectangle.GetArea() > 10000)
                Warnings.Add($"Прямоугольник '{rectangle.Name}' очень большой");
        }

        public void VisitCircle(DiagramCircle circle)
        {
            if (circle.Radius <= 0)
                Errors.Add($"Круг '{circle.Name}' имеет некорректный радиус");

            if (circle.Position.X < 0 || circle.Position.Y < 0)
                Warnings.Add($"Круг '{circle.Name}' частично за пределами области");

            if (circle.GetArea() > 10000)
                Warnings.Add($"Круг '{circle.Name}' очень большой");
        }

        public void VisitTriangle(DiagramTriangle triangle)
        {
            if (triangle.SideLength <= 0)
                Errors.Add($"Треугольник '{triangle.Name}' имеет некорректную длину стороны");

            if (triangle.Position.X < 0 || triangle.Position.Y < 0)
                Warnings.Add($"Треугольник '{triangle.Name}' частично за пределами области");
        }

        public string GetValidationReport()
        {
            var report = new StringBuilder();

            if (Errors.Count == 0 && Warnings.Count == 0)
                return "Диаграмма корректна!";

            if (Errors.Count > 0)
            {
                report.AppendLine($"Ошибки ({Errors.Count}):");
                foreach (var error in Errors)
                    report.AppendLine($"  ❌ {error}");
            }

            if (Warnings.Count > 0)
            {
                report.AppendLine($"Предупреждения ({Warnings.Count}):");
                foreach (var warning in Warnings)
                    report.AppendLine($"  ⚠️ {warning}");
            }

            return report.ToString();
        }

        public void Reset()
        {
            Errors.Clear();
            Warnings.Clear();
        }
    }

    // ==================== ADAPTER ПАТТЕРН ====================
    public interface IFileAdapter
    {
        void Save(IEnumerable<DiagramShape> shapes, string filename);
        string GetDescription();
        string GetFileExtension();
    }

    public class JsonFileAdapter : IFileAdapter
    {
        private JsonExportVisitor visitor = new JsonExportVisitor();

        public void Save(IEnumerable<DiagramShape> shapes, string filename)
        {
            foreach (var shape in shapes)
                shape.Accept(visitor);

            File.WriteAllText(filename, visitor.GetJson());
        }

        public string GetDescription() => "JSON формат с метаданными";
        public string GetFileExtension() => "json";
    }

    public class XmlFileAdapter : IFileAdapter
    {
        private XmlExportVisitor visitor = new XmlExportVisitor();

        public void Save(IEnumerable<DiagramShape> shapes, string filename)
        {
            foreach (var shape in shapes)
                shape.Accept(visitor);

            visitor.GetXml().Save(filename);
        }

        public string GetDescription() => "XML формат со схемой";
        public string GetFileExtension() => "xml";
    }

    public class CsvFileAdapter : IFileAdapter
    {
        public void Save(IEnumerable<DiagramShape> shapes, string filename)
        {
            var lines = new List<string> { "Type,Name,X,Y,Width,Height,Radius,SideLength,Area,FillColor,BorderColor,CreatedAt" };

            foreach (var shape in shapes)
            {
                var line = $"{shape.GetType().Name},{shape.Name},{shape.Position.X},{shape.Position.Y},";

                if (shape is DiagramRectangle rect)
                    line += $"{rect.Size.Width},{rect.Size.Height},,";
                else if (shape is DiagramCircle circle)
                    line += $",{circle.Radius},,";
                else if (shape is DiagramTriangle triangle)
                    line += $",,{triangle.SideLength},";

                line += $"{shape.GetArea():F2},{shape.FillColor.Value},{shape.BorderColor.Value},{shape.CreatedAt}";
                lines.Add(line);
            }

            File.WriteAllLines(filename, lines);
        }

        public string GetDescription() => "CSV формат для Excel";
        public string GetFileExtension() => "csv";
    }

    public class PdfFileAdapter : IFileAdapter
    {
        public void Save(IEnumerable<DiagramShape> shapes, string filename)
        {
            var content = new StringBuilder();
            content.AppendLine("PDF Export Simulation");
            content.AppendLine($"Generated: {DateTime.Now}");
            content.AppendLine($"Total shapes: {shapes.Count()}");

            foreach (var shape in shapes)
            {
                content.AppendLine($"Shape: {shape.Name} at {shape.Position}");
            }

            File.WriteAllText(filename.Replace(".pdf", ".txt"), content.ToString());
        }

        public string GetDescription() => "PDF документ (симуляция)";
        public string GetFileExtension() => "pdf";
    }

    // ==================== ДИАЛОГ ВЫБОРА ФОРМАТА ====================
    public class FormatSelectionDialog : Window
    {
        public string SelectedFormat { get; private set; } = "JSON";

        public FormatSelectionDialog(IEnumerable<string> formats)
        {
            Title = "Выбор формата сохранения";
            Width = 350;
            Height = 250;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var panel = new StackPanel { Margin = new Thickness(20) };

            panel.Children.Add(new TextBlock
            {
                Text = "Выберите формат для сохранения диаграммы:",
                Margin = new Thickness(0, 0, 0, 15),
                FontWeight = FontWeights.Bold
            });

            var combo = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
            foreach (var format in formats)
                combo.Items.Add(format);
            combo.SelectedIndex = 0;
            panel.Children.Add(combo);

            var description = new TextBlock
            {
                Text = "JSON - полные метаданные\nXML - структурированные данные\nCSV - для Excel\nPDF - документ (симуляция)",
                Margin = new Thickness(0, 10, 0, 20),
                FontSize = 11,
                Foreground = Brushes.Gray
            };
            panel.Children.Add(description);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "Сохранить",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) =>
            {
                if (combo.SelectedItem != null)
                    SelectedFormat = combo.SelectedItem.ToString();
                DialogResult = true;
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 80,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            panel.Children.Add(buttonPanel);

            Content = panel;
        }
    }

    // ==================== ГЛАВНОЕ ОКНО БЕЗ ====================
    public partial class MainWindow : Window
    {
        private readonly List<DiagramShape> shapes = new List<DiagramShape>();
        private DiagramShape selectedShape = null;  // Исправлено: явная инициализация
        private readonly Dictionary<string, IFileAdapter> fileAdapters = new Dictionary<string, IFileAdapter>();
        private readonly StatisticsVisitor statisticsVisitor = new StatisticsVisitor();
        private readonly ValidationVisitor validationVisitor = new ValidationVisitor();
        private int shapeCounter = 1;

        public MainWindow()
        {
            InitializeComponent();
            InitializeFileAdapters();
        }

        private void InitializeFileAdapters()
        {
            fileAdapters["JSON"] = new JsonFileAdapter();
            fileAdapters["XML"] = new XmlFileAdapter();
            fileAdapters["CSV"] = new CsvFileAdapter();
            fileAdapters["PDF"] = new PdfFileAdapter();
        }

        private void AddRectangle_Click(object sender, RoutedEventArgs e)
        {
            var rect = new DiagramRectangle(
                new Point(50 + (shapeCounter * 20), 50 + (shapeCounter * 20)),
                new DiagramSize(80, 60),
                new DiagramColor("Blue"),
                new DiagramColor("DarkBlue"),
                $"Rectangle_{shapeCounter}"
            );

            shapes.Add(rect);
            DrawShape(rect);
            shapeCounter++;
        }

        private void AddCircle_Click(object sender, RoutedEventArgs e)
        {
            var circle = new DiagramCircle(
                new Point(200 + (shapeCounter * 20), 50 + (shapeCounter * 20)),
                40,
                new DiagramColor("Red"),
                new DiagramColor("DarkRed"),
                $"Circle_{shapeCounter}"
            );

            shapes.Add(circle);
            DrawShape(circle);
            shapeCounter++;
        }

        private void AddTriangle_Click(object sender, RoutedEventArgs e)
        {
            var triangle = new DiagramTriangle(
                new Point(350 + (shapeCounter * 20), 50 + (shapeCounter * 20)),
                60,
                new DiagramColor("Green"),
                new DiagramColor("DarkGreen"),
                $"Triangle_{shapeCounter}"
            );

            shapes.Add(triangle);
            DrawShape(triangle);
            shapeCounter++;
        }

        private void CloneShape_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null)
            {
                MessageBox.Show("Сначала выберите фигуру, кликнув по ней!");
                return;
            }

            // Используем паттерн Prototype
            var clone = selectedShape.Clone();
            shapes.Add(clone);
            DrawShape(clone);
            shapeCounter++;
            MessageBox.Show($"Фигура '{selectedShape.Name}' клонирована!");
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для сохранения. Создайте сначала фигуры!");
                return;
            }

            // Используем паттерн Adapter
            var dialog = new FormatSelectionDialog(fileAdapters.Keys);
            if (dialog.ShowDialog() == true)
            {
                var adapter = fileAdapters[dialog.SelectedFormat];
                var filename = $"diagram_{DateTime.Now:yyyyMMdd_HHmmss}.{adapter.GetFileExtension()}";

                try
                {
                    adapter.Save(shapes, filename);
                    MessageBox.Show($"Файл сохранен!\n\nФормат: {adapter.GetDescription()}\nФайл: {filename}\nКоличество фигур: {shapes.Count}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для анализа. Создайте сначала фигуры!");
                return;
            }

            // Используем паттерн Visitor
            statisticsVisitor.Reset();
            foreach (var shape in shapes)
                shape.Accept(statisticsVisitor);

            MessageBox.Show(statisticsVisitor.GetReport(), "Статистика диаграммы");
        }

        private void ValidateDiagram_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для проверки. Создайте сначала фигуры!");
                return;
            }

            // Используем паттерн Visitor
            validationVisitor.Reset();
            foreach (var shape in shapes)
                shape.Accept(validationVisitor);

            MessageBox.Show(validationVisitor.GetValidationReport(), "Результат валидации");
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Диаграмма уже пуста!");
                return;
            }

            var result = MessageBox.Show("Точно удалить все фигуры?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                shapes.Clear();
                DrawingCanvas.Children.Clear();
                selectedShape = null;
                shapeCounter = 1;
                MessageBox.Show("Диаграмма очищена!");
            }
        }

        private void DrawShape(DiagramShape shape)
        {
            if (shape is DiagramRectangle rect)
            {
                var rectangle = new Rectangle
                {
                    Width = rect.Size.Width,
                    Height = rect.Size.Height,
                    Fill = rect.FillColor.ToBrush(),
                    Stroke = rect.BorderColor.ToBrush(),
                    StrokeThickness = 2,
                    ToolTip = $"{rect.Name}\nПлощадь: {rect.GetArea():F1}\nПериметр: {rect.GetPerimeter():F1}"
                };

                Canvas.SetLeft(rectangle, rect.Position.X);
                Canvas.SetTop(rectangle, rect.Position.Y);
                DrawingCanvas.Children.Add(rectangle);
                rectangle.MouseDown += (s, e) => SelectShape(rect, rectangle);
            }
            else if (shape is DiagramCircle circle)
            {
                var ellipse = new Ellipse
                {
                    Width = circle.Radius * 2,
                    Height = circle.Radius * 2,
                    Fill = circle.FillColor.ToBrush(),
                    Stroke = circle.BorderColor.ToBrush(),
                    StrokeThickness = 2,
                    ToolTip = $"{circle.Name}\nПлощадь: {circle.GetArea():F1}\nПериметр: {circle.GetPerimeter():F1}"
                };

                Canvas.SetLeft(ellipse, circle.Position.X - circle.Radius);
                Canvas.SetTop(ellipse, circle.Position.Y - circle.Radius);
                DrawingCanvas.Children.Add(ellipse);
                ellipse.MouseDown += (s, e) => SelectShape(circle, ellipse);
            }
            else if (shape is DiagramTriangle triangle)
            {
                var polygon = new Polygon();
                var points = new PointCollection();

                // Равносторонний треугольник
                var height = triangle.SideLength * Math.Sqrt(3) / 2;
                points.Add(new System.Windows.Point(triangle.Position.X, triangle.Position.Y - height / 2));
                points.Add(new System.Windows.Point(triangle.Position.X - triangle.SideLength / 2, triangle.Position.Y + height / 2));
                points.Add(new System.Windows.Point(triangle.Position.X + triangle.SideLength / 2, triangle.Position.Y + height / 2));

                polygon.Points = points;
                polygon.Fill = triangle.FillColor.ToBrush();
                polygon.Stroke = triangle.BorderColor.ToBrush();
                polygon.StrokeThickness = 2;
                polygon.ToolTip = $"{triangle.Name}\nПлощадь: {triangle.GetArea():F1}\nПериметр: {triangle.GetPerimeter():F1}";

                DrawingCanvas.Children.Add(polygon);
                polygon.MouseDown += (s, e) => SelectShape(triangle, polygon);
            }
        }

        private void SelectShape(DiagramShape shape, FrameworkElement element)
        {
            selectedShape = shape;

            // Сброс выделения с других фигур
            foreach (FrameworkElement child in DrawingCanvas.Children)
            {
                if (child is Rectangle rect) rect.StrokeThickness = 2;
                else if (child is Ellipse ellipse) ellipse.StrokeThickness = 2;
                else if (child is Polygon polygon) polygon.StrokeThickness = 2;
            }

            if (element is Rectangle selectedRect) selectedRect.StrokeThickness = 4;
            else if (element is Ellipse selectedEllipse) selectedEllipse.StrokeThickness = 4;
            else if (element is Polygon selectedPolygon) selectedPolygon.StrokeThickness = 4;

        }
    }
}