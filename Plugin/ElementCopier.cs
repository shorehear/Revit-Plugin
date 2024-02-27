using Autodesk.Revit.DB;
using System;
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

        public void CopyElementAtPosition(XYZ position)
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
                            translation = translation.Add(position);
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public void RotateCopiedElement(double angle, char axis) { }
        public void RotateMoveCopiedElement(XYZ position, double angle, char axis) { }
    }
}
