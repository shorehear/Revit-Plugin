using System;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Plugin;

public partial class DefaultWindow : Window, IDisposable
{
    private TextBox amountTextBox;
    private TextBox distanceTextBox;
    private TextBox coordinatesTextBox;
    private CheckBox useMove;

    private readonly Document doc;
    private readonly Element selectedElement;

    public DefaultWindow(Document doc, Element selectedElement)
    {
        this.doc = doc;
        this.selectedElement = selectedElement;
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        Title = "Дублировать объект";
        Width = 420;
        Height = 460;

        var infoLabel = new Label()
        {
            Content = $"Выбранный объект: {selectedElement.Category.Name}",
            Margin = new Thickness(10, 10, 0, 0)
        };

        var amountLabel = new Label()
        {
            Content = "Введите количество объектов, которое вы желаете добавить: ",
            Margin = new Thickness(10, 10, 0, 0)
        };
        amountTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

        var distanceLabel = new Label()
        {
            Content = "Введите расстояние между объектами, которые будут добавлены:\n(по координате X)",
            Margin = new Thickness(10, 10, 0, 0)
        };
        distanceTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

        useMove = new CheckBox()
        {
            Content = "Использовать координаты",
            Margin = new Thickness(10, 10, 0, 0)
        };
        var coordinatesLabel = new Label()
        {
            Content = "Введите координаты точки размещения (X, Y, Z):",
            Margin = new Thickness(10, 0, 0, 0)
        };
        coordinatesTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

        var okButton = new Button()
        {
            Content = "OK",
            Margin = new Thickness(10, 10, 350, 0)
        };
        okButton.Click += OkButton_Click;

        StackPanel panel = new StackPanel();
        panel.Children.Add(infoLabel);
        panel.Children.Add(amountLabel);
        panel.Children.Add(amountTextBox);
        panel.Children.Add(distanceLabel);
        panel.Children.Add(distanceTextBox);


        panel.Children.Add(useMove);
        panel.Children.Add(coordinatesLabel);
        panel.Children.Add(coordinatesTextBox);

        panel.Children.Add(okButton);

        Content = panel;
    }

    public void Dispose()
    {
        selectedElement?.Dispose();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            bool move = useMove.IsChecked ?? false;

            switch (move)
            {
                case false:
                    ElementCopier defaultCopier = new ElementCopier(doc, selectedElement)
                    {
                        AmountOfElements = int.Parse(amountTextBox.Text),
                        DistanceBetweenElements = double.Parse(distanceTextBox.Text)
                    };
                    defaultCopier.CopyElements();
                    break;

                case true:
                    string[] pointCoordinates = coordinatesTextBox.Text.Split(',');
                    XYZ coordinatesPoint = new XYZ(double.Parse(pointCoordinates[0]), double.Parse(pointCoordinates[1]), double.Parse(pointCoordinates[2]));

                    ElementCopier moveCopier = new ElementCopier(doc, selectedElement)
                    {
                        AmountOfElements = int.Parse(amountTextBox.Text),
                        DistanceBetweenElements = double.Parse(distanceTextBox.Text)
                    };
                    moveCopier.MoveCopiedElements(coordinatesPoint);
                    break;

            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
            Close();
        }
        finally
        {
            DialogResult = true;
            Close();
        }
    }
}