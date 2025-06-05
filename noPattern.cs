// Этот файл демонстрирует код, написанный без применения паттернов
// Показывает проблемы: дублирование кода, нарушение SOLID, сложность поддержки

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace DiagramEditor
{
    // Простые классы фигур без паттернов
    public class SimpleRectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string FillColor { get; set; }
        public string BorderColor { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SimpleCircle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public string FillColor { get; set; }
        public string BorderColor { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SimpleTriangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double SideLength { get; set; }
        public string FillColor { get; set; }
        public string BorderColor { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Главный класс без паттернов - все операции в одном месте
    public partial class MainWindowOld : Window
    {
        private readonly List<object> shapes = new List<object>();
        private object selectedShape = null;
        private int shapeCounter = 1;

        public MainWindowOld()
        {
            InitializeComponent();
        }

        // Создание фигур - простые методы
        private void AddRectangle_Click(object sender, RoutedEventArgs e)
        {
            var rect = new SimpleRectangle
            {
                X = 50 + (shapeCounter * 20),
                Y = 50 + (shapeCounter * 20),
                Width = 80,
                Height = 60,
                FillColor = "Blue",
                BorderColor = "DarkBlue",
                Name = $"Rectangle_{shapeCounter}",
                CreatedAt = DateTime.Now
            };

            shapes.Add(rect);
            DrawRectangle(rect);
            shapeCounter++;
        }

        private void AddCircle_Click(object sender, RoutedEventArgs e)
        {
            var circle = new SimpleCircle
            {
                X = 200 + (shapeCounter * 20),
                Y = 50 + (shapeCounter * 20),
                Radius = 40,
                FillColor = "Red",
                BorderColor = "DarkRed",
                Name = $"Circle_{shapeCounter}",
                CreatedAt = DateTime.Now
            };

            shapes.Add(circle);
            DrawCircle(circle);
            shapeCounter++;
        }

        private void AddTriangle_Click(object sender, RoutedEventArgs e)
        {
            var triangle = new SimpleTriangle
            {
                X = 350 + (shapeCounter * 20),
                Y = 50 + (shapeCounter * 20),
                SideLength = 60,
                FillColor = "Green",
                BorderColor = "DarkGreen",
                Name = $"Triangle_{shapeCounter}",
                CreatedAt = DateTime.Now
            };

            shapes.Add(triangle);
            DrawTriangle(triangle);
            shapeCounter++;
        }

        // ПРОБЛЕМА: Дублирование кода клонирования для каждого типа
        private void CloneShape_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null)
            {
                MessageBox.Show("Сначала выберите фигуру!");
                return;
            }

            object clone = null;

            // ПРОБЛЕМА: Ручная проверка типа и копирование каждого поля
            if (selectedShape is SimpleRectangle rect)
            {
                clone = new SimpleRectangle
                {
                    X = rect.X + 20,                    // ПРОБЛЕМА: Дублирование логики смещения
                    Y = rect.Y + 20,                    // ПРОБЛЕМА: Легко забыть поле
                    Width = rect.Width,                 // ПРОБЛЕМА: Нет гарантии корректности
                    Height = rect.Height,
                    FillColor = rect.FillColor,
                    BorderColor = rect.BorderColor,
                    Name = rect.Name + "_copy",         // ПРОБЛЕМА: Логика именования разбросана
                    CreatedAt = DateTime.Now
                };
                DrawRectangle((SimpleRectangle)clone);
            }
            else if (selectedShape is SimpleCircle circle)
            {
                clone = new SimpleCircle
                {
                    X = circle.X + 20,                 // ПРОБЛЕМА: Повторение той же логики
                    Y = circle.Y + 20,                 // ПРОБЛЕМА: Нарушение DRY принципа
                    Radius = circle.Radius,
                    FillColor = circle.FillColor,
                    BorderColor = circle.BorderColor,
                    Name = circle.Name + "_copy",
                    CreatedAt = DateTime.Now
                };
                DrawCircle((SimpleCircle)clone);
            }
            else if (selectedShape is SimpleTriangle triangle)
            {
                clone = new SimpleTriangle
                {
                    X = triangle.X + 20,               // ПРОБЛЕМА: Код растет экспоненциально
                    Y = triangle.Y + 20,
                    SideLength = triangle.SideLength,
                    FillColor = triangle.FillColor,
                    BorderColor = triangle.BorderColor,
                    Name = triangle.Name + "_copy",
                    CreatedAt = DateTime.Now
                };
                DrawTriangle((SimpleTriangle)clone);
            }

            if (clone != null)
            {
                shapes.Add(clone);
                shapeCounter++;
                MessageBox.Show("Фигура клонирована!");
            }

            // ПРОБЛЕМА: Для каждого нового типа фигуры нужен новый блок кода
        }

        // ПРОБЛЕМА: Отдельные методы сохранения с дублированием логики
        private void SaveAsJson_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для сохранения!");
                return;
            }

            var jsonObjects = new List<object>();

            // ПРОБЛЕМА: Дублирование проверки типов
            foreach (var shape in shapes)
            {
                if (shape is SimpleRectangle rect)
                {
                    jsonObjects.Add(new
                    {
                        Type = "Rectangle",
                        Name = rect.Name,
                        X = rect.X,
                        Y = rect.Y,
                        Width = rect.Width,
                        Height = rect.Height,
                        Area = rect.Width * rect.Height,          // ПРОБЛЕМА: Формулы разбросаны
                        FillColor = rect.FillColor,
                        BorderColor = rect.BorderColor,
                        CreatedAt = rect.CreatedAt
                    });
                }
                else if (shape is SimpleCircle circle)
                {
                    jsonObjects.Add(new
                    {
                        Type = "Circle",
                        Name = circle.Name,
                        X = circle.X,
                        Y = circle.Y,
                        Radius = circle.Radius,
                        Area = Math.PI * circle.Radius * circle.Radius,  // ПРОБЛЕМА: Дублирование формул
                        FillColor = circle.FillColor,
                        BorderColor = circle.BorderColor,
                        CreatedAt = circle.CreatedAt
                    });
                }
                else if (shape is SimpleTriangle triangle)
                {
                    jsonObjects.Add(new
                    {
                        Type = "Triangle",
                        Name = triangle.Name,
                        X = triangle.X,
                        Y = triangle.Y,
                        SideLength = triangle.SideLength,
                        Area = (Math.Sqrt(3) / 4) * triangle.SideLength * triangle.SideLength,
                        FillColor = triangle.FillColor,
                        BorderColor = triangle.BorderColor,
                        CreatedAt = triangle.CreatedAt
                    });
                }
                // ПРОБЛЕМА: Для каждого нового типа - новый блок
            }

            var json = JsonSerializer.Serialize(jsonObjects, new JsonSerializerOptions { WriteIndented = true });
            var filename = $"diagram_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            File.WriteAllText(filename, json);
            MessageBox.Show($"Сохранено в JSON! Файл: {filename}");
        }

        // ПРОБЛЕМА: Практически идентичный код для XML
        private void SaveAsXml_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для сохранения!");
                return;
            }

            var root = new XElement("Diagram");

            // ПРОБЛЕМА: Снова те же проверки типов
            foreach (var shape in shapes)
            {
                if (shape is SimpleRectangle rect)
                {
                    root.Add(new XElement("Rectangle",
                        new XAttribute("Name", rect.Name),
                        new XAttribute("X", rect.X),
                        new XAttribute("Y", rect.Y),
                        new XAttribute("Width", rect.Width),
                        new XAttribute("Height", rect.Height),
                        new XAttribute("Area", rect.Width * rect.Height),    // ПРОБЛЕМА: Формула повторяется
                        new XAttribute("FillColor", rect.FillColor),
                        new XAttribute("BorderColor", rect.BorderColor),
                        new XAttribute("CreatedAt", rect.CreatedAt)
                    ));
                }
                else if (shape is SimpleCircle circle)
                {
                    root.Add(new XElement("Circle",
                        new XAttribute("Name", circle.Name),
                        new XAttribute("X", circle.X),
                        new XAttribute("Y", circle.Y),
                        new XAttribute("Radius", circle.Radius),
                        new XAttribute("Area", Math.PI * circle.Radius * circle.Radius),
                        new XAttribute("FillColor", circle.FillColor),
                        new XAttribute("BorderColor", circle.BorderColor),
                        new XAttribute("CreatedAt", circle.CreatedAt)
                    ));
                }
                else if (shape is SimpleTriangle triangle)
                {
                    root.Add(new XElement("Triangle",
                        new XAttribute("Name", triangle.Name),
                        new XAttribute("X", triangle.X),
                        new XAttribute("Y", triangle.Y),
                        new XAttribute("SideLength", triangle.SideLength),
                        new XAttribute("Area", (Math.Sqrt(3) / 4) * triangle.SideLength * triangle.SideLength),
                        new XAttribute("FillColor", triangle.FillColor),
                        new XAttribute("BorderColor", triangle.BorderColor),
                        new XAttribute("CreatedAt", triangle.CreatedAt)
                    ));
                }
                // ПРОБЛЕМА: Логика обхода повторяется везде
            }

            var filename = $"diagram_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            root.Save(filename);
            MessageBox.Show($"Сохранено в XML! Файл: {filename}");
        }

        // ПРОБЛЕМА: Еще один метод с тем же дублированием
        private void SaveAsCsv_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для сохранения!");
                return;
            }

            var lines = new List<string> { "Type,Name,X,Y,Width,Height,Radius,SideLength,Area,FillColor,BorderColor,CreatedAt" };

            // ПРОБЛЕМА: И снова те же проверки
            foreach (var shape in shapes)
            {
                var line = "";
                if (shape is SimpleRectangle rect)
                {
                    line = $"Rectangle,{rect.Name},{rect.X},{rect.Y},{rect.Width},{rect.Height},,," +
                           $"{rect.Width * rect.Height},{rect.FillColor},{rect.BorderColor},{rect.CreatedAt}";
                }
                else if (shape is SimpleCircle circle)
                {
                    line = $"Circle,{circle.Name},{circle.X},{circle.Y},,,{circle.Radius}," +
                           $"{Math.PI * circle.Radius * circle.Radius},{circle.FillColor},{circle.BorderColor},{circle.CreatedAt}";
                }
                else if (shape is SimpleTriangle triangle)
                {
                    line = $"Triangle,{triangle.Name},{triangle.X},{triangle.Y},,,{triangle.SideLength}," +
                           $"{(Math.Sqrt(3) / 4) * triangle.SideLength * triangle.SideLength},{triangle.FillColor},{triangle.BorderColor},{triangle.CreatedAt}";
                }

                if (!string.IsNullOrEmpty(line))
                    lines.Add(line);
            }

            var filename = $"diagram_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            File.WriteAllLines(filename, lines);
            MessageBox.Show($"Сохранено в CSV! Файл: {filename}");
        }

        // ПРОБЛЕМА: Логика статистики встроена в UI
        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для анализа!");
                return;
            }

            int rectCount = 0, circleCount = 0, triangleCount = 0;
            double totalArea = 0;
            var colorCount = new Dictionary<string, int>();

            // ПРОБЛЕМА: Дублирование логики подсчета
            foreach (var shape in shapes)
            {
                if (shape is SimpleRectangle rect)
                {
                    rectCount++;
                    totalArea += rect.Width * rect.Height;          // ПРОБЛЕМА: Формулы везде

                    if (colorCount.ContainsKey(rect.FillColor))
                        colorCount[rect.FillColor]++;
                    else
                        colorCount[rect.FillColor] = 1;
                }
                else if (shape is SimpleCircle circle)
                {
                    circleCount++;
                    totalArea += Math.PI * circle.Radius * circle.Radius;

                    if (colorCount.ContainsKey(circle.FillColor))
                        colorCount[circle.FillColor]++;
                    else
                        colorCount[circle.FillColor] = 1;
                }
                else if (shape is SimpleTriangle triangle)
                {
                    triangleCount++;
                    totalArea += (Math.Sqrt(3) / 4) * triangle.SideLength * triangle.SideLength;

                    if (colorCount.ContainsKey(triangle.FillColor))
                        colorCount[triangle.FillColor]++;
                    else
                        colorCount[triangle.FillColor] = 1;
                }
            }

            var report = new StringBuilder();
            report.AppendLine("СТАТИСТИКА ДИАГРАММЫ");
            report.AppendLine($"Всего фигур: {shapes.Count}");
            report.AppendLine($"Прямоугольники: {rectCount}");
            report.AppendLine($"Круги: {circleCount}");
            report.AppendLine($"Треугольники: {triangleCount}");
            report.AppendLine($"Общая площадь: {totalArea:F2}");

            if (colorCount.Any())
            {
                report.AppendLine("\nЦвета:");
                foreach (var color in colorCount)
                    report.AppendLine($"  {color.Key}: {color.Value}");
            }

            MessageBox.Show(report.ToString(), "Статистика");
        }

        // ПРОБЛЕМА: Логика валидации тоже встроена
        private void ValidateDiagram_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Нет фигур для проверки!");
                return;
            }

            var errors = new List<string>();
            var warnings = new List<string>();

            // ПРОБЛЕМА: Снова дублирование обхода
            foreach (var shape in shapes)
            {
                if (shape is SimpleRectangle rect)
                {
                    if (rect.Width <= 0 || rect.Height <= 0)
                        errors.Add($"Прямоугольник '{rect.Name}' имеет некорректные размеры");

                    if (rect.X < 0 || rect.Y < 0)
                        warnings.Add($"Прямоугольник '{rect.Name}' частично за пределами области");

                    if (rect.Width * rect.Height > 10000)
                        warnings.Add($"Прямоугольник '{rect.Name}' очень большой");
                }
                else if (shape is SimpleCircle circle)
                {
                    if (circle.Radius <= 0)
                        errors.Add($"Круг '{circle.Name}' имеет некорректный радиус");

                    if (circle.X < 0 || circle.Y < 0)
                        warnings.Add($"Круг '{circle.Name}' частично за пределами области");

                    if (Math.PI * circle.Radius * circle.Radius > 10000)
                        warnings.Add($"Круг '{circle.Name}' очень большой");
                }
                else if (shape is SimpleTriangle triangle)
                {
                    if (triangle.SideLength <= 0)
                        errors.Add($"Треугольник '{triangle.Name}' имеет некорректную длину стороны");

                    if (triangle.X < 0 || triangle.Y < 0)
                        warnings.Add($"Треугольник '{triangle.Name}' частично за пределами области");
                }
            }

            var report = new StringBuilder();

            if (errors.Count == 0 && warnings.Count == 0)
            {
                report.AppendLine("Диаграмма корректна!");
            }
            else
            {
                if (errors.Count > 0)
                {
                    report.AppendLine($"Ошибки ({errors.Count}):");
                    foreach (var error in errors)
                        report.AppendLine($"  - {error}");
                }

                if (warnings.Count > 0)
                {
                    report.AppendLine($"Предупреждения ({warnings.Count}):");
                    foreach (var warning in warnings)
                        report.AppendLine($"  - {warning}");
                }
            }

            MessageBox.Show(report.ToString(), "Результат валидации");
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (shapes.Count == 0)
            {
                MessageBox.Show("Диаграмма уже пуста!");
                return;
            }

            var result = MessageBox.Show("Удалить все фигуры?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                shapes.Clear();
                DrawingCanvas.Children.Clear();
                selectedShape = null;
                shapeCounter = 1;
                MessageBox.Show("Диаграмма очищена!");
            }
        }

        // ПРОБЛЕМА: Отдельные методы рисования для каждого типа
        private void DrawRectangle(SimpleRectangle rect)
        {
            var rectangle = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(rect.FillColor)),
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(rect.BorderColor)),
                StrokeThickness = 2
            };

            Canvas.SetLeft(rectangle, rect.X);
            Canvas.SetTop(rectangle, rect.Y);
            DrawingCanvas.Children.Add(rectangle);
            rectangle.MouseDown += (s, e) => { selectedShape = rect; };
        }

        private void DrawCircle(SimpleCircle circle)
        {
            var ellipse = new Ellipse
            {
                Width = circle.Radius * 2,
                Height = circle.Radius * 2,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(circle.FillColor)),
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(circle.BorderColor)),
                StrokeThickness = 2
            };

            Canvas.SetLeft(ellipse, circle.X - circle.Radius);
            Canvas.SetTop(ellipse, circle.Y - circle.Radius);
            DrawingCanvas.Children.Add(ellipse);
            ellipse.MouseDown += (s, e) => { selectedShape = circle; };
        }

        private void DrawTriangle(SimpleTriangle triangle)
        {
            var polygon = new Polygon();
            var points = new PointCollection();

            var height = triangle.SideLength * Math.Sqrt(3) / 2;
            points.Add(new System.Windows.Point(triangle.X, triangle.Y - height / 2));
            points.Add(new System.Windows.Point(triangle.X - triangle.SideLength / 2, triangle.Y + height / 2));
            points.Add(new System.Windows.Point(triangle.X + triangle.SideLength / 2, triangle.Y + height / 2));

            polygon.Points = points;
            polygon.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(triangle.FillColor));
            polygon.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(triangle.BorderColor));
            polygon.StrokeThickness = 2;

            DrawingCanvas.Children.Add(polygon);
            polygon.MouseDown += (s, e) => { selectedShape = triangle; };
        }
    }
}

/*
ПРОБЛЕМЫ ДАННОГО ПОДХОДА БЕЗ ПАТТЕРНОВ:

1. ДУБЛИРОВАНИЕ КОДА (75% дублирования):
   - Логика обхода фигур повторяется в каждом методе
   - Проверки типов if-else везде одинаковые
   - Формулы площади разбросаны по всему коду
   - Логика смещения координат при клонировании дублируется

2. НАРУШЕНИЕ ПРИНЦИПА ЕДИНСТВЕННОЙ ОТВЕТСТВЕННОСТИ:
   - MainWindow отвечает за UI, бизнес-логику, экспорт, валидацию
   - Смешивание уровней абстракции
   - 450+ строк кода в одном классе

3. СЛОЖНОСТЬ ДОБАВЛЕНИЯ НОВЫХ ТИПОВ ФИГУР:
   - Нужно изменить 8+ методов
   - Легко забыть добавить поддержку в каком-то методе
   - Высокий риск ошибок

4. СЛОЖНОСТЬ ДОБАВЛЕНИЯ НОВЫХ ОПЕРАЦИЙ:
   - Каждая новая операция требует нового метода в MainWindow
   - Дублирование логики обхода фигур
   - Нарушение принципа открытости/закрытости

5. ОТСУТСТВИЕ БЕЗОПАСНОСТИ ДАННЫХ:
   - Координаты и размеры можно случайно изменить
   - Нет валидации при создании объектов
   - Возможны некорректные состояния объектов

6. ВЫСОКАЯ ЦИКЛОМАТИЧЕСКАЯ СЛОЖНОСТЬ:
   - Множественные if-else проверки
   - Сложная логика ветвления
   - Трудность тестирования

КОЛИЧЕСТВЕННЫЕ ПОКАЗАТЕЛИ:
- Объем кода: 450+ строк
- Дублирование: 75%
- Цикломатическая сложность: 25-30
- Время добавления нового типа: 8 изменений в разных местах
- Время добавления новой операции: 50+ строк дублированного кода
*/