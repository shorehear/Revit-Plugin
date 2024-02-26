<<<<<<< HEAD
﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace Plugin
{
    public class ElementCreator
    {
        private Element selectedElement;
        private Document doc;
        public int AmountOfElements;
        public double DistanceBetweenElements;
        public double AngleOfInclination;
        public ElementCreator(Document doc, Element selectedElement)
        {
            this.doc = doc;
            this.selectedElement = selectedElement;
        }

        //Стандартное создание элемента, задает нахождение от исходного по координате X.
        public void CreateElements() 
        {
            if (selectedElement != null)
            {
                Transaction transaction = new Transaction(doc, "Создание элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

                        if (newElementIds.Count > 0)
                        {
                            translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0));
                        }
                    }
                    transaction.Commit();
                }
            }
        }


        //Создание элемента, если требуется повернуть модель.
        public void CreateElementsWithRotationSelf(double angleOfInclination)
        {
            if (selectedElement != null)
            {
                AngleOfInclination = angleOfInclination; 
                Transaction transaction = new Transaction(doc, "Создание элементов с вращением");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

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

        //Создание элемента, если требуется разместить копию/и в какой-то конкретной точке.
        public void CreateElementsWithRotationAround(XYZ position) { }

        //Создание элемента, если требуется и разместить копию в конкретной точке, и вращать его.
        public void CreateElementWithBoth(double angleOfInclination, XYZ position) { }
        private XYZ RotateVector(XYZ vector, double angle)
        {
            // Добавьте свою логику вращения вектора здесь
            // Это может включать в себя использование матрицы вращения или другие методы вращения вектора
            // В данном примере просто возвращается исходный вектор, так как угол наклона равен 0
            return vector;
        }
    }
}
=======
﻿using Autodesk.Revit.DB;
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
            return vector;
        }

        public void OpenRotationWindow()
        {
        }
    }
}
>>>>>>> df9d18eefb07f6425ffd24c7822836ce112a4e96
