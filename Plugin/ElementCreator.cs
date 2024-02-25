using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace Plugin
{
    public class ElementCreator
    {
        private Document doc;
        public ElementCreator(Document doc, Element selectedElement)
        {
            this.doc = doc;
            SelectedElement = selectedElement;
        }
        public Element SelectedElement { get; set; }
        public int AmountOfElements { get; set; }
        public double DistanceBetweenElements { get; set; }
        public double AngleOfInclination { get; set; } = 0;
        public bool UseRotation { get; set; }

        public void CreateElements() //без вращений в пространстве, без вращений элементов
        {
            if (SelectedElement != null)
            {
                Transaction transaction = new Transaction(doc, "Создание элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, SelectedElement.Id, translation);

                        if (newElementIds.Count > 0)
                        {
                            translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0));
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        public void CreateElementsWithRotationSelf()  //вращение элементов
        {
            if (SelectedElement != null)
            {
                Transaction transaction = new Transaction(doc, "Создание элементов с вращением");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, SelectedElement.Id, translation);

                        if (newElementIds.Count > 0)
                        {
                            translation = RotateVector(translation, AngleOfInclination);
                            translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0));
                        }
                    }

                    transaction.Commit();
                }
            }
        }
       
        public void CreateElementsWithRotationAround() { }
        public void CreateElementWithBoth() { }
        private XYZ RotateVector(XYZ vector, double angle)
        {
            // Добавьте свою логику вращения вектора здесь
            // Это может включать в себя использование матрицы вращения или другие методы вращения вектора
            // В данном примере просто возвращается исходный вектор, так как угол наклона равен 0
            return vector;
        }

        public void OpenRotationWindow()
        {
            // Откройте окно для уточнения параметров вращения
            // Добавьте код для создания и отображения окна WPF с полями для уточнения параметров
            // например, угла наклона, если UseRotation установлен в true
        }
    }
}
