<<<<<<< HEAD
﻿using System.Windows;
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
                Element selectedElement = doc.GetElement(pickedRef);

                MainWindow mainWindow = new MainWindow(doc, selectedElement);
                mainWindow.ShowDialog();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
=======
﻿using System.Windows;
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
                Element selectedElement = doc.GetElement(pickedRef);

                MainWindow mainWindow = new MainWindow(doc, selectedElement);
                mainWindow.ShowDialog();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
>>>>>>> df9d18eefb07f6425ffd24c7822836ce112a4e96
}