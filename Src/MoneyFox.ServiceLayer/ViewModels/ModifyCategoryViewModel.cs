﻿using System;
using System.Threading.Tasks;
using GenericServices;
using MoneyFox.BusinessLogic.Backup;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Parameters;
using MoneyFox.ServiceLayer.QueryObject;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;

namespace MoneyFox.ServiceLayer.ViewModels
{
    public interface IModifyCategoryViewModel : IBaseViewModel
    {
        /// <summary>
        ///     Saves changes to a CategoryViewModel if in edit mode <see cref="IsEdit" />  or creates
        ///     a new CategoryViewModel.
        /// </summary>
        MvxAsyncCommand SaveCommand { get; }
    
        /// <summary>
        ///     Cancel the current operation
        /// </summary>
        MvxAsyncCommand CancelCommand { get; }

        /// <summary>
        ///     Selected category.
        /// </summary>
        CategoryViewModel SelectedCategory { get; }
    }

    /// <summary>
    ///     View Model for creating and editing Categories without dialog
    /// </summary>
    public abstract class ModifyCategoryViewModel : BaseNavigationViewModel<ModifyCategoryParameter>, IModifyCategoryViewModel
    {
        private readonly ICrudServicesAsync crudServices;
        private readonly IDialogService dialogService;
        private readonly ISettingsFacade settingsFacade;
        private readonly IBackupManager backupManager;

        private CategoryViewModel selectedCategory;

        /// <summary>
        ///     Constructor
        /// </summary>
        protected ModifyCategoryViewModel(ICrudServicesAsync crudServices,
                                       IDialogService dialogService,
                                       ISettingsFacade settingsFacade,
                                       IBackupManager backupManager,
                                       IMvxLogProvider logProvider,
                                       IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            this.dialogService = dialogService;
            this.settingsFacade = settingsFacade;
            this.backupManager = backupManager;
            this.crudServices = crudServices;
        }


        protected abstract Task SaveCategory();

        private async Task SaveCategoryBase()
        {
            if (await crudServices.ReadManyNoTracked<AccountViewModel>().AnyWithName(SelectedCategory.Name))
            {
                await dialogService.ShowMessage(Strings.MandatoryFieldEmptyTitle, Strings.NameRequiredMessage);
                return;
            }

            await SaveCategory();

            settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
            await backupManager.EnqueueBackupTask();
        }

        /// <summary>
        ///     Saves changes to a CategoryViewModel if in edit mode <see cref="IsEdit" />  or creates
        ///     a new CategoryViewModel.
        /// </summary>
        public MvxAsyncCommand SaveCommand => new MvxAsyncCommand(SaveCategoryBase);

        /// <summary>
        ///     Cancel the current operation
        /// </summary>
        public MvxAsyncCommand CancelCommand => new MvxAsyncCommand(Cancel);

        /// <summary>
        ///     The currently selected CategoryViewModel
        /// </summary>
        public CategoryViewModel SelectedCategory
        {
            get => selectedCategory;
            set
            {
                selectedCategory = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Returns the Title based on whether a CategoryViewModel is being created or edited
        /// </summary>
        public virtual string Title => Strings.AddCategoryTitle;

        protected int CategoryId;

        /// <inheritdoc />
        public override void Prepare(ModifyCategoryParameter parameter)
        {
            CategoryId = parameter.CategoryId;
        }

        private async Task Cancel()
        {
            SelectedCategory = await crudServices.ReadSingleAsync<CategoryViewModel>(SelectedCategory.Id);
            await NavigationService.Close(this);
        }
    }
}