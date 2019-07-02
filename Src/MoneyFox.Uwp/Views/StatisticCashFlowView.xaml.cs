﻿using System;
using Windows.UI.Xaml;
using MoneyFox.Presentation;
using MoneyFox.Uwp.Views.Dialogs;

namespace MoneyFox.Uwp.Views
{
    public sealed partial class StatisticCashFlowView
    {
        public StatisticCashFlowView()
        {
            InitializeComponent();
        }

        private async void SetDate(object sender, RoutedEventArgs e)
        {
            await new SelectDateRangeDialog
            {
                DataContext = ViewModelLocator.SelectDateRangeDialogVm
            }.ShowAsync();
        }
    }
}