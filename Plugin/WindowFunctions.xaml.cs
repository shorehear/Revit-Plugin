using System;
using System.Windows;
using Autodesk.Revit.DB;
using System.Windows.Controls;
using TextBox = System.Windows.Controls.TextBox;

namespace Plugin
{
    public abstract class ObjectManipulate : Window
    {
        protected Button applyButton;
        protected ObjectManipulate(string title)
        {
            InitializeComponent(title);
        }
        private void InitializeComponent(string title)
        {
            Title = title;
            Width = 300;
            Height = 300;

            applyButton = new Button() { Content = "Применить", Margin = new Thickness(10, 10, 180, 0) };
            applyButton.Click += ApplyButton_Click;

            StackPanel panel = new StackPanel();
            SetAdditionalControls(panel);

            Content = panel;
        }

        protected void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyManipulateSettings();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        protected abstract void SetAdditionalControls(StackPanel panel);

        protected abstract void ApplyManipulateSettings();
    }

    //Отрисовывает базовое окно + функционал, чтобы задать координаты точки, в которой размещать объект/ы
    public class MoveCopiedObject : ObjectManipulate
    {
        private Element element;
        private Document doc;
        private ElementCopier createCopy;
        protected TextBox coordinatesPointTextBox;
        public MoveCopiedObject(Document doc, Element element, TextBox amountElements) : base("Настройки размещения (Plasement Position)")
        {
            this.element = element;
            this.doc = doc;

            createCopy = new ElementCopier(doc, element) { AmountOfElements = int.Parse(amountElements.Text) };
        }

        protected override void SetAdditionalControls(StackPanel panel)
        {
            var coordinatesPointLabel = new Label() { Content = "Координаты точки, в которой необходимо разместить копирование (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0) };
            coordinatesPointTextBox = new TextBox() { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };
            panel.Children.Add(coordinatesPointLabel);
            panel.Children.Add(coordinatesPointTextBox);
            panel.Children.Add(applyButton);
        }

        protected override void ApplyManipulateSettings() //Задает позицию размещения будущего/их объектов
        {
            string[] pointCoordinates = coordinatesPointTextBox.Text.Split(',');
            double plasementPointX = double.Parse(pointCoordinates[0]);
            double plasementPointY = double.Parse(pointCoordinates[1]);
            double plasementPointZ = double.Parse(pointCoordinates[2]);
            XYZ PlasementPosition = new XYZ(plasementPointX, plasementPointY, plasementPointZ);
            createCopy.CopyElementAtPosition(PlasementPosition);
        }
    }

    public class RotateCopiedObject : ObjectManipulate
    {
        private Element element;
        private ElementCopier createCopy;
        private Document doc;

        private TextBox rotationAngleTextBox;
        private char rotationAxis;

        public RotateCopiedObject(Document doc, Element element, TextBox amountElements) : base("Настройки вращения (RotateSelf)")
        {
            this.element = element;
            this.doc = doc;

            createCopy = new ElementCopier(doc, element) { AmountOfElements = int.Parse(amountElements.Text) };
        }

        protected override void SetAdditionalControls(StackPanel panel)
        {
            var rotationAngleLabel = new Label() { Content = "Угол вращения (в градусах):", Margin = new Thickness(10, 10, 0, 0) };
            rotationAngleTextBox = new TextBox { Width = 100, Height = 20, Margin = new Thickness(10, 0, 0, 0) };

            var rotationAxisLabel = new Label { Content = "Выберите ось вращения:", Margin = new Thickness(10, 0, 0, 0) };
            var radioButtonX = new RadioButton { Content = "Ось X" };
            var radioButtonY = new RadioButton { Content = "Ось Y" };
            var radioButtonZ = new RadioButton { Content = "Ось Z" };

            panel.Children.Add(rotationAxisLabel);

            RadioButton[] radioButtons = { radioButtonX, radioButtonY, radioButtonZ };
            foreach (RadioButton radioButton in radioButtons)
            {
                radioButton.Checked += (sender, e) => { rotationAxis = radioButton.Content.ToString()[0]; };
                panel.Children.Add(radioButton);
            }
            panel.Children.Add(rotationAngleLabel);
            panel.Children.Add(rotationAngleTextBox);
            panel.Children.Add(applyButton);
        }

        protected override void ApplyManipulateSettings()
        {
            double rotationAngle = double.Parse(rotationAngleTextBox.Text);
            createCopy.RotateCopiedElement(rotationAngle, rotationAxis);
        }
    }

    public class RotateMoveCopiedObject : ObjectManipulate
    {
        private Element element;
        private Document doc;
        private ElementCopier createCopy;

        private TextBox coordinatesPointTextBox;
        private TextBox rotationAngleTextBox;
        private char rotationAxis;

        public RotateMoveCopiedObject(Document doc, Element element, TextBox amountElements) : base("Настройки размещения и вращения (Rotate and Move)")
        {
            this.element = element;
            this.doc = doc;

            createCopy = new ElementCopier(doc, element) { AmountOfElements = int.Parse(amountElements.Text) };
        }

        protected override void SetAdditionalControls(StackPanel panel)
        {
            var coordinatesPointTextBlock = new TextBlock { Text = "Координаты точки, в которой необходимо\nразместить копирование (X, Y, Z):", Margin = new Thickness(10, 10, 0, 0), Width = 300, TextWrapping = TextWrapping.Wrap };
            coordinatesPointTextBox = new TextBox() { Width = 100, Height = 20};
            panel.Children.Add(coordinatesPointTextBlock);
            panel.Children.Add(coordinatesPointTextBox);

            var rotationAngleLabel = new Label() { Content = "Угол вращения (в градусах):", Margin = new Thickness(10, 10, 0, 0) };
            rotationAngleTextBox = new TextBox { Width = 100, Height = 20 };

            var rotationAxisLabel = new Label { Content = "Выберите ось вращения:", Margin = new Thickness(10, 0, 0, 0) };
            var radioButtonX = new RadioButton { Content = "Ось X" };
            var radioButtonY = new RadioButton { Content = "Ось Y" };
            var radioButtonZ = new RadioButton { Content = "Ось Z" };

            panel.Children.Add(rotationAxisLabel);

            RadioButton[] radioButtons = { radioButtonX, radioButtonY, radioButtonZ };
            foreach (RadioButton radioButton in radioButtons)
            {
                radioButton.Checked += (sender, e) => { rotationAxis = radioButton.Content.ToString()[0]; };
                panel.Children.Add(radioButton);
            }

            panel.Children.Add(rotationAngleLabel);
            panel.Children.Add(rotationAngleTextBox);
            panel.Children.Add(applyButton);
        }

        protected override void ApplyManipulateSettings()
        {
            double rotationAngle = double.Parse(rotationAngleTextBox.Text);
            string[] pointCoordinates = coordinatesPointTextBox.Text.Split(',');
            double plasementPointX = double.Parse(pointCoordinates[0]);
            double plasementPointY = double.Parse(pointCoordinates[1]);
            double plasementPointZ = double.Parse(pointCoordinates[2]);
            XYZ plasementPosition = new XYZ(plasementPointX, plasementPointY, plasementPointZ);

            createCopy.RotateMoveCopiedElement(plasementPosition, rotationAngle, rotationAxis);
        }
    }

}