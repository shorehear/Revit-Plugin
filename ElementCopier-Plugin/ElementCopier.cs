using Autodesk.Revit.DB;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Plugin
{
    public class ElementCopier
    {
        private Element selectedElement;
        private Document doc;
        public int AmountOfElements;
        public double DistanceBetweenElements;
        public double AngleOfInclination;
        public ElementCopier(Document doc, Element selectedElement)
        {
            this.doc = doc;
            this.selectedElement = selectedElement;
        }

        //Стандартное создание элемента, задает нахождение от исходного по координате X.
        public void CopyElements()
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

        public void MoveCopiedElements(XYZ position)
        {
            if (selectedElement != null)
            {
                Transaction transaction = new Transaction(doc, "Перемещение элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(0, 0, 0); // Начальное смещение равно нулю

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

                        if (newElementIds != null && newElementIds.Count > 0)
                        {
                            ElementId newElementId = newElementIds.FirstOrDefault();
                            Element newElement = doc.GetElement(newElementId);
                            ElementTransformUtils.MoveElement(doc, newElement.Id, position + translation); 
                        }

                        translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0)); 
                    }

                    transaction.Commit();
                }
            }
        }


        public void RotateCopiedElements(XYZ rotationBasis, double angle)
        {
            if (selectedElement != null)
            {
                Transaction transaction = new Transaction(doc, "Поворот элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ previousAxis = rotationBasis;

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);

                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

                        if (newElementIds != null && newElementIds.Count > 0)
                        {
                            ElementId newElementId = newElementIds.FirstOrDefault();
                            Element newElement = doc.GetElement(newElementId);

                            ElementTransformUtils.RotateElement(doc, newElementId, Line.CreateBound(previousAxis, previousAxis), angle);
                            previousAxis = rotationBasis;
                        }

                        translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0));
                    }

                    transaction.Commit();
                }
            }
        }

        public void RotateMoveCopiedElement(XYZ position, double angle, char axis) { }
    }
}
