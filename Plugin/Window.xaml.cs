﻿using System;
using System.Windows;
using System.Windows.Controls;

using DB = Autodesk.Revit.DB;
using TextBox = System.Windows.Controls.TextBox;

using Plugin;

public partial class MainWindow : Window
{
    private TextBox amountTextBox;
    private TextBox distanceTextBox;
    private CheckBox useRotateAround;
    private CheckBox useRotateSelf;

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
            Content = "Введите расстояние между объектами, которые будут добавлены: ",
            Margin = new Thickness(10, 10, 0, 0)  
        };
        distanceTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

        useRotateAround = new CheckBox()
        {
            Content = "Разместить объекты в произвольной зоне",
            Margin = new Thickness(10, 10, 0, 0)
        };

        useRotateSelf = new CheckBox()
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
        panel.Children.Add(useRotateAround);
        panel.Children.Add(useRotateSelf);
        panel.Children.Add(okButton);

        Content = panel;
    }

    enum ObjectState
    {
        DntNeedAnyRotation,
        NeedRotationSelf,
        NeedRotationAround,
        NeedBoth
    }

    private ObjectState GetRotationState(bool rotateAround, bool rotateSelf)
    {
        if (!rotateAround && !rotateSelf)
        {
            return ObjectState.DntNeedAnyRotation;
        }
        else if (rotateSelf && !rotateAround)
        {
            return ObjectState.NeedRotationSelf;
        }
        else if (rotateAround && !rotateSelf)
        {
            return ObjectState.NeedRotationAround;
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
            ElementCreator addElement = new ElementCreator(doc, selectedElement);
            bool rotateAround = useRotateAround.IsChecked ?? false;
            bool rotateSelf = useRotateSelf.IsChecked ?? false;

            switch (GetRotationState(rotateAround, rotateSelf))
            {
                case ObjectState.DntNeedAnyRotation:
                    addElement.AmountOfElements = int.Parse(amountTextBox.Text);
                    addElement.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    addElement.CreateElements();
                    break;

                case ObjectState.NeedRotationSelf:
                    RotateSelfWindow rotateSelfWindow = new RotateSelfWindow();
                    rotateSelfWindow.ShowDialog();


                    addElement.AmountOfElements = int.Parse(amountTextBox.Text);
                    addElement.DistanceBetweenElements = double.Parse(distanceTextBox.Text);
                    addElement.CreateElementsWithRotationSelf(); 
                    break;

                case ObjectState.NeedRotationAround:
                    //
                    RotateAroundWindow rotateAroundWindow = new RotateAroundWindow();
                    rotateAroundWindow.ShowDialog();

                    addElement.CreateElementsWithRotationAround();
                    break;
                case ObjectState.NeedBoth:
                    //
                    RotateBothWindow rotateBothWindow = new RotateBothWindow();
                    rotateBothWindow.ShowDialog();

                    addElement.CreateElementWithBoth();
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

public abstract class RotationWindowBase : Window
{
    protected TextBox rotationAngleTextBox;
    protected TextBox rotationPointTextBox;
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

public class RotateAroundWindow : RotationWindowBase
{
    public RotateAroundWindow() : base("Настройки вращения (RotateAround)")
    {
    }

    protected override void SetAdditionalControls(StackPanel panel)
    {
        var rotationPointLabel = new Label() { Content = "Координаты точки вращения (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0) };
        rotationPointTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        panel.Children.Add(rotationPointLabel);
        panel.Children.Add(rotationPointTextBox);
        panel.Children.Add(applyButton);

    }

    protected override void ApplyRotationSettings()
    {
        double rotationAngle = double.Parse(rotationAngleTextBox.Text);
        string[] pointCoordinates = rotationPointTextBox.Text.Split(',');
        double rotationPointX = double.Parse(pointCoordinates[0]);
        double rotationPointY = double.Parse(pointCoordinates[1]);
        double rotationPointZ = double.Parse(pointCoordinates[2]);
        //
    }
}

public class RotateSelfWindow : RotationWindowBase
{
    public RotateSelfWindow() : base("Настройки вращения (RotateSelf)")
    {
    }

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
        //
    }
}

public class RotateBothWindow : RotationWindowBase
{
    public RotateBothWindow() : base("Настройки вращения (RotateBoth)")
    {
    }

    protected override void SetAdditionalControls(StackPanel panel)
    {
        var rotationAngleLabel = new Label() { Content = "Угол вращения (в градусах):", Margin = new Thickness(10, 10, 0, 0) };
        rotationAngleTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        panel.Children.Add(rotationAngleLabel);
        panel.Children.Add(rotationAngleTextBox);

        var rotationPointLabel = new Label() { Content = "Координаты точки вращения (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0) };
        rotationPointTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
        panel.Children.Add(rotationPointLabel);
        panel.Children.Add(rotationPointTextBox);

        panel.Children.Add(applyButton);
    }

    protected override void ApplyRotationSettings()
    {
        double rotationAngle = double.Parse(rotationAngleTextBox.Text);
        string[] pointCoordinates = rotationPointTextBox.Text.Split(',');
        double rotationPointX = double.Parse(pointCoordinates[0]);
        double rotationPointY = double.Parse(pointCoordinates[1]);
        double rotationPointZ = double.Parse(pointCoordinates[2]);
    }
}

