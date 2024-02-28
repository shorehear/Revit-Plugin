using System;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using DB = Autodesk.Revit.DB;
using TextBox = System.Windows.Controls.TextBox;
using Plugin;

public partial class MainWindow : Window
{
    private TextBox amountTextBox;
    private TextBox distanceTextBox;
    private TextBox coordinatesTextBox;
    private TextBox rotateTextBox;
    private CheckBox useMove;
    private CheckBox useRotate;
    private char rotationAxis;

    private DB.Document doc;
    private DB.Element selectedElement;

    public MainWindow(DB.Document doc, DB.Element selectedElement)
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

        useRotate = new CheckBox()
        {
            Content = "Использовать вращение",
            Margin = new Thickness(10, 10, 0, 0)
        };
        var rotateLabel = new Label()
        {
            Content = "Введите угол вращения:",
            Margin = new Thickness(10, 0, 0, 0)
        };
        rotateTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

        var rotationAxisLabel = new Label { Content = "Выберите ось вращения:", Margin = new Thickness(10, 0, 0, 0) };
        var radioButtonX = new RadioButton { Content = "Ось X" };
        var radioButtonY = new RadioButton { Content = "Ось Y" };
        var radioButtonZ = new RadioButton { Content = "Ось Z" };

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

        panel.Children.Add(useRotate);
        panel.Children.Add(rotateLabel);
        panel.Children.Add(rotateTextBox);
        panel.Children.Add(rotationAxisLabel);

        RadioButton[] radioButtons = { radioButtonX, radioButtonY, radioButtonZ };
        foreach (RadioButton radioButton in radioButtons)
        {
            radioButton.Checked += (sender, e) => { rotationAxis = radioButton.Content.ToString()[0]; };
            panel.Children.Add(radioButton);
        }

        panel.Children.Add(okButton);

        Content = panel;
    }
    enum ObjectState
    {
        DntNeedAnyRotation,
        NeedRotation,
        NeedMove,
        NeedBoth
    }

    private ObjectState GetState(bool move, bool rotate)
    {
        if (!move && !rotate)
        {
            return ObjectState.DntNeedAnyRotation;
        }
        else if (rotate && !move)
        {
            return ObjectState.NeedRotation;
        }
        else if (move && !rotate)
        {
            return ObjectState.NeedMove;
        }
        else
        {
            return ObjectState.NeedBoth;
        }
    }
    public XYZ GetRotationBasis(char rotationAxis)
    {
        switch (rotationAxis)
        {
            case 'X':
                return XYZ.BasisX;
            case 'Y':
                return XYZ.BasisY;
            case 'Z':
                return XYZ.BasisZ;
            default:
                return XYZ.Zero;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            bool move = useMove.IsChecked ?? false;
            bool rotate = useRotate.IsChecked ?? false;

            switch (GetState(move, rotate))
            {
                case ObjectState.DntNeedAnyRotation:
                    ElementCopier defaultCopier = new ElementCopier(doc, selectedElement);
                    defaultCopier.AmountOfElements = int.Parse(amountTextBox.Text);
                    defaultCopier.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    defaultCopier.CopyElements();
                    break;

                case ObjectState.NeedMove:
                    string[] pointCoordinates = coordinatesTextBox.Text.Split(',');
                    XYZ coordinatesPoint = new XYZ(double.Parse(pointCoordinates[0]), double.Parse(pointCoordinates[1]), double.Parse(pointCoordinates[2]));

                    ElementCopier moveCopier = new ElementCopier(doc, selectedElement);
                    moveCopier.AmountOfElements = int.Parse(amountTextBox.Text);
                    moveCopier.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    moveCopier.MoveCopiedElements(coordinatesPoint);
                    break;

                case ObjectState.NeedRotation:
                    double rotationAngle = double.Parse(rotateTextBox.Text);

                    ElementCopier rotationCopier = new ElementCopier(doc, selectedElement);
                    rotationCopier.AmountOfElements = int.Parse(amountTextBox.Text);
                    rotationCopier.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    rotationCopier.RotateCopiedElements(GetRotationBasis(rotationAxis), rotationAngle);
                    break;

                case ObjectState.NeedBoth:


                    // RotateMoveCopiedObject rotateMoveWindow = new RotateMoveCopiedObject(doc, selectedElement, amountTextBox);
                    // rotateMoveWindow.ShowDialog();
                    break;
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
            Close();
        }
        finally
        {
            DialogResult = true;
            Close();
        }
    }
}