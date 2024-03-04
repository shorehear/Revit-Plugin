using Autodesk.Revit.DB;
using System.Linq;
using System.Collections.Generic;

namespace Plugin
{
    public class ElementCopier
    {
        private Document doc;
        private Element selectedElement;
        private CurveElement selectedLine;

        public int AmountOfElements;
        public double DistanceBetweenElements;

        public ElementCopier(Document doc, Element selectedElement, CurveElement selectedLine = null)
        {
            this.doc = doc;
            this.selectedElement = selectedElement;
            this.selectedLine = selectedLine;
        }

        public void CopyElements()
        {
            if (selectedElement != null && selectedLine != null)
            {
                Transaction transaction = new Transaction(doc, "Копирование и вращение элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);

                    double rotationAngle = GetRotationAngle(selectedElement, selectedLine);

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

                        if (newElementIds.Count > 0)
                        {
                            Element newElement = doc.GetElement(newElementIds.First());
                            ElementTransformUtils.RotateElement(doc, newElement.Id, Line.CreateBound(new XYZ(0, 0, 0), XYZ.BasisZ), rotationAngle);

                            translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0));
                        }
                    }
                    transaction.Commit();
                }
            }
            else if(selectedElement != null && selectedLine == null)
            {
                Transaction transaction = new Transaction(doc, "Копирование элементов");
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
        } // Копирование объектов, если дистанция ними задается по оси абсцисс/не задается вовсе.

        private double GetRotationAngle(Element selectedElement, CurveElement selectedLine)
        {
            XYZ lineDirection = (selectedLine as ModelCurve).GeometryCurve.GetEndPoint(1) - (selectedLine as ModelCurve).GeometryCurve.GetEndPoint(0);
            XYZ elementDirection = (selectedElement.Location as LocationCurve).Curve.GetEndPoint(1) - (selectedElement.Location as LocationCurve).Curve.GetEndPoint(0);

            double rotationAngle = lineDirection.AngleTo(elementDirection);

            return rotationAngle;
        }

        public void MoveCopiedElements(XYZ position)
        {
            if (selectedElement != null && selectedLine != null)
            {
                Transaction transaction = new Transaction(doc, "Копирование, перемещение и вращение элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(DistanceBetweenElements, 0, 0);
                    double rotationAngle = GetRotationAngle(selectedElement, selectedLine);

                    for (int i = 0; i < AmountOfElements; i++)
                    {
                        ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElement(doc, selectedElement.Id, translation);

                        if (newElementIds != null && newElementIds.Count > 0)
                        {
                            ElementId newElementId = newElementIds.FirstOrDefault();
                            Element newElement = doc.GetElement(newElementId);
                            ElementTransformUtils.RotateElement(doc, newElement.Id, Line.CreateBound(new XYZ(0, 0, 0), XYZ.BasisZ), rotationAngle);
                            ElementTransformUtils.MoveElement(doc, newElement.Id, position + translation);
                        }

                        translation = translation.Add(new XYZ(DistanceBetweenElements, 0, 0));
                    }

                    transaction.Commit();
                }
            }
            else if (selectedElement != null && selectedLine == null)
            {

                Transaction transaction = new Transaction(doc, "Копирование и перемещение элементов");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    XYZ translation = new XYZ(0, 0, 0);

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
        } //Копирование объектов, если дистанция между ними задается по произвольной оси.
    }
}

