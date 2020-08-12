﻿using AutoMapper;
using GalaSoft.MvvmLight.Command;
using MediatR;
using MoneyFox.Application.Categories.Queries.GetCategoryById;
using MoneyFox.Application.Common.Messages;
using MoneyFox.Application.Resources;
using MoneyFox.Domain.Exceptions;
using MoneyFox.Extensions;
using MoneyFox.Messages;
using MoneyFox.Services;
using MoneyFox.ViewModels.Accounts;
using MoneyFox.ViewModels.Categories;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MoneyFox.ViewModels.Payments
{
    public abstract class ModifyPaymentViewModel : BaseViewModel
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private PaymentViewModel selectedPayment = new PaymentViewModel();

        private ObservableCollection<AccountViewModel> chargedAccounts = new ObservableCollection<AccountViewModel>();
        private ObservableCollection<AccountViewModel> targetAccounts = new ObservableCollection<AccountViewModel>();

        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly IDialogService dialogService;

        protected ModifyPaymentViewModel(IMediator mediator,
                                         IMapper mapper,
                                         IDialogService dialogService)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.dialogService = dialogService;

            MessengerInstance.Register<CategorySelectedMessage>(this, async message => await ReceiveMessageAsync(message));
        }

        /// <summary>
        /// The currently selected PaymentViewModel
        /// </summary>
        public PaymentViewModel SelectedPayment
        {
            get => selectedPayment;
            set
            {
                selectedPayment = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gives access to all accounts for Charged Dropdown list
        /// </summary>
        public ObservableCollection<AccountViewModel> ChargedAccounts
        {
            get => chargedAccounts;
            private set
            {
                chargedAccounts = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gives access to all accounts for Target Dropdown list
        /// </summary>
        public ObservableCollection<AccountViewModel> TargetAccounts
        {
            get => targetAccounts;
            private set
            {
                targetAccounts = value;
                RaisePropertyChanged();
            }
        }

        public virtual async Task InitializeAsync()
        {

        }

        public RelayCommand GoToSelectCategoryDialogCommand => new RelayCommand(async () => await Shell.Current.GoToModalAsync(ViewModelLocator.SelectCategoryRoute));
        public RelayCommand ResetCategoryCommand => new RelayCommand(() => SelectedPayment.Category = null);

        public RelayCommand SaveCommand => new RelayCommand(async () => await SavePaymentBaseAsync());

        protected abstract Task SavePaymentAsync();

        private async Task SavePaymentBaseAsync()
        {
            if(SelectedPayment.ChargedAccount == null)
            {
                await dialogService.ShowMessageAsync(Strings.MandatoryFieldEmptyTitle, Strings.AccountRequiredMessage);
                return;
            }

            if(SelectedPayment.Amount < 0)
            {
                await dialogService.ShowMessageAsync(Strings.AmountMayNotBeNegativeTitle, Strings.AmountMayNotBeNegativeMessage);
                return;
            }

            await dialogService.ShowLoadingDialogAsync(Strings.SavingPaymentMessage);

            try
            {
                await SavePaymentAsync();
                MessengerInstance.Send(new ReloadMessage());
                await App.Current.MainPage.Navigation.PopModalAsync();

            }
            catch(InvalidEndDateException)
            {
                await dialogService.ShowMessageAsync(Strings.InvalidEnddateTitle, Strings.InvalidEnddateMessage);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                await dialogService.HideLoadingDialogAsync();
            }
        }

        private async Task ReceiveMessageAsync(CategorySelectedMessage message)
        {
            if(SelectedPayment == null || message == null)
            {
                return;
            }

            SelectedPayment.Category = mapper.Map<CategoryViewModel>(await mediator.Send(new GetCategoryByIdQuery(message.CategoryId)));
        }
    }
}
