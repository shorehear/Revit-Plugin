using System;
using System.Windows;
using System.Windows.Controls;

namespace Plugin
{
    public partial class ChoiceWindow : Window
    {
        public bool IsDefaultCopy { get; private set; }
        public event EventHandler<bool> ChoiceMade;

        public ChoiceWindow()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            Title = "Выбрать тип копирования";
            Width = 350;
            Height = 150;

            var infoLabel = new Label()
            {
                Content = "Какой из типов копирования вас интересует?",
                Margin = new Thickness(10, 0, 0, 0)
            };

            var choiceDefaultButton = new Button()
            {
                Content = "Стандартный",
                Margin = new Thickness(10, 10, 30, 0),
                ToolTip = new ToolTip { Content = "Производится выбор объекта, определяется расстояние между объектами, направление копирования." }
            };
            choiceDefaultButton.Click += ChoiceDefaultButton_Click;

            var choiceCustomButton = new Button()
            {
                Content = "Пользовательский",
                Margin = new Thickness(10, 10, 30, 0),
                ToolTip = new ToolTip { Content = "Производится выбор объекта и направляющей линии, определяется расстояние между объектами, направление копирования, вращение копируемых объектов" }
            };
            choiceCustomButton.Click += ChoiceCustomButton_Click;

            StackPanel panel = new StackPanel();
            panel.Children.Add(choiceDefaultButton);
            panel.Children.Add(choiceCustomButton);

            Content = panel;
        }
        private void ChoiceDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            IsDefaultCopy = true;
            OnChoiceMade();
            Close();
        }

        private void ChoiceCustomButton_Click(object sender, RoutedEventArgs e)
        {
            IsDefaultCopy = false;
            OnChoiceMade();
            Close();
        }

        protected virtual void OnChoiceMade()
        {
            ChoiceMade?.Invoke(this, IsDefaultCopy);
        }
    }
}
