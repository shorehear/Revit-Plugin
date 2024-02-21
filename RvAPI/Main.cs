using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RvAPI
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RVAPI : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiapp = commandData.Application;
                var uidoc = uiapp.ActiveUIDocument;
                var doc = uidoc.Document;

                Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Выберите элемент.");
                if (pickedRef == null)
                    return Result.Cancelled;
                Autodesk.Revit.DB.Element selectedElement = doc.GetElement(pickedRef);

                InputForm Task = new InputForm(doc, selectedElement);
                Task.ShowDialog();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }

    public class InputForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox amountTextBox;
        private System.Windows.Forms.TextBox distanceTextBox;
        private Document doc;
        private Autodesk.Revit.DB.Element selectedElement;
        public InputForm(Document doc, Element selectedElement)
        {
            this.doc = doc;
            this.selectedElement = selectedElement;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Дублировать объект";
            Width = 400;
            Height = 280;

            var infoLabel = new Label()
            { Text = $"Выбранный объект: {selectedElement.Category.Name}", Top = 10, Left = 10, Width = 200 };
            Controls.Add(infoLabel);

            var amountLabel = new Label()
            { Text = "Введите количество объектов, которое вы желаете добавить: ", Top = 40, Left = 10, Width = 400 };
            Controls.Add(amountLabel);
            amountTextBox = new System.Windows.Forms.TextBox() { Top = 63, Left = 10, Width = 100 };
            Controls.Add(amountTextBox);

            var distanceLabel = new Label()
            { Text = "Введите расстояние между объектами, которые будут добавлены: ", Top = 90, Left = 10, Width = 400 };
            Controls.Add(distanceLabel);
            distanceTextBox = new System.Windows.Forms.TextBox() { Top = 113, Left = 10, Width = 100 };
            Controls.Add(distanceTextBox);

            var angleLabel = new Label()
            { Text = "Введите угол направления объектов, которые будут добавлены: ", Top = 140, Left = 10, Width = 400 };
            Controls.Add(angleLabel);
            var angleTextBox = new System.Windows.Forms.TextBox() { Top = 163, Left = 10, Width = 100 };
            Controls.Add(angleTextBox);

            var okButton = new Button()
            { Text = "OK", Top = 190, Left = (Width/2 - 50) };
            okButton.Click += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(amountTextBox.Text) && !string.IsNullOrEmpty(distanceTextBox.Text))
                {
                    CreatedElement addElement = new CreatedElement(doc, selectedElement);
                    addElement.amountOfElements = int.Parse(amountTextBox.Text);
                    addElement.distanceBetweenElements = double.Parse(distanceTextBox.Text);
                    addElement.angleOfInclination = double.Parse(angleTextBox.Text);
                    addElement.CreateElements();

                    DialogResult = DialogResult.OK;
                    Close();
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(okButton);
        }
    }
    public class CreatedElement
    {
        private Document doc;
        public CreatedElement(Document doc, Element selectedElement)
        {
            this.doc = doc;
            this.selectedElement = selectedElement;
        }
        public Element selectedElement { get; set; }
        public int amountOfElements { get; set; }
        public double distanceBetweenElements { get; set; }
        public double angleOfInclination { get; set; }
        public void CreateElements()
        {
            if (selectedElement != null)
            {
                Transaction transaction = new Transaction(doc, "Создание элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(distanceBetweenElements, 0, 0);

                    for (int i = 0; i < amountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

                        if (newElementIds.Count > 0)
                        {
                            translation = RotateVector(translation, angleOfInclination); 
                            translation = translation.Add(new XYZ(distanceBetweenElements, 0, 0)); 
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public XYZ RotateVector(XYZ vector, double angleOfInclination)
        {
            double angleInRadians = angleOfInclination * Math.PI / 180.0;
            Transform rotationTransform = Transform.CreateRotationAtPoint(new XYZ(0, 0, 1), angleInRadians, XYZ.Zero);
            return rotationTransform.OfPoint(vector);
        }
    }
}
