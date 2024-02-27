using System;
using System.Windows;
using System.Windows.Controls;
using DB = Autodesk.Revit.DB;
using TextBox = System.Windows.Controls.TextBox;


using Plugin;

public partial class MainWindow : Window
{
    private TextBox amountTextBox;
    private TextBox distanceTextBox;
    private CheckBox useMove;
    private CheckBox useRotate;

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
        Height = 300;

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
            Content = "Разместить объекты в произвольной зоне",
            Margin = new Thickness(10, 10, 0, 0)
        };

        useRotate = new CheckBox()
        {
            Content = "Вращать добавляемый объект",
            Margin = new Thickness(10, 10, 0, 0)
        };

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
        panel.Children.Add(useRotate);
        panel.Children.Add(okButton);

        Content = panel;
    }

    enum ObjectState
    {
        DntNeedAnyRotation,
        //Копия объекта не нуждается во вращении, кастомном размещении. Используются только размещения на координате X, задание количества объектов.

        NeedRotation,
        //Копия объекта нуждается во вращении, но не в кастомном размещении. Используются размещения на координате Х, задание количества объектов, угол поворота.

        NeedMove,
        //Копия объекта нуждается в кастомном размещении, не во вращении. Используются размещения по XYZ, задание количества объектов.

        NeedBoth
        //Копия объекта нуждается и в кастомном размещении, и во вращении. Используются размещения по XYZ, угол поворота, задание количества объектов.
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

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            bool move = useMove.IsChecked ?? false;
            bool rotate = useRotate.IsChecked ?? false;

            switch (GetState(move, rotate))
            {
                case ObjectState.DntNeedAnyRotation:
                    ElementCopier defaultWindow = new ElementCopier(doc, selectedElement);
                    defaultWindow.AmountOfElements = int.Parse(amountTextBox.Text);
                    defaultWindow.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    defaultWindow.CopyElements();
                    break;

                case ObjectState.NeedMove:
                    MoveCopiedObject moveWindow = new MoveCopiedObject(doc, selectedElement, amountTextBox);
                    moveWindow.Show();
                    break;

                case ObjectState.NeedRotation:
                    RotateCopiedObject rotateWindow = new RotateCopiedObject(doc, selectedElement, amountTextBox);
                    rotateWindow.ShowDialog();
                    break;

                case ObjectState.NeedBoth:
                    RotateMoveCopiedObject rotateMoveWindow = new RotateMoveCopiedObject(doc, selectedElement, amountTextBox);
                    rotateMoveWindow.ShowDialog();
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
