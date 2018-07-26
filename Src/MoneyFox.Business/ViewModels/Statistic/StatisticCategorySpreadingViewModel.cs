﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microcharts;
using MoneyFox.Business.StatisticDataProvider;
using MoneyFox.Foundation.Interfaces;
using MvvmCross.Plugin.Messenger;
using SkiaSharp;

namespace MoneyFox.Business.ViewModels.Statistic
{
    /// <summary>
    ///     Representation of the category Spreading View
    /// </summary>
    public class StatisticCategorySpreadingViewModel : StatisticViewModel, IStatisticCategorySpreadingViewModel
    {
        private readonly IDialogService dialogService;
        private readonly CategorySpreadingDataProvider spreadingDataProvider;
        private DonutChart chart;

        public static readonly SKColor[] Colors =
        {
            SKColor.Parse("#266489"),
            SKColor.Parse("#68B9C0"),
            SKColor.Parse("#90D585"),
            SKColor.Parse("#F3C151"),
            SKColor.Parse("#F37F64"),
            SKColor.Parse("#424856"),
            SKColor.Parse("#8F97A4")
        };

        /// <summary>
        ///     Contstructor
        /// </summary>
        public StatisticCategorySpreadingViewModel(CategorySpreadingDataProvider spreadingDataProvider,
                                                   IMvxMessenger messenger,
                                                   ISettingsManager settingsManager, 
                                                   IDialogService dialogService)
            : base(messenger, settingsManager)
        {
            this.spreadingDataProvider = spreadingDataProvider;
            this.dialogService = dialogService;
        }

        /// <summary>
        ///     Chart to render.
        /// </summary>
        public DonutChart Chart
        {
            get => chart;
            set
            {
                if (chart == value) return;
                chart = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Statistic items to display.
        /// </summary>
        public ObservableCollection<StatisticEntry> StatisticItems { get; set; }

        /// <inheritdoc />
        public override async Task Initialize()
        {
            dialogService.ShowLoadingDialog();
            await Task.Run(async () => await Load());
            dialogService.HideLoadingDialog();
        }

        /// <summary>
        ///     Set a custom CategprySpreadingModel with the set Start and Enddate
        /// </summary>
        protected override async Task Load()
        {
            StatisticItems = new ObservableCollection<StatisticEntry>(await spreadingDataProvider.GetValues(StartDate, EndDate));

            var microChartItems = StatisticItems
                                  .Select(x => new Entry(x.Value) {Label = x.Label, ValueLabel = x.ValueLabel})
                                  .ToList();

            for (int i = 0; i < microChartItems.Count; i++)
            {
                microChartItems[i].Color = Colors[i];
            }

            Chart = new DonutChart
            {
                Entries = microChartItems,
                BackgroundColor = BackgroundColor,
                LabelTextSize = 26f
            };
        }
    }
}