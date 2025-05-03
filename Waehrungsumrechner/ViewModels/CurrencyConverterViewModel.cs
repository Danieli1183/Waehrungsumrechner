using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waehrungsumrechner.Models;
using Waehrungsumrechner.Services;

namespace Waehrungsumrechner.ViewModels
{
    public class CurrencyConverterViewModel : INotifyPropertyChanged
    {
        private readonly ICurrencyService _service;
        private Currency _sourceCurrency;
        private Currency _targetCurrency;
        private decimal _amount = 1m;
        private decimal _converted;
        private Dictionary<string, decimal> _currentRates = new Dictionary<string, decimal>();

        public ObservableCollection<Currency> Currencies { get; } = new ObservableCollection<Currency>();

        public CurrencyConverterViewModel() : this(new CurrencyService()) { }

        public CurrencyConverterViewModel(ICurrencyService service)
        {
            _service = service;
            _ = LoadCurrenciesAsync();
        }

        private async Task LoadCurrenciesAsync()
        {
            var list = await _service.GetCurrenciesAsync();
            foreach (var c in list)
                Currencies.Add(c);

            SourceCurrency = Currencies.FirstOrDefault(c => c.Code == "EUR");
            TargetCurrency = Currencies.FirstOrDefault(c => c.Code == "USD");
            Amount = 1m;
        }

        public Currency SourceCurrency
        {
            get => _sourceCurrency;
            set
            {
                if (_sourceCurrency != value)
                {
                    _sourceCurrency = value;
                    OnPropertyChanged();
                    _ = LoadRatesAsync();
                }
            }
        }

        public Currency TargetCurrency
        {
            get => _targetCurrency;
            set
            {
                if (_targetCurrency != value)
                {
                    _targetCurrency = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                    Recalculate();
                }
            }
        }

        public decimal Converted
        {
            get => _converted;
            private set
            {
                if (_converted != value)
                {
                    _converted = value;
                    OnPropertyChanged();
                }
            }
        }

        private async Task LoadRatesAsync()
        {
            if (SourceCurrency == null) return;
            _currentRates = await _service.GetRatesAsync(SourceCurrency.Code);
            Recalculate();
        }

        private void Recalculate()
        {
            if (_currentRates == null || TargetCurrency == null)
            {
                Converted = 0;
                return;
            }

            if (_currentRates.TryGetValue(TargetCurrency.Code, out var rate))
                Converted = Amount * rate;
            else
                Converted = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
