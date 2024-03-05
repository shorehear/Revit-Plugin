using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;

namespace Plugin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RVAPI : IExternalCommand
    {
        private char typeOperation; 
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {

                ChoiceWindow choiceWindow = new ChoiceWindow();
                choiceWindow.ChoiceMade += ChoiceWindow_ChoiceMade;
                choiceWindow.ShowDialog();

                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                if (typeOperation == 'D')
                {
                    Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Выберите элемент.");
                    if (pickedRef == null)
                        return Result.Cancelled;
                    Element selectedElement = doc.GetElement(pickedRef);

                    using (DefaultWindow mainWindow = new DefaultWindow(doc, selectedElement))
                    {
                        mainWindow.ShowDialog();
                    }
                }
                else if (typeOperation == 'C')
                {
                    Reference pickedRefObject = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Выберите элемент, затем выберите линию направления.");
                    Reference pickedRefLine = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Выберите линию направления.");

                    if (pickedRefObject == null || pickedRefLine == null)
                        return Result.Cancelled;

                    Element selectedElement = doc.GetElement(pickedRefObject);
                    Element lineElement = doc.GetElement(pickedRefLine);

                    if (!(lineElement is CurveElement))
                    {
                        TaskDialog.Show("Ошибка", "Выбранный элемент не является линией направления.");
                        return Result.Cancelled;
                    }

                    CurveElement selectedLine = lineElement as CurveElement;

                    using (CustomCopyWindow customCopyWindow = new CustomCopyWindow(doc, selectedElement, selectedLine))
                    {
                        customCopyWindow.ShowDialog();
                    }
                }
                else { 
                    return Result.Cancelled; 
                }

            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: ", ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void ChoiceWindow_ChoiceMade(object sender, bool isDefaultCopy) { typeOperation = isDefaultCopy ?  'D' : 'C'; }
    }
}