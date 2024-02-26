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
    private CheckBox usePlace;
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
        Height = 280;

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
            Content = "Введите расстояние между объектами, которые будут добавлены (по координате X): ",
            Margin = new Thickness(10, 10, 0, 0)  
        };
        distanceTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

        usePlace = new CheckBox()
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
        panel.Children.Add(usePlace);
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

        NeedPlaseInPosition,
        //Копия объекта нуждается в кастомном размещении, не во вращении. Используются размещения по XYZ, задание количества объектов.

        NeedBoth
        //Копия объекта нуждается и в кастомном размещении, и во вращении. Используются размещения по XYZ, угол поворота, задание количества объектов.
    }

    private ObjectState GetState(bool place, bool rotate)
    {
        if (!place && !rotate)
        {
            return ObjectState.DntNeedAnyRotation;
        }
        else if (rotate && !place)
        {
            return ObjectState.NeedRotation;
        }
        else if (place && !rotate)
        {
            return ObjectState.NeedPlaseInPosition;
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
            bool place = usePlace.IsChecked ?? false;
            bool rotate = useRotate.IsChecked ?? false;

            switch (GetState(place, rotate))
            {
                case ObjectState.DntNeedAnyRotation:
                    ElementCreator addElement = new ElementCreator(doc, selectedElement);
                    addElement.AmountOfElements = int.Parse(amountTextBox.Text);
                    addElement.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    addElement.CreateElements();
                    break;

                case ObjectState.NeedRotation:
                    RotateWindow rotateWindow = new RotateWindow(doc, selectedElement, amountTextBox);
                    rotateWindow.ShowDialog();
                    break;

                case ObjectState.NeedPlaseInPosition:
                    PlasementPositionWindow placementPositionWindow = new PlasementPositionWindow(doc, selectedElement, amountTextBox);
                    placementPositionWindow.ShowDialog();
                    break;
                case ObjectState.NeedBoth:
                    BothFunctionsWindow rotateBothWindow = new BothFunctionsWindow(doc, selectedElement, amountTextBox);
                    rotateBothWindow.ShowDialog();
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

public abstract class RotationWindowBase : Window //Отрисовывает базовое окно 
{
    protected TextBox rotationAngleTextBox;
    protected TextBox placeablePointTextBox;
    protected Button applyButton;

    protected RotationWindowBase(string title)
    {
        InitializeComponent(title);
    }

    private void InitializeComponent(string title)
    {
        Title = title;
        Width = 300;
        Height = 180;

        rotationAngleTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        applyButton = new Button() { Content = "Применить", Margin = new Thickness(10, 10, 180, 0) };
        applyButton.Click += ApplyButton_Click;

        StackPanel panel = new StackPanel();
        SetAdditionalControls(panel); 

        Content = panel;
    }

    protected abstract void SetAdditionalControls(StackPanel panel);

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyRotationSettings();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
            Close();
        }
    }

    protected abstract void ApplyRotationSettings();
}

//Отрисовывает базовое окно + функционал, чтобы задать координаты точки, в которой размещать объект/ы
public class PlasementPositionWindow : RotationWindowBase 
{
    private Element selectedElement;
    private Document doc;
    private ElementCreator addElement;
    public PlasementPositionWindow(Document doc, Element selectedElement, TextBox amountElements) : base("Настройки размещения (Plasement Position)")
    {
        this.selectedElement = selectedElement;
        this.doc = doc;

        addElement = new ElementCreator(doc, selectedElement) { AmountOfElements = int.Parse(amountElements.Text) };
    }

    protected override void SetAdditionalControls(StackPanel panel)
    {
        if (addElement.AmountOfElements != 0)
        {
            if (addElement.AmountOfElements > 1)
            {
                var rotationPointLabel = new Label() { Content = "Координаты точки, в которой необходимо разместить объекты (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0) };
                placeablePointTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
                panel.Children.Add(rotationPointLabel);
                panel.Children.Add(placeablePointTextBox);
                panel.Children.Add(applyButton);
            }
            else if (addElement.AmountOfElements == 1) 
            { 
                var rotationPointLabel = new Label() { Content = "Координаты точки, в которой необходимо разместить объект (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0) };
                placeablePointTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
                panel.Children.Add(rotationPointLabel);
                panel.Children.Add(placeablePointTextBox);
                panel.Children.Add(applyButton);
            }
        }
    }

    protected override void ApplyRotationSettings() //Задает позицию размещения будущего/их объектов
    {
        string[] pointCoordinates = placeablePointTextBox.Text.Split(',');
        double plasementPointX = double.Parse(pointCoordinates[0]);
        double plasementPointY = double.Parse(pointCoordinates[1]);
        double plasementPointZ = double.Parse(pointCoordinates[2]);
        XYZ PlasementPosition = new XYZ(plasementPointX, plasementPointY, plasementPointZ);
        addElement.CreateElementsWithRotationAround(PlasementPosition);
    }
}

public class RotateWindow : RotationWindowBase
{
    private Element selectedElement;
    private ElementCreator addElement;
    private Document doc;

    public RotateWindow(Document doc, Element selectedElement, TextBox amountElements) : base("Настройки вращения (RotateSelf)")
    {
        this.selectedElement = selectedElement;
        this.doc = doc;

        addElement = new ElementCreator(doc, selectedElement) { AmountOfElements = int.Parse(amountElements.Text) };

    }

//    addElement.CreateElementsWithRotationSelf();
    protected override void SetAdditionalControls(StackPanel panel)
    {
        //
        var rotationAngleLabel = new Label() { Content = "Угол вращения (в градусах):", Margin = new Thickness(10, 10, 0, 0) };
        rotationAngleTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        panel.Children.Add(rotationAngleLabel);
        panel.Children.Add(rotationAngleTextBox);
        panel.Children.Add(applyButton);
    }

    protected override void ApplyRotationSettings()
    {
        double rotationAngle = double.Parse(rotationAngleTextBox.Text);
        
        Location location = selectedElement.Location;
        if (location is LocationPoint locationPoint)
        {
        }
        else if (location is LocationCurve locationCurve)
        {

        }
        else
        {
            MessageBox.Show("Выбранный элемент не поддерживает расположение точки или на кривой для вращения.");
        }
    }
}

public class BothFunctionsWindow : RotationWindowBase
{
    private Element selectedElement;
    private Document doc;
    private ElementCreator addElement;
    public BothFunctionsWindow(Document doc, Element selectedElement, TextBox amountElements) : base("Настройки вращения (RotateBoth)")
    {
        this.selectedElement = selectedElement;
        this.doc = doc;

        addElement = new ElementCreator(doc, selectedElement) { AmountOfElements = int.Parse(amountElements.Text) };
    }

    protected override void SetAdditionalControls(StackPanel panel)
    {
        var rotationAngleLabel = new Label() { Content = "Угол вращения (в градусах):", Margin = new Thickness(10, 10, 0, 0) };
        rotationAngleTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        panel.Children.Add(rotationAngleLabel);
        panel.Children.Add(rotationAngleTextBox);

        var placeablePointLabel = new Label() { Content = "Координаты точки перемещения (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0) };
        placeablePointTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        panel.Children.Add(placeablePointLabel);
        panel.Children.Add(placeablePointTextBox);

        panel.Children.Add(applyButton);
    }

    protected override void ApplyRotationSettings()
    {
        double rotationAngle = double.Parse(rotationAngleTextBox.Text);
        string[] pointCoordinates = placeablePointTextBox.Text.Split(',');
        double positionPointX = double.Parse(pointCoordinates[0]);
        double positionPointY = double.Parse(pointCoordinates[1]);
        double positionPointZ = double.Parse(pointCoordinates[2]);
        XYZ position = new XYZ(positionPointX, positionPointY, positionPointZ);
        addElement.CreateElementWithBoth(rotationAngle, position);
    }
}

